/*
Copyright © 2009-2014 MCSharp team (Modified for use with MCZall/MCLawl/MCForge/MCForge-Redux)
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
permissions and limitations under the Licenses.
*/
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
//using MCForge.Gui.Popups;
using MCForge.Util;
using Microsoft.Win32;

namespace MCForge.Gui {
    public partial class PropertyWindow : Form {
        System.Timers.Timer lavaUpdateTimer;
        string lsLoadedMap = "";

        public PropertyWindow() {
            InitializeComponent();
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
            this.Font = SystemFonts.IconTitleFont;
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e) {
            if (e.Category == UserPreferenceCategory.Window) {
                this.Font = SystemFonts.IconTitleFont;
            }
        }

        private void PropertyWindow_FormClosing(object sender, FormClosingEventArgs e) {
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
        }

        private void PropertyWindow_Load(object sender, EventArgs e) {

            Object[] colors = new Object[16];
            colors[0] = ( "black" ); colors[1] = ( "navy" );
            colors[2] = ( "green" ); colors[3] = ( "teal" );
            colors[4] = ( "maroon" ); colors[5] = ( "purple" );
            colors[6] = ( "gold" ); colors[7] = ( "silver" );
            colors[8] = ( "gray" ); colors[9] = ( "blue" );
            colors[10] = ( "lime" ); colors[11] = ( "aqua" );
            colors[12] = ( "red" ); colors[13] = ( "pink" );
            colors[14] = ( "yellow" ); colors[15] = ( "white" );
            cmbDefaultColour.Items.AddRange(colors);
            cmbIRCColour.Items.AddRange(colors);
            cmbColor.Items.AddRange(colors);
            cmbGlobalChatColor.Items.AddRange(colors);

            grpIRC.BackColor = Server.irc ? Color.White : Color.LightGray;

            string opchatperm = String.Empty;
            string adminchatperm = String.Empty;
            string verifyadminsperm = String.Empty;
            string grieferstonerank = String.Empty;
            string afkkickrank = String.Empty;
            string viewqueuerank = String.Empty;
            string enterqueuerank = String.Empty;
            string leavequeuerank = String.Empty;
            string clearqueuerank = String.Empty;
            string gotonextrank = String.Empty;

            foreach ( Group grp in Group.GroupList ) {
                cmbDefaultRank.Items.Add(grp.name);
                cmbOpChat.Items.Add(grp.name);
                cmbAdminChat.Items.Add(grp.name);
                cmbVerificationRank.Items.Add(grp.name);
                cmbGrieferStoneRank.Items.Add(grp.name);
                cmbAFKKickPerm.Items.Add(grp.name);
                cmbViewQueue.Items.Add(grp.name);
                cmbEnterQueue.Items.Add(grp.name);
                cmbLeaveQueue.Items.Add(grp.name);
                cmbClearQueue.Items.Add(grp.name);
                cmbGotoNext.Items.Add(grp.name);

                if ( grp.Permission == Server.opchatperm )
                    opchatperm = grp.name;
                if ( grp.Permission == Server.adminchatperm )
                    adminchatperm = grp.name;
                if ( grp.Permission == Server.verifyadminsrank )
                    verifyadminsperm = grp.name;
                if ( grp.Permission == Server.grieferStoneRank )
                    grieferstonerank = grp.name;
                if ( grp.Permission == Server.afkkickperm )
                    afkkickrank = grp.name;
                if ( grp.Permission == Server.reviewenter )
                    enterqueuerank = grp.name;
                if ( grp.Permission == Server.reviewleave )
                    leavequeuerank = grp.name;
                if ( grp.Permission == Server.reviewview )
                    viewqueuerank = grp.name;
                if ( grp.Permission == Server.reviewclear )
                    clearqueuerank = grp.name;
                if ( grp.Permission == Server.reviewnext )
                    gotonextrank = grp.name;
            }
            listPasswords.Items.Clear();
            if ( Directory.Exists("extra/passwords") ) {
                DirectoryInfo di = new DirectoryInfo("extra/passwords/");
                FileInfo[] fi = di.GetFiles("*.xml");
                Thread.Sleep(10);
                foreach ( FileInfo file in fi ) {
                    listPasswords.Items.Add(file.Name.Replace(".xml", ""));
                }
            }
            cmbDefaultRank.SelectedIndex = 1;
            cmbOpChat.SelectedIndex = ( opchatperm != String.Empty ? cmbOpChat.Items.IndexOf(opchatperm) : 1 );
            cmbAdminChat.SelectedIndex = ( adminchatperm != String.Empty ? cmbAdminChat.Items.IndexOf(adminchatperm) : 1 );
            cmbVerificationRank.SelectedIndex = ( verifyadminsperm != String.Empty ? cmbVerificationRank.Items.IndexOf(verifyadminsperm) : 1 );
            cmbGrieferStoneRank.SelectedIndex = ( grieferstonerank != String.Empty ? cmbGrieferStoneRank.Items.IndexOf(grieferstonerank) : 1 );
            cmbAFKKickPerm.SelectedIndex = ( afkkickrank != String.Empty ? cmbAFKKickPerm.Items.IndexOf(afkkickrank) : 1 );
            cmbEnterQueue.SelectedIndex = ( enterqueuerank != String.Empty ? cmbEnterQueue.Items.IndexOf(enterqueuerank) : 1 );
            cmbLeaveQueue.SelectedIndex = ( leavequeuerank != String.Empty ? cmbLeaveQueue.Items.IndexOf(leavequeuerank) : 1 );
            cmbViewQueue.SelectedIndex = ( viewqueuerank != String.Empty ? cmbViewQueue.Items.IndexOf(viewqueuerank) : 1 );
            cmbClearQueue.SelectedIndex = ( clearqueuerank != String.Empty ? cmbClearQueue.Items.IndexOf(clearqueuerank) : 1 );
            cmbGotoNext.SelectedIndex = ( gotonextrank != String.Empty ? cmbGotoNext.Items.IndexOf(gotonextrank) : 1 );

            for ( ushort b = 1; b < 50; b++ )
                cmbGrieferStoneType.Items.Add(Block.Name(b));

            comboBoxProtection.Items.Add("Off");
            comboBoxProtection.Items.Add("Dev");
            comboBoxProtection.Items.Add("Mod");

            //Load server stuff
            LoadProp("properties/server.properties");
            LoadRanks();
            txechx.Checked = Server.UseTextures;
            try {
                LoadCommands();
                LoadBlocks();
                LoadExtraCmdCmds();
            }
            catch {
                Server.s.Log("Failed to load commands and blocks!");
            }

            try { nudCooldownTime.Value = Server.reviewcooldown; }
            catch { }
            try { reviewlist_update(); }
            catch ( Exception ex ) { Server.ErrorLog(ex); }

            //Sigh. I wish there were SOME event to help me.
            foreach(var command in Command.all.commands) {
                if ( Command.core.commands.Contains( command ) )
                    continue;

                lstCommands.Items.Add ( command.name );
            }
        }

