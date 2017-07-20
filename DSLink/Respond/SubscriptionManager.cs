using System.Collections.Generic;
using DSLink.Nodes;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace DSLink.Respond
{
    public class SubscriptionManager
    {
        private readonly Dictionary<int, Node> _subscriptionToNode;
        private readonly DSLinkContainer _link;

        public SubscriptionManager(DSLinkContainer link)
        {
            _subscriptionToNode = new Dictionary<int, Node>();
            _link = link;
        }

        public void Subscribe(int subscriptionId, Node node)
        {
            node.Subscribers.Add(subscriptionId);
            node.OnSubscribed?.Invoke(subscriptionId);
            _subscriptionToNode.Add(subscriptionId, node);
        }

        public void Unsubscribe(int sid)
        {
            try
            {
                var node = _subscriptionToNode[sid];
                lock (node.Subscribers)
                {
                    _subscriptionToNode[sid].Subscribers.Remove(sid);
                }
                _subscriptionToNode[sid].OnUnsubscribed?.Invoke(sid);
                _subscriptionToNode.Remove(sid);
            }
            catch (KeyNotFoundException)
            {
                _link.Logger.Debug($"Failed to Unsubscribe: unknown subscription id {sid}");
            }
        }

        public async Task UpdateSubscribers(Node node)
        {
            if (node.Building)
            {
                return;
            }

            if (node.Streams.Count > 0)
            {
                var responses = new JArray();
                lock (node.Streams)
                {
                    foreach (var stream in node.Streams)
                    {
                        responses.Add(new JObject
                        {
                            new JProperty("rid", stream),
                            new JProperty("stream", "open"),
                            new JProperty("updates", node.SerializeUpdates())
                        });
                    }
                }
                await _link.Connector.Write(new JObject
                {
                    new JProperty("responses", responses)
                });
            }
        }

        public void ClearAll()
        {
            _subscriptionToNode.Clear();
        }
    }
}
