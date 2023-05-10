// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;

namespace Crestron.Panopto.Common.Helpers
{
    public static class UserAttributeHelper
    {
        public static List<AttributeTypeDefaults> DefaultValues
        {
            get
            {
                return new List<AttributeTypeDefaults>
                {
                    (new AttributeTypeDefaults
                    {
                        AttributeType = UserAttributeType.DeviceId,
                        AttributeDetails = new List<AttributeDetails>
                        {
                            new AttributeDetails(AttributeField.ParameterId, "ID", FieldAccessibility.ReadOnly ),
                            new AttributeDetails(AttributeField.Label, "Device ID", FieldAccessibility.ReadOnly ),
                            new AttributeDetails(AttributeField.Description, "The Device ID used in the API.", FieldAccessibility.ReadOnly ),
                            new AttributeDetails(AttributeField.DataType, UserAttributeDataType.String, FieldAccessibility.Editable ),
                            new AttributeDetails(AttributeField.DefaultValue, "1", FieldAccessibility.Editable ),
                        }
                    }),
                    (new AttributeTypeDefaults
                    {
                        AttributeType = UserAttributeType.OnScreenId,
                        AttributeDetails = new List<AttributeDetails>
                        {
                            new AttributeDetails(AttributeField.ParameterId, "OnScreenID", FieldAccessibility.ReadOnly ),
                            new AttributeDetails(AttributeField.Label, "On-Screen ID", FieldAccessibility.ReadOnly ),
                            new AttributeDetails(AttributeField.Description, "The ID shown on-screen by the device.", FieldAccessibility.ReadOnly ),
                            new AttributeDetails(AttributeField.DataType, UserAttributeDataType.String, FieldAccessibility.Editable ),
                            new AttributeDetails(AttributeField.DefaultValue, "1", FieldAccessibility.Editable ),
                        }
                    }),
                    (new AttributeTypeDefaults
                    {
                        AttributeType = UserAttributeType.MessageBox,
                        AttributeDetails = new List<AttributeDetails>
                        {
                            new AttributeDetails(AttributeField.ParameterId, null, FieldAccessibility.NotVisible ),
                            new AttributeDetails(AttributeField.Label, "Message", FieldAccessibility.Editable ),
                            new AttributeDetails(AttributeField.Description, "Message to show the user.", FieldAccessibility.Editable ),
                            new AttributeDetails(AttributeField.DataType, UserAttributeDataType.String, FieldAccessibility.ReadOnly ),
                            new AttributeDetails(AttributeField.DefaultValue, string.Empty, FieldAccessibility.Editable ),
                        }
                    }),
                    (new AttributeTypeDefaults
                    {
                        AttributeType = UserAttributeType.Custom,
                        AttributeDetails = new List<AttributeDetails>
                        {
                            new AttributeDetails(AttributeField.ParameterId, string.Empty, FieldAccessibility.Editable ),
                            new AttributeDetails(AttributeField.Label, "Message", FieldAccessibility.Editable ),
                            new AttributeDetails(AttributeField.Description, "Message to show the user.", FieldAccessibility.Editable ),
                            new AttributeDetails(AttributeField.DataType, UserAttributeDataType.String, FieldAccessibility.Editable ),
                            new AttributeDetails(AttributeField.DefaultValue, string.Empty, FieldAccessibility.Editable ),
                        }
                    }),
                };
            }
        } 
    }

    public class AttributeTypeDefaults
    {
        public UserAttributeType AttributeType { get; set; }
        public List<AttributeDetails> AttributeDetails { get; set; }
    }

    public class AttributeDetails
    {
        public AttributeField FieldName { get; private set; }
        public object DefaultValue { get; private set; }
        public FieldAccessibility Accessibility { get; private set; }

        public AttributeDetails(AttributeField field, object value, FieldAccessibility access)
        {
            FieldName = field;
            DefaultValue = value;
            Accessibility = access;
        }
    }

    public enum AttributeField
    {
        AttributeType = 1,
        ParameterId = 2,
        Label = 3,
        Description = 4,
        DataType = 5,
        DataMask = 6,
        DefaultValue = 7
    }

    public enum FieldAccessibility
    {
        NotVisible = 1,
        Editable = 2,
        ReadOnly = 3
    }
}