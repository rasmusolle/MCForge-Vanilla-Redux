/*
	Copyright 2011-2014 MCForge-Redux (Modified for use with MCSpleef)
		
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
using System.IO;

namespace MCForge
{
    public static class Economy
    {
        public struct EcoStats {
            public string playerName, purchase, payment;
            public int money, totalSpent;
            public EcoStats(string name, int mon, int tot, string pur, string pay) {
                playerName = name;
                money = mon;
                totalSpent = tot;
                purchase = pur;
                payment = pay;
            }
        }

        public static class Settings {
            public static bool Enabled = false;

            public static bool Titles = false;
            public static int TitlePrice = 100;

            public static bool Colors = false;
            public static int ColorPrice = 100;

            public static bool TColors = false;
            public static int TColorPrice = 100;
        }

        public static void LoadDatabase() { }

        public static void Load(bool loadDatabase = false) {

		if (!File.Exists("properties/economy.properties")) { Server.s.Log("Economy properties don't exist, creating"); File.Create("properties/economy.properties").Close(); Save(); }
			using (StreamReader r = File.OpenText("properties/economy.properties")) {
				string line;
				while (!r.EndOfStream) {
					line = r.ReadLine().ToLower().Trim();
					string[] linear = line.ToLower().Trim().Split(':');
					try {
						switch (linear[0]) {
							case "enabled":
								if (linear[1] == "true") { Settings.Enabled = true; } else if (linear[1] == "false") { Settings.Enabled = false; }
								break;

							case "title":
								if (linear[1] == "price") { Settings.TitlePrice = int.Parse(linear[2]); }
								if (linear[1] == "enabled") {
									if (linear[2] == "true") { Settings.Titles = true; } else if (linear[2] == "false") { Settings.Titles = false; }
								}
								break;

							case "color":
								if (linear[1] == "price") { Settings.ColorPrice = int.Parse(linear[2]); }
								if (linear[1] == "enabled") {
									if (linear[2] == "true") { Settings.Colors = true; } else if (linear[2] == "false") { Settings.Colors = false; }
								}
								break;
							case "titlecolor":
								if (linear[1] == "price") { Settings.TColorPrice = int.Parse(linear[2]); }
								if (linear[1] == "enabled") {
									if (linear[2] == "true") { Settings.TColors = true; } else if (linear[2] == "false") { Settings.TColors = false; }
								}
								break;
						}
					} catch { }
				}
				r.Close();
			}
			Save();
		}

		public static void Save() 
		{
			if (!File.Exists("properties/economy.properties")) { Server.s.Log("Economy properties don't exist, creating"); }
				File.Delete("properties/economy.properties");
				using (StreamWriter w = File.CreateText("properties/economy.properties")) {

					w.WriteLine("enabled:" + Settings.Enabled);

					w.WriteLine();
					w.WriteLine("title:enabled:" + Settings.Titles);
					w.WriteLine("title:price:" + Settings.TitlePrice);

					w.WriteLine();
					w.WriteLine("color:enabled:" + Settings.Colors);
					w.WriteLine("color:price:" + Settings.ColorPrice);

					w.WriteLine();
					w.WriteLine("titlecolor:enabled:" + Settings.TColors);
					w.WriteLine("titlecolor:price:" + Settings.TColorPrice);

					w.Close();
				}
		}


		public static EcoStats RetrieveEcoStats(string playername) {
			EcoStats es = new EcoStats();
			es.playerName = playername;
			EconomyDB.Load (es);
			return es;
		}

		public static void UpdateEcoStats(EcoStats es) { EconomyDB.Save(es); }
	}
}
