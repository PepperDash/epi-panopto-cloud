// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

//using Crestron.SimplSharp;

//namespace ThirdPartyCommon.Class
//{
//    public enum DeviceTypes { Display, MediaDevice, CableBox, VideoCodec, AudioCodec, All }
//    public enum TransportTypes { Ir, Comport, Tcp, Telnet, Http, Https, All }
//    public enum DownloadManagerErrorType { Search, Download }

//    public delegate void SearchCompletedHandler(int numberOfTotalResults, int numberOfResultsCurrentPage, int numberOfPages, int currentPage, DriverData[] driverData);
//    public delegate void FileDownloadedHandler(string fileName);
//    public delegate void ErrorHandler(DownloadManagerErrorType errorType, string message);

//    public class DownloadManager
//    {
//        private DriverData _driverData;

//        public string DownloadPath { get; set; }
//        public int PageSize { get; set; }
//        public string LocalDatabse { get; set; }
//        public bool UseLocalDataBase { get; set; }

//        public void Search(DeviceTypes deviceType, string make, string model, TransportTypes transportType, int page)
//        {
//            //var fileTransferClient = new CrestronFileTransferClient();
//        }

//        public void Search(string deviceType, string make, string model, string transportType, int page)
//        {
            
//        }

//        public void Download(string url)
//        {
            
//        }

//        public event SearchCompletedHandler SearchCompleted;
//        public event FileDownloadedHandler FileDownloaded;
//    }

//    public class DriverData
//    {
//        public DriverData(DeviceTypes deviceType, string make, string model, TransportTypes transportType)
//        {
//            Model = model;
//            Make = make;
//            TransportType = transportType;
//            DeviceType = deviceType;
//        }

//        public DeviceTypes DeviceType { get; private set; }
//        public TransportTypes TransportType { get; private set; }
//        public string Make { get; private set; }
//        public string Model { get; private set; }
//        public string Url { get; set; }
//    }
//}
