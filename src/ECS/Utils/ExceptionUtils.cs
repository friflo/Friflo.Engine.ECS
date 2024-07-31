// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;

namespace Friflo.Engine.ECS.Utils;

internal static class ExceptionUtils
{
    /// <summary>
    /// Replace calls of <see cref="System.ArgumentException(string, string)"/> by this method.
    /// </summary>
    internal static ArgumentException ArgumentException(string message, string parameterName) {
        // required as Unity format exception message is different from CLR
        return new ArgumentException($"{message} (Parameter '{parameterName}')");
    }
    
    /// <summary>
    /// Replace calls of <see cref="System.ArgumentNullException(string, string)"/> by this method.
    /// </summary>
    internal static ArgumentException ArgumentNullException(string message, string parameterName) {
        // required as Unity format exception message is different from CLR
        return new ArgumentException($"{message} (Parameter '{parameterName}')");
    }
}