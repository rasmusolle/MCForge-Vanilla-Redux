using System;
namespace MCSpleef.Commands
{
	public class CmdRestart : Command
	{
		public override string name { get { return "restart"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
		public override void Use(Player p, string message)
		{
			MCSpleef.Gui.Program.ExitProgram(true);
		}
		public override void Help(Player p)
		{
			Player.SendMessage(p, "/restart - Restarts the server.");
		}
	}
}