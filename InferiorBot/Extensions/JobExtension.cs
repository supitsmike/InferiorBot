using InferiorBot.Classes;
using Infrastructure.InferiorBot;

namespace InferiorBot.Extensions
{
    public static class JobExtension
    {
        public static (decimal, bool) GetPayAmount(this Job job)
        {
            var randomNumber = NumericRandomizer.GetCryptoRandom().NextDouble();
            if (randomNumber <= job.Probability) return (NumericRandomizer.GenerateRandomNumber(job.PayMin, job.PayMax), true);

            var adjustedMin = job.PayMin - job.PayMax * 0.2m;
            return (NumericRandomizer.GenerateRandomNumber(adjustedMin, job.PayMin), false);
        }
    }
}
