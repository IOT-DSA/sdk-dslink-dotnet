using System.Collections.Generic;

namespace DSLink.Nodes
{
    /// <summary>
    /// Represents permissions for DSA.
    /// </summary>
    public class Permission
    {
        public static readonly Permission Read = new Permission("read");
        public static readonly Permission Write = new Permission("write");
        public static readonly Permission Config = new Permission("config");
        public static readonly Permission Never = new Permission("never");

        /// <summary>
        /// Dictionary mapping strings to permissions.
        /// </summary>
        internal static Dictionary<string, Permission> PermissionMap = new Dictionary<string, Permission>
        {
            {"read", Read},
            {"write", Write},
            {"config", Config},
            {"never", Never}
        };

        /// <summary>
        /// Permission string.
        /// </summary>
        internal string Permit;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Nodes.Permission"/> class.
        /// </summary>
        /// <param name="permit">Permit</param>
        internal Permission(string permit)
        {
            Permit = permit;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        public override string ToString()
        {
            return Permit;
        }

        /// <summary>
        /// Converts from string.
        /// </summary>
        /// <param name="permit">Permission</param>
        /// <returns>Permission from specific string</returns>
        public static Permission FromString(string permit)
        {
            if (string.IsNullOrEmpty(permit))
            {
                return null;
            }
            permit = permit.ToLower();
            return PermissionMap.ContainsKey(permit) ? PermissionMap[permit] : null;
        }
    }
}
