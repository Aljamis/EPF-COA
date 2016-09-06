using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Newtonsoft.Json;



namespace EPF_NCOAmanager
{
    /// <summary>
    /// Singleton class responsible for storing session settings
    /// </summary>
    public class EPFController
    {
        private static EPFController MyController;
        private WebProxy myProxy;
        public bool isProxyValid = false;


        //
        private string EPFtokenKey = "";
        private string EPFlogonKey = "";


        private const string baseURL = "https://epfws.usps.gov/ws/resources";


        /// <summary>
        /// Constructor is private to insure Singleton Pattern
        /// </summary>
        private EPFController() { }


        public static EPFController getInstance()
        {
            if (MyController == null)
                MyController = new EPFController();
            return MyController;
        }



        /// <summary>
        /// Request (http get) current version of USPS EPF website.
        /// 
        /// When adding Newtonsoft.Json you must also add it to Intellitrace!!!!!!!!!
        /// Otherwise very strange errors arise:
        /// 
        /// System.Security.VerificationException was unhandled
        ///   HResult=-2146233075
        ///   Message=Operation could destabilize the runtime.
        ///   
        /// </summary>
        public JSONversion getVersion()
        {
            WebRequest req = WebRequest.Create( baseURL +"/epf/version");
            req.Proxy = myProxy;
            isProxyValid = false;       // Reset .. just in case user changes settings

            try
            {
                WebResponse resp = req.GetResponse();
                JSONversion deserialized = JsonConvert.DeserializeObject<JSONversion>(extractResponsePayload(resp));
                isProxyValid = true;
                return deserialized;
            }
            catch (WebException wEx)
            {
                //  This is added to look for a way to trace the 407 errors 
                //  we are getting in CEI.
                HttpWebResponse resp = (HttpWebResponse)wEx.Response;
                if ( resp.StatusCode == HttpStatusCode.ProxyAuthenticationRequired ) {
                    MessageBox.Show("Got the 407 from "+ resp.ResponseUri + Environment.NewLine + req.RequestUri );
                    
                }
                //  Done with investigatory code
                MessageBox.Show(wEx.Message);
                return null;
            }
        }



        /// <summary>
        /// Login to the EPF website
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        public bool epfLogin(string user, string pwd)
        {
            if (!isProxyValid)
            {
                displayErrorMsg("Valid Proxy Settings are missing");
                return false;
            }

            JSONlogin jLogin = new JSONlogin();     // No need to CommonToAllRequests ==> does not require Logon/Token keys
            jLogin.pword = pwd;
            jLogin.login = user;

            HttpWebResponse resp = performRequest(createRequest(jLogin, "/epf/login"));
            if (resp == null) return false;

//            JSONlogin respJSON = JsonConvert.DeserializeObject<JSONlogin>(extractResponsePayload(resp));
            return true;
        }



