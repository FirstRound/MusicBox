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
            List<Audio> audio_list = new List<Audio>();
            audio_list = sendAudioRequest("https://api.vk.com/method/audio.get?owner_id=" + VkSettings.Id + "&need_user=0&access_token=" + VkSettings.Token);
            return audio_list;
        }

        public List<Audio> searchAudio(String search)
        {
            List<Audio> audio_list = new List<Audio>();
            audio_list = sendAudioRequest("https://api.vk.com/method/audio.search?&access_token=" + VkSettings.Token + "&q=" + search + "&need_userauto_complete=" + AppSettings.SearchAutoComplete + "&lyrics=0&performer_only=0&sort=2&search_own=0");
            return audio_list;
        }

        public List<Audio> getPopularAudioList()
        {
            List<Audio> audio_list = new List<Audio>();
            Random rand = new Random((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            audio_list = sendAudioRequest("https://api.vk.com/method/audio.getPopular?&access_token=" + VkSettings.Token + "&only_eng=" + rand.Next() % 2 + "&offset=" + rand.Next() % 100);
            return audio_list;
        }

        public List<Audio> getAdviseAudioList()
        {
            List<Audio> audio_list = new List<Audio>();
            Random rand = new Random((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            audio_list = sendAudioRequest("https://api.vk.com/method/audio.getRecommendations?&access_token=" + VkSettings.Token + "&user_id=" + VkSettings.Id + "&offset=" + rand.Next() % 100 + "&shuffle" + rand.Next() % 2); 
            return audio_list;
        }

        public void addToMyAudio(Audio audio)
        {
            try
            {
                String req = "https://api.vk.com/method/audio.add?&access_token=" + VkSettings.Token + "&audio_id=" + audio.aid + "&owner_id=" + audio.owner_id;
                String resp = sendRequest(req);
            }
            catch (Exception ex)
            {
                // do smth
            }
        }


        public void deleteFromMyAudio(Audio audio)
        {
            try
            {
                String req = "https://api.vk.com/method/audio.delete?&access_token=" + VkSettings.Token + "&audio_id=" + audio.aid + "&owner_id=" + audio.owner_id;
                String resp = sendRequest(req);
            }
            catch (Exception ex)
            {
                // do smth
            }
        }

        //BEGIN PRIVATE METHODS

        private List<Audio> sendAudioRequest(String request_text)
        {
            List<Audio> audio_list = new List<Audio>();
            try
            {
                String response_from_server = sendRequest(request_text);

                JToken token = JToken.Parse(response_from_server);
                audio_list = token["response"].Children().Skip(1).Select(c => c.ToObject<Audio>()).ToList();
            }
            catch
            {
                //do smth
            }
            return audio_list;
        }

        private String sendRequest(String request_text)
        {
            String response_from_server = "";
            try
            {
                WebRequest request = WebRequest.Create(request_text);
                WebResponse response = request.GetResponse();
                Stream data_stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(data_stream);
                response_from_server = reader.ReadToEnd();
                reader.Close();
                response.Close();
                response_from_server = HttpUtility.HtmlDecode(response_from_server);
            }
            catch (Exception ex)
            {
                // do smth
            }

            return response_from_server;
        }

        //END PRIVATE METHODS
    }
}
