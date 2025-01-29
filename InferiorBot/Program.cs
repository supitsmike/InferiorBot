using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace InferiorBot
{
    public class DiscordBot
    {
        private static ServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var socketConfig = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildBans |
                                 GatewayIntents.GuildVoiceStates | GatewayIntents.GuildMessages |
                                 GatewayIntents.GuildMessageReactions | GatewayIntents.DirectMessages |
                                 GatewayIntents.DirectMessageReactions | GatewayIntents.MessageContent,
                LogLevel = LogSeverity.Debug,
            };

            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddInfrastructure(configuration)
                .AddMediatR(x => x.RegisterServicesFromAssembly(typeof(DiscordBot).Assembly))
                .AddSingleton(socketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>().Rest))
                .AddSingleton<DiscordEventListener>();

            return services.BuildServiceProvider();
        }

        public static async Task Main()
        {
            await MainAsync();
        }

        private static async Task MainAsync()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            await using var services = ConfigureServices();
            var listener = services.GetRequiredService<DiscordEventListener>();
            await listener.StartAsync();

            await Task.Delay(Timeout.Infinite);
            //try { await Task.Delay(Timeout.Infinite); } catch { /*ignored*/ }
        }
    }
}
