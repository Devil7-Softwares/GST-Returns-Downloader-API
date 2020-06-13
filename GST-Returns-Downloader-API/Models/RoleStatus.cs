using System.Collections.Generic;

namespace Devil7.Automation.GSTR.Downloader.Models
{
    public class Return
    {
        public string return_ty { get; set; }
        public string due_dt { get; set; }
        public string status { get; set; }
        public bool tileDisable { get; set; }
    }

    public class User
    {
        public List<Return> returns { get; set; }
    }

    public class RoleData
    {
        public string userType { get; set; }
        public string userPref { get; set; }
        public List<User> user { get; set; }
    }

    public class RoleStatus
    {
        public int status { get; set; }
        public RoleData data { get; set; }
    }
}