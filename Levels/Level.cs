/*
Copyright (C) 2010-2013 David Mitchell

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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

using Timer = System.Timers.Timer;
//WARNING! DO NOT CHANGE THE WAY THE LEVEL IS SAVED/LOADED!
//You MUST make it able to save and load as a new version other wise you will make old levels incompatible!

namespace MCForge
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
    public enum PhysicsState
    {
        Stopped,
        Warning,
        Other
    }

    public enum MapType
    {
        General,
        Game,
    }

    public sealed class Level : IDisposable
    {
        #region Delegates

        public delegate void OnLevelLoad(string level);

        public delegate void OnLevelLoaded(Level l);

        public delegate void OnLevelSave(Level l);

        public delegate void OnLevelUnload(Level l);

        #endregion
        public int speedphysics = 250;
        public bool Death;

        public static bool cancelload;
        public static bool cancelsave;
        public readonly List<Check> ListCheck = new List<Check>(); //A list of blocks that need to be updated
        public readonly List<Update> ListUpdate = new List<Update>(); //A list of block to change after calculation

        public bool ai = true;
        public bool backedup;
        public List<Blockchange> blockCache = new List<Blockchange>();
        public ushort[] blocks;
        public bool cancelsave1;
        public bool cancelunload;
        public bool changed;

        public ushort depth; // y       THIS IS STUPID, SHOULD HAVE BEEN Z
        public ushort height; // z      THIS IS STUPID, SHOULD HAVE BEEN Y
        public int id;
        public int lastCheck;
        public int lastUpdate;
        public string motd = "ignore";
        public string name;

        public int maxBuildHeight;
        public ushort divider;

        // IsoCat
        public ushort[,] shadows;
        public void CalculateShadows()
        {
            try
            {
                if (shadows != null) return;

                shadows = new ushort[width, height];
                for (ushort x = 0; x < width; x++)
                {
                    for (ushort y = 0; y < height; y++)
                    {
                        for (ushort z = (ushort)(depth - 1); z >= 0; z--)
                        {
                            switch (GetTile(x, y, z))
                            {
                                case Block.air:
                                case Block.mushroom:
                                case Block.glass:
                                case Block.leaf:
                                case Block.redflower:
                                case Block.redmushroom:
                                case Block.yellowflower:
                                    continue;
                                default:
                                    shadows[x, y] = z;
                                    break;
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) { Server.ErrorLog(ex); shadows = new ushort[width, height]; }
        }
        public LevelPermission permissionbuild = LevelPermission.Guest;
        public LevelPermission permissionvisit = LevelPermission.Guest;


        public bool randomFlow = true;
        public bool realistic = true;
        public byte rotx;
        public byte roty;
        public bool rp = true;
        public ushort spawnx;
        public ushort spawny;
        public ushort spawnz;
        public ushort[] backupBlocks;

        public string theme = "Normal";
        public bool unload = true;
        public ushort width; // x
        public bool worldChat = true;
        public List<BlockQueue.block> blockqueue = new List<BlockQueue.block>();

        public Level(string n, ushort x, ushort y, ushort z, string type, int seed = 0, bool useSeed = false, MapType mt = MapType.General)
        {
            //onLevelSave += null;
            width = x;
            depth = y;
            height = z;
            if (width < 16)
            {
                width = 16;
            }
            if (depth < 16)
            {
                depth = 16;
            }
            if (height < 16)
            {
                height = 16;
            }

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
                //no need for default
            }
            spawnx = (ushort)(width / 2);
            spawny = (ushort)(depth * 0.75f);
            spawnz = (ushort)(height / 2);
            rotx = 0;
            roty = 0;
            //season = new SeasonsCore(this);
        }

        public ushort length
        {
            get { return height; }
        }

        public List<Player> players
        {
            get { return getPlayers(); }
        }

        public static event OnLevelUnload LevelUnload = null;
        public static event OnLevelSave LevelSave = null;
        //public static event OnLevelSave onLevelSave = null;
  //      public event OnLevelUnload onLevelUnload = null;
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
            if (name.Contains("&cMuseum ")) return false;
            if (LevelUnload != null)
                LevelUnload(this);
            //OnLevelUnloadEvent.Call(this);
            if (cancelunload)
            {
                Server.s.Log("Unload canceled by Plugin! (Map: " + name + ")");
                cancelunload = false;
                return false;
            }
            Player.players.ForEach(
                delegate(Player pl) { if (pl.level == this) Command.all.Find("goto").Use(pl, Server.mainLevel.name); });

            if (changed && mapType != MapType.Game)
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
            //if (PosToInt(x, y, z) >= blocks.Length) { return null; }
            //Avoid internal overflow
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
            //blockchanges[x + width * z + width * height * y] = pName;
        }
        public void SetTile(ushort x, ushort y, ushort z, ushort type)
        {
            if (blocks == null) return;
            if (!InBound(x, y, z)) return;
            blocks[PosToInt(x, y, z)] = (ushort)type;
            //blockchanges[x + width * z + width * height * y] = pName;
        }

        public bool InBound(ushort x, ushort y, ushort z)
        {
            return x >= 0 && y >= 0 && z >= 0 && x < width && y < depth && z < height;
        }

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

        public static Level FindExact(string levelName)
        {
            return Server.levels.Find(lvl => levelName.ToLower() == lvl.name.ToLower());
        }


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
                if (!Block.AllowBreak(b))
                {
                    if (!Block.canPlace(p, b) && !Block.BuildIn(b))
                    {
                        p.SendBlockchange(x, y, z, b);
                        return;
                    }
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
                {
                    Blockchange(p, x, (ushort)(y - 1), z, Block.dirt);
                }

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

        public static void SaveSettings(Level level)
        {
            try
            {
                File.Create("levels/level properties/" + level.name + ".properties").Dispose();
                using (StreamWriter SW = File.CreateText("levels/level properties/" + level.name + ".properties"))
                {
                    SW.WriteLine("#Level properties for " + level.name);
                    SW.WriteLine("WorldChat = " + level.worldChat.ToString());
                    SW.WriteLine("PerBuild = " +
                                 (Group.Exists(PermissionToName(level.permissionbuild).ToLower())
                                      ? PermissionToName(level.permissionbuild).ToLower()
                                      : PermissionToName(LevelPermission.Guest)));
                    SW.WriteLine("PerVisit = " +
                                 (Group.Exists(PermissionToName(level.permissionvisit).ToLower())
                                      ? PermissionToName(level.permissionvisit).ToLower()
                                      : PermissionToName(LevelPermission.Guest)));
                    SW.WriteLine("Type = " + level.mapType.ToString());
                }
            }
            catch (Exception)
            {
                Server.s.Log("Failed to save level properties!");
            }
        }
        public void Blockchange(int b, ushort type, bool overRide = false, string extraInfo = "")
        //Block change made by physics
        {
            if (b < 0) return;
            if (b >= blocks.Length) return;
            ushort bb = GetTile(b);

            try
            {
                if (!overRide)
                    if (Block.OPBlocks(bb) || (Block.OPBlocks(type) && extraInfo != "")) return;

                if (Block.Convert(bb) != Block.Convert(type))
                    //Should save bandwidth sending identical looking blocks, like air/op_air changes.
                    Player.GlobalBlockchange(this, b, type);

                SetTile(b, type); //Updates server level blocks
            }
            catch
            {
                SetTile(b, type);
            }
        }
        public void Blockchange(ushort x, ushort y, ushort z, ushort type, bool overRide = false, string extraInfo = "")
        //Block change made by physics
        {
            if (x < 0 || y < 0 || z < 0) return;
            if (x >= width || y >= depth || z >= height) return;
            ushort b = GetTile(x, y, z);

            try
            {
                if (!overRide)
                    if (Block.OPBlocks(b) || (Block.OPBlocks(type) && extraInfo != "")) return;

                if (Block.Convert(b) != Block.Convert(type))
                    //Should save bandwidth sending identical looking blocks, like air/op_air changes.
                    Player.GlobalBlockchange(this, x, y, z, type);

                SetTile(x, y, z, type); //Updates server level blocks
            }
            catch
            {
                SetTile(x, y, z, type);
            }
        }

        // Returns true if ListCheck does not already have an check in the position.
        // Useful for fireworks, which depend on two physics blocks being checked, one with extraInfo.
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
            if(mapType == MapType.Game)
            {
                return;
            }
            //if (season.started)
            //    season.Stop(this);
            if (blocks == null) return;
            string path = "levels/" + name + ".mcf";
            if (LevelSave != null)
                LevelSave(this);
            //OnLevelSaveEvent.Call(this);
            if (cancelsave1)
            {
                cancelsave1 = false;
                return;
            }
            if (cancelsave)
            {
                cancelsave = false;
                return;
            }
            try
            {
                if (!Directory.Exists("levels")) Directory.CreateDirectory("levels");
                if (!Directory.Exists("levels/level properties")) Directory.CreateDirectory("levels/level properties");

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
                                if (blocks[i] < 57)
                                //CHANGED THIS TO INCOPARATE SOME MORE SPACE THAT I NEEDED FOR THE door_orange_air ETC.
                                {
                                    if(blocks[i] != Block.air)
                                        blockVal = (ushort)blocks[i];
                                }
                                else
                                {

                                }
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
                    else
                    {
                    	File.Move(backFile, path);
                    }

                    SaveSettings(this);

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
                            if (blocks[i] < 80)
                            {
                                level[i] = blocks[i];
                            }
                            else
                            {
                                level[i] = Block.SaveConvert(blocks[i]);
                            }
                        } fs.Write(level, 0, level.Length); fs.Close();
                    }*/
                }
                else
                {
                    Server.s.Log("Skipping level save for " + name + ".");
                }
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
            //season.Start(this);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public MapType mapType;

        public int Backup(bool Forced = false, string backupName = "")
        {
            if (!backedup || Forced)
            {
                int backupNumber = 1;
                string backupPath = @Server.backupLocation;
                if (Directory.Exists(string.Format("{0}/{1}", backupPath, name)))
                {
                    backupNumber = Directory.GetDirectories(string.Format("{0}/" + name, backupPath)).Length + 1;
                }
                else
                {
                    Directory.CreateDirectory(backupPath + "/" + name);
                }
                string path = string.Format("{0}/" + name + "/" + backupNumber, backupPath);
                if (backupName != "")
                {
                    path = string.Format("{0}/" + name + "/" + backupName, backupPath);
                }
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

        public static void CreateLeveldb(string givenName)
        {
          //  Database.executeQuery("CREATE TABLE if not exists `Portals" + givenName +
          //                        "` (EntryX SMALLINT UNSIGNED, EntryY SMALLINT UNSIGNED, EntryZ SMALLINT UNSIGNED, ExitMap CHAR(20), ExitX SMALLINT UNSIGNED, ExitY SMALLINT UNSIGNED, ExitZ SMALLINT UNSIGNED)");
        //    Database.executeQuery("CREATE TABLE if not exists `Zone" + givenName +
        //                          "` (SmallX SMALLINT UNSIGNED, SmallY SMALLINT UNSIGNED, SmallZ SMALLINT UNSIGNED, BigX SMALLINT UNSIGNED, BigY SMALLINT UNSIGNED, BigZ SMALLINT UNSIGNED, Owner VARCHAR(20));");
        }

        public static Level Load(string givenName)
        {
            return Load(givenName, 0);
        }

        //givenName is safe against SQL injections, it gets checked in CmdLoad.cs
        public static Level Load(string givenName, byte phys, bool bite = false) 
        {
			GC.Collect();
            if (LevelLoad != null)
                LevelLoad(givenName);
            //OnLevelLoadEvent.Call(givenName);
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

                        //level.permissionvisit = (LevelPermission)header[14];
                        //level.permissionbuild = (LevelPermission)header[15];
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
                    //level.textures = new LevelTextures(level);
                    level.backedup = true;

                    //level.physic.StartPhysics(level);
                    //level.physChecker.Elapsed += delegate
                    //{
                    //    if (!level.physicssate && level.physics > 0)
                    //        level.physic.StartPhysics(level);
                    //};
                    //level.physChecker.Start();
                    //level.season = new SeasonsCore(level);

                    try
                    {
                        string foundLocation;
                        foundLocation = "levels/level properties/" + level.name + ".properties";
                        if (!File.Exists(foundLocation))
                        {
                            foundLocation = "levels/level properties/" + level.name;
                        }

                        foreach (string line in File.ReadAllLines(foundLocation))
                        {
                            try
                            {
                                if (line[0] == '#') continue;
                                string value = line.Substring(line.IndexOf(" = ") + 3);

                                switch (line.Substring(0, line.IndexOf(" = ")).ToLower())
                                {
                                    case "worldchat":
                                        level.worldChat = bool.Parse(value);
                                        break;
                                    case "perbuild":
                                        level.permissionbuild = PermissionFromName(value) != LevelPermission.Null ? PermissionFromName(value) : LevelPermission.Guest;
                                        break;
                                    case "pervisit":
                                        level.permissionvisit = PermissionFromName(value) != LevelPermission.Null ? PermissionFromName(value) : LevelPermission.Guest;
                                        break;
                                    case "type":
                                        level.mapType = (MapType)Enum.Parse(typeof(MapType), value);
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Server.ErrorLog(e);
                            }
                        }
                    }
                    catch
                    {
                    }
                    level.CalculateShadows();
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

        public static bool CheckLoadOnGoto(string givenName)
        {
            try
            {
                string foundLocation;
                foundLocation = "levels/level properties/" + givenName + ".properties";
                if (!File.Exists(foundLocation))
                    foundLocation = "levels/level properties/" + givenName;
                if (!File.Exists(foundLocation))
                    return true;

                foreach (string line in File.ReadAllLines(foundLocation))
                {
                    try
                    {
                        if (line[0] == '#') continue;
                        string value = line.Substring(line.IndexOf(" = ") + 3);

                        switch (line.Substring(0, line.IndexOf(" = ")).ToLower())
                        {
                            case "loadongoto":
                                return bool.Parse(value);
                        }
                    }
                    catch (Exception e)
                    {
                        Server.ErrorLog(e);
                    }
                }
            }
            catch
            {
            }
            return true;
        }

        public void ChatLevel(string message)
        {
            foreach (Player pl in Player.players.Where(pl => pl.level == this))
            {
                pl.SendMessage(message);
            }
        }

        public void ChatLevelOps(string message)
        {
            foreach (
                Player pl in
                    Player.players.Where(
                        pl =>
                        pl.level == this &&
                        (pl.group.Permission >= Server.opchatperm || pl.isStaff )))
            {
                pl.SendMessage(message);
            }
        }

        public int PosToInt(ushort x, ushort y, ushort z)
        {
            if (x < 0 || x >= width || y < 0 || y >= depth || z < 0 || z >= height)
                return -1;
            return x + (z * width) + (y * width * height);
            //alternate method: (h * widthY + y) * widthX + x;
        }

        public void IntToPos(int pos, out ushort x, out ushort y, out ushort z)
        {
            y = (ushort)(pos / width / height);
            pos -= y * width * height;
            z = (ushort)(pos / width);
            pos -= z * width;
            x = (ushort)pos;
        }

        public int IntOffset(int pos, int x, int y, int z)
        {
            return pos + x + z * width + y * width * height;
        }

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

        public List<Player> getPlayers()
        {
            return Player.players.Where(p => p.level == this).ToList();
        }

        #region ==Physics==

        public string foundInfo(ushort x, ushort y, ushort z)
        {
            Check foundCheck = null;
            try
            {
                foundCheck = ListCheck.Find(Check => Check.b == PosToInt(x, y, z));
            }
            catch
            {
            }
            if (foundCheck != null)
                return foundCheck.extraInfo;
            return "";
        }

        public void AddCheck(int b, string extraInfo = "", bool overRide = false, MCForge.Player Placer = null)
        {
            try
            {
                if (!ListCheck.Exists(Check => Check.b == b))
                {
                    ListCheck.Add(new Check(b, extraInfo, Placer)); //Adds block to list to be updated
                }
                else
                {
                    if (overRide)
                    {
                        foreach (Check C2 in ListCheck)
                        {
                            if (C2.b == b)
                            {
                                C2.extraInfo = extraInfo; //Dont need to check physics here because if the list is active, then physics is active :)
                                return;
                            }
                        }
                    }
                }
            }
            catch
            {
                //ListCheck.Add(new Check(b));    //Lousy back up plan
            }
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
            catch
            {
                //s.Log("Warning-PhysicsUpdate");
                //ListUpdate.Add(new Update(b, (byte)type));    //Lousy back up plan
                return false;
            }
        }

        public void placeBlock(ushort x, ushort y, ushort z, ushort b)
        {
            AddUpdate(PosToInt((ushort)x, (ushort)y, (ushort)z), b, true);
            AddCheck(PosToInt((ushort)x, (ushort)y, (ushort)z));
        }

        public void finiteMovement(Check C, ushort x, ushort y, ushort z)
        {
            var rand = new Random();

            var bufferfiniteWater = new List<int>();
            var bufferfiniteWaterList = new List<Pos>();

            if (GetTile(x, (ushort)(y - 1), z) == Block.air)
            {
                AddUpdate(PosToInt(x, (ushort)(y - 1), z), blocks[C.b], false, C.extraInfo);
                AddUpdate(C.b, Block.air);
                C.extraInfo = "";
            }
            else if (GetTile(x, (ushort)(y - 1), z) == Block.waterstill ||
                     GetTile(x, (ushort)(y - 1), z) == Block.lavastill)
            {
                AddUpdate(C.b, Block.air);
                C.extraInfo = "";
            }
            else
            {
                for (int i = 0; i < 25; ++i) bufferfiniteWater.Add(i);

                for (int k = bufferfiniteWater.Count - 1; k > 1; --k)
                {
                    int randIndx = rand.Next(k); //
                    int temp = bufferfiniteWater[k];
                    bufferfiniteWater[k] = bufferfiniteWater[randIndx]; // move random num to end of list.
                    bufferfiniteWater[randIndx] = temp;
                }

                Pos pos;

                for (var xx = (ushort)(x - 2); xx <= x + 2; ++xx)
                {
                    for (var zz = (ushort)(z - 2); zz <= z + 2; ++zz)
                    {
                        pos.x = xx;
                        pos.z = zz;
                        bufferfiniteWaterList.Add(pos);
                    }
                }

                foreach (int i in bufferfiniteWater)
                {
                    pos = bufferfiniteWaterList[i];
                    if (GetTile(pos.x, (ushort)(y - 1), pos.z) == Block.air&&
                        GetTile(pos.x, y, pos.z) == Block.air)
                    {
                        if (pos.x < x) pos.x = (ushort)(Math.Floor((double)(pos.x + x) / 2));
                        else pos.x = (ushort)(Math.Ceiling((double)(pos.x + x) / 2));
                        if (pos.z < z) pos.z = (ushort)(Math.Floor((double)(pos.z + z) / 2));
                        else pos.z = (ushort)(Math.Ceiling((double)(pos.z + z) / 2));

                        if (GetTile(pos.x, y, pos.z) == Block.air)
                        {
                            if (AddUpdate(PosToInt(pos.x, y, pos.z), blocks[C.b], false, C.extraInfo))
                            {
                                AddUpdate(C.b, Block.air);
                                C.extraInfo = "";
                                break;
                            }
                        }
                    }
                }
            }
        }

        public struct Pos
        {
            public ushort x, z;
        }

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
    public MCForge.Player p;

    public Check(int b, string extraInfo = "", MCForge.Player placer = null)
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
