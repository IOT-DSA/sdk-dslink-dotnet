using System.Collections.Generic;

namespace DSLink.Nodes
{
    /// <summary>
    /// Represents permissions for DSA.
    /// </summary>
    public class Permission
    {
        public static readonly Permission Never = new Permission("never");
        public static readonly Permission Read = new Permission("read");
        public static readonly Permission Write = new Permission("write");
        public static readonly Permission Config = new Permission("config");

        /// <summary>
        /// Dictionary mapping strings to permissions.
        /// </summary>
        internal static readonly Dictionary<string, Permission> PermissionMap = new Dictionary<string, Permission>
        {
            {"never", Never},
            {"read", Read},
            {"write", Write},
            {"config", Config}
        };

        /// <summary>
        /// Permission string used in the DSA protocol.
        /// </summary>
        public readonly string Permit;

        private Permission(string permit)
        {
            Permit = permit;
        }

        /// <summary>
        /// Converts this Permission to its permission string.
        /// </summary>
        public override string ToString()
        {
            return Permit;
        }
    }
}