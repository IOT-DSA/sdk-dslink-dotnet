using System;
using DSLink.Container;

namespace DSLink.Connection
{
    /// <summary>
    /// Manages the selected Connector.
    /// <see cref="Connector"/>
    /// </summary>
    public static class ConnectorManager
    {
        /// <summary>
        /// Connector Type.
        /// </summary>
        public static Type ConnectorType
        {
            private set;
            get;
        }

        /// <summary>
        /// Create an instance of the selected Connetor.
        /// </summary>
        /// <param name="link">DSLink container instance</param>
        public static Connector Create(AbstractContainer link)
        {
            return (Connector) Activator.CreateInstance(ConnectorType, link);
        }

        /// <summary>
        /// Set the Connector type.
        /// </summary>
        /// <param name="type">Connector Type</param>
        public static void SetConnector(Type type)
        {
            if (ConnectorType == null)
            {
                ConnectorType = type;
            }
        }
    }
}
