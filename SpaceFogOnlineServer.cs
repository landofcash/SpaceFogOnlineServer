namespace SpaceFogOnlineServer;

using NLog;
using Riptide;

public class WorldRecordJson
{
    public string N { get; set; }
    public List<int> C { get; set; }
    public List<float> P { get; set; }
    public List<float> R { get; set; }
}

public class SpaceFogOnlineServer
{
    public static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public static SpaceFogOnlineServer SFServer { get; } = new();
    private const ushort _maxClientCount = 10000;

    private ushort _port;
    private Server _server;
    public  Server Server => _server;
    private Thread? _loopThread;
    
    public bool IsRunning { get; private set; }
    public World World {get; private set; }

    private SpaceFogOnlineServer()
    {
        World = new World();
        World.InitWorld();
        IsRunning = false;
        _loopThread = null;
    }

    public void Start(ushort port)
    {
        _port = port;
        _loopThread = new Thread(Loop);
        _loopThread.Start();
    }

    public void Stop()
    {
        IsRunning = false;
    }

    private void Loop()
    {
        IsRunning = true;
        LOGGER.Info($"Starting server port:{_port} _maxClientCount:{_maxClientCount}");
        _server = new Server();

        _server.ClientConnected += Server_OnClientConnected;
        _server.ClientDisconnected += Server_OnClientDisconnected;

        _server.Start(_port, _maxClientCount);
        LOGGER.Info($"Started server. {DateTime.Now:G}");
        while (IsRunning)
        {
            _server.Update();
            Thread.Sleep(10);
        }
        LOGGER.Info($"Stopped server. {DateTime.Now:G}");
        _server.Stop();
        _loopThread = null;
    }

    private void Server_OnClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
    {
        LOGGER.Debug($"Client Disconnected id:{e.Client.Id} reason:{nameof(e.Reason)}");
        World.Players.Remove(e.Client.Id);
        Message syncDeleteOtherPlayersMessage = Message.Create(MessageSendMode.Reliable, ServerToClientId.SyncDeleteOtherPlayers);
        syncDeleteOtherPlayersMessage.AddUShorts(World.Players.Keys.ToArray());
        _server.SendToAll(syncDeleteOtherPlayersMessage);
    }

    private void Server_OnClientConnected(object? sender, ServerConnectedEventArgs e)
    {
    }
}