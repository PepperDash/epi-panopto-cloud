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
using Crestron.RAD.Common.Enums;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Http;
using Newtonsoft.Json;

namespace Crestron.RAD.Common
{
    public delegate void SearchCompletedHandler(int numberOfTotalResults, int numberOfResultsCurrentPage, int numberOfPages, int currentPage, DriverData[] driverData);

    public delegate void FileDownloadedHandler(string fileName, DeviceTypes deviceType);

    public delegate void ErrorHandler(DownloadManagerErrorType errorType, string message);

    public static class DownloadManager
    {
        public static Stopwatch StopWatch = new Stopwatch();

        private static DriverInfo _driverInfo;
        private static DriverData[] _driverData;

        public static string DownloadPath { get; set; }
        public static string LocalDatabase { get; set; }
        public static bool UseLocalDataBase { get; set; }

        private static int _internalPageSize = 20;
        public static int PageSize
        {
            get { return _internalPageSize; }
            set { _internalPageSize = value; }
        }

        private const string CloudUrl = "http://dynamicdrivers.crestronfusion.com/api/";

        private static int _lastRequestPageSize;
        private static int _lastRequestPage;

        private static bool _searchingOrDownloading;

        public static void Search(DeviceTypes deviceType, string make, string model, TransportTypes transportType, string sdkVersion, int page)
        {
            if (_searchingOrDownloading)
            {
                return;
            }

            try
            {
                _searchingOrDownloading = true;

                _searchingOrDownloading = true;

                var request = new StringBuilder(UseLocalDataBase ? LocalDatabase : CloudUrl);
                request.Append("Driver/?");

                if (deviceType != DeviceTypes.Unknown)
                {
                    request.Append("DeviceType=");
                    request.Append(deviceType);
                    request.Append("&");
                }

                if (make != string.Empty)
                {
                    request.Append("Make=");
                    request.Append(make);
                    request.Append("&");
                }

                if (model != string.Empty)
                {
                    request.Append("Model=");
                    request.Append(model);
                    request.Append("&");
                }

                if (transportType != TransportTypes.Unknown)
                {
                    request.Append("TransportType=");
                    request.Append(transportType);
                    request.Append("&");
                }

                if (sdkVersion != string.Empty)
                {
                    request.Append("SDKVersion=");
                    request.Append(sdkVersion);
                    request.Append("&");
                }

                request.Append("PageSize=");
                request.Append(PageSize);
                request.Append("&");

                request.Append("Page=");
                request.Append(page);

                _lastRequestPageSize = PageSize;
                _lastRequestPage = page;

                StopWatch.Start();

                using (var client = new HttpClient())
                {
                    client.GetAsync(request.ToString(), SearchComplete);
                }
            }
            catch (Exception e)
            {
                _searchingOrDownloading = false;
                DownloadManagerError(DownloadManagerErrorType.Search, e.ToString());
            }
        }

        public static void Search(string deviceType, string make, string model, string transportType, string sdkVersion, int page)
        {
            if (_searchingOrDownloading)
            {
                return;
            }

            try
            {
                _searchingOrDownloading = true;

                var request = new StringBuilder(UseLocalDataBase ? LocalDatabase : CloudUrl);
                request.Append("Driver/?");

                if (deviceType != string.Empty)
                {
                    request.Append("DeviceType=");
                    request.Append(deviceType);
                    request.Append("&");
                }

                if (make != string.Empty)
                {
                    request.Append("Make=");
                    request.Append(make);
                    request.Append("&");
                }

                if (model != string.Empty)
                {
                    request.Append("Model=");
                    request.Append(model);
                    request.Append("&");
                }

                if (transportType != string.Empty)
                {
                    request.Append("TransportType=");
                    request.Append(transportType);
                    request.Append("&");
                }

                if (sdkVersion != string.Empty)
                {
                    request.Append("SDKVersion=");
                    request.Append(sdkVersion);
                    request.Append("&");
                }

                request.Append("PageSize=");
                request.Append(PageSize);
                request.Append("&");

                request.Append("Page=");
                request.Append(page);

                _lastRequestPageSize = PageSize;
                _lastRequestPage = page;

                StopWatch.Start();
                using (var client = new HttpClient())
                {
                    var response = client.Get(request.ToString());
                    SearchComplete(response, HTTP_CALLBACK_ERROR.COMPLETED);
                }
            }
            catch (Exception e)
            {
                _searchingOrDownloading = false;
                DownloadManagerError(DownloadManagerErrorType.Search, e.ToString());
            }
        }

