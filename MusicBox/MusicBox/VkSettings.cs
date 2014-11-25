using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace MusicBox
{
    public class VkSettings
    {
        private bool _auth;
        private String _user_id;
        private String _token;

        public bool Auth{get; set;}
        public String UserId { get; set; }
        public String Token { get; set; }


    }
}
