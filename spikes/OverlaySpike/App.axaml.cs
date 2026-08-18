using Avalonia.Markup.Xaml;

namespace OverlaySpike;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
