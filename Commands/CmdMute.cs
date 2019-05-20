/*
	Copyright 2010 MCLawl team (Modified for use with MCSpleef)
	
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
	public class CmdMute : Command {
		public override string name { get { return "mute"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
		public override void Use(Player p, string message) {
			if (message == "" || message.Split(' ').Length > 2) { Help(p); return; }
			Player who = Player.Find(message);
			if (who == null) {
				Player.SendMessage(p, "The player entered is not online.");
				return;
			}
			if (who.muted) {
				who.muted = false;
				Player.GlobalChat(null, who.color + who.name + Server.DefaultColor + " has been &bun-muted", false);
			} else {
				if (p != null) {
					if (who != p) if (who.group.Permission > p.group.Permission) { Player.SendMessage(p, "Cannot mute someone of a higher rank."); return; }
				}
				who.muted = true;
				Player.GlobalChat(null, who.color + who.name + Server.DefaultColor + " has been &8muted", false);
			}
		}
		public override void Help(Player p) {
			Player.SendMessage(p, "/mute <player> - Mutes or unmutes the player.");
		}
	}
}