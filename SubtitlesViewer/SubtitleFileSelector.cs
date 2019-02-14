using System;
using AppKit;
using Foundation;

namespace SubtitlesViewer
{
    public class SubtitleFileSelector
    {
        public NSUrl GetFile()
        {
            // Create the File Open Dialog class.
            NSOpenPanel openDlg = NSOpenPanel.OpenPanel;

            // Enable the selection of files in the dialog.
            openDlg.CanChooseFiles = true;

            // Multiple files not allowed
            openDlg.AllowsMultipleSelection = false;
            //openDlg.

            // Can't select a directory
            openDlg.CanChooseDirectories = false;

            // Display the dialog. If the OK button was pressed,
            // process the files.
            var openResult = openDlg.RunModal();
            Console.Write($"{openResult}");
            if (openResult == 1)
            {
                // Get an array containing the full filenames of all
                // files and directories selected.
                NSUrl[] files = openDlg.Urls;

                // Loop through all the files and process them.
                return files[0];
            }
            return null;

    }
}
}
