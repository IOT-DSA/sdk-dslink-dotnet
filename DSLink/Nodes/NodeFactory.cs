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
        /// Builds the Node
        /// </summary>
        /// <returns>Node</returns>
        public Node BuildNode()
        {
            _node.Parent.UpdateSubscribers();
            _node.Building = false;
            return _node;
        }

        /// <summary>
        /// Set a config.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Node</returns>
        public NodeFactory SetConfig(string key, Value value)
        {
            _node.SetConfig(key, value);
            return this;
        }

        /// <summary>
        /// Set an attribute.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>Node</returns>
        public NodeFactory SetAttribute(string key, Value value)
        {
            _node.SetAttribute(key, value);
            return this;
        }

        /// <summary>
        /// Sets the display name.
        /// </summary>
        /// <param name="displayName">Display name</param>
        /// <returns>Node</returns>
        public NodeFactory SetDisplayName(string displayName)
        {
            _node.DisplayName = displayName;
            return this;
        }

        /// <summary>
        /// Sets the profile.
        /// </summary>
        /// <param name="profile">Profile</param>
        /// <returns>Node</returns>
        public NodeFactory SetProfile(string profile)
        {
            _node.Profile = profile;
            return this;
        }

        /// <summary>
        /// Sets the writable permission.
        /// </summary>
        /// <param name="writable">Writable permission</param>
        /// <returns>Node</returns>
        public NodeFactory SetWritable(Permission writable)
        {
            _node.Writable = writable;
            return this;
        }

        /// <summary>
        /// Sets the invokable permission.
        /// </summary>
        /// <param name="invokable">Invokable permission</param>
        /// <returns>Node</returns>
        public NodeFactory SetInvokable(Permission invokable)
        {
            _node.Invokable = invokable;
            return this;
        }

        /// <summary>
        /// Sets the action group.
        /// </summary>
        /// <param name="actionGroup">Action group</param>
        /// <returns>Node</returns>
        public NodeFactory SetActionGroup(string actionGroup)
        {
            _node.ActionGroup = actionGroup;
            return this;
        }

        /// <summary>
        /// Sets the action group subtitle.
        /// </summary>
        /// <param name="actionGroupSubtitle">Action group subtitle</param>
        /// <returns>Node</returns>
        public NodeFactory SetActionGroupSubtitle(string actionGroupSubtitle)
        {
            _node.ActionGroupSubtitle = actionGroupSubtitle;
            return this;
        }

        /// <summary>
        /// Sets whether the node is serializable.
        /// </summary>
        /// <param name="serializable">Serializable</param>
        /// <returns>Node</returns>
        public NodeFactory SetSerializable(bool serializable)
        {
            _node.Serializable = true;
            return this;
        }

        /// <summary>
        /// Sets the type.
        /// </summary>
        /// <param name="type">Type string</param>
        /// <returns>Node</returns>
        public NodeFactory SetType(string type)
        {
            // TODO: Check for valid type.
            _node.SetConfig("type", new Value(type));
            return this;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Node</returns>
        public NodeFactory SetValue(dynamic value)
        {
            _node.Value.Set(value);
            return this;
        }

        /// <summary>
        /// Sets the action.
        /// </summary>
        /// <param name="actionHandler">Action</param>
        /// <returns>Node</returns>
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
        /// <returns>Node</returns>
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
        /// <returns>Node</returns>
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
