///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib;

/// <summary>
/// Indicates that the annotated type is immutable.
/// The attribute is evaluated by the <see cref="Immutability"/> class when determining whether a type is immutable.
/// The attribute is not inherited, so derived classes must be declared immutable as well, if appropriate.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class ImmutableAttribute : Attribute;
