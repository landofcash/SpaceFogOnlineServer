namespace XSpaceFogOnline.Networking.Entities
{
    using Riptide;

    public class NetworkOtherPlayer : NetworkChunkedEntity
    {
        public ushort SessionId { get; set; }

        public override void Serialize(Message message)
        {
            base.Serialize(message);
            message.AddUShort(SessionId);
        }

        public override void Deserialize(Message message)
        {
            base.Deserialize(message);
            SessionId =  message.GetUShort();
        }

        public override int SizeInBytes()
        {
            return Name.Length * sizeof(char) + 3 * sizeof(int) + 3 * sizeof(float) + 4 * sizeof(float) + sizeof(ushort);
        }

    }
}