using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace reyz_Twitch.TV
{
    class MainRequest
    {
        string path_application;

        string content;

        public string sendRequest(string url)
        {

            try
            {
                var request = (HttpWebRequest)
                    WebRequest.Create(url);

                request.Method = "GET";

                //New
                request.Proxy = null;

                request.ContentType = "application/x-www-form-urlencoded";
                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();


                var stream = new StreamReader(resp.GetResponseStream());
                content = stream.ReadToEnd();

                resp.Close();
                stream.Close();

            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        content = sr.ReadToEnd();

                        path_application = Path.GetDirectoryName(Application.ExecutablePath);
                        StreamWriter sw = new StreamWriter(path_application + "/log.txt", true);
                        sw.WriteLine(DateTime.Now.ToString() + " - " + content);
                        sw.Flush();
                        sw.Close();
                    }
                }

            }

            return content;
        }




        public string requestChannelStatus(string channel)
        {

            try
            {
                var request = (HttpWebRequest)
                    WebRequest.Create("https://api.twitch.tv/kraken/streams/" + channel + "");

                request.Method = "GET";

                //New
                request.Proxy = null;

                request.ContentType = "application/x-www-form-urlencoded";
                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();


                var stream = new StreamReader(resp.GetResponseStream());
                content = stream.ReadToEnd();

                resp.Close();
                stream.Close();



                //"stream":null}
                content = Regex.Match(content, "stream\":null").ToString();


            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        content = sr.ReadToEnd();

                        path_application = Path.GetDirectoryName(Application.ExecutablePath);
                        StreamWriter sw = new StreamWriter(path_application + "/log.txt", true);
                        sw.WriteLine(DateTime.Now.ToString() + " - " + content);
                        sw.Flush();
                        sw.Close();
                    }
                }

            }

            return content;
        }




        public string requestQuality(string channel)
        {
            //http://api.twitch.tv/api/channels/ultimaomega07/access_token

            channel = channel.ToLower();
            string source_content = sendRequest("http://api.twitch.tv/api/channels/" + channel + "/access_token");

            //regex token
            string regex_token = Regex.Match(source_content, "{\"token\":\"(.+?)\",\"sig\"").Groups[1].ToString();
            string regex_sig = Regex.Match(source_content, "\"sig\":\"(.+?)\",").Groups[1].ToString();


            string unescaped_token = Regex.Unescape(regex_token);

            string jsonRequest = sendRequest("http://usher.twitch.tv/select/" + channel + ".json?nauthsig=" + regex_sig + "&nauth=" + unescaped_token + "&allow_source=true");

            return jsonRequest;
        }




        public void startProcess(string path_livestreamer, string channel, string quality)
        {

            quality = quality.ToLower();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = path_livestreamer;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "http://twitch.tv/" + channel + " " + quality;
            Process.Start(startInfo);
        }



    }
}
