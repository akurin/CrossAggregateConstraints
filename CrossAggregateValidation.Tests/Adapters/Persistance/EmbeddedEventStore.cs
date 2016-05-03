using System;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using EventStore.Core.Data;

namespace CrossAggregateValidation.Tests.Adapters.Persistance
{
    public static class EmbeddedEventStore
    {
        public static ClusterVNode StartAndWaitUntilReady()
        {
            var node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .OnDefaultEndpoints()
                .NoStatsOnPublicInterface()
                .NoAdminOnPublicInterface()
                .RunProjections(ProjectionsMode.All)
                .Build();

            node.StartAndWaitUntilReady().Wait(TimeSpan.FromSeconds(10));

            return node;
        }
    }
}