        public static bool EditTextOpen = false;

        private void PropertyWindow_Unload(object sender, EventArgs e) {
            lavaUpdateTimer.Dispose();
            Window.prevLoaded = false;
        }

        List<Group> storedRanks = new List<Group>();
        List<GrpCommands.rankAllowance> storedCommands = new List<GrpCommands.rankAllowance>();
        List<Block.Blocks> storedBlocks = new List<Block.Blocks>();

        public void LoadRanks() {
            txtCmdRanks.Text = "The following ranks are available: \r\n\r\n";
            txtcmdranks2.Text = "The following ranks are available: \r\n\r\n";
            listRanks.Items.Clear();
            storedRanks.Clear();
            storedRanks.AddRange(Group.GroupList);
            foreach ( Group grp in storedRanks ) {
                txtCmdRanks.Text += "\t" + grp.name + " (" + (int)grp.Permission + ")\r\n";
                txtcmdranks2.Text += "\t" + grp.name + " (" + (int)grp.Permission + ")\r\n";
                listRanks.Items.Add(grp.trueName + " = " + (int)grp.Permission);
            }
            txtBlRanks.Text = txtCmdRanks.Text;
            listRanks.SelectedIndex = 0;
        }
        public void SaveRanks() {
            Group.saveGroups(storedRanks);
            Group.InitAll();
            LoadRanks();
        }

        public void LoadCommands() {
            listCommands.Items.Clear();
            storedCommands.Clear();
            foreach ( GrpCommands.rankAllowance aV in GrpCommands.allowedCommands ) {
                storedCommands.Add(aV);
                listCommands.Items.Add(aV.commandName);
            }
            if ( listCommands.SelectedIndex == -1 )
                listCommands.SelectedIndex = 0;
        }
        public void SaveCommands() {
            GrpCommands.Save(storedCommands);
            GrpCommands.fillRanks();
            LoadCommands();
        }

        public void LoadBlocks() {
            listBlocks.Items.Clear();
            storedBlocks.Clear();
            storedBlocks.AddRange(Block.BlockList);
            foreach ( Block.Blocks bs in storedBlocks ) {
                if ( Block.Name(bs.type) != "unknown" )
                    listBlocks.Items.Add(Block.Name(bs.type));
            }
            if ( listBlocks.SelectedIndex == -1 )
                listBlocks.SelectedIndex = 0;
        }
        public static bool prevLoaded = false;
        Form PropertyForm;
        //Form UpdateForm; // doesnt seem to be used, uncomment as needed.
        //Form EditTxtForm;

        public void SaveBlocks() {
            Block.SaveBlocks(storedBlocks);
            Block.SetBlocks();
            LoadBlocks();
        }

