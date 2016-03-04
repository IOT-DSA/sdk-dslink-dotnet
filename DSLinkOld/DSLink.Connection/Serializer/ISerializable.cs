using Newtonsoft.Json.Linq;

namespace DSLink.Connection.Serializer
{
    public interface ISerializable
    {
        JContainer Serialize();
    }
}
