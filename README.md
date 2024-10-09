# Panopto Cloud Essentials Plugin (c) 2021

## License

Provided under MIT license

## Overview

## Compatibility

## Cloning Instructions

After forking this repository into your own GitHub space, you can create a new repository using this one as the template. Then you must install the necessary dependencies as indicated below.

## Dependencies

The [Essentials](https://github.com/PepperDash/Essentials) libraries are required. They referenced via nuget. You must have nuget.exe installed and in the `PATH` environment variable to use the following command. Nuget.exe is available at [nuget.org](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe).

### Installing Dependencies

To install dependencies once nuget.exe is installed, run the following command from the root directory of your repository:
`nuget install .\packages.config -OutputDirectory .\packages -excludeVersion`.
To verify that the packages installed correctly, open the plugin solution in your repo and make sure that all references are found, then try and build it.

## Feature Notes

> **Important to note this version of the plugin currently implements both eiscApi and EiscApiAdvanced as valid bridge types**

```javascript
"type": "eiscApiAdvanced"
```

## Installation

This plugin is provided as a published [nuget package](https://www.nuget.org/packages/PepperDash.Essentials.Plugin.PanoptoCloudEpi) for your convenience.

Place the **\*.cplz** file in the /users/programXX/plugins folder, and restart your program.

---

## Controls and Configs

### Config Notes

> This configuration matches a standard essentials device configuration at the base level, with only the type being different. This may have the type **`panopto`**, or **`panoptocloud`**.

```javascript

        "key": "recorder",
        "uid": 6,
        "name": "recorder",
        "type": "PanoptoCloud",
        "group": "recorders",
        "properties": {
          "url": "",
          "username": "",
          "password": "",
          "clientId": "",
          "clientSecret": ""
        }
      }
```
<!-- START Interfaces Implemented -->
### Interfaces Implemented

- ICommunicationMonitor
<!-- END Interfaces Implemented -->
<!-- START Base Classes -->
### Base Classes

- JoinMapBaseAdvanced
- StatusMonitorBase
- ReconfigurableBridgableDevice
<!-- END Base Classes -->
<!-- START Supported Types -->
### Supported Types

- panopto
- panoptocloud
<!-- END Supported Types -->
<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 1.12.8
<!-- END Minimum Essentials Framework Versions -->
<!-- START Public Methods -->
### Public Methods

- public void SetOnlineStatus(bool isOnline)
- public void UpdateTimers()
- public void SetClientId(string clientId)
- public void SetClientSecret(string clientSecret)
- public bool CheckTokenAndUpdate()
- public bool UpdateToken()
- public void SetDeviceName(string name)
- public void IncrementDefaultLength(ushort inc)
- public void DecrementDefaultLength(ushort dec)
- public void SetDefaultLength(ushort value)
- public bool PollRecorder()
- public void StartRecording()
- public void StopRecording()
- public void PauseRecording()
- public void ResumeRecording()
- public void ExtendRecording()
- public void ExtendRecording(int minutes)
- public void PollCurrentRecording()
- public void ProcessCurrentRecording(HttpsClientResponse response)
- public RecoderInfo GetRecorder(string name, string url, string token)
<!-- END Public Methods -->
<!-- START Join Maps -->
### Join Maps

#### Digitals

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Recorder Online |
| 2 | R | Start Recording |
| 3 | R | Stop Recording |
| 4 | R | Pause Recording |
| 5 | R | Resume Recording |
| 6 | R | Extend Recording |
| 11 | R | Increment Length |
| 12 | R | Decrement Length |
| 5 | R | Recording Is Paused |
| 6 | R | Recording Is In Progress |
| 20 | R | Next Recording Exists |

#### Analogs

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Recorder Status |
| 11 | R | Default Recording Length |

#### Serials

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Recorder Name |
| 2 | R | Recorder Status |
| 11 | R | CurrentRecordingId |
| 12 | R | Recorder Name |
| 13 | R | CurrentRecordingStartTime |
| 14 | R | CurrentRecordingEndTime |
| 15 | R | CurrentRecordingLength |
| 16 | R | CurrentRecordingMinutesRemaining |
| 21 | R | NextRecordingId |
| 22 | R | Recorder Name |
| 23 | R | NextRecordingStartTime |
| 24 | R | NextRecordingEndTime |
| 25 | R | NextRecordingLength |
| 26 | R | NextRecordingMinutesRemaining |
<!-- END Join Maps -->
