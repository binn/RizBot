using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RizBot.Data;

namespace RizBot.Modules
{
    public class AnnounceModule : InteractionModuleBase<SocketInteractionContext>
    {
        private RizContext _ctx;
        private readonly string[]? _admins;
        private readonly ILogger<AnnounceModule> _logger;

        public AnnounceModule(IConfiguration configuration, ILogger<AnnounceModule> logger, RizContext ctx)
        {
            _ctx = ctx;
            _logger = logger;
            _admins = configuration["Admins"]?.Split(',')?.Select(x => x.Trim())?.ToArray();
        }


        [SlashCommand("oo", "Announces a new play to all registered channels.")]
        public async Task AnnounceNewPlayAsync(string ticker, string strike, string price, string stoploss, string? comment = null)
        {
            if (_admins?.Contains(Context.User.Id.ToString()) != true)
            {
                await RespondAsync("You do not have permission to run this command.");
                return;
            }

            ticker = ticker.ToUpper();
            strike = strike.ToUpper();

            var embed = new EmbedBuilder()
                .WithTitle("New Trade")
                .WithDescription($":moneybag: **Ticker**: {ticker}\n:notepad_spiral: **Contract**: {strike}\n:green_circle: **Entry**: {price}\n:stop_sign: **Stop Loss**: {stoploss}\n\n"
                + "***This is not financial advice, trade at your own risk and make sure to do your own research.***"
                + (string.IsNullOrWhiteSpace(comment) ? "" : ":speech_balloon: **Comments**: " + comment))
                .WithCurrentTimestamp()
                .WithFooter("@RizVTrades", "https://bin.moe/s/9608b4d737859664b11c53ec0d95f9f2.jpg")
                .WithColor(Color.Green)
                .Build();

            _ = Task.Run(async () =>
            {
                var channels = await _ctx.Channels.ToListAsync();
                foreach (var channel in channels)
                {
                    var guildChannel = (ITextChannel)await Context.Client.GetChannelAsync(ulong.Parse(channel.Id));
                    await guildChannel.SendMessageAsync(embed: embed);
                }
            });

            await RespondAsync("Created entry play.");
        }


        [SlashCommand("oc", "Announces a new close play to all registered channels.")]
        public async Task AnnounceClosePlayAsync(string ticker, string strike, string price, string? comment = null)
        {
            if (_admins?.Contains(Context.User.Id.ToString()) != true)
            {
                await RespondAsync("You do not have permission to run this command.");
                return;
            }

            ticker = ticker.ToUpper();
            strike = strike.ToUpper();

            var embed = new EmbedBuilder()
                .WithTitle("Close Trade")
                .WithDescription($":moneybag: **Ticker**: {ticker}\n:notepad_spiral: **Contract**: {strike}\n:red_circle: **Exit**: {price}\n\n"
                + "***This is not financial advice, trade at your own risk and make sure to do your own research.***"
                + (string.IsNullOrWhiteSpace(comment) ? "" : ":speech_balloon: **Comments**: " + comment))
                .WithCurrentTimestamp()
                .WithFooter("@RizVTrades", "https://bin.moe/s/9608b4d737859664b11c53ec0d95f9f2.jpg")
                .WithColor(Color.Red)
                .Build();

            _ = Task.Run(async () =>
            {
                var channels = await _ctx.Channels.ToListAsync();
                foreach (var channel in channels)
                {
                    var guildChannel = (ITextChannel)await Context.Client.GetChannelAsync(ulong.Parse(channel.Id));
                    await guildChannel.SendMessageAsync(embed: embed);
                }
            });

            await RespondAsync("Created exit play.");
        }
    }
}
