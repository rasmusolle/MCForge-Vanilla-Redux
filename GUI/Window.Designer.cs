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
using System.Windows.Forms;

namespace MCForge.Gui
{
    public partial class Window
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message msg)
        {
            /*const int WM_SIZE = 0x0005;
            const int SIZE_MINIMIZED = 1;

            if ((msg.Msg == WM_SIZE) && ((int)msg.WParam == SIZE_MINIMIZED) && (Window.Minimize != null))
            {
                this.Window_Minimize(this, EventArgs.Empty);
            }*/

            base.WndProc(ref msg);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Window));
            this.mapsStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.physicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.physicsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.finiteModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.randomFlowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.edgeWaterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.growingGrassToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeGrowingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leafDecayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autpPhysicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unloadToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.loadOngotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miscToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animalAIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.survivalDeathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.killerBlocksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.instantBuildingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rPChatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gunsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actiondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playerStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.whoisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.banToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.voiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clonesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.promoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.demoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iconContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openConsole = new System.Windows.Forms.ToolStripMenuItem();
            this.shutdownServer = new System.Windows.Forms.ToolStripMenuItem();
            this.restartServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnProperties = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.Restart = new System.Windows.Forms.Button();
            this.txtLogMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.nightModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dateStampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoScrollToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.copySelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.gBCommands = new System.Windows.Forms.GroupBox();
            this.txtCommandsUsed = new MCForge.Gui.AutoScrollTextBox();
            this.gBChat = new System.Windows.Forms.GroupBox();
            this.txtLog = new MCForge.Gui.Components.ColoredTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCommands = new System.Windows.Forms.TextBox();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.dgvPlayers = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.txtSystem = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtChangelog = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.txtErrors = new System.Windows.Forms.TextBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.LogsTxtBox = new System.Windows.Forms.RichTextBox();
            this.tabPage10 = new System.Windows.Forms.TabPage();
            this.grpRCUsers = new System.Windows.Forms.GroupBox();
            this.liRCUsers = new System.Windows.Forms.ListBox();
            this.grpRCSettings = new System.Windows.Forms.GroupBox();
            this.grpConnectedRCs = new System.Windows.Forms.GroupBox();
            this.dgvMaps = new System.Windows.Forms.DataGridView();
            this.button_saveall = new System.Windows.Forms.Button();
            this.killphysics_button = new System.Windows.Forms.Button();
            this.Unloadempty_button = new System.Windows.Forms.Button();
            this.mapsStrip.SuspendLayout();
            this.playerStrip.SuspendLayout();
            this.iconContext.SuspendLayout();
            this.txtLogMenuStrip.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.gBCommands.SuspendLayout();
            this.gBChat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPlayers)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMaps)).BeginInit();
            this.SuspendLayout();
            // 
            // mapsStrip
            // 
            this.mapsStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.physicsToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.actiondToolStripMenuItem,
            this.toolStripSeparator1,
            this.infoToolStripMenuItem});
            this.mapsStrip.Name = "mapsStrip";
            this.mapsStrip.Size = new System.Drawing.Size(144, 98);
            // 
            // physicsToolStripMenuItem
            // 
            this.physicsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5,
            this.toolStripMenuItem6,
            this.toolStripMenuItem7});
            this.physicsToolStripMenuItem.Name = "physicsToolStripMenuItem";
            this.physicsToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.physicsToolStripMenuItem.Text = "Physics Level";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(135, 22);
            this.toolStripMenuItem2.Text = "Off";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click_1);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(135, 22);
            this.toolStripMenuItem3.Text = "Normal";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click_1);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(135, 22);
            this.toolStripMenuItem4.Text = "Advanced";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click_1);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(135, 22);
            this.toolStripMenuItem5.Text = "Hardcore";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.toolStripMenuItem5_Click_1);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(135, 22);
            this.toolStripMenuItem6.Text = "Instant";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.toolStripMenuItem6_Click_1);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(135, 22);
            this.toolStripMenuItem7.Text = "Doors-Only";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.toolStripMenuItem7_Click_1);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.physicsToolStripMenuItem1,
            this.loadingToolStripMenuItem,
            this.miscToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // physicsToolStripMenuItem1
            // 
            this.physicsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.finiteModeToolStripMenuItem,
            this.randomFlowToolStripMenuItem,
            this.edgeWaterToolStripMenuItem,
            this.growingGrassToolStripMenuItem,
            this.treeGrowingToolStripMenuItem,
            this.leafDecayToolStripMenuItem,
            this.autpPhysicsToolStripMenuItem});
            this.physicsToolStripMenuItem1.Name = "physicsToolStripMenuItem1";
            this.physicsToolStripMenuItem1.Size = new System.Drawing.Size(117, 22);
            this.physicsToolStripMenuItem1.Text = "Physics";
            // 
            // finiteModeToolStripMenuItem
            // 
            this.finiteModeToolStripMenuItem.Name = "finiteModeToolStripMenuItem";
            this.finiteModeToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.finiteModeToolStripMenuItem.Text = "Finite Mode";
            this.finiteModeToolStripMenuItem.Click += new System.EventHandler(this.finiteModeToolStripMenuItem_Click);
            // 
            // randomFlowToolStripMenuItem
            // 
            this.randomFlowToolStripMenuItem.Name = "randomFlowToolStripMenuItem";
            this.randomFlowToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.randomFlowToolStripMenuItem.Text = "Random Flow";
            this.randomFlowToolStripMenuItem.Click += new System.EventHandler(this.randomFlowToolStripMenuItem_Click);
            // 
            // edgeWaterToolStripMenuItem
            // 
            this.edgeWaterToolStripMenuItem.Name = "edgeWaterToolStripMenuItem";
            this.edgeWaterToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.edgeWaterToolStripMenuItem.Text = "Edge Water";
            this.edgeWaterToolStripMenuItem.Click += new System.EventHandler(this.edgeWaterToolStripMenuItem_Click);
            // 
            // growingGrassToolStripMenuItem
            // 
            this.growingGrassToolStripMenuItem.Name = "growingGrassToolStripMenuItem";
            this.growingGrassToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.growingGrassToolStripMenuItem.Text = "Grass Growing";
            this.growingGrassToolStripMenuItem.Click += new System.EventHandler(this.growingGrassToolStripMenuItem_Click);
            // 
            // treeGrowingToolStripMenuItem
            // 
            this.treeGrowingToolStripMenuItem.Name = "treeGrowingToolStripMenuItem";
            this.treeGrowingToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.treeGrowingToolStripMenuItem.Text = "Tree Growing";
            this.treeGrowingToolStripMenuItem.Click += new System.EventHandler(this.treeGrowingToolStripMenuItem_Click);
            // 
            // leafDecayToolStripMenuItem
            // 
            this.leafDecayToolStripMenuItem.Name = "leafDecayToolStripMenuItem";
            this.leafDecayToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.leafDecayToolStripMenuItem.Text = "Leaf Decay";
            this.leafDecayToolStripMenuItem.Click += new System.EventHandler(this.leafDecayToolStripMenuItem_Click);
            // 
            // autpPhysicsToolStripMenuItem
            // 
            this.autpPhysicsToolStripMenuItem.Name = "autpPhysicsToolStripMenuItem";
            this.autpPhysicsToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.autpPhysicsToolStripMenuItem.Text = "Auto Physics";
            this.autpPhysicsToolStripMenuItem.Click += new System.EventHandler(this.autpPhysicsToolStripMenuItem_Click);
            // 
            // loadingToolStripMenuItem
            // 
            this.loadingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.unloadToolStripMenuItem1,
            this.loadOngotoToolStripMenuItem});
            this.loadingToolStripMenuItem.Name = "loadingToolStripMenuItem";
            this.loadingToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.loadingToolStripMenuItem.Text = "Loading";
            // 
            // unloadToolStripMenuItem1
            // 
            this.unloadToolStripMenuItem1.Name = "unloadToolStripMenuItem1";
            this.unloadToolStripMenuItem1.Size = new System.Drawing.Size(150, 22);
            this.unloadToolStripMenuItem1.Text = "Auto Unload";
            this.unloadToolStripMenuItem1.Click += new System.EventHandler(this.unloadToolStripMenuItem1_Click);
            // 
            // loadOngotoToolStripMenuItem
            // 
            this.loadOngotoToolStripMenuItem.Name = "loadOngotoToolStripMenuItem";
            this.loadOngotoToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.loadOngotoToolStripMenuItem.Text = "Load on /goto";
            this.loadOngotoToolStripMenuItem.Click += new System.EventHandler(this.loadOngotoToolStripMenuItem_Click);
            // 
            // miscToolStripMenuItem
            // 
            this.miscToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.animalAIToolStripMenuItem,
            this.survivalDeathToolStripMenuItem,
            this.killerBlocksToolStripMenuItem,
            this.instantBuildingToolStripMenuItem,
            this.rPChatToolStripMenuItem,
            this.gunsToolStripMenuItem});
            this.miscToolStripMenuItem.Name = "miscToolStripMenuItem";
            this.miscToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.miscToolStripMenuItem.Text = "Misc";
            // 
            // animalAIToolStripMenuItem
            // 
            this.animalAIToolStripMenuItem.Name = "animalAIToolStripMenuItem";
            this.animalAIToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.animalAIToolStripMenuItem.Text = "Animal AI";
            this.animalAIToolStripMenuItem.Click += new System.EventHandler(this.animalAIToolStripMenuItem_Click);
            // 
            // survivalDeathToolStripMenuItem
            // 
            this.survivalDeathToolStripMenuItem.Name = "survivalDeathToolStripMenuItem";
            this.survivalDeathToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.survivalDeathToolStripMenuItem.Text = "Survival Death";
            this.survivalDeathToolStripMenuItem.Click += new System.EventHandler(this.survivalDeathToolStripMenuItem_Click);
            // 
            // killerBlocksToolStripMenuItem
            // 
            this.killerBlocksToolStripMenuItem.Name = "killerBlocksToolStripMenuItem";
            this.killerBlocksToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.killerBlocksToolStripMenuItem.Text = "Killer Blocks";
            this.killerBlocksToolStripMenuItem.Click += new System.EventHandler(this.killerBlocksToolStripMenuItem_Click);
            // 
            // instantBuildingToolStripMenuItem
            // 
            this.instantBuildingToolStripMenuItem.Name = "instantBuildingToolStripMenuItem";
            this.instantBuildingToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.instantBuildingToolStripMenuItem.Text = "Instant Building";
            this.instantBuildingToolStripMenuItem.Click += new System.EventHandler(this.instantBuildingToolStripMenuItem_Click);
            // 
            // rPChatToolStripMenuItem
            // 
            this.rPChatToolStripMenuItem.Name = "rPChatToolStripMenuItem";
            this.rPChatToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.rPChatToolStripMenuItem.Text = "RP Chat";
            this.rPChatToolStripMenuItem.Click += new System.EventHandler(this.rPChatToolStripMenuItem_Click);
            // 
            // gunsToolStripMenuItem
            // 
            this.gunsToolStripMenuItem.Name = "gunsToolStripMenuItem";
            this.gunsToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.gunsToolStripMenuItem.Text = "Guns";
            this.gunsToolStripMenuItem.Click += new System.EventHandler(this.gunsToolStripMenuItem_Click);
            // 
            // actiondToolStripMenuItem
            // 
            this.actiondToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.reloadToolStripMenuItem,
            this.unloadToolStripMenuItem,
            this.moveAllToolStripMenuItem});
            this.actiondToolStripMenuItem.Name = "actiondToolStripMenuItem";
            this.actiondToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.actiondToolStripMenuItem.Text = "Actions";
            this.actiondToolStripMenuItem.Click += new System.EventHandler(this.actiondToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click_1);
            // 
            // reloadToolStripMenuItem
            // 
            this.reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            this.reloadToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.reloadToolStripMenuItem.Text = "Reload";
            this.reloadToolStripMenuItem.Click += new System.EventHandler(this.reloadToolStripMenuItem_Click);
            // 
            // unloadToolStripMenuItem
            // 
            this.unloadToolStripMenuItem.Name = "unloadToolStripMenuItem";
            this.unloadToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.unloadToolStripMenuItem.Text = "Unload";
            this.unloadToolStripMenuItem.Click += new System.EventHandler(this.unloadToolStripMenuItem_Click_1);
            // 
            // moveAllToolStripMenuItem
            // 
            this.moveAllToolStripMenuItem.Name = "moveAllToolStripMenuItem";
            this.moveAllToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.moveAllToolStripMenuItem.Text = "Move All";
            this.moveAllToolStripMenuItem.Click += new System.EventHandler(this.moveAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(140, 6);
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.infoToolStripMenuItem.Text = "Info";
            this.infoToolStripMenuItem.Click += new System.EventHandler(this.infoToolStripMenuItem_Click);
            // 
            // playerStrip
            // 
            this.playerStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.whoisToolStripMenuItem,
            this.kickToolStripMenuItem,
            this.banToolStripMenuItem,
            this.voiceToolStripMenuItem,
            this.clonesToolStripMenuItem,
            this.promoteToolStripMenuItem,
            this.demoteToolStripMenuItem});
            this.playerStrip.Name = "playerStrip";
            this.playerStrip.Size = new System.Drawing.Size(121, 158);
            // 
            // whoisToolStripMenuItem
            // 
            this.whoisToolStripMenuItem.Name = "whoisToolStripMenuItem";
            this.whoisToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.whoisToolStripMenuItem.Text = "Whois";
            this.whoisToolStripMenuItem.Click += new System.EventHandler(this.whoisToolStripMenuItem_Click);
            // 
            // kickToolStripMenuItem
            // 
            this.kickToolStripMenuItem.Name = "kickToolStripMenuItem";
            this.kickToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.kickToolStripMenuItem.Text = "Kick";
            this.kickToolStripMenuItem.Click += new System.EventHandler(this.kickToolStripMenuItem_Click);
            // 
            // banToolStripMenuItem
            // 
            this.banToolStripMenuItem.Name = "banToolStripMenuItem";
            this.banToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.banToolStripMenuItem.Text = "Ban";
            this.banToolStripMenuItem.Click += new System.EventHandler(this.banToolStripMenuItem_Click);
            // 
            // voiceToolStripMenuItem
            // 
            this.voiceToolStripMenuItem.Name = "voiceToolStripMenuItem";
            this.voiceToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.voiceToolStripMenuItem.Text = "Voice";
            this.voiceToolStripMenuItem.Click += new System.EventHandler(this.voiceToolStripMenuItem_Click);
            // 
            // clonesToolStripMenuItem
            // 
            this.clonesToolStripMenuItem.Name = "clonesToolStripMenuItem";
            this.clonesToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.clonesToolStripMenuItem.Text = "Clones";
            this.clonesToolStripMenuItem.Click += new System.EventHandler(this.clonesToolStripMenuItem_Click);
            // 
            // promoteToolStripMenuItem
            // 
            this.promoteToolStripMenuItem.Name = "promoteToolStripMenuItem";
            this.promoteToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.promoteToolStripMenuItem.Text = "Promote";
            this.promoteToolStripMenuItem.Click += new System.EventHandler(this.promoteToolStripMenuItem_Click);
            // 
            // demoteToolStripMenuItem
            // 
            this.demoteToolStripMenuItem.Name = "demoteToolStripMenuItem";
            this.demoteToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.demoteToolStripMenuItem.Text = "Demote";
            this.demoteToolStripMenuItem.Click += new System.EventHandler(this.demoteToolStripMenuItem_Click);
            // 
            // iconContext
            // 
            this.iconContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openConsole,
            this.shutdownServer,
            this.restartServerToolStripMenuItem});
            this.iconContext.Name = "iconContext";
            this.iconContext.Size = new System.Drawing.Size(164, 70);
            // 
            // openConsole
            // 
            this.openConsole.Name = "openConsole";
            this.openConsole.Size = new System.Drawing.Size(163, 22);
            this.openConsole.Text = "Open Console";
            this.openConsole.Click += new System.EventHandler(this.openConsole_Click);
            // 
            // shutdownServer
            // 
            this.shutdownServer.Name = "shutdownServer";
            this.shutdownServer.Size = new System.Drawing.Size(163, 22);
            this.shutdownServer.Text = "Shutdown Server";
            this.shutdownServer.Click += new System.EventHandler(this.shutdownServer_Click);
            // 
            // restartServerToolStripMenuItem
            // 
            this.restartServerToolStripMenuItem.Name = "restartServerToolStripMenuItem";
            this.restartServerToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.restartServerToolStripMenuItem.Text = "Restart Server";
            this.restartServerToolStripMenuItem.Click += new System.EventHandler(this.restartServerToolStripMenuItem_Click);
            // 
            // btnProperties
            // 
            this.btnProperties.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnProperties.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProperties.Location = new System.Drawing.Point(536, 5);
            this.btnProperties.Name = "btnProperties";
            this.btnProperties.Size = new System.Drawing.Size(92, 23);
            this.btnProperties.TabIndex = 34;
            this.btnProperties.Text = "Properties";
            this.btnProperties.UseVisualStyleBackColor = true;
            this.btnProperties.Click += new System.EventHandler(this.btnProperties_Click_1);
            // 
            // btnClose
            // 
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(747, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 23);
            this.btnClose.TabIndex = 35;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click_1);
            // 
            // Restart
            // 
            this.Restart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Restart.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Restart.Location = new System.Drawing.Point(634, 5);
            this.Restart.Name = "Restart";
            this.Restart.Size = new System.Drawing.Size(107, 23);
            this.Restart.TabIndex = 36;
            this.Restart.Text = "Restart";
            this.Restart.UseVisualStyleBackColor = true;
            this.Restart.Click += new System.EventHandler(this.Restart_Click);
            // 
            // txtLogMenuStrip
            // 
            this.txtLogMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nightModeToolStripMenuItem,
            this.colorsToolStripMenuItem,
            this.dateStampToolStripMenuItem,
            this.autoScrollToolStripMenuItem,
            this.toolStripSeparator2,
            this.copySelectedToolStripMenuItem,
            this.copyAllToolStripMenuItem,
            this.toolStripSeparator3,
            this.clearToolStripMenuItem});
            this.txtLogMenuStrip.Name = "txtLogMenuStrip";
            this.txtLogMenuStrip.Size = new System.Drawing.Size(150, 170);
            // 
            // nightModeToolStripMenuItem
            // 
            this.nightModeToolStripMenuItem.Name = "nightModeToolStripMenuItem";
            this.nightModeToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.nightModeToolStripMenuItem.Text = "Night Theme";
            this.nightModeToolStripMenuItem.Click += new System.EventHandler(this.nightModeToolStripMenuItem_Click_1);
            // 
            // colorsToolStripMenuItem
            // 
            this.colorsToolStripMenuItem.Checked = true;
            this.colorsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.colorsToolStripMenuItem.Name = "colorsToolStripMenuItem";
            this.colorsToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.colorsToolStripMenuItem.Text = "Colors";
            this.colorsToolStripMenuItem.Click += new System.EventHandler(this.colorsToolStripMenuItem_Click_1);
            // 
            // dateStampToolStripMenuItem
            // 
            this.dateStampToolStripMenuItem.Checked = true;
            this.dateStampToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dateStampToolStripMenuItem.Name = "dateStampToolStripMenuItem";
            this.dateStampToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.dateStampToolStripMenuItem.Text = "Date Stamp";
            this.dateStampToolStripMenuItem.Click += new System.EventHandler(this.dateStampToolStripMenuItem_Click);
            // 
            // autoScrollToolStripMenuItem
            // 
            this.autoScrollToolStripMenuItem.Checked = true;
            this.autoScrollToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoScrollToolStripMenuItem.Name = "autoScrollToolStripMenuItem";
            this.autoScrollToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.autoScrollToolStripMenuItem.Text = "Auto Scroll";
            this.autoScrollToolStripMenuItem.Click += new System.EventHandler(this.autoScrollToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(146, 6);
            // 
            // copySelectedToolStripMenuItem
            // 
            this.copySelectedToolStripMenuItem.Name = "copySelectedToolStripMenuItem";
            this.copySelectedToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.copySelectedToolStripMenuItem.Text = "Copy Selected";
            this.copySelectedToolStripMenuItem.Click += new System.EventHandler(this.copySelectedToolStripMenuItem_Click);
            // 
            // copyAllToolStripMenuItem
            // 
            this.copyAllToolStripMenuItem.Name = "copyAllToolStripMenuItem";
            this.copyAllToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.copyAllToolStripMenuItem.Text = "Copy All";
            this.copyAllToolStripMenuItem.Click += new System.EventHandler(this.copyAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(146, 6);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Transparent;
            this.tabPage1.Controls.Add(this.Unloadempty_button);
            this.tabPage1.Controls.Add(this.killphysics_button);
            this.tabPage1.Controls.Add(this.button_saveall);
            this.tabPage1.Controls.Add(this.gBCommands);
            this.tabPage1.Controls.Add(this.dgvMaps);
            this.tabPage1.Controls.Add(this.gBChat);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.txtCommands);
            this.tabPage1.Controls.Add(this.txtInput);
            this.tabPage1.Controls.Add(this.txtUrl);
            this.tabPage1.Controls.Add(this.dgvPlayers);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(826, 491);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Main";
            // 
            // gBCommands
            // 
            this.gBCommands.Controls.Add(this.txtCommandsUsed);
            this.gBCommands.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gBCommands.Location = new System.Drawing.Point(13, 326);
            this.gBCommands.Name = "gBCommands";
            this.gBCommands.Size = new System.Drawing.Size(493, 123);
            this.gBCommands.TabIndex = 34;
            this.gBCommands.TabStop = false;
            this.gBCommands.Text = "Commands";
            // 
            // txtCommandsUsed
            // 
            this.txtCommandsUsed.BackColor = System.Drawing.Color.White;
            this.txtCommandsUsed.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtCommandsUsed.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommandsUsed.Location = new System.Drawing.Point(9, 16);
            this.txtCommandsUsed.Multiline = true;
            this.txtCommandsUsed.Name = "txtCommandsUsed";
            this.txtCommandsUsed.ReadOnly = true;
            this.txtCommandsUsed.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCommandsUsed.Size = new System.Drawing.Size(478, 100);
            this.txtCommandsUsed.TabIndex = 0;
            // 
            // gBChat
            // 
            this.gBChat.Controls.Add(this.txtLog);
            this.gBChat.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gBChat.Location = new System.Drawing.Point(13, 34);
            this.gBChat.Name = "gBChat";
            this.gBChat.Size = new System.Drawing.Size(493, 286);
            this.gBChat.TabIndex = 32;
            this.gBChat.TabStop = false;
            this.gBChat.Text = "Chat";
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.SystemColors.Window;
            this.txtLog.ContextMenuStrip = this.txtLogMenuStrip;
            this.txtLog.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(6, 30);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.txtLog.Size = new System.Drawing.Size(480, 250);
            this.txtLog.TabIndex = 0;
            this.txtLog.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(512, 462);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Command:";
            // 
            // txtCommands
            // 
            this.txtCommands.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommands.Location = new System.Drawing.Point(575, 459);
            this.txtCommands.Name = "txtCommands";
            this.txtCommands.Size = new System.Drawing.Size(234, 21);
            this.txtCommands.TabIndex = 28;
            this.txtCommands.Text = "/";
            this.txtCommands.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCommands_KeyDown);
            // 
            // txtInput
            // 
            this.txtInput.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInput.Location = new System.Drawing.Point(57, 459);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(449, 21);
            this.txtInput.TabIndex = 27;
            this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
            // 
            // txtUrl
            // 
            this.txtUrl.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtUrl.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUrl.Location = new System.Drawing.Point(13, 7);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.ReadOnly = true;
            this.txtUrl.Size = new System.Drawing.Size(493, 21);
            this.txtUrl.TabIndex = 25;
            this.txtUrl.DoubleClick += new System.EventHandler(this.txtUrl_DoubleClick);
            // 
            // dgvPlayers
            // 
            this.dgvPlayers.AllowUserToAddRows = false;
            this.dgvPlayers.AllowUserToDeleteRows = false;
            this.dgvPlayers.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvPlayers.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvPlayers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPlayers.ContextMenuStrip = this.playerStrip;
            this.dgvPlayers.Location = new System.Drawing.Point(512, 7);
            this.dgvPlayers.MultiSelect = false;
            this.dgvPlayers.Name = "dgvPlayers";
            this.dgvPlayers.ReadOnly = true;
            this.dgvPlayers.RowHeadersVisible = false;
            this.dgvPlayers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPlayers.Size = new System.Drawing.Size(297, 250);
            this.dgvPlayers.TabIndex = 37;
            this.dgvPlayers.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dgvPlayers_RowPrePaint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(19, 462);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Chat:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabControl1.Font = new System.Drawing.Font("Calibri", 8.25F);
            this.tabControl1.Location = new System.Drawing.Point(1, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(834, 517);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.Color.Transparent;
            this.tabPage4.Controls.Add(this.txtSystem);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(826, 491);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "System";
            // 
            // txtSystem
            // 
            this.txtSystem.BackColor = System.Drawing.Color.White;
            this.txtSystem.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtSystem.Location = new System.Drawing.Point(7, 6);
            this.txtSystem.Multiline = true;
            this.txtSystem.Name = "txtSystem";
            this.txtSystem.ReadOnly = true;
            this.txtSystem.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSystem.Size = new System.Drawing.Size(754, 471);
            this.txtSystem.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.Transparent;
            this.tabPage2.Controls.Add(this.txtChangelog);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(826, 491);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Changelog";
            // 
            // txtChangelog
            // 
            this.txtChangelog.BackColor = System.Drawing.Color.White;
            this.txtChangelog.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtChangelog.Location = new System.Drawing.Point(7, 6);
            this.txtChangelog.Multiline = true;
            this.txtChangelog.Name = "txtChangelog";
            this.txtChangelog.ReadOnly = true;
            this.txtChangelog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtChangelog.Size = new System.Drawing.Size(751, 471);
            this.txtChangelog.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.Transparent;
            this.tabPage3.Controls.Add(this.txtErrors);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(826, 491);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Errors";
            // 
            // txtErrors
            // 
            this.txtErrors.BackColor = System.Drawing.Color.White;
            this.txtErrors.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtErrors.Location = new System.Drawing.Point(7, 6);
            this.txtErrors.Multiline = true;
            this.txtErrors.Name = "txtErrors";
            this.txtErrors.ReadOnly = true;
            this.txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtErrors.Size = new System.Drawing.Size(754, 471);
            this.txtErrors.TabIndex = 1;
            // 
            // tabPage5
            // 
            this.tabPage5.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage5.Controls.Add(this.label3);
            this.tabPage5.Controls.Add(this.dateTimePicker1);
            this.tabPage5.Controls.Add(this.LogsTxtBox);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(826, 491);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Logs";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "View logs from:";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(92, 5);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker1.TabIndex = 2;
            this.dateTimePicker1.Value = new System.DateTime(2011, 7, 20, 18, 31, 50, 0);
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.DatePicker1_ValueChanged);
            // 
            // LogsTxtBox
            // 
            this.LogsTxtBox.BackColor = System.Drawing.SystemColors.Window;
            this.LogsTxtBox.Location = new System.Drawing.Point(3, 32);
            this.LogsTxtBox.Name = "LogsTxtBox";
            this.LogsTxtBox.ReadOnly = true;
            this.LogsTxtBox.Size = new System.Drawing.Size(755, 453);
            this.LogsTxtBox.TabIndex = 0;
            this.LogsTxtBox.Text = "";
            // 
            // tabPage10
            // 
            this.tabPage10.Location = new System.Drawing.Point(0, 0);
            this.tabPage10.Name = "tabPage10";
            this.tabPage10.Size = new System.Drawing.Size(200, 100);
            this.tabPage10.TabIndex = 0;
            // 
            // grpRCUsers
            // 
            this.grpRCUsers.Location = new System.Drawing.Point(0, 0);
            this.grpRCUsers.Name = "grpRCUsers";
            this.grpRCUsers.Size = new System.Drawing.Size(200, 100);
            this.grpRCUsers.TabIndex = 0;
            this.grpRCUsers.TabStop = false;
            // 
            // liRCUsers
            // 
            this.liRCUsers.Location = new System.Drawing.Point(0, 0);
            this.liRCUsers.Name = "liRCUsers";
            this.liRCUsers.Size = new System.Drawing.Size(120, 96);
            this.liRCUsers.TabIndex = 0;
            // 
            // grpRCSettings
            // 
            this.grpRCSettings.Location = new System.Drawing.Point(0, 0);
            this.grpRCSettings.Name = "grpRCSettings";
            this.grpRCSettings.Size = new System.Drawing.Size(200, 100);
            this.grpRCSettings.TabIndex = 0;
            this.grpRCSettings.TabStop = false;
            // 
            // grpConnectedRCs
            // 
            this.grpConnectedRCs.Location = new System.Drawing.Point(0, 0);
            this.grpConnectedRCs.Name = "grpConnectedRCs";
            this.grpConnectedRCs.Size = new System.Drawing.Size(200, 100);
            this.grpConnectedRCs.TabIndex = 0;
            this.grpConnectedRCs.TabStop = false;
            // 
            // dgvMaps
            // 
            this.dgvMaps.AllowUserToAddRows = false;
            this.dgvMaps.AllowUserToDeleteRows = false;
            this.dgvMaps.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvMaps.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvMaps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMaps.ContextMenuStrip = this.mapsStrip;
            this.dgvMaps.Location = new System.Drawing.Point(512, 292);
            this.dgvMaps.MultiSelect = false;
            this.dgvMaps.Name = "dgvMaps";
            this.dgvMaps.ReadOnly = true;
            this.dgvMaps.RowHeadersVisible = false;
            this.dgvMaps.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMaps.Size = new System.Drawing.Size(297, 150);
            this.dgvMaps.TabIndex = 38;
            this.dgvMaps.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMaps_CellContentClick);
            // 
            // button_saveall
            // 
            this.button_saveall.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_saveall.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_saveall.Location = new System.Drawing.Point(513, 263);
            this.button_saveall.Name = "button_saveall";
            this.button_saveall.Size = new System.Drawing.Size(63, 23);
            this.button_saveall.TabIndex = 39;
            this.button_saveall.Text = "Save All";
            this.button_saveall.UseVisualStyleBackColor = true;
            this.button_saveall.Click += new System.EventHandler(this.button_saveall_Click);
            // 
            // killphysics_button
            // 
            this.killphysics_button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.killphysics_button.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.killphysics_button.Location = new System.Drawing.Point(582, 263);
            this.killphysics_button.Name = "killphysics_button";
            this.killphysics_button.Size = new System.Drawing.Size(88, 23);
            this.killphysics_button.TabIndex = 40;
            this.killphysics_button.Text = "Kill All Physics";
            this.killphysics_button.UseVisualStyleBackColor = true;
            // 
            // Unloadempty_button
            // 
            this.Unloadempty_button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Unloadempty_button.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Unloadempty_button.Location = new System.Drawing.Point(676, 263);
            this.Unloadempty_button.Name = "Unloadempty_button";
            this.Unloadempty_button.Size = new System.Drawing.Size(81, 23);
            this.Unloadempty_button.TabIndex = 41;
            this.Unloadempty_button.Text = "Unload Empty";
            this.Unloadempty_button.UseVisualStyleBackColor = true;
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 525);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnProperties);
            this.Controls.Add(this.Restart);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Window";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Window_FormClosing);
            this.Load += new System.EventHandler(this.Window_Load);
            this.Resize += new System.EventHandler(this.Window_Resize);
            this.mapsStrip.ResumeLayout(false);
            this.playerStrip.ResumeLayout(false);
            this.iconContext.ResumeLayout(false);
            this.txtLogMenuStrip.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.gBCommands.ResumeLayout(false);
            this.gBCommands.PerformLayout();
            this.gBChat.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPlayers)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMaps)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion


        private TabPage tabPage10;
        private Button btnClose;
        private ContextMenuStrip iconContext;
        private ToolStripMenuItem openConsole;
        private ToolStripMenuItem shutdownServer;
        private ContextMenuStrip playerStrip;
        private ToolStripMenuItem whoisToolStripMenuItem;
        private ToolStripMenuItem kickToolStripMenuItem;
        private ToolStripMenuItem banToolStripMenuItem;
        private ToolStripMenuItem voiceToolStripMenuItem;
        private ContextMenuStrip mapsStrip;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem finiteModeToolStripMenuItem;
        private ToolStripMenuItem animalAIToolStripMenuItem;
        private ToolStripMenuItem edgeWaterToolStripMenuItem;
        private ToolStripMenuItem growingGrassToolStripMenuItem;
        private ToolStripMenuItem survivalDeathToolStripMenuItem;
        private ToolStripMenuItem killerBlocksToolStripMenuItem;
        private ToolStripMenuItem rPChatToolStripMenuItem;
        private ToolStripMenuItem clonesToolStripMenuItem;
        private Button Restart;
        private ToolStripMenuItem restartServerToolStripMenuItem;
        private ToolStripMenuItem promoteToolStripMenuItem;
        private ToolStripMenuItem demoteToolStripMenuItem;
        private ToolStripMenuItem unloadToolStripMenuItem1;
        private ToolStripMenuItem loadOngotoToolStripMenuItem;
        private ToolStripMenuItem autpPhysicsToolStripMenuItem;
        private ToolStripMenuItem instantBuildingToolStripMenuItem;
        private ToolStripMenuItem gunsToolStripMenuItem;
        private ToolStripMenuItem infoToolStripMenuItem;
        private ToolStripMenuItem actiondToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem unloadToolStripMenuItem;
        private ToolStripMenuItem moveAllToolStripMenuItem;
        private ToolStripMenuItem reloadToolStripMenuItem;
        private ToolStripMenuItem randomFlowToolStripMenuItem;
        private ToolStripMenuItem leafDecayToolStripMenuItem;
        private ToolStripMenuItem treeGrowingToolStripMenuItem;
        private ToolStripMenuItem physicsToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripMenuItem toolStripMenuItem6;
        private ToolStripMenuItem toolStripMenuItem7;
        private ToolStripMenuItem physicsToolStripMenuItem1;
        private ToolStripMenuItem loadingToolStripMenuItem;
        private ToolStripMenuItem miscToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ContextMenuStrip txtLogMenuStrip;
        private ToolStripMenuItem nightModeToolStripMenuItem;
        private ToolStripMenuItem colorsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem copySelectedToolStripMenuItem;
        private ToolStripMenuItem copyAllToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem clearToolStripMenuItem;
        private ToolStripMenuItem dateStampToolStripMenuItem;
        private ToolStripMenuItem autoScrollToolStripMenuItem;
        private Button btnProperties;
        private GroupBox grpRCUsers;
        private GroupBox grpRCSettings;
        private GroupBox grpConnectedRCs;
        public ListBox liRCUsers;
        private TabPage tabPage1;
        private GroupBox gBCommands;
        private AutoScrollTextBox txtCommandsUsed;
        private GroupBox gBChat;
        private Components.ColoredTextBox txtLog;
        private Label label2;
        private TextBox txtCommands;
        private TextBox txtInput;
        private TextBox txtUrl;
        private DataGridView dgvPlayers;
        private Label label1;
        private TabControl tabControl1;
        private TabPage tabPage4;
        private TextBox txtSystem;
        private TabPage tabPage2;
        private TextBox txtChangelog;
        private TabPage tabPage3;
        private TextBox txtErrors;
        private TabPage tabPage5;
        private Label label3;
        private DateTimePicker dateTimePicker1;
        internal RichTextBox LogsTxtBox;
        private Button Unloadempty_button;
        private Button killphysics_button;
        private Button button_saveall;
        private DataGridView dgvMaps;
    }
}