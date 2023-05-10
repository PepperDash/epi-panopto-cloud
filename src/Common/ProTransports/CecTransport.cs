// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.RAD.Common.Transports;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;

namespace Crestron.RAD.ProTransports
{
    public class CecTransport : ATransportDriver
    {
        private Cec _cec;
        private bool _sendAndReceive;
        private string _lastMessage;

        [System.Obsolete("MessageTimeoutTimer is deprecated.", false)]
        protected CTimer MessageTimeoutTimer;

        public CecTransport()
        {
        }

        public void Initialize(Cec hdmiOutput)
        {
            _cec = hdmiOutput;
            _cec.CecChange -= Cec_CecChange;
            _cec.CecChange += Cec_CecChange;
        }

        void Cec_CecChange(Cec cecDevice, CecEventArgs args)
        {
            switch (args.EventId)
            {
                case (CecEventIds.CecMessageReceivedEventId):
                {
                    var rx = cecDevice.Received.StringValue;

                    if (EnableRxDebug)
                        Log("Rx: " + rx);

                    if (_sendAndReceive &&
                        DataHandler != null)
                    {
                        DataHandler(rx);
                    }
                    break;
                }
                case (CecEventIds.ErrorFeedbackEventId):
                {
                    if (EnableLogging)
                    {
                        Log("CecTransport, Cec_CecChange: " + cecDevice.ErrorFeedback.StringValue); 
                    }
                    break;
                }
            }
        }

        public override void SendMethod(string message, object[] paramaters)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    if (paramaters != null && EnableLogging)
                    {
                        Log("RADProTransports.CecTransport.SendMethod Warning: parameters is null");
                    }

                    if (_sendAndReceive)
                    {
                        _lastMessage = message;
                        _cec.Send.StringValue = message;
                    }
                }
                else if (EnableLogging)
                {
                    Log("RADProTransports.CecTransport.SendMethod Notice: message is empty or null");
                }
            }
            catch (System.Exception ex)
            {
                Log(string.Format("RADProTransports.CecTransport.SendMethod error message: {0}", ex.Message));
            }
        }

        public override void Start()
        {
            _sendAndReceive = true;
        }

        public override void Stop()
        {
            _sendAndReceive = false;
        }

        protected virtual void ResponseTimerExpired(object nullParam)
        { }
    }
}
