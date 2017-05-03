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
using System.Drawing;
namespace MCSpleef.Gui.Utils
{
	public struct RECT
	{
		private int left;
		private int top;
		private int right;
		private int bottom;

		public int Top { get { return top; } set { top = value; } }
		public int Right { get { return right; } set { right = value; } }
		public int Bottom { get { return bottom; } set { bottom = value; } }
		public int Left { get { return left; } set { left = value; } }

		public RECT(int left, int right, int top, int bottom) {
			this.top = top;
			this.bottom = bottom;
			this.right = right;
			this.left = left;
		}

		public int Height { get { return Bottom - Top + 1; } }
		public int Width { get { return Right - Left + 1; } }
		public Size Size { get { return new Size(Width, Height); } }
		public Point Location { get { return new Point(Left, Top); } }

		public static implicit operator Rectangle(RECT margs) { return new Margins(margs.Left, margs.Right, margs.Top, margs.Bottom); }
		public static implicit operator RECT(Rectangle margs) { return new Margins(margs.Left, margs.Right, margs.Top, margs.Bottom); }

		public void Inflate(int width, int height) {
			this.Left -= width; this.Top -= height; this.Right += width; this.Bottom += height;
		}
	}
}