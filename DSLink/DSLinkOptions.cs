namespace DSLink
{
    /// <summary>
    /// This class represents the full set of DSLink command line options.
    /// </summary>
    public class DSLinkOptions
    {
        public DSLinkOptions(string LinkName)
        {
        }

        public string BrokerUrl { get; set; }
        public string BrokerToken { get; set; }
        public string LinkName { get; set; }
        public string Key { get; set; }
        public string DSLinkJsonPath { get; set; }
        public string NodesFilename { get; set; }
    }
}