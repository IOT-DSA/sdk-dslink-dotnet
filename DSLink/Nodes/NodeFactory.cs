using System.Collections.Generic;
using DSLink.Nodes.Actions;
using Newtonsoft.Json.Linq;

namespace DSLink.Nodes
{
    /// <summary>
    /// Easy builder class for a Node.
    /// </summary>
    public class NodeFactory
    {
        /// <summary>
        /// Node that is being built on.
        /// </summary>
        private readonly Node _node;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Nodes.NodeFactory"/> class.
        /// </summary>
        /// <param name="node">Node.</param>
        public NodeFactory(Node node)
        {
            _node = node;
            _node.Building = true;
        }

        /// <summary>
        /// Builds the Node.
        /// </summary>
        /// <returns>Node</returns>
        public Node BuildNode()
        {
            _node.Building = false;
            _node.Parent.UpdateSubscribers();
            return _node;
        }

        /// <summary>
        /// Set a config.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetConfig(string name, Value value)
        {
            _node.SetConfig(name, value);
            return this;
        }

        /// <summary>
        /// Set an attribute.
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <param name="value">Attribute value</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetAttribute(string name, Value value)
        {
            _node.SetAttribute(name, value);
            return this;
        }

        /// <summary>
        /// Sets the display name.
        /// </summary>
        /// <param name="displayName">Display name</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetDisplayName(string displayName)
        {
            _node.DisplayName = displayName;
            return this;
        }

        /// <summary>
        /// Sets the profile.
        /// </summary>
        /// <param name="profile">Profile</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetProfile(string profile)
        {
            _node.Profile = profile;
            return this;
        }

        /// <summary>
        /// Sets the writable permission.
        /// </summary>
        /// <param name="writable">Writable permission</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetWritable(Permission writable)
        {
            _node.Writable = writable;
            return this;
        }

        /// <summary>
        /// Sets the invokable permission.
        /// </summary>
        /// <param name="invokable">Invokable permission</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetInvokable(Permission invokable)
        {
            _node.Invokable = invokable;
            return this;
        }

        /// <summary>
        /// Sets the action group.
        /// </summary>
        /// <param name="actionGroup">Action group</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetActionGroup(string actionGroup)
        {
            _node.ActionGroup = actionGroup;
            return this;
        }

        /// <summary>
        /// Sets the action group subtitle.
        /// </summary>
        /// <param name="actionGroupSubtitle">Action group subtitle</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetActionGroupSubtitle(string actionGroupSubtitle)
        {
            _node.ActionGroupSubtitle = actionGroupSubtitle;
            return this;
        }

        /// <summary>
        /// Sets whether the node is serializable.
        /// </summary>
        /// <param name="serializable">Serializable</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetSerializable(bool serializable)
        {
            _node.Serializable = true;
            return this;
        }

        /// <summary>
        /// Sets the type.
        /// </summary>
        /// <param name="valueType">ValueType</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetType(ValueType valueType)
        {
            _node.ValueType = valueType;
            return this;
        }

        /// <summary>
        /// Sets the result.
        /// </summary>
        /// <param name="resultType">Result type.</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetResult(ResultType resultType)
        {
            _node.Result = resultType;
            return this;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetValue(dynamic value)
        {
            _node.Value.Set(value);
            return this;
        }

        /// <summary>
        /// Sets the action.
        /// </summary>
        /// <param name="actionHandler">Action</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory SetAction(ActionHandler actionHandler)
        {
            _node.Invokable = actionHandler.Permission;
            _node.ActionHandler = actionHandler;
            return this;
        }

        /// <summary>
        /// Adds a parameter.
        /// </summary>
        /// <param name="parameter">Parameter</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory AddParameter(Parameter parameter)
        {
            if (_node.Parameters == null)
            {
                _node.Parameters = new JArray();
            }
            _node.Parameters.Add(parameter);
            return this;
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="column">Column</param>
        /// <returns>NodeFactory</returns>
        public NodeFactory AddColumn(Column column)
        {
            if (_node.Columns == null)
            {
                _node.Columns = new JArray();
            }
            _node.Columns.Add(column);
            return this;
        }
    }
}
