using Discord;
using Discord.Interactions;
using InferiorBot.Classes;
using Infrastructure.InferiorBot;
using Microsoft.Extensions.Configuration;

namespace InferiorBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class EconomyModule(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
    {
        private readonly InferiorBotContext _context = context;

        [SlashCommand("daily", "Claim your daily bonus.")]
        public async Task DailyBonus()
        {
            if (UserData.DailyCooldown != null && UserData.DailyCooldown > DateTime.Now)
            {
                await RespondAsync(embed: new EmbedBuilder
                {
                    Title = "Daily bonus not ready",
                    Description = $":hourglass_flowing_sand: You have already collected your daily bonus.{Environment.NewLine}Try again {Format.Bold(DiscordFormatter.Timestamp(UserData.DailyCooldown.Value))}",
                    Color = Color.DarkRed
                }.Build(), ephemeral: true);
                return;
            }

            var dailyBonus = Configuration.GetValue<decimal>("Settings:DailyBonus");
            UserData.Balance += dailyBonus;
            UserData.DailyCooldown = DateTime.Now.AddDays(1);
            UserData.DailyStreak++;

            if (!_context.ChangeTracker.HasChanges())
            {
                await RespondAsync("Failed to update user.", ephemeral: true);
                return;
            }
            await _context.SaveChangesAsync();

            await RespondAsync(embed: new EmbedBuilder
            {
                Title = "Daily Bonus",
                Description = $"Daily bonus of {Format.Bold($"{dailyBonus:C}")} collected!{Environment.NewLine}{Environment.NewLine}{LevelingSystem.CanPrestige(UserData.Xp)} {LevelingSystem.GetLevelFromXp(UserData.Xp)}",
                Color = Color.DarkGreen,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Streak: Day {UserData.DailyStreak}"
                }
            }.Build(), ephemeral: true);
        }
    }
}
