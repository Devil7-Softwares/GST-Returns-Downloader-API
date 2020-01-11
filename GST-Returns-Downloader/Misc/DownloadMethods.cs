using System;
using Devil7.Automation.GSTR.Downloader.Models;
using Newtonsoft.Json;
using RestSharp;
using Serilog;

namespace Devil7.Automation.GSTR.Downloader.Misc
{
    public class DownloadMethods
    {
        #region Private Methods
        private static CommandResult GSTR_GENERATE(RestClient client, string monthValue, FileTypes fileType, string returnName)
        {
            CommandResult result = new CommandResult(CommandResult.Results.Failed, "Unknown Error");
            try
            {
                RestRequest request = new RestRequest(GenerateURL(true, fileType, monthValue, returnName), Method.GET);
                request.AddCookie("Lang", "en");
                request.AddHeader("Referer", URLs.GstrOfflineDownloadURL);
                RestResponse response = (RestResponse)client.Execute(request);
                if (response.IsSuccessful)
                {
                    ReturnResponse returnResponse = JsonConvert.DeserializeObject<ReturnResponse>(response.Content);
                    if (returnResponse != null)
                    {
                        if (returnResponse.data != null && returnResponse.data.status == 1)
                        {
                            result.Message = "Generate Request Successful: " + monthValue;
                            result.Result = CommandResult.Results.Success;
                            Log.Information(result.Message);
                        }
                        else if (returnResponse.error != null)
                        {
                            if (returnResponse.error.errorCode == "RTN_24")
                            {
                                result.Message = "File Generation is Already In Progress: " + monthValue;
                                result.Result = CommandResult.Results.Success;
                                Log.Warning(result.Message);
                            }
                            else
                            {
                                result.Message = returnResponse.error.detailMessage;
                                Log.Warning(result.Message);
                            }
                        }
                    }
                }
                else
                {

                    Log.Error(string.Format("Request Error on {0} JSON Generate!", returnName));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("Error on {0} JSON Generate! {1}", returnName, ex.Message));
            }

            return result;
        }

        private static CommandResult GSTR_DOWNLOAD(RestClient client, string monthValue, FileTypes fileType, string returnName)
        {
            CommandResult result = new CommandResult(CommandResult.Results.Failed, "Unknown Error");
            try
            {
                RestRequest request = new RestRequest(GenerateURL(false, fileType, monthValue, returnName), Method.GET);
                request.AddCookie("Lang", "en");
                request.AddHeader("Referer", URLs.GstrOfflineDownloadURL);
                RestResponse response = (RestResponse)client.Execute(request);
                if (response.IsSuccessful)
                {
                    ReturnResponse returnResponse = JsonConvert.DeserializeObject<ReturnResponse>(response.Content);
                    if (returnResponse != null)
                    {
                        if (returnResponse.data != null)
                        {
                            if (returnResponse.data.status == 1)
                            {
                                result.Message = "Generate Request Not Given Previously, Given Now: " + monthValue;
                                Log.Warning(result.Message);
                            }
                            else if (returnResponse.data.status == 0 && returnResponse.data.url != null && returnResponse.data.url.Count > 0)
                            {
                                result.Result = CommandResult.Results.Success;
                                result.Message = string.Format("{0} Download Request Successful: {1}", returnName, monthValue);
                                result.Data = returnResponse.data.url;
                                Log.Information(result.Message);
                            }
                        }
                        else if (returnResponse.error != null)
                        {
                            if (returnResponse.error.errorCode == "RTN_24")
                            {
                                result.Message = "File Generation is In Progress: " + monthValue;
                                result.Result = CommandResult.Results.Success;
                                Log.Warning(result.Message);
                            }
                            else
                            {
                                result.Message = returnResponse.error.detailMessage;
                                result.Result = CommandResult.Results.Failed;
                                Log.Warning(result.Message);
                            }
                        }
                    }
                }
                else
                {
                    Log.Error(string.Format("Request Error on {0} JSON Download!", returnName));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("Error on {0} JSON Download! {1}", returnName, ex.Message));
            }

            return result;
        }
        #endregion

        #region Public Methods
        public static CommandResult GSTR1_PDF_DOWNLOAD(RestClient client, string monthValue)
        {
            CommandResult result = new CommandResult(CommandResult.Results.Failed, "Unknown Error");
            try
            {
                RestRequest request = new RestRequest(string.Format(URLs.Gstr1Data, monthValue), Method.GET);
                request.AddCookie("Lang", "en");
                request.AddHeader("Referer", URLs.Gstr1URL);
                RestResponse response = (RestResponse)client.Execute(request);
                if (response.IsSuccessful)
                {
                    ReturnDataGSTR1 returnResponse = JsonConvert.DeserializeObject<ReturnDataGSTR1>(response.Content);
                    if (returnResponse != null)
                    {
                        if (returnResponse.data != null && returnResponse.status == 1)
                        {
                            result.Message = "GSTR1 Data Request Successful: " + monthValue;
                            result.Result = CommandResult.Results.Success;
                            result.Data = response.Content;
                            Log.Information(result.Message);
                        }
                        else
                        {
                            Log.Error("GSTR1 Status is 0");
                        }
                    }
                }
                else
                {

                    Log.Error(string.Format("Request Error on GSTR1 PDF Download!"));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("Error on GSTR1 PDF Download! {0}", ex.Message));
            }
            return result;
        }

        public static CommandResult GSTR1_JSON_GENERATE(RestClient client, string monthValue)
        {
            return GSTR_GENERATE(client, monthValue, FileTypes.JSON, "GSTR1");
        }

        public static CommandResult GSTR1_JSON_DOWNLOAD(RestClient client, string monthValue)
        {
            return GSTR_DOWNLOAD(client, monthValue, FileTypes.JSON, "GSTR1");
        }

        public static CommandResult GSTR2A_JSON_GENERATE(RestClient client, string monthValue)
        {
            return GSTR_GENERATE(client, monthValue, FileTypes.JSON, "GSTR2A");
        }

        public static CommandResult GSTR2A_JSON_DOWNLOAD(RestClient client, string monthValue)
        {
            return GSTR_DOWNLOAD(client, monthValue, FileTypes.JSON, "GSTR2A");
        }

        public static CommandResult GSTR2A_EXCEL_GENERATE(RestClient client, string monthValue)
        {
            return GSTR_GENERATE(client, monthValue, FileTypes.EXCEL, "GSTR2A");
        }

        public static CommandResult GSTR2A_EXCEL_DOWNLOAD(RestClient client, string monthValue)
        {
            return GSTR_DOWNLOAD(client, monthValue, FileTypes.EXCEL, "GSTR2A");
        }
        #endregion

        #region Enums
        private enum FileTypes
        {
            JSON,
            EXCEL
        }
        #endregion

        #region Private Helper Methods
        private static string GenerateURL(bool forceGenerate, FileTypes fileType, string returnPeriod, string returnName)
        {
            return string.Format(URLs.GstrReturnGenerateOrDownload, fileType == FileTypes.EXCEL ? "file_type=EX&" : "", forceGenerate ? 1 : 0, returnPeriod, returnName);
        }
        #endregion
    }
}