        public bool epfLogout()
        {
            JSONlogin jlogout = (JSONlogin)CommonToAllRequest(new JSONlogin());

            HttpWebResponse resp = performRequest( createRequest(jlogout , "/epf/logout") );
            if (resp == null) return false;
            return true;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public List<JSONfiles> epfGetFiles( EPFProductCodeIDs product , string fileStatus)
        {
            JSONlistIn jFileList = (JSONlistIn)CommonToAllRequest(new JSONlistIn());
            jFileList.productcode = product.Code;
            jFileList.productid = product.ID;
            jFileList.status = fileStatus;

            try
            {
                HttpWebResponse resp = performRequest(createRequest(jFileList, "/download/list"));
                if (resp == null) return null;

                processResponse(resp);
                JSONlistOut listOut = JsonConvert.DeserializeObject<JSONlistOut>(extractResponsePayload(resp));

                return listOut.filelist;
            }
            catch (WebException wEx)
            {
                Console.Write ( wEx.StackTrace );
            }
            return null;
        }




        /// <summary>
        /// Change the status
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newStatus"></param>
        public void changeStatus(JSONfiles file, string newStatus ) 
        {
            JSONchangeStatus j = (JSONchangeStatus)CommonToAllRequest(new JSONchangeStatus());
            j.fileid = file.fileid;
            j.newstatus = newStatus;
            file.status = newStatus;

            HttpWebResponse resp = performRequest(createRequest(j, "/download/status"));
            processResponse(resp);
        }



        /// <summary>
        /// Execute an HttpWebRequest and return an HttpWebResponse
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private HttpWebResponse performRequest(HttpWebRequest req) 
        {
            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;

            if ((req.HaveResponse) && resp != null)
            {
                if (!processResponse(resp))
                    return null;
            }
            else
            {
                MessageBox.Show("Could not get a response from [" + baseURL + "]");
                return null;
            }
            return resp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonObj"></param>
        /// <param name="RestMethod"></param>
        /// <returns></returns>
        private HttpWebRequest createRequest(Object jsonObj, string RestMethod)
        {
            string JSONstr = JsonConvert.SerializeObject(jsonObj);
            HttpWebRequest req = WebRequest.Create(baseURL + RestMethod) as HttpWebRequest;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            req.Proxy = myProxy;

            try
            {
                using (var streamwriter = new StreamWriter(req.GetRequestStream()))
                // Catch some WebExceptions
                {
                    streamwriter.Write("obj=" + JSONstr);
                    streamwriter.Flush();
                    streamwriter.Close();
                }
            }
            catch (WebException wEx)
            {
                MessageBox.Show(wEx.Message);
            }
            return req;
        }



        /// <summary>
        /// Common to all responses (except Version & Logout)
        /// 1)  check for failures
        /// 2)  Store the Token Logon keys for future calls.
        /// TRUE - Request was successful
        /// FALSE - There were issues
        /// </summary>
        /// <param name="resp"></param>
        private bool processResponse(WebResponse resp)
        {
            if ("failed".Equals( resp.Headers.Get("Service-Response") ) )
            {
                displayErrorMsg( resp.Headers.Get("Service-Messages") );
                return false;
            }

            this.EPFtokenKey = resp.Headers.Get("User-Tokenkey");
            this.EPFlogonKey = resp.Headers.Get("User-Logonkey");
            displayHeaders(resp);
            return true;
        }



        /// <summary>
        /// Extract the JSON string in the payload
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        private string extractResponsePayload(WebResponse resp)
        {
            StreamReader respReader = new StreamReader(resp.GetResponseStream());
            string sLine = "";
            string str = null;
            while ((str = respReader.ReadLine()) != null)
            {
                sLine += str;
            }
            return sLine;
        }



        /// <summary>
        /// All requests require populating Logon Key an Token key from the previous 
        /// WebResponse
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private Object CommonToAllRequest(EPF_JSONbase json)
        {
            json.logonkey = this.EPFlogonKey;
            json.tokenkey = this.EPFtokenKey;
            return json;
        }




        /// <summary>
        /// Temporary method to display all the Headers
        /// </summary>
        /// <param name="resp"></param>
        private void displayHeaders(WebResponse resp)
        {
            for (int i = 0; i < resp.Headers.Count; ++i)
            {
                Console.WriteLine("   {0} : {1}", resp.Headers.Keys[i], resp.Headers[i]);
            }
        }




        /// <summary>
        /// Instantiate and setup a Proxy
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        public void setupProxy(string host, int port, string user, string pwd)
        {
            myProxy = new WebProxy(host, port);
            myProxy.Credentials = new NetworkCredential(user, pwd);
        }


        /// <summary>
        /// Display a MessageBox with an error message
        /// </summary>
        /// <param name="msg"></param>
        public void displayErrorMsg(String msg , ArrayList missingSettings)
        {
            string combindedMsg = msg;
            foreach (string message in missingSettings)
            {
                combindedMsg += "\n";
                combindedMsg += message;
            }

            MessageBox.Show(combindedMsg);
        }

        /// <summary>
        /// Display a Message only with out an array of errors.
        /// </summary>
        /// <param name="msg"></param>
        public void displayErrorMsg(String msg)
        {
            displayErrorMsg(msg, new ArrayList());
        }

    }
}