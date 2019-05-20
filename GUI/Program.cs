/*
	Copyright 2009 MCSharp team (Modified for use with MCZall/MCLawl/MCForge/MCForge-Redux/MCSpleef)
	
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
namespace MCSpleef.Gui {
	public static class Program {
		public static DateTime startTime;
		public static string parent = Path.GetFileName(Assembly.GetEntryAssembly().Location);
		public static string parentfullpath = Assembly.GetEntryAssembly().Location;
		public static string parentfullpathdir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

		[DllImport("kernel32")]
		public static extern IntPtr GetConsoleWindow();
		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		public static void GlobalExHandler(object sender, UnhandledExceptionEventArgs e) {
			Exception ex = (Exception)e.ExceptionObject;
			Server.ErrorLog(ex);
			Thread.Sleep(500);
		}

		public static void ThreadExHandler(object sender, ThreadExceptionEventArgs e) {
			Exception ex = e.Exception;
			Server.ErrorLog(ex);
			Thread.Sleep(500);
		}

		[STAThread]
		public static void Main(string[] args) {
			startTime = DateTime.Now;
			if (Process.GetProcessesByName("MCForge").Length != 1) {
				foreach (Process pr in Process.GetProcessesByName("MCForge")) {
					if (pr.MainModule.BaseAddress == Process.GetCurrentProcess().MainModule.BaseAddress)
						if (pr.Id != Process.GetCurrentProcess().Id)
							pr.Kill();
				}
			}
			PidgeonLogger.Init();
			AppDomain.CurrentDomain.UnhandledException += GlobalExHandler;
			Application.ThreadException += ThreadExHandler;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MCSpleef.Gui.Window());
		}

		private static void WriteToConsole(string message) {
			if (!message.Contains("&") && !message.Contains("%")) {
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(message);
				return;
			}
			string[] splitted = message.Split('&', '%');
			for (int i = 0; i < splitted.Length; i++) {
				string elString = splitted[i];
				if (String.IsNullOrEmpty(elString))
					continue;
				Console.ForegroundColor = GetColor(elString[0]);
				Console.Write(elString.Substring(1));
				if (i != splitted.Length - 1)
					continue;
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write('\n');
			}
		}

		private static ConsoleColor GetColor(char c) {
			switch (c) {
				case 'e':
					return ConsoleColor.Yellow;
				case '0':
					return ConsoleColor.Black;
				case '1':
					return ConsoleColor.DarkBlue;
				case '2':
					return ConsoleColor.DarkGreen;
				case '3':
					return ConsoleColor.DarkCyan;
				case '4':
					return ConsoleColor.DarkMagenta;
				//No love for purples
				case '7':
					return ConsoleColor.Gray;
				case '6':
					return ConsoleColor.DarkYellow;
				case '8':
					return ConsoleColor.DarkGray;
				case '9':
					return ConsoleColor.Blue;
				case 'a':
					return ConsoleColor.Green;
				case 'b':
					return ConsoleColor.Cyan;
				case 'c':
					return ConsoleColor.Red;
				case 'd':
					return ConsoleColor.Magenta;
				//Dont need f, it will default to white.
				default:
					return ConsoleColor.White;
			}
		}

		public static void handleComm() {
			string s, msg;
			while (true) {
				try {
					string sentCmd = String.Empty, sentMsg = String.Empty;
					s = Console.ReadLine().Trim(); // Make sure we have no whitespace!

					if (s.Length < 1)
						continue;
					if (s[0] == '/')
						s = s.Remove(0, 1);
					else
						goto talk;
					if (s.IndexOf(' ') != -1) {
						sentCmd = s.Substring(0, s.IndexOf(' '));
						sentMsg = s.Substring(s.IndexOf(' ') + 1);
					} else if (s != String.Empty)
						sentCmd = s;
					else
						goto talk;

					try {
						if (Server.Check(sentCmd, sentMsg)) { Server.cancelcommand = false; continue; }
						Command cmd = Command.all.Find(sentCmd);
						if (cmd != null) {
							cmd.Use(null, sentMsg);
							Console.WriteLine("CONSOLE: USED /" + sentCmd + " " + sentMsg);
							if (sentCmd.ToLower() != "restart")
								continue;
							break;
						} else {
							Console.WriteLine("CONSOLE: Unknown command.");
							continue;
						}
					} catch (Exception e) {
						Server.ErrorLog(e);
						Console.WriteLine("CONSOLE: Failed command.");
						continue;
					}

				talk:
					if (s[0] == '@') {
						s = s.Remove(0, 1);
						int spacePos = s.IndexOf(' ');
						if (spacePos == -1) { Console.WriteLine("No message entered."); continue; }
						Player pl = Player.Find(s.Substring(0, spacePos));
						if (pl == null) { Console.WriteLine("Player not found."); continue; }
						msg = String.Format("&9[>] {0}Console [&a{1}{0}]: &f{2}", Server.DefaultColor, Server.ZallState, s.Substring(spacePos + 1));
						Player.SendMessage(pl, msg);
					} else if (s[0] == '#') {
						msg = String.Format("To Ops -{0}Console [&a{1}{0}]&f- {2}", Server.DefaultColor, Server.ZallState, s);
						Player.GlobalMessageOps(msg);
					} else {
						msg = String.Format("{0}Console [&a{1}{0}]: &f{2}", Server.DefaultColor, Server.ZallState, s);
						Player.GlobalMessage(msg);
					}
					WriteToConsole(msg);
				} catch (Exception ex) {
					Server.ErrorLog(ex);
				}
			}
		}

		public static System.Timers.Timer updateTimer = new System.Timers.Timer(120 * 60 * 1000);

		static public void ExitProgram(bool AutoRestart) {
			Server.restarting = AutoRestart;
			Server.shuttingDown = true;

			Server.Exit(AutoRestart);

			new Thread(new ThreadStart(delegate {
				if (AutoRestart) {
					saveAll(true);

					if (Server.listen != null)
						Server.listen.Close();
					Process.Start(parent);
					Environment.Exit(0);
				} else {
					saveAll(false);
					Application.Exit();
					Environment.Exit(0);
				}
			})).Start();
		}

		static public void saveAll(bool restarting) {
			try {
				List<Player> kickList = new List<Player>();
				kickList.AddRange(Player.players);
				foreach (Player p in kickList) {
					if (restarting)
						p.Kick("Server restarted! Rejoin!");
					else
						p.Kick("Server is shutting down.");
				}
			} catch (Exception exc) { Server.ErrorLog(exc); }
		}
	}
}