/*
	Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCSpleef)
	
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
//TODO: Cleanup this
using System;
using System.Collections.Generic;
namespace MCSpleef.Commands {
	public class CmdAwards : Command {
		public override string name { get { return "awards"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
		public override void Use(Player p, string message) {
			if (message.Split(' ').Length > 1) { Help(p); return; }

			int totalCount = 0;
			string foundPlayer = "";

			if (message != "") {
				if (message.Split(' ').Length == 2) {
					foundPlayer = message.Split(' ')[0];
					Player who = Player.Find(foundPlayer);
					if (who != null)
						foundPlayer = who.name;
					try { totalCount = int.Parse(message.Split(' ')[1]); } catch { Help(p); return; }
				} else {
					if (message.Length <= 3) {
						try { totalCount = int.Parse(message); } catch {
							foundPlayer = message;
							Player who = Player.Find(foundPlayer);
							if (who != null)
								foundPlayer = who.name;
						}
					} else {
						foundPlayer = message;
						Player who = Player.Find(foundPlayer);
						if (who != null)
							foundPlayer = who.name;
					}
				}
			}


			List<Awards.awardData> awardList = new List<Awards.awardData>();
			if (foundPlayer == "") { awardList = Awards.allAwards; } else {
				foreach (string s in Awards.getPlayersAwards(foundPlayer)) {
					Awards.awardData aD = new Awards.awardData();
					aD.awardName = s;
					aD.description = Awards.getDescription(s);
					awardList.Add(aD);
				}
			}

			if (awardList.Count == 0) { Player.SendMessage(p, "No awards found."); return; }

			if (foundPlayer != "") { Player.SendMessage(p, Server.FindColor(foundPlayer) + foundPlayer + Server.DefaultColor + " has the following awards:"); } else { Player.SendMessage(p, "Awards available: "); }

			foreach (Awards.awardData aD in awardList) { Player.SendMessage(p, "&6" + aD.awardName + ": &7" + aD.description); }
		}
		public override void Help(Player p) {
			Player.SendMessage(p, "/awards [player] - Gives a full list of awards");
			Player.SendMessage(p, "If [player] is specified, shows awards for that player");
		}
	}
}