/*
	Copyright � 2011-2014 MCForge-Redux
		
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
using System.Linq;

namespace MCForge
{
    public class OnServerLogEvent
    {
        internal static List<OnServerLogEvent> events = new List<OnServerLogEvent>();
        Plugin plugin;
        Server.OnServerLog method;
        Priority priority;
        internal OnServerLogEvent(Server.OnServerLog method, Priority priority, Plugin plugin) { this.plugin = plugin; this.priority = priority; this.method = method; }
        public static void Call(string message)
        {
            events.ForEach(delegate(OnServerLogEvent p1)
            {
                try
                {
                    p1.method(message);
                }
                catch (Exception e) { Server.s.Log("The plugin " + p1.plugin.name + " errored when calling the Server Log Event!"); Server.ErrorLog(e); }
            });
        }
        static void Organize()
        {
            List<OnServerLogEvent> temp = new List<OnServerLogEvent>();
            List<OnServerLogEvent> temp2 = events;
            OnServerLogEvent temp3 = null;
            int i = 0;
            int ii = temp2.Count;
            while (i < ii)
            {
                foreach (OnServerLogEvent p in temp2)
                {
                    if (temp3 == null)
                        temp3 = p;
                    else if (temp3.priority < p.priority)
                        temp3 = p;
                }
                temp.Add(temp3);
                temp2.Remove(temp3);
                temp3 = null;
                i++;
            }
            events = temp;
        }
        /// <summary>
        /// Find a event
        /// </summary>
        /// <param name="plugin">The plugin that registered this event</param>
        /// <returns>The event</returns>
        public static OnServerLogEvent Find(Plugin plugin)
        {
            return events.ToArray().FirstOrDefault(p => p.plugin == plugin);
        }

        /// <summary>
        /// Register this event
        /// </summary>
        /// <param name="method">This is the delegate that will get called when this event occurs</param>
        /// <param name="priority">The priority (imporantce) of this call</param>
        /// <param name="plugin">The plugin object that is registering the event</param>
        /// <param name="bypass">Register more than one of the same event</param>
        public static void Register(Server.OnServerLog method, Priority priority, Plugin plugin, bool bypass = false)
        {
            if (Find(plugin) != null)
                if (!bypass)
                throw new Exception("The user tried to register 2 of the same event!");
            events.Add(new OnServerLogEvent(method, priority, plugin));
            Organize();
        }
        /// <summary>
        /// UnRegister this event
        /// </summary>
        /// <param name="plugin">The plugin object that has this event registered</param>
        public static void UnRegister(Plugin plugin)
        {
            if (Find(plugin) == null)
                throw new Exception("This plugin doesnt have this event registered!");
            events.Remove(Find(plugin));
        }
    }
}
