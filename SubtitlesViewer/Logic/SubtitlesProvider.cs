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
        List<SubtitleItem> _items;
        int _currentIndex = -1;
        DateTime _startTime;
        int _startIndex;
        float _offset = 0;

        Timer _timer = new Timer();
        private bool _playing = true;
        private bool _previousPlaying = false;

        public bool Playing { get => _playing;
            set {
                _previousPlaying = _playing;
                _playing = value;
            }
        }

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
         
            if (passedMilliseconds+_offset > _items[_currentIndex].EndTime - _items[_startIndex].StartTime)
            {
                _currentIndex++;
                if (_currentIndex == _items.Count)
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
                SubtitleChanged.Invoke(new SubtitleEventArgs { Item = _items[_currentIndex], RowIndex = _currentIndex });
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

        public void SetSubTitle(int index, bool setStartTime=false)
        {
            if ((Playing && Playing != _previousPlaying) || setStartTime)
            {
                _timer.Stop();
                _currentIndex = index;
                _startIndex = index;
                _startTime = DateTime.Now;
                Playing = true;
                _offset = 0;
                NotifyListener();
                _timer.Start();
                NotifyPlayStateChanged();
            }
            else
                _timer.Stop();

            
            
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
                    var items = parser.ParseStream(fileStream, Encoding.UTF8, mostLikelyFormat);
                    if (items.Any())
                    {
                        items.Insert(0, new SubtitleItem { });
                        _items = items;
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

        internal void Forward()
        {
            _currentIndex++;
            SetSubTitle(_currentIndex, true);
        }

        internal void Back()
        {
            if (_currentIndex > 0) _currentIndex--;
            SetSubTitle(_currentIndex, true);
        }
    }
}
