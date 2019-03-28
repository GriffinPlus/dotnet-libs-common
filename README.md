# Griffin+ Common Library

[![Build (master)](https://img.shields.io/appveyor/ci/ravenpride/dotnet-libs-common/master.svg?logo=appveyor)](https://ci.appveyor.com/project/ravenpride/dotnet-libs-common/branch/master)
[![Tests (master)](https://img.shields.io/appveyor/tests/ravenpride/dotnet-libs-common/master.svg?logo=appveyor)](https://ci.appveyor.com/project/ravenpride/dotnet-libs-common/branch/master/tests)
[![NuGet Version](https://img.shields.io/nuget/v/GriffinPlus.Lib.Common.svg)](https://www.nuget.org/packages/GriffinPlus.Lib.Common)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GriffinPlus.Lib.Common.svg)](https://www.nuget.org/packages/GriffinPlus.Lib.Common)

## Overview

The *Griffin+ Common Library* contains very basic functionality that is frequently used in other Griffin+ projects.
The library follows semantic versioning, so it should be safe to use higher versions of the library, as long as the major version number does not change.

It contains the following functionality (organized by namespace):

#### Namespace: GriffinPlus.Lib

This namespace provides common functionality and contains the following classes:

- `BitMask`: A bit mask with variable length that supports all relevant comparisons and logical operations.
- `IdentityComparer`: An equality comparer that uses `System.Object.ReferenceEquals()` to check two objects for equality.
- `RegexHelpers`: Helper methods providing common functionality when pattern matching is required.

Extension methods for common types

| Type              | Methods
| :---------------- | ----------------------------------------------------------------------------------------------------------- |
| `System.Byte`     | `Equals`: Equality comparison with tolerance.
| `System.Byte[]`   | `Equals`: Equality comparison
|                   | `ToHexString`: Conversion to a hexadecimal string.
|                   | `ToRfc4122Guid`: Conversion to a RFC 4122 compliant GUID.
|                   | `Swap2`, `Swap4`: Helpers to swap bytes in array.
| `System.DateTime` | `Truncate`: Reduces the precision of a date/time.
| `System.Double`   | `Equals`: Equality comparison with tolerance.
| `System.Enum`     | `ToSeparateFlags`: Converts flags in a flag enumeration to an array of separate enumeration values.
| `System.Guid`     | `ToUuidByteArray`: Conversion to a byte array containing a RFC 4122 compliant GUID.
| `System.Int16`    | `Equals`: Equality comparison with tolerance.
| `System.Int32`    | `Equals`: Equality comparison with tolerance.
| `System.Int64`    | `Equals`: Equality comparison with tolerance.
| `System.Single`   | `Equals`: Equality comparison with tolerance.
| `System.UInt16`   | `Equals`: Equality comparison with tolerance.
| `System.UInt32`   | `Equals`: Equality comparison with tolerance.
| `System.UInt64`   | `Equals`: Equality comparison with tolerance.
| `System.SByte`    | `Equals`: Equality comparison with tolerance.
| `System.String`   | `HexToByteArray`: Parses the hexadecimal encoded byte array. 
| `System.Type`     | `GetPublicMethods`: Gets public methods of a type (supports extended interfaces).
|                   | `GetPublicProperties` Gets public properties of a type (supports extended interfaces).
|                   | `IsSubclassOfRawGeneric`: Checks whether a type is an instance of a certain generic type definition.

#### Namespace: GriffinPlus.Lib.Conversion

The `Converters` class provides converters for converting objects of the following types to a string and vice versa:

- `System.Byte`
- `System.Byte[]`
- `System.Boolean`
- `System.DateTime`
- `System.Decimal`
- `System.Double`
- `System.Enum`
- `System.Guid`
- `System.Int16`
- `System.Int32`
- `System.Int64`
- `System.SByte`
- `System.Single`
- `System.String`
- `System.TimeSpan`
- `System.UInt16`
- `System.UInt32`
- `System.UInt64`
- `System.Net.IPAddress`

Custom converters can be used by implementing the `IConverter` interface and registering the converter using the `Converters.RegisterGlobalConverter()` method.

#### Namespace: GriffinPlus.Lib.Events

This namespace contains classes concerning event handling. The event manager classes ease working with events that should be fired in the context of the thread registering an event handler. The `EventManager<EventArgs>` class supports events of the type `System.EventHandler<EventArgs>`, while the `PropertyChangedEventManager` covers firing the `INotifyPropertyChanged.PropertyChanged` event (comes in handy when working with WPF view models). The `WeakEventManager<EventArgs>` class works just as the `EventManager<EventArgs>` class, but holds weak references to registered event recipients. This comes with some administrative overhead, but avoids keeping objects alive that are otherwise not referenced any more. This is very useful when implementing static events.

#### Namespace: GriffinPlus.Lib.Io

This namespace provides everything concerning generic i/o operations and contains the following classes:

- `ChainableMemoryBlock`: A buffer that can be linked with other buffers (building block of the `MemoryBlockStream` class).
- `MemoryBlockStream`: A stream that grows on demand by maintaining a linked list of buffers.

#### Namespace: GriffinPlus.Lib.Threading

This namespace provides threading specific functionality and contains the following classes:

- `AsyncContext`: Async/await capable synchronization context (derived from Stephen Cleary's work)
- `AsyncContextThread`: Async/await capable worker thread (derived from Stephen Cleary's work)
- `LocklessStack<T>`: A thread-safe stack implementation using interlocked operations only.
- `MonitorSynchronizedEnumerator<T>`: An enumerator that keeps a monitor locked until it is disposed.

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
