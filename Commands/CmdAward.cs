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
namespace MCForge.Commands
{
	public class CmdAward : Command
	{
		public override string name { get { return "award"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
		public override void Use(Player p, string message)
		{
			if (message == "" || message.IndexOf(' ') == -1) { Help(p); return; }
			
			string foundPlayer = message.Split(' ')[0];
			Player who = Player.Find(message);
			if (who != null) foundPlayer = who.name;
			string awardName = message.Substring(message.IndexOf(' ') + 1);
			if (!Awards.awardExists(awardName))
			{
				Player.SendMessage(p, "The award you entered doesn't exist");
				Player.SendMessage(p, "Use /awards for a list of awards");
				return;
			}

			if (Awards.giveAward(foundPlayer, awardName)) 
				Player.GlobalChat(p, Server.FindColor(foundPlayer) + foundPlayer + Server.DefaultColor + " was awarded: &b" + Awards.camelCase(awardName), false);
			else 
				Player.SendMessage(p, "The player already has that award!");

			Awards.Save();
		}
		public override void Help(Player p)
		{
			Player.SendMessage(p, "/award [player] [award] - Gives [player] the [award]");
			Player.SendMessage(p, "[award] needs to be the full award's name. Not partial");
		}
	}
}