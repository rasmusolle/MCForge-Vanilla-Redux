/*
	Copyright 2017 MCSpleef
	
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
namespace MCSpleef.Commands {
	public class CmdGamble : Command {
		public override string name { get { return "gamble"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
		public override void Use(Player p, string message) {
			if (message.Split(' ').Length != 2) { Help(p); return; }

			string[] command = message.Trim().Split(' ');
			string par0 = command[0];
			string par1 = command[1];

			Random random = new Random();
			int randomNumber = random.Next(1, 7);

			int AmountGamble;
			int ValueGamble;
			try { AmountGamble = int.Parse(par0); } catch { Player.SendMessage(p, "[amount] is invalid."); return; }
			try { ValueGamble = int.Parse(par1); } catch { Player.SendMessage(p, "[value] is invalid."); return; }
			if (AmountGamble > p.money) { Player.SendMessage(p, "You don't have that much money."); return; }
			if (Decimal.Add(AmountGamble, p.money) >= 16777215) { Player.SendMessage(p, "Can't let you have more than 16777215 " + Server.moneys); return; }
			if (AmountGamble <= 0) { Player.SendMessage(p, "Invalid amount."); return; }

			if (randomNumber == ValueGamble) {
				Player.GlobalMessage(p.color + p.name + Server.DefaultColor + " won &aBIG! " + Server.DefaultColor + "(" + AmountGamble + " turned into " + Decimal.Multiply(AmountGamble, 2) + ")");
				p.money += AmountGamble;
			} else {
				Player.GlobalMessage(p.color + p.name + Server.DefaultColor + " &cLOST " + Server.DefaultColor + "money. (Lost " + AmountGamble + ")");
				p.money -= AmountGamble;
			}
		}
		public override void Help(Player p) {
			Player.SendMessage(p, "/gamble [amount] [value] - Gambles [amount] money on [value].");
			Player.SendMessage(p, "[value] is a number between 1 and 6.");
			Player.SendMessage(p, "If you win, you get double your gambled money.");
			Player.SendMessage(p, "If you lose, you lose all your gambled money.");
		}
	}
}