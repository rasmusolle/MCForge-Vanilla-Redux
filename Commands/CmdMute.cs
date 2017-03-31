using System;

namespace MCForge.Commands
{
    public class CmdMute : Command
    {
        public override string name { get { return "mute"; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override void Use(Player p, string message)
        {
            if (message == "" || message.Split(' ').Length > 2) { Help(p); return; }
            Player who = Player.Find(message);
            if (who == null)
            {
                Player.SendMessage(p, "The player entered is not online.");
                return;
            }
            if (who.muted)
            {
                who.muted = false;
                Player.GlobalChat(null, who.color + who.name + Server.DefaultColor + " has been &bun-muted", false);
            }
            else
            {
                if (p != null)
                {
                    if (who != p) if (who.group.Permission > p.group.Permission) { Player.SendMessage(p, "Cannot mute someone of a higher rank."); return; }
                }
                who.muted = true;
                Player.GlobalChat(null, who.color + who.name + Server.DefaultColor + " has been &8muted", false);
            }
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/mute <player> - Mutes or unmutes the player.");
        }
    }
}