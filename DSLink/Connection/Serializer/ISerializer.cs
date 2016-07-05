namespace DSLink.Connection.Serializer
{
    public interface ISerializer
    {
        dynamic Serialize(RootObject data);
        RootObject Deserialize(dynamic data);
    }
}
