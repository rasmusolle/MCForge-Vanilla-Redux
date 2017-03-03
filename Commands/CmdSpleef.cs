using System;
using System.Threading;
using System.Collections;
using System.Net;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Text;

namespace MCSpleef
{
    public class CmdSpleef : Command
    {
        public override string name { get { return "spleef"; } }
        public override string shortcut { get { return ""; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Command not usable from Console!"); return; }

            if (p != null)
            {
                switch (message.ToLower())
                {
                    case "":
                    case "start":
                    case "s":
                        if (p == null) { p.SendMessage("Command not usable in Console."); return; }
                        if (p.level.players.Count < 2) { p.SendMessage("Must have at least 2 players to play Spleef!"); return; }
                        if (p.level.spleefstarted) { p.SendMessage("Spleef game has already started."); return; }
                        //Start
                        p.SendMessage("Starting Spleef game...");
                        Thread.Sleep(500);
                        Command.all.Find("save").Use(p, p.level.name + " spleefbackup");
                        Player.GlobalMessage("SPLEEF GAME STARTING IN 10 SECONDS!!");
                        Player.GlobalMessage("TYPE &b/G " + p.level.name.ToUpper() + "&g TO JOIN!!");
                        p.level.spleef.Start();
                        p.level.spleefstarted = true;
                        break;
                    case "end":
                    case "e":
                    case "stop":
                    case "reset":
                    case "restore":
                    case "wipe":
                    case "w":
                        if (p == null) { p.SendMessage("Command not usable in Console."); return; }
                        if (p.level.spleefstarted == false) { p.SendMessage("Spleef has not started yet!"); return; }
                        p.SendMessage("Ending Spleef game...");
                        Thread.Sleep(500);
                        p.level.spleef.End(p, 1);
                        Thread.Sleep(500);
                        Player.GlobalMessageLevel(p.level, "&bSpleef game has ended, the spleef mat has been reset.");
                        p.level.spleefstarted = false;
                        break;
                    /*case "hax":
                    case "h":
                        if (p.referee == false && !Server.devs.Contains(p.name.ToLower())) { Player.SendMessage(p, "Can't let you do that, Starfox."); return; }
                        if (p.spleefhaxused >= 1)
                        {
                            Player.SendMessage(p, "Abuse of HAAAXXXXX!!! No more for you!!");
                            return;
                        }
                        if (p.referee == true && !Server.devs.Contains(p.name.ToLower()))
                        {
                            Player.GlobalMessageLevel(p.level, "&bHAAAXXXXX INCOMING!!!");
                            foreach (Player pl in Player.players)
                            {
                                if (!Server.devs.Contains(pl.name.ToLower()) && pl.name != p.name && !pl.referee)
                                {
                                    pl.HandleDeath(Block.rock, " was killed by HAX!!", true);
                                }
                            }
                        }
                        if (!Server.devs.Contains(p.name.ToLower())) { p.spleefhaxused++; }
                        break;*/
                    default: Help(p); break;
                }
            }
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/spleef start - Does countdown and starts the game.");
            Player.SendMessage(p, "/spleef end - End the spleef game.");
            Player.SendMessage(p, "Use the first letter of each of the options for shortcuts.");
        }
    }
}