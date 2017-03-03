using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace MCForge.Commands
{
    public class CmdCrashServer : Command
    {
        public override string name { get { return "crashserver"; } }
        public override string shortcut { get { return "crash"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public override void Use(Player p, string message)
        {
            p.Kick("Server crash! Error code 0x0005A4");
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/crashserver - Crash the server!");
        }
    }

}
