namespace DSLink.Nodes
{
    public abstract class BaseType
    {
        public readonly string String;

        internal BaseType(string str)
        {
            String = str;
        }
    }

    public class ConfigType : BaseType
    {
        public static readonly ConfigType DisplayName = new ConfigType("name");
        public static readonly ConfigType Hidden = new ConfigType("hidden");
        public static readonly ConfigType ClassName = new ConfigType("is");
        public static readonly ConfigType ValueType = new ConfigType("type");
        public static readonly ConfigType Result = new ConfigType("result");
        public static readonly ConfigType Writable = new ConfigType("writable");
        public static readonly ConfigType Invokable = new ConfigType("invokable");
        public static readonly ConfigType Parameters = new ConfigType("params");
        public static readonly ConfigType Columns = new ConfigType("columns");
        public static readonly ConfigType ActionGroup = new ConfigType("actionGroup");
        public static readonly ConfigType ActionGroupSubtitle = new ConfigType("actionGroupSubTitle");

        private ConfigType(string str)
            : base(str)
        {
        }
    }
}