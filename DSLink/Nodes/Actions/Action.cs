using System;
using System.Collections.Generic;
using DSLink.Request;

namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Used for action callbacks and permission.
    /// </summary>
    public class Action
    {
        /// <summary>
        /// Permission of the action.
        /// </summary>
        public readonly Permission Permission;

        /// <summary>
        /// Callback for action.
        /// </summary>
        public readonly Action<Dictionary<string, Value>, InvokeRequest> Function;

        public Action(Permission permission, Action<Dictionary<string, Value>, InvokeRequest> function)
        {
            Permission = permission;
            Function = function;
        }
    }
}
