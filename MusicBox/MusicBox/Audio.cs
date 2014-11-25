using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace MusicBox
{
    public class Audio
    {

        [JsonProperty("aid")] 
        public int AudioID {get; set;}

        [JsonProperty("owner_id")] 
        public int OwnerID {get; set;}

        [JsonProperty("artist")] 
        public String Artist  {get; set;}

        [JsonProperty("title")] 
        public String Title {get; set;}

        [JsonProperty("duration")] 
        public int Duration {get; set;}

        [JsonProperty("url")] 
        public String Url {get; set;}

        [JsonProperty("lurics_id")] 
        public String LuricsID {get; set;}

        [JsonProperty("genre")]
        public int Genre { get; set; }
    }
}
