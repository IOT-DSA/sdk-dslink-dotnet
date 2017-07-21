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
            node._subscribers.Add(subscriptionId);
            node.OnSubscribed?.Invoke(subscriptionId);
            _subscriptionToNode.Add(subscriptionId, node);
        }

        public void Unsubscribe(int sid)
        {
            try
            {
                var node = _subscriptionToNode[sid];
                lock (node._subscribers)
                {
                    _subscriptionToNode[sid]._subscribers.Remove(sid);
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
                            new JProperty("updates", SerializeUpdates(node))
                        });
                    }
                }
                await _link.Connector.Write(new JObject
                {
                    new JProperty("responses", responses)
                });
            }
        }

        public JArray SerializeUpdates(Node node)
        {
            var updates = new JArray();

            updates.Merge(node.Configs.CreateUpdateArray());
            updates.Merge(node.Attributes.CreateUpdateArray());

            lock (node.Children)
            {
                foreach (var child in node.Children)
                {
                    if (child.Value.Building) continue;
                    var value = new JObject();

                    foreach (var config in child.Value.Configs)
                    {
                        value[config.Key] = config.Value.JToken;
                    }
                    foreach (var attr in child.Value.Attributes)
                    {
                        value[attr.Key] = attr.Value.JToken;
                    }

                    if (!child.Value.Value.IsNull)
                    {
                        value["value"] = child.Value.Value.JToken;
                        value["ts"] = child.Value.Value.LastUpdated;
                    }

                    updates.Add(new JArray
                    {
                        child.Key,
                        value
                    });
                }
            }

            lock (node.RemovedChildren)
            {
                foreach (Node removedChild in node.RemovedChildren)
                {
                    updates.Add(new JObject
                    {
                        new JProperty("name", removedChild.Name),
                        new JProperty("change", "remove")
                    });
                }
                node.ClearRemovedChildren();
            }

            return updates;
        }

        public void ClearAll()
        {
            _subscriptionToNode.Clear();
        }
    }
}
