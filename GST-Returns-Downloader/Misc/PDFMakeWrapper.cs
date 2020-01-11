using PuppeteerSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Devil7.Automation.GSTR.Downloader.Misc
{
    public class PDFMakeWrapper
    {
        #region Variables
        private Browser browser;
        private Page page;
        #endregion

        #region Properties
        public string DownloadsFolder { get; }
        public string GSTIN { get; set; }
        public string RegisteredName { get; set; }
        public string TradeName { get; set; }
        #endregion

        #region Constructor
        public PDFMakeWrapper(string DownloadsFolder)
        {
            this.DownloadsFolder = DownloadsFolder;
        }
        #endregion

        #region Private Methods
        private async Task InitializeBrowser()
        {
            string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".html";
            Log.Verbose("Writing pdfmake html to \"{0}\"...", fileName);
            System.IO.File.WriteAllText(fileName, Properties.Resources.pdfmake);

            Log.Information("Initializing Headless Browser...");
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            Log.Information("Initialized Headless Browser.");

            Log.Verbose("Creating new browser page & Navigating to pdfmake html...");
            page = await browser.NewPageAsync();
            page.Console += Page_Console;
            await page.GoToAsync(fileName);
            Log.Verbose("Navigated to pdfmake html.");
        }
        #endregion

        #region Private Functions
        private string GetFinancialYear(string monthValue)
        {
            int year = 0;
            int month = 0;
            if (monthValue.Length != 6 || !Int32.TryParse(monthValue.Substring(0,2), out month) || !Int32.TryParse(monthValue.Substring(2, 4), out year))
            {
                return string.Format("{0}-{1}", month > 3 ? year : year -1, (month > 3 ? year + 1 : year).ToString().Substring(2,2));
            }
            else
            {
                return "";
            }
        }

        private string GetMonth(string monthValue)
        {
            int month = 0;
            if (monthValue.Length != 6 || !Int32.TryParse(monthValue.Substring(0, 2), out month))
            {
                return CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
            }
            else
            {
                return "";
            }
        }
        #endregion

        #region Private Events
        private void Page_Console(object sender, ConsoleEventArgs e)
        {
            string value = e.Message.Text;
            if (value.StartsWith("PDF"))
            {
                string[] values = value.Split(":", 3);
                string fileName = values[1];
                string fileBase64 = values[2];
                byte[] fileData = Convert.FromBase64String(fileBase64);

                System.IO.File.WriteAllBytes(System.IO.Path.Combine(this.DownloadsFolder, fileName + ".pdf"), fileData);
                Log.Information("Downloading PDF : {0}", fileName);
            }
        }
        #endregion

        #region Public Methods
        public async Task GenerateGSTR1(string data, string monthValue, string filingStatus)
        {
            if (browser == null || page == null)
            {
                await InitializeBrowser();
            }
            await page.EvaluateFunctionAsync("generateGSTR1PDF", data, filingStatus, GSTIN, monthValue, GetFinancialYear(monthValue), RegisteredName, TradeName, null, null);
        }
        #endregion
    }
}
