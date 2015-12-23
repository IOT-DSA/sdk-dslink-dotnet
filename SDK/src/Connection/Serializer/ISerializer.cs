using System.Collections.Generic;

namespace DSLink.Connection.Serializer
{
    public interface ISerializer
    {
        dynamic Serialize(RootObject data);
        RootObject Deserialize(dynamic data);
    }
}
