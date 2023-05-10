using System;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IObserver
    {
        void Update(object updateInfo);
        void RescheduleConflictResult(bool result, PanoptoSession newSession);
    }
}