using System;
using System.Collections.Generic;
using DSLink.Request;
using Newtonsoft.Json.Linq;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DSLink.Nodes.Actions.ActionHandler"/> class.
        /// </summary>
        /// <param name="permission">Permission</param>
        /// <param name="function">Function</param>
        public ActionHandler(Permission permission, Action<InvokeRequest> function)
        {
            Permission = permission;
            Function = function;
        }
    }
}
