using System;
using System.Collections.Generic;
using System.Linq;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes.Actions;
using DSLink.Util;
using Newtonsoft.Json.Linq;
using Action = DSLink.Nodes.Actions.Action;

namespace DSLink.Nodes
{
    /// <summary>
    /// Node
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Set of banned characters from DSA names.
        /// </summary>
        public static readonly char[] BannedChars = {
            '%', '.', '/', '\\', '?', '*', ':', '|', '<', '>', '$', '@', ',', '\'', '"'
        };

        /// <summary>
        /// Dictionary of children
        /// </summary>
        private readonly IDictionary<string, Node> _children;

        /// <summary>
        /// Dictionary of Node configurations
        /// </summary>
        private readonly IDictionary<string, Value> _configs;

        /// <summary>
        /// Dictionary of Node attributes
        /// </summary>
        private readonly IDictionary<string, Value> _attributes;

        /// <summary>
        /// List of removed children, used to notify watchers about
        /// removed children.
        /// </summary>
        private readonly IList<Node> _removedChildren;

        /// <summary>
        /// List of subscription IDs belonging to this Node
        /// </summary>
        internal readonly List<int> Subscribers;

        /// <summary>
        /// Event fired when something subscribes to this node.
        /// </summary>
        public Action<int> OnSubscribed;

        /// <summary>
        /// Event fired when something unsubscribes to this node.
        /// </summary>
        public Action<int> OnUnsubscribed;

        /// <summary>
        /// List of request IDs belonging to this Node
        /// </summary>
        internal readonly List<int> Streams;

        /// <summary>
        /// DSLink container instance.
        /// </summary>
        private readonly AbstractContainer _link;

        /// <summary>
        /// Used to lock the children dictionary.
        /// </summary>
        private readonly object _childrenLock = new object();

        /// <summary>
        /// Used to lock the removed children list.
        /// </summary>
        private readonly object _removedChildrenLock = new object();

        /// <summary>
        /// Name of this node.
        /// </summary>
        public string Name { get; }

        private string _path;

