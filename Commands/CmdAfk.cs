/*
	Copyright � 2009-2014 MCSharp team (Modified for use with MCZall/MCLawl/MCForge/MCForge-Redux)
	
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
    /// <summary>
    /// This is the command /afk
    /// use /help afk in-game for more info
    /// </summary>
    public class CmdAfk : Command
    {
        public override string name { get { return "afk"; } }
        public override string shortcut { get { return  ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public static string keywords { get { return ""; } }
        public CmdAfk() { }

        public override void Use(Player p, string message)
        {
            if (p != null)
            {

                if (Server.chatmod) {
                    Player.SendMessage(p, "You cannot use /afk while chat moderation is enabled");
                    return;
                }

                if (message != "list")
                {
                    if (p.joker)
                    {
                        message = "";
                    }
                    if (!Server.afkset.Contains(p.name))
                    {
                        Server.afkset.Add(p.name);
                        if (p.muted)
                        {
                            message = "";
                        }
                        Player.GlobalMessage("-" + p.color + p.name + Server.DefaultColor + "- is AFK " + message);
                        //IRCBot.Say(p.name + " is AFK " + message);
                        Server.IRC.Say(p.name + " is AFK " + message);
                        p.afkStart = DateTime.Now;
                        return;

                    }
                    else
                    {
                        Server.afkset.Remove(p.name);
                        Player.GlobalMessage("-" + p.color + p.name + Server.DefaultColor + "- is no longer AFK");
                        //IRCBot.Say(p.name + " is no longer AFK");
                        Server.IRC.Say(p.name + " is no longer AFK");
                        return;
                    }
                }
                else
                {
                    foreach (string s in Server.afkset) Player.SendMessage(p, s);
                    return;
                }
            }
            Player.SendMessage(p, "This command can only be used in-game");
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/afk <reason> - mark yourself as AFK. Use again to mark yourself as back");
        }
    }
}