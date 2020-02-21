using Foundation;

namespace ImageRenamer.Mac
{
    [Register( "AppDelegate" )]
    public partial class AppDelegate : NSObject
    {
        public AppDelegate()
        {
            Common.Device.Platform = new MacPlatform();
        }
    }
}
