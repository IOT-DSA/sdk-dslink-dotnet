using System.Collections.Generic;
using DSLink.Container;
using DSLink.Nodes;

namespace DSLink.Respond
{
    public class StreamManager
    {
        private readonly Dictionary<int, string> _requestIdToPath = new Dictionary<int, string>();
        private readonly AbstractContainer _link;

        public StreamManager(AbstractContainer link)
        {
            _link = link;
        }

        public void OpenStream(int requestId, Node node)
        {
            _requestIdToPath.Add(requestId, node.Path);
            lock (node.Streams)
            {
                node.Streams.Add(requestId);
            }
        }

        public void OpenStreamLater(int requestId, string path)
        {
            _requestIdToPath.Add(requestId, path);
        }

        public void CloseStream(int requestId)
        {
            try
            {
                var node = _link.Responder.SuperRoot.Get(_requestIdToPath[requestId]);
                if (node != null)
                {
                    lock (node.Streams)
                    {
                        node.Streams.Remove(requestId);
                    }
                }
                _requestIdToPath.Remove(requestId);
            }
            catch (KeyNotFoundException)
            {
                _link.Logger.Debug($"Failed to Close: unknown request id or node for {requestId}");
            }
        }

        public void OnActivateNode(Node node)
        {
            foreach (var id in _requestIdToPath.Keys)
            {
                var path = _requestIdToPath[id];
                if (path == node.Path)
                {
                    node.Streams.Add(id);
                }
            }
        }
                 
        internal void ClearAll()
        {
            _requestIdToPath.Clear();
        }
    }
}
