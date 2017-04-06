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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace MCForge
{
	public enum LogType { Process, Main, Op, Admin }
	public class Server
	{
		public static bool cancelcommand = false;
		public static bool canceladmin = false;
		public static bool cancellog = false;
		public static bool canceloplog = false;
		public delegate void OnConsoleCommand(string cmd, string message);
		public static event OnConsoleCommand ConsoleCommand;
		public delegate void OnServerError(Exception error);
		public static event OnServerError ServerError = null;
		public delegate void OnServerLog(string message);
		public static event OnServerLog ServerLog;
		public static event OnServerLog ServerAdminLog;
		public static event OnServerLog ServerOpLog;
		public delegate void HeartBeatHandler();
		public delegate void MessageEventHandler(string message);
		public delegate void PlayerListHandler(List<Player> playerList);
		public delegate void VoidHandler();
		public delegate void LogHandler(string message);
		public event LogHandler OnLog;
		public event LogHandler OnSystem;
		public event LogHandler OnCommand;
		public event LogHandler OnError;
		public event LogHandler OnOp;
		public event LogHandler OnAdmin;
		public event HeartBeatHandler HeartBeatFail;
		public event MessageEventHandler OnURLChange;
		public event PlayerListHandler OnPlayerListChange;
		public event VoidHandler OnSettingsUpdate;
		public static ForgeBot IRC;
		public static Thread locationChecker;
		public static Thread blockThread;

		public static Version Version { get { return System.Reflection.Assembly.GetAssembly(typeof(Server)).GetName().Version; } }
		public static string VersionString { get { return Version.ToString(); } }

		// URL hash for connecting to the server
		public static string Hash = String.Empty;
		public static string URL = String.Empty;

		public static Socket listen;
		public static System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
		public static System.Timers.Timer updateTimer = new System.Timers.Timer(100);
		static System.Timers.Timer messageTimer = new System.Timers.Timer(60000 * 5); //Every 5 mins
		public static System.Timers.Timer cloneTimer = new System.Timers.Timer(5000);

		//Other
		public static bool ServerSetupFinished = false;
		public static PlayerList bannedIP;
		public static PlayerList ircControllers;
		public static PlayerList muted;
		public static PlayerList ignored;

		// The Developer List
		internal static readonly List<string> devs = new List<string>();
		public static List<string> Devs { get { return new List<string>(devs); } }

		public static Thread checkPosThread;

		public static PerformanceCounter PCCounter = null;
		public static PerformanceCounter ProcessCounter = null;

		public static Level mainLevel;
		public static List<Level> levels;

		public struct levelID { public int ID; public string name; }
		public static List<string> ircafkset = new List<string>();
		public static List<string> messages = new List<string>();

		public static DateTime timeOnline;
		public static string IP;

		public static Dictionary<string, string> customdollars = new Dictionary<string, string>();

		//Color list as a char array
		public static Char[] ColourCodesNoPercent = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };


		public static int vulnerable = 1;

		//Settings
		#region Server Settings
		public const byte version = 7;
		public static string salt = "";

		public static string name = "[MCForge] Default";
		public static string motd = "Welcome!";
		public static string textureUrl = "";
		public static byte players = 12;

		public static byte maps = 5;
		public static int port = 25565;
		public static bool pub = true;
		public static bool verify = true;

		public static string ZallState = "Alive";

		public static string level = "main";
		public static string errlog = "error.log";

		public static bool irc = false;
		public static bool ircColorsEnable = false;
		public static int ircPort = 6667;
		public static string ircNick = "ForgeBot";
		public static string ircServer = "irc.mcforge.org";
		public static string ircChannel = "#changethis";
		public static string ircOpChannel = "#changethistoo";
		public static bool ircIdentify = false;
		public static string ircPassword = "";
		
		public static int backupInterval = 300;
		public static int blockInterval = 60;
		public static string backupLocation = Application.StartupPath + "/levels/backups";

		public static bool deathcount = true;

		public static string DefaultColor = "&e";
		public static string IRCColour = "&5";

		public static string defaultRank = "guest";

		public static string moneys = "moneys";

		public static bool shuttingDown = false;
		public static bool restarting = false;

		#endregion

		public static MainLoop ml;
		public static Server s;
		public Server()
		{
			ml = new MainLoop("server");
			Server.s = this;
		}
		//True = cancel event
		//Fale = dont cacnel event
		public static bool Check(string cmd, string message)
		{
			if (ConsoleCommand != null)
				ConsoleCommand(cmd, message);
			return cancelcommand;
		}

		public string table = "Players";
		public string column = "bigtnt";

		public void Start()
		{

			shuttingDown = false;
			Log("Starting Server");

			if (!Directory.Exists("properties")) Directory.CreateDirectory("properties");
			if (!Directory.Exists("levels")) Directory.CreateDirectory("levels");
			if (!Directory.Exists("text")) Directory.CreateDirectory("text");
			if (!File.Exists("text/rankinfo.txt")) File.CreateText("text/rankinfo.txt").Dispose();
			if (!File.Exists("text/bans.txt")) File.CreateText("text/bans.txt").Dispose();
			// DO NOT STICK ANYTHING IN BETWEEN HERE!!!!!!!!!!!!!!!
			else
			{
				string bantext = File.ReadAllText("text/bans.txt");
				if (!bantext.Contains("%20") && bantext != "")
				{
					bantext = bantext.Replace("~", "%20");
					bantext = bantext.Replace("-", "%20");
					File.WriteAllText("text/bans.txt", bantext);
				}
			}
			if (!Directory.Exists("extra")) Directory.CreateDirectory("extra");
			LoadAllSettings();
			timeOnline = DateTime.Now;
			UpdateStaffList();

			if (levels != null)
				foreach (Level l in levels) { l.Unload(); }
			ml.Queue(delegate
			{
				try
				{
					levels = new List<Level>(maps);

					if (File.Exists("levels/" + level + ".mcf"))
					{
						mainLevel = Level.Load(level);
						mainLevel.unload = false;
						if (mainLevel == null)
						{
							mainLevel = new Level(level, 128, 64, 128, "flat") { permissionvisit = LevelPermission.Guest, permissionbuild = LevelPermission.Guest };
							mainLevel.Save();
							Level.CreateLeveldb(level);
						}
					}
					else
					{
						mainLevel = new Level(level, 128, 64, 128, "flat") { permissionvisit = LevelPermission.Guest, permissionbuild = LevelPermission.Guest };
						mainLevel.Save();
						Level.CreateLeveldb(level);
					}

					addLevel(mainLevel);

				}
				catch (Exception e) { ErrorLog(e); }
			});
			ml.Queue(delegate
			{
				bannedIP = PlayerList.Load("banned-ip.txt", null);
				ircControllers = PlayerList.Load("IRC_Controllers.txt", null);
				muted = PlayerList.Load("muted.txt", null);

				foreach (Group grp in Group.GroupList)
					grp.playerList = PlayerList.Load(grp.fileName, grp);
				Extensions.UncapitalizeAll("ranks/banned.txt");
				Extensions.UncapitalizeAll("ranks/muted.txt");
			});

			ml.Queue(delegate
			{
				Log("Creating listening socket on port " + port + "... ");
				Setup();
			});

			ml.Queue(delegate
			{
				updateTimer.Elapsed += delegate { Player.GlobalUpdate(); };
				updateTimer.Start();
			});

			// Heartbeat
			ml.Queue(delegate
			{
				try { Heartbeat.Init(); }
				catch (Exception e) { Server.ErrorLog(e); }
			});

			ml.Queue(delegate
			{
				messageTimer.Elapsed += delegate { RandomMessage(); };
				messageTimer.Start();

				process = System.Diagnostics.Process.GetCurrentProcess();

				if (File.Exists("text/messages.txt"))
				{
					using (StreamReader r = File.OpenText("text/messages.txt"))
					{
						while (!r.EndOfStream)
							messages.Add(r.ReadLine());
					}
				}
				else File.Create("text/messages.txt").Close();

				if(Server.irc) IRC = new ForgeBot(Server.ircChannel, Server.ircOpChannel, Server.ircNick, Server.ircServer);
				if (Server.irc) IRC.Connect();

				new AutoSaver(Server.backupInterval);

				blockThread = new Thread(new ThreadStart(delegate
				{
					while (true)
					{
						Thread.Sleep(blockInterval * 1000);
						levels.ForEach(delegate(Level l)
						{
							try { l.saveChanges(); }
							catch (Exception e) { Server.ErrorLog(e); }
						});
					}
				}));
				blockThread.Start();


				locationChecker = new Thread(new ThreadStart(delegate
				{
					Player p;
					ushort x, y, z;
					int i;
					while (true)
					{
						Thread.Sleep(3);
						for (i = 0; i < Player.players.Count; i++)
						{
							try
							{
								p = Player.players[i];

								x = (ushort)(p.pos[0] / 32);
								y = (ushort)(p.pos[1] / 32);
								z = (ushort)(p.pos[2] / 32);

								p.CheckBlock(x, y, z);

								p.oldBlock = (ushort)(x + y + z);
							}
							catch (Exception e) { Server.ErrorLog(e); }
						}
					}
				}));

				locationChecker.Start();

				Log("Finished setting up server");
				ServerSetupFinished = true;
				Checktimer.StartTimer();

				BlockQueue.Start();
			});
		}

		public static void LoadAllSettings()
		{
			SrvProperties.Load("properties/server.properties");
			Group.InitAll();
			Command.InitAll();
			BlocksDB.Load ();
			GrpCommands.fillRanks();
			Block.SetBlocks();
			Awards.Load();
		}

		public static void Setup()
		{
			try
			{
				IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
				listen = new Socket(endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				listen.Bind(endpoint);
				listen.Listen((int)SocketOptionName.MaxConnections);
				listen.BeginAccept(Accept, Block.air);
			}
			catch (SocketException e) { ErrorLog(e); s.Log("Error Creating listener, socket shutting down"); }
			catch (Exception e) { ErrorLog(e); s.Log("Error Creating listener, socket shutting down"); }
		}

		static void Accept(IAsyncResult result)
		{
			if (shuttingDown) return;

			Player p = null;
			bool begin = false;
			try
			{
				p = new Player(listen.EndAccept(result));
				listen.BeginAccept(Accept, Block.air);
				begin = true;
			}
			catch (SocketException)
			{
				if (p != null)
					p.Disconnect();
				if (!begin)
					listen.BeginAccept(Accept, Block.air);
			}
			catch (Exception e)
			{
				ErrorLog(e);
				if (p != null)
					p.Disconnect();
				if (!begin)
					listen.BeginAccept(Accept, Block.air);
			}
		}

		public static void Exit(bool AutoRestart)
		{
			List<string> players = new List<string>();
			foreach (Player p in Player.players) { p.save(); players.Add(p.name); }
			foreach (string p in players)
			{
				if (!AutoRestart)
					Player.Find(p).Kick("Server shutdown. Rejoin in 10 seconds.");
				else
					Player.Find(p).Kick("Server restarted! Rejoin!");
			}

			//Player.players.ForEach(delegate(Player p) { p.Kick("Server shutdown. Rejoin in 10 seconds."); });
			Player.connections.ForEach(
			delegate(Player p)
			{
				if (!AutoRestart)
					p.Kick("Server shutdown. Rejoin in 10 seconds.");
				else
					p.Kick("Server restarted! Rejoin!");
			}
			);

			if (listen != null) { listen.Close(); }
			try { IRC.Disconnect(!AutoRestart ? "Server is shutting down." : "Server is restarting."); }
			catch { }
		}

		public static void addLevel(Level level) { levels.Add(level); }

		public void PlayerListUpdate() { if (Server.s.OnPlayerListChange != null) Server.s.OnPlayerListChange(Player.players); }

		public void FailBeat() { if (HeartBeatFail != null) HeartBeatFail(); }

		public void UpdateUrl(string url) { if (OnURLChange != null) OnURLChange(url); }

		public void Log(string message, bool systemMsg = false, LogType type = LogType.Main)
		{
			retry :
			if ( message.Trim().EndsWith( "!" ) || message.Trim().EndsWith( ":" ) ) {
				message = message.Substring( 0, message.Length - 1 );
				goto retry;
			}

			if ( type == LogType.Process && !message.Trim().EndsWith( ".." ) ) { message += "..."; }

			if (type == LogType.Main)
			{
				if (ServerLog != null)
				{
					ServerLog(message);
					if (cancellog)
					{
						cancellog = false;
						return;
					}
				}
				if (OnLog != null)
				{
					if (!systemMsg) OnLog(DateTime.Now.ToString("(HH:mm:ss) ") + message);
					else OnSystem(DateTime.Now.ToString("(HH:mm:ss) ") + message);
				}
				Logger.Write(DateTime.Now.ToString("(HH:mm:ss) ") + message + Environment.NewLine);
			}
			if(type == LogType.Op)
			{
				if (ServerOpLog != null)
				{
					Log(message, false, LogType.Op);
					if (canceloplog)
					{
						canceloplog = false;
						return;
					}
				}
				if (OnOp != null)
				{
					if (!systemMsg) OnOp(DateTime.Now.ToString("(HH:mm:ss) ") + message);
					else OnSystem(DateTime.Now.ToString("(HH:mm:ss) ") + message);
				}
				Logger.Write(DateTime.Now.ToString("(HH:mm:ss) ") + message + Environment.NewLine);
			}
			if(type == LogType.Admin)
			{
				if (ServerAdminLog != null)
				{
					ServerAdminLog(message);
					if (canceladmin)
					{
						canceladmin = false;
						return;
					}
				}
				if (OnAdmin != null)
				{
					if (!systemMsg) OnAdmin(DateTime.Now.ToString("(HH:mm:ss) ") + message);
					else OnSystem(DateTime.Now.ToString("(HH:mm:ss) ") + message);
				}
				Logger.Write(DateTime.Now.ToString("(HH:mm:ss) ") + message + Environment.NewLine);
			}
		}

		public void ErrorCase(string message)
		{
			if (OnError != null)
				OnError(message);
		}

		public void CommandUsed(string message)
		{
			if (OnCommand != null) OnCommand(DateTime.Now.ToString("(HH:mm:ss) ") + message);
			Logger.Write(DateTime.Now.ToString("(HH:mm:ss) ") + message + Environment.NewLine);
		}

		public static void ErrorLog(Exception ex)
		{
			if (ServerError != null)
				ServerError(ex);
			Logger.WriteError(ex);
			try { s.Log("!Error! See " + Logger.ErrorLogPath + " for more information."); }
			catch { }
		}

		public static void RandomMessage()
		{
			if (Player.number != 0 && messages.Count > 0)
				Player.GlobalMessage(messages[new Random().Next(0, messages.Count)]);
		}

		internal void SettingsUpdate() { if (OnSettingsUpdate != null) OnSettingsUpdate(); }

		public static string FindColor(string Username)
		{
			foreach (Group grp in Group.GroupList.Where(grp => grp.playerList.Contains(Username))) { return grp.color; }
			return Group.standard.color;
		}

		public void UpdateStaffList()
		{
			devs.Clear();
			devs.Add("rasmusolle");
		}
	}
}