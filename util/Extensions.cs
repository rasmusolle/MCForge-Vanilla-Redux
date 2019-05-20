/*
	Copyright 2011-2014 MCForge-Redux (Modified for use with MCSpleef)
	
	Author: fenderrock87
	
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
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

using System.Collections.Generic;
using System.Threading;
namespace MCSpleef {
	public static class Extensions {
		public static string Truncate(this string source, int maxLength) {
			if (source.Length > maxLength) { source = source.Substring(0, maxLength); }
			return source;
		}
		public static byte[] GZip(this byte[] bytes) {
			using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
				GZipStream gs = new GZipStream(ms, CompressionMode.Compress, true);
				gs.Write(bytes, 0, bytes.Length);
				gs.Close();
				ms.Position = 0;
				bytes = new byte[ms.Length];
				ms.Read(bytes, 0, (int)ms.Length);
				ms.Close();
				ms.Dispose();
			}
			return bytes;
		}
	}
	// MainLoop.cs
	public delegate void MainLoopResult(object result);
	public delegate object MainLoopJob();
	public delegate void MainLoopTask();

	public class MainLoop {
		private class SchedulerTask {
			public Exception StoredException;
			public MainLoopTask Task;

			public void Execute() {
				try {
					Task();
				} catch (Exception ex) {
					StoredException = ex;
					throw;
				}
			}
		}

		AutoResetEvent handle = new AutoResetEvent(false);
		Queue<SchedulerTask> tasks = new Queue<SchedulerTask>();
		internal Thread thread;

		public MainLoop(string name) {
			thread = new Thread(Loop);
			thread.Name = name;
			thread.IsBackground = true;
			thread.Start();
		}

		void Loop() {
			while (true) {
				SchedulerTask task = null;
				lock (tasks) {
					if (tasks.Count > 0)
						task = tasks.Dequeue();
				}

				if (task == null) {
					handle.WaitOne();
				} else {
					task.Execute();
				}
				Thread.Sleep(10);
			}
		}

		/// <summary> Queues an action that is asynchronously executed. </summary>
		public void Queue(MainLoopTask action) {
			SchedulerTask task = new SchedulerTask();
			task.Task = action;

			lock (tasks) {
				tasks.Enqueue(task);
				handle.Set();
			}
		}
	}
}