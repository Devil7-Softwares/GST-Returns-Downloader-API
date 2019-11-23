using System.Collections.Generic;

namespace Devil7.Automation.GSTR.Downloader.Models {
    public class Month {
        public string month { get; set; }
        public string value { get; set; }
    }

    public class Year {
        public string year { get; set; }
        public IList<Month> months { get; set; }
    }

    public class Data {
        public IList<Year> Years { get; set; }
    }

    public class MonthsResponseData {
        public int status { get; set; }
        public Data data { get; set; }
    }
}