using SubtitlesParser.Classes;
using System;

namespace SubtitlesPlayer
{
    public class SubtitleEventArgs : EventArgs
    {
        public SubtitleItem Item { get; set; }
        public int RowIndex { get; set; }
    }
}
