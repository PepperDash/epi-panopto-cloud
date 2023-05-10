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
    /// General purpose implementation of <see cref="IDevicePreset"/>.
    /// Device drivers can derive this or implement their own class.
    /// </summary>
    public abstract class ADevicePreset : IDevicePreset
    {
        /// <summary>
        /// Constructs this immutable object with all the required properties.
        /// </summary>
        /// <param name="id">See <see cref="Id"/>.</param>
        /// <param name="name">See <see cref="Name"/>.</param>
        /// <param name="supportsIsActive">See <see cref="SupportsIsActive"/>.</param>
        /// <param name="isActive">See <see cref="IsActive"/>.</param>
        protected ADevicePreset(string id, string name, bool supportsIsActive, bool isActive)
        {
            Id = id;
            Name = name;
            SupportsIsActive = supportsIsActive;
            IsActive = isActive;
        }

        /// <summary>
        /// See <see cref="IDevicePreset.Id"/>
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// See <see cref="IDevicePreset.Name"/>
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// See <see cref="IDevicePreset.SupportsIsActive"/>
        /// </summary>
        public bool SupportsIsActive { get; private set; }

        /// <summary>
        /// See <see cref="IDevicePreset.IsActive"/>
        /// </summary>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// See <see cref="IDevicePreset.SupportsToggle"/>
        /// </summary>
        public bool SupportsToggle { get; protected set; }
    }
}