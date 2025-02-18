using Discord;
using Discord.Interactions;
using InferiorBot.Classes;
using InferiorBot.Extensions;
using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;
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
            if (UserData.DailyCooldown.HasValue && UserData.DailyCooldown > DateTime.Now)
            {
                await RespondAsync(embed: new EmbedBuilder
                {
                    Title = "Daily bonus not ready",
                    Description = $"""
                                   :hourglass_flowing_sand: You have already collected your daily bonus.
                                   Try again {Format.Bold(DiscordFormatter.Timestamp(UserData.DailyCooldown.Value))}
                                   """,
                    Color = Color.DarkRed
                }.Build(), ephemeral: true);
                return;
            }

            UserData.DailyCount++;
            UserData.DailyStreak =
                UserData.DailyCooldown.HasValue && UserData.DailyCooldown.Value.Date == DateTime.Now.Date
                    ? UserData.DailyStreak + 1
                    : 1;
            UserData.DailyCooldown = DateTime.Now.AddDays(1);

            var dailyBonus = Configuration.GetValue<decimal>("Settings:DailyBonus");
            UserData.Balance += dailyBonus;

            if (!_context.ChangeTracker.HasChanges())
            {
                await RespondAsync("Failed to update user.", ephemeral: true);
                return;
            }
            await _context.SaveChangesAsync();

            await RespondAsync(embed: new EmbedBuilder
            {
                Title = "Daily Bonus",
                Description = $"Daily bonus of {Format.Bold($"{dailyBonus:C}")} collected!",
                Color = Color.DarkGreen,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Streak: Day {UserData.DailyStreak}"
                }
            }.Build(), ephemeral: true);
        }

        [SlashCommand("select-job", "Claim your daily bonus.")]
        public async Task SelectJob()
        {
            var jobList = await _context.Jobs
                .Select(job => new SelectMenuOptionBuilder(job.JobTitle, $"{job.JobId}",
                    $"Pay rate: {job.PayMin:$#,##0;-$#,##0} to {job.PayMax:$#,##0;-$#,##0} | Success rate {job.Probability:P0}",
                    null, null))
                .ToListAsync();

            var components = new ComponentBuilder()
                .WithSelectMenu("select-job-menu", jobList, "Select Job...")
                .Build();
            
            await RespondAsync("Please select a job: ", components: components, ephemeral: true);
        }

        [ComponentInteraction("select-job-menu")]
        public async Task SelectJobMenu()
        {
            if (Context.Interaction is not IComponentInteraction componentInteraction) return;
            
            var jobId = Convert.ToInt32(componentInteraction.Data.Values.First());
            var selectedJob = await _context.Jobs.Where(job => job.JobId == jobId).FirstOrDefaultAsync();
            if (selectedJob == null) return;

            UserData.JobId = selectedJob.JobId;

            if (!_context.ChangeTracker.HasChanges())
            {
                await RespondAsync("Failed to update user.", ephemeral: true);
                return;
            }
            await _context.SaveChangesAsync();

            await RespondAsync($"Your job is now a {Format.Bold(selectedJob.JobTitle)}!", ephemeral: true);
        }


        [SlashCommand("work", "Work to earn money.")]
        public async Task DoWork()
        {
            var selectedJob = await _context.Jobs.Where(job => job.JobId == UserData.JobId).FirstOrDefaultAsync();
            if (selectedJob == null)
            {
                var selectJob = Commands.FirstOrDefault(x => x.Name == "select-job");
                await RespondAsync(
                    $"You must select a job first. Do {(selectJob != null ? DiscordFormatter.Mention(selectJob) : "/select-job")} first.",
                    ephemeral: true);
                return;
            }

            if (UserData.WorkCooldown.HasValue && UserData.WorkCooldown.Value > DateTime.Now)
            {
                await RespondAsync(embed: new EmbedBuilder
                {
                    Title = "Cannot work again yet",
                    Description =
                        $":hourglass_flowing_sand: You can work again {Format.Bold(DiscordFormatter.Timestamp(UserData.WorkCooldown.Value))}",
                    Color = Color.DarkRed
                }.Build(), ephemeral: true);
                return;
            }

            var (amount, success) = selectedJob.GetPayAmount();
            UserData.Balance += amount;

            UserData.WorkCooldown = DateTime.Now.AddMinutes(selectedJob.Cooldown);
            UserData.WorkCount++;

            if (!_context.ChangeTracker.HasChanges())
            {
                await RespondAsync("Failed to update user.", ephemeral: true);
                return;
            }
            await _context.SaveChangesAsync();

            await RespondAsync(embed: new EmbedBuilder
            {
                Title = "Work Reward",
                Description = $"""
                               {(success == false ? "You failed the job." : null)}

                               You {(success == false ? "lost" : "earned")} {Format.Bold($"{amount:$#,##0.00;$#,##0.00}")} while working.
                               """,
                Color = Color.DarkGreen,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"New balance is {UserData.Balance:C}"
                }
            }.Build(), ephemeral: true);
        }
    }
}
