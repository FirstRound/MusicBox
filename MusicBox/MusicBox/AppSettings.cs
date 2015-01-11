using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace MusicBox
{
    public class AppSettings
    {

        public static int sSearchAutoComplete;
        public static bool sIsPlaying = false;
        public static bool sIsFind = false;
        public static bool sRepeatSong = false;
        public static bool sAddMode = false;
        public static bool sMenuOn = false;
        public static bool sRandomOrder = false;
        public static bool sVolumeActive = false;
        public static bool sAdviseMode = false;
        public static bool sPopularMode = false;
        public static bool sIsDownloading = false;

        //
        public int SearchAutoComplete { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsFind { get; set; }
        public bool RepeatSong { get; set; }
        public bool AddMode { get; set; }
        public bool MenuOn { get; set; }
        public bool RandomOrder { get; set; }
        public bool VolumeActive { get; set; }
        public bool AdviseMode { get; set; }
        public bool PopularMode { get; set; }
        public bool IsDownloading { get; set; }
    }
}
