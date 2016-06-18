using System;
using System.Collections.Generic;
using DSLink.Container;
using Newtonsoft.Json.Linq;

namespace DSLink.Nodes
{
    public class RemoteNode : Node
    {
        public RemoteNode(string name, Node parent) : base(name, parent, null)
        {
        }

        public override NodeFactory CreateChild(string name)
        {
            throw new InvalidOperationException("Cannot create a remote node");
        }

        protected override void ValueSet(Value value)
        {
        }

        internal override void UpdateSubscribers()
        {
        }

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

