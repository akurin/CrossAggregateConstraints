using System;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using EventStore.Core.Data;

namespace ESSecondaryIndices.Tests.Ports.Persistance
{
    public static class TestEventStore
    {
        public static ClusterVNode StartEmbedded()
        {
            var node = EmbeddedVNodeBuilder
                .AsSingleNode()
                .OnDefaultEndpoints()
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