// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using System.Text;
using Crestron.RAD.Common.Enums;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

namespace Crestron.RAD.Common
{
    public static class DriverManager
    {
        /// <summary>
        /// The path to the folder where the Dynamic Driver files are located.
        /// </summary>
        public static string DriverPath { get; set; }

        /// <summary>
        /// List of all Driver Data generated form the manifest files.
        /// </summary>
        public static List<DriverManifest> Drivers { get; private set; }

        static DriverManager()
        {
            Drivers = new List<DriverManifest>();
            DownloadManager.FileDownloaded += DownloadManagerOnFileDownloaded;
        }

        /// <summary>
        /// Function to call Refresh when a file is downloaded by the download manager.
        /// </summary>
        private static void DownloadManagerOnFileDownloaded(string fileName, DeviceTypes deviceType)
        {
            Refresh();
        }

        /// <summary>
        /// Function to check the DriverPath for Manifest Files and update the List "Drivers".  Called automatically when a file is downloaded
        /// from the DownloadManager.
        /// </summary>
        public static void Refresh()
        {
            FindZips(DriverPath);
            FindManifests(DriverPath);
        }

        public static void FindZips(string path)
        {

            var zipfiles = Directory.GetFiles(path, "*.zip");
            foreach (var zipFile in zipfiles)
            {
                CrestronZIP.Unzip(zipFile, path);
            }
        }

        /// <summary>
        /// Helper function called by Refresh.
        /// </summary>
        private static void FindManifests(string path)
        {
            var manifestFiles = Directory.GetFiles(path, "*.manifest");
            Drivers.Clear();
            foreach (var manifestFile in manifestFiles)
            {
                var read = File.ReadToEnd(manifestFile, Encoding.Default);
                var driverData = JsonConvert.DeserializeObject<DriverManifest>(read);
                Drivers.Add(driverData);
            }
        }
    }

    public class DriverManifest
    {
        public string DeviceTypeSring { get; set; }
        public DeviceTypes DeviceTypeEnum
        {
            get
            {
                try
                {
                    return (DeviceTypes)Enum.Parse(typeof(DeviceTypes), DeviceTypeSring, true);
                }
                catch (Exception)
                {
                    return (DeviceTypes)Enum.Parse(typeof(DeviceTypes), "Unknown", true);
                }
            }
        }

        public string TransportType { get; set; }
        public TransportTypes TransportTypeEnum
        {
            get
            {
                try
                {
                    return (TransportTypes)Enum.Parse(typeof(TransportTypes), TransportType, true);
                }
                catch (Exception)
                {
                    return TransportTypes.Unknown;
                }
            }
        }

        public MakeAndModel[] MakeAndModels { get; set; }

        public string FileName { get; set; }
        public string Description { get; set; }
        public DateTime VersionDate { get; set; }
        public string Version { get; set; }
        // ReSharper disable once InconsistentNaming
        public string SDKVersion { get; set; }
    }

    public class MakeAndModel
    {
        public string Make { get; set; }
        public string[] Models { get; set; }
    }

}
