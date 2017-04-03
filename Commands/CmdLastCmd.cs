using System;
namespace MCForge.Commands
{
    public class CmdLastCmd : Command
    {
        public override string name { get { return "lastcmd"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override void Use(Player p, string message)
        {
            foreach (Player pl in Player.players)
            {
                Player.SendMessage(p, pl.color + pl.name + Server.DefaultColor + " last used \"" + pl.lastCMD + "\"");
            }
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/lastcmd - Shows last commands used.");
        }
    }
}