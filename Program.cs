namespace SpaceFogOnlineServer;

using Riptide.Utils;

internal class Program
{
    static void Main(string[] args)
    {
        Console.Title = "Server";
        RiptideLogger.Initialize(Console.WriteLine, true);
        SpaceFogOnlineServer spaceFogServer = SpaceFogOnlineServer.SFServer;
        spaceFogServer.World.InitWorld();
        spaceFogServer.Start(7878);

        Console.WriteLine("Press enter to stop the server at any time.");
        Console.ReadLine();
        spaceFogServer.Stop();
        Console.ReadLine();
    }
}
