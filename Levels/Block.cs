/*
	Copyright © 2009-2014 MCSharp team (Modified for use with MCZall/MCLawl/MCForge/MCForge-Redux)
	
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

namespace MCForge
{
    public class Block
    {
		public const int maxblocks = 261;

		public const ushort air = (ushort)0;
        public const ushort rock = (ushort)1;
        public const ushort grass = (ushort)2;
        public const ushort dirt = (ushort)3;
        public const ushort stone = (ushort)4;
        public const ushort wood = (ushort)5;
        public const ushort shrub = (ushort)6;
        public const ushort blackrock = (ushort)7;// adminium
        public const ushort water = (ushort)8;
        public const ushort waterstill = (ushort)9;
        public const ushort lava = (ushort)10;
        public const ushort lavastill = (ushort)11;
        public const ushort sand = (ushort)12;
        public const ushort gravel = (ushort)13;
        public const ushort goldrock = (ushort)14;
        public const ushort ironrock = (ushort)15;
        public const ushort coal = (ushort)16;
        public const ushort trunk = (ushort)17;
        public const ushort leaf = (ushort)18;
        public const ushort sponge = (ushort)19;
        public const ushort glass = (ushort)20;
        public const ushort red = (ushort)21;
        public const ushort orange = (ushort)22;
        public const ushort yellow = (ushort)23;
        public const ushort lightgreen = (ushort)24;
        public const ushort green = (ushort)25;
        public const ushort aquagreen = (ushort)26;
        public const ushort cyan = (ushort)27;
        public const ushort lightblue = (ushort)28;
        public const ushort blue = (ushort)29;
        public const ushort purple = (ushort)30;
        public const ushort lightpurple = (ushort)31;
        public const ushort pink = (ushort)32;
        public const ushort darkpink = (ushort)33;
        public const ushort darkgrey = (ushort)34;
        public const ushort lightgrey = (ushort)35;
        public const ushort white = (ushort)36;
        public const ushort yellowflower = (ushort)37;
        public const ushort redflower = (ushort)38;
        public const ushort mushroom = (ushort)39;
        public const ushort redmushroom = (ushort)40;
        public const ushort goldsolid = (ushort)41;
        public const ushort iron = (ushort)42;
        public const ushort staircasefull = (ushort)43;
        public const ushort staircasestep = (ushort)44;
        public const ushort brick = (ushort)45;
        public const ushort tnt = (ushort)46;
        public const ushort bookcase = (ushort)47;
        public const ushort stonevine = (ushort)48;
        public const ushort obsidian = (ushort)49;
        public const ushort cobblestoneslab = (ushort)50;
        public const ushort rope = (ushort)51;
        public const ushort sandstone = (ushort)52;
        public const ushort snowreal = (ushort)53;
        public const ushort firereal = (ushort)54;
        public const ushort lightpinkwool = (ushort)55;
        public const ushort forestgreenwool = (ushort)56;
        public const ushort brownwool = (ushort)57;
        public const ushort deepblue = (ushort)58;
        public const ushort turquoise = (ushort)59;
        public const ushort ice = (ushort)60;
        public const ushort ceramictile = (ushort)61;
        public const ushort magmablock = (ushort)62;
        public const ushort pillar = (ushort)63;
        public const ushort crate = (ushort)64;
        public const ushort stonebrick = (ushort)65;
        public const ushort Zero = 0xff;

        //Death
        public const ushort deathlava = (ushort)190;
        public const ushort activedeathlava = (ushort)194;

        // Block.Zero is 255

        public static List<Blocks> BlockList = new List<Blocks>();
        public class Blocks
        {
            public ushort type;
            public LevelPermission lowestRank;
            public List<LevelPermission> disallow = new List<LevelPermission>();
            public List<LevelPermission> allow = new List<LevelPermission>();

            public bool IncludeInBlockProperties()
            {
                if (Block.Name(type).ToLower() == "unknown")
                    return false;

                return true;
            }
        }

        public static void SetBlocks()
        {
            BlockList = new List<Blocks>();
            Blocks b = new Blocks();
            b.lowestRank = LevelPermission.Guest;

            for (int i = 0; i < (maxblocks+ 1); i++)
            {
                b = new Blocks();
                b.type = (ushort)i;
                BlockList.Add(b);
            }

            List<Blocks> storedList = new List<Blocks>();

            foreach (Blocks bs in BlockList)
            {
                b = new Blocks();
                b.type = bs.type;

                switch (bs.type)
                {
                    case Zero:
                        b.lowestRank = LevelPermission.Admin;
                        break;

                    case blackrock:
                        b.lowestRank = LevelPermission.Operator;
                        break;

                    case water:
                    case lava:
                    case deathlava:
                    case activedeathlava:
                        b.lowestRank = LevelPermission.AdvBuilder;
                        break;

                    default:
                        b.lowestRank = LevelPermission.Banned;
                        break;
                }

                storedList.Add(b);
            }

            //CHECK FOR SPECIAL RANK ALLOWANCES SET BY USER
            if (File.Exists("properties/block.properties"))
            {
                string[] lines = File.ReadAllLines("properties/block.properties");

                //if (lines.Length == 0) ; // this is useless?
                /*else */if (lines[0] == "#Version 2")
                {
                    string[] colon = new string[] { " : " };
                    foreach (string line in lines)
                    {
                        if (line != "" && line[0] != '#')
                        {
                            //Name : Lowest : Disallow : Allow
                            string[] block = line.Split(colon, StringSplitOptions.None);
                            Blocks newBlock = new Blocks();

                            if (Block.Ushort(block[0]) == Block.Zero)
                            {
                                continue;
                            }
                            newBlock.type = Block.Ushort(block[0]);

                            string[] disallow = new string[0];
                            if (block[2] != "")
                                disallow = block[2].Split(',');
                            string[] allow = new string[0];
                            if (block[3] != "")
                                allow = block[3].Split(',');

                            try
                            {
                                newBlock.lowestRank = (LevelPermission)int.Parse(block[1]);
                                foreach (string s in disallow) { newBlock.disallow.Add((LevelPermission)int.Parse(s)); }
                                foreach (string s in allow) { newBlock.allow.Add((LevelPermission)int.Parse(s)); }
                            }
                            catch
                            {
                                Server.s.Log("Hit an error on the block " + line);
                                continue;
                            }

                            int current = 0;
                            foreach (Blocks bS in storedList)
                            {
                                if (newBlock.type == bS.type)
                                {
                                    storedList[current] = newBlock;
                                    break;
                                }
                                current++;
                            }
                        }
                    }
                }
                else
                {
                    foreach (string s in lines)
                    {
                        if (s[0] != '#')
                        {
                            try
                            {
                                Blocks newBlock = new Blocks();
                                newBlock.type = Block.Ushort(s.Split(' ')[0]);
                                newBlock.lowestRank = Level.PermissionFromName(s.Split(' ')[2]);
                                if (newBlock.lowestRank != LevelPermission.Null)
                                    storedList[storedList.FindIndex(sL => sL.type == newBlock.type)] = newBlock;
                                else
                                    throw new Exception();
                            }
                            catch { Server.s.Log("Could not find the rank given on " + s + ". Using default"); }
                        }
                    }
                }
            }

            BlockList.Clear();
            BlockList.AddRange(storedList);
            SaveBlocks(BlockList);
        }
        public static void SaveBlocks(List<Blocks> givenList)
        {
            try
            {
				using (StreamWriter w = File.CreateText("properties/block.properties"))
				{
					w.WriteLine("#Version 2");
					w.WriteLine("#   This file dictates what levels may use what blocks");
					w.WriteLine("#   If someone has royally screwed up the ranks, just delete this file and let the server restart");
					w.WriteLine("#   Allowed ranks: " + Group.concatList(false, false, true));
					w.WriteLine("#   Disallow and allow can be left empty, just make sure there's 2 spaces between the colons");
					w.WriteLine("#   This works entirely on permission values, not names. Do not enter a rank name. Use it's permission value");
					w.WriteLine("#   BlockName : LowestRank : Disallow : Allow");
					w.WriteLine("#   lava : 60 : 80,67 : 40,41,55");
					w.WriteLine("");

					foreach (Blocks bs in givenList)
					{
						if (bs.IncludeInBlockProperties())
						{
							string line = Block.Name(bs.type) + " : " + (int)bs.lowestRank + " : " + GrpCommands.getInts(bs.disallow) + " : " + GrpCommands.getInts(bs.allow);
							w.WriteLine(line);
						}
					}
				}
            }
            catch (Exception e) { Server.ErrorLog(e); }
        }

        public static bool canPlace(Player p, ushort b) { return canPlace(p.group.Permission, b); }
        public static bool canPlace(LevelPermission givenPerm, ushort givenBlock)
        {
            foreach (Blocks b in BlockList)
            {
                if (givenBlock == b.type)
                {
                    if ((b.lowestRank <= givenPerm && !b.disallow.Contains(givenPerm)) || b.allow.Contains(givenPerm)) return true;
                    return false;
                }
            }

            return false;
        }

        public static bool Walkthrough(ushort type)
        {
            switch (type)
            {
				case air:
                case water:
                case waterstill:
                case lava:
                case lavastill:
                case yellowflower:
                case redflower:
                case mushroom:
                case redmushroom:
                case shrub:
                    return true;
            }
            return false;
        }

        public static bool AnyBuild(ushort type)
        {
            switch (type)
            {
                case Block.rock:
                case Block.grass:
                case Block.dirt:
                case Block.stone:
                case Block.wood:
                case Block.shrub:
                case Block.sand:
                case Block.gravel:
                case Block.goldrock:
                case Block.ironrock:
                case Block.coal:
                case Block.trunk:
                case Block.leaf:
                case Block.sponge:
                case Block.glass:
                case Block.red:
                case Block.orange:
                case Block.yellow:
                case Block.lightgreen:
                case Block.green:
                case Block.aquagreen:
                case Block.cyan:
                case Block.lightblue:
                case Block.blue:
                case Block.purple:
                case Block.lightpurple:
                case Block.pink:
                case Block.darkpink:
                case Block.darkgrey:
                case Block.lightgrey:
                case Block.white:
                case Block.yellowflower:
                case Block.redflower:
                case Block.mushroom:
                case Block.redmushroom:
                case Block.goldsolid:
                case Block.iron:
                case Block.staircasefull:
                case Block.staircasestep:
                case Block.brick:
                case Block.tnt:
                case Block.bookcase:
                case Block.stonevine:
                case Block.obsidian:
                    return true;
            }
            return false;
        }

        public static bool AllowBreak(ushort type)
        {
            return false;
        }

        public static bool Placable(ushort type)
        {
            switch (type)
            {
                case Block.blackrock:
                case Block.water:
                case Block.waterstill:
                case Block.lava:
                case Block.lavastill:
                    return false;
            }

            if (type > 49) { return false; }
            return true;
        }

        public static bool RightClick(ushort type, bool countAir = false)
        {
            if (countAir && type == Block.air) return true;

            switch (type)
            {
                case Block.water:
                case Block.lava:
                case Block.waterstill:
                case Block.lavastill:
                    return true;
            }
            return false;
        }

        public static bool OPBlocks(ushort type)
        {
            switch (type)
            {
                case Block.blackrock:

                case Block.Zero:
                    return true;
            }
            return false;
        }

        public static bool Death(ushort type)
        {
            switch (type)
            {
                case Block.deathlava:
                case activedeathlava:
                    return true;
            }
            return false;
        }

        public static bool BuildIn(ushort type)
        {
            switch (Block.Convert(type))
            {
				case 0:
                case water:
                case lava:
                case waterstill:
                case lavastill:
                    return true;
            }
            return false;
        }


        public static bool LavaKill(ushort type)
        {
            switch (type)
            {
                case Block.wood:
                case Block.shrub:
                case Block.trunk:
                case Block.leaf:
                case Block.sponge:
                case Block.red:
                case Block.orange:
                case Block.yellow:
                case Block.lightgreen:
                case Block.green:
                case Block.aquagreen:
                case Block.cyan:
                case Block.lightblue:
                case Block.blue:
                case Block.purple:
                case Block.lightpurple:
                case Block.pink:
                case Block.darkpink:
                case Block.darkgrey:
                case Block.lightgrey:
                case Block.white:
                case Block.yellowflower:
                case Block.redflower:
                case Block.mushroom:
                case Block.redmushroom:
                case Block.bookcase:
                    return true;
            }
            return false;
        }
        public static bool WaterKill(ushort type)
        {
            switch (type)
            {
                case Block.shrub:
                case Block.leaf:
                case Block.yellowflower:
                case Block.redflower:
                case Block.mushroom:
                case Block.redmushroom:
                    return true;
            }
            return false;
        }

        public static bool LightPass(ushort type)
        {
            switch (Convert(type))
            {
				case Block.air:
                case Block.glass:
                case Block.leaf:
                case Block.redflower:
                case Block.yellowflower:
                case Block.mushroom:
                case Block.redmushroom:
                case Block.shrub:
                case Block.rope:
                    return true;

                default:
                    return false;
            }
        }

        public static string Name(ushort type)
        {
            switch (type)
            {
				case 0: return "air";
                case 1: return "stone";
                case 2: return "grass";
                case 3: return "dirt";
                case 4: return "cobblestone";
                case 5: return "wood";
                case 6: return "plant";
                case 7: return "adminium";
                case 8: return "active_water";
                case 9: return "water";
                case 10: return "active_lava";
                case 11: return "lava";
                case 12: return "sand";
                case 13: return "gravel";
                case 14: return "gold_ore";
                case 15: return "iron_ore";
                case 16: return "coal";
                case 17: return "tree";
                case 18: return "leaves";
                case 19: return "sponge";
                case 20: return "glass";
                case 21: return "red";
                case 22: return "orange";
                case 23: return "yellow";
                case 24: return "greenyellow";
                case 25: return "green";
                case 26: return "springgreen";
                case 27: return "cyan";
                case 28: return "blue";
                case 29: return "blueviolet";
                case 30: return "indigo";
                case 31: return "purple";
                case 32: return "magenta";
                case 33: return "pink";
                case 34: return "black";
                case 35: return "gray";
                case 36: return "white";
                case 37: return "yellow_flower";
                case 38: return "red_flower";
                case 39: return "brown_shroom";
                case 40: return "red_shroom";
                case 41: return "gold";
                case 42: return "iron";
                case 43: return "double_stair";
                case 44: return "stair";
                case 45: return "brick";
                case 46: return "tnt";
                case 47: return "bookcase";
                case 48: return "mossy_cobblestone";
                case 49: return "obsidian";
                case 50: return "cobblestoneslab";
                case 51: return "rope";
                case 52: return "sandstone";
                case 53: return "snowreal";
                case 54: return "firereal";
                case 55: return "lightpinkwool";
                case 56: return "forestgreenwool";
                case 57: return "brownwool";
                case 58: return "deepblue";
                case 59: return "turquoise";
                case 60: return "ice";
                case 61: return "ceramictile";
                case 62: return "magmablock";
                case 63: return "pillar";
                case 64: return "crate";
                case 65: return "stonebrick";

                case 190: return "hot_lava";
                case activedeathlava: return "active_hot_lava";

                default: return "unknown";
            }
        }
        public static ushort Ushort(string type)
        {
            switch (type.ToLower())
            {
                case "air": return 0;
                case "stone": return 1;
                case "grass": return 2;
                case "dirt": return 3;
                case "cobblestone": return 4;
                case "wood": return 5;
                case "plant": return 6;
                case "solid":
                case "admintite":
                case "blackrock":
                case "adminium": return 7;
                case "activewater":
                case "active_water": return 8;
                case "water": return 9;
                case "activelava":
                case "active_lava": return 10;
                case "lava": return 11;
                case "sand": return 12;
                case "gravel": return 13;
                case "gold_ore": return 14;
                case "iron_ore": return 15;
                case "coal": return 16;
                case "tree": return 17;
                case "leaves": return 18;
                case "sponge": return 19;
                case "glass": return 20;
                case "red": return 21;
                case "orange": return 22;
                case "yellow": return 23;
                case "greenyellow": return 24;
                case "green": return 25;
                case "springgreen": return 26;
                case "cyan": return 27;
                case "blue": return 28;
                case "blueviolet": return 29;
                case "indigo": return 30;
                case "purple": return 31;
                case "magenta": return 32;
                case "pink": return 33;
                case "black": return 34;
                case "gray": return 35;
                case "white": return 36;
                case "yellow_flower": return 37;
                case "red_flower": return 38;
                case "brown_shroom": return 39;
                case "red_shroom": return 40;
                case "gold": return 41;
                case "iron": return 42;
                case "double_stair": return 43;
                case "stair": return 44;
                case "brick": return 45;
                case "tnt": return 46;
                case "bookcase": return 47;
                case "mossy_cobblestone": return 48;
                case "obsidian": return 49;
                case "cobblestoneslab": return 50;
                case "rope": return 51;
                case "sandstone": return 52;
                case "snowreal": return 53;
                case "firereal": return 54;
                case "lightpinkwool": return 55;
                case "forestgreenwool": return 56;
                case "brownwool": return 57;
                case "deepblue": return 58;
                case "turquoise": return 59;
                case "ice": return 60;
                case "ceramictile": return 61;
                case "magmablock": return 62;
                case "pillar": return 63;
                case "crate": return 64;
                case "stonebrick": return 65;

                case "hot_lava": return 190;
                case "ahl":
                case "active_hot_lava": return activedeathlava;

                default: return Zero;
            }
        }

        public static ushort Convert(ushort b)
        {
            switch (b)
            {
                case Block.deathlava: return lavastill;
                case activedeathlava: return lava;

                default:
                    if (b < 66 || b == Block.air) return b; else return 22;
            }
        }

        public static ushort ConvertCPE(ushort b)
        {
            switch (b)
            {
                case 50: return 44;
                case 51: return 39;
                case 52: return 12;
                case 53: return 0;
                case 54: return 10;
                case 55: return 33;
                case 56: return 25;
                case 57: return 3;
                case 58: return 29;
                case 59: return 28;
                case 60: return 20;
                case 61: return 42;
                case 62: return 49;
                case 63: return 36;
                case 64: return 5;
                case 65: return 1;
                default:
                    return b;
            }
        }
    }
}