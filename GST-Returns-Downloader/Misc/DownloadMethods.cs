using System;
using Devil7.Automation.GSTR.Downloader.Models;
using Newtonsoft.Json;
using RestSharp;

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
                            string Message = "Generate Request Successful: " + monthValue;
                            Console.WriteLine(Message);
                            result.Message = Message;
                            result.Result = CommandResult.Results.Success;
                        }
                        else if (returnResponse.error != null)
                        {
                            if (returnResponse.error.errorCode == "RTN_24")
                            {
                                string Message = "File Generation is Already In Progress: " + monthValue;
                                Console.WriteLine(Message);
                                result.Message = Message;
                                result.Result = CommandResult.Results.Success;
                            }
                            else
                            {
                                Console.WriteLine(returnResponse.error.detailMessage);
                                result.Message = returnResponse.error.detailMessage;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Request Error on GSTR1 JSON Generate!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on GSTR1 JSON Generate! " + ex.Message);
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
                                string Message = "Generate Request Not Given Previously, Given Now: " + monthValue;
                                Console.WriteLine(Message);
                                result.Message = Message;
                            }
                            else if (returnResponse.data.status == 0 && returnResponse.data.url != null && returnResponse.data.url.Count > 0)
                            {
                                string Message = "GSTR1 Downlaod Request Successful: " + monthValue;
                                Console.WriteLine(Message);
                                result.Result = CommandResult.Results.Success;
                                result.Message = Message;
                                result.Data = returnResponse.data.url;
                            }
                        }
                        else if (returnResponse.error != null)
                        {
                            if (returnResponse.error.errorCode == "RTN_24")
                            {
                                string Message = "File Generation is In Progress: " + monthValue;
                                Console.WriteLine(Message);
                                result.Result = CommandResult.Results.Success;
                            }
                            else
                            {
                                Console.WriteLine(returnResponse.error.detailMessage);
                                result.Message = returnResponse.error.detailMessage;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Request Error on GSTR1 JSON Download!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on GSTR1 JSON Download! " + ex.Message);
            }

            return result;
        }
    }
}
