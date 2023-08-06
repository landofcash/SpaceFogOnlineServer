namespace SpaceFogOnlineServer;

using System.Numerics;
using Newtonsoft.Json;
using NLog;
using XSpaceFogOnline.Networking.Entities;

public class World
{
    public static Logger LOGGER = LogManager.GetCurrentClassLogger();
    public Dictionary<Chunk,List<NetworkChunkedEntity>> WorldChunks { get; private set; }
    public Dictionary<ushort, NetworkOtherPlayer> Players { get; private set; }

    public void InitWorld()
    {
        LOGGER.Debug($"InitWorld starting");
        Dictionary<Chunk, List<NetworkChunkedEntity>> world = new Dictionary<Chunk, List<NetworkChunkedEntity>>();
        var definition = new[] {new[] {new WorldRecordJson(){}}};
        string worldJson = File.ReadAllText("World/world.json");
        var res = JsonConvert.DeserializeAnonymousType(worldJson, definition);
        foreach (var chankJson in res)
        {
            foreach (var item in chankJson)
            {
                string name = item.N;
                var chunk = new Chunk(item.C[0], item.C[1], item.C[2]);
                var position = new Vector3(item.P[0], item.P[1], item.P[2]);
                var rotation = new Quaternion(item.R[0], item.R[1], item.R[2], item.R[2]);
                if (!world.ContainsKey(chunk))
                {
                    world.Add(chunk, new List<NetworkChunkedEntity>());
                }

                var entity = new NetworkChunkedEntity() { Chunk = chunk, Name = name, Position = position, Rotation = rotation };
                LOGGER.Debug($"{entity}");
                world[chunk].Add(entity);
            }
        }

        WorldChunks = world;
        Players = new Dictionary<ushort, NetworkOtherPlayer>();
    }
}