using System;
using System.Collections.Generic;
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
        public RemoteNode(string name, Node parent) : base(name, parent, null)
        {
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
        public void FromSerialized(List<dynamic> serialized)
        {
            foreach (JArray a in serialized)
            {
                var key = a[0].ToString();
                var value = a[1];
                if (key.StartsWith("$"))
                {
                    SetConfig(key, new Value(value.ToString()));
                }
                else if (key.StartsWith("@"))
                {
                    SetAttribute(key, new Value(value.ToString()));
                }
                else
                {
                    var child = new RemoteNode(key, this);
                    foreach (KeyValuePair<string, JToken> kp in value as JObject)
                    {
                        if (kp.Key.StartsWith("$"))
                        {
                            child.SetConfig(kp.Key, new Value(kp.Value.ToString()));
                        }
                        else if (kp.Key.StartsWith("@"))
                        {
                            child.SetAttribute(kp.Key, new Value(kp.Value.ToString()));
                        }
                    }
                    AddChild(child);
                }
            }
        }
    }
}

