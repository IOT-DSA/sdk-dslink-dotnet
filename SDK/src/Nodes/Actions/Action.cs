using System;
using System.Collections.Generic;

namespace DSLink.Nodes.Actions
{
    public class Action
    {
        // TODO: Swap out dynamic for Values.
        public readonly Permission Permission;
        public readonly Func<Dictionary<string, Parameter>, Dictionary<string, dynamic>> Function;

        public Action(Permission permission, Func<Dictionary<string, Parameter>, Dictionary<string, dynamic>> function)
        {
            Permission = permission;
            Function = function;
        }
    }
}
