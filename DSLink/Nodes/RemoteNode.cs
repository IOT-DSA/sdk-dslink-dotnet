using System;
using Newtonsoft.Json.Linq;

namespace DSLink.Nodes
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a remote Node that isn't on our Node tree.
    /// </summary>
    public class RemoteNode : Node
    {
        public RemoteNode(string name, Node parent, string path) : base(name, parent, null)
        {
            Path = path;
        }

        /// <inheritdoc />
        public override NodeFactory CreateChild(string name)
        {
            throw new InvalidOperationException("Cannot create a remote node");
        }

        /// <inheritdoc />
        protected override void ValueSet(Value value)
        {
        }

        /// <inheritdoc />
        protected override void UpdateSubscribers()
        {
        }

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
                    if (value is JObject jObject)
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