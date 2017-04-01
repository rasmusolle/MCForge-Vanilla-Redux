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
using System.IO;

namespace MCForge {
	public class EconomyDB {
		public static bool Load( Economy.EcoStats p ) {
			if ( File.Exists( "players/economy/" + p.playerName.ToLower() + "DB.txt" ) ) {
				foreach ( string line in File.ReadAllLines( "players/economy/" + p.playerName.ToLower() + "DB.txt" ) ) {
					if ( !string.IsNullOrEmpty( line ) && !line.StartsWith( "#" ) ) {
						string key = line.Split( '=' )[0].Trim();
						string value = line.Split( '=' )[1].Trim();
						string section = "nowhere yet...";

						try {
							switch ( key.ToLower() ) {
								case "money":
								p.money = int.Parse(value);
								section = key;
								break;
								case "total":
								p.totalSpent = int.Parse(value);
								section = key;
								break;
								case "purchase":
								p.purchase = value;
								section = key;
								break;
								case "payment":
								p.payment = value;
								section = key;
								break;
							}
						} catch(Exception e) {
							Server.s.Log( "Loading " + p.playerName + "'s economy database failed at section: " + section );
							Server.ErrorLog( e );
						}
					}
				}
				return true;
			} else {
				p.money = Player.Find(p.playerName).money;
				p.payment = "";
				p.purchase = "";
				p.totalSpent = 0;
				Save( p );
				return false;
			}
		}

		public static void Save( Economy.EcoStats p ) {
			StreamWriter sw = new StreamWriter( File.Create( "players/economy/" + p.playerName.ToLower() + "DB.txt" ) );
			sw.WriteLine( "Money = " + p.money);
			sw.WriteLine( "Total = " + p.totalSpent);
			sw.WriteLine( "Purchase = " + p.purchase);
			sw.WriteLine( "Payment = " + p.payment);
			sw.Flush();
			sw.Close();
			sw.Dispose();
		}
	}
}
