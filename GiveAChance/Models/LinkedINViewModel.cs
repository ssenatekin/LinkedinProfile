using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GiveAChance.Models
{
    public class LinkedINViewModel
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }


    public class LinkedINResVM
    {
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string email { get; set; }  
        public Url picture { get; set; }
        public Sitestandardprofilerequest siteStandardProfileRequest { get; set; }
    }

    public class Sitestandardprofilerequest
    {
        public string url { get; set; }
    }
}