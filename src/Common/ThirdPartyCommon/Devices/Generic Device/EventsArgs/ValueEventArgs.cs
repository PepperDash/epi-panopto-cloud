// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Events
{
    /// <summary>
    /// Argument for events that refer to a value.
    /// </summary>
    public sealed class ValueEventArgs<T> : EventArgs
    {
        public ValueEventArgs(T value)
        {
            Value = value;
        }

        /// <summary>
        /// New value being referred to by the event that was raised
        /// </summary>
        public T Value { get; private set; }
    }
}