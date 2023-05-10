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
    public sealed class DevicePropertyValueEventArgs<T> : DevicePropertyValueEventArgs
    {
        public DevicePropertyValueEventArgs(string key, T value)
        {
            Key = key;
            Value = value;
        }

        public T Value { get; private set; }

        public override TValue GetValue<TValue>()
        {
            if (typeof(TValue) == typeof(T))
                return ((DevicePropertyValueEventArgs<TValue>)(object)this).Value;

            return (TValue)(object)Value;
        }
    }

    public abstract class DevicePropertyValueEventArgs : EventArgs
    {
        /// <summary>
        /// Key of property; See <see cref="IDeviceProperty.Key"/>.
        /// </summary>
        public string Key { get; protected set; }

        /// <summary>
        /// Retrieves the new value of the property referred to by <see cref="Key"/>.
        /// </summary>
        public abstract TValue GetValue<TValue>();
    }
}