        public void LoadProp(string givenPath) {
            //int count = 0;
            if ( !File.Exists(givenPath) ) return;
            string[] lines = File.ReadAllLines(givenPath);

            foreach ( string line in lines ) {
                if ( line != "" && line[0] != '#' ) {
                    //int index = line.IndexOf('=') + 1; // not needed if we use Split('=')
                    string key = line.Split('=')[0].Trim();
                    string value = "";
                    if ( line.IndexOf('=') >= 0 )
                        value = line.Substring(line.IndexOf('=') + 1).Trim(); // allowing = in the values
                    string color = "";

                    switch ( key.ToLower() ) {
                        case "server-name":
                            if ( ValidString(value, "![]:.,{}~-+()?_/\\' ") ) txtName.Text = value;
                            else txtName.Text = "[MCForge] Minecraft server";
                            break;
                        case "motd":
                            if ( ValidString(value, "=![]&:.,{}~-+()?_/\\' ") ) txtMOTD.Text = value; // allow = in the motd
                            else txtMOTD.Text = "Welcome to my server!";
                            break;
                        case "port":
                            try { txtPort.Text = Convert.ToInt32(value).ToString(); }
                            catch { txtPort.Text = "25565"; }
                            break;
                        case "verify-names":
                            chkVerify.Checked = ( value.ToLower() == "true" );
                            break;
                        case "public":
                            chkPublic.Checked = ( value.ToLower() == "true" );
                            break;
                        case "world-chat":
                            chkWorld.Checked = ( value.ToLower() == "true" );
                            break;
                        case "max-players":
                            try {
                                if ( Convert.ToByte(value) > 128 ) {
                                    value = "128";
                                }
                                else if ( Convert.ToByte(value) < 1 ) {
                                    value = "1";
                                }
                                numPlayers.Value = Convert.ToInt16(value);
                            }
                            catch {
                                Server.s.Log("max-players invalid! setting to default.");
                                numPlayers.Value = 12;
                            }
                            numGuests.Maximum = numPlayers.Value;
                            break;
                        case "max-guests":
                            try {
                                if ( Convert.ToByte(value) > numPlayers.Value ) {
                                    value = numPlayers.Value.ToString();
                                }
                                else if ( Convert.ToByte(value) < 0 ) {
                                    value = "0";
                                }
                                numGuests.Minimum = 0;
                                numGuests.Maximum = numPlayers.Value;
                                numGuests.Value = Convert.ToInt16(value);
                            }
                            catch {
                                Server.s.Log("max-guests invalid! setting to default.");
                                numGuests.Value = 10;
                            }
                            break;
                        case "max-maps":
                            try {
                                if ( Convert.ToByte(value) > 100 ) {
                                    value = "100";
                                }
                                else if ( Convert.ToByte(value) < 1 ) {
                                    value = "1";
                                }
                                txtMaps.Text = value;
                            }
                            catch {
                                Server.s.Log("max-maps invalid! setting to default.");
                                txtMaps.Text = "5";
                            }
                            break;
                        case "mcforge-protection-level":
                            comboBoxProtection.SelectedItem = value;
                            break;
                        case "irc":
                            chkIRC.Checked = ( value.ToLower() == "true" );
                            break;
                        case "irc-server":
                            txtIRCServer.Text = value;
                            break;
                        case "irc-port":
                            txtIRCPort.Text = value;
                            break;
                        case "irc-nick":
                            txtNick.Text = value;
                            break;
                        case "irc-channel":
                            txtChannel.Text = value;
                            break;
                        case "irc-opchannel":
                            txtOpChannel.Text = value;
                            break;
                        case "irc-identify":
                            chkIrcId.Checked = ( value.ToLower() == "true" );
                            break;
                        case "irc-password":
                            txtIrcId.Text = value;
                            break;
                        case "anti-tunnels":
                            ChkTunnels.Checked = ( value.ToLower() == "true" );
                            break;
                        case "max-depth":
                            txtDepth.Text = value;
                            break;

                        case "rplimit":
                            try { txtRP.Text = value; }
                            catch { txtRP.Text = "500"; }
                            break;
                        case "rplimit-norm":
                            try { txtNormRp.Text = value; }
                            catch { txtNormRp.Text = "10000"; }
                            break;

                        case "log-heartbeat":
                            chkLogBeat.Checked = ( value.ToLower() == "true" );
                            break;

                        case "force-cuboid":
                            chkForceCuboid.Checked = ( value.ToLower() == "true" );
                            break;

                        case "profanity-filter":
                            chkProfanityFilter.Checked = ( value.ToLower() == "true" );
                            break;

                        case "notify-on-join-leave":
                            chkNotifyOnJoinLeave.Checked = ( value.ToLower() == "true" );
                            break;

                        case "backup-time":
                            txtBackup.Text = Convert.ToInt32(value) > 1 ? value : "300";
                            break;

                        case "backup-location":
                            if ( !value.Contains("System.Windows.Forms.TextBox, Text:") )
                                txtBackupLocation.Text = value;
                            break;

                        case "physicsrestart":
                            chkPhysicsRest.Checked = ( value.ToLower() == "true" );
                            break;
                        case "deathcount":
                            chkDeath.Checked = ( value.ToLower() == "true" );
                            break;

                        case "defaultcolor":
                            color = c.Parse(value);

                            if ( color == "" ) {
                                color = c.Name(value); if ( color != "" ) color = value; else { Server.s.Log("Could not find " + value); return; }
                            }
                            cmbDefaultColour.SelectedIndex = cmbDefaultColour.Items.IndexOf(c.Name(color)); break;

                        case "irc-color":
                            color = c.Parse(value);
                            if ( color == "" ) {
                                color = c.Name(value); if ( color != "" ) color = value; else { Server.s.Log("Could not find " + value); return; }
                            }
                            cmbIRCColour.SelectedIndex = cmbIRCColour.Items.IndexOf(c.Name(color)); break;
                        case "default-rank":
                            try {
                                if ( cmbDefaultRank.Items.IndexOf(value.ToLower()) != -1 )
                                    cmbDefaultRank.SelectedIndex = cmbDefaultRank.Items.IndexOf(value.ToLower());
                            }
                            catch { cmbDefaultRank.SelectedIndex = 1; }
                            break;

                        case "cheapmessage":
                            chkCheap.Checked = ( value.ToLower() == "true" );
                            break;
                        case "cheap-message-given":
                            txtCheap.Text = value;
                            break;

                        case "rank-super":
                            chkrankSuper.Checked = ( value.ToLower() == "true" );
                            break;

                        case "custom-ban":
                            chkBanMessage.Checked = ( value.ToLower() == "true" );
                            break;

                        case "custom-ban-message":
                            txtBanMessage.Text = value;
                            break;

                        case "custom-shutdown":
                            chkShutdown.Checked = ( value.ToLower() == "true" );
                            break;

                        case "custom-shutdown-message":
                            txtShutdown.Text = value;
                            break;

                        case "custom-griefer-stone":
                            chkGrieferStone.Checked = ( value.ToLower() == "true" );
                            break;

                        case "custom-griefer-stone-message":
                            txtGrieferStone.Text = value;
                            break;

                        case "auto-restart":
                            chkRestartTime.Checked = ( value.ToLower() == "true" );
                            break;
                        case "restarttime":
                            txtRestartTime.Text = value;
                            break;
                        case "afk-minutes":
                            try { txtafk.Text = Convert.ToInt16(value).ToString(); }
                            catch { txtafk.Text = "10"; }
                            break;
                        case "afk-kick":
                            try { txtAFKKick.Text = Convert.ToInt16(value).ToString(); }
                            catch { txtAFKKick.Text = "45"; }
                            break;
                        case "autoload":
                            chkAutoload.Checked = ( value.ToLower() == "true" );
                            break;
                        case "parse-emotes":
                            chkSmile.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "allow-tp-to-higher-ranks":
                            chkTpToHigherRanks.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "agree-to-rules-on-entry":
                            chkAgreeToRules.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "admins-join-silent":
                            chkAdminsJoinSilent.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "main-name":
                            txtMain.Text = value;
                            break;
                        case "dollar-before-dollar":
                            chk17Dollar.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "money-name":
                            txtMoneys.Text = value;
                            break;
                        /*case "mono":
                            chkMono.Checked = (value.ToLower() == "true") ? true : false;
                            break;*/
                        case "restart-on-error":
                            chkRestart.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "repeat-messages":
                            chkRepeatMessages.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "host-state":
                            if ( value != "" ) txtHost.Text = value;
                            break;
                        case "kick-on-hackrank":
                            hackrank_kick.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "hackrank-kick-time":
                            hackrank_kick_time.Text = value;
                            break;
                        case "server-owner":
                            txtServerOwner.Text = value;
                            break;
                        case "guest-limit-notify":
                            chkGuestLimitNotify.Checked = ( value.ToLower() == "true" );
                            break;
                        case "ignore-ops":
                            chkIgnoreGlobal.Checked = ( value.ToLower() == "true" );
                            break;
                        case "admin-verification":
                            chkEnableVerification.Checked = ( value.ToLower() == "true" );
                            break;
                        case "mute-on-spam":
                            chkSpamControl.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "spam-messages":
                            try {
                                numSpamMessages.Value = Convert.ToInt16(value);
                            }
                            catch {
                                numSpamMessages.Value = 8;
                            }
                            break;
                        case "spam-mute-time":
                            try {
                                numSpamMute.Value = Convert.ToInt16(value);
                            }
                            catch {
                                numSpamMute.Value = 60;
                            }
                            break;
                        case "show-empty-ranks":
                            chkShowEmptyRanks.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;

                        case "global-chat-enabled":
                            chkGlobalChat.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;

                        case "global-chat-nick":
                            if ( value != "" ) txtGlobalChatNick.Text = value;
                            break;

                        case "global-chat-color":
                            color = c.Parse(value);
                            if ( color == "" ) {
                                color = c.Name(value); if ( color != "" ) color = value; else { Server.s.Log("Could not find " + value); return; }
                            }
                            cmbGlobalChatColor.SelectedIndex = cmbGlobalChatColor.Items.IndexOf(c.Name(color)); break;

                        case "griefer-stone-tempban":
                            chkGrieferStoneBan.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;

                        case "griefer-stone-type":
                            try { cmbGrieferStoneType.SelectedIndex = cmbGrieferStoneType.Items.IndexOf(value); }
                            catch {
                                try { cmbGrieferStoneType.SelectedIndex = cmbGrieferStoneType.Items.IndexOf(Block.Name(Convert.ToByte(value))); }
                                catch { Server.s.Log("Could not find " + value); }
                            }
                            break;
                        case "wom-direct":
                            chkWomDirect.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "premium-only":
                            chkPrmOnly.Checked = ( value.ToLower() == "true" ) ? true : false;
                            break;
                        case "view":
                            Server.reviewview = Level.PermissionFromName(value.ToLower());
                            break;
                        case "enter":
                            Server.reviewenter = Level.PermissionFromName(value.ToLower());
                            break;
                        case "leave":
                            Server.reviewleave = Level.PermissionFromName(value.ToLower());
                            break;
                        case "cooldown":
                            try {
                                Server.reviewcooldown = Convert.ToInt32(value.ToLower()) < 600 ? Convert.ToInt32(value.ToLower()) : 600;
                            }
                            catch {
                                Server.reviewcooldown = 60;
                                Server.s.Log("An error occurred reading the review cooldown value");
                            }
                            break;
                        case "clear":
                            Server.reviewclear = Level.PermissionFromName(value.ToLower());
                            break;
                        case "next":
                            Server.reviewnext = Level.PermissionFromName(value.ToLower());
                            break;
                        case "ignoreomnibans":
                            chkIgnoreOmnibans.Checked = value.ToLower() == "true";
                            break;
                    }
                }
            }
            //Save(givenPath);
            //else Save(givenPath);
        }
        public bool ValidString(string str, string allowed) {
            string allowedchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01234567890" + allowed;
            foreach ( char ch in str ) {
                if ( allowedchars.IndexOf(ch) == -1 ) {
                    return false;
                }
            } return true;
        }

