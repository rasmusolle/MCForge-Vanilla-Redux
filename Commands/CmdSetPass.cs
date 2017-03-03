/*
 
    Copyright 2012 MCForge
        
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
using MCForge.Util;
namespace MCForge.Commands
{
    public class CmdSetPass : Command
    {
        public override string name { get { return "setpass"; } }
        public override string shortcut { get { return  ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdSetPass() { }

        public override void Use(Player p, string message)
        {
            if (!Directory.Exists("extra/passwords"))
                Directory.CreateDirectory("extra/passwords");
            if (p.group.Permission < Server.verifyadminsrank)
            {
                Player.SendMessage(p, "You do not have the &crequired rank " + Server.DefaultColor + "to use this command!");
                return;
            }
            if (!Server.verifyadmins)
            {
                Player.SendMessage(p, "Verification of admins is &cdisabled!");
                return;
            }
            if (p.adminpen)
            {
                if (File.Exists("extra/passwords/" + p.name + ".dat"))
                {
                    Player.SendMessage(p, "&cYou already have a password set. " + Server.DefaultColor + "You &ccannot change " + Server.DefaultColor + "it unless &cyou verify it with &a/pass [Password]. " + Server.DefaultColor + "If you have &cforgotten " + Server.DefaultColor + "your password, contact &c" + Server.server_owner + Server.DefaultColor + " and they can &creset it!");
                    return;
                }
            }
            if (String.IsNullOrEmpty(message.Trim()))
            {
                Help(p);
                return;

            }
            int number = message.Split(' ').Length;
            if (number > 1)
            {
                Player.SendMessage(p, "Your password must be one word!");
                return;
            }
            PasswordHasher.StoreHash(p.name, message);
            Player.SendMessage(p, "Your password has &asuccessfully &abeen set to:");
            Player.SendMessage(p, "&c" + message);
        }

        public override void Help(Player p)
        {
            Player.SendMessage(p, "/setpass [Password] - Sets your admin password to [password].");
            Player.SendMessage(p, "Note: Do NOT set this as your Minecraft password!");
        }
    }
}
