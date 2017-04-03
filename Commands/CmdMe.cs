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
	public class CmdMe : Command
	{
		public override string name { get { return "me"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
		public override void Use(Player p, string message)
		{
			Player.GlobalChat(p, p.color + "*" + p.name + " " + message, false);
			Server.IRC.Say("*" + p.name + " " + message);
		}
		public override void Help(Player p)
		{
			Command.all.Find("award").Use(p, p.name + " Needing Help");
			Player.SendMessage(p, "What do you need help with, m'boy?! Are you stuck down a well?!");
		}
	}
}