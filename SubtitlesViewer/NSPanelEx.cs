using System;
using AppKit;
using CoreGraphics;

namespace SubtitlesViewer
{
    public class NSPanelEx : NSPanel
    {
        public NSPanelEx(CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation) : base(contentRect, aStyle, bufferingType, deferCreation)
        {
        }

        public override void KeyDown(NSEvent theEvent)
        {
            base.KeyDown(theEvent);
        }
    }
}
