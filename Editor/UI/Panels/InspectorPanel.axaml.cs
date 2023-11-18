using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Friflo.Fliox.Editor.UI.Panels;

public partial class InspectorPanel : UserControl
{
    public InspectorPanel()
    {
        InitializeComponent();
    }

    // ReSharper disable once RedundantOverriddenMember
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
    }
}