        public void Save(string givenPath) {
            try {
                using ( StreamWriter w = new StreamWriter(File.Create(givenPath)) ) {
                    if ( givenPath.IndexOf("server") != -1 ) {
                        saveAll(); // saves everything to the server variables
                        SrvProperties.SaveProps(w); // When we have this, why define it again?
                    }
                }
            }
            catch ( Exception ) {
                Server.s.Log("SAVE FAILED! " + givenPath);
            }
        }

        private void saveAll() {

            Server.name = txtName.Text;
            Server.motd = txtMOTD.Text;
            Server.port = int.Parse(txtPort.Text);
            Server.verify = chkVerify.Checked;
            Server.pub = chkPublic.Checked;
            Server.players = (byte)numPlayers.Value;
            Server.maxGuests = (byte)numGuests.Value;
            Server.maps = byte.Parse(txtMaps.Text);
            Server.worldChat = chkWorld.Checked;
            Server.AutoLoad = chkAutoload.Checked;
            Server.autorestart = chkRestartTime.Checked;
            try { Server.restarttime = DateTime.Parse(txtRestartTime.Text); }
            catch { } // ignore bad values
            Server.restartOnError = chkRestart.Checked;
            Server.level = ( Player.ValidName(txtMain.Text) ? txtMain.Text : "main" );
            Server.irc = chkIRC.Checked;
            Server.ircNick = txtNick.Text;
            Server.ircServer = txtIRCServer.Text;
            Server.ircChannel = txtChannel.Text;
            Server.ircOpChannel = txtOpChannel.Text;
            Server.ircPort = int.Parse(txtIRCPort.Text);
            Server.ircIdentify = chkIrcId.Checked;
            Server.ircPassword = txtIrcId.Text;
            Server.forgeProtection = comboBoxProtection.SelectedItem.ToString() == "Dev" ? ForgeProtection.Dev : comboBoxProtection.SelectedItem.ToString() == "Mod" ? ForgeProtection.Mod : ForgeProtection.Off;

            Server.antiTunnel = ChkTunnels.Checked;
            Server.maxDepth = byte.Parse(txtDepth.Text);
            Server.rpLimit = int.Parse(txtRP.Text);
            Server.rpNormLimit = int.Parse(txtRP.Text);
            Server.physicsRestart = chkPhysicsRest.Checked;
            Server.deathcount = chkDeath.Checked;
            Server.afkminutes = int.Parse(txtafk.Text);
            Server.afkkick = int.Parse(txtAFKKick.Text);
            Server.parseSmiley = chkSmile.Checked;
            Server.dollardollardollar = chk17Dollar.Checked;
            //Server.useWhitelist = ; //We don't have a setting for this?
            Server.moneys = txtMoneys.Text;
            Server.opchatperm = Group.GroupList.Find(grp => grp.name == cmbOpChat.SelectedItem.ToString()).Permission;
            Server.adminchatperm = Group.GroupList.Find(grp => grp.name == cmbAdminChat.SelectedItem.ToString()).Permission;
            Server.logbeat = chkLogBeat.Checked;
            Server.forceCuboid = chkForceCuboid.Checked;
            Server.profanityFilter = chkProfanityFilter.Checked;
            Server.notifyOnJoinLeave = chkNotifyOnJoinLeave.Checked;
            Server.repeatMessage = chkRepeatMessages.Checked;
            Server.ZallState = txtHost.Text;
            Server.agreetorulesonentry = chkAgreeToRules.Checked;
            Server.adminsjoinsilent = chkAdminsJoinSilent.Checked;
            Server.server_owner = txtServerOwner.Text;

            Server.guestLimitNotify = chkGuestLimitNotify.Checked;


            Server.backupInterval = int.Parse(txtBackup.Text);
            Server.backupLocation = txtBackupLocation.Text;

            Server.DefaultColor = c.Parse(cmbDefaultColour.SelectedItem.ToString());
            Server.IRCColour = cmbIRCColour.SelectedItem.ToString();


            //Server.mono = chkMono.Checked;


            Server.customBan = chkBanMessage.Checked;
            Server.customBanMessage = txtBanMessage.Text;
            Server.customShutdown = chkShutdown.Checked;
            Server.customShutdownMessage = txtShutdown.Text;
            Server.customGrieferStone = chkGrieferStone.Checked;
            Server.customGrieferStoneMessage = txtGrieferStone.Text;
            Server.higherranktp = chkTpToHigherRanks.Checked;
            Server.globalignoreops = chkIgnoreGlobal.Checked; // Wasn't in previous setting-saver

            Server.cheapMessage = chkCheap.Checked;
            Server.cheapMessageGiven = txtCheap.Text;
            Server.rankSuper = chkrankSuper.Checked;
            Server.defaultRank = cmbDefaultRank.SelectedItem.ToString();

            Server.hackrank_kick = hackrank_kick.Checked;
            Server.hackrank_kick_time = int.Parse(hackrank_kick_time.Text);


            Server.verifyadmins = chkEnableVerification.Checked;
            Server.verifyadminsrank = Group.GroupList.Find(grp => grp.name == cmbVerificationRank.SelectedItem.ToString()).Permission;

            Server.checkspam = chkSpamControl.Checked;
            Server.spamcounter = (int)numSpamMessages.Value;
            Server.mutespamtime = (int)numSpamMute.Value;
            Server.spamcountreset = (int)numCountReset.Value;

            Server.showEmptyRanks = chkShowEmptyRanks.Checked;

            Server.UseGlobalChat = chkGlobalChat.Checked;
            Server.GlobalChatNick = txtGlobalChatNick.Text;
            Server.GlobalChatColor = cmbGlobalChatColor.SelectedItem.ToString();

            Server.grieferStoneBan = chkGrieferStoneBan.Checked;
            Server.grieferStoneType = Block.Ushort(cmbGrieferStoneType.SelectedItem.ToString());
            Server.grieferStoneRank = Group.GroupList.Find(grp => grp.name == cmbGrieferStoneRank.SelectedItem.ToString()).Permission;

            Server.WomDirect = chkWomDirect.Checked;
            //Server.Server_ALT = ;
            //Server.Server_Disc = ;
            //Server.Server_Flag = ;
            Server.PremiumPlayersOnly = chkPrmOnly.Checked;

            Server.reviewview = Group.GroupList.Find(grp => grp.name == cmbViewQueue.SelectedItem.ToString()).Permission;
            Server.reviewenter = Group.GroupList.Find(grp => grp.name == cmbEnterQueue.SelectedItem.ToString()).Permission;
            Server.reviewleave = Group.GroupList.Find(grp => grp.name == cmbLeaveQueue.SelectedItem.ToString()).Permission;
            Server.reviewclear = Group.GroupList.Find(grp => grp.name == cmbClearQueue.SelectedItem.ToString()).Permission;
            Server.reviewnext = Group.GroupList.Find(grp => grp.name == cmbGotoNext.SelectedItem.ToString()).Permission;
            Server.reviewcooldown = (int)nudCooldownTime.Value;

            Server.IgnoreOmnibans = chkIgnoreOmnibans.Checked;
        }

