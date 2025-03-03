using Infrastructure.InferiorBot;
using Microsoft.EntityFrameworkCore;

namespace InferiorBot.Extensions
{
    public static class InferiorBotContextExtension
    {
        public static async Task<List<GameUser>> GetGameUsers(this InferiorBotContext context, Game game)
        {
            return await context.GameUsers.Where(x => x.GameId == game.GameId)
                .Include(gameUser => gameUser.User).ThenInclude(user => user.UserStat).ToListAsync();
        }
    }
}
