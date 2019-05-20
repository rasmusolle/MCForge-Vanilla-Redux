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
namespace MCSpleef.Commands {
	class CmdEco : Command {
		public override string name { get { return "eco"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
		public override void Use(Player p, string message) {
			string[] command = message.Trim().Split(' ');
			string par0 = String.Empty;
			string par1 = String.Empty;
			string par2 = String.Empty;
			try {
				par0 = command[0].ToLower();
				par1 = command[1].ToLower();
				par2 = command[2];
			} catch { }

			if (par0 == String.Empty) { Help(p); return; }

			switch (par0) {
				case "give":
					if (p.group.Permission >= LevelPermission.Operator) {
						if (message.IndexOf(' ') == -1) { Help(p); return; }
						if (message.Split(' ').Length != 3) { Help(p); return; }

						Player givep = Player.Find(par1);
						if (givep == null) { Player.SendMessage(p, "Could not find player entered"); return; }
						//if (givep == p) { Player.SendMessage(p, "You can't give yourself money."); return; }

						int amountGiven;
						try { amountGiven = int.Parse(par2); } catch { Player.SendMessage(p, "Invalid amount"); return; }

						if (givep.money + amountGiven > 16777215) { Player.SendMessage(p, "Invalid amount"); return; }
						if (amountGiven < 0) { Player.SendMessage(p, "Cannot give someone negative " + Server.moneys); return; }

						givep.money += amountGiven;
						Player.GlobalMessage(givep.color + givep.prefix + givep.name + Server.DefaultColor + " was given " + amountGiven + " " + Server.moneys);
					} else {
						Player.SendMessage(p, "This command's only for operators and higher.");
						return;
					}
					break;
				case "pay":
					if (message.IndexOf(' ') == -1) { Help(p); return; }
					if (message.Split(' ').Length != 3) { Help(p); return; }

					Player payp = Player.Find(par1);
					if (payp == null) { Player.SendMessage(p, "Could not find player entered"); return; }
					if (payp == p) { Player.SendMessage(p, "You can't pay yourself."); return; }

					int amountPaid;
					try { amountPaid = int.Parse(par2); } catch { Player.SendMessage(p, "Invalid amount"); return; }

					if (payp.money + amountPaid > 16777215) { Player.SendMessage(p, "Invalid amount"); return; }
					if (p.money - amountPaid < 0) { Player.SendMessage(p, "You don't have that much " + Server.moneys); return; }
					if (amountPaid < 0) { Player.SendMessage(p, "Cannot pay negative " + Server.moneys); return; }

					payp.money += amountPaid;
					p.money -= amountPaid;
					Player.GlobalMessage(p.color + p.name + Server.DefaultColor + " paid " + payp.color + payp.name + Server.DefaultColor + " " + amountPaid + " " + Server.moneys);
					break;
				case "take":
					if (message.IndexOf(' ') == -1) { Help(p); return; }
					if (message.Split(' ').Length != 3) { Help(p); return; }

					Player takep = Player.Find(par1);
					if (takep == null) { Player.SendMessage(p, "Could not find player entered"); return; }
					if (takep == p) { Player.SendMessage(p, "Sorry. Can't allow you to take money from yourself"); return; }

					int amountTaken;
					try { amountTaken = int.Parse(par2); } catch { Player.SendMessage(p, "Invalid amount"); return; }

					if (takep.money - amountTaken < 0) { Player.SendMessage(p, "Invalid amount"); return; }
					if (amountTaken < 0) { Player.SendMessage(p, "Cannot take negative " + Server.moneys); return; }

					takep.money -= amountTaken;
					Player.GlobalMessage(takep.color + takep.prefix + takep.name + Server.DefaultColor + " was rattled down for " + amountTaken + " " + Server.moneys);
					break;
				case "money":
					if (Player.players.Count != 1)
						Player.SendMessage(p, "Money of currently active players:");
					else
						Player.SendMessage(p, "Money of currently active player:");
					foreach (Player pl in Player.players) {
						Player.SendMessage(p, pl.name + " - " + pl.money + " " + Server.moneys);
					}
					break;
				default:
					Help(p);
					return;

			}
		}
		public override void Help(Player p) {
			Player.SendMessage(p, "/eco give [player] <amount> - Gives [player] <amount> " + Server.moneys);
			Player.SendMessage(p, "/eco pay [player] <amount> - Pays <amount> of " + Server.moneys + " to [player]");
			Player.SendMessage(p, "/eco take [player] <amount> - Takes <amount> of " + Server.moneys + " from [player]");
			Player.SendMessage(p, "/eco money - Displays money of all players online");
		}
	}
}