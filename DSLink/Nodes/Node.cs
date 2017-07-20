using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSLink.Nodes.Actions;
using DSLink.Util;
using Newtonsoft.Json.Linq;

namespace DSLink.Nodes
{
    /// <summary>
    /// A DSA Node
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
        /// List of removed children, used to notify watchers about
        /// removed children.
        /// </summary>
        private readonly List<Node> _removedChildren;

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
        private readonly DSLinkContainer _link;

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
        public readonly string Name;

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
                _path = value.TrimEnd('/');
            }
        }

        /// <summary>
        /// Parent of this Node.
        /// </summary>
        public Node Parent
        {
            get;
        }

        /// <summary>
        /// Value of this Node.
        /// </summary>
        public Value Value
        {
            get;
        }

        /// <summary>
        /// Node action
        /// </summary>
        public ActionHandler ActionHandler;

        /// <summary>
        /// Metadata object attached to a node.
        /// </summary>
        public object Metadata;

        /// <summary>
        /// Node is serializable
        /// </summary>
        public bool Serializable = true;

        /// <summary>
        /// Node is still being created via NodeFactory
        /// </summary>
        internal bool Building;

        /// <summary>
        /// Node is subscribed to
        /// </summary>
        public bool Subscribed => Subscribers.Count != 0;

        /// <summary>
        /// Class name of the node.
        /// </summary>
        public string ClassName { get; internal set; }

        /// <summary>
        /// Flag for when the Node Class is initialized.
        /// Used to prevent duplicate initializations.
        /// </summary>
        private bool _initializedClass;

        /// <summary>
        /// Public-facing dictionary of children.
        /// </summary>
        public ReadOnlyDictionary<string, Node> Children => new ReadOnlyDictionary<string, Node>(_children);

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
        /// <param name="className">Node class name</param>
        public Node(string name, Node parent, DSLinkContainer link, string className = "node")
        {
            if (name == null)
            {
                throw new ArgumentException("Name must not be null.");
            }
            ClassName = className;
            Parent = parent;
            _children = new Dictionary<string, Node>();

            Configs = new MetadataMap("$");
            Attributes = new MetadataMap("@");
            PrivateConfigs = new MetadataMap("");

            Configs.OnSet += UpdateSubscribers;
            Attributes.OnSet += UpdateSubscribers;

            _removedChildren = new List<Node>();
            Subscribers = new List<int>();
            Streams = new List<int>();
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
            Configs.Set("is", new Value(ClassName));
        }

        /// <summary>
        /// Initializes the Node Class.
        /// </summary>
        public void InitializeClass()
        {
            if (_initializedClass || _link == null)
            {
                return;
            }
            _initializedClass = true;
            if (_link.Responder.NodeClasses.ContainsKey(ClassName) &&
                (!PrivateConfigs.Has("nodeClassInit") || PrivateConfigs.Get("nodeClassInit").Boolean == false))
            {
                PrivateConfigs.Set("nodeClassInit", new Value(true));
                ResetNode();
                _link.Responder.NodeClasses[ClassName](this);
            }
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

        /// <summary>
        /// Check whether this Node has a value.
        /// </summary>
        /// <value>True when Node has a value</value>
        public bool HasValue => Value != null && !Value.IsNull;

        /// <summary>
        /// Create a child via the NodeFactory.
        /// The node will not be added to the parent until NodeFactory.BuildNode() is called.
        /// </summary>
        /// <param name="name">Node's name</param>
        /// <param name="className">Node's class</param>
        /// <returns>NodeFactory of new child</returns>
        public virtual NodeFactory CreateChild(string name, string className)
        {
            if (name.IndexOfAny(BannedChars) != -1)
            {
                throw new ArgumentException("Invalid character(s) in Node name.");
            }
            Node child = new Node(name, this, _link, className);
            return new NodeFactory(child);
        }

        /// <summary>
        /// Create a child via the NodeFactory.
        /// The node will not be added to the parent until NodeFactory.BuildNode() is called.
        /// </summary>
        /// <param name="name">Node's name</param>
        /// <returns>NodeFactory of new child</returns>
        public virtual NodeFactory CreateChild(string name)
        {
            return CreateChild(name, "node");
        }

        /// <summary>
        /// Add a pre-existing Node as a child.
        /// </summary>
        /// <param name="child">Child Node</param>
        public void AddChild(Node child)
        {
            lock (_childrenLock)
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
        /// Adds a parameter.
        /// </summary>
        /// <param name="parameter">Parameter</param>
        public void AddParameter(Parameter parameter)
        {
            if (!Configs.Has(ConfigType.Parameters))
            {
                Configs.Set(ConfigType.Parameters, new Value(new JArray()));
            }
            var parameters = Configs.Get(ConfigType.Parameters).JArray;
            foreach (JToken token in parameters)
            {
                if (token["name"].Value<string>() == parameter.Name)
                {
                    throw new Exception($"Parameter {parameter.Name} already exists on {_path}");
                }
            }
            parameters.Add(parameter);
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="column">Column</param>
        public void AddColumn(Column column)
        {
            if (!Configs.Has(ConfigType.Columns))
            {
                Configs.Set(ConfigType.Columns, new Value(new JArray()));
            }
            var columns = Configs.Get(ConfigType.Columns).JArray;
            foreach (JToken token in columns)
            {
                if (token["name"].Value<string>() == column.Name)
                {
                    throw new Exception($"Column {column.Name} already exists on {_path}");
                }
            }
            columns.Add(column);
        }

        /// <summary>
        /// Sets the action.
        /// </summary>
        /// <param name="actionHandler">Action</param>
        public void SetAction(ActionHandler actionHandler)
        {
            Configs.Set(ConfigType.Invokable, new Value(actionHandler.Permission.Permit));
            ActionHandler = actionHandler;
        }

        /// <summary>
        /// Remove a Node from being a child.
        /// </summary>
        /// <param name="name">Child Node's name</param>
        public void RemoveChild(string name)
        {
            lock (_childrenLock)
            {
                if (!_children.ContainsKey(name)) return;
                lock (_removedChildrenLock)
                {
                    _removedChildren.Add(_children[name]);
                }
                _children.Remove(name);
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
            List<Task> tasks = new List<Task>();
            lock (Subscribers)
            {
                foreach (var sid in Subscribers)
                {
                    tasks.Add(_link.Connector.AddValueUpdateResponse(new JArray
                    {
                        sid,
                        value.JToken,
                        value.LastUpdated
                    }));
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Serialize the Node updates to an array.
        /// </summary>
        /// <returns>Serialized data</returns>
        public JArray SerializeUpdates()
        {
            var updates = new JArray();

            updates.Merge(Configs.CreateUpdateArray());
            updates.Merge(Attributes.CreateUpdateArray());

            lock (_childrenLock)
            {
                foreach (var child in _children)
                {
                    if (child.Value.Building) continue;
                    var value = new JObject();
                    foreach (var config in child.Value.Configs)
                    {
                        value[config.Key] = config.Value.JToken;
                    }
                    foreach (var attr in child.Value.Attributes)
                    {
                        value[attr.Key] = attr.Value.JToken;
                    }
                    if (child.Value.HasValue)
                    {
                        value["value"] = child.Value.Value.JToken;
                        value["ts"] = child.Value.Value.LastUpdated;
                    }
                    updates.Add(new JArray
                    {
                        child.Key,
                        value
                    });
                }
            }

            lock (_removedChildrenLock)
            {
                foreach (Node node in _removedChildren)
                {
                    updates.Add(new JObject
                    {
                        new JProperty("name", node.Name),
                        new JProperty("change", "remove")
                    });
                }
                _removedChildren.Clear();
            }

            return updates;
        }

        /// <summary>
        /// Trigger node serialization.
        /// </summary>
        public async Task TriggerSerialize()
        {
            await _link.Responder.DiskSerializer.SerializeToDisk();
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
                serializedObject.Add(new JProperty(entry.Key, entry.Value.JToken));
            }

            foreach (var entry in Attributes)
            {
                serializedObject.Add(new JProperty(entry.Key, entry.Value.JToken));
            }

            var privateConfigs = new JObject();
            serializedObject.Add("privateConfigs", privateConfigs);
            foreach (var entry in PrivateConfigs)
            {
                privateConfigs.Add(new JProperty(entry.Key, entry.Value.JToken));
            }

            foreach (var entry in Children)
            {
                if (entry.Value.Serializable)
                {
                    serializedObject.Add(new JProperty(entry.Key, entry.Value.Serialize()));
                }
            }

            if (HasValue)
            {
                serializedObject.Add(new JProperty("?value", Value.JToken));
            }

            if (ClassName != "node")
            {
                serializedObject.Add(new JProperty("?class", ClassName));
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
                        string className;
                        if (prop.Value["?class"] != null)
                        {
                            className = prop.Value["?class"].Value<string>();
                        }
                        else
                        {
                            className = "node";
                        }
                        var builder = CreateChild(name, className);
                        builder.Deserialize((JObject) prop.Value);
                        builder.BuildNode();
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
            var i = path.IndexOf('/');
            var child = i == -1 ? path : path.Substring(0, path.IndexOf('/'));
            path = path.TrimStart(child.ToCharArray());
            try
            {
                return Children[child].Get(path);
            }
            catch (KeyNotFoundException)
            {
                _link?.Logger.Debug($"Non-existant node {path} requested.");
                return null;
            }
        }

        /// <summary>
        /// Clones this node.
        /// </summary>
        /// <returns>A new Node instance that is exactly the same as this node.</returns>
        public Node Clone()
        {
            var node = new Node(Name, Parent, _link, Configs.Get(ConfigType.ClassName).String);
            node.Deserialize(node.Serialize());
            return node;
        }

        internal virtual async void UpdateSubscribers()
        {
            if (Building)
            {
                return;
            }

            if (Streams.Count > 0)
            {
                var responses = new JArray();
                lock (Streams)
                {
                    foreach (var stream in Streams)
                    {
                        responses.Add(new JObject
                        {
                            new JProperty("rid", stream),
                            new JProperty("stream", "open"),
                            new JProperty("updates", SerializeUpdates())
                        });
                    }
                }
                await _link.Connector.Write(new JObject
                {
                    new JProperty("responses", responses)
                });
            }
        }
    }
}
