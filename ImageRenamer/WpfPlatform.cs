using System;

using ImageRenamer.Common;

namespace ImageRenamer
{
    public class WpfPlatform : IPlatform
    {
        public bool IsMainThread => System.Windows.Application.Current.Dispatcher.CheckAccess();

        public void BeginInvokeOnMainThread( Action action )
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke( action );
        }
    }
}
