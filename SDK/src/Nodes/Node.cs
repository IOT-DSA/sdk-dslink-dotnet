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
        private static readonly char[] BannedChars = {
            '%', '.', '/', '\\', '?', '*', ':', '|', '<', '>', '$', '@', ','
        };

        private readonly IDictionary<string, Node> _children;
        private readonly IDictionary<string, Value> _configs;
        private readonly IDictionary<string, Value> _attributes;
        private readonly IList<Node> _removedChildren; 
        internal readonly List<int> Subscribers;
        internal readonly List<int> Streams; 
        private readonly AbstractContainer _link;

        private readonly object _childrenLock = new object();
        private readonly object _removedChildrenLock = new object();

        public string Name { get; }
        public string Path { get; }
        public Node Parent { get; }
        public Value Value { get; }
        public Action Action;
        public bool Transient;
        internal bool Building;
        public bool Subscribed => Subscribers.Count != 0;

        public ReadOnlyDictionary<string, Node> Children => new ReadOnlyDictionary<string, Node>(_children);
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

        public Node(string name, Node parent, AbstractContainer link)
        {
            if (name.IndexOfAny(BannedChars) != -1)
            {
                throw new ArgumentException("Invalid character(s) in Node name");
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

            if (name == null)
            {
                throw new ArgumentException("name");
            }

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

        public void SetConfig(string key, Value value)
        {
            UpdateSubscribers();
            _configs["$" + key] = value;
        }

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

        public void SetAttribute(string key, Value value)
        {
            UpdateSubscribers();
            _attributes["@" + key] = value;
        }

        public Value GetAttribute(string key)
        {
            return _attributes["@" + key];
        }

        public void SetDisplayName(string displayName)
        {
            SetConfig("name", new Value(displayName));
        }

        public void SetProfile(string profile)
        {
            SetConfig("is", new Value(profile));
        }

        public void SetWritable(Permission permission)
        {
            SetConfig("writable", new Value(permission.ToString()));
        }

        public void SetInvokable(Permission permission)
        {
            SetConfig("invokable", new Value(permission.ToString()));
        }

        public void SetParameters(List<Parameter> parameters)
        {
            SetConfig("params", new Value(parameters));
        }

        public void SetColumns(List<Column> columns)
        {
            SetConfig("columns", new Value(columns));
        }

        public bool HasValue()
        {
            return Value.Get() != null;
        }

        public NodeFactory CreateChild(string name)
        {
            Node child = new Node(name, this, _link);
            AddChild(child);
            return new NodeFactory(child);
        }

        public void AddChild(Node child)
        {
            lock (_childrenLock)
            {
                _children.Add(child.Name, child);
            }
            UpdateSubscribers();
        }

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

        public void ValueSet(Value value)
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
            foreach (var sid in Subscribers)
            {
                rootObject.Responses[0].Updates.Add(new[] {sid, value.Get(), value.LastUpdated});
            }
            _link.Connector.Write(rootObject);
        }

        public List<dynamic> Serialize()
        {
            var val = _configs.Select(config => new List<dynamic> {config.Key, config.Value.Get()}).Cast<dynamic>().ToList();
            val.AddRange(_attributes.Select(attribute => new List<dynamic> {attribute.Key, attribute.Value.Get()}));

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
                    val.Add(new List<dynamic>{child.Key, value});
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

        internal void UpdateSubscribers()
        {
            if (Building)
            {
                return;
            }
            var responses = Streams.Select(stream => new ResponseObject
            {
                RequestId = stream, Stream = "open", Updates = Serialize()
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

        internal static int NthIndexOf(string target, string value, int n)
        {
            var m = Regex.Match(target, "((" + Regex.Escape(value) + ").*?){" + n + "}");
            return m.Success ? m.Groups[2].Captures[n - 1].Index : -1;
        }
    }
}
