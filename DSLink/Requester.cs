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
    }
}
