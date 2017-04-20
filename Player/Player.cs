/*
	Copyright 2011-2014 MCForge-Redux (Modified for use with MCSpleef)
		
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
namespace MCSpleef
{
	public partial class Player : IDisposable
	{
		#region EMOTES
		private static readonly char[] UnicodeReplacements = " ☺☻♥♦♣♠•◘○\n♂♀♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼".ToCharArray();

		/// <summary> List of chat keywords, and emotes that they stand for. </summary>
		public Dictionary<string, object> ExtraData = new Dictionary<string, object>();
		public static readonly Dictionary<string, char> EmoteKeywords = new Dictionary<string, char> 
		{
			{ "darksmile", '\u0001' },

			{ "smile", '\u0002' }, // ☻

			{ "heart", '\u0003' }, // ♥
			{ "hearts", '\u0003' },

			{ "diamond", '\u0004' }, // ♦
			{ "diamonds", '\u0004' },
			{ "rhombus", '\u0004' },

			{ "club", '\u0005' }, // ♣
			{ "clubs", '\u0005' },
			{ "clover", '\u0005' },
			{ "shamrock", '\u0005' },

			{ "spade", '\u0006' }, // ♠
			{ "spades", '\u0006' },

			{ "*", '\u0007' }, // •
			{ "bullet", '\u0007' },
			{ "dot", '\u0007' },
			{ "point", '\u0007' },

			{ "hole", '\u0008' }, // ◘

			{ "circle", '\u0009' }, // ○
			{ "o", '\u0009' },

			{ "male", '\u000B' }, // ♂
			{ "mars", '\u000B' },

			{ "female", '\u000C' }, // ♀
			{ "venus", '\u000C' },

			{ "8", '\u000D' }, // ♪
			{ "note", '\u000D' },
			{ "quaver", '\u000D' },

			{ "notes", '\u000E' }, // ♫
			{ "music", '\u000E' },

			{ "sun", '\u000F' }, // ☼
			{ "celestia", '\u000F' },

			{ ">>", '\u0010' }, // ►
			{ "right2", '\u0010' },

			{ "<<", '\u0011' }, // ◄
			{ "left2", '\u0011' },

			{ "updown", '\u0012' }, // ↕
			{ "^v", '\u0012' },

			{ "!!", '\u0013' }, // ‼

			{ "p", '\u0014' }, // ¶
			{ "para", '\u0014' },
			{ "pilcrow", '\u0014' },
			{ "paragraph", '\u0014' },

			{ "s", '\u0015' }, // §
			{ "sect", '\u0015' },
			{ "section", '\u0015' },

			{ "-", '\u0016' }, // ▬
			{ "_", '\u0016' },
			{ "bar", '\u0016' },
			{ "half", '\u0016' },

			{ "updown2", '\u0017' }, // ↨
			{ "^v_", '\u0017' },

			{ "^", '\u0018' }, // ↑
			{ "up", '\u0018' },

			{ "v", '\u0019' }, // ↓
			{ "down", '\u0019' },

			{ ">", '\u001A' }, // →
			{ "->", '\u001A' },
			{ "right", '\u001A' },

			{ "<", '\u001B' }, // ←
			{ "<-", '\u001B' },
			{ "left", '\u001B' },

			{ "l", '\u001C' }, // ∟
			{ "angle", '\u001C' },
			{ "corner", '\u001C' },

			{ "<>", '\u001D' }, // ↔
			{ "<->", '\u001D' },
			{ "leftright", '\u001D' },

			{ "^^", '\u001E' }, // ▲
			{ "up2", '\u001E' },

			{ "vv", '\u001F' }, // ▼
			{ "down2", '\u001F' },

			{ "house", '\u007F' } // ⌂
		};
		public static string ReplaceEmoteKeywords(string message ) {
			if ( message == null )
				throw new ArgumentNullException( "message" );
			int startIndex = message.IndexOf( '(' );
			if ( startIndex == -1 ) {
				return message; // break out early if there are no opening braces
			}

			StringBuilder output = new StringBuilder( message.Length );
			int lastAppendedIndex = 0;
			while ( startIndex != -1 ) {
				int endIndex = message.IndexOf( ')', startIndex + 1 );
				if ( endIndex == -1 ) {
					break; // abort if there are no more closing braces
				}

				// see if emote was escaped (if odd number of backslashes precede it)
				bool escaped = false;
				for ( int i = startIndex - 1; i >= 0 && message[i] == '\\'; i-- ) {
					escaped = !escaped;
				}
				// extract the keyword
				string keyword = message.Substring( startIndex + 1, endIndex - startIndex - 1 );
				char substitute;
				if ( EmoteKeywords.TryGetValue( keyword.ToLowerInvariant(), out substitute ) ) {
					if ( escaped ) {
						// it was escaped; remove escaping character
						startIndex++;
						output.Append( message, lastAppendedIndex, startIndex - lastAppendedIndex - 2 );
						lastAppendedIndex = startIndex - 1;
					} else {
						// it was not escaped; insert substitute character
						output.Append( message, lastAppendedIndex, startIndex - lastAppendedIndex );
						output.Append( substitute );
						startIndex = endIndex + 1;
						lastAppendedIndex = startIndex;
					}
				} else {
					startIndex++; // unrecognized macro, keep going
				}
				startIndex = message.IndexOf( '(', startIndex );
			}
			// append the leftovers
			output.Append( message, lastAppendedIndex, message.Length - lastAppendedIndex );
			return output.ToString();
		}
		private static readonly Regex EmoteSymbols = new Regex( "[\x00-\x1F\x7F☺☻♥♦♣♠•◘○\n♂♀♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼⌂]" );

		#endregion

		public void ClearChat() { OnChat = null; }
		/// <summary>
		/// List of all server players.
		/// </summary>
		public static List<Player> players = new List<Player>();
		/// <summary>
		/// Key - Name
		/// Value - IP
		/// All players who have left this restart.
		/// </summary>
		public static Dictionary<string, string> left = new Dictionary<string, string>();

		public static List<Player> connections = new List<Player>(Server.players);
		System.Timers.Timer muteTimer = new System.Timers.Timer(1000);
		public static List<string> emoteList = new List<string>();
		public List<string> listignored = new List<string>();
		public List<string> mapgroups = new List<string>();
		public static byte number { get { return (byte)players.Count; } }
		static System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
		static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
		public static string lastMSG = "";
		public int health = 100;

		public static bool storeHelp = false;
		public static string storedHelp = "";
		private string truename;
		internal bool dontmindme = false;
		public Socket socket;
		System.Timers.Timer timespent = new System.Timers.Timer(1000);
		System.Timers.Timer loginTimer = new System.Timers.Timer(1000);
		public System.Timers.Timer pingTimer = new System.Timers.Timer(2000);
		System.Timers.Timer extraTimer = new System.Timers.Timer(22000);

		byte[] buffer = new byte[0];
		byte[] tempbuffer = new byte[0xFF];
		public bool disconnected = false;
		public string time;
		public string name;
		public string DisplayName;
		public string SkinName;
		public bool identified = false;
		public bool UsingID = false;
		public int ID = 0;
		public int warn = 0;
		public byte id;
		public int userID = -1;
		public string ip;
		public string exIP; // external IP
		public string color;
		public Group group;
		public bool hidden = false;
		public bool muted = false;
		public string prefix = "";
		public string title = "";
		public string titlecolor;
		public string model = "humanoid";

		public bool deleteMode = false;
		public bool ignorePermission = false;
		public bool parseSmiley = true;
		public bool smileySaved = true;
		public bool opchat = false;
		public bool whisper = false;
		public string whisperTo = "";
		public bool ignoreglobal = false;

		public string storedMessage = "";

		// Only used for possession.
		//Using for anything else can cause unintended effects!
		public bool canBuild = true;

		public int money = 0;
		public int points = 0;
		public long overallBlocks = 0;

		public int loginBlocks = 0;

		public DateTime timeLogged;
		public DateTime firstLogin;
		public int totalLogins = 0;
		public int totalKicked = 0;
		public int overallDeath = 0;

		public Thread commThread;
		public bool commUse = false;

		public bool adminpen = false;

		public bool spawned = false;

		public string prevMsg = "";


		//Movement
		public ushort oldBlock = 0;
		public ushort deathCount = 0;
		public ushort deathblock;

		//Games
		public DateTime lastDeath = DateTime.Now;

		public byte blockAction; //0-Nothing 1-solid 2-lava 3-water 4-active_lava 5 Active_water 6 OpGlass 7 BluePort 8 OrangePort
		public ushort modeType;
		public ushort[] bindings = new ushort[(ushort)128];
		public string[] cmdBind = new string[10];
		public string[] messageBind = new string[10];
		public string lastCMD = "";

		public Level level = Server.mainLevel;
		public bool Loading = true; //True if player is loading a map.
		public ushort[] lastClick = new ushort[] { 0, 0, 0 };

		public ushort[] pos = new ushort[] { 0, 0, 0 };
		ushort[] oldpos = new ushort[] { 0, 0, 0 };
		public byte[] rot = new byte[] { 0, 0 };
		byte[] oldrot = new byte[] { 0, 0 };

		// grief/spam detection
		public static int spamBlockCount = 200;
		public static int spamBlockTimer = 5;
		Queue<DateTime> spamBlockLog = new Queue<DateTime>(spamBlockCount);

		//Random...
		public Random random = new Random();

		public bool loggedIn;
		public Dictionary<string, string> sounds = new Dictionary<string, string>();

		public bool isDev, isMod; //is this player a dev/mod?
		public bool isStaff;
		public bool verifiedName;

		public string appName;
		public int extensionCount;
		public List<string> extensions = new List<string>();
		public bool extension;

		public struct OfflinePlayer
		{
			public string name, color, title, titleColor;
			public int money;
			//need moar? add moar! just make sure you adjust Player.FindOffline() method
			/// <summary>
			/// Creates a new OfflinePlayer object.
			/// </summary>
			/// <param name="nm">Name of the player.</param>
			/// <param name="clr">Color of player name.</param>
			/// <param name="tl">Title of player.</param>
			/// <param name="tlclr">Title color of player</param>
			/// <param name="mon">Player's money.</param>
			public OfflinePlayer(string nm, string clr, string tl, string tlclr, int mon) { name = nm; color = clr; title = tl; titleColor = tlclr; money = mon; }
		}

		public bool CheckIfInsideBlock()
		{
			return CheckIfInsideBlock(this);
		}

		public static bool CheckIfInsideBlock(Player p)
		{
			ushort x, y, z;
			x = (ushort)(p.pos[0] / 32);
			y = (ushort)(p.pos[1] / 32);
			y = (ushort)Math.Round((decimal)(((y * 32) + 4) / 32));
			z = (ushort)(p.pos[2] / 32);

			ushort b = p.level.GetTile(x, y, z);
			ushort b1 = p.level.GetTile(x, (ushort)(y - 1), z);

			if (Block.Walkthrough(Block.Convert(b)) && Block.Walkthrough(Block.Convert(b1)))
			{
				return false;
			}
			return Block.Convert(b) != Block.Zero;
		}

		//This is so that plugin devs can declare a player without needing a socket..
		//They would still have to do p.Dispose()..
		public Player(string playername) { name = playername; }

		public NetworkStream Stream;
		public BinaryReader Reader;

		public Player(Socket s)
		{
			try
			{
				socket = s;
				ip = socket.RemoteEndPoint.ToString().Split(':')[0];

				exIP = ip;

				Server.s.Log(ip + " connected to the server.");

				for (byte i = 0; i < 128; ++i) bindings[i] = i;

				socket.BeginReceive(tempbuffer, 0, tempbuffer.Length, SocketFlags.None, new AsyncCallback(Receive), this);
				timespent.Elapsed += delegate
				{
					if (!Loading)
					{
						try
						{
							int Days = Convert.ToInt32(time.Split(' ')[0]);
							int Hours = Convert.ToInt32(time.Split(' ')[1]);
							int Minutes = Convert.ToInt32(time.Split(' ')[2]);
							int Seconds = Convert.ToInt32(time.Split(' ')[3]);
							Seconds++;
							if (Seconds >= 60) { Minutes++; Seconds = 0; }
							if (Minutes >= 60) { Hours++; Minutes = 0; }
							if (Hours >= 24) { Days++; Hours = 0; }
							time = "" + Days + " " + Hours + " " + Minutes + " " + Seconds;
						}
						catch { time = "0 0 0 1"; }
					}
				};
				timespent.Start();
				loginTimer.Elapsed += delegate
				{
					if (!Loading)
					{
						loginTimer.Stop();
						// Makes so if the permission's Player or Banned, can't place blocks.
						if (this.group.Permission <= LevelPermission.Guest)
						{
							for (byte blockid = 1; blockid <= 49; blockid++)
							{
								SendSetBlockPermission(blockid, 0, 1);
							}
						}
						if (File.Exists("text/welcome.txt"))
						{
							try
							{
								using (StreamReader wm = File.OpenText("text/welcome.txt"))
								{
									List<string> welcome = new List<string>();
									while (!wm.EndOfStream)
										welcome.Add(wm.ReadLine());
									foreach (string w in welcome)
										SendMessage(w);
								}
							}
							catch { }
						}
						else
						{
							File.WriteAllText("text/welcome.txt", "Welcome to my server!");
							SendMessage("Welcome to my server!");
						}
						extraTimer.Start();
						loginTimer.Dispose();
					}
				}; loginTimer.Start();

				pingTimer.Elapsed += delegate { SendPing(); };
				pingTimer.Start();

				extraTimer.Elapsed += delegate
				{
					extraTimer.Stop();

					try
					{
						if (!Group.Find("Nobody").commands.Contains("pay") && !Group.Find("Nobody").commands.Contains("give") && !Group.Find("Nobody").commands.Contains("take")) SendMessage("You currently have &a" + money + Server.DefaultColor + " " + Server.moneys);
					}
					catch { }
					if (players.Count == 1)
						SendMessage("There is currently &a" + players.Count + " player online.");
					else
						SendMessage("There are currently &a" + players.Count + " players online.");
					try
					{
						if (!Group.Find("Nobody").commands.Contains("award") && !Group.Find("Nobody").commands.Contains("awards") && !Group.Find("Nobody").commands.Contains("awardmod")) SendMessage("You have " + Awards.awardAmount(name) + " awards.");
					}
					catch { }

					extraTimer.Dispose();
				};

				connections.Add(this);
			}
			catch (Exception e) { Kick("Login failed!"); Server.ErrorLog(e); }
		}

		public DateTime lastlogin;
		public void save()
		{
			PlayerDB.Save(this);

			try
			{
				if (!smileySaved)
				{
					if (parseSmiley)
						emoteList.RemoveAll(s => s == name);
					else
						emoteList.Add(name);

					File.WriteAllLines("text/emotelist.txt", emoteList.ToArray());
					smileySaved = true;
				}
			}
			catch (Exception e)
			{
				Server.ErrorLog(e);
			}
		}

		#region == INCOMING ==
		static void Receive(IAsyncResult result)
		{
			Player p = (Player)result.AsyncState;
			if (p.disconnected || p.socket == null)
				return;
			try
			{
				int length = p.socket.EndReceive(result);
				if (length == 0) { p.Disconnect(); return; }

				byte[] b = new byte[p.buffer.Length + length];
				Buffer.BlockCopy(p.buffer, 0, b, 0, p.buffer.Length);
				Buffer.BlockCopy(p.tempbuffer, 0, b, p.buffer.Length, length);

				p.buffer = p.HandleMessage(b);
				if (p.dontmindme && p.buffer.Length == 0)
				{
					Server.s.Log("Disconnected");
					p.socket.Close();
					p.disconnected = true;
					return;
				}
				if (!p.disconnected)
					p.socket.BeginReceive(p.tempbuffer, 0, p.tempbuffer.Length, SocketFlags.None,
										  new AsyncCallback(Receive), p);
			}
			catch (SocketException) { p.Disconnect(); }
			catch (ObjectDisposedException)
			{
				if (connections.Contains(p))
					connections.Remove(p);
				p.disconnected = true;
			}
			catch (Exception e)
			{
				Server.ErrorLog(e);
				p.Kick("Error!");
			}
		}
		byte[] HandleMessage(byte[] buffer)
		{
			try
			{
				int length = 0; byte msg = buffer[0];
				// Get the length of the message by checking the first byte
				switch (msg)
				{
					case 0:
						length = 130;
						break; // login
					case 5:
						if (!loggedIn)
							goto default;
						length = 8;
						break; // blockchange
					case 8:
						if (!loggedIn)
							goto default;
						length = 9;
						break; // input
					case 13:
						if (!loggedIn)
							goto default;
						length = 65;
						break; // chat
					case 16:
						length = 66;
						break;
					case 17:
						length = 68;
						break;
					case 19:
						length = 1;
						break;
					default:
						if (!dontmindme)
							Kick("Unhandled message id \"" + msg + "\"!");
						else
							Server.s.Log(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
						return new byte[0];
				}
				if (buffer.Length > length)
				{
					byte[] message = new byte[length];
					Buffer.BlockCopy(buffer, 1, message, 0, length);

					byte[] tempbuffer = new byte[buffer.Length - length - 1];
					Buffer.BlockCopy(buffer, length + 1, tempbuffer, 0, buffer.Length - length - 1);

					buffer = tempbuffer;

					// Thread thread = null;
					switch (msg)
					{
						case 0:
							HandleLogin(message);
							break;
						case 5:
							if (!loggedIn)
								break;
							HandleBlockchange(message);
							break;
						case 8:
							if (!loggedIn)
								break;
							HandleInput(message);
							break;
						case 13:
							if (!loggedIn)
								break;
							HandleChat(message);
							break;
						case 16:
							HandleExtInfo(message);
							break;
						case 17:
							HandleExtEntry(message);
							break;
					}
					//thread.Start((object)message);
					if (buffer.Length > 0)
						buffer = HandleMessage(buffer);
					else
						return new byte[0];
				}
			}
			catch (Exception e)
			{
				Server.ErrorLog(e);
			}
			return buffer;
		}

		public void HandleExtInfo(byte[] message)
		{
			appName = enc.GetString(message, 0, 64).Trim();
			extensionCount = message[65];
		}
		public struct CPE { public string name; public int version; }
		public List<CPE> ExtEntry = new List<CPE>();
		void HandleExtEntry(byte[] msg)
		{
			AddExtension(enc.GetString(msg, 0, 64).Trim(), NTHO_Int(msg, 64));
			extensionCount--;
		}

		public static int NTHO_Int(byte[] x, int offset)
		{
			byte[] y = new byte[4];
			Buffer.BlockCopy(x, offset, y, 0, 4); Array.Reverse(y);
			return BitConverter.ToInt32(y, 0);
		}

		void HandleLogin(byte[] message)
		{
			try
			{
				if (loggedIn)
					return;
				byte version = message[0];
				name = enc.GetString(message, 1, 64).Trim();
				DisplayName = name;
				SkinName = name;
				truename = name;

				string verify = enc.GetString(message, 65, 32).Trim();
				ushort type = message[129];

				verifiedName = Server.verify ? true : false;
				if (Server.verify)
				{
					if (verify == BitConverter.ToString(md5.ComputeHash(enc.GetBytes(Server.salt + truename))).Replace("-", "").ToLower().TrimStart('0'))
						identified = true;

					if (IPInPrivateRange(ip)) { identified = true; }
					if (identified == false) { Kick("Login failed! Try again."); return; }
				}

				// ban check
				if (Server.bannedIP.Contains(ip)) { Kick("You're banned!"); return; }
				if (Group.findPlayerGroup(name) == Group.findPerm(LevelPermission.Banned)) { Kick("You're banned!"); return; }

				//server maxplayer check
				if (!isDev && !isMod)
				{
					if (Player.players.Count >= Server.players && !IPInPrivateRange(ip)) { Kick("Server full!"); return; }
				}

				if (version != Server.version) { Kick("Wrong version!"); return; }

				foreach (Player p in players)
				{
					if (p.name == name)
					{
						if (Server.verify) { p.Kick("Someone logged in as you!"); break; }
						else { Kick("Already logged in!"); return; }
					}
				}
				if (type == 0x42)
				{
					extension = true;
					SendExtInfo(10);
					SendExtEntry("ClickDistance", 1);
					SendExtEntry("HeldBlock", 1);
					SendExtEntry("TextHotKey", 1);
					SendExtEntry("EnvColors", 1);
					SendExtEntry("SelectionCuboid", 1);
					SendExtEntry("BlockPermissions", 1);
					SendExtEntry("EnvMapAppearance", 1);
					SendExtEntry("HackControl", 1);
					SendExtEntry("EmoteFix", 1);
					SendExtEntry("MessageTypes", 1);
				}


				try { left.Remove(name.ToLower()); }
				catch { }

				group = Group.findPlayerGroup(name);

				SendMotd();
				SendMap();
				Loading = true;

				if (disconnected) return;

				this.id = FreeId();

				lock (players)
					players.Add(this);

				connections.Remove(this);

				Server.s.PlayerListUpdate();
				//Test code to show when people come back with different accounts on the same IP
				string temp = name + " is lately known as:";
				bool found = false;
				if (!ip.StartsWith("127.0.0."))
				{
					foreach (KeyValuePair<string, string> prev in left)
					{
						if (prev.Value == ip)
						{
							found = true;
							temp += " " + prev.Key;
						}
					}
					if (found) { Server.s.Log(temp); }
				}
			}
			catch (Exception e)
			{
				Server.ErrorLog(e);
				Player.GlobalMessage("An error occurred: " + e.Message);
			}
			//OpenClassic Client Check
			SendBlockchange(0, 0, 0, 0);

			if(!Directory.Exists("players"))
				Directory.CreateDirectory ("players");

			PlayerDB.Load (this);
				SendMessage("Welcome back " + color + prefix + name + Server.DefaultColor + "! You've been here " + totalLogins + " times!"); {
				if (Server.muted.Contains(name))
				{
					muted = true;
					GlobalMessage(name + " is still muted from the last time they went offline.");
				}
			}

			SetPrefix();

			if (PlayerConnect != null)
				PlayerConnect(this);

			//Re-implenting MCLawl-Era Dev recognition.
			if (isDev)
			{
				if (color == Group.standard.color) { color = "&9"; }
				if (prefix == "") { title = "Dev"; }
				SetPrefix();
			}

			if (!spawned)
			{
				try
				{
					ushort x = (ushort)((0.5 + level.spawnx) * 32);
					ushort y = (ushort)((1 + level.spawny) * 32);
					ushort z = (ushort)((0.5 + level.spawnz) * 32);
					pos = new ushort[3] { x, y, z }; rot = new byte[2] { level.rotx, level.roty };

					GlobalSpawn(this, x, y, z, rot[0], rot[1], true);
					foreach (Player p in players)
					{
						if (p.level == level && p != this && !p.hidden)
							SendSpawn(p.id, p.color + p.name, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1], p.DisplayName, p.SkinName);
					}
					if (HasExtension("EnvMapAppearance"))
						SendSetMapAppearance(Server.textureUrl, 7, 8, (short)(level.depth/2));
				}
				catch (Exception e)
				{
					Server.ErrorLog(e);
					Server.s.Log("Error spawning player \"" + name + "\"");
				}
				spawned = true;
			}

			Loading = false;

			if (emoteList.Contains(name)) parseSmiley = false;

			loggedIn = true;
			lastlogin = DateTime.Now;
			//very very sloppy, yes I know.. but works for the time
			//^Perhaps we should update this? -EricKilla
			//which bit is this referring to? - HeroCane


			string joinm = "&a+ " + this.color + this.prefix + this.name + Server.DefaultColor + " joined the server.";

			Player.players.ForEach(p1 => { Player.SendMessage(p1, joinm); });

			try
			{
				if (File.Exists("ranks/muted.txt"))
				{
					using (StreamReader read = new StreamReader("ranks/muted.txt"))
					{
						string line;
						while ((line = read.ReadLine()) != null)
						{
							if (line.ToLower() == this.name.ToLower())
							{
								this.muted = true;
								Player.SendMessage(this, "!%cYou are still %8muted%c since your last login.");
								break;
							}
						}
					}
				}
				else { File.Create("ranks/muted.txt").Close(); }
			}
			catch { muted = false; }
			Server.s.Log(name + " [" + ip + "] + has joined the server.");
		}

		public void SetPrefix()
		{
			string viptitle = isDev ? string.Format("{1}[{0}Dev{1}] ", c.Parse("blue"), color) : isMod ? string.Format("{1}[{0}Mod{1}] ", c.Parse("lime"), color) : "";
			prefix = (title == "") ? "" : (titlecolor == "") ? color + "[" + title + "] " : color + "[" + titlecolor + title + color + "] ";
			prefix = viptitle + prefix;
		}

		void HandleBlockchange(byte[] message)
		{
			int section = 0;
			try
			{
				if (!loggedIn)
					return;
				if (CheckBlockSpam())
					return;

				section++;
				ushort x = NTHO(message, 0);
				ushort y = NTHO(message, 2);
				ushort z = NTHO(message, 4);
				byte action = message[6];
				ushort type = message[7];

				manualChange(x, y, z, action, type);
			}
			catch (Exception e)
			{
				GlobalMessageOps(name + " has triggered a block change error");
				GlobalMessageOps(e.GetType().ToString() + ": " + e.Message);
				Server.ErrorLog(e);
			}
		}

		public void manualChange(ushort x, ushort y, ushort z, byte action, ushort type)
		{
			if (type > 65)
			{
				Kick("Unknown block type!");
				return;
			}

			ushort b = level.GetTile(x, y, z);

			if (b == Block.Zero) { return; }

			if (!deleteMode)
			{
				string info = level.foundInfo(x, y, z);
				if (info.Contains("wait")) { return; }
			}

			if (!canBuild)
			{
				SendBlockchange(x, y, z, b);
				return;
			}

			Blockchange bP = new Blockchange();
			bP.username = name;
			bP.level = level.name;
			bP.timePerformed = DateTime.Now;
			bP.x = x; bP.y = y; bP.z = z;
			bP.type = type;

			lastClick[0] = x;
			lastClick[1] = y;
			lastClick[2] = z;
			if (Blockchange != null)
			{
				if (Blockchange.Method.ToString().IndexOf("AboutBlockchange") == -1 && !level.name.Contains("Museum " + Server.DefaultColor))
				{
					bP.deleted = true;
					level.blockCache.Add(bP);
				}

				Blockchange(this, x, y, z, type);
				return;
			}
			if (cancelBlock)
			{
				cancelBlock = false;
				return;
			}

			if (group.Permission == LevelPermission.Banned) return;
			if (group.Permission == LevelPermission.Guest)
			{
				int Diff = 0;

				Diff = Math.Abs((int)(pos[0] / 32) - x);
				Diff += Math.Abs((int)(pos[1] / 32) - y);
				Diff += Math.Abs((int)(pos[2] / 32) - z);

				if (Diff > 12)
				{
					Server.s.Log(name + " attempted to build with a " + Diff.ToString() + " distance offset");
					GlobalMessageOps("To Ops &f-" + color + name + "&f- attempted to build with a " + Diff.ToString() + " distance offset");
					SendMessage("You can't build that far away.");
					SendBlockchange(x, y, z, b); return;
				}
			}

			if (!Block.canPlace(this, b) && !Block.BuildIn(b) && !Block.AllowBreak(b))
			{
				SendMessage("Cannot build here!");
				SendBlockchange(x, y, z, b);
				return;
			}

			if (!Block.canPlace(this, type))
			{
				SendMessage("You can't place this block type!");
				SendBlockchange(x, y, z, b);
				return;
			}

			if (b >= 200 && b < 220)
			{
				SendMessage("Block is active, you cant disturb it!");
				SendBlockchange(x, y, z, b);
				return;
			}

			if (action > 1) { Kick("Unknown block action!"); }

			ushort oldType = type;
			type = bindings[(int)type];
			if (b == (byte)(( action == 1) ? type : (byte)0))
			{
				if (oldType != type) { SendBlockchange(x, y, z, b); } return;
			}

			if (action == 0)
			{
				bP.deleted = true;
				level.blockCache.Add(bP);
				deleteBlock(b, type, x, y, z);
			}
			else
			{
				bP.deleted = false;
				level.blockCache.Add(bP);
				placeBlock(b, type, x, y, z);
			}
		}

		private bool checkOp() { return group.Permission < LevelPermission.Operator; }

		private void deleteBlock(ushort b, ushort type, ushort x, ushort y, ushort z)
		{
			Random rand = new Random();
			level.Blockchange(this, x, y, z, (ushort)Block.air);
		}

		public void placeBlock(ushort b, ushort type, ushort x, ushort y, ushort z)
		{
			switch (blockAction)
			{
				case 0:
					switch (type)
					{
						case Block.dirt: //instant dirt to grass
							if (Block.LightPass(level.GetTile(x, (ushort)(y + 1), z))) level.Blockchange(this, x, y, z, (byte)(Block.grass));
							else level.Blockchange(this, x, y, z, (byte)(Block.dirt));
							break;
						case Block.staircasestep: //stair handler
							if (level.GetTile(x, (ushort)(y - 1), z) == Block.staircasestep)
							{
								SendBlockchange(x, y, z, Block.air); //send the air block back only to the user.
								level.Blockchange(this, x, (ushort)(y - 1), z, (byte)(Block.staircasefull));
								break;
							}
							level.Blockchange(this, x, y, z, type);
							break;
						default:
							level.Blockchange(this, x, y, z, type);
							break;
					}
					break;
				case 6:
					if (b == modeType) { SendBlockchange(x, y, z, b); return; }
					level.Blockchange(this, x, y, z, modeType);
					break;
				default:
					Server.s.Log(name + " is breaking something");
					blockAction = 0;
					break;
			}
		}

		void HandleInput(object m)
		{
			if (!loggedIn)
				return;

			byte[] message = (byte[])m;

			ushort x = NTHO(message, 1);
			ushort y = NTHO(message, 3);
			ushort z = NTHO(message, 5);

			if (cancelmove)
			{
				unchecked { SendPos((byte)-1, pos[0], pos[1], pos[2], rot[0], rot[1]); }
				return;
			}
			byte rotx = message[7];
			byte roty = message[8];
			pos = new ushort[3] { x, y, z };
			rot = new byte[2] { rotx, roty };
		}

		public void CheckBlock(ushort x, ushort y, ushort z)
		{
			y = (ushort)Math.Round((decimal)(((y * 32) + 4) / 32));

			ushort b = this.level.GetTile(x, y, z);
			ushort b1 = this.level.GetTile(x, (ushort)((int)y - 1), z);

			if (Block.Death(b)) HandleDeath(b); else if (Block.Death(b1)) HandleDeath(b1);
		}

		public void HandleDeath(ushort b, string customMessage = "", bool explode = false)
		{
			ushort x = (ushort)(pos[0] / (ushort)32);
			ushort y = (ushort)(pos[1] / 32);
			ushort z = (ushort)(pos[2] / 32);
			ushort y1 = (ushort)((int)pos[1] / 32 - 1);
			ushort xx = pos[0];
			ushort yy = pos[1];
			ushort zz = pos[2];
			if (lastDeath.AddSeconds(2) < DateTime.Now)
			{
				switch (b)
				{   
					case Block.deathlava:
					case Block.activedeathlava: GlobalChat(this, this.color + this.prefix + this.name + Server.DefaultColor + " stood in &cmagma and melted.", false); break;
				}
				Command.all.Find("spawn").Use(this, "");
				overallDeath++;

				if (Server.deathcount)
					if (overallDeath > 0 && overallDeath % 10 == 0) GlobalChat(this, this.color + this.prefix + this.name + Server.DefaultColor + " has died &3" + overallDeath + " times", false);
				lastDeath = DateTime.Now;
			}
		}

		void HandleChat(byte[] message)
		{
			try
			{
				if (!loggedIn) return;

				string text = enc.GetString(message, 1, 64).Trim();
				text = text.Trim('\0');

				// makes so it doesn't display a "command not found" message
				if (text.Truncate(6) == "/womid")
					return;

				if (MessageHasBadColorCodes(this, text)) return;
				if (storedMessage != "")
				{
					if (!text.EndsWith(">") && !text.EndsWith("<"))
					{
						text = storedMessage.Replace("|>|", " ").Replace("|<|", "") + text;
						storedMessage = "";
					}
				}
				if (text.StartsWith(">") || text.StartsWith("<")) return;
				if (text.EndsWith(">"))
				{
					storedMessage += text.Replace(">", "|>|");
					SendMessage(c.teal + "Partial message: " + c.white + storedMessage.Replace("|>|", " ").Replace("|<|", ""));
					return;
				}
				if (text.EndsWith("<"))
				{
					storedMessage += text.Replace("<", "|<|");
					SendMessage(c.teal + "Partial message: " + c.white + storedMessage.Replace("|<|", "").Replace("|>|", " "));
					return;
				}
				if (Regex.IsMatch(text, "%[^a-f0-9]"))//This causes all players to crash!
				{
					SendMessage(this, "You're not allowed to send that message!");
					return;
				}

				text = Regex.Replace(text, @"\s\s+", " ");
				if (text.Any(ch => ch < 32 || ch >= 127 || ch == '&'))
				{
					Kick("Illegal character in chat message!");
					return;
				}
				if (text.Length == 0)
					return;

				if (text.StartsWith("//"))
				{
					text = text.Remove(0, 1);
					goto hello;
				}

				if (text[0] == '/' || text[0] == '!')
				{
					text = text.Remove(0, 1);

					int pos = text.IndexOf(' ');
					if (pos == -1)
					{
						HandleCommand(text.ToLower(), "");
						return;
					}
					string cmd = text.Substring(0, pos).ToLower();
					string msg = text.Substring(pos + 1);
					HandleCommand(cmd, msg);
					return;
				}
			hello:
				// People who are muted can't speak or vote
				if (muted) { this.SendMessage("You are muted."); return; } //Muted: Only allow commands

				Player.lastMSG = this.name;

				if (text.Length >= 2 && text[0] == '@' && text[1] == '@')
				{
					text = text.Remove(0, 2);
					if (text.Length < 1) { SendMessage("No message entered"); return; }
					SendChat(this, Server.DefaultColor + "[<] Console: &f" + text);
					Server.s.Log("[>] " + this.name + ": " + text);
					return;
				}
				if (text[0] == '@' || whisper)
				{
					string newtext = text;
					if (text[0] == '@') newtext = text.Remove(0, 1).Trim();

					if (whisperTo == "")
					{
						int pos = newtext.IndexOf(' ');
						if (pos != -1)
						{
							string to = newtext.Substring(0, pos);
							string msg = newtext.Substring(pos + 1);
							HandleQuery(to, msg); return;
						}
						else
						{
							SendMessage("No message entered");
							return;
						}
					}
					else
					{
						HandleQuery(whisperTo, newtext);
						return;
					}
				}
				if (text[0] == '#' || opchat)
				{
					string newtext = text;
					if (text[0] == '#') newtext = text.Remove(0, 1).Trim();

					GlobalMessageOps("To Ops &f-" + color + name + "&f- " + newtext);
					if (group.Permission < LevelPermission.Operator && !isStaff)
						SendMessage("To Ops &f-" + color + name + "&f- " + newtext);
					Server.s.Log("(OPs): " + name + ": " + newtext);
					Server.IRC.Say(name + ": " + newtext, true);
					return;
				}

				if (text[0] == '%')
				{
					string newtext = text;
					GlobalChat(this, newtext);
					Server.s.Log("<" + name + "> " + newtext);
					if (OnChat != null)
						OnChat(this, text);
					if (PlayerChat != null)
						PlayerChat(this, text);
					return;
				}
				Server.s.Log("<" + name + "> " + text);
				if (OnChat != null)
					OnChat(this, text);
				if (PlayerChat != null)
					PlayerChat(this, text);
				if (cancelchat)
				{
					cancelchat = false;
					return;
				}
				GlobalChat(this, text);
			}
			catch (Exception e) { Server.ErrorLog(e); Player.GlobalMessage("An error occurred: " + e.Message); }
		}

		public void HandleCommand(string cmd, string message)
		{
			try
			{
				if (cmd == String.Empty) { SendMessage("No command entered."); return; }

				if (cmd.ToLower() == "care") { SendMessage("Dmitchell94 now loves you with all his heart."); return; }
				if (cmd.ToLower() == "facepalm") { SendMessage("Fenderrock87's bot army just simultaneously facepalm'd at your use of this command."); return; }
				if (cmd.ToLower() == "alpaca") { SendMessage("Leitrean's Alpaca Army just raped your woman and pillaged your villages!"); return; }

				if (cmd.ToLower() == "crashserver") { Kick("Server crash! Error code 0x0005A4"); return; }
				//DO NOT REMOVE THE TWO COMMANDS BELOW, /PONY AND /RAINBOWDASHLIKESCOOLTHINGS. -EricKilla
				if (cmd.ToLower() == "pony")
				{
					GlobalMessage(this.color + this.name + Server.DefaultColor + " just so happens to be a proud brony! Everyone give " + this.color + this.name + Server.DefaultColor + " a brohoof!");
					return;
				}
				if (cmd.ToLower() == "rainbowdashlikescoolthings")
				{
					GlobalMessage("&1T&2H&3I&4S &5S&6E&7R&8V&9E&aR &bJ&cU&dS&eT &fG&0O&1T &22&30 &4P&CE&7R&DC&EE&9N&1T &5C&6O&7O&8L&9E&aR&b!");
					return;
				}

				if (CommandHasBadColourCodes(this, message))
					return;
				if (cancelcommand)
				{
					cancelcommand = false;
					return;
				}
				try
				{
					int foundCb = int.Parse(cmd);
					if (messageBind[foundCb] == null) { SendMessage("No CMD is stored on /" + cmd); return; }
					message = messageBind[foundCb] + " " + message;
					message = message.TrimEnd(' ');
					cmd = cmdBind[foundCb];
				}
				catch { }

				Command command = Command.all.Find(cmd);
				if (command != null)
				{
					if (group.CanExecute(command))
					{
						//TODO: /repeat doesn't exist. Try to rewrite it.
						if (cmd != "repeat") lastCMD = cmd + " " + message;
						if (this.muted == true && cmd.ToLower() == "me")
						{
							SendMessage("Cannot use /me while muted");
							return;
						}
						Server.s.CommandUsed(name + " used /" + cmd + " " + message);

						this.commThread = new Thread(new ThreadStart(delegate
						{
							try { command.Use(this, message); }
							catch (Exception e)
							{
								Server.ErrorLog(e);
								Player.SendMessage(this, "An error occured when using the command!");
								Player.SendMessage(this, e.GetType().ToString() + ": " + e.Message);
							}
							//finally { if (old != null) this.group = old; }
						}));
						commThread.Start();
					}
					else { SendMessage("You are not allowed to use \"" + cmd + "\"!"); }
				}
				else if (Block.Ushort(cmd.ToLower()) != Block.Zero)
					HandleCommand("mode", cmd.ToLower());
				else
				{
					bool retry = true;

					switch (cmd.ToLower())
					{
						case "ops": message = "op"; cmd = "viewranks"; break;
						case "banned": message = cmd; cmd = "viewranks"; break;

						case "cmdlist":
						case "commands": cmd = "help"; message = "commands"; break;
						case "colour": cmd = "color"; break;

						//Shortcuts
						case "z": cmd = "cuboid"; break;
						case "k": cmd = "kick"; break;

						default: retry = false; break; //Unknown command, then
					}

					if (retry) HandleCommand(cmd, message);
					else SendMessage("Unknown command \"" + cmd + "\"!");
				}
			}
			catch (Exception e) { Server.ErrorLog(e); SendMessage("Command failed."); }
		}

		void HandleQuery(string to, string message)
		{
			Player p = Find(to);
			if (p == this) { SendMessage("Trying to talk to yourself, huh?"); return; }
			if (p == null) { SendMessage("Could not find player."); return; }
			if (p.hidden) { if (this.hidden == false) { Player.SendMessage(p, "Could not find player."); } }
			if (p.ignoreglobal == true)
			{
					if (this.group.Permission >= LevelPermission.Operator)
					{
						if (p.group.Permission < this.group.Permission)
						{
							Server.s.Log(name + " @" + p.name + ": " + message);
							SendChat(this, Server.DefaultColor + "[<] " + p.color + p.prefix + p.name + ": &f" + message);
							SendChat(p, "&9[>] " + this.color + this.prefix + this.name + ": &f" + message);
							return;
						}
					}
				Server.s.Log(name + " @" + p.name + ": " + message);
				SendChat(this, Server.DefaultColor + "[<] " + p.color + p.prefix + p.name + ": &f" + message);
				return;
			}
			foreach (string ignored2 in p.listignored)
			{
				if (ignored2 == this.name)
				{
					Server.s.Log(name + " @" + p.name + ": " + message);
					SendChat(this, Server.DefaultColor + "[<] " + p.color + p.prefix + p.name + ": &f" + message);
					return;
				}
			}
			if (p != null && !p.hidden || p.hidden && this.group.Permission >= p.group.Permission)
			{
				Server.s.Log(name + " @" + p.name + ": " + message);
				SendChat(this, Server.DefaultColor + "[<] " + p.color + p.prefix + p.name + ": &f" + message);
				SendChat(p, "&9[>] " + this.color + this.prefix + this.name + ": &f" + message);
			}
			else { SendMessage("Player \"" + to + "\" doesn't exist!"); }
		}

		#endregion
		#region == OUTGOING ==

		public void SendRaw(OpCode id) { SendRaw(id, new byte[0]); }
		public void SendRaw(OpCode id, byte send) { SendRaw(id, new byte[] { send }); }
		public void SendRaw(OpCode id, byte[] send)
		{
			// Abort if socket has been closed
			if (socket == null || !socket.Connected)
				return;
			byte[] buffer = new byte[send.Length + 1];
			buffer[0] = (byte)id;
			for (int i = 0; i < send.Length; i++) { buffer[i + 1] = send[i]; }
			try
			{
				socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, delegate(IAsyncResult result) { }, Block.air);
				buffer = null;
			}
			catch (SocketException e)
			{
				buffer = null;
				Disconnect();
#if DEBUG
				Server.ErrorLog(e);
#endif
			}
		}

		public static void SendMessage(Player p, string message)
		{
			if (p == null) { Server.s.Log(message); return; }
			SendMessage(p, MessageType.Chat, message, true);
		}
		public static void SendMessage(Player p, MessageType type, string message, bool colorParse)
		{
			if (p == null)
			{
				if (storeHelp) { storedHelp += message + "\r\n"; }
				else
				{
					if (!Server.irc || String.IsNullOrEmpty(Server.IRC.usedCmd))
						Server.s.Log(message);
					else
						Server.IRC.Pm(Server.IRC.usedCmd, message);
				}
				return;
			}
			p.SendMessage(type, Server.DefaultColor + message, colorParse);
		}

		public void SendMessage(string message) { SendMessage(message, true); }
		public void SendMessage(string message, bool colorParse)
		{
			if (this == null) { Server.s.Log(message); return; }
			unchecked { SendMessage(MessageType.Chat, Server.DefaultColor + message, colorParse); }
		}
		public void SendChat(Player p, string message)
		{
			if (this == null) { Server.s.Log(message); return; }
			Player.SendMessage(p, message);
		}
		public void SendMessage(byte id, string message)
		{
			SendMessage(MessageType.Chat, message, true);
		}

		public enum MessageType
		{
			Chat = (byte)0,
			Status1 = (byte)1,
			Status2 = (byte)2,
			Status3 = (byte)3,
			BottomRight1 = (byte)11,
			BottomRight2 = (byte)12,
			BottomRight3 = (byte)13,
			Announcement = (byte)100
		}

		public void SendMessage (MessageType type, string message, bool colorParse)
		{
			if (this == null) {
				Server.s.Log (message);
				return;
			}

			byte[] buffer = new byte[65];
			unchecked {
				buffer [0] = (byte)type;
			}

			StringBuilder sb = new StringBuilder (message);

			if (colorParse) {
				for (int i = 0; i < 10; i++) {
					sb.Replace ("%" + i, "&" + i);
					sb.Replace ("&" + i + " &", " &");
				}
				for (char ch = 'a'; ch <= 'f'; ch++) {
					sb.Replace ("%" + ch, "&" + ch);
					sb.Replace ("&" + ch + " &", " &");
				}
				// Begin fix to replace all invalid color codes typed in console or chat with "."
				for (char ch = (char)0; ch <= (char)47; ch++) // Characters that cause clients to disconnect
					sb.Replace ("&" + ch, String.Empty);
				for (char ch = (char)58; ch <= (char)96; ch++) // Characters that cause clients to disconnect
					sb.Replace ("&" + ch, String.Empty);
				for (char ch = (char)103; ch <= (char)127; ch++) // Characters that cause clients to disconnect
					sb.Replace ("&" + ch, String.Empty);
				// End fix
			}

			sb.Replace ("$name", name);
			sb.Replace ("$date", DateTime.Now.ToString ("yyyy-MM-dd"));
			sb.Replace ("$time", DateTime.Now.ToString ("HH:mm:ss"));
			sb.Replace ("$ip", ip);
			sb.Replace ("$serverip", IsLocalIpAddress (ip) ? ip : Server.IP);
			if (colorParse)
				sb.Replace ("$color", color);
			sb.Replace ("$rank", group.name);
			sb.Replace ("$level", level.name);
			sb.Replace ("$deaths", overallDeath.ToString ());
			sb.Replace ("$money", money.ToString ());
			sb.Replace ("$blocks", overallBlocks.ToString ());
			sb.Replace ("$first", firstLogin.ToString ());
			sb.Replace ("$kicked", totalKicked.ToString ());
			sb.Replace ("$server", Server.name);
			sb.Replace ("$motd", Server.motd);
			sb.Replace ("$banned", Player.GetBannedCount ().ToString ());
			sb.Replace ("$irc", Server.ircServer + " > " + Server.ircChannel);

			foreach (var customReplacement in Server.customdollars) {
				if (!customReplacement.Key.StartsWith ("//")) {
					try { sb.Replace (customReplacement.Key, customReplacement.Value); }
					catch { }
				}
			}

			sb.Replace(":)", "(darksmile)");
			sb.Replace(":D", "(smile)");
			sb.Replace("<3", "(heart)");
			
			byte[] stored = new byte[1];

			stored[0] = (byte)1;
			sb.Replace("(darksmile)", enc.GetString(stored));
			stored[0] = (byte)2;
			sb.Replace("(smile)", enc.GetString(stored));
			stored[0] = (byte)3;
			sb.Replace("(heart)", enc.GetString(stored));
			stored[0] = (byte)4;
			sb.Replace("(diamond)", enc.GetString(stored));
			stored[0] = (byte)7;
			sb.Replace("(bullet)", enc.GetString(stored));
			stored[0] = (byte)8;
			sb.Replace("(hole)", enc.GetString(stored));
			stored[0] = (byte)11;
			sb.Replace("(male)", enc.GetString(stored));
			stored[0] = (byte)12;
			sb.Replace("(female)", enc.GetString(stored));
			stored[0] = (byte)15;
			sb.Replace("(sun)", enc.GetString(stored));
			stored[0] = (byte)16;
			sb.Replace("(right)", enc.GetString(stored));
			stored[0] = (byte)17;
			sb.Replace("(left)", enc.GetString(stored));
			stored[0] = (byte)19;
			sb.Replace("(double)", enc.GetString(stored));
			stored[0] = (byte)22;
			sb.Replace("(half)", enc.GetString(stored));
			stored[0] = (byte)24;
			sb.Replace("(uparrow)", enc.GetString(stored));
			stored[0] = (byte)25;
			sb.Replace("(downarrow)", enc.GetString(stored));
			stored[0] = (byte)26;
			sb.Replace("(rightarrow)", enc.GetString(stored));
			stored[0] = (byte)30;
			sb.Replace("(up)", enc.GetString(stored));
			stored[0] = (byte)31;
			sb.Replace("(down)", enc.GetString(stored));

			message = ReplaceEmoteKeywords(sb.ToString());
			if (HasBadColorCodes(message))
				return;
			int totalTries = 0;
			if (MessageRecieve != null)
				MessageRecieve(this, message);
			if (OnMessageRecieve != null)
				OnMessageRecieve(this, message);
			if (cancelmessage)
			{
				cancelmessage = false;
				return;
			}
		retryTag: try
			{
				foreach (string line in Wordwrap(message))
				{
					string newLine = line;
					if (newLine.TrimEnd(' ')[newLine.TrimEnd(' ').Length - 1] < '!')
					{
						if (HasExtension("EmoteFix")) { newLine += '\''; }
					}

					if (HasBadColorCodes(newLine))
						continue;

					StringFormat(newLine, 64).CopyTo(buffer, 1);
					SendRaw(OpCode.Message, buffer);
				}
			}
			catch (Exception e)
			{
				message = "&f" + message;
				totalTries++;
				if (totalTries < 10) goto retryTag;
				else Server.ErrorLog(e);
			}
		}

		public void SendMotd()
		{
			byte[] buffer = new byte[130];
			buffer[0] = (byte)8;
			StringFormat(Server.name, 64).CopyTo(buffer, 1);
			StringFormat(Server.motd, 64).CopyTo(buffer, 65);

			if (Block.canPlace(this, Block.blackrock))
				buffer[129] = 100;
			else
				buffer[129] = 0;
			SendRaw(0, buffer);

		}

		public void SendUserMOTD()
		{
			byte[] buffer = new byte[130];
			buffer[0] = Server.version;
			if (level.motd == "ignore")
			{
				StringFormat(Server.name, 64).CopyTo(buffer, 1);
				StringFormat(Server.motd, 64).CopyTo(buffer, 65);
			}
			else StringFormat(level.motd, 128).CopyTo(buffer, 1);

			if (Block.canPlace(this.group.Permission, Block.blackrock))
				buffer[129] = 100;
			else
				buffer[129] = 0;
			SendRaw(0, buffer);
		}

		public void SendMap()
		{
			if (level.blocks == null) return;
			try
			{
				byte[] buffer = new byte[level.blocks.Length + 4];
				BitConverter.GetBytes(IPAddress.HostToNetworkOrder(level.blocks.Length)).CopyTo(buffer, 0);
			for ( int i = 0; i < level.blocks.Length; ++i ) {
				buffer[4 + i] = (byte)Block.Convert( level.blocks[i] );
			}
				SendRaw(OpCode.MapBegin);
				buffer = buffer.GZip();
				int number = (int)Math.Ceiling(((double)buffer.Length) / 1024);
				for (int i = 1; buffer.Length > 0; ++i)
				{
					short length = (short)Math.Min(buffer.Length, 1024);
					byte[] send = new byte[1027];
					HTNO(length).CopyTo(send, 0);
					Buffer.BlockCopy(buffer, 0, send, 2, length);
					byte[] tempbuffer = new byte[buffer.Length - length];
					Buffer.BlockCopy(buffer, length, tempbuffer, 0, buffer.Length - length);
					buffer = tempbuffer;
					send[1026] = (byte)(i * 100 / number);
					SendRaw(OpCode.MapChunk, send);
					if (ip == "127.0.0.1") { }
					else if (Server.updateTimer.Interval > 1000) Thread.Sleep(100);
					else Thread.Sleep(10);
				} buffer = new byte[6];
				HTNO((short)level.width).CopyTo(buffer, 0);
				HTNO((short)level.depth).CopyTo(buffer, 2);
				HTNO((short)level.height).CopyTo(buffer, 4);

				SendRaw(OpCode.MapEnd, buffer);
				Loading = false;
			}
			catch (Exception ex)
			{
				Command.all.Find("goto").Use(this, Server.mainLevel.name);
				SendMessage("There was an error sending the map data, you have been sent to the main level.");
				Server.ErrorLog(ex);
			}
			finally
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}
		public void SendSpawn(byte id, string name, ushort x, ushort y, ushort z, byte rotx, byte roty, string displayName, string skinName)
		{
			if (!HasExtension("ExtPlayerList", 2))
			{
				name = name.TrimEnd('+');
				byte[] buffer = new byte[73]; buffer[0] = id;
				StringFormat(name, 64).CopyTo(buffer, 1);
				HTNO(x).CopyTo(buffer, 65);
				HTNO(y).CopyTo(buffer, 67);
				HTNO(z).CopyTo(buffer, 69);
				buffer[71] = rotx; buffer[72] = roty;
				SendRaw(OpCode.AddEntity, buffer);
			}
			else
			{
				byte[] buffer = new byte[137];
				buffer[0] = id;
				StringFormat(displayName, 64).CopyTo(buffer, 1);
				StringFormat(skinName, 64).CopyTo(buffer, 65);
				HTNO(x).CopyTo(buffer, 129);
				HTNO(y).CopyTo(buffer, 131);
				HTNO(z).CopyTo(buffer, 133);
				buffer[135] = rotx;
				buffer[136] = roty;
				SendRaw((OpCode)33, buffer);
			}
		}
		public void SendPos(byte id, ushort x, ushort y, ushort z, byte rotx, byte roty)
		{
			if (x < 0) x = 32;
			if (y < 0) y = 32;
			if (z < 0) z = 32;
			if (x > level.width * 32) x = (ushort)(level.width * 32 - 32);
			if (z > level.height * 32) z = (ushort)(level.height * 32 - 32);
			if (x > 32767) x = 32730;
			if (y > 32767) y = 32730;
			if (z > 32767) z = 32730;

			pos[0] = x; pos[1] = y; pos[2] = z;
			rot[0] = rotx; rot[1] = roty;

			byte[] buffer = new byte[9]; buffer[0] = id;
			HTNO(x).CopyTo(buffer, 1);
			HTNO(y).CopyTo(buffer, 3);
			HTNO(z).CopyTo(buffer, 5);
			buffer[7] = rotx; buffer[8] = roty;
			SendRaw(OpCode.Teleport, buffer);
		}
		// Update user type for weather or not they are opped
		public void SendUserType(bool op)
		{
			SendRaw(OpCode.SetPermission, op ? (byte)100 : (byte)0);
		}
		public void SendDie(byte id) { SendRaw(OpCode.RemoveEntity, new byte[1] { id }); }
		public void SendBlockchange(ushort x, ushort y, ushort z, ushort type)
		{
			if (type == Block.air) { type = 0; }
			if (x < 0 || y < 0 || z < 0) return;
			if (type > Block.maxblocks)
			{
				this.SendMessage("The server was not able to detect your held block, please try again!");
				return;
			}
			if (x >= level.width || y >= level.depth || z >= level.height) return;

			byte[] buffer = new byte[7];
			HTNO(x).CopyTo(buffer, 0);
			HTNO(y).CopyTo(buffer, 2);
			HTNO(z).CopyTo(buffer, 4);

			SendRaw(OpCode.SetBlockServer, buffer);
		}
		void SendKick(string message) { SendRaw(OpCode.Kick, StringFormat(message, 64)); }
		void SendPing() { SendRaw(OpCode.Ping); }

		public void SendExtInfo(short count)
		{
			byte[] buffer = new byte[66];
			//StringFormat("MCForge Version: " + Server.Version, 64).CopyTo(buffer, 0);
			StringFormat("lol.wom", 64).CopyTo(buffer, 0);
			HTNO(count).CopyTo(buffer, 64);
			SendRaw(OpCode.ExtInfo, buffer);
		}
		public void SendExtEntry(string name, int version)
		{
			byte[] version_ = BitConverter.GetBytes(version);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(version_);
			byte[] buffer = new byte[68];
			StringFormat(name, 64).CopyTo(buffer, 0);
			version_.CopyTo(buffer, 64);
			SendRaw(OpCode.ExtEntry, buffer);
		}
		public void SendClickDistance(short distance)
		{
			byte[] buffer = new byte[2];
			HTNO(distance).CopyTo(buffer, 0);
			SendRaw(OpCode.SetClickDistance, buffer);
		}
		public void SendHoldThis(byte type, byte locked)
		{
			byte[] buffer = new byte[2];
			buffer[0] = type;
			buffer[1] = locked;
			SendRaw(OpCode.HoldThis, buffer);
		}
		public void SendTextHotKey(string label, string command, int keycode, byte mods)
		{
			byte[] buffer = new byte[133];
			StringFormat(label, 64).CopyTo(buffer, 0);
			StringFormat(command, 64).CopyTo(buffer, 64);
			BitConverter.GetBytes(keycode).CopyTo(buffer, 128);
			buffer[132] = mods;
			SendRaw(OpCode.SetTextHotKey, buffer);
		}
		public void SendEnvSetColor(byte type, short r, short g, short b)
		{
			byte[] buffer = new byte[7];
			buffer[0] = type;
			HTNO(r).CopyTo(buffer, 1);
			HTNO(g).CopyTo(buffer, 3);
			HTNO(b).CopyTo(buffer, 5);
			SendRaw(OpCode.EnvSetColor, buffer);
		}
		public void SendMakeSelection(byte id, string label, short smallx, short smally, short smallz, short bigx, short bigy, short bigz, short r, short g, short b, short opacity)
		{
			byte[] buffer = new byte[85];
			buffer[0] = id;
			StringFormat(label, 64).CopyTo(buffer, 1);
			HTNO(smallx).CopyTo(buffer, 65);
			HTNO(smally).CopyTo(buffer, 67);
			HTNO(smallz).CopyTo(buffer, 69);
			HTNO(bigx).CopyTo(buffer, 71);
			HTNO(bigy).CopyTo(buffer, 73);
			HTNO(bigz).CopyTo(buffer, 75);
			HTNO(r).CopyTo(buffer, 77);
			HTNO(g).CopyTo(buffer, 79);
			HTNO(b).CopyTo(buffer, 81);
			HTNO(opacity).CopyTo(buffer, 83);
			SendRaw(OpCode.MakeSelection, buffer);
		}
		public void SendDeleteSelection(byte id)
		{
			byte[] buffer = new byte[1];
			buffer[0] = id;
			SendRaw(OpCode.RemoveSelection, buffer);
		}
		public void SendSetBlockPermission(byte type, byte canplace, byte candelete)
		{
			byte[] buffer = new byte[3];
			buffer[0] = type;
			buffer[1] = canplace;
			buffer[2] = candelete;
			SendRaw(OpCode.SetBlockPermission, buffer);
		}

		public void SendSetMapAppearance(string url, byte sideblock, byte edgeblock, short sidelevel)
		{
			byte[] buffer = new byte[68];
			StringFormat(url, 64).CopyTo(buffer, 0);
			buffer[64] = sideblock;
			buffer[65] = edgeblock;
			HTNO(sidelevel).CopyTo(buffer, 66);
			SendRaw(OpCode.EnvMapAppearance, buffer);
		}

		public void SendHackControl(byte allowflying, byte allownoclip, byte allowspeeding, byte allowrespawning, byte allowthirdperson, byte allowchangingweather, short maxjumpheight)
		{
			byte[] buffer = new byte[7];
			buffer[0] = allowflying;
			buffer[1] = allownoclip;
			buffer[2] = allowspeeding;
			buffer[3] = allowrespawning;
			buffer[4] = allowthirdperson;
			buffer[5] = allowchangingweather;
			HTNO(maxjumpheight).CopyTo(buffer, 6);
			SendRaw(OpCode.HackControl, buffer);
		}

		public void UpdatePosition()
		{
			byte changed = 0;

			if (oldpos[0] != pos[0] || oldpos[1] != pos[1] || oldpos[2] != pos[2])
				changed |= 1;

			if (oldrot[0] != rot[0] || oldrot[1] != rot[1])
			{
				changed |= 2;
			}

			if (Math.Abs(pos[0] - oldpos[0]) > 32 || Math.Abs(pos[1] - oldpos[1]) > 32 || Math.Abs(pos[2] - oldpos[2]) > 32)
				changed |= 4;
			if (changed == 0) { if (oldpos[0] != pos[0] || oldpos[1] != pos[1] || oldpos[2] != pos[2]) changed |= 1; }

			byte[] buffer = new byte[0]; OpCode msg = 0;
			if ((changed & 4) != 0)
			{
				msg = OpCode.Teleport;
				buffer = new byte[9]; buffer[0] = id;
				HTNO(pos[0]).CopyTo(buffer, 1);
				HTNO(pos[1]).CopyTo(buffer, 3);
				HTNO(pos[2]).CopyTo(buffer, 5);
				buffer[7] = rot[0];
				
				buffer[8] = rot[1];
			}
			else if (changed == 1)
			{
				try
				{
					msg = OpCode.Move;
					buffer = new byte[4]; buffer[0] = id;
					Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[0] - oldpos[0])), 0, buffer, 1, 1);
					Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[1] - oldpos[1])), 0, buffer, 2, 1);
					Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[2] - oldpos[2])), 0, buffer, 3, 1);
				}
				catch { }
			}
			else if (changed == 2)
			{
				msg = OpCode.Rotate;
				buffer = new byte[3]; buffer[0] = id;
				buffer[1] = rot[0];

				buffer[2] = rot[1];
			}
			else if (changed == 3)
			{
				try
				{
					msg = OpCode.MoveRotate;
					buffer = new byte[6]; buffer[0] = id;
					Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[0] - oldpos[0])), 0, buffer, 1, 1);
					Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[1] - oldpos[1])), 0, buffer, 2, 1);
					Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[2] - oldpos[2])), 0, buffer, 3, 1);
					buffer[4] = rot[0];

					buffer[5] = rot[1];
				}
				catch { }
			}

			oldpos = pos; oldrot = rot;
			if (changed != 0)
				try
				{
					foreach (Player p in players) { if (p != this && p.level == level) { p.SendRaw(msg, buffer); } }
				}
				catch { }
		}
		#endregion
		#region == GLOBAL MESSAGES ==
		public static void GlobalBlockchange(Level level, int b, ushort type)
		{
			ushort x, y, z;
			level.IntToPos(b, out x, out y, out z);
			GlobalBlockchange(level, x, y, z, type);
		}
		public static void GlobalBlockchange(Level level, ushort x, ushort y, ushort z, ushort type)
		{
			players.ForEach(delegate(Player p) { if (p.level == level) { p.SendBlockchange(x, y, z, type); } });
		}

		// THIS IS NOT FOR SENDING GLOBAL MESSAGES!!! IT IS TO SEND A MESSAGE FROM A SPECIFIED PLAYER!!!!!!!!!!!!!!
		public static void GlobalChat(Player from, string message) { GlobalChat(from, message, true); }
		public static void GlobalChat(Player from, string message, bool showname)
		{
			if (from == null) return;

			if (MessageHasBadColorCodes(from, message))
				return;

			if (showname)
				message = from.color + from.prefix + from.DisplayName + ": &f" + message;

			players.ForEach(delegate(Player p)
			{
				if (from != null)
				{
					if (!p.listignored.Contains(from.name))
					{
						Player.SendMessage(p, message);
						return;
					}
					return;
				}
				if (from.group.Permission >= LevelPermission.Operator)
				{
					if (p.group.Permission < from.group.Permission) { Player.SendMessage(p, message); }
				}
				if (from != null)
				{
					if (from == p)
					{
						Player.SendMessage(from, message);
						return;
					}
				}
			});
		}

		public static bool MessageHasBadColorCodes(Player from, string message)
		{
			if (HasBadColorCodes(message))
			{
				SendMessage(from, "Message not sent. You have bad color codes.");
				return true;
			}
			return false;
		}

		public static bool HasBadColorCodes(string message)
		{
			string[] sections = message.Split(new[] { '&', '%' });
			for (int i = 0; i < sections.Length; i++)
			{
				if (String.IsNullOrEmpty(sections[i].Trim()) && i == 0) { continue; }
				if (String.IsNullOrEmpty(sections[i].Trim()) && i - 1 != sections.Length) { continue; }
				if (String.IsNullOrEmpty(sections[i]) && i - 1 != sections.Length) { return true; }

				if (!IsValidColorChar(sections[i][0])) { sections[i] = 'a' + sections[i].Substring(1); }
			}
			return false;
		}

		public static bool IsValidColorChar(char color) { return (color >= '0' && color <= '9') || (color >= 'a' && color <= 'f') || (color >= 'A' && color <= 'F'); }

		public static bool HasBadColorCodesTwo(string message)
		{
			string[] split = message.Split('&');
			for (int i = 0; i < split.Length; i++)
			{
				string section = split[i];

				if (String.IsNullOrEmpty(section.Trim()))
					return true;

				if (!IsValidColorChar(section[0]))
					return true;

			}

			return false;
		}

		public static bool CommandHasBadColourCodes(Player who, string message)
		{
			string[] checkmessagesplit = message.Split(' ');
			bool lastendwithcolour = false;
			foreach (string s in checkmessagesplit)
			{
				s.Trim();
				if (s.StartsWith("%"))
				{
					if (lastendwithcolour)
					{
						if (who != null)
						{
							who.SendMessage("Sorry, Your colour codes in this command were invalid (You cannot use 2 colour codes next to each other");
							who.SendMessage("Command failed.");
							Server.s.Log(who.name + " attempted to send a command with invalid colours codes (2 colour codes were next to each other)!");
							GlobalMessageOps(who.color + who.name + " " + Server.DefaultColor + " attempted to send a command with invalid colours codes (2 colour codes were next to each other)!");
						}
						return true;
					}
					else if (s.Length == 2) { lastendwithcolour = true; }
				}
				if (s.TrimEnd(Server.ColourCodesNoPercent).EndsWith("%")) { lastendwithcolour = true; }
				else { lastendwithcolour = false; }
			}
			return false;
		}

		public static string EscapeColours(string message)
		{
			try
			{
				int index = 1;
				StringBuilder sb = new StringBuilder();
				Regex r = new Regex("^[0-9a-f]$");
				foreach (char c in message)
				{
					if (c == '%')
					{
						if (message.Length >= index)
							sb.Append(r.IsMatch(message[index].ToString()) ? '&' : '%');
						else
							sb.Append('%');
					}
					else
						sb.Append(c);
					index++;
				}
				return sb.ToString();
			}
			catch { return message; }
		}

		public static void GlobalMessage(string message) { GlobalMessage(MessageType.Chat, message); }

		public static void GlobalMessage(MessageType type, string message)
		{
			players.ForEach(delegate(Player p)
			{
				Player.SendMessage(p, message);
			});
		}

		public static void GlobalMessageOps(string message)
		{
			try
			{
				players.ForEach(delegate(Player p)
				{
					if (p.group.Permission >= LevelPermission.Operator || p.isStaff)
					{
						Player.SendMessage(p, message);
					}
				});

			}
			catch { Server.s.Log("Error occured with Op Chat"); }
		}

		public static void GlobalSpawn(Player from, ushort x, ushort y, ushort z, byte rotx, byte roty, bool self, string possession = "")
		{
			players.ForEach(delegate(Player p)
			{
				if (p.Loading && p != from) { return; }
				if (p.level != from.level || (from.hidden && !self)) { return; }
				if (p != from)
				{
					p.SendSpawn(from.id, from.color + from.name + possession, x, y, z, rotx, roty, from.DisplayName, from.SkinName);
				}
				else if (self)
				{
					if (!p.ignorePermission)
					{
						p.pos = new ushort[3] { x, y, z }; p.rot = new byte[2] { rotx, roty };
						p.oldpos = p.pos; p.oldrot = p.rot;
						unchecked { p.SendSpawn((byte)-1, from.color + from.name + possession, x, y, z, rotx, roty, from.DisplayName, from.SkinName); }
					}
				}
			});
		}
		
		public static void GlobalDie(Player from, bool self)
		{
			players.ForEach(delegate(Player p)
			{
				if (p.level != from.level || (from.hidden && !self)) { return; }
				if (p != from) { p.SendDie(from.id); }
				else if (self) { p.SendDie(255); }
			});
		}

		public bool MarkPossessed(string marker = "")
		{
			if (marker != "")
			{
				Player controller = Player.Find(marker);
				if (controller == null) { return false; }
				marker = " (" + controller.color + controller.name + color + ")";
			}
			GlobalDie(this, true);
			GlobalSpawn(this, pos[0], pos[1], pos[2], rot[0], rot[1], true, marker);
			return true;
		}

		public static void GlobalUpdate() { players.ForEach(delegate(Player p) { if (!p.hidden) { p.UpdatePosition(); } }); }
		#endregion
		#region == DISCONNECTING ==
		public void Disconnect() { leftGame(); }
		public void Kick(string kickString) { leftGame(kickString); }

		internal void CloseSocket()
		{
			// Try to close the socket.
			// Sometimes its already closed so these lines will cause an error
			// We just trap them and hide them from view :P
			try
			{
				// Close the damn socket connection!
				socket.Shutdown(SocketShutdown.Both);
#if DEBUG
				Server.s.Log("Socket was shutdown for " + this.name ?? this.ip);
#endif
			}
			catch (Exception e)
			{
#if DEBUG
				Exception ex = new Exception("Failed to shutdown socket for " + this.name ?? this.ip, e);
				Server.ErrorLog(ex);
#endif
			}

			try
			{
				socket.Close();
#if DEBUG
				Server.s.Log("Socket was closed for " + this.name ?? this.ip);
#endif
			}
			catch (Exception e)
			{
#if DEBUG
				Exception ex = new Exception("Failed to close socket for " + this.name ?? this.ip, e);
				Server.ErrorLog(ex);
#endif
			}
		}

		public void leftGame(string kickString = "", bool skip = false)
		{
			if (name == "")
			{
				if (socket != null)
					CloseSocket();
				if (connections.Contains(this))
					connections.Remove(this);
				disconnected = true;
				return;
			}
			try
			{
				if (disconnected)
				{
					this.CloseSocket();
					if (connections.Contains(this))
						connections.Remove(this);
					return;
				}
				disconnected = true;
				pingTimer.Stop();
				pingTimer.Dispose();

				muteTimer.Stop();
				muteTimer.Dispose();
				timespent.Stop();
				timespent.Dispose();

				if (kickString == "") kickString = "Disconnected.";

				SendKick(kickString);

				if (loggedIn)
				{
					GlobalDie(this, false);
					if (kickString == "Disconnected." || kickString.IndexOf("Server shutdown") != -1 || kickString == "Server shutdown. Rejoin in 10 seconds.")
					{
						if (!hidden)
						{
							string leavem = "&c- " + color + prefix + name + Server.DefaultColor + " Disconnected";
							Player.players.ForEach(delegate(Player p1)
							{
								Player.SendMessage(p1, leavem);
							});
						}
						Server.IRC.Say(name + " left the game.");
						Server.s.Log(name + " disconnected.");
					}
					else
					{
						totalKicked++;
						GlobalChat(this, "&c- " + color + prefix + name + Server.DefaultColor + " kicked (" + kickString + Server.DefaultColor + ").", false);
						Server.IRC.Say(name + " kicked (" + kickString + ").");
						Server.s.Log(name + " kicked (" + kickString + ").");
					}

					try { save(); }
					catch (Exception e) { Server.ErrorLog(e); }
					players.Remove(this);
					players.ForEach(delegate(Player p)
					{
						if (p != this && p.extension)
						{
							//TODO: Remove this.
						}
					});
					Server.s.PlayerListUpdate();
					try { left.Add(this.name.ToLower(), this.ip); }
					catch (Exception) { }

					if (PlayerDisconnect != null)
						PlayerDisconnect(this, kickString);

					this.Dispose();
				}
				else
				{
					connections.Remove(this);

					Server.s.Log(ip + " disconnected.");
				}

			}
			catch (Exception e) { Server.ErrorLog(e); }
			finally
			{
				CloseSocket();
			}
		}

		public void Dispose()
		{
			if (connections.Contains(this)) connections.Remove(this);
			spamBlockLog.Clear();

		}

		public bool IsAloneOnCurrentLevel() { return players.All(pl => pl.level != level || pl == this); }

		#endregion
		#region == CHECKING ==
		public static List<Player> GetPlayers() { return new List<Player>(players); }
		public static bool Exists(string name) { foreach (Player p in players) { if (p.name.ToLower() == name.ToLower()) { return true; } } return false; }
		public static bool Exists(byte id) { foreach (Player p in players) { if (p.id == id) { return true; } } return false; }
		public static Player Find(string name)
		{
			List<Player> tempList = new List<Player>();
			tempList.AddRange(players);
			Player tempPlayer = null; bool returnNull = false;

			foreach (Player p in tempList)
			{
				if (p.name.ToLower() == name.ToLower()) return p;
				if (p.name.ToLower().IndexOf(name.ToLower()) != -1)
				{
					if (tempPlayer == null) tempPlayer = p;
					else returnNull = true;
				}
			}

			if (returnNull == true) return null;
			if (tempPlayer != null) return tempPlayer;
			return null;
		}
		public static Group GetGroup(string name) { return Group.findPlayerGroup(name); }
		public static string GetColor(string name) { return GetGroup(name).color; }
		#endregion
		#region == OTHER ==
		static byte FreeId()
		{
			for (ushort i = 0; i < Block.maxblocks; i++)
			{
				bool used = players.Any(p => p.id == i);

				if (!used)
					return (byte)i;
			}
			return (byte)1;
		}
		public static byte[] StringFormat(string str, int size)
		{
			byte[] bytes = new byte[size];
			bytes = enc.GetBytes(str.PadRight(size).Substring(0, size));
			return bytes;
		}

		// TODO: Optimize this using a StringBuilder
		static List<string> Wordwrap(string message)
		{
			List<string> lines = new List<string>();
			message = Regex.Replace(message, @"(&[0-9a-f])+(&[0-9a-f])", "$2");
			message = Regex.Replace(message, @"(&[0-9a-f])+$", "");

			int limit = 64; string color = "";
			while (message.Length > 0)
			{
				if (lines.Count > 0)
				{
					if (message[0].ToString() == "&")
						message = "> " + message.Trim();
					else
						message = "> " + color + message.Trim();
				}

				if (message.IndexOf("&") == message.IndexOf("&", message.IndexOf("&") + 1) - 2)
					message = message.Remove(message.IndexOf("&"), 2);

				if (message.Length <= limit) { lines.Add(message); break; }
				for (int i = limit - 1; i > limit - 20; --i)
					if (message[i] == ' ')
					{
						lines.Add(message.Substring(0, i));
						goto Next;
					}

			retry:
				if (message.Length == 0 || limit == 0) { return lines; }

				try
				{
					if (message.Substring(limit - 2, 1) == "&" || message.Substring(limit - 1, 1) == "&")
					{
						message = message.Remove(limit - 2, 1);
						limit -= 2;
						goto retry;
					}
					else if (message[limit - 1] < 32 || message[limit - 1] > 127)
					{
						message = message.Remove(limit - 1, 1);
						limit -= 1;
					}
				}
				catch { return lines; }
				lines.Add(message.Substring(0, limit));

			Next: message = message.Substring(lines[lines.Count - 1].Length);
				if (lines.Count == 1) limit = 60;

				int index = lines[lines.Count - 1].LastIndexOf('&');
				if (index != -1)
				{
					if (index < lines[lines.Count - 1].Length - 1)
					{
						char next = lines[lines.Count - 1][index + 1];
						if ("0123456789abcdef".IndexOf(next) != -1) { color = "&" + next; }
						if (index == lines[lines.Count - 1].Length - 1)
						{
							lines[lines.Count - 1] = lines[lines.Count - 1].Substring(0, lines[lines.Count - 1].Length - 2);
						}
					}
					else if (message.Length != 0)
					{
						char next = message[0];
						if ("0123456789abcdef".IndexOf(next) != -1) { color = "&" + next; }
						lines[lines.Count - 1] = lines[lines.Count - 1].Substring(0, lines[lines.Count - 1].Length - 1);
						message = message.Substring(1);
					}
				}
			}
			char[] temp;
			for (int i = 0; i < lines.Count; i++)
			{
				temp = lines[i].ToCharArray();
				if (temp[temp.Length - 2] == '%' || temp[temp.Length - 2] == '&')
				{
					temp[temp.Length - 1] = ' ';
					temp[temp.Length - 2] = ' ';
				}
				StringBuilder message1 = new StringBuilder();
				message1.Append(temp);
				lines[i] = message1.ToString();
			}
			return lines;
		}
		public static bool ValidName(string name, Player p = null)
		{
			string allowedchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890._@+";
			return name.All(ch => allowedchars.IndexOf(ch) != -1);
		}

		public static int GetBannedCount()
		{
			try { return File.ReadAllLines("ranks/banned.txt").Length; }
			catch { return 0; }
		}

		#endregion
		#region == Host <> Network ==
		public static byte[] HTNO(ushort x) { byte[] y = BitConverter.GetBytes(x); Array.Reverse(y); return y; }
		public static ushort NTHO(byte[] x, int offset)
		{
			byte[] y = new byte[2];
			Buffer.BlockCopy(x, offset, y, 0, 2); Array.Reverse(y);
			return BitConverter.ToUInt16(y, 0);
		}
		public static byte[] HTNO(short x) { byte[] y = BitConverter.GetBytes(x); Array.Reverse(y); return y; }
		#endregion

		bool CheckBlockSpam()
		{
			if (spamBlockLog.Count >= spamBlockCount)
			{
				DateTime oldestTime = spamBlockLog.Dequeue();
				double spamTimer = DateTime.Now.Subtract(oldestTime).TotalSeconds;
				if (spamTimer < spamBlockTimer)
				{
					this.Kick("You were kicked by antigrief system. Slow down.");
					SendMessage(c.red + name + " was kicked for suspected griefing.");
					Server.s.Log(name + " was kicked for block spam (" + spamBlockCount + " blocks in " + spamTimer + " seconds)");
					return true;
				}
			}
			spamBlockLog.Enqueue(DateTime.Now);
			return false;
		}

		#region getters
		public ushort[] footLocation { get { return getLoc(false); } }
		public ushort[] headLocation { get { return getLoc(true); } }

		public ushort[] getLoc(bool head)
		{
			ushort[] myPos = pos;
			myPos[0] /= 32;
			if (head) myPos[1] = (ushort)((myPos[1] + 4) / 32);
			else myPos[1] = (ushort)((myPos[1] + 4) / 32 - 1);
			myPos[2] /= 32;
			return myPos;
		}

		public void setLoc(ushort[] myPos)
		{
			myPos[0] *= 32;
			myPos[1] *= 32;
			myPos[2] *= 32;
			unchecked { SendPos((byte)-1, myPos[0], myPos[1], myPos[2], rot[0], rot[1]); }
		}

		#endregion

		public static bool IPInPrivateRange(string ip)
		{
			if (ip.StartsWith("172.") && (int.Parse(ip.Split('.')[1]) >= 16 && int.Parse(ip.Split('.')[1]) <= 31))
				return true;
			return IPAddress.IsLoopback(IPAddress.Parse(ip)) || ip.StartsWith("192.168.") || ip.StartsWith("10.");
		}

		public static bool IsLocalIpAddress(string host)
		{
			try
			{
				IPAddress[] hostIPs = Dns.GetHostAddresses(host);
				IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

				foreach (IPAddress hostIP in hostIPs)
				{
					if (IPAddress.IsLoopback(hostIP)) return true;
					foreach (IPAddress localIP in localIPs) { if (hostIP.Equals(localIP)) return true; }
				}
			}
			catch { }
			return false;
		}

		public bool EnoughMoney(int amount)
		{
			if (this.money >= amount)
				return true;
			return false;
		}

		public string ReadString(int count = 64)
		{
			if (Reader == null) return null;
			var chars = new byte[count];
			Reader.Read(chars, 0, count);
			return Encoding.UTF8.GetString(chars).TrimEnd().Replace("\0", string.Empty);
		}
	}
}

