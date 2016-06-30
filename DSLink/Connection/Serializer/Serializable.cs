using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DSLink.Connection.Serializer
{
    public abstract class Serializable
    {
        public abstract Dictionary<dynamic, dynamic> Serialize();
        public abstract void Deserialize(Dictionary<dynamic, dynamic> data);
    }
}