        private static void SearchComplete(string response, HTTP_CALLBACK_ERROR error)
        {
            StopWatch.Stop();
            CrestronConsole.PrintLine("Search Run Time: " + StopWatch.ElapsedMilliseconds);
            StopWatch.Reset();

            _searchingOrDownloading = false;

            try
            {
                if (error != HTTP_CALLBACK_ERROR.COMPLETED)
                {
                    if (DownloadManagerError != null)
                    {
                        DownloadManagerError(DownloadManagerErrorType.Search, response);
                    }
                    return;
                }

                _driverInfo = JsonConvert.DeserializeObject<DriverInfo>(response);
                ConverToDriverData(_driverInfo.DriverMetadata);

                if (SearchCompleted != null)
                {
                    var numberOfPages = (_driverInfo.Count + _lastRequestPageSize - 1) / _lastRequestPageSize;
                    SearchCompleted(_driverInfo.Count, _driverData.Length, numberOfPages, _lastRequestPage, _driverData);
                }
            }
            catch (Exception)
            {
                if (DownloadManagerError != null)
                {
                    DownloadManagerError(DownloadManagerErrorType.Search, response);
                }
            }
        }

        private static void ConverToDriverData(IList<DriverMetadata> driverMetaData)
        {
            _driverData = new DriverData[driverMetaData.Count];
            for(var i = 0; i<driverMetaData.Count; i++)
            {
                DateTime versionDate;
                try
                {
                    versionDate = DateTime.Parse(driverMetaData[i].VersionDate);
                }
                catch (Exception)
                {
                    versionDate = new DateTime();
                }

                _driverData[i] = new DriverData(driverMetaData[i].DeviceType, driverMetaData[i].Make, driverMetaData[i].Model,
                    driverMetaData[i].TransportType, driverMetaData[i].Url, driverMetaData[i].Filename, driverMetaData[i].Description,
                    versionDate, driverMetaData[i].Version, driverMetaData[i].SDKVersion);
            }
        }

        private static string _fileToBeDownload;
        private static DeviceTypes _deviceTypeToBeDownload;
        public static void Download(int index)
        {
            if (_searchingOrDownloading || _driverData == null || _driverData.Length <= index)
            {
                return;
            }

            try
            {
                _searchingOrDownloading = true;

                using (var client = new HttpClient())
                {
                    client.GetResponseAsync(_driverData[index].Url, WriteFile);
                }
                _fileToBeDownload = _driverData[index].FileName;
                _deviceTypeToBeDownload = _driverData[index].DeviceType;
            }
            catch (Exception e)
            {
                _searchingOrDownloading = false;
                DownloadManagerError(DownloadManagerErrorType.Download, e.ToString());
            }
        }

