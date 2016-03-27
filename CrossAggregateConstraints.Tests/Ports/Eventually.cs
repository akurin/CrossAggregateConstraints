using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CrossAggregateConstraints.Tests.Ports
{
    public static class Eventually
    {
        public static void IsTrue(Func<bool> condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                var conditionResult = condition();
                if (conditionResult)
                    return;

                if (stopwatch.Elapsed > TimeSpan.FromSeconds(3000))
                    Assert.Fail();
            }
        }
    }
}