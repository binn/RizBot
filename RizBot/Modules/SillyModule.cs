using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RizBot.Data;

namespace RizBot.Modules
{
    public class SillyModule : InteractionModuleBase<SocketInteractionContext>
    {
        private RizContext _ctx;
        private readonly IServiceProvider _services;
        private readonly string[]? _admins;

        public SillyModule(IConfiguration configuration, RizContext ctx, IServiceProvider services)
        {
            _ctx = ctx;
            _services = services;
            _admins = configuration["Admins"]?.Split(',')?.Select(x => x.Trim())?.ToArray();
        }

        [SlashCommand("honk", "Honk.")]
        public async Task AnnounceHonkAsync()
        {
            if (_admins?.Contains(Context.User.Id.ToString()) != true)
            {
                await RespondAsync("You do not have permission to run this command.");
                return;
            }

            await RespondAsync("Honk!");
            _ = Task.Run(async () =>
            {
                var scope = _services.CreateScope();
                var logger = scope.ServiceProvider.GetService<ILogger<SillyModule>>();
                using var ctx = scope.ServiceProvider.GetService<RizContext>();
                var channels = await ctx!.Channels.ToListAsync();
                foreach (var channel in channels)
                {
                    try
                    {
                        var guildChannel = await Context.Client.GetChannelAsync(ulong.Parse(channel.Id));
                        if (guildChannel == null)
                            continue;

                        var textChannel = (ITextChannel)guildChannel;
                        await textChannel.SendMessageAsync(":rocket: Honk honk!");
                    }
                    catch (Exception exception)
                    {
                        logger!.LogError(exception, "An error occured while trying to send a message to {channel}.", channel.Id);
                    }
                }
            });
        }
    }
}
