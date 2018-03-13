using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SlidableLive.Identity.Migrate
{
    public class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public const string LocalPostgres = "Host=localhost;Database=aspnet;Username=slidable;Password=secretsquirrel";
        public static readonly string AssemblyName = typeof(DesignTimeApplicationDbContextFactory).Assembly.GetName().Name;

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(LocalPostgres, b => b.MigrationsAssembly(AssemblyName));
            return new ApplicationDbContext(builder.Options);
        }
    }
}