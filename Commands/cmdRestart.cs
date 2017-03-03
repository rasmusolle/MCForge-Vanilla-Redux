using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCForge_.Gui;

namespace MCForge.Commands
{
    public class CmdRestart : Command
    {
        public override string name { get { return "restart"; } }
        public override string shortcut { get { return ""; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdRestart() { }

        public override void Use(Player p, string message)
        {
            //TODO: Fix /restart
            //MCForge_.Gui.Program.restartMe();
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/restart - Restarts the server!");
        }
    }
}
