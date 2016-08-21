using System.Linq;
using NSpec;
using NSpec.Domain;
using NSpec.Domain.Formatters;
using NUnit.Framework;

namespace CrossAggregateValidation.Tests
{
    [TestFixture]
    public class Program
    {
        [Test]
        public static void Main()
        {
            var tagOrClassName = "EventHandlingSubscriptionStarterSpec";

            var types = typeof(Program).Assembly.GetTypes(); 
            // OR
            // var types = new Type[]{typeof(Some_Type_Containg_some_Specs)};
            var finder = new SpecFinder(types, "");
            var tagsFilter = new Tags().Parse(tagOrClassName);
            var builder = new ContextBuilder(finder, tagsFilter, new DefaultConventions());
            var runner = new ContextRunner(tagsFilter, new ConsoleFormatter(), failFast: true);
            var results = runner.Run(builder.Contexts().Build());

            //assert that there aren't any failures
            results.Failures().Count().should_be(0);
        }
    }
}
