using DSLink.Nodes.Actions;
using Newtonsoft.Json.Linq;

namespace DSLink.Nodes
{
    /// <summary>
    /// Builder class to help simplify the process of creating a Node.
    /// </summary>
    public class NodeFactory
    {
        private readonly Node _node;

        public NodeFactory(Node node)
        {
            _node = node;
            _node.Building = true;
        }

        public Node BuildNode()
        {
            _node.Building = false;
            _node.Parent.AddChild(_node);
            _node.InitializeClass();
            return _node;
        }

        public NodeFactory SetNodeClass(string className)
        {
            _node.ClassName = className;
            return this;
        }

        public NodeFactory SetConfig(string name, Value value)
        {
            _node.SetConfig(name, value);
            return this;
        }

        public NodeFactory SetAttribute(string name, Value value)
        {
            _node.SetAttribute(name, value);
            return this;
        }

        public NodeFactory SetDisplayName(string displayName)
        {
            _node.DisplayName = displayName;
            return this;
        }

        public NodeFactory SetProfile(string profile)
        {
            _node.Profile = profile;
            return this;
        }

        public NodeFactory SetWritable(Permission writable)
        {
            _node.Writable = writable;
            return this;
        }

        public NodeFactory SetInvokable(Permission invokable)
        {
            _node.Invokable = invokable;
            return this;
        }

        public NodeFactory SetActionGroup(string actionGroup)
        {
            _node.ActionGroup = actionGroup;
            return this;
        }

        public NodeFactory SetActionGroupSubtitle(string actionGroupSubtitle)
        {
            _node.ActionGroupSubtitle = actionGroupSubtitle;
            return this;
        }

        public NodeFactory SetSerializable(bool serializable)
        {
            _node.Serializable = serializable;
            return this;
        }

        public NodeFactory SetType(ValueType valueType)
        {
            _node.ValueType = valueType;
            return this;
        }

        public NodeFactory SetResult(ResultType resultType)
        {
            _node.Result = resultType;
            return this;
        }

        public NodeFactory SetValue(dynamic value)
        {
            _node.Value.Set(value);
            return this;
        }

        public NodeFactory SetAction(ActionHandler actionHandler)
        {
            _node.SetAction(actionHandler);
            return this;
        }

        public NodeFactory AddParameter(Parameter parameter)
        {
            _node.AddParameter(parameter);
            return this;
        }

        public NodeFactory AddColumn(Column column)
        {
            _node.AddColumn(column);
            return this;
        }

        public void Deserialize(JObject obj)
        {
            _node.Deserialize(obj);
        }
    }
}
