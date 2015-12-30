using System;
using System.Collections.Generic;

namespace DSLink.Nodes.Actions
{
    public class Action
    {
        public readonly Permission Permission;
        public readonly Func<Dictionary<string, Value>, List<dynamic>> Function;

        public Action(Permission permission, Func<Dictionary<string, Value>, List<dynamic>> function)
        {
            Permission = permission;
            Function = function;
        }
    }
}
