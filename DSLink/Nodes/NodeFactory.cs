using DSLink.Nodes.Actions;
using Newtonsoft.Json.Linq;
using System.Security;

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
            _node.InitializeClass();
        }

        public Node BuildNode()
        {
            _node.Building = false;
            _node.Parent.AddChild(_node);
            return _node;
        }

        public NodeFactory SetNodeClass(string className)
        {
            _node.ClassName = className;
            return this;
        }

        public NodeFactory SetConfig(string name, Value value)
        {
            _node.Configs.Set(name, value);
            return this;
        }

        public NodeFactory SetConfig(BaseType type, Value value)
        {
            return SetConfig(type.String, value);
        }

        public NodeFactory SetEncryptedConfig(string name, Value value, SecureString password, byte[] salt)
        {
            _node.Configs.SetEncrypted(name, value, password, salt);
            return this;
        }

        public NodeFactory SetEncryptedConfig(BaseType type, Value value, SecureString password, byte[] salt)
        {
            return SetEncryptedConfig(type.String, value, password, salt);
        }

        public NodeFactory SetAttribute(string name, Value value)
        {
            _node.Attributes.Set(name, value);
            return this;
        }

        public NodeFactory SetAttribute(BaseType type, Value value)
        {
            return SetAttribute(type.String, value);
        }

        public NodeFactory SetDisplayName(string displayName)
        {
            SetConfig(ConfigType.DisplayName, new Value(displayName));
            return this;
        }

        public NodeFactory SetHidden(bool hidden)
        {
            SetConfig(ConfigType.Hidden, new Value(hidden));
            return this;
        }

        public NodeFactory SetClassName(string className)
        {
            SetConfig(ConfigType.ClassName, new Value(className));
            return this;
        }

        public NodeFactory SetWritable(Permission writable)
        {
            SetConfig(ConfigType.Writable, new Value(writable.Permit));
            return this;
        }

        public NodeFactory SetInvokable(Permission invokable)
        {
            SetConfig(ConfigType.Invokable, new Value(invokable.Permit));
            return this;
        }

        public NodeFactory SetActionGroup(string actionGroup)
        {
            SetConfig(ConfigType.ActionGroup, new Value(actionGroup));
            return this;
        }

        public NodeFactory SetActionGroupSubtitle(string actionGroupSubtitle)
        {
            SetConfig(ConfigType.ActionGroupSubtitle, new Value(actionGroupSubtitle));
            return this;
        }

        public NodeFactory SetSerializable(bool serializable)
        {
            _node.Serializable = serializable;
            return this;
        }

        public NodeFactory SetUserObject(object userObject)
        {
            _node.UserObject = userObject;
            return this;
        }

        public NodeFactory SetType(ValueType valueType)
        {
            SetConfig(ConfigType.ValueType, valueType.TypeValue);
            return this;
        }

        public NodeFactory SetResult(ResultType resultType)
        {
            SetConfig(ConfigType.Result, resultType.Value);
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
