using System;
using System.Collections.Generic;
using DSLink.Request;

namespace DSLink.Nodes.Actions
{
    public class Action
    {
        public readonly Permission Permission;
        public readonly Action<Dictionary<string, Value>, InvokeRequest> Function;

        public Action(Permission permission, Action<Dictionary<string, Value>, InvokeRequest> function)
        {
            Permission = permission;
            Function = function;
        }
    }
}
