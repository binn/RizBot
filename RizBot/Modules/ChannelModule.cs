using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RizBot.Data;
using System.ComponentModel.DataAnnotations;

namespace RizBot.Modules
{
    public class ChannelModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RizContext _ctx;
        private readonly ILogger<ChannelModule> _logger;

        public ChannelModule(ILogger<ChannelModule> logger, RizContext ctx)
        {
            _ctx = ctx;
            _logger = logger;
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [SlashCommand("setchannel", "Adds a channel for new plays to be announced in.")]
        public async Task SetChannelAsync([Required] IGuildChannel channel)
        {
            bool channelExists = await _ctx.Channels.AnyAsync(x => x.Id == channel.Id.ToString());
            if (channelExists)
            {
                await FollowupAsync("This channel is already registered to receive new play announcements.");
                return;
            }

            var channelRecord = new Channel()
            {
                Id = channel.Id.ToString()
            };

            await _ctx.Channels.AddAsync(channelRecord);
            await _ctx.SaveChangesAsync();

            _logger.LogInformation("Channel #{channel} was added by {user}", channel.Name, Context.User.ToString());
            await FollowupAsync("Channel added.");
        }

        [RequireUserPermission(GuildPermission.ManageChannels)]
        [SlashCommand("removechannel", "Removes a channel from receiving announcements.")]
        public async Task RemoveChannel([Required] IGuildChannel target)
        {
            var channel = await _ctx.Channels.FirstOrDefaultAsync(x => x.Id == target.Id.ToString());
            if (channel == null)
            {
                await FollowupAsync("This channel isn't registered to receive new play announcements.");
                return;
            }

            _ctx.Channels.Remove(channel);
            await _ctx.SaveChangesAsync();

            _logger.LogInformation("Channel #{channel} was removed by {user}", target.Name, Context.User.ToString());
            await FollowupAsync("This channel has been removed from recieving new play announcements.");
        }

        [SlashCommand("add", "Sends a link to add this bot to your server.")]
        public async Task GetBotLinkAsync()
        {
            await FollowupAsync($"Add this bot to your server by clicking this link:\nhttps://discord.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=8&scope=bot%20applications.commands");
        }
    }
}
