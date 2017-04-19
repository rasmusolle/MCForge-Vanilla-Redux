/*
	Copyright 2017 MCSpleef
	
	Dual-licensed under the	Educational Community License, Version 2.0 and
	the GNU General Public License, Version 3 (the "Licenses"); you may
	not use this file except in compliance with the Licenses. You may
	obtain a copy of the Licenses at
	
	http://www.opensource.org/licenses/ecl2.php
	http://www.gnu.org/licenses/gpl-3.0.html
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the Licenses are distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the Licenses for the specific language governing
	permissions and limitations under the Licenses.
*/
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace MCSpleef
{
	public static class Heartbeat
	{

		static string hash;
		public static string serverURL;
		static string staticVars;

		static HttpWebRequest request;

		static System.Timers.Timer heartbeatTimer = new System.Timers.Timer(500);

		public static void Init()
		{
			staticVars = "port=" + Server.port +
							"&users=" + 10 +
							"&max=" + Server.players +
							"&name=" + UrlEncode(Server.name) +
							"&public=" + Server.pub +
							"&salt=" + Server.salt +
							"&version=" + Server.version +
							"&software=MCForge-Redux";

			Thread backupThread = new Thread(new ThreadStart(delegate
			{
				heartbeatTimer.Elapsed += delegate
				{
					heartbeatTimer.Interval = 55000;
					try { Pump(); }
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
		retry: try {
				totalTries++;

				request = (HttpWebRequest)WebRequest.Create(new Uri(url));
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
				byte[] formData = Encoding.ASCII.GetBytes(postVars);
				request.ContentLength = formData.Length;
				request.Timeout = 15000;

			retryStream: try {
					using (Stream requestStream = request.GetRequestStream()) {
						requestStream.Write(formData, 0, formData.Length);
						requestStream.Close();
					}
				}
				catch (WebException e) {
					if (e.Status == WebExceptionStatus.Timeout) { goto retryStream; }
				}

				using (WebResponse response = request.GetResponse()) {
					using (StreamReader responseReader = new StreamReader(response.GetResponseStream())) {
						if (hash == null) {
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
			catch (WebException e) {
				if (e.Status == WebExceptionStatus.Timeout) { Pump(); }
			} catch {
				if (totalTries < 3) goto retry;
				return false;
			} finally { request.Abort(); }
			return true;
		}

		public static string UrlEncode(string input)
		{
			StringBuilder output = new StringBuilder();
			for (int i = 0; i < input.Length; i++) {
				if ((input[i] >= '0' && input[i] <= '9') ||
					(input[i] >= 'a' && input[i] <= 'z') ||
					(input[i] >= 'A' && input[i] <= 'Z') ||
					input[i] == '-' || input[i] == '_' || input[i] == '.' || input[i] == '~') {
					output.Append(input[i]);
				}
				else if (Array.IndexOf<char>(reservedChars, input[i]) != -1) { output.Append('%').Append(((int)input[i]).ToString("X")); }
			}
			return output.ToString();
		}


		public static char[] reservedChars = { ' ', '!', '*', '\'', '(', ')', ';', ':', '@', '&',
												 '=', '+', '$', ',', '/', '?', '%', '#', '[', ']' };
	}
}