        private void cmbDefaultColour_SelectedIndexChanged(object sender, EventArgs e) {
            lblDefault.BackColor = Color.FromName(cmbDefaultColour.Items[cmbDefaultColour.SelectedIndex].ToString());
        }

        private void cmbIRCColour_SelectedIndexChanged(object sender, EventArgs e) {
            lblIRC.BackColor = Color.FromName(cmbIRCColour.Items[cmbIRCColour.SelectedIndex].ToString());
        }

        void removeDigit(TextBox foundTxt) {
        }

        private void txtPort_TextChanged(object sender, EventArgs e) { removeDigit(txtPort); }
        private void txtMaps_TextChanged(object sender, EventArgs e) { removeDigit(txtMaps); }
        private void txtBackup_TextChanged(object sender, EventArgs e) { removeDigit(txtBackup); }
        private void txtDepth_TextChanged(object sender, EventArgs e) { removeDigit(txtDepth); }

        private void btnSave_Click(object sender, EventArgs e) { saveStuff(); Dispose(); }
        private void btnApply_Click(object sender, EventArgs e) { saveStuff(); }

        void saveStuff() {
            foreach ( Control tP in tabControl.Controls )
                if ( tP is TabPage && tP != tabPage3 && tP != tabPage5 )
                    foreach ( Control ctrl in tP.Controls )
                        if ( ctrl is TextBox && ctrl.Name.ToLower() != "txtgrpmotd" )
                            if ( ctrl.Text == "" ) {
                                MessageBox.Show("A textbox has been left empty. It must be filled.\n" + ctrl.Name);
                                return;
                            }

            Save("properties/server.properties");
            SaveRanks();
            SaveCommands();
            SaveOldExtraCustomCmdChanges();
            SaveBlocks();

            SrvProperties.Load("properties/server.properties", true); // loads when saving?
            GrpCommands.fillRanks();
        }

        private void btnDiscard_Click(object sender, EventArgs e) {
            this.Dispose();
        }

        private void toolTip_Popup(object sender, PopupEventArgs e) {

        }

        private void tabPage2_Click(object sender, EventArgs e) {

        }

        private void tabPage1_Click(object sender, EventArgs e) {

        }

        private void chkPhysicsRest_CheckedChanged(object sender, EventArgs e) {
        }

        private void chkGC_CheckedChanged(object sender, EventArgs e) {
        }

        private void chkIRC_CheckedChanged(object sender, EventArgs e) {
            if ( !chkIRC.Checked ) {
                grpIRC.BackColor = Color.LightGray;
            }
            else {
                grpIRC.BackColor = Color.White;
            }
        }

        private void btnBackup_Click(object sender, EventArgs e) {
            /*FolderBrowserDialog folderDialog = new FolderBrowserDialog();
folderDialog.Description = "Select Folder";
if (folderDialog.ShowDialog() == DialogResult.OK) {
txtBackupLocation.Text = folderDialog.SelectedPath;
}*/
            MessageBox.Show("Currently glitchy! Just type in the location by hand.");
        }

        #region rankTab
        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e) {
            lblColor.BackColor = Color.FromName(cmbColor.Items[cmbColor.SelectedIndex].ToString());
            storedRanks[listRanks.SelectedIndex].color = c.Parse(cmbColor.Items[cmbColor.SelectedIndex].ToString());
        }

        bool skip = false;
        private void listRanks_SelectedIndexChanged(object sender, EventArgs e) {
            if ( skip ) return;
            Group foundRank = storedRanks.Find(grp => grp.trueName == listRanks.Items[listRanks.SelectedIndex].ToString().Split('=')[0].Trim());
            if ( foundRank.Permission == LevelPermission.Nobody ) { listRanks.SelectedIndex = 0; return; }

            txtRankName.Text = foundRank.trueName;
            txtPermission.Text = ( (int)foundRank.Permission ).ToString();
            txtLimit.Text = foundRank.maxBlocks.ToString();
            txtMaxUndo.Text = foundRank.maxUndo.ToString();
            cmbColor.SelectedIndex = cmbColor.Items.IndexOf(c.Name(foundRank.color));
            txtGrpMOTD.Text = String.IsNullOrEmpty(foundRank.MOTD) ? String.Empty : foundRank.MOTD;
            txtFileName.Text = foundRank.fileName;
        }

        private void txtRankName_TextChanged(object sender, EventArgs e) {
            if ( txtRankName.Text != "" && txtRankName.Text.ToLower() != "nobody" ) {
                storedRanks[listRanks.SelectedIndex].trueName = txtRankName.Text;
                skip = true;
                listRanks.Items[listRanks.SelectedIndex] = txtRankName.Text + " = " + (int)storedRanks[listRanks.SelectedIndex].Permission;
                skip = false;
            }
        }

        private void txtPermission_TextChanged(object sender, EventArgs e) {
            if ( txtPermission.Text != "" ) {
                int foundPerm;
                try {
                    foundPerm = int.Parse(txtPermission.Text);
                }
                catch {
                    if ( txtPermission.Text != "-" )
                        txtPermission.Text = txtPermission.Text.Remove(txtPermission.Text.Length - 1);
                    return;
                }

                if ( foundPerm < -50 ) { txtPermission.Text = "-50"; return; }
                else if ( foundPerm > 119 ) { txtPermission.Text = "119"; return; }

                storedRanks[listRanks.SelectedIndex].Permission = (LevelPermission)foundPerm;
                skip = true;
                listRanks.Items[listRanks.SelectedIndex] = storedRanks[listRanks.SelectedIndex].trueName + " = " + foundPerm;
                skip = false;
            }
        }

