using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Crestron.Panopto.Common.BasicDriver
{
    /// <summary>
    /// Used for TX and general driver maintenance 
    /// </summary>
    public static class DriverClock
    {
        internal static uint Clock25ms = 25;
        internal static CTimer Driver25msClock = new CTimer(Driver25msClockCallback, null, Clock25ms);
        internal static Action Driver25msClockEventHandler;

        internal static void Driver25msClockCallback(object notUsed)
        {
            // Try/Catch so that the timer is reset even if something threw an exception
            try
            {
                if (Driver25msClockEventHandler != null)
                {
                    Driver25msClockEventHandler();
                }
            }
            catch (Exception e)
            {
                // Printing to error log for now since this exception should not happen very often.
                // CCDRV-2586 covers updating this at a later point to use a static logger.
                ErrorLog.Notice(
                    "Crestron.Panopto.Common.BasicDriver.DriverClock.Driver25msClockCallback encountered an exception: {0}",
                    e);
            }

            Driver25msClock.Reset(Clock25ms);
        }
    }
}