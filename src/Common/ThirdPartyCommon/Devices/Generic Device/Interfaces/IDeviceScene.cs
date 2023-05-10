// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Interfaces
{
    /// <summary>
    /// Defines a device scene.
    /// A device scene may affect only the owning device or many devices in this device's ecosystem.
    /// </summary>
    /// <dev>
    /// IMPORTANT: This interface intentionally defines an immutable object.
    /// In other words, there are no methods or property setters.
    /// <para>
    /// For ease of implementation and for thread-safety, we want to force consumers
    /// to go through the device capability itself and not each individual scene.
    /// If we allow editing through this interface, it makes device driver implementations
    /// more difficult as well as subsystem code (as subsystems would then have to wrap each
    /// property to ensure it is being called on that subsystem's thread).
    /// </para>
    /// <para>
    /// The above statement appears as a dev-only comment because it is specific to the
    /// way we have implemented subsystems in our Tools.Runtime codebase.
    /// </para>
    /// </dev>
    public interface IDeviceScene
    {
        /// <summary>
        /// ID of the scene (assigned by the device or its ecosystem).
        /// <para>
        /// Uniqueness is only guaranteed within the owning device.
        /// If the owning device is the primary controller of an entire ecosystem,
        /// this ID would be unique across all the devices provided by that ecosystem.
        /// </para>
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Name of the scene (as configured using the device or its software).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates if the <see cref="IsActive"/> property is supported for this scene.
        /// <para>
        /// Depending on the device capabilities affected by a scene,
        /// some scenes may not be capable of providing feedback.
        /// This value can change if the device's scene is modified.
        /// </para>
        /// </summary>
        /// <remarks>
        /// NOTE: The changed event appears on the owning device interface <see cref="ISceneController"/>.
        /// This makes implementation easier and allows consumers of these interfaces to
        /// have context as to which device's scene changed.
        /// </remarks>
        bool SupportsIsActive { get; }

        /// <summary>
        /// Indicates if this scene is currently active.
        /// </summary>
        /// <remarks>
        /// NOTE: The changed event appears on the owning device interface <see cref="ISceneController"/>.
        /// This makes implementation easier and allows consumers of these interfaces to
        /// have context as to which device's scene changed.
        /// </remarks>
        bool IsActive { get; }
    }
}