using System;
using System.Collections.Generic;
using System.Linq;

namespace SubtitlesParser.Classes
{
    public class SubtitleItem
    {

        //Properties------------------------------------------------------------------
        
        //StartTime and EndTime times are in milliseconds
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public List<string> Lines { get; set; } 
        public string SubTitle { get { return Lines.Count==0 ? "": Lines.Aggregate((i, j) => i + ' ' + j); } }
        
        public string StartTimeFormatted
        {
            get {
                var startTs = new TimeSpan(0, 0, 0, 0, StartTime).ToString(@"hh\:mm\:ss");
                return startTs;
            }
        }
        //Constructors-----------------------------------------------------------------

        /// <summary>
        /// The empty constructor
        /// </summary>
        public SubtitleItem()
        {
            this.Lines = new List<string>();
        }


        // Methods --------------------------------------------------------------------------

        /*public override string ToString()
        {
            var startTs = new TimeSpan(0, 0, 0, 0, StartTime);
            var endTs = new TimeSpan(0, 0, 0, 0, EndTime);

            var res = string.Format("{0} --> {1}: {2}", startTs.ToString("G"), 
                endTs.ToString("G"), string.Join(Environment.NewLine, Lines));
            return res;
        }*/

    }
}