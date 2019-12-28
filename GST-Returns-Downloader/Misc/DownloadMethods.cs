using System;
using Devil7.Automation.GSTR.Downloader.Models;
using Newtonsoft.Json;
using RestSharp;
using Serilog;

namespace Devil7.Automation.GSTR.Downloader.Misc
{
    public class DownloadMethods
    {
        public static CommandResult GSTR1_JSON_GENERATE(RestClient client, string monthValue)
        {
            CommandResult result = new CommandResult(CommandResult.Results.Failed, "Unknown Error");
            try
            {
                RestRequest request = new RestRequest(string.Format(URLs.Gstr1JsonGenerateForce, monthValue), Method.GET);
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

                    Log.Error("Request Error on GSTR1 JSON Generate!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error on GSTR1 JSON Generate! " + ex.Message);
            }

            return result;
        }

        public static CommandResult GSTR1_JSON_DOWNLOAD(RestClient client, string monthValue)
        {
            CommandResult result = new CommandResult(CommandResult.Results.Failed, "Unknown Error");
            try
            {
                RestRequest request = new RestRequest(string.Format(URLs.Gstr1JsonGenerateForce, monthValue), Method.GET);
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
                                result.Message = "GSTR1 Downlaod Request Successful: " + monthValue;
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
                    Log.Error("Request Error on GSTR1 JSON Download!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error on GSTR1 JSON Download! " + ex.Message);
            }

            return result;
        }
    }
}
