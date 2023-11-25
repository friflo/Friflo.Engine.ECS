// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Friflo.Fliox.Editor.UI.Inspector;

internal class InspectorControlModel : INotifyPropertyChanged
{
    private int     tagCount;
    private int     componentCount;
    private int     scriptCount;
    
    internal int TagCount       { get => tagCount;          set { tagCount          = value; OnPropertyChanged(); } }
    internal int ComponentCount { get => componentCount;    set { componentCount    = value; OnPropertyChanged(); } }
    internal int ScriptCount    { get => scriptCount;       set { scriptCount       = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

