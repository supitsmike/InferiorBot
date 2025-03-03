using System.Security.Cryptography;

namespace InferiorBot.Classes
{
    public static class NumericRandomizer
    {
        public static Random GetCryptoRandom(int size = sizeof(int))
        {
            var bytes = new byte[size];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);

            var seed = BitConverter.ToInt32(bytes, 0);
            return new Random(seed);
        }

        public static int GenerateRandomNumber(int maxValue)
        {
            return GetCryptoRandom().Next(maxValue);
        }

        public static int GenerateRandomNumber(int minValue, int maxValue)
        {
            return GetCryptoRandom().Next(minValue, maxValue + 1);
        }

        public static double GenerateRandomNumber(double maxValue)
        {
            var value = GetCryptoRandom(sizeof(double)).NextDouble() * maxValue;
            return Math.Round(value, 2);
        }

        public static double GenerateRandomNumber(double minValue, double maxValue)
        {
            var range = maxValue - minValue;
            var value = GetCryptoRandom(sizeof(double)).NextDouble() * range + minValue;
            return Math.Round(value, 2);
        }

        public static decimal GenerateRandomNumber(decimal maxValue)
        {
            var value = GenerateRandomNumber(Convert.ToDouble(maxValue));
            return Convert.ToDecimal(value);
        }

        public static decimal GenerateRandomNumber(decimal minValue, decimal maxValue)
        {
            var value = GenerateRandomNumber(Convert.ToDouble(minValue), Convert.ToDouble(maxValue));
            return Convert.ToDecimal(value);
        }
    }
}
