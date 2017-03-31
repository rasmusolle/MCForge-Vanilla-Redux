using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCForge.Gui;

namespace MCForge.Commands
{
    public class CmdRestart : Command
    {
        public override string name { get { return "restart"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override void Use(Player p, string message)
        {
            MCForge.Gui.Program.ExitProgram(true);
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/restart - Restarts the server.");
        }
    }
}
