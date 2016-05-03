using System.Collections.Generic;
using DSLink.Connection.Serializer;
using DSLink.Container;

namespace DSLink
{
    public class Requester
    {
        private readonly AbstractContainer _link;

        internal Requester(AbstractContainer link)
        {
            _link = link;
        }

        internal List<RequestObject> ProcessRequests(List<ResponseObject> responses)
        {
            var requests = new List<RequestObject>();
            // TODO: finish up requester
            /*foreach (var response in responses)
            {
                response.
            }*/

            return requests;
        }
    }
}
