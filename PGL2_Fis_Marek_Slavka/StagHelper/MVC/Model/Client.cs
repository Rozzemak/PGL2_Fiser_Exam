using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Windows.Media.Animation;
using Newtonsoft.Json;
using PGL2_Fis_Marek_Slavka.StagHelper.Debug;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Services;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model
{
    class OnlyOsIdJson
    {
        [JsonProperty("osCislo")]
        public List<string> OsId { get; set; }
    }



    class Client
    {
        public HttpClientHandler HttpClientHandler = new HttpClientHandler();
        public HttpClient HClient;
        public delegate string Worker(string request);
        public Worker SendRequest;
        private Thread _workerThread;
        public List<Task> Works = new List<Task>();
        private readonly BaseDebug _debug;

        public  StagUser StagUser;

        private const string Null1 = "null";
        private const string Null2 = "[]";
        private const string Null3 = "[null]";

        public Client(BaseDebug debug, StagUser stagUser)
        {
            this._debug = debug;
 
            this.StagUser = stagUser;

            HttpClientHandler.UseCookies = false;    

            HClient = new HttpClient(HttpClientHandler)
            {
                BaseAddress = new Uri("https://ws.ujep.cz/ws/services/rest2/"),
            };
            this.HClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.SendRequest += AddHttpGetRequest;        
            DoWork();

            if (string.IsNullOrEmpty(StagUser.StagOsId))
            {
                this.LogIn(this.StagUser);
            }
        }

        private void DoWork()
        {
            _workerThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(33);
                    for (int i = 0; i < Works.Count; i++)
                    {
                           // Old impl. bad if statement...
                           // Works[i].Status != TaskStatus.Running
                           // && Works[i].Status != TaskStatus.RanToCompletion
                           // && Works[i].Status != TaskStatus.WaitingForChildrenToComplete
                           // && Works[i].Status != TaskStatus.Faulted
                        if (Works[i].Status == TaskStatus.Created)
                            Works[i].Start();
                        else if (Works[i].Status == TaskStatus.Faulted)
                        {
                            var ex = Works[i].Exception;
                            var faulted = Works[i].IsFaulted;
                            if (faulted && ex != null)
                            {
                                if (Works[i] as Task<string> != null)
                                {
                                    _debug.AddMessage_Assync<object>(new Message<object>("Http response NOT received" + " |TaskID[" + Works[i].Id + "]"
                                        + " |TaskResult[" + (Works[i] as Task<string>).Exception.InnerException.Message + "]", MessageTypeEnum.Error)).Wait(-1);
                                }
                                else
                                {
                                    _debug.AddMessage_Assync<object>(new Message<object>("Http response NOT received" + " |TaskID[" + Works[i].Id + "]"
                                       + " |TaskResult[" + Works[i].Exception.Message + "]", MessageTypeEnum.Error)).Wait(-1);
                                    _debug.AddMessage_Assync<object>(new Message<object>(ex.Data, MessageTypeEnum.Exception)).Wait(-1);
                                }
                                Works.RemoveAt(i);
                            };
                        }
                        if (i < Works.Count && Works[i].Status == TaskStatus.RanToCompletion)
                        {
                            Works.RemoveAt(i);
                        }
                    }
                }
            });
            _workerThread.Start();
        }

        public void LogIn(StagUser stagUser)
        {
            if (stagUser.UserName != "guest" && stagUser.UserName != "")
            {
                // Set auth headers, and thenm just for the case, add additional Network Credentials for handler.
                // var byteArray = Encoding.ASCII.GetBytes(stagUser.FormatCredentials());
                //AuthenticationMode authMode = ((AuthenticationSection)WebConfigurationManager.GetSection("http://stag.ujep.cz/")).Mode;

                var ca = new CredentialCache
                {
                    { new Uri("https://ws.ujep.cz/"), "Basic", new NetworkCredential(stagUser.UserName, stagUser.Password)}
                };
                HttpClientHandler.Credentials = ca;
                HClient.DefaultRequestHeaders.Add("JSESSIONID", "token="+);
                // Parse OsId from Json
                stagUser.StagOsId =  GetAndParseStudentOsIdFromStagName(stagUser.UserName);
                // If OsID is non existent, then user does not exist.
                if (string.IsNullOrEmpty(stagUser.StagOsId))
                {
                    _debug.AddMessage_Assync<object>(new Message<object>("No stag user named:[" + stagUser.UserName + "] was found!", MessageTypeEnum.Exception)).Wait(-1);
                }
                else
                {
                    _debug.AddMessage<object>(new Message<object>("Login credentials set. You are [" + stagUser.UserName + "]" + "[" + stagUser.StagOsId + "]"));
                }

                Console.Title = "Debug:["+stagUser.UserName+"]";
            }
            else
            {
                Console.Title = "Debug:[guest]";
            }
        }

        private string AddHttpGetRequest(string request)
        {
            Task<string> task = new Task<string>(() => { return _SendReq(request); });
            Works.Add(task);
            _debug.AddMessage<object>(new Message<object>("Http request sent:[" + request + "]" + " |TaskID[" + task.Id + "]", MessageTypeEnum.HttpClient));
            while (Works.Contains(task)) { Thread.Sleep(1);}
            if (task.Status == TaskStatus.RanToCompletion)
                _debug.AddMessage<object>(new Message<object>("Http response received" + " |TaskID[" + task.Id + "]", MessageTypeEnum.HttpClient));
            // There is a problem when getting result from task, that has faulted => exception will raise as from the thread,
            // that calls and gets task result. (Bassically, the Exc will be rethrown.)
            try
            {
                return task.Result;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is ClientException)
                    {
                        // Create bunch of custom exceptions!
                        _debug.AddMessage_Assync<object>(new Message<object>((e.Message),MessageTypeEnum.Exception)).Wait(-1);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return "";
        }

        private string GetAndParseStudentOsIdFromStagName(string stagName)
        {
            Task<string> logInTask = new Task<string>(() =>
            {
                var parsedJson = AddHttpGetRequest("users/getOsobniCislaByExternalLogin?" +
                                                   "login=" + stagName +
                                                   "&outputFormat=json").Trim('[', ']').Trim();
                var osIdList = JsonConvert.DeserializeObject<OnlyOsIdJson>(parsedJson).OsId;
                if (osIdList.Count > 0)
                    return osIdList.First();
                throw new ClientException("No OsIds for specified person:[" + stagName + "] found.");
            });
            Works.Add(logInTask);
            try
            {
                return logInTask.Result;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is ClientException)
                    {
                        // Create bunch of custom exceptions!
                        _debug.AddMessage_Assync<object>(new Message<object>((e.Message), MessageTypeEnum.Exception)).Wait(-1);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Send html request (get), 
        /// </summary>
        /// <param name="request">request -(stag base address)</param>
        /// <returns>Result string of request previously sent to page.</returns>
        private string _SendReq(string request)
        {
            string jsonText = "";
            var message = new HttpRequestMessage(HttpMethod.Get, request);
            if(this.HttpClientHandler?.CookieContainer?.GetCookies(new Uri("https://portal.ujep.cz")).Count != 0)
            message.Headers.Add("cookie", this.HttpClientHandler?.CookieContainer?.GetCookies(new Uri("https://portal.ujep.cz"))[0].Value);
            //var response = this.HClient.GetAsync(message.RequestUri).Result;

            var result = HClient.SendAsync(message); 
            jsonText = result.Result.Content.ReadAsStringAsync().Result;
            if (result.Result.IsSuccessStatusCode)
            {
                if (jsonText != null && jsonText != Null1 && jsonText != Null2 && jsonText != Null3 &&
                    jsonText != "n")
                {
                    return jsonText;
                }
            }
            throw new ClientException("Http error:["+ result.Result.StatusCode.ToString() + "]");
            
            //throw new ClientException("Bad unidentified response message.");
        }
    }
}
