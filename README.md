# PlaneDisaster.NET

[![Build](https://github.com/zippy1981/plane-disaster/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/zippy1981/plane-disaster/actions/workflows/build.yml?query=branch%3Amaster)
[![Lint Markdown](https://github.com/zippy1981/plane-disaster/actions/workflows/markdown-lint.yml/badge.svg?branch=master)](https://github.com/zippy1981/plane-disaster/actions/workflows/markdown-lint.yml?query=branch%3Amaster)
[![Release](https://github.com/zippy1981/plane-disaster/actions/workflows/release.yml/badge.svg)](https://github.com/zippy1981/plane-disaster/actions/workflows/release.yml)
[![Latest release](https://img.shields.io/github/v/release/zippy1981/plane-disaster)](https://github.com/zippy1981/plane-disaster/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/zippy1981/plane-disaster/total)](https://github.com/zippy1981/plane-disaster/releases)
[![License: GPL v2](https://img.shields.io/badge/license-GPL--2.0-blue)](gpl.txt)

PlaneDisaster.NET is a .NET database front end for JetSQL (the Microsoft Access file format) and SQLite. Besides browsing and querying, it can create new Access and SQLite files, and compact and repair Access databases.

![A table with pictures](WebSite/screenshots/table_with_pictures.png)

## Download

Get the MSI installer or a portable zip, for x86 or x64, from the [latest release](https://github.com/zippy1981/plane-disaster/releases/latest). The x64 build needs the Access Database Engine to open Access files (see [Building](#building)).

## What's in the solution

| Project | What it is |
| --- | --- |
| `PlaneDisaster` | The Windows Forms application |
| `PlaneDisaster.Dba` | Data access for Access (OleDb and ODBC) and SQLite |
| `Jet2SQLite` | A tool that copies an Access database into SQLite |
| `jsql_admin` | A command line tool that creates an Access database (`-c -d file.mdb`) and runs a SQL script against it (`-s script.sql`) |
| `UnitTests` | NUnit 2.6 tests |
| `PlaneDisaster.NET Setup` | The WiX 5 installer (`dotnet build "PlaneDisaster.NET Setup" -c Release -p:Platform=x64`) |

## Building

The projects are SDK-style and target .NET Framework 2.0 (what ships), .NET Framework 4.8.1 and .NET 11, for x86 and x64. CI builds them on `windows-latest`; see [`.github/workflows/build.yml`](.github/workflows/build.yml) for the exact commands. You need the .NET 11 SDK, plus Visual Studio or its Build Tools for the .NET Framework builds:

```powershell
dotnet restore PlaneDisaster.NET.sln -p:Platform=x64
msbuild PlaneDisaster.NET.sln -p:Configuration=Release -p:Platform=x64 -p:TargetFramework=net20
dotnet build PlaneDisaster.NET.sln -c Release -p:Platform=x64 -p:TargetFramework=net11.0-windows
```

No .NET Framework 3.5 install is needed: the SDK downloads the 2.0 reference assemblies.

Opening Access files needs an Access driver. 32 bit builds use Jet 4.0, which ships with Windows. 64 bit builds need the [Microsoft Access Database Engine 2016 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?id=54920).

## License

GPL version 2. See [gpl.txt](gpl.txt).