        private static void WriteFile(HttpClientResponse userobj, HTTP_CALLBACK_ERROR error)
        {
            _searchingOrDownloading = false;
            try
            {
                if (error != HTTP_CALLBACK_ERROR.COMPLETED)
                {
                    if (DownloadManagerError != null)
                    {
                        DownloadManagerError(DownloadManagerErrorType.Download, userobj.ContentString);
                    }
                    return;
                }

                var content = userobj.ContentBytes;

                var fullPath = (DownloadPath != null) ? Path.Combine(DownloadPath, _fileToBeDownload) : _fileToBeDownload;

                //If the file exists append a "-2" before the extension
                while (File.Exists(fullPath))
                {
                    var extensionStart = _fileToBeDownload.IndexOf('.');
                    var newFile = new StringBuilder(_fileToBeDownload.Substring(0, extensionStart));
                    newFile.Append("-2");
                    newFile.Append(_fileToBeDownload.Substring(extensionStart, _fileToBeDownload.Length - extensionStart));
                    _fileToBeDownload = newFile.ToString();
                    fullPath = (DownloadPath != null) ? Path.Combine(DownloadPath, _fileToBeDownload) : _fileToBeDownload;
                }

                var writer = new BinaryWriter(fullPath);
                writer.Write(content);
                writer.Close();

                if (FileDownloaded != null)
                {
                    FileDownloaded(fullPath, _deviceTypeToBeDownload);
                }
            }
            catch (Exception)
            {
                if (DownloadManagerError != null)
                {
                    DownloadManagerError(DownloadManagerErrorType.Download, userobj.ContentString);
                }
            }
        }

        public static List<string> GetAllDeviceTypes()
        {
            var result = new List<string>();
            var request = new StringBuilder(UseLocalDataBase ? LocalDatabase : CloudUrl);
            request.Append("devicetype");

            using (var client = new HttpClient())
            {
                var response = client.Get(request.ToString());
                result = JsonConvert.DeserializeObject<string[]>(response).ToList();
            }
            return result;
        }

        public static List<string> GetAllMakes()
        {
            var result = new List<string>();
            var request = new StringBuilder(UseLocalDataBase ? LocalDatabase : CloudUrl);
            request.Append("make");

            using (var client = new HttpClient())
            {
                var response = client.Get(request.ToString());
                result = JsonConvert.DeserializeObject<string[]>(response).ToList();
            }
            return result;
        }

        public static List<string> GetAllModels()
        {
            var result = new List<string>();
            var request = new StringBuilder(UseLocalDataBase ? LocalDatabase : CloudUrl);
            request.Append("models");

            using (var client = new HttpClient())
            {
                var response = client.Get(request.ToString());
                result = JsonConvert.DeserializeObject<string[]>(response).ToList();
            }
            return result;
        }

        public static List<string> GetAllTransports()
        {
            var result = new List<string>();
            var request = new StringBuilder(UseLocalDataBase ? LocalDatabase : CloudUrl);
            request.Append("transporttype");

            using (var client = new HttpClient())
            {
                var response = client.Get(request.ToString());
                result = JsonConvert.DeserializeObject<string[]>(response).ToList();
            }
            return result;
        }

        public static event SearchCompletedHandler SearchCompleted;
        public static event FileDownloadedHandler FileDownloaded;
        public static event ErrorHandler DownloadManagerError;
    }

    public class DriverData
    {
        public DriverData(string deviceType, string make, string model, string transportType, string url,
            string fileName, string description, DateTime versionDate, string version, string sdkVersion)
        {
            try
            {
                DeviceType = (DeviceTypes)Enum.Parse(typeof(DeviceTypes), deviceType, true);
            }
            catch (Exception)
            {
                DeviceType = DeviceTypes.Unknown;
            }

            try
            {
                TransportType = (TransportTypes)Enum.Parse(typeof(TransportTypes), transportType, true);
            }
            catch (Exception)
            {
                TransportType = TransportTypes.Unknown;
            }
            
            Make = make;
            Model = model;
            
            Url = url;
            FileName = fileName;
            Description = description;
            VersionDate = versionDate;
            Version = version;
            SDKVersion = sdkVersion;
        }

        public DeviceTypes DeviceType { get; private set; }
        public TransportTypes TransportType { get; private set; }
        public string Make { get; private set; }
        public string Model { get; private set; }
        public string Url { get; private set; }

        public string FileName { get; private set; }
        public string Description { get; private set; }
        public DateTime VersionDate { get; private set; }
        public string Version { get; private set; }
// ReSharper disable once InconsistentNaming
        public string SDKVersion { get; private set; }
    }
}
