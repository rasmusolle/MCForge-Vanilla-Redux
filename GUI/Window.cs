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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
namespace MCSpleef.Gui 
{
	public partial class Window : Form 
	{
		delegate void StringCallback(string s);
		delegate void PlayerListCallback(List<Player> players);
		delegate void VoidDelegate();
		public static bool fileexists = false;
		public static Window thisWindow;

		PlayerCollection pc = new PlayerCollection(new PlayerListView());

		public NotifyIcon notifyIcon1 = new NotifyIcon();

		internal static Server s;

		readonly System.Timers.Timer UpdateListTimer = new System.Timers.Timer(10000);

		public Window() { InitializeComponent(); }

		private void Window_Load(object sender, EventArgs e) {
			btnProperties.Enabled = false;
			MaximizeBox = false;
			this.Text = "Starting Server...";
			this.Show();
			this.BringToFront();
			WindowState = FormWindowState.Normal;
			new Thread(() => {
				s = new Server();
				s.OnLog += WriteLine;
				s.OnCommand += newCommand;
				s.OnError += newError;

				s.HeartBeatFail += HeartBeatFail;
				s.OnURLChange += UpdateUrl;
				s.OnPlayerListChange += UpdateClientList;
				s.OnSettingsUpdate += SettingsUpdate;
				s.Start();

				RunOnUiThread(() => btnProperties.Enabled = true);
			}).Start();

			notifyIcon1.Text = ( "Server: " + Server.name ).Truncate(64);

			this.notifyIcon1.ContextMenuStrip = this.iconContext;
			this.notifyIcon1.Icon = this.Icon;
			this.notifyIcon1.Visible = true;
			this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);

			UpdateListTimer.Elapsed += delegate {
				try { UpdateClientList(Player.players); }
				catch { }
			}; UpdateListTimer.Start();
		}

		public void RunOnUiThread(Action act) {
			var d = new VoidDelegate(() => Invoke(new VoidDelegate(act)));  //SOME ADVANCED STUFF RIGHT HERR
			d.Invoke();
		}

		void SettingsUpdate() {
			if ( Server.shuttingDown ) return;
			if ( txtLog.InvokeRequired ) {
				this.Invoke(new VoidDelegate(SettingsUpdate));
			}
			else {
				this.Text = Server.name + " - MCForge " + Server.VersionString;
				notifyIcon1.Text = ( "MCForge Server: " + Server.name ).Truncate(64);
			}
		}

		void HeartBeatFail() { WriteLine("Recent Heartbeat Failed"); }

		void newError(string message) {
			try {
				if ( txtErrors.InvokeRequired ) {
					this.Invoke(new LogDelegate(newError), new object[] { message });
				} else {
					txtErrors.AppendText(Environment.NewLine + message);
				}
			}
			catch { }
		}

		delegate void LogDelegate(string message);

		public void WriteLine(string s) {
			if ( Server.shuttingDown ) return;
			if ( this.InvokeRequired ) {
				this.Invoke(new LogDelegate(WriteLine), new object[] { s });
			} else {
				string cleaned = s;

				int substr = s.IndexOf(')');
				if ( substr == -1 ) {
					cleaned = s;
				} else {
					cleaned = s.Substring(substr + 1);
				}

				txtLog.AppendLog(cleaned + Environment.NewLine);
			}
		}

		public void UpdateClientList(List<Player> players) {

			if ( InvokeRequired ) {
				Invoke(new PlayerListCallback(UpdateClientList), players);
			}
			else {

				if ( dgvPlayers.DataSource == null )
					dgvPlayers.DataSource = pc;

				// Try to keep the same selection on update
				string selected = null;
				if ( pc.Count > 0 && dgvPlayers.SelectedRows.Count > 0 ) {
					selected = ( from DataGridViewRow row in dgvPlayers.Rows where row.Selected select pc[row.Index] ).First().name;
				}

				// Update the data source and control
				//dgvPlayers.SuspendLayout();

				pc = new PlayerCollection(new PlayerListView());
				Player.players.ForEach(p => pc.Add(p));

				//dgvPlayers.Invalidate();
				dgvPlayers.DataSource = pc;
				// Reselect player
				if ( selected != null ) {
					foreach ( Player t in Player.players )
						for ( int j = 0; j < dgvPlayers.Rows.Count; j++ )
							if ( Equals(dgvPlayers.Rows[j].Cells[0].Value, selected) )
								dgvPlayers.Rows[j].Selected = true;
				}

				dgvPlayers.Refresh();
				//dgvPlayers.ResumeLayout();
			}

		}

