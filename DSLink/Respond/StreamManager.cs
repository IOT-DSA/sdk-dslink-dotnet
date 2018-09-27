using System.Collections.Generic;
using DSLink.Nodes;
using DSLink.Logging;

namespace DSLink.Respond
{
    public class StreamManager
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly Dictionary<int, string> _requestIdToPath = new Dictionary<int, string>();
        private readonly DSLinkContainer _link;

        public StreamManager(DSLinkContainer link)
        {
            _link = link;
        }

        public void OpenStream(int requestId, Node node)
        {
            _requestIdToPath.Add(requestId, node.Path);
            lock (node._streams)
            {
                node._streams.Add(requestId);
            }
        }

        public void OpenStreamLater(int requestId, string path)
        {
            _requestIdToPath.Add(requestId, path);
        }

        public void CloseStream(int requestId)
        {
            if (_requestIdToPath.TryGetValue(requestId, out string path))
            {
                var node = _link.Responder.SuperRoot.Get(path);
                if (node != null)
                {
                    lock (node._streams)
                    {
                        node._streams.Remove(requestId);
                    }
                }
                _requestIdToPath.Remove(requestId);
            }
            else
            {
                Logger.Debug($"Failed to Close: unknown request id or node for {requestId}");
            }
        }

        public void OnActivateNode(Node node)
        {
            foreach (var id in _requestIdToPath.Keys)
            {
                var path = _requestIdToPath[id];
                if (path == node.Path)
                {
                    lock (node._streams)
                    {
                        node._streams.Add(id);
                    }
                }
            }
        }
                 
        internal void ClearAll()
        {
            _requestIdToPath.Clear();
        }
    }
}
