using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DSLink.Connection.Serializer;
using DSLink.Container;
using DSLink.Nodes.Actions;
using DSLink.Util;
using Action = DSLink.Nodes.Actions.Action;

namespace DSLink.Nodes
{
    public class Node
    {
        /// <summary>
        /// Set of banned characters from DSA names.
        /// </summary>
        public static readonly char[] BannedChars = {
            '%', '.', '/', '\\', '?', '*', ':', '|', '<', '>', '$', '@', ','
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
        /// List of request IDs belonging to this Node
        /// </summary>
        internal readonly List<int> Streams;

        /// <summary>
        /// DSLink container instance
        /// </summary>
        private readonly AbstractContainer _link;

        /// <summary>
        /// Used to lock the children dictionary
        /// </summary>
        private readonly object _childrenLock = new object();

        /// <summary>
        /// Used to lock the removed children list
        /// </summary>
        private readonly object _removedChildrenLock = new object();

        /// <summary>
        /// Node name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Node path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Node parent
        /// </summary>
        public Node Parent { get; }

        /// <summary>
        /// Node value
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
        /// Public-facing dictionary of children
        /// </summary>
        public ReadOnlyDictionary<string, Node> Children => new ReadOnlyDictionary<string, Node>(_children);

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
            if (link == null)
            {
                throw new ArgumentException("Link must not be null.");
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

        /// <summary>
        /// Set the Node's display name.
        /// </summary>
        /// <param name="displayName">Node display name</param>
        public void SetDisplayName(string displayName)
        {
            SetConfig("name", new Value(displayName));
        }

        /// <summary>
        /// Set the Node's profile.
        /// </summary>
        /// <param name="profile">Node profile</param>
        public void SetProfile(string profile)
        {
            SetConfig("is", new Value(profile));
        }

        /// <summary>
        /// Set the Node's writable permission.
        /// </summary>
        /// <param name="permission">Writable permission</param>
        public void SetWritable(Permission permission)
        {
            SetConfig("writable", new Value(permission.ToString()));
        }

        /// <summary>
        /// Set the Node's invokable permission.
        /// </summary>
        /// <param name="permission">Invoke permission</param>
        public void SetInvokable(Permission permission)
        {
            SetConfig("invokable", new Value(permission.ToString()));
        }

        /// <summary>
        /// Set the Node's invoke parameters.
        /// </summary>
        /// <param name="parameters">Invoke parameters</param>
        public void SetParameters(List<Parameter> parameters)
        {
            SetConfig("params", new Value(parameters));
        }

        /// <summary>
        /// Set the Node's invoke columns.
        /// </summary>
        /// <param name="columns">Invoke columns</param>
        public void SetColumns(List<Column> columns)
        {
            SetConfig("columns", new Value(columns));
        }

        /// <summary>
        /// Check if Node has a value.
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
        public NodeFactory CreateChild(string name)
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
        protected void ValueSet(Value value)
        {
            var rootObject = new RootObject
            {
                Msg = _link.MessageId,
                Responses = new List<ResponseObject>
                {
                    new ResponseObject
                    {
                        RequestId = 0,
                        Updates = new List<dynamic>()
                    }
                }
            };
            bool hasUpdates = false;
            foreach (var sid in Subscribers)
            {
                hasUpdates = true;
                rootObject.Responses[0].Updates.Add(new[] { sid, value.Get(), value.LastUpdated });
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
        public List<dynamic> Serialize()
        {
            var val = new List<dynamic>();
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
                val.AddRange(_removedChildren.Select(child => new Dictionary<string, dynamic>
                {
                    {"name", child.Name}, {"change", "remove"}
                }));
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
                _link.Logger.Warn("Non-existant Node requested");
                return null;
            }
        }

        /// <summary>
        /// Update all subscribers of this Node.
        /// </summary>
        internal void UpdateSubscribers()
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
                _link.Connector.Write(new RootObject()
                {
                    Msg = _link.MessageId,
                    Responses = responses
                });
            }
        }

        /// <summary>
        /// Internal utility
        /// </summary>
        internal static int NthIndexOf(string target, string value, int n)
        {
            var m = Regex.Match(target, "((" + Regex.Escape(value) + ").*?){" + n + "}");
            return m.Success ? m.Groups[2].Captures[n - 1].Index : -1;
        }
    }
}