		public void PopupNotify(string message, ToolTipIcon icon = ToolTipIcon.Info) {
			notifyIcon1.ShowBalloonTip(3000, Server.name, message, icon);
		}

		public delegate void UpdateList();

		public void UpdateUrl(string s) {
			if ( this.InvokeRequired ) {
				StringCallback d = UpdateUrl;
				this.Invoke(d, new object[] { s });
			}
			else
				txtUrl.Text = s;
		}

		private void Window_FormClosing(object sender, FormClosingEventArgs e) {
			if (e.CloseReason == CloseReason.WindowsShutDown) {
				MCSpleef.Gui.Program.ExitProgram(false);
			}
			if (Server.shuttingDown || MessageBox.Show("Really Shutdown the Server? All Connections will break!", "Exit", MessageBoxButtons.OKCancel) == DialogResult.OK) {
				if (!Server.shuttingDown) {
					MCSpleef.Gui.Program.ExitProgram(false);
				}
			}
			else {
				// Prevents form from closing when user clicks the X and then hits 'cancel'
				e.Cancel = true;
			}
		}

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				string text = txtInput.Text.Trim();
				if (String.IsNullOrEmpty(text)) return;
				switch (text[0])
				{
					case '#':
						text = text.Remove(0, 1);
						Player.GlobalMessageOps(text);
						Server.s.Log("(OPs): Console: " + text, false, LogType.Op);
						Server.IRC.Say("Console: " + text, true);
						break;
					default:
						Player.GlobalMessage("Console [&a" + Server.ZallState + Server.DefaultColor + "]:&f " + text);
						Server.IRC.Say("Console [" + Server.ZallState + "]: " + text);
						WriteLine("<CONSOLE> " + text);
						break;
				}
				txtInput.Clear();
			}
		}

		private void txtCommands_KeyDown(object sender, KeyEventArgs e) {
			if ( e.KeyCode != Keys.Enter )
				return;
			string sentCmd, sentMsg = "";

			if ( txtCommands.Text == null || txtCommands.Text.Trim() == "" ) {
				newCommand("CONSOLE: Whitespace commands are not allowed.");
				txtCommands.Clear();
				return;
			}

			if ( txtCommands.Text[0] == '/' && txtCommands.Text.Length > 1 )
				txtCommands.Text = txtCommands.Text.Substring(1);

			if ( txtCommands.Text.IndexOf(' ') != -1 ) {
				sentCmd = txtCommands.Text.Split(' ')[0];
				sentMsg = txtCommands.Text.Substring(txtCommands.Text.IndexOf(' ') + 1);
			} else if ( txtCommands.Text != "" ) {
				sentCmd = txtCommands.Text;
			} else {
				return;
			}

			new Thread(() => {
				try {
					Command commandcmd = Command.all.Find(sentCmd);
					if ( commandcmd == null ) {
						Server.s.Log("No such command!");
						return;
					}
					commandcmd.Use(null, sentMsg);
					newCommand("CONSOLE: USED /" + sentCmd + " " + sentMsg);
				}
				catch ( Exception ex ) {
					Server.ErrorLog(ex);
					newCommand("CONSOLE: Failed command.");
				}
			}).Start();

			txtCommands.Clear();
		}

		private void btnClose_Click_1(object sender, EventArgs e) { Close(); }

		public void newCommand(string p) {
			WriteLine(p);
		}

		public static bool prevLoaded = false;

		private void Window_Resize(object sender, EventArgs e) {
			this.ShowInTaskbar = ( this.WindowState != FormWindowState.Minimized );
		}

		private void notifyIcon1_MouseClick(object sender, MouseEventArgs e) {
			this.Show();
			this.BringToFront();
			WindowState = FormWindowState.Normal;
		}

		private void openConsole_Click(object sender, EventArgs e) {
			this.Show();
			this.BringToFront();
			WindowState = FormWindowState.Normal;
		}

		private void shutdownServer_Click(object sender, EventArgs e) {
			Close();
		}

		private Player GetSelectedPlayer() {

			if ( this.dgvPlayers.SelectedRows.Count <= 0 )
				return null;

			return (Player)( this.dgvPlayers.SelectedRows[0].DataBoundItem );
		}

		private void Restart_Click(object sender, EventArgs e) {
			if ( MessageBox.Show("Are you sure you want to restart?", "Restart", MessageBoxButtons.OKCancel) == DialogResult.OK ) {
				MCSpleef.Gui.Program.ExitProgram(true);
			}

		}

		private void restartServerToolStripMenuItem_Click(object sender, EventArgs e) {
			Restart_Click(sender, e);
		}

		private void DatePicker1_ValueChanged(object sender, EventArgs e) {
			string dayofmonth = dateTimePicker1.Value.Day.ToString().PadLeft(2, '0');
			string year = dateTimePicker1.Value.Year.ToString();
			string month = dateTimePicker1.Value.Month.ToString().PadLeft(2, '0');

			string ymd = year + "-" + month + "-" + dayofmonth;
			string filename = ymd + ".txt";

			if ( !File.Exists(Path.Combine("logs/", filename)) ) {
				//MessageBox.Show("Sorry, the log for " + ymd + " doesn't exist, please select another one");
				LogsTxtBox.Text = "No logs found for: " + ymd;
			}
			else {
				LogsTxtBox.Text = null;
				LogsTxtBox.Text = File.ReadAllText(Path.Combine("logs/", filename));
			}

		}

		private void txtUrl_DoubleClick(object sender, EventArgs e) {
			txtUrl.SelectAll();
		}

		private void dgvPlayers_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e) {
			e.PaintParts &= ~DataGridViewPaintParts.Focus;
		}

		#region Colored Reader Context Menu

		private void nightModeToolStripMenuItem_Click_1(object sender, EventArgs e) {
			if ( MessageBox.Show("Changing to and from night mode will clear your logs. Do you still want to change?", "You sure?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No )
				return;

			txtLog.NightMode = nightModeToolStripMenuItem.Checked;
			nightModeToolStripMenuItem.Checked = !nightModeToolStripMenuItem.Checked;
		}

		private void colorsToolStripMenuItem_Click_1(object sender, EventArgs e) {
			txtLog.Colorize = !colorsToolStripMenuItem.Checked;
			colorsToolStripMenuItem.Checked = !colorsToolStripMenuItem.Checked;

		}

		private void dateStampToolStripMenuItem_Click(object sender, EventArgs e) {
			txtLog.DateStamp = !dateStampToolStripMenuItem.Checked;
			dateStampToolStripMenuItem.Checked = !dateStampToolStripMenuItem.Checked;
		}

		private void autoScrollToolStripMenuItem_Click(object sender, EventArgs e) {
			txtLog.AutoScroll = !autoScrollToolStripMenuItem.Checked;
			autoScrollToolStripMenuItem.Checked = !autoScrollToolStripMenuItem.Checked;
		}

		private void copySelectedToolStripMenuItem_Click(object sender, EventArgs e) {
			if ( String.IsNullOrEmpty(txtLog.SelectedText) )
				return;

			Clipboard.SetText(txtLog.SelectedText, TextDataFormat.Text);
		}
		private void copyAllToolStripMenuItem_Click(object sender, EventArgs e) {
			Clipboard.SetText(txtLog.Text, TextDataFormat.Text);
		}
		private void clearToolStripMenuItem_Click(object sender, EventArgs e) {
			if ( MessageBox.Show("Are you sure you want to clear logs?", "You sure?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes ) {
				txtLog.Clear();
			}
		}
		#endregion
	}
}
