using System;
using System.Threading.Tasks;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using EventStore.Core.Data;

namespace CrossAggregateConstraints.Tests.Adapters.Persistance
{
    public static class EmbeddedEventStore
    {
        public static ClusterVNode Start()
        {
            var node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .OnDefaultEndpoints()
                .NoStatsOnPublicInterface()
                .NoAdminOnPublicInterface()
                .RunProjections(ProjectionsMode.All)
                .Build();

            var taskCompletionSource = new TaskCompletionSource<object>();

            node.NodeStatusChanged += (sender, args) =>
            {
                if (args.NewVNodeState == VNodeState.Master)
                {
                    taskCompletionSource.SetResult(new object());
                }
            };

            node.Start();

            taskCompletionSource.Task.Wait(TimeSpan.FromSeconds(30));

            return node;
        }
    }
}