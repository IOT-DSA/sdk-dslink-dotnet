using System;
using DSLink.Connection.Serializer;
using DSLink.Container;

namespace DSLink.Connection
{
    public static class ConnectorManager
    {
        public static Type ConnectorType
        {
            private set;
            get;
        }

        public static Connector Create(AbstractContainer link, Configuration config)
        {
            return (Connector) Activator.CreateInstance(ConnectorType, link, config);
        }

        public static void SetConnector(Type type)
        {
            if (ConnectorType == null)
            {
                ConnectorType = type;
            }
        }
    }
}
