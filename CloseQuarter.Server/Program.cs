using CloseQuarter.Server.Data;
using CloseQuarter.Server.Services;

Console.WriteLine("CloseQuarter Game Server Starting...");

using (var db = new AppDbContext())
{
    db.Database.EnsureCreated();
    Console.WriteLine("[DB] Database ready!");
}

Console.WriteLine("Running Server...");