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
using System.Collections.Generic;

namespace MCSpleef
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

					case water:
					case lava:
					case deathlava:
					case activedeathlava:
					case blackrock:
						b.lowestRank = LevelPermission.Operator;
						break;

					default:
						b.lowestRank = LevelPermission.Banned;
						break;
				}

				storedList.Add(b);
			}

			BlockList.Clear();
			BlockList.AddRange(storedList);
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
	}
}