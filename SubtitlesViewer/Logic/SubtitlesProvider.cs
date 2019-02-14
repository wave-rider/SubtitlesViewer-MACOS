using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers; 

namespace SubtitlesPlayer
{
    public delegate void SubtitleEventHandler(SubtitleEventArgs e);

    public class SubtitlesProvider
    {
        List<SubtitleItem> _items = new List<SubtitleItem>();
        private int _currentIndex = -1;
        private int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                _currentIndex = value;
            }
        }
        DateTime _startTime;
        private int StartIndex
        {
            get => _startIndex;
            set
            {
                _startIndex = value;
            }
        }
        float _offset = 0;

        Timer _timer = new Timer();
        private bool _playing = true;
        private bool _previousPlaying = false;
        private int _startIndex;

        public bool Playing
        {
            get => _playing;
            set
            {
                _previousPlaying = _playing;
                _playing = value;
            }
        }

        private bool _fileLoaded = false;

        public bool FileLoaded { get { return _fileLoaded; } set { _fileLoaded = value; } }

        public event SubtitleEventHandler SubtitleChanged;
        public event EventHandler PlayStateChanged;

        public SubtitlesProvider()
        {
            _timer.Interval = 200;  //in milliseconds
            _timer.Elapsed += T_Tick;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void T_Tick(object sender, EventArgs e)
        {
            if (!Playing)
                return;

            var timeSpan = DateTime.Now.Subtract(_startTime);
            var passedMilliseconds = timeSpan.TotalMilliseconds;

            if (passedMilliseconds + _offset > _items[CurrentIndex].EndTime - _items[StartIndex].StartTime)
            {
                CurrentIndex++;
                if (CurrentIndex == _items.Count)
                {
                    StartStop(true);
                }
                else
                {
                    NotifyListener();
                }
            }
        }

        private void NotifyListener()
        {
            if (SubtitleChanged != null)
            {
                var currentItem = _items.ElementAtOrDefault(CurrentIndex);
                SubtitleChanged.Invoke(new SubtitleEventArgs { Item = currentItem, RowIndex = CurrentIndex });
            }
        }

        public void StartStop(bool stop)
        {
            if (stop)
            {
                _timer.Stop();
                Playing = false;
            }
            else
            {
                Playing = true;
                _startTime = DateTime.Now;
                _timer.Start();
            }

            NotifyPlayStateChanged();
        }

        private void NotifyPlayStateChanged()
        {
            PlayStateChanged?.Invoke(this, new EventArgs());
        }

        public void SetSubTitle(int index)
        {

            _timer.Stop();
            CurrentIndex = index;
            StartIndex = index;
            _startTime = DateTime.Now;
            Playing = true;
            _offset = 0;
            NotifyListener();
            _timer.Start();
            NotifyPlayStateChanged();
        }

        public List<SubtitleItem> ReadFromFile(string file)
        {
            var parser = new SubtitlesParser.Classes.Parsers.SubParser();

            var fileName = Path.GetFileName(file);
            using (var fileStream = File.OpenRead(file))
            {
                try
                {
                    var mostLikelyFormat = parser.GetMostLikelyFormat(fileName);
                    var parsedItems = parser.ParseStream(fileStream, Encoding.UTF8, mostLikelyFormat);
                    var items = GetItems(parsedItems);

                    if (items.Any())
                    {
                        items.Insert(0, new SubtitleItem { });
                        _items = items;
                        _fileLoaded = true;
                        return items;
                    }
                    else
                    {
                        throw new ArgumentException("Not items found!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Parsing of file {0}: FAILURE\n{1}", fileName, ex);
                }
            }

            return new List<SubtitleItem>();
        }

        private List<SubtitleItem> GetItems(List<SubtitleItem> items)
        {
            var list = new List<SubtitleItem>();
            foreach (var item in items)
            {
                list.Add(
                    new SubtitleItem
                    {
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        Lines = item.Lines
                    }
                    );
            }

            return list;
        }

        internal void Forward()
        {
            if (CurrentIndex + 1 >= _items.Count)
            {
                return;
            }

            CurrentIndex++;
            SetSubTitle(CurrentIndex);
        }

        internal void Back()
        {
            if (CurrentIndex - 1 < 0)
                return;
            if (CurrentIndex > 0)
                CurrentIndex--;
            SetSubTitle(CurrentIndex);
        }
    }
}
