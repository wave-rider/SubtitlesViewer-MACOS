using System;

using AppKit;
using CoreGraphics;
using Foundation;
using SubtitlesPlayer;

namespace SubtitlesViewer
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
            }
        }

        CGRect screenResolution()
        {

            CGRect screenRect = new CGRect(0,0,0,0);
            NSScreen[] screenArray = NSScreen.Screens;
            int screenCount = screenArray.Length;

            for (int index=0; index<screenCount; index++)
            {
                NSScreen screen = screenArray[index];
                screenRect = screen.VisibleFrame;
            }

            return screenRect;
        }

        NSButton subtitleTextButton = null;
        private NSButton forwardButton;
        private SubtitleFileSelector subtitleFileSelector = new SubtitleFileSelector();
        private SubtitlesProvider subtitlesProvider = new SubtitlesProvider();
        private string fileName;
        NSPanel subtitlesPanel;
        private NSTextField subtitleTextField;
        private NSButton backButton;
        private NSButton startStopButton;

        public override void ViewWillAppear()
        {
            base.ViewWillAppear();
            SetupView();
        }

        private void SetupView()
        { 
            var screenRes = screenResolution();
            int PANEL_HIGHT = 200;
            subtitlesPanel = new NSPanel
            (
                new CoreGraphics.CGRect(40, 50, screenRes.Width - 80, PANEL_HIGHT),
                NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Miniaturizable | NSWindowStyle.DocModal,
                NSBackingStore.Buffered, true
            )
            {
                BackgroundColor = NSColor.FromCalibratedRgba(0, 0, 0, 0.0f),
                ReleasedWhenClosed = true,
                HidesOnDeactivate = false,
                FloatingPanel = true,
                StyleMask = NSWindowStyle.NonactivatingPanel,
                Level = NSWindowLevel.MainMenu - 1,
                IsMovable = true,
                CollectionBehavior = NSWindowCollectionBehavior.CanJoinAllSpaces |
                NSWindowCollectionBehavior.FullScreenAuxiliary
            };

            subtitlesPanel.OrderFront(null);

            subtitleTextButton = new NSButton(new CoreGraphics.CGRect(40, 0, screenRes.Width - 120, PANEL_HIGHT-30))
            {
                Title = "",
                WantsLayer = true
            };

            subtitleTextButton.Layer.BackgroundColor = NSColor.Clear.CGColor;

            subtitleTextField = new NSTextField(new CoreGraphics.CGRect(40, 0, screenRes.Width - 120, PANEL_HIGHT-30))
            {
                Alignment = NSTextAlignment.Center
            };
            subtitleTextField.Cell.Alignment = NSTextAlignment.Center;

            forwardButton = new NSButton(new CoreGraphics.CGRect(0, 0, 40, 30));
            forwardButton.Title = ">>";
            forwardButton.Activated += (object sender, EventArgs e) => {
                subtitlesProvider.Forward();
            };

            backButton = new NSButton(new CoreGraphics.CGRect(0, 30, 40, 30));
            backButton.Title = "<<";
            backButton.Activated += (object sender, EventArgs e) => {
                subtitlesProvider.Back();
            };

            startStopButton = new NSButton(new CoreGraphics.CGRect(0, 60, 40, 30));
            startStopButton.Title = "Play";
            startStopButton.Activated += (object sender, EventArgs e) => {
                subtitlesProvider.StartStop(subtitlesProvider.Playing);
            };

            subtitlesPanel.ContentView.AddSubview(subtitleTextButton, NSWindowOrderingMode.Below, null);
            subtitlesPanel.ContentView.AddSubview(subtitleTextField, NSWindowOrderingMode.Below, null);

            subtitlesPanel.ContentView.AddSubview(forwardButton, NSWindowOrderingMode.Below, null);
            subtitlesPanel.ContentView.AddSubview(backButton, NSWindowOrderingMode.Below, null);
            subtitlesPanel.ContentView.AddSubview(startStopButton, NSWindowOrderingMode.Below, null);

            SetupSubtitlesProvider();
        }

        private NSAttributedString GetAttributedString(string text)
        {
            var paragraph = new NSMutableParagraphStyle();

            paragraph.Alignment = NSTextAlignment.Center;
            paragraph.LineBreakMode = NSLineBreakMode.ByWordWrapping;


            var attrString = new NSAttributedString
            (
                text,
                font: NSFont.FromFontName("Arial", 72.0f),
                foregroundColor: NSColor.White,
                backgroundColor: NSColor.FromCalibratedRgba(0, 0, 0, 0.0f),
                paragraphStyle: paragraph
            );
            return attrString;
        }

        private void SetupSubtitlesProvider()
        {
            subtitlesProvider.PlayStateChanged+=SubtitlesProvider_PlayStateChanged;
            subtitlesProvider.SubtitleChanged+=SubtitlesProvider_SubtitleChanged;
        }

        void SubtitlesProvider_SubtitleChanged(SubtitleEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Item.SubTitle))
            {
                NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
                {
                    subtitleTextField.AttributedStringValue = GetAttributedString(e.Item.StartTimeFormatted 
                    + " "+e.Item.SubTitle);
                });
            }
        }


        void SubtitlesProvider_PlayStateChanged(object sender, EventArgs e)
        {
            bool playState = subtitlesProvider.Playing;
            startStopButton.Title = playState ? "Stop" : "Play";
        }

        partial void ClickedButton(NSObject sender)
        {
            var nsUrl = subtitleFileSelector.GetFile();
            if (nsUrl == null)
                return;

            fileName = nsUrl.Path;
            subtitlesProvider.ReadFromFile(fileName);
            subtitlesProvider.SetSubTitle(0);
            startStopButton.Title = "Stop";
        }
    }
}
