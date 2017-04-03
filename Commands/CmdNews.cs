using System;
namespace MCForge.Commands
{
	class CmdNews : Command
	{
		public override string name { get { return "news"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
		public override void Use(Player p, string message)
		{
			Command.all.Find("view").Use(p, "news");
		}
		public override void Help(Player p)
		{
			Player.SendMessage(p, "/news - Read the news.");
			Player.SendMessage(p, "Shortcut to /view news.");
		}
	}
}
