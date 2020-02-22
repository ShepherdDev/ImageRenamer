using System.Windows;

using ImageRenamer.Common;

namespace ImageRenamer.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Device.Platform = new WpfPlatform();
        }
    }
}
