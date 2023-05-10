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
    /// General purpose implementation of <see cref="IDeviceProperty"/>.
    /// Device drivers can derive this or implement their own class.
    /// </summary>
    /// <seealso cref="DeviceProperty{T}"/>
    public abstract class ADeviceProperty : IDeviceProperty
    {
        protected ADeviceProperty(
            string key,
            int localizedNameId,
            DevicePropertyAttributes attributes,
            DevicePropertyType type,
            DevicePropertyUnit units,
            DevicePropertyRenderHint renderHint,
            string parentPropertyKey)
        {
            Key = key;
            LocalizedNameId = localizedNameId;
            Attributes = attributes;
            Type = type;
            Units = units;
            RenderHint = renderHint;
            ParentPropertyKey = parentPropertyKey;
        }

        public string Key { get; private set; }
        public int LocalizedNameId { get; private set; }
        public DevicePropertyAttributes Attributes { get; private set; }
        public DevicePropertyType Type { get; private set; }
        public DevicePropertyUnit Units { get; protected set; }
        public DevicePropertyRenderHint RenderHint { get; private set; }
        public string ParentPropertyKey { get; private set; }
        public abstract TValue GetValue<TValue>();
        public abstract TValue GetMinValue<TValue>();
        public abstract TValue GetMaxValue<TValue>();
        public abstract TValue GetStepSize<TValue>();

        public DevicePropertyStates State { get; protected set; }
    }

    /// <summary>
    /// General purpose implementation of <see cref="IDeviceProperty{T}"/>.
    /// Device drivers can derive this or implement their own class.
    /// </summary>
    public class DeviceProperty<T> : ADeviceProperty, IDeviceProperty<T>
    {
        protected DeviceProperty(
            string key,
            int localizedNameId,
            DevicePropertyAttributes attributes,
            DevicePropertyType type,
            DevicePropertyUnit units,
            DevicePropertyRenderHint renderHint,
            string parentPropertyKey)

            : base(key, localizedNameId, attributes, type, units, renderHint, parentPropertyKey)
        {
            MinValue = default(T);
            MaxValue = default(T);
            StepSize = default(T);
        }

        protected DeviceProperty(
            string key,
            int localizedNameId,
            DevicePropertyAttributes attributes,
            DevicePropertyType type,
            DevicePropertyUnit units,
            DevicePropertyRenderHint renderHint,
            string parentPropertyKey,
            T minValue,
            T maxValue,
            T stepSize)

            : base(key, localizedNameId, attributes, type, units, renderHint, parentPropertyKey)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            StepSize = stepSize;
        }

        public T Value { get; protected set; }

        public T MinValue { get; private set; }
        public T MaxValue { get; private set; }
        public T StepSize { get; private set; }

        public override TValue GetValue<TValue>()
        {
            var @this = this as DeviceProperty<TValue>;
            if (@this != null)
                return @this.Value;

            return (TValue)(object)Value;
        }

        public override TValue GetMinValue<TValue>()
        {
            var @this = this as DeviceProperty<TValue>;
            if (@this != null)
                return @this.MinValue;

            return (TValue)(object)MinValue;
        }

        public override TValue GetMaxValue<TValue>()
        {
            var @this = this as DeviceProperty<TValue>;
            if (@this != null)
                return @this.MaxValue;

            return (TValue)(object)MaxValue;
        }

        public override TValue GetStepSize<TValue>()
        {
            var @this = this as DeviceProperty<TValue>;
            if (@this != null)
                return @this.StepSize;

            return (TValue)(object)StepSize;
        }

        #region IDeviceProperty<T> Members

        public virtual IEnumerable<IDevicePropertyAvailableValue<T>> AvailableValues
        {
            get { return default(IEnumerable<IDevicePropertyAvailableValue<T>>); }
        }

        #endregion

    }
}