        private void txtLimit_TextChanged(object sender, EventArgs e) {
            if ( txtLimit.Text != "" ) {
                int foundLimit;
                try {
                    foundLimit = int.Parse(txtLimit.Text);
                }
                catch {
                    txtLimit.Text = txtLimit.Text.Remove(txtLimit.Text.Length - 1);
                    return;
                }

                if ( foundLimit < 1 ) { txtLimit.Text = "1"; return; }

                storedRanks[listRanks.SelectedIndex].maxBlocks = foundLimit;
            }
        }

        private void txtMaxUndo_TextChanged(object sender, EventArgs e) {
            if ( txtMaxUndo.Text != "" ) {
                long foundMax;
                try {
                    foundMax = long.Parse(txtMaxUndo.Text);
                }
                catch {
                    txtMaxUndo.Text = txtMaxUndo.Text.Remove(txtMaxUndo.Text.Length - 1);
                    return;
                }

                if ( foundMax < -1 ) { txtMaxUndo.Text = "0"; return; }

                storedRanks[listRanks.SelectedIndex].maxUndo = foundMax;
            }

        }

        private void txtFileName_TextChanged(object sender, EventArgs e) {
            if ( txtFileName.Text != "" ) {
                storedRanks[listRanks.SelectedIndex].fileName = txtFileName.Text;
            }
        }

        private void btnAddRank_Click(object sender, EventArgs e) {
            Group newGroup = new Group((LevelPermission)5, 600, 30, "CHANGEME", '1', String.Empty, "CHANGEME.txt");
            storedRanks.Add(newGroup);
            listRanks.Items.Add(newGroup.trueName + " = " + (int)newGroup.Permission);
        }

        private void button1_Click(object sender, EventArgs e) {
            if ( listRanks.Items.Count > 1 ) {
                storedRanks.RemoveAt(listRanks.SelectedIndex);
                skip = true;
                listRanks.Items.RemoveAt(listRanks.SelectedIndex);
                skip = false;

                listRanks.SelectedIndex = 0;
            }
        }
        #endregion

        #region commandTab
        private void listCommands_SelectedIndexChanged(object sender, EventArgs e) {
            Command cmd = Command.all.Find(listCommands.SelectedItem.ToString());
            GrpCommands.rankAllowance allowVar = storedCommands.Find(aV => aV.commandName == cmd.name);

            if ( Group.findPerm(allowVar.lowestRank) == null ) allowVar.lowestRank = cmd.defaultRank;
            txtCmdLowest.Text = (int)allowVar.lowestRank + "";

            bool foundOne = false;
            txtCmdDisallow.Text = "";
            foreach ( LevelPermission perm in allowVar.disallow ) {
                foundOne = true;
                txtCmdDisallow.Text += "," + (int)perm;
            }
            if ( foundOne ) txtCmdDisallow.Text = txtCmdDisallow.Text.Remove(0, 1);

            foundOne = false;
            txtCmdAllow.Text = "";
            foreach ( LevelPermission perm in allowVar.allow ) {
                foundOne = true;
                txtCmdAllow.Text += "," + (int)perm;
            }
            if ( foundOne ) txtCmdAllow.Text = txtCmdAllow.Text.Remove(0, 1);
        }
        private void txtCmdLowest_TextChanged(object sender, EventArgs e) {
            fillLowest(ref txtCmdLowest, ref storedCommands[listCommands.SelectedIndex].lowestRank);
        }
        private void txtCmdDisallow_TextChanged(object sender, EventArgs e) {
            fillAllowance(ref txtCmdDisallow, ref storedCommands[listCommands.SelectedIndex].disallow);
        }
        private void txtCmdAllow_TextChanged(object sender, EventArgs e) {
            fillAllowance(ref txtCmdAllow, ref storedCommands[listCommands.SelectedIndex].allow);
        }
        #endregion

        #region BlockTab
        private void listBlocks_SelectedIndexChanged(object sender, EventArgs e) {
            ushort b = Block.Ushort(listBlocks.SelectedItem.ToString());
            Block.Blocks bs = storedBlocks.Find(bS => bS.type == b);

            txtBlLowest.Text = (int)bs.lowestRank + "";

            bool foundOne = false;
            txtBlDisallow.Text = "";
            foreach ( LevelPermission perm in bs.disallow ) {
                foundOne = true;
                txtBlDisallow.Text += "," + (int)perm;
            }
            if ( foundOne ) txtBlDisallow.Text = txtBlDisallow.Text.Remove(0, 1);

            foundOne = false;
            txtBlAllow.Text = "";
            foreach ( LevelPermission perm in bs.allow ) {
                foundOne = true;
                txtBlAllow.Text += "," + (int)perm;
            }
            if ( foundOne ) txtBlAllow.Text = txtBlAllow.Text.Remove(0, 1);
        }
        private void txtBlLowest_TextChanged(object sender, EventArgs e) {
            fillLowest(ref txtBlLowest, ref storedBlocks[Block.Ushort(listBlocks.SelectedItem.ToString())].lowestRank);
        }
        private void txtBlDisallow_TextChanged(object sender, EventArgs e) {
            fillAllowance(ref txtBlDisallow, ref storedBlocks[listBlocks.SelectedIndex].disallow);
        }
        private void txtBlAllow_TextChanged(object sender, EventArgs e) {
            fillAllowance(ref txtBlAllow, ref storedBlocks[listBlocks.SelectedIndex].allow);
        }
        #endregion
        private void fillAllowance(ref TextBox txtBox, ref List<LevelPermission> addTo) {
            addTo.Clear();
            if ( txtBox.Text != "" ) {
                string[] perms = txtBox.Text.Split(',');
                for ( int i = 0; i < perms.Length; i++ ) {
                    perms[i] = perms[i].Trim().ToLower();
                    int foundPerm;
                    try {
                        foundPerm = int.Parse(perms[i]);
                    }
                    catch {
                        Group foundGroup = Group.Find(perms[i]);
                        if ( foundGroup != null ) foundPerm = (int)foundGroup.Permission;
                        else { Server.s.Log("Could not find " + perms[i]); continue; }
                    }
                    addTo.Add((LevelPermission)foundPerm);
                }

                txtBox.Text = "";
                foreach ( LevelPermission p in addTo ) {
                    txtBox.Text += "," + (int)p;
                }
                if ( txtBox.Text != "" ) txtBox.Text = txtBox.Text.Remove(0, 1);
            }
        }
        private void fillLowest(ref TextBox txtBox, ref LevelPermission toChange) {
            if ( txtBox.Text != "" ) {
                txtBox.Text = txtBox.Text.Trim().ToLower();
                int foundPerm = -100;
                try {
                    foundPerm = int.Parse(txtBox.Text);
                }
                catch {
                    Group foundGroup = Group.Find(txtBox.Text);
                    if ( foundGroup != null ) foundPerm = (int)foundGroup.Permission;
                    else { Server.s.Log("Could not find " + txtBox.Text); }
                }

                txtBox.Text = "";
                if ( foundPerm < -99 ) txtBox.Text = (int)toChange + "";
                else txtBox.Text = foundPerm + "";

                toChange = (LevelPermission)Convert.ToInt16(txtBox.Text);
            }
        }

