/*
	Copyright (C) 2010-2013 David Mitchell (Modified for use with MCSpleef)

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

//WARNING! DO NOT CHANGE THE WAY THE LEVEL IS SAVED/LOADED!
//You MUST make it able to save and load as a new version other wise you will make old levels incompatible!

namespace MCSpleef
{
	public enum LevelPermission //int is default
	{
		Banned = -20,
		Guest = 0,
		Operator = 80,
		Admin = 100,
		Nobody = 120,
		Null = 150
	}

	public sealed class Level : IDisposable
	{
		#region Delegates

		public delegate void OnLevelLoad(string level);
		public delegate void OnLevelLoaded(Level l);
		public delegate void OnLevelSave(Level l);
		public delegate void OnLevelUnload(Level l);

		#endregion
		public bool Death;

		public static bool cancelload;
		public static bool cancelsave;
		public readonly List<Check> ListCheck = new List<Check>(); //A list of blocks that need to be updated
		public readonly List<Update> ListUpdate = new List<Update>(); //A list of block to change after calculation

		public bool backedup;
		public List<Blockchange> blockCache = new List<Blockchange>();
		public ushort[] blocks;
		public bool cancelsave1;
		public bool changed;

		public ushort depth; // y	   THIS IS STUPID, SHOULD HAVE BEEN Z
		public ushort height; // z	  THIS IS STUPID, SHOULD HAVE BEEN Y
		public int id;
		public int lastCheck;
		public int lastUpdate;
		public string motd = "ignore";
		public string name;

		public LevelPermission permissionbuild = LevelPermission.Guest;
		public LevelPermission permissionvisit = LevelPermission.Guest;

		public bool realistic = true;
		public byte rotx;
		public byte roty;
		public ushort spawnx;
		public ushort spawny;
		public ushort spawnz;
		public ushort[] backupBlocks;

		public ushort width; // x
		public List<BlockQueue.block> blockqueue = new List<BlockQueue.block>();

		public Level(string n, ushort x, ushort y, ushort z, string type, int seed = 0, bool useSeed = false)
		{
			width = x;
			depth = y;
			height = z;
			if (width < 16)  width = 16;
			if (depth < 16)  depth = 16;
			if (height < 16) height = 16;

			name = n;
			blocks = new ushort[width * depth * height];

			var half = (ushort)(depth / 2);
			switch (type)
			{
				case "flat":
					for (x = 0; x < width; ++x)
						for (z = 0; z < height; ++z)
							for (y = 0; y <= half; ++y)
								SetTile(x, y, z, y < half ? Block.dirt : Block.grass);
					//SetTile(x, y, z, (byte)(y != half ? (y >= half) ? 0 : 3 : 2));
					break;
			}
			spawnx = (ushort)(width / 2);
			spawny = (ushort)(depth * 0.75f);
			spawnz = (ushort)(height / 2);
			rotx = 0;
			roty = 0;
		}

		public ushort length { get { return height; } }
		public List<Player> players { get { return getPlayers(); } }

		public static event OnLevelUnload LevelUnload = null;
		public static event OnLevelSave LevelSave = null;
		public static event OnLevelLoad LevelLoad = null;
		public static event OnLevelLoaded LevelLoaded;

		public void CopyBlocks(byte[] source, int offset)
		{
			blocks = new ushort[width * depth * height];
			Array.Copy(source, offset, blocks, 0, blocks.Length);

			for (int i = 0; i < blocks.Length; i++)
			{
				if (blocks[i] >= 50) blocks[i] = 0;
				switch (blocks[i])
				{
					case Block.waterstill:
						blocks[i] = Block.water;
						break;
					case Block.water:
						blocks[i] = Block.waterstill;
						break;
					case Block.lava:
						blocks[i] = Block.lavastill;
						break;
					case Block.lavastill:
						blocks[i] = Block.lava;
						break;
				}
			}
		}

		public bool Unload(bool silent = false, bool save = true)
		{
			if (Server.mainLevel == this) return false;
			if (LevelUnload != null)
				LevelUnload(this);

			Player.players.ForEach(
				delegate(Player pl) { if (pl.level == this) Command.all.Find("goto").Use(pl, Server.mainLevel.name); });

			if (changed)
			{
				Save(false, true);
				saveChanges();
			}

			Server.levels.Remove(this);
			{
				Dispose();
				GC.Collect();
				GC.WaitForPendingFinalizers();

				if (!silent) Player.GlobalMessageOps("&3" + name + Server.DefaultColor + " was unloaded.");
				Server.s.Log(string.Format("{0} was unloaded.", name));
			}
			return true;
		}

		public void Dispose()
		{
			ListCheck.Clear();
			ListUpdate.Clear();
			blockCache.Clear();
			blockqueue.Clear();
			blocks = null;
		}

		public void saveChanges()
		{
			if (blockCache.Count == 0) return;
			List<Blockchange> tempCache = blockCache;
			blockCache = new List<Blockchange>();
			tempCache.ForEach( delegate( Blockchange bP ) {
				BlocksDB.blockchanges.Add( bP );
				BlocksDB.Save();
			} );
			tempCache.Clear();
		}

		public ushort GetTile(ushort x, ushort y, ushort z)
		{
			if (blocks == null) return Block.Zero;
			return !InBound(x, y, z) ? Block.Zero : blocks[PosToInt(x, y, z)];
		}
		public ushort GetTile(int b)
		{
			ushort x = 0, y = 0, z = 0;
			IntToPos(b, out x, out y, out z);
			return GetTile(x, y, z);
		}
		public void SetTile(int b, ushort type)
		{
			if (blocks == null) return;
			if (b >= blocks.Length) return;
			if (b < 0) return;
			blocks[b] = (ushort)type;
		}
		public void SetTile(ushort x, ushort y, ushort z, ushort type)
		{
			if (blocks == null) return;
			if (!InBound(x, y, z)) return;
			blocks[PosToInt(x, y, z)] = (ushort)type;
		}

		public bool InBound(ushort x, ushort y, ushort z) { return x >= 0 && y >= 0 && z >= 0 && x < width && y < depth && z < height; }

		public static Level Find(string levelName)
		{
			Level tempLevel = null;
			bool returnNull = false;

			foreach (Level level in Server.levels)
			{
				if (level.name.ToLower() == levelName) return level;
				if (level.name.ToLower().IndexOf(levelName.ToLower(), System.StringComparison.Ordinal) == -1) continue;
				if (tempLevel == null) tempLevel = level;
				else returnNull = true;
			}

			return returnNull ? null : tempLevel;
		}

		public static Level FindExact(string levelName) { return Server.levels.Find(lvl => levelName.ToLower() == lvl.name.ToLower()); }

		public void Blockchange(Player p, ushort x, ushort y, ushort z, ushort type, bool addaction = true)
		{
			string errorLocation = "start";
		retry:
			try
			{
				if (x < 0 || y < 0 || z < 0) return;
				if (x >= width || y >= depth || z >= height) return;

				ushort b = GetTile(x, y, z);

				errorLocation = "Block rank checking";
				if (!Block.AllowBreak(b)) {
					if (!Block.canPlace(p, b) && !Block.BuildIn(b)) { p.SendBlockchange(x, y, z, b); return; }
				}


				errorLocation = "Map rank checking";
				if (p.group.Permission < permissionbuild)
				{
					p.SendBlockchange(x, y, z, b);
					Player.SendMessage(p, "Must be at least " + PermissionToName(permissionbuild) + " to build here");
					return;
				}

				errorLocation = "Block sending";
				if (Block.Convert(b) != Block.Convert(type))
					Player.GlobalBlockchange(this, x, y, z, type);


				errorLocation = "Setting tile";
				p.loginBlocks++;
				p.overallBlocks++;
				SetTile(x, y, z, (ushort)type); //Updates server level blocks

				errorLocation = "Growing grass";
				if (GetTile(x, (ushort)(y - 1), z) == Block.grass && !Block.LightPass(type))
					Blockchange(p, x, (ushort)(y - 1), z, Block.dirt);

				changed = true;
				backedup = false;
			}
			catch (OutOfMemoryException)
			{
				Player.SendMessage(p, "You clearly have a potato as a PC.");
				goto retry;
			}
			catch (Exception e)
			{
				Server.ErrorLog(e);
				Player.GlobalMessageOps(p.name + " triggered a non-fatal error on " + name);
				Player.GlobalMessageOps("Error location: " + errorLocation);
				Server.s.Log(p.name + " triggered a non-fatal error on " + name);
				Server.s.Log("Error location: " + errorLocation);
			}
		}

		public void Blockchange(int b, ushort type, bool overRide = false, string extraInfo = "")
		{
			if (b < 0) return;
			if (b >= blocks.Length) return;
			ushort bb = GetTile(b);

			try
			{
				if (!overRide) { if (Block.OPBlocks(bb) || (Block.OPBlocks(type) && extraInfo != "")) return; }
				if (Block.Convert(bb) != Block.Convert(type)) { Player.GlobalBlockchange(this, b, type); }
				SetTile(b, type);
			}
			catch { SetTile(b, type); }
		}
		public void Blockchange(ushort x, ushort y, ushort z, ushort type, bool overRide = false, string extraInfo = "")
		{
			if (x < 0 || y < 0 || z < 0) return;
			if (x >= width || y >= depth || z >= height) return;
			ushort b = GetTile(x, y, z);

			try
			{
				if (!overRide) { if (Block.OPBlocks(b) || (Block.OPBlocks(type) && extraInfo != "")) return; }
				if (Block.Convert(b) != Block.Convert(type)) { Player.GlobalBlockchange(this, x, y, z, type); }
				SetTile(x, y, z, type);
			}
			catch { SetTile(x, y, z, type); }
		}

		public bool CheckClear(ushort x, ushort y, ushort z)
		{
			int b = PosToInt(x, y, z);
			return !ListCheck.Exists(Check => Check.b == b);
		}

		public void skipChange(ushort x, ushort y, ushort z, ushort type)
		{
			if (x < 0 || y < 0 || z < 0) return;
			if (x >= width || y >= depth || z >= height) return;

			SetTile(x, y, z, type);
		}

		public void Save(bool Override = false, bool clearPhysics = false)
		{
			if (blocks == null) return;
			string path = "levels/" + name + ".mcf";
			if (LevelSave != null)
				LevelSave(this);
			if (cancelsave1) { cancelsave1 = false; return; }
			if (cancelsave) { cancelsave = false; return; }
			try
			{
				if (!Directory.Exists("levels")) Directory.CreateDirectory("levels");

				if (changed || !File.Exists(path) || Override)
				{
					string backFile = string.Format("{0}.back", path);
					string backupFile = string.Format("{0}.backup", path);
					
					using (FileStream fs = File.OpenWrite(backFile))
					{
						using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress))
						{
							var header = new byte[16];
							BitConverter.GetBytes(1874).CopyTo(header, 0);
							gs.Write(header, 0, 2);

							BitConverter.GetBytes(width).CopyTo(header, 0);
							BitConverter.GetBytes(height).CopyTo(header, 2);
							BitConverter.GetBytes(depth).CopyTo(header, 4);
							changed = false;
							BitConverter.GetBytes(spawnx).CopyTo(header, 6);
							BitConverter.GetBytes(spawnz).CopyTo(header, 8);
							BitConverter.GetBytes(spawny).CopyTo(header, 10);
							header[12] = rotx;
							header[13] = roty;
							header[14] = (byte)permissionvisit;
							header[15] = (byte)permissionbuild;
							gs.Write(header, 0, header.Length);
							var level = new byte[blocks.Length * 2];
							for (int i = 0; i < blocks.Length; ++i)
							{
								ushort blockVal = 0;
								if (blocks[i] < 57) { if (blocks[i] != Block.air) { blockVal = (ushort)blocks[i]; } }

								level[i*2] = (byte)blockVal;
								level[i*2 + 1] = (byte)(blockVal >> 8);
							}
							gs.Write(level, 0, level.Length);
						}
					}

					// Safely replace the original file (if it exists) after making a backup.
					if (File.Exists(path))
					{
						File.Delete(backupFile);
						File.Replace(backFile, path, backupFile);
					}
					else { File.Move(backFile, path); }

					Server.s.Log(string.Format("SAVED: Level \"{0}\". ({1}/{2}/{3})", name, players.Count,
											   Player.players.Count, Server.players));

					// UNCOMPRESSED LEVEL SAVING! DO NOT USE!
					/*using (FileStream fs = File.Create(path + ".wtf"))
					{
						byte[] header = new byte[16];
						BitConverter.GetBytes(1874).CopyTo(header, 0);
						fs.Write(header, 0, 2);

						BitConverter.GetBytes(width).CopyTo(header, 0);
						BitConverter.GetBytes(height).CopyTo(header, 2);
						BitConverter.GetBytes(depth).CopyTo(header, 4);
						BitConverter.GetBytes(spawnx).CopyTo(header, 6);
						BitConverter.GetBytes(spawnz).CopyTo(header, 8);
						BitConverter.GetBytes(spawny).CopyTo(header, 10);
						header[12] = rotx; header[13] = roty;
						header[14] = (byte)permissionvisit;
						header[15] = (byte)permissionbuild;
						fs.Write(header, 0, header.Length);
						byte[] level = new byte[blocks.Length];
						for (int i = 0; i < blocks.Length; ++i)
						{
							if (blocks[i] < 80) { level[i] = blocks[i]; }
							else { level[i] = Block.SaveConvert(blocks[i]); }
						} fs.Write(level, 0, level.Length); fs.Close();
					}*/
				}
				else { Server.s.Log("Skipping level save for " + name + "."); }
			}
			catch (OutOfMemoryException e)
			{
				Server.ErrorLog(e);
				Command.all.Find("restart").Use(null, "");
			}
			catch (Exception e)
			{
				Server.s.Log("FAILED TO SAVE :" + name);
				Player.GlobalMessage("FAILED TO SAVE :" + name);

				Server.ErrorLog(e);
				return;
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public int Backup(bool Forced = false, string backupName = "")
		{
			if (!backedup || Forced)
			{
				int backupNumber = 1;
				string backupPath = @Server.backupLocation;
				if (Directory.Exists(string.Format("{0}/{1}", backupPath, name))) { backupNumber = Directory.GetDirectories(string.Format("{0}/" + name, backupPath)).Length + 1; }
				else { Directory.CreateDirectory(backupPath + "/" + name); }

				string path = string.Format("{0}/" + name + "/" + backupNumber, backupPath);
				if (backupName != "") { path = string.Format("{0}/" + name + "/" + backupName, backupPath); }
				Directory.CreateDirectory(path);

				string BackPath = string.Format("{0}/{1}.mcf", path, name);
				string current = string.Format("levels/{0}.mcf", name);
				try
				{
					File.Copy(current, BackPath, true);
					backedup = true;
					return backupNumber;
				}
				catch (Exception e)
				{
					Server.ErrorLog(e);
					Server.s.Log(string.Format("FAILED TO INCREMENTAL BACKUP :{0}", name));
					return -1;
				}
			}
			Server.s.Log("Level unchanged, skipping backup");
			return -1;
		}

		//TODO: Remove this.
		public static void CreateLeveldb(string givenName) {

		}

		public static Level Load(string givenName) { return Load(givenName, 0); }

		public static Level Load(string givenName, byte phys, bool bite = false) 
		{
			GC.Collect();
			if (LevelLoad != null)
				LevelLoad(givenName);
			if (cancelload)
			{
				cancelload = false;
				return null;
			}
			CreateLeveldb(givenName);

			string path = string.Format("levels/{0}.{1}", bite ? "byte/" + givenName : givenName, bite ? "lvl" : "mcf"); //OMG RLLY? .MCF??
			if (File.Exists(path))
			{
				FileStream fs = File.OpenRead(path);
				try
				{
					var gs = new GZipStream(fs, CompressionMode.Decompress);
					var ver = new byte[2];
					gs.Read(ver, 0, ver.Length);
					ushort version = BitConverter.ToUInt16(ver, 0);
					var vars = new ushort[6];
					var rot = new byte[2];

					if (version == 1874)
					{
						var header = new byte[16];
						gs.Read(header, 0, header.Length);

						vars[0] = BitConverter.ToUInt16(header, 0);
						vars[1] = BitConverter.ToUInt16(header, 2);
						vars[2] = BitConverter.ToUInt16(header, 4);
						vars[3] = BitConverter.ToUInt16(header, 6);
						vars[4] = BitConverter.ToUInt16(header, 8);
						vars[5] = BitConverter.ToUInt16(header, 10);

						rot[0] = header[12];
						rot[1] = header[13];
					}
					else
					{
						var header = new byte[12];
						gs.Read(header, 0, header.Length);

						vars[0] = version;
						vars[1] = BitConverter.ToUInt16(header, 0);
						vars[2] = BitConverter.ToUInt16(header, 2);
						vars[3] = BitConverter.ToUInt16(header, 4);
						vars[4] = BitConverter.ToUInt16(header, 6);
						vars[5] = BitConverter.ToUInt16(header, 8);

						rot[0] = header[10];
						rot[1] = header[11];
					}

					var level = new Level(givenName, vars[0], vars[2], vars[1], "empty")
									{
										permissionbuild = (LevelPermission)30,
										spawnx = vars[3],
										spawnz = vars[4],
										spawny = vars[5],
										rotx = rot[0],
										roty = rot[1],
										name = givenName
									};


					var blocks = new byte[(bite ? 1 : 2) * level.width * level.height * level.depth];
					gs.Read(blocks, 0, blocks.Length);
					if(!bite)
						for (int i = 0; i < (blocks.Length / 2); ++i)
						{
							level.blocks[i] = (ushort)(blocks[i * 2] + (blocks[(i * 2) + 1] << 8));
							level.blocks[i] = BitConverter.ToUInt16(new byte[] { blocks[i * 2], blocks[(i * 2) + 1] }, 0);
						}
					else
						for (int i = 0; i < blocks.Length; ++i)
							level.blocks[i] = (ushort)blocks[i];
					gs.Close();
					gs.Dispose();
					level.backedup = true;

					Server.s.Log(string.Format("Level \"{0}\" loaded.", level.name));
					if (LevelLoaded != null)
						LevelLoaded(level);
					GC.Collect();
					GC.WaitForPendingFinalizers();
					return level;
				}
				catch (Exception ex)
				{
					Server.ErrorLog(ex);
					return null;
				}
				finally
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
					fs.Close();
					fs.Dispose();
				}
			}
			Server.s.Log("ERROR loading level.");
			GC.Collect();
			GC.WaitForPendingFinalizers();
			return null;
		}

		public int PosToInt(ushort x, ushort y, ushort z)
		{
			if (x < 0 || x >= width || y < 0 || y >= depth || z < 0 || z >= height)
				return -1;
			return x + (z * width) + (y * width * height);
		}

		public void IntToPos(int pos, out ushort x, out ushort y, out ushort z)
		{
			y = (ushort)(pos / width / height);
			pos -= y * width * height;
			z = (ushort)(pos / width);
			pos -= z * width;
			x = (ushort)pos;
		}

		public int IntOffset(int pos, int x, int y, int z) { return pos + x + z * width + y * width * height; }

		public static LevelPermission PermissionFromName(string name)
		{
			Group foundGroup = Group.Find(name);
			return foundGroup != null ? foundGroup.Permission : LevelPermission.Null;
		}

		public static string PermissionToName(LevelPermission perm)
		{
			Group foundGroup = Group.findPerm(perm);
			return foundGroup != null ? foundGroup.name : ((int)perm).ToString();
		}

		public List<Player> getPlayers() { return Player.players.Where(p => p.level == this).ToList(); }

		#region ==Physics==

		public string foundInfo(ushort x, ushort y, ushort z)
		{
			Check foundCheck = null;
			try { foundCheck = ListCheck.Find(Check => Check.b == PosToInt(x, y, z)); }
			catch { }
			if (foundCheck != null)
				return foundCheck.extraInfo;
			return "";
		}

		public void AddCheck(int b, string extraInfo = "", bool overRide = false, MCSpleef.Player Placer = null)
		{
			try
			{
				if (!ListCheck.Exists(Check => Check.b == b)) { ListCheck.Add(new Check(b, extraInfo, Placer)); }
				else
				{
					if (overRide)
					{
						foreach (Check C2 in ListCheck) { if (C2.b == b) { C2.extraInfo = extraInfo; return; } }
					}
				}
			}
			catch { }
		}

		public bool AddUpdate(int b, ushort type, bool overRide = false, string extraInfo = "")
		{
			try
			{
				if (overRide)
				{
					ushort x, y, z;
					IntToPos(b, out x, out y, out z);
					AddCheck(b, extraInfo, true);
					Blockchange(x, y, z, (ushort)type, true, extraInfo);
					return true;
				}
				return false;
			}
			catch { return false; }
		}

		public void placeBlock(ushort x, ushort y, ushort z, ushort b)
		{
			AddUpdate(PosToInt((ushort)x, (ushort)y, (ushort)z), b, true);
			AddCheck(PosToInt((ushort)x, (ushort)y, (ushort)z));
		}

		public struct Pos { public ushort x, z; }

		#endregion

		#region Nested type: BlockPos

		public struct BlockPos
		{
			public DateTime TimePerformed;
			public bool deleted;
			public string name;
			public ushort type;
			public ushort x, y, z;
		}

		#endregion
	}
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------
public class Check
{
	public int b;
	public string extraInfo = "";
	public byte time;
	public MCSpleef.Player p;

	public Check(int b, string extraInfo = "", MCSpleef.Player placer = null)
	{
		this.b = b;
		time = 0;
		this.extraInfo = extraInfo;
		p = placer;
	}
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------
public class Update
{
	public int b;
	public string extraInfo = "";
	public ushort type;

	public Update(int b, ushort type, string extraInfo = "")
	{
		this.b = b;
		this.type = type;
		this.extraInfo = extraInfo;
	}
}
