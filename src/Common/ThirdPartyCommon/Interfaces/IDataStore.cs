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
using Crestron.SimplSharp.CrestronDataStore;

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IDataStore
    {
        CrestronDataStore.CDS_ERROR ClearLocal(string tag);
        CrestronDataStore.CDS_ERROR GetLocalValue(string tag, Type t, out object value);
        CrestronDataStore.CDS_ERROR GetGlobalValue(string tag, Type t, out object value);
        CrestronDataStore.CDS_ERROR Initialize();
        CrestronDataStore.CDS_ERROR SetLocalValue(string tag, object value);
        CrestronDataStore.CDS_ERROR SetGlobalValue(string tag, object value);
    }
}