        private void btnBlHelp_Click(object sender, EventArgs e) {
            getHelp(listBlocks.SelectedItem.ToString());
        }
        private void btnCmdHelp_Click(object sender, EventArgs e) {
            getHelp(listCommands.SelectedItem.ToString());
        }
        private void getHelp(string toHelp) {
            Player.storedHelp = "";
            Player.storeHelp = true;
            Command.all.Find("help").Use(null, toHelp);
            Player.storeHelp = false;
            string messageInfo = "Help information for " + toHelp + ":\r\n\r\n";
            messageInfo += Player.storedHelp;
            MessageBox.Show(messageInfo);
        }

        private void CrtCustCmd_Click(object sender, EventArgs e) {
            if ( txtCommandName.Text != null ) {
                if ( File.Exists("extra/commands/source/Cmd" + txtCommandName.Text + ".cs") ) {
                    MessageBox.Show("Sorry, That command already exists!!");
                }
                else {
                    Command.all.Find("cmdcreate").Use(null, txtCommandName.Text);
                    MessageBox.Show("Command Created!!");
                }
            }
            else {
                MessageBox.Show("You didnt specify a name for the command!!");
            }
        }

        private void CompileCustCmd_Click(object sender, EventArgs e) {
            if ( txtCommandName.Text != null ) {
                if ( File.Exists("extra/commands/dll/Cmd" + txtCommandName.Text + ".dll") ) {
                    MessageBox.Show("Sorry, That command already exists!!");
                }
                else {
                    Command.all.Find("compile").Use(null, txtCommandName.Text);
                    MessageBox.Show("Command Compiled!!");
                }
            }
            else {
                MessageBox.Show("You didnt specify a name for the command!!");
            }
        }



        private void numPlayers_ValueChanged(object sender, EventArgs e) {
            // Ensure that number of guests is never more than number of players
            if ( numGuests.Value > numPlayers.Value ) {
                numGuests.Value = numPlayers.Value;
            }
            numGuests.Maximum = numPlayers.Value;
        }

        private void editTxtsBt_Click_1(object sender, EventArgs e) {
            if ( EditTextOpen ) {
                return;
            }
            PropertyForm = new EditText();
            PropertyForm.Show();
        }

        private void btnCreate_Click(object sender, EventArgs e) {
            if(String.IsNullOrEmpty(txtCommandName.Text.Trim())) {
                MessageBox.Show ( "Command must have a name" );
                return;
            }

            if ( radioButton1.Checked ) {
                if ( File.Exists("extra/commands/source/Cmd" + txtCommandName.Text + ".vb") || File.Exists("extra/commands/source/Cmd" + txtCommandName.Text + ".cs") ) {
                    MessageBox.Show("Command already exists", "", MessageBoxButtons.OK);
                }
                else {
                    Command.all.Find("cmdcreate").Use(null, txtCommandName.Text.ToLower() + " vb");
                    MessageBox.Show("New Command Created: " + txtCommandName.Text.ToLower() + " Created.");
                }
            }
            else {



                if ( File.Exists("extra/commands/source/Cmd" + txtCommandName.Text + ".cs") || File.Exists("extra/commands/source/Cmd" + txtCommandName.Text + ".vb") ) {
                    MessageBox.Show("Command already exists", "", MessageBoxButtons.OK);
                }
                else {
                    Command.all.Find("cmdcreate").Use(null, txtCommandName.Text.ToLower());
                    MessageBox.Show("New Command Created: " + txtCommandName.Text.ToLower() + " Created.");
                }
            }
        }

       

        private void btnLoad_Click(object sender, EventArgs e) {
            Command[] commands = null;

            using(FileDialog fileDialog = new OpenFileDialog()) {

                fileDialog.Filter = "Accepted File Types (*.cs, *.vb, *.dll)|*.cs;*.vb;*.dll|C# Source (*.cs)|*.cs|Visual Basic Source (*.vb)|*.vb|.NET Assemblies (*.dll)|*.dll";

                if ( fileDialog.ShowDialog() == DialogResult.OK ) {

                    string fileName = fileDialog.FileName;

                    if ( fileName.EndsWith ( ".dll" ) ) {
                        commands = MCForgeScripter.FromAssemblyFile ( fileName );
                    }
                    else {

                        ScriptLanguage language = fileName.EndsWith ( ".cs" ) ? ScriptLanguage.CSharp : ScriptLanguage.VB;

                        if ( File.Exists ( fileName ) ) {
                            var result = MCForgeScripter.Compile ( File.ReadAllText ( fileName ), language );

                            if ( result == null ) {
                                MessageBox.Show ( "Error compiling files" );
                                return;
                            }

                            if ( result.CompilerErrors != null ) {
                                foreach ( CompilerError err in result.CompilerErrors ) {
                                    Server.s.ErrorCase( "Error #" + err.ErrorNumber );
                                    Server.s.ErrorCase( "Message: " + err.ErrorText );
                                    Server.s.ErrorCase( "Line: " + err.Line );

                                    Server.s.ErrorCase( "=================================" );
                                }
                                MessageBox.Show ( "Error compiling from source. Check logs for error" );
                                return;
                            }

                            commands = result.Commands;
                        }
                    }
                }
            }

            if ( commands == null ) {
                MessageBox.Show( "Error compiling files" );
                return;
            }

            for ( int i = 0; i < commands.Length; i++ ) {
                Command command = commands[ i ];

                if(lstCommands.Items.Contains(command.name)) {
                    MessageBox.Show ( "Command " + command.name + " already exists. As a result, it was not loaded" );
                    continue;
                }

                lstCommands.Items.Add ( command.name );
                Command.all.Add(command);
                Server.s.Log("Added " + command.name + " to commands");
            }
            
            GrpCommands.fillRanks();
        }

        private void btnUnload_Click(object sender, EventArgs e) {
            Command foundCmd = Command.all.Find(lstCommands.SelectedItem.ToString());
            if ( foundCmd == null ) {
                MessageBox.Show(txtCommandName.Text + " is not a valid or loaded command.", "");
                return;
            }

            lstCommands.Items.Remove( foundCmd.name );
            Command.all.Remove(foundCmd);
            GrpCommands.fillRanks();
            MessageBox.Show("Command was successfully unloaded.", "");
        }


