using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RizBot.Data;
using System.Reflection;

namespace RizBot
{
    public class Program
    {
        private DiscordSocketClient? _client;
        private InteractionService? _interactions;
        private IConfiguration? _configuration;
        private IServiceProvider? _services;
        private ILogger? _logger;

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables("RIZ_")
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .Build();

            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds,
            };

            var services = new ServiceCollection();
            _client = new DiscordSocketClient(config);
            _interactions = new InteractionService(_client);

            services.AddSingleton(_configuration);
            services.AddLogging(c => c.AddConfiguration(_configuration).AddConsole());
            services.AddSingleton(_client);
            services.AddSingleton(_interactions);
            services.AddDbContext<RizContext>(o => o.UseNpgsql(_configuration.GetConnectionString("Main")));
            
            _services = services.BuildServiceProvider();
            
            var scope = _services.CreateScope();
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            _client.Log += Log;
            _client.Ready += Ready;
            _client.JoinedGuild += JoinedGuild;

            await _client.LoginAsync(TokenType.Bot, _configuration["Token"]);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task JoinedGuild(SocketGuild guild) =>
            await _interactions!.RegisterCommandsToGuildAsync(guild.Id, true);

        private async Task Ready()
        {
            await _interactions!.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
            _client!.InteractionCreated += async interaction =>
            {
                var scope = _services!.CreateScope();
                var ctx = new SocketInteractionContext(_client, interaction);
                await _interactions.ExecuteCommandAsync(ctx, scope.ServiceProvider);
            };

            foreach(var guild in _client.Guilds)
                await _interactions.RegisterCommandsToGuildAsync(guild.Id, true);
        }

        private Task Log(LogMessage msg)
        {
            _logger!.Log(LogLevel.Information, msg.Exception, msg.Message);
            return Task.CompletedTask;
        }
    }
}