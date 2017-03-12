/*
	Copyright © 2009-2014 MCSharp team (Modified for use with MCZall/MCLawl/MCForge/MCForge-Redux)
	
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
using System.Data;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace MCForge.Commands
{
    public class CmdBanip : Command
    {
        public override string name { get { return "banip"; } }
        public override string shortcut { get { return "bi"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override void Use(Player p, string message)
        {
            if (String.IsNullOrEmpty(message.Trim())) { Help(p); return; }
            if (!Regex.IsMatch(message.ToLower(), @".*%([0-9]|[a-f]|[k-r])%([0-9]|[a-f]|[k-r])%([0-9]|[a-f]|[k-r])"))
            {
                if (Regex.IsMatch(message.ToLower(), @".*%([0-9]|[a-f]|[k-r])(.+?).*"))
                {
                    Regex rg = new Regex(@"%([0-9]|[a-f]|[k-r])(.+?)");
                    MatchCollection mc = rg.Matches(message.ToLower());
                    if (mc.Count > 0)
                    {
                        Match ma = mc[0];
                        GroupCollection gc = ma.Groups;
                        message.Replace("%" + gc[1].ToString().Substring(1), "&" + gc[1].ToString().Substring(1));
                    }
                }
            }
            string name = "";
            if (message[0] == '@')
            {
                message = message.Remove(0, 1).Trim();
                Player who = Player.Find(message);

                // If player isn't online, search the DB and copy over the IP to banned-ip.txt
                if (who == null)
                {
                    if (File.Exists("players/" + message + "DB.txt"))
                    {
                        string offlineIP = "0.0.0.0";
                        foreach (string line in File.ReadAllLines("players/" + message + "DB.txt"))
                        {
                            if (!string.IsNullOrEmpty(line) && !line.StartsWith("#"))
                            {
                                string key = line.Split('=')[0].Trim();
                                string value = line.Split('=')[1].Trim();
                                string section = "nowhere yet...";

                                try
                                {
                                    switch (key.ToLower())
                                    {
                                        case "ip":
                                            offlineIP = value;
                                            section = key;
                                            break;
                                    }

                                }
                                catch (Exception e)
                                {
                                    Server.s.Log("Loading IP from DB failed. (/banip)");
                                    Server.ErrorLog(e);
                                }
                            }
                        }
                        Server.bannedIP.Add(offlineIP);
                        Server.bannedIP.Save("banned-ip.txt", false);

                        goto msg;
                    }
                    else
                    {
                        Player.SendMessage(p, "User not found in the DB.");
                        return;
                    }
                }
                else
                {
                    name = who.name.ToLower();
                    message = who.ip;
                }
            }
            else
            {
                Player who = Player.Find(message);
                if (who != null)
                {
                    name = who.name.ToLower();
                    message = who.ip;
                }
            }

            if (message.Equals("127.0.0.1")) { Player.SendMessage(p, "You can't ip-ban the server!"); return; }
            if (message.IndexOf('.') == -1) { Player.SendMessage(p, "Invalid IP!"); return; }
            if (message.Split('.').Length != 4) { Player.SendMessage(p, "Invalid IP!"); return; }
            if (p != null && p.ip == message) { Player.SendMessage(p, "You can't ip-ban yourself.!"); return; }
            if (Server.bannedIP.Contains(message)) { Player.SendMessage(p, message + " is already ip-banned."); return; }

            List<string> opNamesWithThatIP = (from pl in Player.players where (pl.ip == message && pl.@group.Permission >= LevelPermission.Operator) select pl.name).ToList();

            if (opNamesWithThatIP != null && opNamesWithThatIP.Count > 0)
            {
                foreach (string opname in opNamesWithThatIP)
                {
                    if (p != null)
                    {
                        Group grp = Group.findPlayerGroup(opname);
                        if (grp != null)
                        {
                            if (grp.Permission >= p.group.Permission)
                            {
                                Player.SendMessage(p, "You can only ipban IPs used by players with a lower rank.");
                                Player.SendMessage(p, Server.DefaultColor + opname + "(" + grp.color + grp.name + Server.DefaultColor + ") uses that IP.");
                                Server.s.Log(p.name + "failed to ipban " + message + " - IP is also used by: " + opname + "(" + grp.name + ")");
                                return;
                            }
                        }
                    }
                }
            }

            msg:
            if (p != null)
            {
                Server.IRC.Say(message.ToLower() + " was ip-banned by " + p.name + ".");
                Server.s.Log("IP-BANNED: " + message.ToLower() + " by " + p.name + ".");
                Player.GlobalMessage(message + " was &8ip-banned" + Server.DefaultColor + " by " + p.color + p.name + Server.DefaultColor + ".");
            }
            else
            {
                Server.IRC.Say(message.ToLower() + " was ip-banned by console.");
                Server.s.Log("IP-BANNED: " + message.ToLower() + " by console.");
                Player.GlobalMessage(message + " was &8ip-banned" + Server.DefaultColor + " by console.");
            }
            Server.bannedIP.Add(message);
            Server.bannedIP.Save("banned-ip.txt", false);
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/banip <ip/name> - Bans an ip. Also accepts a player name when you use @ before the name.");
        }
    }
}