        private void btnDiscardcmd_Click(object sender, EventArgs e) {
            switch ( MessageBox.Show("Are you sure you want to discard this whole file?", "Discard?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ) {
                case DialogResult.Yes:
                    if ( radioButton1.Checked ) {

                        if ( File.Exists("extra/commands/source/Cmd" + txtCommandName.Text + ".vb") ) {
                            File.Delete("extra/commands/source/Cmd" + txtCommandName.Text + ".vb");
                        }
                        else { MessageBox.Show("File: " + txtCommandName.Text + ".vb Doesnt Exist."); }
                    }
                    else {
                        if ( File.Exists("extra/commands/source/Cmd" + txtCommandName.Text + ".cs") ) {
                            File.Delete("extra/commands/source/Cmd" + txtCommandName.Text + ".cs");
                        }
                        else { MessageBox.Show("File: " + txtCommandName.Text + ".cs Doesnt Exist."); }
                    }
                    break;

            }
        }

        private void btnReset_Click(object sender, EventArgs e) {
            if ( listPasswords.Text == "" ) {
                MessageBox.Show("You have not selected a user's password to reset!");
                return;
            }
            try {
                File.Delete("extra/passwords/" + listPasswords.Text + ".xml");
                listPasswords.Items.Clear();
                DirectoryInfo di = new DirectoryInfo("extra/passwords/");
                FileInfo[] fi = di.GetFiles("*.xml");
                Thread.Sleep(10);
                foreach ( FileInfo file in fi ) {
                    listPasswords.Items.Add(file.Name.Replace(".xml", ""));
                }
            }
            catch {
                MessageBox.Show("Failed to reset password!");
            }

        }

        private void groupBox17_Enter(object sender, EventArgs e) {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {

        }

        private void chkZombieOnServerStart_CheckedChanged(object sender, EventArgs e) {

        }

        private void txtNick_TextChanged(object sender, EventArgs e) {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            /*if (chkIrcId.Checked)
            {
                textBox1.Enabled = true;
                textBox1.BackColor = Color.White;
            }
            else
            {
                textBox1.Enabled = false;
                textBox1.BackColor = Color.LightGray;
            }*/
        }

        private void pictureBox1_Click(object sender, EventArgs e) {
            try {
                if ( !Directory.Exists("extra/images") )
                    Directory.CreateDirectory("extra/images");
                if ( !File.Exists( "extra/images/mcpony.png" ) ) {
                    using ( WebClient WEB = new WebClient () ) {
                        WEB.DownloadFileAsync ( new Uri ( "http://mcforge.org/Update/images/mcpony.png" ), "extra/images/mcpony.png" );
                        WEB.DownloadFileCompleted += ( ea, args ) => {
                                                         if ( File.Exists ( "extra/images/mcpony.png" ) ) {
                                                             Image img = Image.FromFile ( "extra/images/mcpony.png" );
                                                             pictureBox1.Image = img;
                                                         }
                                                     };
                    }
                } else {
                    Image img = Image.FromFile( "extra/images/mcpony.png" );
                    pictureBox1.Image = img;
                }
            }
            catch { }
        }

        private void cmbGlobalChatColor_SelectedIndexChanged(object sender, EventArgs e) {
            lblGlobalChatColor.BackColor = Color.FromName(cmbGlobalChatColor.Items[cmbGlobalChatColor.SelectedIndex].ToString());
        }

        private void label55_Click(object sender, EventArgs e) {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e) {

        }


        private void numCountReset_ValueChanged(object sender, EventArgs e) {

        }

        private int oldnumber;
        private Command oldcmd;
        private void listCommandsExtraCmdPerms_SelectedIndexChanged(object sender, EventArgs e) {
            SaveOldExtraCustomCmdChanges();
            Command cmd = Command.all.Find(listCommandsExtraCmdPerms.SelectedItem.ToString());
            oldcmd = cmd;
            extracmdpermnumber.Maximum = CommandOtherPerms.GetMaxNumber(cmd);
            if ( extracmdpermnumber.Maximum == 1 ) { extracmdpermnumber.ReadOnly = true; }
            else { extracmdpermnumber.ReadOnly = false; }
            extracmdpermnumber.Value = 1;
            extracmdpermdesc.Text = CommandOtherPerms.Find(cmd, 1).Description;
            extracmdpermperm.Text = CommandOtherPerms.Find(cmd, 1).Permission.ToString();
            oldnumber = (int)extracmdpermnumber.Value;
        }

        private void SaveOldExtraCustomCmdChanges() {
            if ( oldcmd != null ) {
                CommandOtherPerms.Edit(CommandOtherPerms.Find(oldcmd, oldnumber), int.Parse(extracmdpermperm.Text));
                CommandOtherPerms.Save();
            }
        }

        private void extracmdpermnumber_ValueChanged(object sender, EventArgs e) {
            SaveOldExtraCustomCmdChanges();
            oldnumber = (int)extracmdpermnumber.Value;
            extracmdpermdesc.Text = CommandOtherPerms.Find(oldcmd, (int)extracmdpermnumber.Value).Description;
            extracmdpermperm.Text = CommandOtherPerms.Find(oldcmd, (int)extracmdpermnumber.Value).Permission.ToString();
        }
        private void LoadExtraCmdCmds() {
            listCommandsExtraCmdPerms.Items.Clear();
            foreach ( Command cmd in Command.all.commands ) {
                if ( CommandOtherPerms.Find(cmd) != null ) {
                    listCommandsExtraCmdPerms.Items.Add(cmd.name);
                }
            }
        }

        private void tabControl_Click(object sender, EventArgs e) {
            reviewlist_update();
        }
        public void reviewlist_update() {
            int people = 1;
            listBox1.Items.Clear();
            listBox1.Items.Add("Players in queue:");
            listBox1.Items.Add("----------");
            foreach ( string playerinqueue in Server.reviewlist ) {

                listBox1.Items.Add(people.ToString() + ". " + playerinqueue);
                people++;
            }
            people--;
            listBox1.Items.Add("----------");
            listBox1.Items.Add(people + " player(s) in queue.");
        }

        private void button4_Click(object sender, EventArgs e) {
            try {
                Command.all.Find("review").Use(null, "clear");
                MessageBox.Show("Review queue has been cleared!");
                reviewlist_update();
            }
            catch ( Exception ex ) { Server.ErrorLog(ex); }
        }

        private void txtGrpMOTD_TextChanged(object sender, EventArgs e) {
            if ( txtGrpMOTD.Text != null ) storedRanks[listRanks.SelectedIndex].MOTD = txtGrpMOTD.Text;
        }

        private void tabControl2_Click(object sender, EventArgs e) {

        }

        private void txechx_CheckedChanged(object sender, EventArgs e) {
            Server.UseTextures = txechx.Checked;
        }
        
        private void lstCommands_SelectedIndexChanged ( object sender, EventArgs e ) {
            btnUnload.Enabled = lstCommands.SelectedIndex != -1;
        }
    }
}