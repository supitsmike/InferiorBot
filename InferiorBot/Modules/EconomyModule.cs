using Discord;
using Discord.Interactions;
using InferiorBot.Classes;
using Infrastructure.InferiorBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InferiorBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class EconomyModule(InferiorBotContext context, IServiceProvider services) : BaseUserModule(context, services)
    {
        private readonly InferiorBotContext _context = context;
        private readonly IServiceProvider _services = services;

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

            var configuration = _services.GetRequiredService<IConfiguration>();
            var dailyBonus = configuration.GetValue<decimal>("Settings:DailyBonus");
            UserData.Balance += dailyBonus;
            UserData.DailyCooldown = DateTime.Now.AddDays(1);

            if (!_context.ChangeTracker.HasChanges())
            {
                await RespondAsync("Failed get update user.", ephemeral: true);
                return;
            }
            await _context.SaveChangesAsync();

            await RespondAsync(embed: new EmbedBuilder
            {
                Title = "Daily bonus",
                Description = $"You have collected your daily bonus of {Format.Bold($"${dailyBonus}")}.",
                Color = Color.DarkGreen,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"New balance is {UserData.Balance:C}"
                }
            }.Build(), ephemeral: true);
        }
    }
}
