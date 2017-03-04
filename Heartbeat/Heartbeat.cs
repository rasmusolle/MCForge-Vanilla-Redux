/*
	Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl) Licensed under the
	Educational Community License, Version 2.0 (the "License"); you may
	not use this file except in compliance with the License. You may
	obtain a copy of the License at
	
	http://www.osedu.org/licenses/ECL-2.0
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the License is distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the License for the specific language governing
	permissions and limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Collections;

namespace MCForge
{

    public static class Heartbeat
    {
        //static int _timeout = 60 * 1000;

        static string hash;
        public static string serverURL;
        static string staticVars;

        //static BackgroundWorker worker;
        static HttpWebRequest request;
        static Random lawlBeatSeed = new Random(Process.GetCurrentProcess().Id);
        static StreamWriter beatlogger;

        static System.Timers.Timer heartbeatTimer = new System.Timers.Timer(500);
        static System.Timers.Timer lawlBeatTimer;

        public static void Init()
        {
            lawlBeatTimer = new System.Timers.Timer(1000 + lawlBeatSeed.Next(0, 2500));
            staticVars = "port=" + Server.port +
                            "&users=" + Player.number + "35" +
                            "&max=" + Server.players +
                            "&name=" + UrlEncode(Server.name) +
                            "&public=" + Server.pub +
                            "&salt=" + Server.salt +
                            "&version=" + Server.version +
                            "&software=MCGalaxy";
                            //"&software=MCSpleef (v" + Server.softVersion + ")";

            Thread backupThread = new Thread(new ThreadStart(delegate
            {
                heartbeatTimer.Elapsed += delegate
                {
                    heartbeatTimer.Interval = 55000;
                    try
                    {
                        Pump();
                    }
                    catch (Exception e) { Server.ErrorLog(e); }
                };
                heartbeatTimer.Start();
            }));
            backupThread.Start();
        }

        public static bool Pump()
        {
            string postVars = staticVars;

            string url = "http://www.classicube.net/heartbeat.jsp";
            int totalTries = 0;
    retry:  try
            {
                totalTries++;

                request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                byte[] formData = Encoding.ASCII.GetBytes(postVars);
                request.ContentLength = formData.Length;
                request.Timeout = 15000;

   retryStream: try
                {
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(formData, 0, formData.Length);
                        requestStream.Close();
                    }
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout)
                    {
                        goto retryStream;
                    }
                }

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                    {
                        if (hash == null)
                        {
                            string line = responseReader.ReadLine();
                            hash = line.Substring(line.LastIndexOf('=') + 1);
                            serverURL = line;

                            Server.s.UpdateUrl(serverURL);
                            File.WriteAllText("text/externalurl.txt", serverURL);
                            Server.s.Log("URL found: " + serverURL);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    beatlogger.WriteLine("Timeout detected at " + DateTime.Now.ToString());
                    Pump();
                }
            }
            catch
            {
                beatlogger.WriteLine("Heartbeat failure #" + totalTries + " at " + DateTime.Now.ToString());
                if (totalTries < 3) goto retry;

                beatlogger.WriteLine("Failed three times.  Stopping.");
                beatlogger.Close();
                return false;
            }
            finally
            {
                request.Abort();
            }
            if (beatlogger != null)
            {
                beatlogger.Close();
            }
            return true;
        }

        public static string UrlEncode(string input)
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if ((input[i] >= '0' && input[i] <= '9') ||
                    (input[i] >= 'a' && input[i] <= 'z') ||
                    (input[i] >= 'A' && input[i] <= 'Z') ||
                    input[i] == '-' || input[i] == '_' || input[i] == '.' || input[i] == '~')
                {
                    output.Append(input[i]);
                }
                else if (Array.IndexOf<char>(reservedChars, input[i]) != -1)
                {
                    output.Append('%').Append(((int)input[i]).ToString("X"));
                }
            }
            return output.ToString();
        }



        public static char[] reservedChars = { ' ', '!', '*', '\'', '(', ')', ';', ':', '@', '&',
                                                 '=', '+', '$', ',', '/', '?', '%', '#', '[', ']' };
    }
}
