using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Devil7.Automation.GSTR.Downloader.Models
{
    class ReturnDataGSTR3B
    {

        #region Properties
        public string formDetailsContent { get; set; }
        public FormDetails formDetails { get; }

        public string summaryContent { get; set; }
        public Summary summary { get; }

        public string taxPayableContent { get; set; }
        public TaxPayable taxPayable { get; }
        #endregion

        #region Constructor
        public ReturnDataGSTR3B(string formDetails, string summary, string taxPayable)
        {
            this.formDetailsContent = formDetails;
            this.summaryContent = summary;
            this.taxPayableContent = taxPayable;

            this.formDetails = JsonConvert.DeserializeObject<FormDetails>(formDetails);
            this.summary = JsonConvert.DeserializeObject<Summary>(summary);
            this.taxPayable = JsonConvert.DeserializeObject<TaxPayable>(taxPayable);
        } 
        #endregion

        #region Custom Classes
        public class FormDetails
        {
            public int status { get; set; }
            public Data data { get; set; }

            public class Data
            {
                public string gstin { get; set; }
                public string fy { get; set; }
                public string fm { get; set; }
                public string fp { get; set; }
                public string bn { get; set; }
                public string ln { get; set; }
                public string tn { get; set; }
                public string due_dt { get; set; }
                public string status { get; set; }
                public string apprv_dt { get; set; }
                public string evc_chk { get; set; }
                public string isGstp { get; set; }
                public string fil_dt { get; set; }
                public string arn { get; set; }
            }
        }

        public class Summary
        {
            public int status { get; set; }
            public Data data { get; set; }

            public class OsupDet
            {
                public double txval { get; set; }
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class OsupZero
            {
                public double txval { get; set; }
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class OsupNilExmp
            {
                public double txval { get; set; }
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class IsupRev
            {
                public double txval { get; set; }
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class OsupNongst
            {
                public double txval { get; set; }
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class SupDetails
            {
                public OsupDet osup_det { get; set; }
                public OsupZero osup_zero { get; set; }
                public OsupNilExmp osup_nil_exmp { get; set; }
                public IsupRev isup_rev { get; set; }
                public OsupNongst osup_nongst { get; set; }
            }

            public class ItcAvl
            {
                public string ty { get; set; }
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class ItcRev
            {
                public string ty { get; set; }
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class ItcNet
            {
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class ItcInelg
            {
                public string ty { get; set; }
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class ItcElg
            {
                public IList<ItcAvl> itc_avl { get; set; }
                public IList<ItcRev> itc_rev { get; set; }
                public ItcNet itc_net { get; set; }
                public IList<ItcInelg> itc_inelg { get; set; }
            }

            public class IntrDetails
            {
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class LtfeeDetails
            {
                public double iamt { get; set; }
                public double camt { get; set; }
                public double samt { get; set; }
                public double csamt { get; set; }
            }

            public class IntrLtfee
            {
                public IntrDetails intr_details { get; set; }
                public LtfeeDetails ltfee_details { get; set; }
            }

            public class TtVal
            {
                public double tt_pay { get; set; }
                public double tt_csh_pd { get; set; }
                public double tt_itc_pd { get; set; }
            }

            public class Qn
            {
                public string q1 { get; set; }
                public string q2 { get; set; }
                public string q3 { get; set; }
                public string q4 { get; set; }
                public string q5 { get; set; }
                public string q6 { get; set; }
                public string q7 { get; set; }
            }

            public class Data
            {
                public string gstin { get; set; }
                public string ret_period { get; set; }
                public SupDetails sup_details { get; set; }
                public ItcElg itc_elg { get; set; }
                public IntrLtfee intr_ltfee { get; set; }
                public TtVal tt_val { get; set; }
                public Qn qn { get; set; }
            }


        }

        public class TaxPayable
        {
            public int status { get; set; }
            public Data data { get; set; }

            public class Igst
            {
                public double tx { get; set; }
                public double intr { get; set; }
                public double pen { get; set; }
                public double fee { get; set; }
                public double oth { get; set; }
            }

            public class Cgst
            {
                public double tx { get; set; }
                public double intr { get; set; }
                public double pen { get; set; }
                public double fee { get; set; }
                public double oth { get; set; }
            }

            public class Sgst
            {
                public double tx { get; set; }
                public double intr { get; set; }
                public double pen { get; set; }
                public double fee { get; set; }
                public double oth { get; set; }
            }

            public class Cess
            {
                public double tx { get; set; }
                public double intr { get; set; }
                public double pen { get; set; }
                public double fee { get; set; }
                public double oth { get; set; }
            }

            public class CashBal
            {
                public Igst igst { get; set; }
                public Cgst cgst { get; set; }
                public Sgst sgst { get; set; }
                public Cess cess { get; set; }
                public double igst_tot_bal { get; set; }
                public double cgst_tot_bal { get; set; }
                public double sgst_tot_bal { get; set; }
                public double cess_tot_bal { get; set; }
            }

            public class ItcBal
            {
                public double igst_bal { get; set; }
                public double cgst_bal { get; set; }
                public double sgst_bal { get; set; }
                public double cess_bal { get; set; }
            }

            public class Bal
            {
                public string gstin { get; set; }
                public CashBal cash_bal { get; set; }
                public ItcBal itc_bal { get; set; }
            }

            public class TaxPay
            {
                public Igst igst { get; set; }
                public Sgst sgst { get; set; }
                public Cgst cgst { get; set; }
                public Cess cess { get; set; }
                public int liab_id { get; set; }
                public int trancd { get; set; }
                public string trandate { get; set; }
            }

            public class PdByItc
            {
                public string debit_id { get; set; }
                public int liab_id { get; set; }
                public double igst_igst_amt { get; set; }
                public double igst_cgst_amt { get; set; }
                public double igst_sgst_amt { get; set; }
                public double sgst_sgst_amt { get; set; }
                public double sgst_igst_amt { get; set; }
                public double cgst_cgst_amt { get; set; }
                public double cgst_igst_amt { get; set; }
                public double cess_cess_amt { get; set; }
                public int trancd { get; set; }
                public string trandate { get; set; }
            }

            public class TaxPaid
            {
                public IList<object> pd_by_cash { get; set; }
                public IList<PdByItc> pd_by_itc { get; set; }
            }

            public class ReturnsDbCdredList
            {
                public IList<TaxPay> tax_pay { get; set; }
                public TaxPaid tax_paid { get; set; }
            }

            public class Data
            {
                public Bal bal { get; set; }
                public int status { get; set; }
                public ReturnsDbCdredList returnsDbCdredList { get; set; }
            }


        }
        #endregion

    }
}
