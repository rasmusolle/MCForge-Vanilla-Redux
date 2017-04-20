/*
	Copyright 2014 MCForge-Redux (Modified for use with MCSpleef)
		
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
namespace MCSpleef
{
	public partial class Player
	{
		public int ClickDistance = 0;
		public int HeldBlock = 0;
		public int TextHotKey = 0;
		public int EnvColors = 0;
		public int SelectionCuboid = 0;
		public int BlockPermissions = 0;
		public int ChangeModel = 0;
		public int EnvMapAppearance = 0;
		public int HackControl = 0;
		public int EmoteFix = 0;
		public int MessageTypes = 0;

		public void AddExtension(string Extension, int version)
		{
			lock (this)
			{
				switch (Extension.Trim())
				{
					case "ClickDistance":
						ClickDistance = version;
						break;
					case "HeldBlock":
						HeldBlock = version;
						break;
					case "TextHotKey":
						TextHotKey = version;
						break;
					case "EnvColors":
						EnvColors = version;
						break;
					case "SelectionCuboid":
						SelectionCuboid = version;
						break;
					case "BlockPermissions":
						BlockPermissions = version;
						break;
					case "ChangeModel":
						ChangeModel = version;
						break;
					case "EnvMapAppearance":
						EnvMapAppearance = version;
						break;
					case "HackControl":
						HackControl = version;
						break;
					case "EmoteFix":
						EmoteFix = version;
						break;
					case "MessageTypes":
						MessageTypes = version;
						break;
				}
			}
		}

		public bool HasExtension(string Extension, int version = 1)
		{
			if (!extension) return false;
			switch (Extension)
			{
				case "ClickDistance": return ClickDistance == version;
				case "HeldBlock": return HeldBlock == version;
				case "TextHotKey": return TextHotKey == version;
				case "EnvColors": return EnvColors == version;
				case "SelectionCuboid": return SelectionCuboid == version;
				case "BlockPermissions": return BlockPermissions == version;
				case "ChangeModel": return ChangeModel == version;
				case "EnvMapAppearance": return EnvMapAppearance == version;
				case "HackControl": return HackControl == version;
				case "EmoteFix": return EmoteFix == version;
				case "MessageTypes": return MessageTypes == version;
				default: return false;
			}
		}
	}
}
