# hypersync

[![Documentation Status]]TBD
[![Translation Status]]TBD
[![Linux Build Status]]Successful
[![macOS Build Status]]TBD
[![Windows Build Status]]Success
[![Coverage Status]]TBD

Folder and file synchronizer.

- [Downloads](https://github.com/Akutra/hypersync-console/releases)
- [Web Site](https://github.com/Akutra/hypersync-console)
- [Documentation]()
- [Bug Reports](https://github.com/Akutra/hypersync-console/issues)
- [Donate]()

## Overview

HyperSync Update folders with only newer files. Easily maintain duplicate or migrated data.

## Features

* Support for Windows and Linux
* Easily specify files and folders as source/destination
* Retain setting in an XML file to reuse later
* Three level of logs
* Exclude files or folders
* Specifiy Damaged source to skip failed/damaged files

## Install

To install Hypersync, copy the binary to you Windows path or bin folder.

### Windows

Place the binary in any folder you desire to access it from such as a path located in your %path% folder.

#### Linux (e.g. Ubuntu)

Plase the binary in any folder you desire to access it from such as /usr/local/bin.

## Using the App

Execute the command "Hypersync v" to verify it is installed. If present it will output the installed version.

## Build from Source Code

To build the application from source code, install Visual Studio (Windows) or MonoDevelop (Linux).

- [Git](https://git-scm.com/)
- Visual Studio or MonoDevelop
- .NET 4.8 (Visual Studio)

Prepare MonoDevelop
- Download your desired target (e.g. mono-6.8.0-ubuntu-16.04-x64)
  ``` mkbundle --fetch-target mono-6.8.0-ubuntu-16.04-x64 ```
- Locate the machine.config file (e.g. ~/.mono/targets/mono-6.8.0-ubuntu-16.04-x64/etc/mono/4.5/machine.config)
- Modify the machine.config file remove the references to "$mono_libdir/" in front of all libmono-native.so
- Locate libmono-native.so (e.g. /usr/lib/libmono-native.so) because you will need the path for mkbundle.


### Build the App on Linux

```bash
cd ~/mono
git clone https://github.com/Akutra/hypersync-console.git
cd hypersync-console/hypersync
msbuild hypersync.csproj -p:Platform=x64,Configuration=Release
mkbundle -o hypersync --cross mono-6.8.0-ubuntu-16.04-x64 bin/Win/x64/Release/Hypersync.exe --machine-config /etc/mono/4.5/machine.config --library /usr/lib/libmono-native.so
```

You can now run the built app.

```bash
./hypersync v
```
### Build the App on Windows

Locate your repos directory (e.g. C:\Users\<user>\source\repos\)
```
cd C:\Users\<user>\source\repos
git clone https://github.com/Akutra/hypersync-console.git
```
Open the csproj file in Visual Studio from the repo (e.g. C:\Users\<user>\source\repos\hypersync-console\hypersync\hypersync.csproj
Compile for x64 Release.
Place the resulting exe file in a folder referenced by the %path% variable

You can now run the built app.

```bash
hypersync v
```

## Contributions

To Be Determined.
