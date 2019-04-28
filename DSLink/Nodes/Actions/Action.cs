using System;
using System.Collections.Generic;
using DSLink.Request;

namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Wrap 
    /// </summary>
    public class Action
    {
        /// <summary>
        /// Permission of requester required to be able to invoke this Action.
        /// </summary>
        public readonly Permission Permission;

        /// <summary>
        /// Callback for when the Action is invoked.
        /// </summary>
        public readonly Action<InvokeRequest> Function;

        /// <summary>
        /// List of Parameters in which to add to the action.
        /// </summary>
        public readonly List<Parameter> Parameters = new List<Parameter>();

        public Action(Permission permission, Action<InvokeRequest> function)
        {
            Permission = permission;
            Function = function;
        }
    }
}