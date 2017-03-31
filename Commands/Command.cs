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
            all.Add(new CmdGive());
            all.Add(new CmdHelp());
            all.Add(new CmdInfo());
            all.Add(new CmdIrc());
            all.Add(new CmdKick());
            all.Add(new CmdLastCmd());
            all.Add(new CmdMe());
            all.Add(new CmdMute());
            all.Add(new CmdNewLvl());
            all.Add(new CmdNews());
            all.Add(new CmdPay());
            all.Add(new CmdPCount());
            all.Add(new CmdPlayers());
            all.Add(new CmdRestart());
            all.Add(new CmdRoll());
            all.Add(new CmdRules());
            all.Add(new CmdSave());
            all.Add(new CmdSay());
            all.Add(new CmdServerReport());
            all.Add(new CmdSetRank());
            all.Add(new CmdSetspawn());
            all.Add(new CmdSpawn());
            //all.Add(new CmdSpleef());
            all.Add(new CmdTake());
            all.Add(new CmdTitle());
            all.Add(new CmdUnban());
            all.Add(new CmdUnbanip());
            all.Add(new CmdView());
            all.Add(new CmdViewRanks());
            all.Add(new CmdWhisper());
            all.Add(new CmdWhoip());
            all.Add(new CmdWhois());
            all.Add(new CmdWhowas());
            all.Add(new CmdXban());
            all.Add(new CmdCrashServer());
            all.Add(new CmdAward());
            all.Add(new CmdAwards());

            core.commands = new List<Command>(all.commands);
        }
    }
}