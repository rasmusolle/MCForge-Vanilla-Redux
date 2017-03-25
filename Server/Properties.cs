/*
	Copyright © 2009-2014 MCSharp team (Modified for use with MCZall/MCLawl/MCForge/MCForge-Redux)
	
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
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MCForge {
	public static class SrvProperties {
		public static void Load (string givenPath, bool skipsalt = false)
				{
						/*
			if (!skipsalt)
			{
				Server.salt = "";
				string rndchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
				Random rnd = new Random();
				for (int i = 0; i < 16; ++i) { Server.salt += rndchars[rnd.Next(rndchars.Length)]; }
			}*/
						if (!skipsalt) {
								bool gotSalt = false;
								if (File.Exists ("text/salt.txt")) {
										string salt = File.ReadAllText ("text/salt.txt");
										if (salt.Length != 16)
												Server.s.Log ("Invalid salt in salt.txt!");
										else {
												Server.salt = salt;
												gotSalt = true;
										}
								}
								if (!gotSalt) {
										RandomNumberGenerator prng = RandomNumberGenerator.Create ();
										StringBuilder sb = new StringBuilder ();
										byte[] oneChar = new byte[1];
										while (sb.Length < 16) {
												prng.GetBytes (oneChar);
												if (Char.IsLetterOrDigit ((char)oneChar [0])) {
														sb.Append ((char)oneChar [0]);
												}
										}
										Server.salt = sb.ToString ();
								}

								if (File.Exists (givenPath)) {
										string[] lines = File.ReadAllLines (givenPath);

										foreach (string line in lines) {
												if (line != "" && line [0] != '#') {
														//int index = line.IndexOf('=') + 1; // not needed if we use Split('=')
														string key = line.Split ('=') [0].Trim ();
														string value = "";
														if (line.IndexOf ('=') >= 0)
																value = line.Substring (line.IndexOf ('=') + 1).Trim (); // allowing = in the values
														string color = "";

                                                        switch (key.ToLower())
                                                        {
                                                            case "server-name":
                                                                if (ValidString(value, "![]:.,{}~-+()?_/\\' "))
                                                                {
                                                                    Server.name = value;
                                                                }
                                                                else
                                                                {
                                                                    Server.s.Log("server-name invalid! setting to default.");
                                                                }
                                                                break;
                                                            case "motd":
                                                                if (ValidString(value, "=![]&:.,{}~-+()?_/\\' "))
                                                                { // allow = in the motd
                                                                    Server.motd = value;
                                                                }
                                                                else
                                                                {
                                                                    Server.s.Log("motd invalid! setting to default.");
                                                                }
                                                                break;
                                                            case "port":
                                                                try
                                                                {
                                                                    Server.port = Convert.ToInt32(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("port invalid! setting to default.");
                                                                }
                                                                break;
                                                            case "verify-names":
                                                                Server.verify = (value.ToLower() == "true") ? true : false;
                                                                break;
                                                            case "public":
                                                                Server.pub = (value.ToLower() == "true") ? true : false;
                                                                break;
                                                            case "max-players":
                                                                try
                                                                {
                                                                    if (Convert.ToByte(value) > 128)
                                                                    {
                                                                        value = "128";
                                                                        Server.s.Log("Max players has been lowered to 128.");
                                                                    }
                                                                    else if (Convert.ToByte(value) < 1)
                                                                    {
                                                                        value = "1";
                                                                        Server.s.Log("Max players has been increased to 1.");
                                                                    }
                                                                    Server.players = Convert.ToByte(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("max-players invalid! setting to default.");
                                                                }
                                                                break;
                                                            case "irc":
                                                                Server.irc = (value.ToLower() == "true") ? true : false;
                                                                break;
                                                            case "irc-colorsenable":
                                                                Server.ircColorsEnable = (value.ToLower() == "true") ? true : false;
                                                                break;
                                                            case "irc-server":
                                                                Server.ircServer = value;
                                                                break;
                                                            case "irc-nick":
                                                                Server.ircNick = value;
                                                                break;
                                                            case "irc-channel":
                                                                Server.ircChannel = value;
                                                                break;
                                                            case "irc-opchannel":
                                                                Server.ircOpChannel = value;
                                                                break;
                                                            case "irc-port":
                                                                try
                                                                {
                                                                    Server.ircPort = Convert.ToInt32(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("irc-port invalid! setting to default.");
                                                                }
                                                                break;
                                                            case "irc-identify":
                                                                try
                                                                {
                                                                    Server.ircIdentify = Convert.ToBoolean(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("irc-identify boolean value invalid! Setting to the default of: " + Server.ircIdentify + ".");
                                                                }
                                                                break;
                                                            case "irc-password":
                                                                Server.ircPassword = value;
                                                                break;

                                                            case "deathcount":
                                                                Server.deathcount = (value.ToLower() == "true") ? true : false;
                                                                break;
                                                            case "defaultcolor":
                                                                color = c.Parse(value);
                                                                if (color == "")
                                                                {
                                                                    color = c.Name(value);
                                                                    if (color != "")
                                                                        color = value;
                                                                    else
                                                                    {
                                                                        Server.s.Log("Could not find " + value);
                                                                        return;
                                                                    }
                                                                }
                                                                Server.DefaultColor = color;
                                                                break;
                                                            case "irc-color":
                                                                color = c.Parse(value);
                                                                if (color == "")
                                                                {
                                                                    color = c.Name(value);
                                                                    if (color != "")
                                                                        color = value;
                                                                    else
                                                                    {
                                                                        Server.s.Log("Could not find " + value);
                                                                        return;
                                                                    }
                                                                }
                                                                Server.IRCColour = color;
                                                                break;
                                                            case "log-heartbeat":
                                                                try
                                                                {
                                                                    Server.logbeat = bool.Parse(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("Invalid " + key + ".  Using default.");
                                                                    break;
                                                                }
                                                                break;
                                                            case "notify-on-join-leave":
                                                                try
                                                                {
                                                                    Server.notifyOnJoinLeave = bool.Parse(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("Invalid " + key + ". Using default.");
                                                                    break;
                                                                }
                                                                break;
                                                            case "default-rank":
                                                                try
                                                                {
                                                                    Server.defaultRank = value.ToLower();
                                                                }
                                                                catch
                                                                {
                                                                }
                                                                break;

                                                            case "money-name":
                                                                if (value != "")
                                                                    Server.moneys = value;
                                                                break;
															case "texture-url":
																if (value != "")
																	Server.textureUrl = value;
																break;
                                                            case "restart-on-error":
                                                                try
                                                                {
                                                                    Server.restartOnError = bool.Parse(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("Invalid " + key + ". Using default.");
                                                                }
                                                                break;
                                                            case "repeat-messages":
                                                                try
                                                                {
                                                                    Server.repeatMessage = bool.Parse(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("Invalid " + key + ". Using default.");
                                                                }
                                                                break;
                                                            case "host-state":
                                                                if (value != "")
                                                                    Server.ZallState = value;
                                                                break;
                                                            case "server-owner":
                                                                if (value != "")
                                                                    Server.server_owner = value;
                                                                break;

                                                            case "mute-on-spam":
                                                                try
                                                                {
                                                                    Server.checkspam = bool.Parse(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("Invalid " + key + ". Using default");
                                                                }
                                                                break;
                                                            case "spam-messages":
                                                                try
                                                                {
                                                                    Server.spamcounter = int.Parse(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("Invalid " + key + ". Using default");
                                                                }
                                                                break;
                                                            case "spam-mute-time":
                                                                try
                                                                {
                                                                    Server.mutespamtime = int.Parse(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("Invalid " + key + ". Using default");
                                                                }
                                                                break;
                                                            case "spam-counter-reset-time":
                                                                try
                                                                {
                                                                    Server.spamcountreset = int.Parse(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("Invalid " + key + ". Using default");
                                                                }
                                                                break;
                                                            case "show-empty-ranks":
                                                                try
                                                                {
                                                                    Server.showEmptyRanks = bool.Parse(value);
                                                                }
                                                                catch
                                                                {
                                                                    Server.s.Log("Invalid " + key + ". Using default");
                                                                }
                                                                break;
                                                        }
												}
										}
										Server.s.SettingsUpdate ();
										Save (givenPath);
								} else
										Save (givenPath);
						}
				}
		public static bool ValidString(string str, string allowed) {
			string allowedchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01234567890" + allowed;
			foreach ( char ch in str ) {
				if ( allowedchars.IndexOf(ch) == -1 ) {
					return false;
				}
			} return true;
		}

		public static void Save(string givenPath) {
			try {
				File.Create(givenPath).Dispose();
				using ( StreamWriter w = File.CreateText(givenPath) ) {
					if ( givenPath.IndexOf("server") != -1 ) {
						SaveProps(w);
					}
				}
			}
			catch {
				Server.s.Log("SAVE FAILED! " + givenPath);
			}
		}
		public static void SaveProps(StreamWriter w) {
			w.WriteLine("#   Edit the settings below to modify how your server operates.");
            w.WriteLine();
			w.WriteLine("# Server options");
			w.WriteLine("server-name = " + Server.name);
			w.WriteLine("motd = " + Server.motd);
			w.WriteLine("port = " + Server.port.ToString());
			w.WriteLine("verify-names = " + Server.verify.ToString().ToLower());
			w.WriteLine("public = " + Server.pub.ToString().ToLower());
			w.WriteLine("max-players = " + Server.players.ToString());
			w.WriteLine("texture-url = " + Server.textureUrl);
			w.WriteLine("restart-on-error = " + Server.restartOnError);
			w.WriteLine("main-name = " + Server.level);
			w.WriteLine();
			w.WriteLine("# irc bot options");
			w.WriteLine("irc = " + Server.irc.ToString().ToLower());
			w.WriteLine("irc-colorsenable = " + Server.ircColorsEnable.ToString().ToLower());
			w.WriteLine("irc-nick = " + Server.ircNick);
			w.WriteLine("irc-server = " + Server.ircServer);
			w.WriteLine("irc-channel = " + Server.ircChannel);
			w.WriteLine("irc-opchannel = " + Server.ircOpChannel);
			w.WriteLine("irc-port = " + Server.ircPort.ToString());
			w.WriteLine("irc-identify = " + Server.ircIdentify.ToString());
			w.WriteLine("irc-password = " + Server.ircPassword);
			w.WriteLine();
			w.WriteLine("# other options");
			w.WriteLine("deathcount = " + Server.deathcount.ToString().ToLower());
			w.WriteLine("money-name = " + Server.moneys);
			w.WriteLine("log-heartbeat = " + Server.logbeat.ToString());
			w.WriteLine("notify-on-join-leave = " + Server.notifyOnJoinLeave.ToString());
			w.WriteLine("repeat-messages = " + Server.repeatMessage.ToString());
			w.WriteLine("host-state = " + Server.ZallState.ToString());
			w.WriteLine("server-owner = " + Server.server_owner.ToString());
			w.WriteLine();
			w.WriteLine("#Colors");
			w.WriteLine("defaultColor = " + Server.DefaultColor);
			w.WriteLine("irc-color = " + Server.IRCColour);
			w.WriteLine();
			try { w.WriteLine("default-rank = " + Server.defaultRank); }
			catch { w.WriteLine("default-rank = guest"); }
			w.WriteLine();
			w.WriteLine("#Spam Control");
			w.WriteLine("mute-on-spam = " + Server.checkspam.ToString().ToLower());
			w.WriteLine("spam-messages = " + Server.spamcounter.ToString());
			w.WriteLine("spam-mute-time = " + Server.mutespamtime.ToString());
			w.WriteLine("spam-counter-reset-time = " + Server.spamcountreset.ToString());
			w.WriteLine();
			w.WriteLine("#Show Empty Ranks in /players");
			w.WriteLine("show-empty-ranks = " + Server.showEmptyRanks.ToString().ToLower());
		}
	}
}