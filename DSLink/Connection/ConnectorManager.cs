using System;
using DSLink.Connection.Serializer;
using DSLink.Container;

namespace DSLink.Connection
{
    public static class ConnectorManager
    {
        private static Type _connectorType;

        public static Connector Create(AbstractContainer link, Configuration config, ISerializer serializer)
        {
            return (Connector) Activator.CreateInstance(_connectorType, link, config, serializer);
        }

        public static void SetConnector(Type type)
        {
            if (_connectorType == null)
            {
                _connectorType = type;
            }
        }
    }
}
