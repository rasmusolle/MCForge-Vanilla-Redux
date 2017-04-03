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
using System.Collections.Generic;
namespace MCForge.Commands
{
	class CmdPlayers : Command
	{
		struct groups { public Group group; public List<string> players; }

		public override string name { get { return "players"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
		public override void Use(Player p, string message)
		{
			try
			{
				List<groups> playerList = new List<groups>();

				foreach (Group grp in Group.GroupList)
				{
					if (grp.name != "nobody")
					{
						groups groups;
						groups.group = grp;
						groups.players = new List<string>();
						playerList.Add(groups);
					}
				}

				int totalPlayers = 0;
				foreach (Player pl in Player.players)
				{
					totalPlayers++;
					string foundName = pl.name;
					playerList.Find(grp => grp.group == pl.group).players.Add(foundName);
				}
				if (totalPlayers != 1)
					Player.SendMessage(p, "There are " + totalPlayers + " players online.");
				else
					Player.SendMessage(p, "There is " + totalPlayers + " player online.");

				for (int i = playerList.Count - 1; i >= 0; i--)
				{
					groups groups = playerList[i];
					string appendString = "";

					foreach (string player in groups.players) { appendString += ", " + player; }

					if (appendString != "")
						appendString = appendString.Remove(0, 2);
					appendString = ":" + groups.group.color + Extensions.getPlural(groups.group.trueName) + ": " + appendString;

					Player.SendMessage(p, appendString);
				}
			}
			catch (Exception e) { Server.ErrorLog(e); }
		}
		public override void Help(Player p)
		{
			Player.SendMessage(p, "/players - Displays a list of players online.");
		}
	}
}
