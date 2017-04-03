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
using System.IO;
namespace MCForge.Commands
{
	public class CmdView : Command
	{
		public override string name { get { return "view"; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
		public override void Use(Player p, string message)
		{
			if (!Directory.Exists("text/view")) Directory.CreateDirectory("text/view");
			if (message == "")
			{
				DirectoryInfo di = new DirectoryInfo("text/view");
				string allFiles = "";
				foreach (FileInfo fi in di.GetFiles("*.txt")) { allFiles += ", " + fi.Name; }

				if (allFiles == "") {
					Player.SendMessage(p, "No files are viewable by you");
				} else {
					Player.SendMessage(p, "Available files:");
					Player.SendMessage(p, allFiles.Remove(0, 2));
				}
			}
			else
			{
				rulesretry:
				if (File.Exists("text/view/" + message + ".txt"))
				{
					try
					{
						string[] allLines = File.ReadAllLines("text/view/" + message + ".txt");
						for (int i = 0; i < allLines.Length; i++) 
							Player.SendMessage(p, allLines[i]);
					} catch { Player.SendMessage(p, "An error occurred when retrieving the file"); }
				}
				else if (message == "rules") { File.AppendAllText("text/view/rules.txt", "(This text's customizable in text/view/rules.txt!)" + Environment.NewLine + "Use common sense!"); goto rulesretry; }
				else { Player.SendMessage(p, "File specified doesn't exist"); }
			}
		}
		public override void Help(Player p)
		{
			Player.SendMessage(p, "/view [file] - Views [file]'s contents");
			Player.SendMessage(p, "/view by itself will list all files you can view");
		}
	}
}