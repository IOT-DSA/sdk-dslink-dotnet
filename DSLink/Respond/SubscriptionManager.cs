using System.Collections.Generic;
using DSLink.Container;
using DSLink.Nodes;

namespace DSLink.Respond
{
    public class SubscriptionManager
    {
        private readonly Dictionary<int, Node> _subscriptionToNode;
        private readonly AbstractContainer _link;

        public SubscriptionManager(AbstractContainer link)
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

        public void ClearAll()
        {
            _subscriptionToNode.Clear();
        }
    }
}
