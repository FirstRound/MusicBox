using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MusicBox
{
    public class AudioController
    {
        private List<Audio> _audio_list = new List<Audio>();
        private List<Audio> _my_audio = new List<Audio>();
        private List<int> _order = new List<int>();
        private int _current_song = 0;
        private Request _request;

        private AppSettings _settings = new AppSettings();
        private VkSettings _vk_settings;

        public AudioController(VkSettings vk)
        {
            _request = new Request(vk);
            _settings.IsPlaying = false;
            AppSettings.sIsPlaying = false;
            _my_audio = _request.getMyAudioList();
            _audio_list = _my_audio;

        }

        public List<Audio> CurrentAudioList
        {
            get
            {
                return _audio_list;
            }
            set
            {
                _audio_list = value;
            }
        }

        public void SetCurrentAudioList(List<Audio> audio)
        {
            CurrentAudioList = audio;
            for (int i = 0; i < _audio_list.Count; i++)
            {
                _order.Add(i);
            }
        }

        public void ChangeToMyAudioList()
        {
            _audio_list = _my_audio;
        }

        public void Add(int i)
        {
            if (_settings.AddMode)
            {
                AddAudio(i);
            }
            else
            {
                DeleteAudio(i);
            }
        }

        public void AddAudio(int i) 
        {
            _request.addToMyAudio(CurrentAudioList[i]);
            MyAudioList.Add(CurrentAudioList[i]);
        }

        public void DeleteAudio(int i)
        {
            _request.deleteFromMyAudio(_audio_list[i]);
            _my_audio.Remove(_audio_list[i]);
        }

        public List<Audio> MyAudioList
        {
            get
            {
                return _my_audio;
            }
            set
            {
                _my_audio = value;
            }
        }

        public AppSettings Settings
        {
            get
            {
                return _settings;
            }
        }

        public int CurrentSong
        {
            get
            {
                return _current_song;
            }
            set
            {
                _current_song = value;
            }
        }

        public bool HasNextSong()
        {
            if (CurrentSong + 1 <= CurrentAudioList.Count - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Audio GetNextSong()
        {
            if (HasNextSong())
            {
                return CurrentAudioList[++_current_song];
            }
            else
            {
                return CurrentAudioList[_current_song];
            }
        }

        public bool HasPrevSong()
        {
            if (CurrentSong - 1 >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Audio GetPrevSong()
        {
            if (HasPrevSong())
            {
                return CurrentAudioList[--_current_song];
            }
            else
            {
                return CurrentAudioList[_current_song];
            }
        }

        public Audio CurrentAudio {
            get
            {
                if (_current_song == -1)
                    _current_song = 0;
                return CurrentAudioList[_current_song];
            }
        }

        private void ShuffleOrder()
        {
            _order.Clear();
            for (int i = 0; i < CurrentAudioList.Count; i++)
            {
                _order.Add(i);
            }

            Random rng = new Random();
            int n = CurrentAudioList.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Audio value = CurrentAudioList[k];
                CurrentAudioList[k] = CurrentAudioList[n];
                CurrentAudioList[n] = value;

                int s = _order[k];
                _order[k] = _order[n];
                _order[n] = s;

            }
        }
        private void NormalOrder()
        {
            List<Audio> right_ordered = new List<Audio>();

            for (int i = 0; i < CurrentAudioList.Count; i++)
            {
                right_ordered.Add(new Audio());
            }

            for (int i = 0; i < _order.Count; i++)
            {
                right_ordered[_order[i]] = CurrentAudioList[i];
            }
            _audio_list =  right_ordered;
        }

        public bool Shuffle() 
        {
            if (_settings.RandomOrder)
            {
                NormalOrder();
                _settings.RandomOrder = false;
                return true;
            }
            else
            {
                ShuffleOrder();
                _settings.RandomOrder = true;
                return false;
            }
        }

        public void SearchAudio(String search) 
        {
            CurrentAudioList = _request.searchAudio(search);
        }

        public void GetPopularAudio()
        {
            CurrentAudioList =  _request.getPopularAudioList();
        }

        public void GetAdviceAudio()
        {
            CurrentAudioList = _request.getAdviseAudioList();
        }

        public void GetMyAudio()
        {
            CurrentAudioList = _request.getMyAudioList();
        }

        public String Play(ref MediaElement media_element)
        {
            if (!AppSettings.sIsPlaying)
            {
                if (_audio_list.Count > _current_song)
                {
                    if (media_element.Source != new Uri(CurrentAudio.Url))
                        media_element.Source = new Uri(CurrentAudio.Url);
                    AppSettings.sIsPlaying = true;
                    media_element.Play();
                }
            }
            else
            {
                AppSettings.sIsPlaying = false;
                media_element.Pause();
            }
            return CurrentAudio.Description;
        }

        public String SelectionChanged(ref MediaElement media_element)
        {
            media_element.Source = new Uri(_audio_list[_current_song].Url);
            media_element.Play();
            return _audio_list[_current_song].Artist + ": " + _audio_list[_current_song].Title;
        }

        //
        private void shuffleAudioList(List<Audio> audio_list)
        {
            _order.Clear();
            for (int i = 0; i < audio_list.Count; i++)
            {
                _order.Add(i);
            }

            Random rng = new Random();
            int n = audio_list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Audio value = audio_list[k];
                audio_list[k] = audio_list[n];
                audio_list[n] = value;

                int s = _order[k];
                _order[k] = _order[n];
                _order[n] = s;

            }
        }
    }
}
