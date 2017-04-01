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
using System.IO;

namespace MCForge.Commands
{
    public class CmdInfo : Command
    {
        public override string name { get { return "info"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public override void Use(Player p, string message)
        {
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

            if (Server.irc)
                Player.SendMessage(p, Server.IRCColour + "Irc is &aEnabled " + Server.IRCColour + "(" + Server.ircChannel + " @ " + Server.ircServer + ")");
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/info - Displays information about the server.");
        }
    }
}
