// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Json.Burst;
using Friflo.Json.Fliox;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public static class JsonUtils
{
    private static readonly     Bytes   Indent          = new Bytes("    ");
    
    public static void FormatComponents(in JsonValue components, ref Bytes componentBuf)
    {
        componentBuf.Clear();
        var span    = components.AsReadOnlySpan();
        var start   = 0;
        int n       = 0;
        for (; n < span.Length; n++) {
            if (span[n] != '\n') {
                continue;
            }
            var line = span.Slice(start, n - start + 1);
            componentBuf.AppendBytesSpan(line);
            componentBuf.AppendBytes(Indent);
            start = n + 1;
        }
        var lastLine = span.Slice(start, span.Length - start);
        componentBuf.AppendBytesSpan(lastLine);
    }
}