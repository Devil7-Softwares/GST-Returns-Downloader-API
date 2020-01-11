using System;
using System.Collections.Generic;
using System.Text;

namespace Devil7.Automation.GSTR.Downloader.Models
{
    public class UserRegDetails
    {
        public int status { get; set; }
        public Data data { get; set; }

        public class Data
        {
            public string regName { get; set; }
            public string userType { get; set; }
        }
    }
}
