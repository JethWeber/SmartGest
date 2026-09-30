using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Migrations;

public sealed class SmartGestDbContextFactory : IDesignTimeDbContextFactory<SmartGestDbContext>
{
    public SmartGestDbContext CreateDbContext(string[] args)
    {
        var path = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal))
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SmartGest",
                "smartgest.db");

        var options = new DbContextOptionsBuilder<SmartGestDbContext>()
            .UseSqlite($"Data Source={Path.GetFullPath(path)}")
            .Options;

        return new SmartGestDbContext(options);
    }
}