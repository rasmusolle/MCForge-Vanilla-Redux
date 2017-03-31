/*
	Copyright 2010 MCLawl Team - Written by Valek
 
    Licensed under the
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
    public class CmdIrc : Command
    {
        public override string name { get { return "irc"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public override void Use(Player p, string message)
        {
            string hasirc;
            string ircdetails = "";
            if (Server.irc)
            {
                hasirc = "&aEnabled" + Server.DefaultColor + ".";
                ircdetails = Server.ircServer + " > " + Server.ircChannel;
            }
            else
            {
                hasirc = "&cDisabled" + Server.DefaultColor + ".";
            }
            Player.SendMessage(p, "IRC is " + hasirc);
            if (ircdetails != "")
            {
                Player.SendMessage(p, "Location: " + ircdetails);
            }
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/irc - Displays the server and channel information.");
        }
    }
}