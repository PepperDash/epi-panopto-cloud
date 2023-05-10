// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Transports
{
	public class DataObjects
	{
		/// <summary>
		/// Represents a loaded drivers basic properties
		/// </summary>
		public class DriverMetadata
		{
			public bool DriverLoaded { get; internal set; }
            public string DeviceType { get; internal set; }
            public string Manufacturer { get; internal set; }
            public string DeviceModel { get; internal set; }
		}
	}
}
