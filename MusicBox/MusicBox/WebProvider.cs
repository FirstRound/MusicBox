using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;

namespace MusicBox
{
    public sealed class WebProvider
    {
        private static readonly Lazy<WebProvider> _lazy =
            new Lazy<WebProvider>(() => new WebProvider());
        private VkSettings _setting;
        private String _baseUrl = "https://api.vk.com/method/";
    
        public static WebProvider Instance 
        { 
            get 
            { 
                return _lazy.Value; 
            } 
        }

        private WebProvider()
        {
        }

        public VkSettings VkSettings
        {
            set
            {
                _setting = value;
            }
        }

        public String sendRequest(string request)
        {
            request = _baseUrl + request;
            String response_from_server = "";
            try 
            {
                request += "&access_token=" + _setting.Token;
                var client = new HttpClient();
                var responde = client.GetStringAsync(request);
                response_from_server = responde.Result;
            }
            catch (Exception ex)
            {
                // do smth
            }

            return response_from_server;
        }

        public string getMyAudio(int need_user)
        {
            return sendRequest("audio.get?owner_id=" + _setting.UserId + "&need_user=" + need_user);
        }

        public string searchAudio(string search)
        {
            return sendRequest("audio.search?q=" + search + "&need_userauto_complete=0&lyrics=0&performer_only=0&sort=2&search_own=0");
        }

        public string getPopularAudio(int only_eng, int offset)
        {
            return sendRequest("audio.getPopular?&only_eng=" + only_eng + "&offset=" + offset);
        }

        public string getAdviseAudio(int offset, int shuffle)
        {
            return sendRequest("audio.getRecommendations?&user_id=" + _setting.UserId +
                "&offset=" + offset + "&shuffle" + shuffle);
        }

        public void addAudio(int audio_id, int owner_id) 
        {
            sendRequest("audio.add?&audio_id=" + audio_id + "&owner_id=" + owner_id);
        }

        public void deleteAudio(int audio_id, int owner_id)
        {
            sendRequest("audio.delete?&audio_id=" + audio_id + "&owner_id=" + owner_id);
        }
    }
}