        /// <summary>
        /// Path of this Node.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
            internal set
            {
                _path = value.TrimEnd(new char[] { '/' });
            }
        }

        /// <summary>
        /// Parent of this Node.
        /// </summary>
        public Node Parent { get; }

        /// <summary>
        /// Value of this Node.
        /// </summary>
        public Value Value { get; }

        /// <summary>
        /// Node action
        /// </summary>
        public Action Action;

        /// <summary>
        /// Node is transient
        /// </summary>
        public bool Transient;

        /// <summary>
        /// Node is still being created via NodeFactory
        /// </summary>
        internal bool Building;

        /// <summary>
        /// Node is subscribed to
        /// </summary>
        public bool Subscribed => Subscribers.Count != 0;

        /// <summary>
        /// Public-facing dictionary of children.
        /// </summary>
        public ReadOnlyDictionary<string, Node> Children => new ReadOnlyDictionary<string, Node>(_children);

        /// <summary>
        /// Public-facing dictionary of configurations.
        /// </summary>
        /// <value>The configurations.</value>
        public ReadOnlyDictionary<string, Value> Configurations => new ReadOnlyDictionary<string, Value>(_configs);

        /// <summary>
        /// Public-facing dictionary of attributes.
        /// </summary>
        public ReadOnlyDictionary<string, Value> Attributes => new ReadOnlyDictionary<string, Value>(_attributes);

        /// <summary>
        /// Index operator overload.
        /// Example: Parent["Child"]["ChildOfChild"]
        /// </summary>
        /// <param name="name">Child name</param>
        /// <returns>Child Node</returns>
        public Node this[string name]
        {
            get
            {
                lock (_childrenLock)
                {
                    return _children[name];
                }
            }
        }

        /// <summary>
        /// Node constructor
        /// </summary>
        /// <param name="name">Name of Node</param>
        /// <param name="parent">Parent of Node</param>
        /// <param name="link">DSLink container of Node</param>
        public Node(string name, Node parent, AbstractContainer link)
        {
            if (name == null)
            {
                throw new ArgumentException("Name must not be null.");
            }
            if (name.IndexOfAny(BannedChars) != -1)
            {
                throw new ArgumentException("Invalid character(s) in Node name.");
            }
            Parent = parent;
            _children = new Dictionary<string, Node>();
            _configs = new Dictionary<string, Value>
            {
                {"$is", new Value("node")}
            };
            _attributes = new Dictionary<string, Value>();
            _removedChildren = new List<Node>();
            Subscribers = new List<int>();
            Streams = new List<int>();
            _link = link;

            Value = new Value();
            Value.OnSet += ValueSet;
            Transient = false;

            if (parent != null)
            {
                if (name.Equals(""))
                {
                    throw new ArgumentException("name");
                }
                Name = name;
                Path = (parent.Path.Equals("/") ? "" : parent.Path) + "/" + name;
            }
            else
            {
                Name = name;
                Path = "/" + name;
            }
        }

        /// <summary>
        /// Set a Node configuration.
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="value">Configuration value</param>
        public void SetConfig(string key, Value value)
        {
            UpdateSubscribers();
            _configs["$" + key] = value;
        }

        /// <summary>
        /// Get a Node configuration.
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <returns>Attribute value</returns>
        public Value GetConfig(string key)
        {
            try
            {
                return _configs["$" + key];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        private dynamic GetConfigValue(string key)
        {
            var result = GetConfig(key);
            if (result == null) return null;
            return result.Get();
        }

        /// <summary>
        /// Set a Node attribute.
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <param name="value">Attribute value</param>
        public void SetAttribute(string key, Value value)
        {
            UpdateSubscribers();
            _attributes["@" + key] = value;
        }

        /// <summary>
        /// Get a Node attribute.
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <returns>Attribute value</returns>
        public Value GetAttribute(string key)
        {
            try
            {
                return _attributes["@" + key];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public string DisplayName
        {
            get
            {
                return GetConfigValue("name");
            }
            set
            {
                SetConfig("name", new Value(value));
            }
        }

        public string Profile
        {
            get
            {
                return GetConfigValue("profile");
            }
            set
            {
                SetConfig("profile", new Value(value));
            }
        }

        /// <summary>
        /// Gets or sets the writable permission.
        /// </summary>
        /// <value>The writable.</value>
        public Permission Writable
        {
            get
            {
                return Permission._permMap[GetConfigValue("writable")];
            }
            set
            {
                SetConfig("writable", new Value(value.ToString()));
            }
        }

        /// <summary>
        /// Gets or sets the invokable permission.
        /// </summary>
        /// <value>Invokable permission</value>
        public Permission Invokable
        {
            get
            {
                return Permission.FromString(GetConfigValue("invokable"));
            }
            set
            {
                SetConfig("invokable", new Value(value.ToString()));
            }
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>Parameters</value>
        public List<Parameter> Parameters
        {
            get
            {
                return GetConfigValue("params");
            }
            set
            {
                SetConfig("params", new Value(value));
            }
        }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>Columns</value>
        public List<Column> Columns
        {
            get
            {
                return GetConfigValue("columns");
            }
            set
            {
                SetConfig("columns", new Value(value));
            }
        }

        /// <summary>
        /// Gets or sets the action group.
        /// </summary>
        /// <value>Action group</value>
        public string ActionGroup
        {
            get
            {
                return GetConfigValue("actionGroup");
            }
            set
            {
                SetConfig("actionGroup", new Value(value));
            }
        }

        /// <summary>
        /// Gets or sets the action group subtitle.
        /// </summary>
        /// <value>Action group subtitle</value>
        public string ActionGroupSubtitle
        {
            get
            {
                return GetConfigValue("actionGroupSubTitle");
            }
            set
            {
                SetConfig("actionGroupSubTitle", new Value(value));
            }
        }

        /// <summary>
        /// True if Node has a value.
        /// </summary>
        /// <returns></returns>
        public bool HasValue()
        {
            return Value.Get() != null;
        }

        /// <summary>
        /// Create a child via the NodeFactory.
        /// </summary>
        /// <param name="name">Node's name</param>
        /// <returns>NodeFactory of new child</returns>
        public virtual NodeFactory CreateChild(string name)
        {
            Node child = new Node(name, this, _link);
            AddChild(child);
            return new NodeFactory(child);
        }

        /// <summary>
        /// Add a pre-existing Node as a child.
        /// </summary>
        /// <param name="child">Child Node</param>
        public void AddChild(Node child)
        {
            lock (_childrenLock)
            {
                _children.Add(child.Name, child);
            }
            UpdateSubscribers();
        }

        /// <summary>
        /// Remove a Node from being a child.
        /// </summary>
        /// <param name="name">Child Node's name</param>
        public void RemoveChild(string name)
        {
            lock (_childrenLock)
            {
                if (_children.ContainsKey(name))
                {
                    lock (_removedChildrenLock)
                    {
                        _removedChildren.Add(_children[name]);
                    }
                    _children.Remove(name);
                }
            }
        }

        /// <summary>
        /// Remove this Node from its parent.
        /// </summary>
        public void RemoveFromParent()
        {
            if (Parent != null)
            {
                Parent.RemoveChild(Name);
            }
        }

        /// <summary>
        /// Remove a Node configuration or attribute in the Node tree from here.
        /// </summary>
        /// <param name="path">Path of configuration/attribute</param>
        internal void RemoveConfigAttribute(string path)
        {
            if (path.StartsWith("/$") || path.StartsWith(Path + "/@"))
            {
                _configs.Remove(path.Substring(2));
            }
            else if (path.StartsWith("/@") || path.StartsWith(Path + "/@"))
            {
                _attributes.Remove(path.Substring(2));
            }
            else
            {
                Get(path).RemoveConfigAttribute(path);
            }
        }

        /// <summary>
        /// Event fired when the value is set.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void ValueSet(Value value)
        {
            var rootObject = new RootObject
            {
                Responses = new List<ResponseObject>
                {
                    new ResponseObject
                    {
                        RequestId = 0,
                        Updates = new JArray()
                    }
                }
            };
            bool hasUpdates = false;
            foreach (var sid in Subscribers)
            {
                hasUpdates = true;
                rootObject.Responses[0].Updates.Add(new JArray
                {
                    sid,
                    value.Get(),
                    value.LastUpdated
                });
            }
            if (hasUpdates)
            {
                _link.Connector.Write(rootObject);
            }
        }

        /// <summary>
        /// Serialize the Node.
        /// </summary>
        /// <returns>Serialized data</returns>
        public JArray Serialize()
        {
            var val = new JArray();
            foreach (var pair in _configs)
            {
                val.Add(new List<dynamic>
                {
                    pair.Key, pair.Value.Get()
                });
            }
            foreach (var pair in _attributes)
            {
                val.Add(new List<dynamic>
                {
                    pair.Key, pair.Value.Get()
                });
            }

            lock (_childrenLock)
            {
                foreach (var child in _children)
                {
                    var value = new Dictionary<string, dynamic>();
                    foreach (var config in child.Value._configs)
                    {
                        value.Add(config.Key, config.Value.Get());
                    }
                    foreach (var attr in child.Value._attributes)
                    {
                        value.Add(attr.Key, attr.Value.Get());
                    }
                    if (child.Value.HasValue())
                    {
                        value.Add("value", child.Value.Value.Get());
                        value.Add("ts", child.Value.Value.LastUpdated);
                    }
                    val.Add(new List<dynamic> { child.Key, value });
                }
            }

            lock (_removedChildrenLock)
            {
                dynamic i = _removedChildren.Select(child => new Dictionary<string, dynamic>
                {
                    {"name", child.Name},
                    {"change", "remove"}
                });
                foreach (dynamic node in i)
                {
                    _removedChildren.Add(node);
                }
                _removedChildren.Clear();
            }

            return val;
        }

        /// <summary>
        /// Get a Node in the Node structure from here.
        /// </summary>
        /// <param name="path">Node path</param>
        /// <returns>Node</returns>
        public Node Get(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Equals("/") || path.StartsWith("/$") || path.StartsWith("/@"))
            {
                return this;
            }
            path = path.TrimStart('/');
            var i = path.IndexOf('/');
            var child = i == -1 ? path : path.Substring(0, path.IndexOf('/'));
            path = path.TrimStart(child.ToCharArray());
            try
            {
                return Children[child].Get(path);
            }
            catch (KeyNotFoundException)
            {
                _link?.Logger.Warning("Non-existant Node requested");
                return null;
            }
        }

        /// <summary>
        /// Update all subscribers of this Node.
        /// </summary>
        internal virtual void UpdateSubscribers()
        {
            if (Building)
            {
                return;
            }
            var responses = Streams.Select(stream => new ResponseObject
            {
                RequestId = stream,
                Stream = "open",
                Updates = Serialize()
            }).ToList();
            if (responses.Count > 0)
            {
                _link.Connector.Write(new RootObject
                {
                    Responses = responses
                });
            }
        }
    }
}
