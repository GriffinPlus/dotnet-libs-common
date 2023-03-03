# Griffin+ Common Library

[![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/griffinplus/2f589a5e-e2ab-4c08-bee5-5356db2b2aeb/27/master?label=Build)](https://dev.azure.com/griffinplus/DotNET%20Libraries/_build/latest?definitionId=27&branchName=master)
[![Tests (master)](https://img.shields.io/azure-devops/tests/griffinplus/DotNET%20Libraries/27/master?label=Tests)](https://dev.azure.com/griffinplus/DotNET%20Libraries/_build/latest?definitionId=27&branchName=master)
[![NuGet Version](https://img.shields.io/nuget/v/GriffinPlus.Lib.Common.svg?label=NuGet%20Version)](https://www.nuget.org/packages/GriffinPlus.Lib.Common)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GriffinPlus.Lib.Common.svg?label=NuGet%20Downloads)](https://www.nuget.org/packages/GriffinPlus.Lib.Common)

## Overview

The *Griffin+ Common Library* contains very basic functionality that is frequently used in other Griffin+ projects.
The library follows semantic versioning, so it should be safe to use higher versions of the library, as long as the major version number does not change.

It contains the following functionality (organized by namespace):

#### Namespace: GriffinPlus.Lib

This namespace provides common functionality and contains the following classes:

- Comparers (`IComparer<T>`)
  - `ArrayComparer<T>`: An comparer for arrays of items that implement the `IComparable<T>` interface.
  - `ReadOnlyListComparer<T>`: An comparer for collections implementing the `IReadOnlyList<T>` interface containing items implementing the `IComparable<T>` interface.
- `DataSize`: Size of some piece of data (with support for calculations and formatting using binary/metric units and auto-scaling).
- Equality Comparers (`IEqualityComparer<T>`)
  - `ByteArrayEqualityComparer`: An equality comparer for byte arrays (with span support).
  - `IdentityComparer<T>`: An equality comparer that uses `System.Object.ReferenceEquals()` to check two objects for equality.
  - `KeyValuePairEqualityComparer<TKey,TValue>`: An equality comparer for `KeyValuePair<TKey,TValue>` delegating comparison to specific comparers for key and value.
  - `ReadOnlyListEqualityComparer<T>`: An equality comparer for collections implementing the `IReadOnlyList<T>` interface.
- `BitMask`: A bit mask with variable length that supports all relevant comparisons and logical operations.
- `EndiannessHelper`: Utility class that assists with swapping the byte order in value types to convert little endian to big endian and vice versa.
- `Immutability`: Utility class that assists with determining whether a type is immutable. It analyses types on its own, but supports overriding by annotating types with the `[Immutable]` attribute. Alternatively types can be declared immutable using `AddImmutableType<T>()`.
- `NativeBuffer`: A native buffer (with support for aligned allocations).
- `ObjectPool<T>`: A simple thread-safe implementation of an object pool that allows re-using objects.
- `RegexHelpers`: Helper methods providing common functionality when pattern matching is required.
- `TypeDecomposer`: Utility class that assists with decomposing types to generic type definitions and non-generic types.
- `RuntimeMetadata`: Utility class that assists with loading assemblies and provides various metadata sets (assemblies by their full name, types by assembly (all and public-only), types by their full name (all and public-only).

Extension methods for common types

| Type               | Methods
| :----------------- | ----------------------------------------------------------------------------------------------------------- |
| `System.Byte`      | `Equals`: Equality comparison with tolerance.
| `System.Byte[]`    | `Equals`: Equality comparison
|                    | `ToHexString`: Conversion to a hexadecimal string.
|                    | `ToRfc4122Guid`: Conversion to a RFC 4122 compliant GUID.
|                    | `Swap2`, `Swap4`: Helpers to swap bytes in array.
| `System.DateTime`  | `Truncate`: Reduces the precision of a date/time.
| `System.Double`    | `Equals`: Equality comparison with tolerance.
| `System.Enum`      | `ToSeparateFlags`: Converts flags in a flag enumeration to an array of separate enumeration values.
| `System.Exception` | `GetAllMessages`: Collects messages of inner exceptions and aggregate exceptions.
| `System.Guid`      | `ToUuidByteArray`: Conversion to a byte array containing a RFC 4122 compliant GUID.
| `System.Int16`     | `Equals`: Equality comparison with tolerance.
| `System.Int32`     | `Equals`: Equality comparison with tolerance.
| `System.Int64`     | `Equals`: Equality comparison with tolerance.
| `System.Single`    | `Equals`: Equality comparison with tolerance.
| `System.UInt16`    | `Equals`: Equality comparison with tolerance.
| `System.UInt32`    | `Equals`: Equality comparison with tolerance.
| `System.UInt64`    | `Equals`: Equality comparison with tolerance.
| `System.SByte`     | `Equals`: Equality comparison with tolerance.
| `System.String`    | `HexToByteArray`: Parses the hexadecimal encoded byte array. 
| `System.Type`      | `GetPublicMethods`: Gets public methods of a type (supports extended interfaces).
|                    | `GetPublicProperties` : Gets public properties of a type (supports extended interfaces).
|                    | `IsImmutable` : Checks whether a type is immutable.
|                    | `IsSubclassOfRawGeneric`: Checks whether a type is an instance of a certain generic type definition.
|                    | `Decompose`: Decomposes a type to generic type definitions and non-generic types.

#### Namespace: GriffinPlus.Lib.Caching

This namespace provides some utilities when it comes to caching objects. `IObjectCache<T>` declares the interface of an object cache that is able to swap objects out of memory to save space and reload them on demand. The library only contains a dummy implementation (`DummyObjectCache`) allowing the cache to be used without any real backing store. Real world implementations usually use some serialization mechanism to stream objects to disk to restore them later on. The cache can also be used in conjunctions with collections of objects that can be swapped out (see `GriffinPlus.Lib.Collections.ObjectCacheCollection<T>`).

#### Namespace: GriffinPlus.Lib.Collections

This namespace provides common collections and contains the following classes:

- `ByteSequenceKeyedDictionary<TValue>`: A generic dictionary that uses a byte sequence as key (with span support).
- `Deque<T>`: A double-ended queue that supports adding/removing items at both ends efficiently.
- `FixedItemReadOnlyList<T>`: A read-only list that provides a certain object a specific number of times.
- `IdentityKeyedDictionary<TKey,TValue>`: A generic dictionary that uses an object's reference as key (for reference types only).
- `ObjectCacheCollection<T>`: A collection that uses an `IObjectCache` to swap objects out of memory to save space and reload them on demand.
- `PartialList<T>`: A read-only list wrapping a consecutive subset of items in a collection implementing the `System.Collections.Generic.IList<T>` interface.
- `PartialList`: A read-only list wrapping a consecutive subset of items in a collection implementing the `System.Collections.IList` interface.
- `TypeKeyedDictionary<TValue>`: A generic dictionary that is optimized for `System.Type` as key.

#### Namespace: GriffinPlus.Lib.Configuration

This namespace provides a configuration subsystem with hierarchical configurations that are cascadable, i.e. multiple configurations can be stacked on another. This allows to create a composite configuration that automatically merges settings from different sources into a single configuration. Configurations at a higher level override settings derived from configurations at a lower level. The configuration subsystem comes with built-in support for persisting settings to XML. Custom persistence strategies can be added as well. Details can be found [here](./docs/configuration.md).

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

Custom converters can be created in one of the following ways:

- Implementation of the `IConverter` interface for a maximum flexibility
- Deriving from the `ConverterBase<T>` class for complex converters
- Using the `Converter<T>` class with callbacks for simple converters

These converters can then be globally registered using the `Converters.RegisterGlobalConverter()` method.

#### Namespace: GriffinPlus.Lib.Cryptography

This namespace provides everything concerning cryptography operations and contains the following classes:

- `SecurePasswordHasher`: Utility class for hashing and verifying passwords (supports SHA-1, SHA-256, SHA-384, SHA-512 and PBKDF2 (RFC8018, formerly RFC2898) with SHA-1, SHA-256 and SHA-512 as key derivation functions)

#### Namespace: GriffinPlus.Lib.Events

This namespace contains classes concerning event handling. The event manager classes ease working with events that should be fired in the context of the thread registering an event handler. The `EventManager<EventArgs>` class supports events of the type `System.EventHandler<EventArgs>`, while the `PropertyChangedEventManager` covers firing the `INotifyPropertyChanged.PropertyChanged` event (comes in handy when working with WPF view models). The `WeakEventManager<EventArgs>` class works just as the `EventManager<EventArgs>` class, but holds weak references to registered event recipients. This comes with some administrative overhead, but avoids keeping objects alive that are otherwise not referenced any more. This is very useful when implementing static events.

#### Namespace: GriffinPlus.Lib.Imaging

This namespace provides everything that deals with images and related things like colors and bitmap palettes. All classes have been tailored to integrate with the *Windows Presentation Foundation* without binding to it. This allows to use images in cross-platform capable libraries. The namespace contains the following classes:

- `BitmapPalette`: A color palette that can be used in conjunction with the `NativeBitmap` to create a paletted bitmap.
- `BitmapPalettes`: A predefined set of common color palettes.
- `Color`: A color (supports the sRGB and scRGB color format, no support for ICC profiles).
- `Colors`: A predefined set of common colors.
- `NativeBitmap`: An image backed by a native buffer (supports explicit disposal easing memory management).
- `PixelFormat`: The format of a pixel in a `NativeBitmap`.
- `PixelFormats`: The set of supported pixel formats.

#### Namespace: GriffinPlus.Lib.Io

This namespace provides everything concerning generic i/o operations and contains the following classes:

- `ChainableMemoryBlock`: A buffer that can be linked with other buffers (can be allocated on the heap or rented from an `ArrayPool<byte>`).
- `MemoryBlockStream`: A stream that grows on demand by maintaining a linked list of `ChainableMemoryBlock` buffers. Optionally synchronized for use in multi-threaded scenarios.

#### Namespace: GriffinPlus.Lib.Threading

This namespace provides threading specific functionality and contains the following classes:

- Asynchronous Primitives (primarily derived from Stephen Cleary's [AsyncEx](https://github.com/stephencleary/AsyncEx) library)
  - Context
    - `AsyncContext`: Async/await capable synchronization context.
    - `AsyncContextThread`: Async/await capable worker thread.
  - Coordination
    - `AsyncAutoResetEvent`: An async/await capable auto-reset event.
    - `AsyncConditionVariable`: An async/await capable condition variable.
    - `AsyncCountdownEvent`: An async/await capable event that fires when signaled a specific number of times.
    - `AsyncLazy`: Async/await capable lazy initialization.
    - `AsyncLock`: Async/await capable equivalent of a `lock()` statement.
    - `AsyncManualResetEvent`: An async/await capable manual-reset event.
    - `AsyncMonitor`: An async/await capable monitor implementation.
    - `AsyncProducerConsumerQueue`: An async/await capable producer/consumer queue.
    - `AsyncReaderWriterLock`: An async/await capable reader-writer-lock.
    - `AsyncSemaphore`: An async/await capable semaphore.
    - `PauseToken`: A token source to pause/unpause asynchronous operations.
    - `SerialTaskQueue`: A queue that ensures that synchronous/asynchronous actions/functions are executed one after the other using the TPL.
- `LocklessStack<T>`: A thread-safe stack implementation using interlocked operations only.
- `MonitorSynchronizedEnumerator<T>`: An enumerator that keeps a monitor locked until it is disposed.
- `ReaderWriterLockSlimAutoLock`: A helper that locks a `ReaderWriterLockSlim` when created and releases it appropriately when disposed.

## Supported Platforms

The library is entirely written in C# using .NET Standard 2.0.

More specific builds for .NET Standard 2.1, .NET Framework 4.6.1, .NET Framework 4.8, .NET 5.0 and .NET 6.0 minimize dependencies to framework components and provide optimizations for the different frameworks.

Therefore it should work on the following platforms (or higher):
- .NET Framework 4.6.1
- .NET Core 2/3
- .NET 5/6/7
- Mono 5.4
- Xamarin iOS 10.14
- Xamarin Mac 3.8
- Xamarin Android 8.0
- Universal Windows Platform (UWP) 10.0.16299

The library is tested automatically on the following frameworks and operating systems:
- .NET Framework 4.6.1: Tests with library built for .NET Framework 4.6.1 (Windows Server 2022)
- .NET Framework 4.8: Tests with library built for .NET Framework 4.8 (Windows Server 2022)
- .NET Core 2.2: Tests with library built for .NET Standard 2.0 (Windows Server 2022 and Ubuntu 22.04)
- .NET Core 3.1: Tests with library built for .NET Standard 2.1 (Windows Server 2022 and Ubuntu 22.04)
- .NET 5.0: Tests with library built for .NET 5.0 (Windows Server 2022 and Ubuntu 22.04)
- .NET 6.0/7.0: Tests with library built for .NET 5.0 (Windows Server 2022 and Ubuntu 22.04)
