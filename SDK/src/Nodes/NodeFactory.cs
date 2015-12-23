using System.Collections.Generic;
using DSLink.Nodes.Actions;

namespace DSLink.Nodes
{
    public class NodeFactory
    {
        private readonly Node _node;

        public NodeFactory(Node node)
        {
            _node = node;
        }

        public Node BuildNode()
        {
            _node.Parent.UpdateSubscribers();
            return _node;
        }

        public NodeFactory SetConfig(string key, Value value)
        {
            _node.SetConfig(key, value);
            return this;
        }

        public NodeFactory SetAttribute(string key, Value value)
        {
            _node.SetAttribute(key, value);
            return this;
        }

        public NodeFactory SetDisplayName(string displayName)
        {
            _node.SetDisplayName(displayName);
            return this;
        }

        public NodeFactory SetProfile(string profile)
        {
            _node.SetProfile(profile);
            return this;
        }

        public NodeFactory SetWritable(Permission writable)
        {
            _node.SetWritable(writable);
            return this;
        }

        public NodeFactory SetTransient(bool transient)
        {
            _node.Transient = true;
            return this;
        }

        public NodeFactory SetType(string type)
        {
            // TODO: Check for valid type.
            _node.SetConfig("type", new Value(type));
            return this;
        }

        public NodeFactory SetAction(Action action)
        {
            _node.SetInvokable(action.Permission);
            _node.Action = action;
            return this;
        }

        public NodeFactory AddParameter(Parameter parameter)
        {
            if (_node.GetConfig("params") == null)
            {
                _node.SetParameters(new List<Parameter>());
            }
            _node.GetConfig("params").Get().Add(parameter);
            return this;
        }

        public NodeFactory AddColumn(Column column)
        {
            if (_node.GetConfig("columns") == null)
            {
                _node.SetColumns(new List<Column>());
            }
            _node.GetConfig("columns").Get().Add(column);
            return this;
        }
    }
}
