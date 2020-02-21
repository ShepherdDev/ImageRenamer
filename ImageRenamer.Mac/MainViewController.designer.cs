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
	[Register ("MainViewController")]
	partial class MainViewController
	{
		[Outlet]
		AppKit.NSTableView fileListView { get; set; }

		[Outlet]
		AppKit.NSTableView metadataListView { get; set; }

		[Outlet]
		AppKit.NSImageView previewImageView { get; set; }

		[Action ("filenameTemplateChanged:")]
		partial void filenameTemplateChanged (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (fileListView != null) {
				fileListView.Dispose ();
				fileListView = null;
			}

			if (metadataListView != null) {
				metadataListView.Dispose ();
				metadataListView = null;
			}

			if (previewImageView != null) {
				previewImageView.Dispose ();
				previewImageView = null;
			}
		}
	}
}
