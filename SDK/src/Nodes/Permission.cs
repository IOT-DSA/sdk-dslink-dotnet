namespace DSLink.Nodes
{
    public class Permission
    {
        public static readonly Permission Read = new Permission("read");
        public static readonly Permission Write = new Permission("write");
        public static readonly Permission Config = new Permission("config");
        public static readonly Permission Never = new Permission("never");

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
