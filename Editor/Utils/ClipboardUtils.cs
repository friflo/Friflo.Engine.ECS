// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox;

namespace Friflo.Fliox.Editor.Utils;

internal static class ClipboardUtils
{
    /// <remarks> Enable GC in caller to collect returned clipboard text if large. </remarks>
    private static async Task<JsonValue> GetJsonText(Visual visual)
    {
        var text = await EditorUtils.GetClipboardText(visual);
        if (text == null) {
            return default;
        }
        return new JsonValue(Encoding.UTF8.GetBytes(text));
    }
    
    /// <remarks> Enable GC in caller to collect returned JsonValue if large </remarks>
    internal static async Task<List<DataEntity>> GetDataEntities(Visual visual) {
        var jsonText = await GetJsonText(visual);
        if (jsonText.IsNull()) {
            return null;
        }
        var result      = new List<DataEntity>();
        ECSUtils.JsonArrayToDataEntities (jsonText, result);
        return result;
    }
}