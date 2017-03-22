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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MCForge.Gui {
    public partial class Window : Form {
        // What is this???
        /*Regex regex = new Regex(@"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\." +
                                "([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$");*/
        
        // for cross thread use
        delegate void StringCallback(string s);
        delegate void PlayerListCallback(List<Player> players);
        //delegate void ReportCallback(Report r);
        delegate void VoidDelegate();
        public static bool fileexists = false;
        public static Window thisWindow;

        PlayerCollection pc = new PlayerCollection(new PlayerListView());
        LevelCollection lc = new LevelCollection(new LevelListView());
        LevelCollection lcTAB = new LevelCollection(new LevelListViewForTab());

        //public static event EventHandler Minimize;
        public NotifyIcon notifyIcon1 = new NotifyIcon();
        //  public static bool Minimized = false;

        internal static Server s;

        readonly System.Timers.Timer UpdateListTimer = new System.Timers.Timer(10000);

        public Window() {
            InitializeComponent();
        }

        private void Window_Load(object sender, EventArgs e) {
            btnProperties.Enabled = false;
            //thisWindow = this;
            MaximizeBox = false;
            this.Text = "Starting MCForge...";
            this.Show();
            this.BringToFront();
            WindowState = FormWindowState.Normal;
            new Thread(() => {
                s = new Server();
                s.OnLog += WriteLine;
                s.OnCommand += newCommand;
                s.OnError += newError;
                s.OnSystem += newSystem;
                s.OnAdmin += WriteAdmin;
                s.OnOp += WriteOp;


                s.HeartBeatFail += HeartBeatFail;
                s.OnURLChange += UpdateUrl;
                s.OnPlayerListChange += UpdateClientList;
                s.OnSettingsUpdate += SettingsUpdate;
                s.Start();


                RunOnUiThread(() => btnProperties.Enabled = true);

            }).Start();


            notifyIcon1.Text = ( "MCForge Server: " + Server.name ).Truncate(64);

            this.notifyIcon1.ContextMenuStrip = this.iconContext;
            this.notifyIcon1.Icon = this.Icon;
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);

            if ( File.Exists("Changelog.txt") ) {
                txtChangelog.Text = "Changelog for " + Server.Version + ":";
                foreach ( string line in File.ReadAllLines(( "Changelog.txt" )) ) {
                    txtChangelog.AppendText("\r\n           " + line);
                }
            }

            UpdateListTimer.Elapsed += delegate {
                try {
                    UpdateClientList(Player.players);
                }
                catch { } // needed for slower computers
                //Server.s.Log("Lists updated!");
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

        void HeartBeatFail() {
            WriteLine("Recent Heartbeat Failed");
        }

        void newError(string message) {
            try {
                if ( txtErrors.InvokeRequired ) {
                    this.Invoke(new LogDelegate(newError), new object[] { message });
                }
                else {
                    txtErrors.AppendText(Environment.NewLine + message);
                }
            }
            catch { }
        }
        void newSystem(string message) {
            try {
                if ( txtSystem.InvokeRequired ) {
                    this.Invoke(new LogDelegate(newSystem), new object[] { message });
                }
                else {
                    txtSystem.AppendText(Environment.NewLine + message);
                }
            }
            catch { }
        }

        delegate void LogDelegate(string message);

        /// <summary>
        /// Does the same as Console.WriteLine() only in the form
        /// </summary>
        /// <param name="s">The line to write</param>
        public void WriteLine(string s) {
            if ( Server.shuttingDown ) return;
            if ( this.InvokeRequired ) {
                this.Invoke(new LogDelegate(WriteLine), new object[] { s });
            }
            else {

                string cleaned = s;
                //Begin substring of crappy date stamp

                int substr = s.IndexOf(')');
                if ( substr == -1 ) {
                    cleaned = s;
                }
                else {
                    cleaned = s.Substring(substr + 1);
                }

                //end substring

                txtLog.AppendLog(cleaned + Environment.NewLine);
                // ColorBoxes(txtLog);
            }
        }


        public void WriteOp(string s) {
            if ( Server.shuttingDown ) return;
            if ( this.InvokeRequired ) {
                this.Invoke(new LogDelegate(WriteOp), new object[] { s });
            }
            else {
                //txtLog.AppendText(Environment.NewLine + s);

            }
        }

        public void WriteAdmin(string s) {
            if ( Server.shuttingDown ) return;
            if ( this.InvokeRequired ) {
                this.Invoke(new LogDelegate(WriteAdmin), new object[] { s });
            }
            else {
                //txtLog.AppendText(Environment.NewLine + s);

            }
        }

        /// <summary>
        /// Updates the list of client names in the window
        /// </summary>
        /// <param name="players">The list of players to add</param>
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


        /// <summary>
        /// Places the server's URL at the top of the window
        /// </summary>
        /// <param name="s">The URL to display</param>
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
                MCForge_.Gui.Program.ExitProgram(false);
            }
            if (Server.shuttingDown || MessageBox.Show("Really Shutdown the Server? All Connections will break!", "Exit", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                if (!Server.shuttingDown) {
                    MCForge_.Gui.Program.ExitProgram(false);
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
            }
            else if ( txtCommands.Text != "" ) {
                sentCmd = txtCommands.Text;
            }
            else {
                return;
            }

            new Thread(() => {
                try {
                    Command commandcmd = Command.all.Find(sentCmd);
                    if ( commandcmd == null ) {
                        Server.s.Log("No such command!");
                        return;
                    }
                    if (!Player.CommandProtected(sentCmd, sentMsg)) {
                        commandcmd.Use(null, sentMsg);
                    } else { Server.s.Log("Cannot use command, player has protection level: " + Server.forgeProtection); };
                    newCommand("CONSOLE: USED /" + sentCmd + " " + sentMsg);

                }
                catch ( Exception ex ) {
                    Server.ErrorLog(ex);
                    newCommand("CONSOLE: Failed command.");
                }
            }).Start();

            txtCommands.Clear();
        }

        private void btnClose_Click_1(object sender, EventArgs e) {
            Close();
        }

        public void newCommand(string p) {
            if ( txtCommandsUsed.InvokeRequired ) {
                LogDelegate d = newCommand;
                this.Invoke(d, new object[] { p });
            }
            else {
                txtCommandsUsed.AppendTextAndScroll(p);
            }
        }

        private void btnProperties_Click_1(object sender, EventArgs e) {

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

        private Level GetSelectedLevel() {

            if ( this.dgvMaps.SelectedRows.Count <= 0 )
                return null;

            return (Level)( this.dgvMaps.SelectedRows[0].DataBoundItem );
        }

        private void clonesToolStripMenuItem_Click(object sender, EventArgs e) {
            playerselect("clones");
        }

        private void voiceToolStripMenuItem_Click(object sender, EventArgs e) {
            playerselect("voice");
        }

        private void whoisToolStripMenuItem_Click(object sender, EventArgs e) {
            playerselect("whois");
        }

        private void kickToolStripMenuItem_Click(object sender, EventArgs e) {
            playerselect("kick", " You have been kicked by the console.");
        }


        private void banToolStripMenuItem_Click(object sender, EventArgs e) {
            playerselect("ban");
        }

        private void playerselect(string com) {
            if ( GetSelectedPlayer() != null )
                Command.all.Find(com).Use(null, GetSelectedPlayer().name);
        }
        private void playerselect(string com, string args) {
            if ( GetSelectedPlayer() != null )
                Command.all.Find(com).Use(null, GetSelectedPlayer().name + args);
        }

        private void finiteModeToolStripMenuItem_Click(object sender, EventArgs e) {
            levelcommand("map", " finite");
        }

        private void animalAIToolStripMenuItem_Click(object sender, EventArgs e) {
            levelcommand("map", " ai");
        }

        private void edgeWaterToolStripMenuItem_Click(object sender, EventArgs e) {
            levelcommand("map", " edge");
        }

        private void growingGrassToolStripMenuItem_Click(object sender, EventArgs e) {
            levelcommand("map", " grass");
        }

        private void survivalDeathToolStripMenuItem_Click(object sender, EventArgs e) {
            levelcommand("map", " death");
        }

        private void killerBlocksToolStripMenuItem_Click(object sender, EventArgs e) {
            levelcommand("map", " killer");
        }

        private void rPChatToolStripMenuItem_Click(object sender, EventArgs e) {
            levelcommand("map", " chat");
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            levelcommand("save");
        }

        private void levelcommand(string com) {
            if ( GetSelectedLevel() != null )
                Command.all.Find(com).Use(null, GetSelectedLevel().name);
        }

        private void levelcommand(string com, string args) {
            if ( GetSelectedLevel() != null )
                Command.all.Find(com).Use(null, GetSelectedLevel().name + args);
        }

        private void Restart_Click(object sender, EventArgs e) {
            if ( MessageBox.Show("Are you sure you want to restart?", "Restart", MessageBoxButtons.OKCancel) == DialogResult.OK ) {
                MCForge_.Gui.Program.ExitProgram(true);
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

        private void promoteToolStripMenuItem_Click(object sender, EventArgs e) {
            playerselect("promote");
        }

        private void demoteToolStripMenuItem_Click(object sender, EventArgs e) {
            playerselect("demote");
        }


        private void button_saveall_Click(object sender, EventArgs e) {
            Command.all.Find("save").Use(null, "all");
        }


        /*
        private void button1_Click(object sender, EventArgs e) {
            //Prevent derpy from getting in here..
            if ( !Server.UseTextures ) {
                WoM.Enabled = false;
                return;
            }
            if ( GetSelectedLevelTab() == null ) return;
            var textures = new GUI.Textures { l = GetSelectedLevelTab() };
            textures.Show();
            textures.FormClosing += delegate {
                textures.Dispose();
            };
        }
        */

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

        private void tabPage8_Click(object sender, EventArgs e)
        {

        }
    }
}
