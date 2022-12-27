using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace RizBot.Data
{
    public class RizContextDesignTimeFactory : IDesignTimeDbContextFactory<RizContext>
    {
        public RizContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables("RIZ")
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .Build();

            var builder = new DbContextOptionsBuilder<RizContext>();
            builder.UseNpgsql(configuration.GetConnectionString("Main"));

            return new RizContext(builder.Options);
        }
    }
}
