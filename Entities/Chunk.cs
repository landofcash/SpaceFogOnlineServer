namespace XSpaceFogOnline.Networking.Entities
{
    using System;
    using System.Numerics;
    using Riptide;

    [Serializable]
    public class Chunk : IMessageSerializable
    {
        public Chunk()
        {
        }

        public const float SIZE = 1000.00f;
        public int gx;
        public int gy; //todo make private
        public int gz;

        public bool testEnvironment = false;

        public Chunk(int gx = default, int gy = default, int gz = default)
        {
            this.gx = gx;
            this.gy = gy;
            this.gz = gz;
        }

        public bool IsNeighbour(Chunk other)
        {
            return (gx == other.gx + 1 || gx == other.gx - 1 || gx==other.gx) 
                   && (gy == other.gy + 1 || gy == other.gy - 1 || gy==other.gy) 
                   && (gz == other.gz + 1 || gz == other.gz - 1 || gz == other.gz);
        }

        public static Vector3 ConvertToHomeChunk(Chunk homeChunk, Chunk remoteChunk, Vector3 remotePosition)
        {
            if (homeChunk == remoteChunk)
            {
                return remotePosition;
            }

            if (!homeChunk.IsNeighbour(remoteChunk))
            {
                //Debug.Log($"Trying to get location of not neighbor chunk. homeChunk: {homeChunk}, remoteChunk:{remoteChunk}"); //TODO probably error here
            }

            float dx = (remoteChunk.gx - homeChunk.gx) * SIZE;
            float dy = (remoteChunk.gy - homeChunk.gy) * SIZE;
            float dz = (remoteChunk.gz - homeChunk.gz) * SIZE;
            return remotePosition + new Vector3(dx, dy, dz);
        }

        public static bool IsInRange(float checkValue, float left, float right)
        {
            float eps = 0.1f;
            return checkValue > left - eps && checkValue < right + eps;
        }
        public bool IsInChunk(Vector3 position)
        {
            if (testEnvironment) return true;
            bool inChunk = true;
            if (!IsInRange(position[0],0,SIZE))
            {
                Log($"Not in chunk x: {position[0]}");
                inChunk = false;
            }

            if (!IsInRange(position[1],0,SIZE))
            {
                Log($"Not in chunk y: {position[1]}");
                inChunk = false;
            }

            if (!IsInRange(position[2],0,SIZE))
            {
                Log($"Not in chunk z: {position[2]}");
                inChunk = false;
            }

            return inChunk;
        }

        public Tuple<Chunk,Vector3> NewChunkFromPosition(Vector3 position)
        {
            var newPosition = position;
            var newChunk = new Chunk(gx,gy,gz);
            if (position[0]<0)
            {
                newPosition[0] += SIZE;
                newChunk.gx--;
            }
            if (position[0]>SIZE)
            {
                newPosition[0] -= SIZE;
                newChunk.gx++;
            }
            if (position[1]<0)
            {
                newPosition[1] += SIZE;
                newChunk.gy--;
            }
            if (position[1]>SIZE)
            {
                newPosition[1] -= SIZE;
                newChunk.gy++;
            }
            if (position[2]<0)
            {
                newPosition[2] += SIZE;
                newChunk.gz--;
            }
            if (position[2]>SIZE)
            {
                newPosition[2] -= SIZE;
                newChunk.gz++;
            }
            return new Tuple<Chunk, Vector3>(newChunk, newPosition);
        }

        private void Log(string message)
        {
           //Console.WriteLine(message);
        }

        #region Equalty

        protected bool Equals(Chunk other)
        {
            return gx == other.gx && gy == other.gy && gz == other.gz;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Chunk)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(gx, gy, gz);
        }

        public static bool operator ==(Chunk left, Chunk right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Chunk left, Chunk right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region IMessageSerializable

        public void Serialize(Message message)
        {
            message.AddInts(new[] { gx, gy, gz });
        }

        public void Deserialize(Message message)
        {
            var values = message.GetInts();
            gx = values[0];
            gy = values[1];
            gz = values[2];
        }

        #endregion

        #region FORMATTING

        public override string ToString()
        {
            return $"Gx: {gx}, Gy: {gy}, Gz: {gz}";
        }

        public string ToJson()
        {
            return $"[{gx},{gy},{gz}]";
        }

        #endregion
    }
}