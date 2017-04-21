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
using System.Windows.Forms;
namespace MCSpleef.Gui
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
			this.iconContext = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.openConsole = new System.Windows.Forms.ToolStripMenuItem();
			this.shutdownServer = new System.Windows.Forms.ToolStripMenuItem();
			this.restartServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.btnProperties = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.Restart = new System.Windows.Forms.Button();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.gBChat = new System.Windows.Forms.GroupBox();
			this.txtLog = new MCSpleef.Gui.Components.ColoredTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtCommands = new System.Windows.Forms.TextBox();
			this.txtInput = new System.Windows.Forms.TextBox();
			this.txtUrl = new System.Windows.Forms.TextBox();
			this.dgvPlayers = new System.Windows.Forms.DataGridView();
			this.label1 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.txtErrors = new System.Windows.Forms.TextBox();
			this.tabPage10 = new System.Windows.Forms.TabPage();
			this.grpRCUsers = new System.Windows.Forms.GroupBox();
			this.liRCUsers = new System.Windows.Forms.ListBox();
			this.grpRCSettings = new System.Windows.Forms.GroupBox();
			this.grpConnectedRCs = new System.Windows.Forms.GroupBox();
			this.iconContext.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.gBChat.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvPlayers)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.SuspendLayout();
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
			// tabPage1
			// 
			this.tabPage1.BackColor = System.Drawing.Color.Transparent;
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
			// gBChat
			// 
			this.gBChat.Controls.Add(this.txtLog);
			this.gBChat.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.gBChat.Location = new System.Drawing.Point(13, 34);
			this.gBChat.Name = "gBChat";
			this.gBChat.Size = new System.Drawing.Size(493, 415);
			this.gBChat.TabIndex = 32;
			this.gBChat.TabStop = false;
			this.gBChat.Text = "Chat";
			// 
			// txtLog
			// 
			this.txtLog.BackColor = System.Drawing.SystemColors.Window;
			this.txtLog.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtLog.Location = new System.Drawing.Point(6, 30);
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.txtLog.Size = new System.Drawing.Size(480, 379);
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
			this.dgvPlayers.Location = new System.Drawing.Point(512, 7);
			this.dgvPlayers.MultiSelect = false;
			this.dgvPlayers.Name = "dgvPlayers";
			this.dgvPlayers.ReadOnly = true;
			this.dgvPlayers.RowHeadersVisible = false;
			this.dgvPlayers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvPlayers.Size = new System.Drawing.Size(297, 442);
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
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Cursor = System.Windows.Forms.Cursors.Default;
			this.tabControl1.Font = new System.Drawing.Font("Calibri", 8.25F);
			this.tabControl1.Location = new System.Drawing.Point(1, 12);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(834, 517);
			this.tabControl1.TabIndex = 2;
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
			this.liRCUsers.Size = new System.Drawing.Size(120, 95);
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
			this.iconContext.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.gBChat.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvPlayers)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.tabPage3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion


		private TabPage tabPage10;
		private Button btnClose;
		private ContextMenuStrip iconContext;
		private ToolStripMenuItem openConsole;
		private ToolStripMenuItem shutdownServer;
		private Button Restart;
		private ToolStripMenuItem restartServerToolStripMenuItem;
		private Button btnProperties;
		private GroupBox grpRCUsers;
		private GroupBox grpRCSettings;
		private GroupBox grpConnectedRCs;
		public ListBox liRCUsers;
		private TabPage tabPage1;
		private GroupBox gBChat;
		private Components.ColoredTextBox txtLog;
		private Label label2;
		private TextBox txtCommands;
		private TextBox txtInput;
		private TextBox txtUrl;
		private DataGridView dgvPlayers;
		private Label label1;
		private TabControl tabControl1;
		private TabPage tabPage3;
		private TextBox txtErrors;
	}
}