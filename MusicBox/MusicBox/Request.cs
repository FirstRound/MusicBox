using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
//using System.Threading.Tasks;
using System.Web;

namespace MusicBox
{
    public class Request
    {

        private WebProvider _provider = WebProvider.Instance;

        public Request(VkSettings settings)
        {
            _provider.VkSettings = settings;
            
        }
        public List<Audio> getMyAudioList()
        {
            return getAudioList(_provider.getMyAudio(0));
        }

        public List<Audio> searchAudio(String search)
        {
            return getAudioList(_provider.searchAudio(search));
        }

        public List<Audio> getPopularAudioList()
        {
            Random rand = new Random((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            return getAudioList(_provider.getPopularAudio(rand.Next() % 2, rand.Next() % 100));
        }

        public List<Audio> getAdviseAudioList()
        {
            List<Audio> audio_list = new List<Audio>();
            Random rand = new Random((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            audio_list = getAudioList(_provider.getAdviseAudio(rand.Next() % 100, rand.Next() % 2)); 
            return audio_list;
        }

        public void addToMyAudio(Audio audio)
        {
            try
            {
                _provider.addAudio(audio.AudioID, audio.OwnerID);
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
                _provider.deleteAudio(audio.AudioID, audio.OwnerID);
            }
            catch (Exception ex)
            {
                // do smth
            }
        }

        //BEGIN PRIVATE METHODS

        private List<Audio> getAudioList(String response)
        {
            List<Audio> audio_list = new List<Audio>();
            try
            {
                JToken token = JToken.Parse(response);
                audio_list = token["response"].Children().Skip(1).Select(c => c.ToObject<Audio>()).ToList();
                //audio_list = JsonConvert.DeserializeObject<List<Audio>>(response_from_server);
                
            }
            catch
            {
                //do smth
            }
            return audio_list;
        }

        //END PRIVATE METHODS
    }
}
