namespace XSpaceFogOnline.Networking.Entities
{
    using System;
    using System.Numerics;
    using Riptide;
    using SpaceFogOnlineServer.Helpers;

    [Serializable]
    public struct NetworkPlayerMovementInput : IMessageSerializable
    {
        public Chunk Chunk { get; set; }

        public ushort SessionId { get; set; }
        public bool IsRunning {  get; set; }
        public bool IsDescending {  get; set; }
        public bool IsAscending {  get; set; }
        public float HorizontalMovement {  get; set; }
        public float VerticalMovement {  get; set; }
        
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public bool IsRotating {  get; set; }
        public Quaternion SightRotation { get; set; }

        public void Serialize(Message message)
        {
            message.AddUShort(SessionId);
            message.AddBool(IsRunning);
            message.AddBool(IsDescending);
            message.AddBool(IsAscending);
            message.AddFloat(HorizontalMovement);
            message.AddFloat(VerticalMovement);
            message.AddVector3(Position);
            message.AddQuaternion(Rotation);
            message.AddQuaternion(SightRotation);
            message.AddBool(IsRotating);
            message.AddSerializable(Chunk);
        }

        public void Deserialize(Message message)
        {
            SessionId = message.GetUShort();
            IsRunning = message.GetBool();
            IsDescending = message.GetBool();
            IsAscending = message.GetBool();
            HorizontalMovement = message.GetFloat();
            VerticalMovement = message.GetFloat();
            Position = message.GetVector3();
            Rotation = message.GetQuaternion();
            SightRotation = message.GetQuaternion();
            IsRotating = message.GetBool();
            Chunk = message.GetSerializable<Chunk>();
        }

        public override string ToString()
        {
            return $"C: {Chunk.ToJson()}, P:{Position}, R: {Rotation} State:[{IsRunning},{IsDescending},{IsAscending},{HorizontalMovement},{VerticalMovement}, {IsRotating}]";
        }
    }
}