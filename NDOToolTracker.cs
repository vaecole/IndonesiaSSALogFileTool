using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IndonesiaSSALogFileTool
{
    class NDOToolTracker
    {
        private string toolName = "SSALogFileTool";//your tool name
        private string toolVersion = "V1.0";//tool version,different version means different projects.
        private static int siteNumber = 0;//caculate number of site
        private WebClient webClient = new WebClient();
        public int AddSiteNumber(int num)
        {
            siteNumber += num;
            return siteNumber;
        }

        public void SendToNDOTools(string preURL = "http://ndotools.sinaapp.com/")
        {
            try
            {
                string url = string.Format("{0}index.php?userName={1}&app={2}&version={3}&amount={4}", preURL, Environment.UserName, toolName, toolVersion, siteNumber);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.GetResponse();
            }
            catch
            {
                
            }
        }

        //public void SendToNDOTools()
        //{
        //    try
        //    {
        //        if (IsConnected())
        //        {
        //            //current info
        //            string url = string.Format("{0}index.php?userName={1}&app={2}&version={3}&amount={4}", "http://ndotools.sinaapp.com/", Environment.UserName, toolName, toolVersion, siteNumber);
        //            Uri uri = new Uri(url, UriKind.Absolute);
        //            WebClient client = new WebClient();
        //            client.Proxy = WebRequest.GetSystemWebProxy();
        //            client.OpenRead(uri);
        //            //log records  
        //            StreamReader strR = new StreamReader(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\NDOTool.log");
        //            string EncodeLine = string.Empty;
        //            try
        //            {
        //                List<string> myEncodeLines = new List<string>();
        //                while ((EncodeLine = strR.ReadLine()) != null)
        //                {
        //                    myEncodeLines.Add(EncodeLine);
        //                }
        //                strR.Close();
        //                foreach (string eachLine in myEncodeLines)
        //                {
        //                    string EncodeStr = "";
        //                    byte[] bytes = Convert.FromBase64String(eachLine);
        //                    try
        //                    {
        //                        EncodeStr = Encoding.UTF8.GetString(bytes);
        //                    }
        //                    catch
        //                    {
        //                        EncodeStr = eachLine;
        //                    }
        //                    //sendTCPMsg(DecodeBase64(eachLine));
        //                    string[] infoList = EncodeStr.Split(',');
        //                    string urlLog = string.Format("{0}index.php?userName={1}&app={2}&version={3}&amount={4}", "http://ndotools.sinaapp.com/", infoList[0], infoList[1], infoList[2], infoList[3]);
        //                    Uri uriLog = new Uri(urlLog, UriKind.Absolute);
        //                    WebClient clientLog = new WebClient();
        //                    clientLog.Proxy = WebRequest.GetSystemWebProxy();
        //                    clientLog.OpenRead(uriLog);
        //                }
        //                //clear log file
        //                StreamWriter srW2 = new StreamWriter(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\NDOTool.log", false, System.Text.Encoding.Default);
        //                srW2.Write(string.Empty);
        //                srW2.Close();
        //            }
        //            catch
        //            {
        //                strR.Close();
        //            }
        //        }
        //        else//write local log
        //        {
        //            StreamWriter srW = new StreamWriter(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\NDOTool.log", true, System.Text.Encoding.Default);
        //            string strMsg = Environment.UserName + "," + toolName + "," + toolVersion + "," + siteNumber;
        //            string strWrite = "";
        //            byte[] bytes = Encoding.UTF8.GetBytes(strMsg);
        //            try
        //            {
        //                strWrite = Convert.ToBase64String(bytes);
        //            }
        //            catch
        //            {
        //                strWrite = strMsg;
        //            }
        //            srW.WriteLine(strWrite);
        //            srW.Close();
        //        }
        //    }
        //    catch//write local log
        //    {
        //        StreamWriter srW = new StreamWriter(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\NDOTool.log", true, System.Text.Encoding.Default);
        //        string strMsg = Environment.UserName + "," + toolName + "," + toolVersion + "," + siteNumber;
        //        string strWrite = "";
        //        byte[] bytes = Encoding.UTF8.GetBytes(strMsg);
        //        try
        //        {
        //            strWrite = Convert.ToBase64String(bytes);
        //        }
        //        catch
        //        {
        //            strWrite = strMsg;
        //        }
        //        srW.WriteLine(strWrite);
        //        srW.Close();
        //    }

        //}
        //public bool IsConnected()
        //{
        //    System.Net.NetworkInformation.Ping ping;
        //    System.Net.NetworkInformation.PingReply res;
        //    ping = new System.Net.NetworkInformation.Ping();
        //    try
        //    {
        //        res = ping.Send("http://sinaapp.com/");
        //        //res = ping.Send("www.baidu.com");
        //        if (res.Status != System.Net.NetworkInformation.IPStatus.Success)
        //            return false;
        //        else
        //            return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

    }
}
