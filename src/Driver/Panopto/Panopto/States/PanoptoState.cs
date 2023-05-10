using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;

namespace Crestron.Panopto
{
    public abstract class PanoptoState
    {
        public Panopto.Driver P;

        public virtual void StopState()
        {
            PanoptoLogger.Notice("Panopto.PanoptoState.StopState");
        }

        public virtual void HttpDownloadDataHandler(HttpsResponseArgs args)
        {
        }

        public virtual void HttpDataHandler(HttpsResponseArgs args)
        {
        }

        public virtual void NewRecording()
        {
        }

        public virtual void Record(string recordingName, DateTime startTime, double duration, bool isBroadcast)
        {
            PanoptoLogger.Notice("Panopto.PanoptoState.Record");
        }

        public virtual void Pause()
        {
        }

        public virtual void Resume()
        {
        }

        public virtual void Preview(string recordingName)
        {
        }

        public virtual void RecordEarly()
        {
            PanoptoLogger.Notice("Panopto.PanoptoState.RecordEarly");
        }

        public virtual List<PanoptoSession> GetUpcomingRecordings()
        {
            return new List<PanoptoSession>();
        }

        public virtual void Stop()
        {
        }

        public virtual void Extend()
        {
        }

        public virtual void Back()
        {
        }

        public virtual void SetPreviewRefreshRate(int rate)
        {
        }

        public virtual void RescheduleSession(Guid sessionId, DateTime startTime, DateTime endTime, PanoptoSession newSession)
        {
        }

        public virtual int RecordUntil(string recordingName, bool isBroadcast)
        {
            PanoptoLogger.Notice("Panopto.PanoptoState.RecordUntil");
            return 0;
        }

        public virtual void TryAutoConfigure()
        {
        }

        public abstract string GetStateName();

        protected void ChangeState(PanoptoState state, bool transitioning, DisplayMessageEnum message)
        {
            ChangeState(state, transitioning, message, null);
        }

        protected void ChangeState(PanoptoState state, bool transitioning, DisplayMessageEnum message, PanoptoSession sessionInfo)
        {
            PanoptoLogger.Notice("PanoptoState.ChangeState");
            try
            {
                if (state != null)
                {
                    if (P != null)
                    {
                        P.ChangeState(state, transitioning, message, sessionInfo);
                    }
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("PanoptoState.ChangeState Error Message is {0}", e.Message);
            }
        }

        public virtual PanoptoSession GetCurrentSession(Panopto.Driver panopto)
        {
            return null as PanoptoSession;
        }
    }
}