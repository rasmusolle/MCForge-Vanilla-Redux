﻿/*
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
namespace MCSpleef.Commands
{
	public class CmdRank : Command
	{
		public override string name { get { return "rank"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
		public override void Use(Player p, string message)
		{
			if (message.Split(' ').Length < 2) { Help(p); return; }
			Player who = Player.Find(message.Split(' ')[0]);
			Group newRank = Group.Find(message.Split(' ')[1]);

			if (newRank == null) { Player.SendMessage(p, "Could not find specified rank."); return; }

			Group bannedGroup = Group.findPerm(LevelPermission.Banned);
			if (who == null)
			{
				string foundName = message.Split(' ')[0];
				if (Group.findPlayerGroup(foundName) == bannedGroup || newRank == bannedGroup)
				{
					Player.SendMessage(p, "Cannot change the rank to or from \"" + bannedGroup.name + "\".");
					return;
				}

				if (p != null)
				{
					if (Group.findPlayerGroup(foundName).Permission >= p.group.Permission || newRank.Permission >= p.group.Permission)
					{
						Player.SendMessage(p, "Cannot change the rank of someone equal or higher than you"); return;
					}
				}

				Group oldGroup = Group.findPlayerGroup(foundName);
				oldGroup.playerList.Remove(foundName);
				oldGroup.playerList.Save();

				newRank.playerList.Add(foundName);
				newRank.playerList.Save();

				Player.GlobalMessage(foundName + " &f(offline)" + Server.DefaultColor + "'s rank was set to " + newRank.color + newRank.name);
			}
			else if (who == p)
			{
				Player.SendMessage(p, "Cannot change your own rank."); return;
			}
			else
			{
				if (p != null)
				{
					if (who.group == bannedGroup || newRank == bannedGroup)
					{
						Player.SendMessage(p, "Cannot change the rank to or from \"" + bannedGroup.name + "\".");
						return;
					}

					if (who.group.Permission >= p.group.Permission || newRank.Permission >= p.group.Permission)
					{
						Player.SendMessage(p, "Cannot change the rank of someone equal or higher to yourself."); return;
					}
				}

				who.group.playerList.Remove(who.name);
				who.group.playerList.Save();

				newRank.playerList.Add(who.name);
				newRank.playerList.Save();

				Player.GlobalChat(who, who.color + who.name + Server.DefaultColor + "'s rank was set to " + newRank.color + newRank.name, false);
				who.group = newRank;
				who.color = who.group.color;
				Player.GlobalDie(who, false);
				who.SendMessage("You are now ranked " + newRank.color + newRank.name + Server.DefaultColor + ".");
				Player.GlobalSpawn(who, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1], false);
			}
		}
		public override void Help(Player p)
		{
			Player.SendMessage(p, "/rank <player> <rank> - Sets a players rank.");
		}
	}
}