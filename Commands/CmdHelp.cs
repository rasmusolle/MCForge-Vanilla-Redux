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
using System;
namespace MCSpleef.Commands {
	public class CmdHelp : Command {
		public override string name { get { return "help"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
		public override void Use(Player p, string message) {
			message.ToLower();
			switch (message) {
				case "":
					Player.SendMessage(p, "Welcome $name to SPLEEF!");
					Player.SendMessage(p, "%9============================================");
					Player.SendMessage(p, "What is spleef? Well, Spleef is a game where");
					Player.SendMessage(p, "you break the ground under the opponent.");
					Player.SendMessage(p, "Yeah. It's that simple. Now get down and play");
					Player.SendMessage(p, "some SPLEEF!");
					Player.SendMessage(p, "%9============================================");
					Player.SendMessage(p, "%aDo you want something else maybe?");
					Player.SendMessage(p, "Do &b/help commands" + Server.DefaultColor + " for a list of commands.");
					Player.SendMessage(p, "Use &b/help [command] " + Server.DefaultColor + "to view more info about that command.");
				break;
				case "ranks":
					message = "";
					foreach (Group grp in Group.GroupList) {
						if (grp.name != "nobody")
							Player.SendMessage(p, grp.color + grp.name + " - &cPermission: " + (int)grp.Permission);
					}
				break;
				case "commands":
					string commandsFound = "";
					foreach (Command comm in Command.all.commands) {
						if (p == null || p.group.commands.All().Contains(comm)) {
							try { commandsFound += ", " + getColor(comm.name) + getColor(comm.name) + comm.name; } catch { }
						}
					}
					Player.SendMessage(p, "Available commands:");
					Player.SendMessage(p, commandsFound.Remove(0, 2));
					Player.SendMessage(p, "Type \"/help <command>\" for more help about that command.");
				break;
				default:
					Command cmd = Command.all.Find(message);
					if (cmd != null) {
						cmd.Help(p);
						string foundRank = Level.PermissionToName(GrpCommands.allowedCommands.Find(grpComm => grpComm.commandName == cmd.name).lowestRank);
						Player.SendMessage(p, "Rank needed: " + getColor(cmd.name) + foundRank);
						return;
					}
					Player.SendMessage(p, "Could not find command specified.");
				break;
			}
		}

		private string getColor(string commName) {
			foreach (GrpCommands.rankAllowance aV in GrpCommands.allowedCommands) {
				if (aV.commandName == commName) {
					if (Group.findPerm(aV.lowestRank) != null)
						return Group.findPerm(aV.lowestRank).color;
				}
			}

			return "&f";
		}

		public override void Help(Player p) {
			Command.all.Find("award").Use(p, p.name + " YOU STUPID");
			Player.SendMessage(p, "YOU ARE AN IDIOT");
			Player.SendMessage(p, "HAHAHAHAHAHAHAHAHA");
			System.Threading.Thread.Sleep(4000);
			p.Kick("You've been kicked for being an idiot.");
			System.Threading.Thread.Sleep(2000);
			Player.GlobalMessage(p.color + p.name + Server.DefaultColor + " needed help for the command help.");
			System.Threading.Thread.Sleep(2000);
			Player.GlobalMessage(Server.DefaultColor + "He's been kicked for being stupid.");

		}
	}
}