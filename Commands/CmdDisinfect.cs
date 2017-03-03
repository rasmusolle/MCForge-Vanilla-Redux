/*
    Copyright � 2011-2014 MCForge-Redux
		
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
namespace MCForge.Commands
{
    /// <summary>
    /// This is the command /disinfect
    /// use /help disinfect in-game for more info
    /// </summary>
    public class CmdDisInfect : Command
    {
        public override string name { get { return "disinfect"; } }
        public override string shortcut { get { return  "di"; } }
        public override string type { get { return "game"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdDisInfect() { }
        public override void Use(Player p, string message)
        {
            Player who = null;
            if (message == "") { who = p; message = p.name; } else { who = Player.Find(message); }
            if (!who.infected || !Server.zombie.GameInProgess())
            {
                p.SendMessage("Cannot disinfect player");
            }
            else
            {
                if (!who.referee)
                {
                    Server.zombie.DisinfectPlayer(who);
                    Player.GlobalMessage(p.color + p.name + Server.DefaultColor + " just got Disnfected!");
                }
            }
        }
        public override void Help(Player p)
        {
            Player.SendMessage(p, "/disinfect [name] - disinfects [name]");
        }
    }
}