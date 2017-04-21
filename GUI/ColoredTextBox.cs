/*
	Copyright 2012 MCForge (Modified for use with MCSpleef)
 
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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MCSpleef.Gui.Utils;
namespace MCSpleef.Gui.Components
{
	public partial class ColoredTextBox : RichTextBox
	{
		private string dateStamp { get { return "[" + DateTime.Now.ToString("T") + "] "; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="ColoredTextBox"/> class.
		/// </summary>
		public ColoredTextBox() : base() { InitializeComponent(); }

		/// <summary>
		/// Appends the log.
		/// </summary>
		/// <param name="text">The text to log.</param>
		public void AppendLog(string text, Color foreColor)
		{
			if (InvokeRequired) {
				Invoke((MethodInvoker)(() => AppendLog(text, foreColor)));
				return;
			}
			Append(dateStamp, Color.Gray, BackColor);

			if (!text.Contains('&') && !text.Contains('%')) {
				Append(text, foreColor, BackColor);
				ScrollToEnd();
				return;
			}

			string[] messagesSplit = text.Split(new[] { '%', '&' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < messagesSplit.Length; i++) {
				string split = messagesSplit[i];
				if (String.IsNullOrEmpty(split.Trim()))
					continue;
				Color? color = Utilities.GetDimColorFromChar(split[0]);
				Append(color != null ? split.Substring(1) : split, color ?? foreColor, BackColor);
			}
			ScrollToEnd();
		}

		public void AppendLog(string text) { AppendLog(text, ForeColor); ScrollToEnd(); }

		/// <summary>
		/// Appends the log.
		/// </summary>
		/// <param name="text">The text to log.</param>
		/// <param name="foreColor">Color of the foreground.</param>
		/// <param name="bgColor">Color of the background.</param>
		private void Append(string text, Color foreColor, Color bgColor)
		{
			if (InvokeRequired) {
				Invoke((MethodInvoker)(() => Append(text, foreColor, bgColor)));
				return;
			}

			SelectionStart = TextLength;
			SelectionLength = 0;
			SelectionColor = foreColor;
			SelectionBackColor = bgColor;
			AppendText(text);
			SelectionBackColor = BackColor;
			SelectionColor = ForeColor;

		}

		private void ColoredReader_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
			if (!e.LinkText.StartsWith("http://www.minecraft.net/classic/play/")) {
				if (MessageBox.Show("Never open links from people that you don't trust!", "Warning!!", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
					return;
			}

			try { Process.Start(e.LinkText); }
			catch { }

		}

		/// <summary>
		/// Scrolls to the end of the log
		/// </summary>
		public void ScrollToEnd()
		{
			if (InvokeRequired) {
				Invoke((MethodInvoker)ScrollToEnd);
				return;
			}
			Select(Text.Length - 1, 1);
			ScrollToCaret();
			Invalidate();
			Refresh();
		}

		#region Border Style

		private RECT _border;

		protected override void WndProc(ref Message m)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
				base.WndProc(ref m);
				return;
			}

			switch (m.Msg) {
				case Natives.WM_NCPAINT:
					RenderStyle(ref m);
					break;
				case Natives.WM_NCCALCSIZE:
					CalculateSize(ref m);
					break;
				case Natives.WM_THEMECHANGED:
					UpdateStyles();
					break;
				default:
					base.WndProc(ref m);
					break;
			}
		}

		private void CalculateSize(ref Message m)
		{
			base.WndProc(ref m);

			if (!Natives.CanRender())
				return;

			Natives.NCCALCSIZE_PARAMS par = new Natives.NCCALCSIZE_PARAMS();

			RECT windowRect;

			if (m.WParam == IntPtr.Zero) {
				windowRect = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
			} else {
				par = (Natives.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(Natives.NCCALCSIZE_PARAMS));
				windowRect = par.rgrc0;
			}

			RECT contentRect;
			IntPtr hDC = Natives.GetWindowDC(this.Handle);
			IntPtr hTheme = Natives.OpenThemeData(this.Handle, "EDIT");

			if (Natives.GetThemeBackgroundContentRect(hTheme, hDC, Natives.EP_EDITTEXT, Natives.ETS_NORMAL, ref windowRect, out contentRect) == Natives.S_OK) {
				contentRect.Inflate(-1, -1);
				this._border = new Margins(contentRect.Left - windowRect.Left,
																		contentRect.Top - windowRect.Top,
																		 windowRect.Right - contentRect.Right,
																		 windowRect.Bottom - contentRect.Bottom);


				if (m.WParam == IntPtr.Zero) {
					Marshal.StructureToPtr(contentRect, m.LParam, false);
				} else {
					par.rgrc0 = contentRect;
					Marshal.StructureToPtr(par, m.LParam, false);
				}

				m.Result = new IntPtr(Natives.WVR_REDRAW);
			}

			Natives.CloseThemeData(hTheme);
			Natives.ReleaseDC(this.Handle, hDC);

		}

		private void RenderStyle(ref Message m)
		{
			base.WndProc(ref m);

			if (!Natives.CanRender()) { return; }

			int partId = Natives.EP_EDITTEXT;

			int stateId;
			if (this.Enabled)
				stateId = this.ReadOnly ? Natives.ETS_READONLY : Natives.ETS_NORMAL;
			else
				stateId = Natives.ETS_DISABLED;

			RECT windowRect;
			Natives.GetWindowRect(this.Handle, out windowRect);
			windowRect.Right -= windowRect.Left;
			windowRect.Bottom -= windowRect.Top;
			windowRect.Top = 0;
			windowRect.Left = 0;

			IntPtr hDC = Natives.GetWindowDC(this.Handle);

			RECT clientRect = windowRect;
			clientRect.Left += this._border.Left;
			clientRect.Top += this._border.Top;
			clientRect.Right -= this._border.Right;
			clientRect.Bottom -= this._border.Bottom;

			Natives.ExcludeClipRect(hDC, clientRect.Left, clientRect.Top, clientRect.Right, clientRect.Bottom);

			IntPtr hTheme = Natives.OpenThemeData(this.Handle, "EDIT");

			if (Natives.IsThemeBackgroundPartiallyTransparent(hTheme, Natives.EP_EDITTEXT, Natives.ETS_NORMAL) != 0)
				Natives.DrawThemeParentBackground(this.Handle, hDC, ref windowRect);


			Natives.DrawThemeBackground(hTheme, hDC, partId, stateId, ref windowRect, IntPtr.Zero);
			Natives.CloseThemeData(hTheme);
			Natives.ReleaseDC(this.Handle, hDC);
			m.Result = IntPtr.Zero;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams p = base.CreateParams;

				if (Natives.CanRender() && (p.ExStyle & Natives.WS_EX_CLIENTEDGE) == Natives.WS_EX_CLIENTEDGE)
					p.ExStyle ^= Natives.WS_EX_CLIENTEDGE;

				return p;
			}
		}

		#endregion
	}
}