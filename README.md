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
{
    "key": "remote-recorder",
    "name": "Panopto Remote Recorder",
    "type": "panopto",
    "properties": {
    }
}
```
