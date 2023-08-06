namespace XSpaceFogOnline.Networking.Entities
{
    using System.Numerics;
    using Riptide;
    using SpaceFogOnlineServer.Helpers;

    public class NetworkChunkedEntity : IMessageSerializable
    {
        public NetworkChunkedEntity()
        {
        }

        public NetworkChunkedEntity(Vector3 position, Quaternion rotation, Chunk chunk, string prefabName)
        {
            Chunk = chunk;
            Position = position;
            Rotation = rotation;
            Name = prefabName;
        }

        public Chunk Chunk { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public string Name { get; set; } // TODO replace with something numeric

        public virtual void Serialize(Message message)
        {
            message.AddSerializable(Chunk);
            message.AddVector3(Position);
            message.AddQuaternion(Rotation);
            message.AddString(Name);
        }

        public virtual void Deserialize(Message message)
        {
            Chunk = message.GetSerializable<Chunk>();
            Position = message.GetVector3();
            Rotation = message.GetQuaternion();
            Name = message.GetString();
        }

        public virtual int SizeInBytes()
        {
            return Name.Length * sizeof(char) + 3 * sizeof(int) + 3 * sizeof(float) + 4 * sizeof(float);
        }

        public override string ToString()
        {
            return $"Chunk: {Chunk}, Position: {Position}, Rotation: {Rotation}, Name: {Name}";
        }

        public string ToJson()
        {
            return ("{" +
                   $"'N':'{Name.Replace("'", "").Replace("\"", "")}'," +
                   $"'C': {Chunk.ToJson()}, " +
                   $"'P':  [{string.Join(',',Position)}], " +
                   $"'R':  [{string.Join(',',Rotation)}]" +
                   "}").Replace('\'', '"');
        }
    }
}