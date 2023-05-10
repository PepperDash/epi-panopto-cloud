// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Globalization;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace Crestron.Panopto.Common.Helpers
{
    public static class OsHelper
    {
        public static char OsFileSeparator { get { return Path.DirectorySeparatorChar; } }

        public static string ConvertPathBasedOnOs(string path)
        {
            var convertedPath = path;
            
            // Get correct seperator (OsFileSeperator)
            switch (OsFileSeparator)
            {
                case '\\':
                    // Replace all / with \\
                    convertedPath = convertedPath.Replace('/', '\\');
                    break;
                case '/':
                    convertedPath = convertedPath.Replace('\\', '/');
                    break;

            }
            return convertedPath;
        }
    }
}