using System;
using DSLink.Request;

namespace DSLink.Abstractions
{
    public abstract class Requester
    {
        public abstract IObservable<SubscriptionUpdate> Subscribe(string path);
    }
}