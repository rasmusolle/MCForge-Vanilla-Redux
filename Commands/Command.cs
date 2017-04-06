/*
	Copyright 2009 MCSharp team (Modified for use with MCZall/MCLawl/MCSpleef)
	
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
using MCForge.Commands;
namespace MCForge
{
	public abstract class Command
	{
		public abstract string name { get; }
		public abstract LevelPermission defaultRank { get; }
		public abstract void Use(Player p, string message);
		public abstract void Help(Player p);
		public bool isIntervalized;
		public int intervalInMinutes;
		public DateTime nextExecution;
		public Player intervalUsingPlayer;

		public static CommandList all = new CommandList();
		public static CommandList core = new CommandList();
		public static void InitAll()
		{
			all.Add(new CmdBan());
			all.Add(new CmdBanip());
			all.Add(new CmdColor());
			all.Add(new CmdCuboid());
			all.Add(new CmdEco());
			all.Add(new CmdGamble());
			all.Add(new CmdHelp());
			all.Add(new CmdInfo());
			all.Add(new CmdIrc());
			all.Add(new CmdKick());
			all.Add(new CmdMe());
			all.Add(new CmdMute());
			all.Add(new CmdNewLvl());
			all.Add(new CmdPlayers());
			all.Add(new CmdRestart());
			all.Add(new CmdRules());
			all.Add(new CmdSave());
			all.Add(new CmdSay());
			all.Add(new CmdServerReport());
			all.Add(new CmdRank());
			all.Add(new CmdSetspawn());
			all.Add(new CmdSpawn());
			all.Add(new CmdTitle());
			all.Add(new CmdUnban());
			all.Add(new CmdUnbanip());
			all.Add(new CmdView());
			all.Add(new CmdViewRanks());
			all.Add(new CmdWhoip());
			all.Add(new CmdWhois());
			all.Add(new CmdWhowas());
			all.Add(new CmdXban());
			all.Add(new CmdAward());
			all.Add(new CmdAwards());

			core.commands = new List<Command>(all.commands);
		}
	}
}