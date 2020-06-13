using System.Collections.Generic;

namespace Devil7.Automation.GSTR.Downloader.Models
{
    public class CptySumGstr1
    {
        public string ctin { get; set; }
        public string chksum { get; set; }
        public int ttl_rec { get; set; }
        public double ttl_val { get; set; }
        public double ttl_tax { get; set; }
        public double ttl_igst { get; set; }
        public double ttl_sgst { get; set; }
        public double ttl_cgst { get; set; }
        public double ttl_cess { get; set; }
    }

    public class SecSumGstr1
    {
        public string sec_nm { get; set; }
        public string chksum { get; set; }
        public int ttl_rec { get; set; }
        public double ttl_val { get; set; }
        public double ttl_tax { get; set; }
        public double ttl_igst { get; set; }
        public double ttl_sgst { get; set; }
        public double ttl_cgst { get; set; }
        public double ttl_cess { get; set; }
        public int? ttl_doc_issued { get; set; }
        public int? ttl_doc_cancelled { get; set; }
        public int? net_doc_issued { get; set; }
        public double? ttl_expt_amt { get; set; }
        public double? ttl_ngsup_amt { get; set; }
        public double? ttl_nilsup_amt { get; set; }
        public IList<CptySumGstr1> cpty_sum { get; set; }
    }

    public class DataGstr1
    {
        public string gstin { get; set; }
        public string ret_period { get; set; }
        public string chksum { get; set; }
        public string time { get; set; }
        public IList<SecSumGstr1> sec_sum { get; set; }
        public bool b2bLimitReached { get; set; }
    }

    public class ReturnDataGSTR1
    {
        public int status { get; set; }
        public DataGstr1 data { get; set; }
    }
}
