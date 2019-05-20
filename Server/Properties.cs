/*
	Copyright 2009 MCSharp team (Modified for use with MCZall/MCLawl/MCForge/MCForge-Redux/MCSpleef)

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
using System.Security.Cryptography;
using System.Text;
namespace MCSpleef {
	public static class SrvProperties {
		public static void Load(string givenPath) {
			bool gotSalt = false;
			if (!gotSalt) {
				RandomNumberGenerator prng = RandomNumberGenerator.Create();
				StringBuilder sb = new StringBuilder();
				byte[] oneChar = new byte[1];
				while (sb.Length < 16) {
					prng.GetBytes(oneChar);
					if (Char.IsLetterOrDigit((char)oneChar[0])) { sb.Append((char)oneChar[0]); }
				}
				Server.salt = sb.ToString();
			}

			if (File.Exists(givenPath)) {
				string[] lines = File.ReadAllLines(givenPath);
				foreach (string line in lines) {
					if (line != "" && line [0] != '#') continue;
					string key = line.Split('=')[0].Trim();
					string value = "";
					if (line.IndexOf('=') >= 0)
						value = line.Substring(line.IndexOf('=') + 1).Trim(); // allowing = in the values

					switch (key.ToLower()) {
						case "server-name":
							if (ValidString(value, "![]:.,{}~-+()?_/\\' ")) { Server.name = value; }
							else { Server.s.Log("Server name invalid."); }
						break;
						case "motd":
							if (ValidString(value, "=![]&:.,{}~-+()?_/\\' ")) { Server.motd = value; }
						else { Server.s.Log("MOTD invalid."); }
						break;
						case "port":
							try { Server.port = Convert.ToInt32(value); }
							catch { Server.s.Log("Port invalid."); }
						break;
						case "verify-names":
							Server.verify = (value.ToLower() == "true") ? true : false;
						break;
						case "public":
							Server.pub = (value.ToLower() == "true") ? true : false;
						break;
						case "max-players":
							try {
							if (Convert.ToByte(value) > 128) { value = "128"; }
								else if (Convert.ToByte(value) < 1) { value = "1"; }
								Server.players = Convert.ToByte(value);
							} catch { Server.s.Log("max-players invalid"); }
						break;
						case "default-rank":
							try { Server.defaultRank = value.ToLower(); }
							catch { }
						break;

						case "texture-url":
							if (value != "") { Server.textureUrl = value; }
						break;
					}
				}
			Server.s.SettingsUpdate();
			Save(givenPath);
			} else Save(givenPath);
		}
		public static bool ValidString(string str, string allowed) {
			string allowedchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01234567890" + allowed;
			foreach (char ch in str) {
				if (allowedchars.IndexOf(ch) == -1) { return false; }
			} return true;
		}
		public static void Save(string givenPath) {
			try {
				File.Create(givenPath).Dispose();
				using (StreamWriter w = File.CreateText(givenPath)) {
					if (givenPath.IndexOf("server") != -1) { SaveProps(w); }
				}
			}
			catch { Server.s.Log("SAVE FAILED! " + givenPath); }
		}
		public static void SaveProps(StreamWriter w) {
			w.WriteLine("# Edit the settings below to modify how your server operates.");
			w.WriteLine();
			w.WriteLine("# Server options");
			w.WriteLine("server-name = " + Server.name);
			w.WriteLine("motd = " + Server.motd);
			w.WriteLine("port = " + Server.port.ToString());
			w.WriteLine("verify-names = " + Server.verify.ToString().ToLower());
			w.WriteLine("public = " + Server.pub.ToString().ToLower());
			w.WriteLine("max-players = " + Server.players.ToString());
			w.WriteLine("texture-url = " + Server.textureUrl);
			w.WriteLine();
			w.WriteLine("# other options");
			w.WriteLine("deathcount = " + Server.deathcount.ToString().ToLower());
			w.WriteLine("host-state = " + Server.ZallState.ToString());
			w.WriteLine();
			try { w.WriteLine("default-rank = " + Server.defaultRank); }
			catch { w.WriteLine("default-rank = guest"); }
		}
	}
}