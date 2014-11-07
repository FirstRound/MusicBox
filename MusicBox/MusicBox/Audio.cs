using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace MusicBox
{
    public class Audio
    {
        public int aid {get; set;}
        public int owner_id {get; set;}
        public String artist  {get; set;}
        public String title {get; set;}
        public int duration {get; set;}
        public String url {get; set;}
        public String lurics_id {get; set;}
        public int genre { get; set; }
    }
}
