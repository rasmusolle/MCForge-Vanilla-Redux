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
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace MCSpleef.Gui.Utils {
	[StructLayout( LayoutKind.Sequential )]
	public struct Margins {

		private int _Left;
		private int _Right;
		private int _Top;
		private int _Bottom;

		public int Left { get { return _Left; } set { _Left = value; } }
		public int Right { get { return _Right; } set { _Right = value; } }
		public int Top { get { return _Top; } set { _Top = value; } }
		public int Bottom { get { return _Bottom; } set { _Bottom = value; } }

		public Margins( int left, int right, int top, int bottom ) {
			_Top = top;
			_Bottom = bottom;
			_Right = right;
			_Left = left;
		}

		public Margins( int allMargs ) : this( allMargs, allMargs, allMargs, allMargs ) { }
		public bool IsEmpty { get { return Left <= 0 && Right <= 0 && Top <= 0 && Bottom <= 0; } }

		public bool IsTouchingGlass( Point point ) {
			if (IsEmpty) { return true; }
			return ( point.X < _Left || point.X > _Right || point.Y < _Top || point.Y > _Bottom );
		}

		public static implicit operator RECT( Margins margs ) { return new RECT( margs.Left, margs.Top, margs.Right, margs.Bottom ); }
		public static implicit operator Padding( Margins margs ) { return new Padding( margs.Left, margs.Top, margs.Right, margs.Bottom );}
		public static implicit operator Rectangle( Margins margs ) { return new Rectangle( margs.Left, margs.Top, margs.Right, margs.Bottom ); }
		public static implicit operator Margins( RECT margs ) { return new Margins( margs.Left, margs.Right, margs.Top, margs.Bottom ); }
		public static implicit operator Margins( Rectangle rect ) { return new Margins( rect.X, rect.Width, rect.Y, rect.Height ); }
		public static implicit operator Margins( Padding margs ) { return new Margins( margs.Left, margs.Right, margs.Top, margs.Bottom ); }
	}
}
