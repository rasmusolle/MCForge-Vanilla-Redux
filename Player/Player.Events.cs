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
namespace MCForge
{
	public partial class Player
	{
		internal bool cancelcommand = false;
		internal bool cancelchat = false;
		internal bool cancelmove = false;
		internal bool cancelBlock = false;
		internal bool cancelmessage = false;

		public delegate void BlockchangeEventHandler(Player p, ushort x, ushort y, ushort z, ushort type);
		//[Obsolete("Please use OnBlockChangeEvent.Register()")]
		public event BlockchangeEventHandler Blockchange = null;

		public delegate void OnPlayerConnect(Player p);
		public static event OnPlayerConnect PlayerConnect = null;

		public delegate void OnPlayerDisconnect(Player p, string reason);
		public static event OnPlayerDisconnect PlayerDisconnect = null;

		public delegate void OnPlayerChat(Player p, string message);
		//[Obsolete("Please use OnPlayerChatEvent.Register()")]
		public static event OnPlayerChat PlayerChat = null;
		//[Obsolete("Please use OnPlayerChatEvent.Register()")]
		public event OnPlayerChat OnChat = null;

		//[Obsolete("Please use OnMessageRecieveEvent.Register()")]
		public static event OnPlayerChat MessageRecieve = null;
		//[Obsolete("Please use OnMessageRecieveEvent.Register()")]
		public event OnPlayerChat OnMessageRecieve = null;

		public void ClearPlayerChat() { OnChat = null; }
		public void ClearBlockchange() { Blockchange = null; }
		public bool HasBlockchange() { return (Blockchange == null); }
		public object blockchangeObject = null;
	}
}
