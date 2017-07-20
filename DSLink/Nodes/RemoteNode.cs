using System;
using Newtonsoft.Json.Linq;

namespace DSLink.Nodes
{
    /// <summary>
    /// Represents a remote Node that isn't on our Node tree.
    /// </summary>
    public class RemoteNode : Node
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Nodes.RemoteNode"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parent">Parent</param>
        /// <param name="path">Path of Node</param>
        public RemoteNode(string name, Node parent, string path) : base(name, parent, null)
        {
            Path = path;
        }

        /// <summary>
        /// <see cref="Node"/>
        /// </summary>
        public override NodeFactory CreateChild(string name)
        {
            throw new InvalidOperationException("Cannot create a remote node");
        }

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="value">Value</param>
        protected override void ValueSet(Value value)
        {
        }

        /// <summary>
        /// Updates the subscribers.
        /// </summary>
        internal override void UpdateSubscribers()
        {
        }

        /// <summary>
        /// Deserializes.
        /// </summary>
        /// <param name="serialized">Serialized</param>
        public void FromSerialized(JArray serialized)
        {
            foreach (var jToken in serialized)
            {
                var a = (JArray) jToken;
                var key = a[0].ToString();
                var value = a[1];
                if (key.StartsWith("$"))
                {
                    key = key.Substring(1);
                    if (key.Equals("params") && value.Type == JTokenType.Array)
                    {
                        Configs.Set(key, new Value(value.Value<JArray>()));
                    }
                    else if (key.Equals("columns") && value.Type == JTokenType.Array)
                    {
                        Configs.Set(key, new Value(value.Value<JArray>()));
                    }
                    else
                    {
                        Configs.Set(key, new Value(value.ToString()));
                    }
                }
                else if (key.StartsWith("@"))
                {
                    key = key.Substring(1);
                    Attributes.Set(key, new Value(value.ToString()));
                }
                else
                {
                    var child = new RemoteNode(key, this, Path + "/" + key);
                    var jObject = value as JObject;
                    if (jObject != null)
                    {
                        foreach (var kp in jObject)
                        {
                            if (kp.Key.StartsWith("$"))
                            {
                                child.Configs.Set(kp.Key.Substring(1), new Value(kp.Value.ToString()));
                            }
                            else if (kp.Key.StartsWith("@"))
                            {
                                child.Attributes.Set(kp.Key.Substring(1), new Value(kp.Value.ToString()));
                            }
                        }
                    }
                    AddChild(child);
                }
            }
        }
    }
}
