using System.Globalization;
using Infrastructure.InferiorBot;

namespace InferiorBot.Extensions
{
    public static class UserExtension
    {
        public static void GiveMoney(this User user, decimal amount)
        {
            user.Balance += amount;
        }

        public static void TakeMoney(this User user, decimal amount)
        {
            user.Balance -= amount;
        }

        public static void WonMoney(this User user, decimal amount, bool addToBalance = true)
        {
            if (addToBalance) user.GiveMoney(amount);

            user.UserStat.AllTimeWon += amount;
            user.UserStat.BiggestWin = user.UserStat.BiggestWin > amount ? user.UserStat.BiggestWin : amount;
        }

        public static void LostMoney(this User user, decimal amount, bool removeFromBalance = true)
        {
            if (removeFromBalance) user.TakeMoney(amount);

            user.UserStat.AllTimeLost += amount;
            user.UserStat.BiggestLoss = user.UserStat.BiggestLoss > amount ? user.UserStat.BiggestLoss : amount;
        }

        public static (decimal Value, string Error) GetBetAmount(this User user, string? amount)
        {
            if (string.IsNullOrWhiteSpace(amount)) return (0m, string.Empty);

            switch (user.Balance)
            {
                case 0: return (0m, ":x: You have no money!");
                case < 0: return (0m, ":x: You are already in dept!");
            }

            amount = amount.ToLower();
            if (amount != "all" && !decimal.TryParse(amount, out _)) amount = "0";
            if (amount.Contains('.') && amount.Split('.')[1].Length > 2) return (0m, ":x: Invalid bet amount!");

            var betAmount = amount == "all" ? user.Balance : Convert.ToDecimal(amount);
            if (betAmount > user.Balance) return (0m, ":x: You can't bet more money than you have!");
            if (betAmount <= 0) return (0m, ":x: You can't bet nothing!");

            return (betAmount, string.Empty);
        }
    }
}
