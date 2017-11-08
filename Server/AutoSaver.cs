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
/*
using System;
using System.Threading;
namespace MCSpleef
{
	public class AutoSaver
	{
		static int _interval;

		static int count = 1;
		public AutoSaver(int interval)
		{
			_interval = interval * 1000;

			new Thread(new ThreadStart(delegate
			{
				while (true)
				{
					Thread.Sleep(_interval);
					Server.ml.Queue(delegate { Run(); });
				}
			})).Start();
		}

		public static void Run()
		{
			try
			{
				count--;
				Server.levels.ForEach(delegate(Level l)
				{
					try
					{
						if (!l.changed) return;
						l.Save();
					}
					catch { Server.s.Log("Save for " + l.name + " has caused an error."); }
				});
				if (count <= 0) count = 15;
			}
			catch (Exception e) { Server.ErrorLog(e); }
		}
	}
}
*/