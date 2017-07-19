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
        /// Config values store data on the configuration of
        /// a Node or its children.
        /// </summary>
        private readonly IDictionary<string, Value> _configs;

        /// <summary>
        /// Attributes store data specific values.
        /// </summary>
        private readonly IDictionary<string, Value> _attributes;

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
        public string Name
        {
            get;
        }

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
        /// Public-facing dictionary of configurations.
        /// </summary>
        /// <value>The configurations.</value>
        public ReadOnlyDictionary<string, Value> Configurations => new ReadOnlyDictionary<string, Value>(_configs);

        /// <summary>
        /// Public-facing dictionary of attributes.
        /// </summary>
        public ReadOnlyDictionary<string, Value> Attributes => new ReadOnlyDictionary<string, Value>(_attributes);

        /// <summary>
        /// Private configuration values only stored locally, these
        /// are not available over DSA.
        /// </summary>
        public readonly IDictionary<string, Value> PrivateConfigs;

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
            _configs = new Dictionary<string, Value>();
            _attributes = new Dictionary<string, Value>();
            PrivateConfigs = new Dictionary<string, Value>();
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
            _configs["$is"] = new Value(ClassName);
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
                (!PrivateConfigs.ContainsKey("nodeClassInit") || PrivateConfigs["nodeClassInit"].Boolean == false))
            {
                PrivateConfigs["nodeclassInit"] = new Value(true);
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
            _configs.Clear();
            _attributes.Clear();
            PrivateConfigs.Clear();
            Value.Clear();

            _createInitialData();
        }

        /// <summary>
        /// Set a Node configuration.
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="value">Configuration value</param>
        public void SetConfig(string key, Value value)
        {
            _configs["$" + key] = value;
            UpdateSubscribers();
        }

        /// <summary>
        /// Get a Node configuration.
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <returns>Attribute value</returns>
        public Value GetConfig(string key)
        {
            var name = "$" + key;
            if (!_configs.ContainsKey(name)) return null;
            return _configs[name];
        }

        /// <summary>
        /// Set a Node attribute.
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <param name="value">Attribute value</param>
        public void SetAttribute(string key, Value value)
        {
            _attributes["@" + key] = value;
            UpdateSubscribers();
        }

        /// <summary>
        /// Get a Node attribute.
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <returns>Attribute value</returns>
        public Value GetAttribute(string key)
        {
            var name = "@" + key;
            if (!_attributes.ContainsKey(name)) return null;
            return _attributes[name];
        }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>Display name</value>
        public string DisplayName
        {
            get
            {
                var config = GetConfig("name");
                return config?.String;
            }
            set
            {
                SetConfig("name", new Value(value));
            }
        }

        /// <summary>
        /// Gets or sets the profile.
        /// </summary>
        /// <value>Node profile</value>
        public string Profile
        {
            get
            {
                var config = GetConfig("is");
                return config?.String;
            }
            set
            {
                SetConfig("is", new Value(value));
            }
        }

        /// <summary>
        /// Gets or sets the value type.
        /// </summary>
        /// <value>Value type</value>
        public ValueType ValueType
        {
            get
            {
                var config = GetConfig("type");
                return config != null ? ValueType.FromString(config.String) : null;
            }
            set
            {
                SetConfig("type", value.TypeValue);
            }
        }

        /// <summary>
        /// Gets or sets the result type.
        /// </summary>
        /// <value>Result type</value>
        public ResultType Result
        {
            get
            {
                var config = GetConfig("result");
                return config != null ? ResultType.FromString(config.String) : null;
            }
            set
            {
                SetConfig("result", value.Value);
            }
        }

        /// <summary>
        /// Gets or sets the writable permission.
        /// </summary>
        /// <value>Writable permission</value>
        public Permission Writable
        {
            get
            {
                var writable = GetConfig("writable");
                return writable != null ? Permission.FromString(writable.String) : null;
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
                var config = GetConfig("invokable");
                return config != null ? Permission.FromString(config.String) : null;
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
        public JArray Parameters
        {
            get
            {
                var config = GetConfig("params");
                return config?.JArray;
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
        public JArray Columns
        {
            get
            {
                var columns = GetConfig("columns");
                return columns?.JArray;
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
                var actionGroup = GetConfig("actionGroupSubTitle");
                return actionGroup?.String;
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
                var actionGroupSubtitle = GetConfig("actionGroupSubTitle");
                return actionGroupSubtitle?.String;
            }
            set
            {
                SetConfig("actionGroupSubTitle", new Value(value));
            }
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
            if (Parameters == null)
            {
                Parameters = new JArray();
            }
            foreach (JToken token in Parameters)
            {
                if (token["name"].Value<string>() == parameter.Name)
                {
                    throw new Exception($"Parameter {parameter.Name} already exists on {_path}");
                }
            }
            Parameters.Add(parameter);
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="column">Column</param>
        public void AddColumn(Column column)
        {
            if (Columns == null)
            {
                Columns = new JArray();
            }
            foreach (JToken token in Columns)
            {
                if (token["name"].Value<string>() == column.Name)
                {
                    throw new Exception($"Column {column.Name} already exists on {_path}");
                }
            }
            Columns.Add(column);
        }

        /// <summary>
        /// Sets the action.
        /// </summary>
        /// <param name="actionHandler">Action</param>
        public void SetAction(ActionHandler actionHandler)
        {
            Invokable = actionHandler.Permission;
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
            var val = new JArray();

            foreach (var pair in _configs)
            {
                val.Add(new JArray
                {
                    pair.Key,
                    pair.Value.JToken
                });
            }

            foreach (var pair in _attributes)
            {
                val.Add(new JArray
                {
                    pair.Key,
                    pair.Value.JToken
                });
            }

            lock (_childrenLock)
            {
                foreach (var child in _children)
                {
                    if (child.Value.Building) continue;
                    var value = new JObject();
                    foreach (var config in child.Value._configs)
                    {
                        value[config.Key] = config.Value.JToken;
                    }
                    foreach (var attr in child.Value._attributes)
                    {
                        value[attr.Key] = attr.Value.JToken;
                    }
                    if (child.Value.HasValue)
                    {
                        value["value"] = child.Value.Value.JToken;
                        value["ts"] = child.Value.Value.LastUpdated;
                    }
                    val.Add(new JArray
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
                    val.Add(new JObject
                    {
                        new JProperty("name", node.Name),
                        new JProperty("change", "remove")
                    });
                }
                _removedChildren.Clear();
            }

            return val;
        }

        /// <summary>
        /// Trigger node serialization.
        /// </summary>
        public async Task TriggerSerialize()
        {
            await _link.Responder.NodeSerializer.SerializeToDisk();
        }

        /// <summary>
        /// Save the Node to a JSON object.
        /// </summary>
        /// <returns>Serialized data</returns>
        public JObject Serialize()
        {
            var obj = new JObject();

            foreach (var entry in Configurations)
            {
                obj.Add(new JProperty(entry.Key, entry.Value.JToken));
            }

            foreach (var entry in Attributes)
            {
                obj.Add(new JProperty(entry.Key, entry.Value.JToken));
            }

            var privateConfigs = new JObject();
            obj.Add("privateConfigs", privateConfigs);
            foreach (var entry in PrivateConfigs)
            {
                privateConfigs.Add(new JProperty(entry.Key, entry.Value.JToken));
            }

            foreach (var entry in Children)
            {
                if (entry.Value.Serializable)
                {
                    obj.Add(new JProperty(entry.Key, entry.Value.Serialize()));
                }
            }

            if (HasValue)
            {
                obj.Add(new JProperty("?value", Value.JToken));
            }

            if (ClassName != "node")
            {
                obj.Add(new JProperty("?class", ClassName));
            }

            return obj;
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
                    SetConfig(prop.Key.Substring(1), new Value(prop.Value));
                }
                else if (prop.Key.StartsWith("@"))
                {
                    SetAttribute(prop.Key.Substring(1), new Value(prop.Value));
                }
                else if (prop.Key == "privateConfigs")
                {
                    foreach (var entry in prop.Value.Value<JObject>())
                    {
                        PrivateConfigs[entry.Key] = new Value(entry.Value);
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
            var node = new Node(Name, Parent, _link, Profile);
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
