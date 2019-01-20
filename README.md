# Griffin+ Common Library

[![Build (master)](https://img.shields.io/appveyor/ci/ravenpride/dotnet-libs-common/master.svg?logo=appveyor)](https://ci.appveyor.com/project/ravenpride/dotnet-libs-common/branch/master)
[![Tests (master)](https://img.shields.io/appveyor/tests/ravenpride/dotnet-libs-common/master.svg?logo=appveyor)](https://ci.appveyor.com/project/ravenpride/dotnet-libs-common/branch/master/tests)
[![NuGet Version](https://img.shields.io/nuget/v/GriffinPlus.Lib.Common.svg)](https://www.nuget.org/packages/GriffinPlus.Lib.Common)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GriffinPlus.Lib.Common.svg)](https://www.nuget.org/packages/GriffinPlus.Lib.Common)

## Overview

The *Griffin+ Common Library* contains very basic functionality that is frequently used in other Griffin+ projects.
The library follows semantic versioning, so it should be safe to use higher versions of the library, as long as the major version number does not change.

It contains the following functionality (by class name):

- `BitMask`: A bit mask with variable length that supports all relevant comparisons and logical operations
- `RegexHelpers`: Helper methods providing common functionality when pattern matching is required.
- `MonitorSynchronizedEnumerator`: An enumerator that keeps a monitor locked until it is disposed.

## Supported Platforms

The library is entirely written in C# using .NET Standard 2.0.

Therefore it should work on the following platforms (or higher):
- .NET Framework 4.6.1
- .NET Core 2.0
- Mono 5.4
- Xamarin iOS 10.14
- Xamarin Mac 3.8
- Xamarin Android 8.0
- Universal Windows Platform (UWP) 10.0.16299

