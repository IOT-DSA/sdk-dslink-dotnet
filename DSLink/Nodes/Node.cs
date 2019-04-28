using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSLink.Nodes.Actions;
using DSLink.Util;
using Newtonsoft.Json.Linq;
using DSLink.Logging;
using Action = DSLink.Nodes.Actions.Action;

namespace DSLink.Nodes
{
    /// <summary>
    /// A DSA Node
    /// </summary>
    public class Node
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// Set of banned characters from DSA names.
        /// </summary>
        public static readonly char[] BannedChars =
        {
            '%', '.', '/', '\\', '?', '*', ':', '|', '<', '>', '$', '@', ',', '\'', '"'
        };

        private readonly BaseLinkHandler _link;
        private string _path;
        private readonly IDictionary<string, Node> _children;
        internal readonly List<Node> _removedChildren;
        internal readonly List<int> _subscribers;
        internal readonly List<int> _streams;

        /// <summary>
        /// Name of this node.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Parent of this Node.
        /// </summary>
        public readonly Node Parent;

        /// <summary>
        /// Value of this Node.
        /// </summary>
        public readonly Value Value;

        /// <summary>
        /// Path of this Node.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
            protected set
            {
                _path = value.TrimEnd('/');
            }
        }

        /// <summary>
        /// Class for manipulating configs.
        /// </summary>
        public readonly MetadataMap Configs;

        /// <summary>
        /// Class for manipulating attributes.
        /// </summary>
        public readonly MetadataMap Attributes;

        /// <summary>
        /// Private configuration values only stored locally, these
        /// are not available over DSA.
        /// </summary>
        public readonly MetadataMap PrivateConfigs;

        /// <summary>
        /// Event fired when something subscribes to this node.
        /// </summary>
        public Action<int> OnSubscribed;

        /// <summary>
        /// Event fired when something unsubscribes to this node.
        /// </summary>
        public Action<int> OnUnsubscribed;

        /// <summary>
        /// Node action
        /// </summary>
        public Action Action
        {
            get;
            protected set;
        }

        /// <summary>
        /// Indicates whether the Node is serialized into the 
        /// nodes.json file which restores the state during
        /// future start-up of the DSLink.
        /// </summary>
        public bool Serializable = true;

        /// <summary>
        /// User-defined object to help link third party code
        /// to a DSA Node.
        /// </summary>
        public object UserObject = null;

        /// <summary>
        /// Node is still being created via NodeFactory
        /// </summary>
        internal bool Building;

        /// <summary>
        /// True if Node is subscribed to.
        /// </summary>
        public bool Subscribed => _subscribers.Count != 0;

        /// <summary>
        /// Public-facing dictionary of children.
        /// </summary>
        public ReadOnlyDictionary<string, Node> Children => new ReadOnlyDictionary<string, Node>(_children);

        /// <summary>
        /// List of children that are being removed.
        /// </summary>
        public ReadOnlyList<Node> RemovedChildren => new ReadOnlyList<Node>(_removedChildren);

        /// <summary>
        /// List of request IDs belonging to this Node.
        /// </summary>
        public ReadOnlyList<int> Streams => new ReadOnlyList<int>(_streams);

        /// <summary>
        /// List of subscription IDs belonging to this Node.
        /// </summary>
        public ReadOnlyList<int> Subscribers => new ReadOnlyList<int>(_subscribers);

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
                lock (_children)
                {
                    if (name.StartsWith("/"))
                    {
                        return Get(name);
                    }

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
        public Node(string name, Node parent, BaseLinkHandler link)
        {
            if (name == null)
            {
                throw new ArgumentException("Name must not be null.");
            }

            Parent = parent;
            _children = new Dictionary<string, Node>();

            Configs = new MetadataMap("$");
            Attributes = new MetadataMap("@");
            PrivateConfigs = new MetadataMap("");

            Configs.OnSet += UpdateSubscribers;
            Attributes.OnSet += UpdateSubscribers;

            _removedChildren = new List<Node>();
            _subscribers = new List<int>();
            _streams = new List<int>();
            _link = link;

            _createInitialData();

            Value = new Value();
            Value.OnSet += ValueSet;

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

            link?.Responder?.StreamManager?.OnActivateNode(this);
        }

        private void _createInitialData()
        {
        }

        /// <summary>
        /// Clears everything from the Node, including
        /// children, nodes, config, attributes, etc.
        /// TODO: To become a public API, properly implement
        /// this, don't just kill off children.
        /// </summary>
        internal void ResetNode()
        {
            _children.Clear();
            Configs.Clear();
            Attributes.Clear();
            PrivateConfigs.Clear();
            Value.Clear();

            _createInitialData();
        }

        internal void ClearRemovedChildren()
        {
            _removedChildren.Clear();
        }

        /// <summary>
        /// Create a child via the NodeFactory.
        /// The node will not be added to the parent until NodeFactory.BuildNode() is called.
        /// </summary>
        /// <param name="name">Node's name</param>
        /// <returns>NodeFactory of new child</returns>
        public virtual NodeFactory CreateChild(string name)
        {
            if (name.IndexOfAny(BannedChars) != -1)
            {
                throw new ArgumentException("Invalid character(s) in Node name.");
            }

            Node child = new Node(name, this, _link);
            return new NodeFactory(child);
        }

        /// <summary>
        /// Add a pre-existing Node as a child.
        /// </summary>
        /// <param name="child">Child Node</param>
        public void AddChild(Node child)
        {
            lock (_children)
            {
                if (_children.ContainsKey(child.Name))
                {
                    throw new ArgumentException($"Child already exists at {child.Path}");
                }

                _children.Add(child.Name, child);
            }

            UpdateSubscribers();
        }

        /// <summary>
        /// Sets the action.
        /// </summary>
        /// <param name="action">Action</param>
        public void SetAction(Action action)
        {
            Configs.Set(ConfigType.Invokable, new Value(action.Permission.Permit));
            Action = action;
        }

        /// <summary>
        /// Remove a Node from being a child.
        /// </summary>
        /// <param name="name">Child Node's name</param>
        public void RemoveChild(string name)
        {
            lock (_children)
            {
                if (!_children.ContainsKey(name)) return;
                lock (_removedChildren)
                {
                    _removedChildren.Add(_children[name]);
                }

                _children.Remove(name);
            }

            UpdateSubscribers();
        }

        /// <summary>
        /// Removes all child nodes
        /// </summary>
        public void RemoveAllChildren()
        {
            lock (_children)
            {
                lock (_removedChildren)
                {
                    foreach (var key in _children.Keys)
                    {
                        _removedChildren.Add(_children[key]);
                    }
                }

                _children.Clear();
            }

            UpdateSubscribers();
        }

        /// <summary>
        /// Remove this Node from its parent.
        /// </summary>
        public void RemoveFromParent()
        {
            Parent?.RemoveChild(Name);
        }

        internal void RemoveConfigAttribute(string path)
        {
            if (path.StartsWith("/$") || path.StartsWith(Path + "/$"))
            {
                Configs.Remove(path.Substring(2));
            }
            else if (path.StartsWith("/@") || path.StartsWith(Path + "/@"))
            {
                Attributes.Remove(path.Substring(2));
            }
            else
            {
                Get(path).RemoveConfigAttribute(path);
            }

            UpdateSubscribers();
        }

        /// <summary>
        /// Event fired when the value is set.
        /// </summary>
        /// <param name="value"></param>
        protected virtual async void ValueSet(Value value)
        {
            var tasks = new List<Task>();

            lock (_subscribers)
            {
                foreach (var t in _subscribers)
                {
                    tasks.Add(_link.Connection.AddValueUpdateResponse(new JArray
                    {
                        t,
                        value.As<JToken>(),
                        value.LastUpdatedIso
                    }));
                }
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException e)
            {
                Logger.Warn(e.ToString());
            }
        }

        /// <summary>
        /// Save the Node to a JSON object.
        /// </summary>
        /// <returns>Serialized data</returns>
        public JObject Serialize()
        {
            var serializedObject = new JObject();

            foreach (var entry in Configs)
            {
                serializedObject.Add(new JProperty(entry.Key, entry.Value.As<JToken>()));
            }

            foreach (var entry in Attributes)
            {
                serializedObject.Add(new JProperty(entry.Key, entry.Value.As<JToken>()));
            }

            var privateConfigs = new JObject();
            serializedObject.Add("privateConfigs", privateConfigs);
            foreach (var entry in PrivateConfigs)
            {
                privateConfigs.Add(new JProperty(entry.Key, entry.Value.As<JToken>()));
            }

            foreach (var entry in Children)
            {
                if (entry.Value.Serializable)
                {
                    serializedObject.Add(new JProperty(entry.Key, entry.Value.Serialize()));
                }
            }

            if (!Value.IsNull)
            {
                serializedObject.Add(new JProperty("?value", Value.As<JToken>()));
            }

            return serializedObject;
        }

        // <summary>
        // Deserialize the node from the given object.
        // </summary>
        public void Deserialize(JObject obj)
        {
            foreach (var prop in obj)
            {
                if (prop.Key == "?value")
                {
                    Value.Set(prop.Value);
                }
                else if (prop.Key.StartsWith("$"))
                {
                    Configs.Set(prop.Key.Substring(1), new Value(prop.Value));
                }
                else if (prop.Key.StartsWith("@"))
                {
                    Attributes.Set(prop.Key.Substring(1), new Value(prop.Value));
                }
                else if (prop.Key == "privateConfigs")
                {
                    foreach (var entry in prop.Value.Value<JObject>())
                    {
                        PrivateConfigs.Set(entry.Key, new Value(entry.Value));
                    }
                }
                else if (prop.Value is JObject)
                {
                    string name = prop.Key;

                    if (!Children.ContainsKey(name))
                    {
                        var builder = CreateChild(name);
                        builder.Deserialize((JObject) prop.Value);
                        builder.Build();
                    }
                    else
                    {
                        Children[name].Deserialize((JObject) prop.Value);
                    }
                }
            }
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
            var indexOfFirstSlash = path.IndexOf('/');
            var child = indexOfFirstSlash == -1 ? path : path.Substring(0, path.IndexOf('/'));
            path = path.TrimStart(child.ToCharArray());

            if (Children.TryGetValue(child, out Node childNode))
            {
                return childNode.Get(path);
            }

            return null;
        }

        protected virtual async void UpdateSubscribers()
        {
            if (_link == null) return;
            await _link.Responder.SubscriptionManager.UpdateSubscribers(this);
        }
    }
}