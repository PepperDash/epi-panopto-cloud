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
    /// Defines one available value for use with an <see cref="IDeviceProperty{T}"/> instance.
    /// </summary>
    public interface IDevicePropertyAvailableValue<T> : IDevicePropertyAvailableValue
    {
        /// <summary>
        /// The underlying property value to be set when this available value is chosen.
        /// </summary>
        T Value { get; }
    }

    /// <summary>
    /// Defines one available value for use with an <see cref="IDeviceProperty{T}"/> instance.
    /// </summary>
    public interface IDevicePropertyAvailableValue
    {
        /// <summary>
        /// ID referring to the localized name of the category to which this property value belongs;
        /// This will be 0 if this value does not belong to any category.
        /// <para>
        /// This ID should not be persisted by any consuming code.
        /// It is only for display/categorization purposes for the current instance of the running program/app.
        /// </para>
        /// <para>
        /// When "available values" are intended to be displayed with other
        /// similar "available values", they will all have the same category name.
        /// For example, if this value is the color "Red", this category name may be "Primary Colors".
        /// There may be another value named "Purple" and its category name would be "Secondary Colors".
        /// </para>
        /// <para>
        /// When displaying a mix of categorized and uncategorized values,
        /// it is recommended that the uncategorized values be shown last,
        /// possibly in their own category named "Uncategorized" or "Other", for example.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This localized string can be retrieved from the device via the interface <see cref="ILocalizedDevice"/>.
        /// </remarks>
        int LocalizedCategoryNameId { get; }

        /// <summary>
        /// ID referring to the localized display name of this property value.
        /// </summary>
        /// <remarks>
        /// This localized string can be retrieved from the device via the interface <see cref="ILocalizedDevice"/>.
        /// </remarks>
        int LocalizedNameId { get; }

        /// <summary>
        /// Retrieves the value of this property.
        /// <para>
        /// If the requested type does not match the type referred to by <see cref="Type"/>,
        /// an <see cref="InvalidCastException"/> will be thrown.
        /// </para>
        /// <para>
        /// The only other allowed type is "object", with the caveat that it will cause value types to be boxed.
        /// </para>
        /// </summary>
        TValue GetValue<TValue>();
    }
}