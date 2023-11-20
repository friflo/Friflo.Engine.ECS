// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Friflo.Fliox.Editor.UI.Panels;
using Friflo.Fliox.Engine.Client;
using Friflo.Fliox.Engine.ECS;
using Friflo.Json.Fliox.Hub.Client;
using Friflo.Json.Fliox.Hub.Explorer;
using Friflo.Json.Fliox.Hub.Host;
using Friflo.Json.Fliox.Hub.Host.Event;
using Friflo.Json.Fliox.Hub.Remote;

// Note! Must not using Avalonia namespaces

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
namespace Friflo.Fliox.Editor;

public abstract class EditorCommand { }

public class CopyToClipboardCommand : EditorCommand { }


public partial class Editor
{
    public bool ExecuteCommand(EditorCommand command)
    {
        if (activePanel == null) {
            return false;
        }
        if (activePanel.OnExecuteCommand(command)) {
            return true;
        }
        switch (command) {
            case CopyToClipboardCommand:
                EditorUtils.CopyToClipboard(activePanel, "");
                break;
        }
        return false;
    }
}