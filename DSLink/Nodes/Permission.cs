using System.Collections.Generic;

namespace DSLink.Nodes
{
    public class Permission
    {
        public static readonly Permission Read = new Permission("read");
        public static readonly Permission Write = new Permission("write");
        public static readonly Permission Config = new Permission("config");
        public static readonly Permission Never = new Permission("never");

        internal static Dictionary<string, Permission> _permMap = new Dictionary<string, Permission>
        {
            {"read", Read},
            {"write", Write},
            {"config", Config},
            {"never", Never}
        };

        internal string Permit;

        internal Permission(string permit)
        {
            Permit = permit;
        }

        public override string ToString()
        {
            return Permit;
        }
    }
}
