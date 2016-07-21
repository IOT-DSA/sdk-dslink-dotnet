﻿using System;
using System.Collections.Generic;
using DSLink.Nodes.Actions;
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
            foreach (JArray a in serialized)
            {
                var key = a[0].ToString();
                var value = a[1];
                if (key.StartsWith("$"))
                {
                    key = key.Substring(1);
                    if (key.Equals("params") && value.Type == JTokenType.Array)
                    {
                        var parameters = new List<Parameter>();
                        foreach (var parameter in value.Value<JArray>())
                        {
                            parameters.Add(parameter.ToObject<Parameter>());
                        }
                        SetConfig(key, new Value(parameters));
                    }
                    else if (key.Equals("columns") && value.Type == JTokenType.Array)
                    {
                        var columns = new List<Column>();
                        foreach (var column in value.Value<JArray>())
                        {
                            columns.Add(column.ToObject<Column>());
                        }
                        SetConfig(key, new Value(columns));
                    }
                    else
                    {
                        SetConfig(key, new Value(value.ToString()));
                    }
                }
                else if (key.StartsWith("@"))
                {
                    key = key.Substring(1);
                    SetAttribute(key, new Value(value.ToString()));
                }
                else
                {
                    var child = new RemoteNode(key, this, Path + "/" + key);
                    foreach (KeyValuePair<string, JToken> kp in value as JObject)
                    {
                        if (kp.Key.StartsWith("$"))
                        {
                            child.SetConfig(kp.Key.Substring(1), new Value(kp.Value.ToString()));
                        }
                        else if (kp.Key.StartsWith("@"))
                        {
                            child.SetAttribute(kp.Key.Substring(1), new Value(kp.Value.ToString()));
                        }
                    }
                    AddChild(child);
                }
            }
        }
    }
}

