// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// Note! Must not using Avalonia namespaces

using Friflo.Editor.Utils;

namespace Friflo.Editor;

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