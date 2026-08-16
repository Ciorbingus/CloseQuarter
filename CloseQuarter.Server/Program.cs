using CloseQuarter.Server.Data;
using CloseQuarter.Server.Networking;

Console.WriteLine("CloseQuarter Game Server Starting...");

using (var db = new AppDbContext())
{
    db.Database.EnsureCreated();
    Console.WriteLine("[DB] Database ready!");
}

var server = new NetworkServer(port: 5000);
server.Start();