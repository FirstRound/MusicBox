using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBox
{
    public class AudioController
    {
        private List<Audio> _my_audio;
        private List<Audio> _audio_list = new List<Audio>();
        private List<Audio> _my_audio = new List<Audio>();
        private List<int> _order = new List<int>();
        private int _current_song = 0;
    }
}
