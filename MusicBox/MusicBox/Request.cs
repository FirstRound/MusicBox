using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MusicBox
{
    public class Request
    {
        public List<Audio> getMyAudioList()
        {
            WebRequest request = WebRequest.Create("https://api.vk.com/method/audio.get?owner_id="+VkSettings.Id+"&need_user=0&access_token=" + VkSettings.Token);
            WebResponse response = request.GetResponse();
            Stream data_stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(data_stream);
            String response_from_server = reader.ReadToEnd();
            reader.Close();
            response.Close();
            response_from_server = HttpUtility.HtmlDecode(response_from_server);

            JToken token = JToken.Parse(response_from_server);
            List<Audio> audio_list = new List<Audio>();
            audio_list = token["response"].Children().Skip(1).Select(c => c.ToObject<Audio>()).ToList();
            return audio_list;
        }
    }
}
