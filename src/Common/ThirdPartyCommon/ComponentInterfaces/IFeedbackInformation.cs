// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IFeedbackInformation
    {
        /// <summary>
        /// Indicates if the device supports feedback
        /// </summary>
        bool SupportsFeedback { get; }

        /// <summary>
        /// Approximate minimum number of milliseconds the device or driver will report state/feedback changes.
        /// <para>
        /// The formula for this value is typically:
        /// [Polling Interval (if any)] + [Device Response Time] + [Response Processing Time].
        /// </para>
        /// <para>
        /// This may be 0 or a very small value if the device responds quickly
        /// and driver processing time is negligible.
        /// </para>
        /// <para>
        /// Amongst other reasons, this property can be used to write smarter code that waits at least this amount of time
        /// before processing changes to provide optimize updates to higher level components.
        /// </para>
        /// <para>
        /// If no capabilities of the device support feedback, this can be ignored.
        /// </para>
        /// </summary>
        uint MinimumResponseTime { get; }

        /// <summary>
        /// Approximate maximum number of milliseconds the device or driver will take
        /// before reporting state/feedback changes after the device receives a command.
        /// <para>
        /// The formula for this value is typically:
        /// ([Device Response Time] + [Response Processing Time]) * 2 + [Polling Interval (if any)].
        /// </para>
        /// <para>
        /// The first part of the equation is multiplied by 2 because a command may be sent to the device
        /// just as the driver is beginning a poll.
        /// </para>
        /// <para>
        /// Amongst other reasons, this property can be used to write smarter code that waits at least this amount of time
        /// before timing out or declaring a device unresponsive.
        /// </para>
        /// <para>
        /// If no capabilities of the device support feedback, this can be ignored.
        /// </para>
        /// </summary>
        uint MaximumResponseTime { get; }
    }
}
