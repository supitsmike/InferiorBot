using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using InferiorBot.Attributes;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;

namespace InferiorBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class UserModule(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
    {
        private readonly InferiorBotContext _context = context;
        private readonly IServiceProvider _services = services;

        [Defer(false)]
        [SlashCommand("stats", "View your or another user's game stats.")]
        public async Task UserStats([Summary(description: "The user's stats you want to view.")] SocketUser? user = null)
        {
            user ??= Context.User;
            if (user != Context.User) UserData = await user.GetUserDataAsync(_context, _services);
            
            var fields = new List<EmbedFieldBuilder>();
            // All-Time Stats
            {
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "All-Time Won",
                    Value = $"{UserData.UserStat.AllTimeWon:C}"
                });
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "\u200b",
                    Value = "\u200b"
                });
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "All-Time Lost",
                    Value = $"{UserData.UserStat.AllTimeLost:C}"
                });
            }
            // Biggest Stats
            {
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Biggest Win",
                    Value = $"{UserData.UserStat.BiggestWin:C}"
                });
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "\u200b",
                    Value = "\u200b"
                });
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Biggest Loss",
                    Value = $"{UserData.UserStat.BiggestLoss:C}"
                });
            }
            // Misc Stats
            {
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Work Completed",
                    Value = UserData.UserStat.WorkCount
                });
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "\u200b",
                    Value = "\u200b"
                });
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Daily Rewards Claimed",
                    Value = UserData.UserStat.DailyCount
                });
            }
            // Coin Flip Stats
            {
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Coinflip Wins",
                    Value = UserData.UserStat.CoinFlipWins
                });
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Coinflip Losses",
                    Value = UserData.UserStat.CoinFlipLosses
                });
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Coinflip Win Percentage",
                    Value = $"{Methods.GetWinPercentage(UserData.UserStat.CoinFlipWins, UserData.UserStat.CoinFlipLosses):0.00}%"
                });
            }
            // Guess the Number Stats
            {
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Guess Wins",
                    Value = UserData.UserStat.GuessWins
                });
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Guess Losses",
                    Value = UserData.UserStat.GuessLosses
                });
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = "Guess Win Percentage",
                    Value = $"{Methods.GetWinPercentage(UserData.UserStat.GuessWins, UserData.UserStat.GuessLosses):0.00}%"
                });
            }

            await FollowupAsync(embed: new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = user.GetUserName(),
                    IconUrl = user.GetDisplayAvatarUrl() ?? user.GetAvatarUrl()
                },
                Title = $"User Stats{(UserData.Banned ? " | BANNED" : string.Empty)}",
                Description = $"Balance: {Format.Bold($"{UserData.Balance:C}")}",
                Color = Color.Gold,
                Fields = fields
            }.Build());
        }

        [Defer(false)]
        [SlashCommand("leaderboard", "See who's at the top of the leaderboard.")]
        public async Task LeaderboardCommand()
        {
            var userNumber = 0;
            var description = ":x: There are no users";

            var users = await _context.Users.OrderByDescending(user => user.Balance).ToListAsync();
            if (users.Count > 0)
            {
                description = string.Empty;
                foreach (var user in users)
                {
                    var userId = Convert.ToUInt64(user.UserId);
                    var guildUser = Context.Guild.Users.FirstOrDefault(guildUser => guildUser.Id == userId);
                    if (guildUser == null) continue;

                    description += $"{++userNumber}. {guildUser.Username} - {Format.Bold($"{user.Balance:C}")}{Environment.NewLine}";
                    if (userNumber == 10) break;
                }
            }

            await FollowupAsync(embed: new EmbedBuilder
            {
                Title = $"Top Users in {Context.Guild.Name}",
                Description = description,
                Color = Color.Gold
            }.Build());
        }
    }
}
