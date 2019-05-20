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
	public class CmdKick : Command {
		public override string name { get { return "kick"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
		public override void Use(Player p, string message) {
			if (message == "") { Help(p); return; }
			Player who = Player.Find(message.Split(' ')[0]);
			if (who == null) { Player.SendMessage(p, "Could not find player specified."); return; }
			if (message.Split(' ').Length > 1)
				message = message.Substring(message.IndexOf(' ') + 1);
			else
				if (p == null)
					message = "You were kicked!";
				else
					message = "You were kicked by " + p.name + "!";

			if (p != null)
				if (who == p) {
					Player.SendMessage(p, "You cannot kick yourself!");
					return;
				} else if (who.group.Permission >= p.group.Permission && p != null) {
					Player.GlobalChat(p, p.color + p.name + Server.DefaultColor + " tried to kick " + who.color + who.name + " but failed.", false);
					return;
				}

			who.Kick(message);
		}
		public override void Help(Player p) {
			Player.SendMessage(p, "/kick <player> [message] - Kicks a player.");
		}
	}
}