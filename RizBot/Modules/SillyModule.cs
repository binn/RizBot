using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RizBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RizBot.Modules
{
    public class SillyModule : InteractionModuleBase<SocketInteractionContext>
    {
        private RizContext _ctx;
        private readonly string[]? _admins;

        public SillyModule(IConfiguration configuration, RizContext ctx)
        {
            _ctx = ctx;
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

            var channels = await _ctx.Channels.ToListAsync();
            _ = Task.Run(async () =>
            {
                foreach (var channel in channels)
                {
                    var guildChannel = (ITextChannel)await Context.Client.GetChannelAsync(ulong.Parse(channel.Id));
                    await guildChannel.SendMessageAsync(":rocket: Honk honk!");
                }
            });

            await RespondAsync("Honk!");
        }
    }
}
