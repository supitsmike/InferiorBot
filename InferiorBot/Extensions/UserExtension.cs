using Infrastructure.InferiorBot;

namespace InferiorBot.Extensions
{
    public static class UserExtension
    {
        public static void WonMoney(this User user, decimal amount, bool addToBalance = true)
        {
            if (addToBalance) user.Balance += amount;

            user.UserStat.AllTimeWon += amount;
            user.UserStat.BiggestWin = user.UserStat.BiggestWin > amount ? user.UserStat.BiggestWin : amount;
        }

        public static void LostMoney(this User user, decimal amount, bool removeFromBalance = true)
        {
            if (removeFromBalance) user.Balance -= amount;

            user.UserStat.AllTimeLost += amount;
            user.UserStat.BiggestLoss = user.UserStat.BiggestLoss > amount ? user.UserStat.BiggestLoss : amount;
        }
    }
}
