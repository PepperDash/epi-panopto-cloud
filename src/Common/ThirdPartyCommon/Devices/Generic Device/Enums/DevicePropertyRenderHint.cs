// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
namespace Crestron.Panopto.Common.Enums
{
    /// <summary>
    /// Defines hints for user interfaces as to the intended usage of a device property;
    /// Specified by the interface property <see cref="IDeviceProperty.RenderHint"/>.
    /// </summary>
    public enum DevicePropertyRenderHint
    {
        /// <summary>
        /// Indicates the property value should be displayed using normal editing controls,
        /// such as a check box for booleans, a text box for string, etc...
        /// </summary>
        None,

        /// <summary>
        /// Indicates the property is intended to be displayed as on/off switch.
        /// </summary>
        OnOff,

        /// <summary>
        /// Indicates the property is an additional value that can be set,
        /// in addition to an associated property whose render hint is <see cref="OnOff"/>.
        /// <para>
        /// In other words, there will be 2 properties.
        /// One will be the property that turns on/off the device feature.
        /// The other will be the one that configures that device feature.
        /// </para>
        /// <para>
        /// For example, turning on a strobe light may offer a preset list of flashing patterns to choose from.
        /// The intention here is that a user interface might want to show the 2 properties closely coupled together.
        /// This property’s <see cref="IDeviceProperty{T}.AvailableValues"/> will be populated.
        /// </para>
        /// </summary>
        OnOffValues,

        /// <summary>
        /// Indicates the property is intended to be displayed as range of values the user can target,
        /// for example, as a slider or a numeric spinner.
        /// </summary>
        SetpointTarget,

        /// <summary>
        /// Indicates the property is intended to be displayed as the actual/current value
        /// when there is another property with the hint of <see cref="SetpointTarget"/>.
        /// </summary>
        SetpointActual,

        /// <summary>
        /// Indicates the property is intended to be displayed as slider or similar control.
        /// <para>
        /// How exactly the slider is rendered can vary depending on the number of values specified by the range
        /// <see cref="IDeviceProperty{T}.MinValue"/> and <see cref="IDeviceProperty{T}.MaxValue"/>
        /// and the <see cref="IDeviceProperty{T}.StepSize"/> between them.
        /// </para>
        /// <para>
        /// The <see cref="IDeviceProperty.Units"/> should also be factored in when rendering the slider.
        /// </para>
        /// <para>
        /// If the <see cref="IDeviceProperty{T}.AvailableValues"/> is populated,
        /// it indicates custom labels from the device for each value (e.g. Slow, Medium, Fast).
        /// </para>
        /// </summary>
        Slider,

        /// <summary>
        /// Indicates the property is the status of an associated property.
        /// <para>
        /// For example, turning on pump may require a "priming" phase.
        /// One property would exist to turn the pump on/off.
        /// Another property with this render hint and the appropriate <see cref="IDeviceProperty.OwnerPropertyKey"/>
        /// would have a value of "Priming" while the pump is turning on.
        /// </para>
        /// <para>
        /// The intention here is that a user interface might want to show the 2 properties
        /// closely coupled together since one is the status of the other.
        /// </para>
        /// </summary>
        Status

        //FUTURE:
        //   CheckBox, TextBox, ComboBox,  SegmentedSlider, SmoothSlider
        //   Maybe a ToggleButton (stays pressed)
    }
}