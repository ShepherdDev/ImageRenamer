// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ImageRenamer.Mac
{
	[Register ("ProgressDialogController")]
	partial class ProgressDialogController
	{
		[Outlet]
		AppKit.NSProgressIndicator progressIndicatorView { get; set; }

		[Action ("cancelOperation:")]
		partial void CancelOperation (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (progressIndicatorView != null) {
				progressIndicatorView.Dispose ();
				progressIndicatorView = null;
			}
		}
	}
}
