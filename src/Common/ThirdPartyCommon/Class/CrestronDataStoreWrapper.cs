// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.Panopto.Common.Interfaces;
using Crestron.SimplSharp.CrestronDataStore;

namespace Crestron.Panopto.Common
{
    public class CrestronDataStoreWrapper : IDataStore
    {
        #region IDataStore Members

        public CrestronDataStore.CDS_ERROR ClearLocal(string tag)
        {
            return CrestronDataStoreStatic.clearLocal(tag);
        }

        public CrestronDataStore.CDS_ERROR GetLocalValue(string tag, Type t, out object value)
        {
            CrestronDataStore.CDS_ERROR errorCode = CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
            value = null;
            if (t.Equals(typeof(bool)))
            {
                bool temp;
                errorCode = CrestronDataStoreStatic.GetLocalBoolValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(double)))
            {
                double temp;
                errorCode = CrestronDataStoreStatic.GetLocalDoubleValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(int)))
            {
                int temp;
                errorCode = CrestronDataStoreStatic.GetLocalIntValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(long)))
            {
                long temp;
                errorCode = CrestronDataStoreStatic.GetLocalLongValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(string)))
            {
                string temp;
                errorCode = CrestronDataStoreStatic.GetLocalStringValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(uint)))
            {
                uint temp;
                errorCode = CrestronDataStoreStatic.GetLocalUintValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(ulong)))
            {
                ulong temp;
                errorCode = CrestronDataStoreStatic.GetLocalULongValue(tag, out temp);
                value = temp;
            }
            return errorCode;
        }

        public CrestronDataStore.CDS_ERROR GetGlobalValue(string tag, Type t, out object value)
        {
            CrestronDataStore.CDS_ERROR errorCode = CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
            value = null;
            if (t.Equals(typeof(bool)))
            {
                bool temp;
                errorCode = CrestronDataStoreStatic.GetGlobalBoolValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(double)))
            {
                double temp;
                errorCode = CrestronDataStoreStatic.GetGlobalDoubleValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(int)))
            {
                int temp;
                errorCode = CrestronDataStoreStatic.GetGlobalIntValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(long)))
            {
                long temp;
                errorCode = CrestronDataStoreStatic.GetGlobalLongValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(string)))
            {
                string temp;
                errorCode = CrestronDataStoreStatic.GetGlobalStringValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(uint)))
            {
                uint temp;
                errorCode = CrestronDataStoreStatic.GetGlobalUintValue(tag, out temp);
                value = temp;
            }
            else if (t.Equals(typeof(ulong)))
            {
                ulong temp;
                errorCode = CrestronDataStoreStatic.GetGlobalULongValue(tag, out temp);
                value = temp;
            }
            return errorCode;
        }

        public CrestronDataStore.CDS_ERROR Initialize()
        {
            return CrestronDataStoreStatic.InitCrestronDataStore();
        }

        public CrestronDataStore.CDS_ERROR SetLocalValue(string tag, object value)
        {
            CrestronDataStore.CDS_ERROR errorCode = CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
            Type t = value.GetType();
            if(t.Equals(typeof(bool)))
            {
                errorCode = CrestronDataStoreStatic.SetLocalBoolValue(tag, (bool)value);
            }
            else if(t.Equals(typeof(double)))
            {
                errorCode = CrestronDataStoreStatic.SetLocalDoubleValue(tag, (double)value);
            }
            else if (t.Equals(typeof(int)))
            {
                errorCode = CrestronDataStoreStatic.SetLocalIntValue(tag, (int)value);
            }
            else if (t.Equals(typeof(long)))
            {
                errorCode = CrestronDataStoreStatic.SetLocalLongValue(tag, (long)value);
            }
            else if (t.Equals(typeof(string)))
            {
                errorCode = CrestronDataStoreStatic.SetLocalStringValue(tag, (string)value);
            }
            else if (t.Equals(typeof(uint)))
            {
                errorCode = CrestronDataStoreStatic.SetLocalUintValue(tag, (uint)value);
            }
            else if (t.Equals(typeof(ulong)))
            {
                errorCode = CrestronDataStoreStatic.SetLocalULongValue(tag, (ulong)value);
            }

            if (errorCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
            {
                CrestronDataStoreStatic.Flush();
            }
            return errorCode;
        }

        public CrestronDataStore.CDS_ERROR SetGlobalValue(string tag, object value)
        {
            CrestronDataStore.CDS_ERROR errorCode = CrestronDataStore.CDS_ERROR.CDS_SUCCESS;
            Type t = value.GetType();
            if (t.Equals(typeof(bool)))
            {
                errorCode = CrestronDataStoreStatic.SetGlobalBoolValue(tag, (bool)value);
            }
            else if (t.Equals(typeof(double)))
            {
                errorCode = CrestronDataStoreStatic.SetGlobalDoubleValue(tag, (double)value);
            }
            else if (t.Equals(typeof(int)))
            {
                errorCode = CrestronDataStoreStatic.SetGlobalIntValue(tag, (int)value);
            }
            else if (t.Equals(typeof(long)))
            {
                errorCode = CrestronDataStoreStatic.SetGlobalLongValue(tag, (long)value);
            }
            else if (t.Equals(typeof(string)))
            {
                errorCode = CrestronDataStoreStatic.SetGlobalStringValue(tag, (string)value);
            }
            else if (t.Equals(typeof(uint)))
            {
                errorCode = CrestronDataStoreStatic.SetGlobalUintValue(tag, (uint)value);
            }
            else if (t.Equals(typeof(Int64)))
            {
                errorCode = CrestronDataStoreStatic.SetGlobalLongValue(tag, (Int64)value);
            }
            else if (t.Equals(typeof(UInt64)))
            {
                errorCode = CrestronDataStoreStatic.SetGlobalLongValue(tag, (UInt64)value);
            }

            if (errorCode == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
            {
                CrestronDataStoreStatic.Flush();
            }
            return errorCode;
        }

        #endregion
    }
}