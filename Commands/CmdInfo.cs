/*
	Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl) Licensed under the
	Educational Community License, Version 2.0 (the "License"); you may
	not use this file except in compliance with the License. You may
	obtain a copy of the License at
	
	http://www.osedu.org/licenses/ECL-2.0
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the License is distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the License for the specific language governing
	permissions and limitations under the License.
*/
using System;

namespace MCForge.Commands
{
    public class CmdInfo : Command
    {
        public override string name { get { return "info"; } }
        public override string shortcut { get { return ""; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public override void Use(Player p, string message)
        {
            Player.SendMessage(p, "This server runs on &bMCSpleef" + Server.DefaultColor + ", which is a custom server software based off of &bMCLawl");
            Player.SendMessage(p, "This server's version: &a" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Player.SendMessage(p, "Currently $banned people are %0banned.");
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/info - Displays the server information.");
        }
    }
}
