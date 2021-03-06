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
using System.IO;
namespace MCSpleef.Commands {
	public class CmdInfo : Command {
		public override string name { get { return "info"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
		public override void Use(Player p, string message) {
			Player.SendMessage(p, "You're on \"" + Server.name + "\" with the MOTD " + Server.motd + ".");
			//Player.SendMessage(p, "&d" + Player.players.Count + Server.DefaultColor + " players are currently online.");
			Player.SendMessage(p, "This server runs on &bMCSpleef" + Server.DefaultColor + ", which was originally based off of &bMCLawl" + Server.DefaultColor + ", but was remade as a MCForge fork.");
			Player.SendMessage(p, "This server's version: &a" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
			Player.SendMessage(p, "The server time is " + DateTime.Now.ToString("HH:mm") + ".");

			int count = Directory.GetFiles("players/", "*.txt", SearchOption.TopDirectoryOnly).Length;
			if (count != 1)
				Player.SendMessage(p, "A total of " + count + " unique players have visited this server.");
			else
				Player.SendMessage(p, "A total of " + count + " unique player have visited this server.");
			Player.SendMessage(p, "Currently $banned people are %0banned.");
		}
		public override void Help(Player p) {
			Player.SendMessage(p, "/info - Displays information about the server.");
		}
	}
}