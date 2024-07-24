// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// ReSharper disable RedundantTypeDeclarationBody
namespace Friflo.Editor;

public abstract class EditorCommand { }

public sealed class CopyToClipboardCommand : EditorCommand { }