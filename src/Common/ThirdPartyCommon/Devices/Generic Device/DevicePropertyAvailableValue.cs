// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.Panopto.Common.Interfaces;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common
{
    /// <summary>
    /// General purpose implementation of <see cref="IDevicePropertyAvailableValue{T}"/>.
    /// </summary>
    public sealed class DevicePropertyAvailableValue<T> : IDevicePropertyAvailableValue<T>
    {
        public DevicePropertyAvailableValue(int localizedNameId, T value)
            : this(localizedNameId, value, 0)
        {
        }

        public DevicePropertyAvailableValue(int localizedNameId, T value, int localizedCategoryNameId)
        {
            LocalizedNameId = localizedNameId;
            Value = value;
            LocalizedCategoryNameId = localizedCategoryNameId;
        }

        /// <see cref="IDevicePropertyAvailableValue{T}.LocalizedCategoryNameId"/>
        public int LocalizedCategoryNameId { get; private set; }

        /// <see cref="IDevicePropertyAvailableValue{T}.LocalizedNameId"/>
        public int LocalizedNameId { get; private set; }

        /// <see cref="IDevicePropertyAvailableValue{T}.LocalizedNameId"/>
        public T Value { get; private set; }

        public TValue GetValue<TValue>()
        {
            var @this = this as DevicePropertyAvailableValue<TValue>;
            if (@this != null)
                return @this.Value;

            return (TValue)(object)Value;
        }
    }
}