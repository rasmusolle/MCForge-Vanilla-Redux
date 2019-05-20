/*
	Copyright 2011-2014 MCForge-Redux (Modified for use with MCSpleef)

	Dual-licensed under the Educational Community License, Version 2.0 and
	the GNU General Public License, Version 3 (the "Licenses"); you may
	not use this file except in compliance with the Licenses. You may
	obtain a copy of the Licenses at

	http://www.opensource.org/licenses/ecl2.php
	http://www.gnu.org/licenses/gpl-3.0.html

	Unless required by applicable law or agreed to in writing,
	software distributed under the Licenses are distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the Licenses for the specific language governing
	permiusing MCForge;ssions and limitations under the Licenses.
*/
﻿using System;
namespace MCSpleef {
	public static class Checktimer {
		static System.Timers.Timer t;
		public static void StartTimer() {
			t = new System.Timers.Timer();
			t.AutoReset = false;
			t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
			t.Interval = GetInterval();
			t.Start();
		}
		static double GetInterval() {
			DateTime now = DateTime.Now;
			return ((60 - now.Second) * 1000 - now.Millisecond);
		}
		/// <summary>
		/// Put methods to make them execute every 60 seconds
		/// </summary>
		/// <param name="sender">For the timer</param>
		/// <param name="e">For the timer</param>
		static void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
			t.Interval = GetInterval();
			t.Start();

			// methods to be executed every 60 seconds

		}
	}
}