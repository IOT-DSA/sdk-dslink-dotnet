using System;
using DSLink.Request;

namespace DSLink.Nodes.Actions
{
    /// <summary>
    /// Used for action callbacks and permission.
    /// </summary>
    public class ActionHandler
    {
        /// <summary>
        /// Permission of the action.
        /// </summary>
        public readonly Permission Permission;

        /// <summary>
        /// Callback for action.
        /// </summary>
        public readonly Action<InvokeRequest> Function;

        public ActionHandler(Permission permission, Action<InvokeRequest> function)
        {
            Permission = permission;
            Function = function;
        }
    }
}