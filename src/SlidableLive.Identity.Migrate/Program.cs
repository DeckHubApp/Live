using System;
using System.Threading.Tasks;
using RendleLabs.EntityFrameworkCore.MigrateHelper;

namespace SlidableLive.Identity.Migrate
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Trying migration...");
            await new MigrationHelper().TryMigrate(args);
            Console.WriteLine("Done.");
        }
    }
}
