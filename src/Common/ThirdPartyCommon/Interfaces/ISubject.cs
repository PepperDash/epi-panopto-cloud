using System;
using System.Collections.Generic;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface ISubject
    {
        List<IObserver> Observers
        {
            get;
            set;
        }

        void Register(IObserver observer);

        void Unregister(IObserver observer);

        void Notify();
    }
}