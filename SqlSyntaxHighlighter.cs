/*
 * Copyright 2026 Justin Dearing
 *
 * This file is part of PlaneDisaster.NET.
 *
 * PlaneDisaster.NET is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; version 2 of the License.
 *
 * PlaneDisaster.NET is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with PlaneDisaster.NET; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using PlaneDisaster.Dba;

namespace PlaneDisaster
{
	/// <summary>
	/// Colors SQL in a <see cref="RichTextBox"/> as the user types. Highlighting waits for a short
	/// pause in typing, and keeps the caret, selection and scroll position where they were.
	/// </summary>
	public sealed class SqlSyntaxHighlighter : IDisposable
	{
		private const int WM_SETREDRAW = 0x000B;
		private const int EM_GETSCROLLPOS = 0x04DD;
		private const int EM_SETSCROLLPOS = 0x04DE;

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int X;
			public int Y;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref POINT lParam);

		private readonly RichTextBox _textBox;
		private readonly Timer _timer;
		private readonly Dictionary<SqlTokenType, Color> _colors = new Dictionary<SqlTokenType, Color>();
		private bool _highlighting;

		/// <summary>Starts highlighting <paramref name="textBox"/>.</summary>
		public SqlSyntaxHighlighter(RichTextBox textBox)
		{
			_textBox = textBox;
			_colors[SqlTokenType.Keyword] = Color.Blue;
			_colors[SqlTokenType.Function] = Color.DarkMagenta;
			_colors[SqlTokenType.String] = Color.DarkRed;
			_colors[SqlTokenType.Number] = Color.DarkCyan;
			_colors[SqlTokenType.Comment] = Color.Green;
			_colors[SqlTokenType.QuotedIdentifier] = Color.SaddleBrown;
			_colors[SqlTokenType.Parameter] = Color.DarkOrange;

			_timer = new Timer();
			_timer.Interval = 200;
			_timer.Tick += new EventHandler(OnTimerTick);
			_textBox.TextChanged += new EventHandler(OnTextChanged);
			_textBox.KeyDown += new KeyEventHandler(OnKeyDown);
		}


		/// <summary>The color for each token type. Change it, then call <see cref="Highlight"/>.</summary>
		public IDictionary<SqlTokenType, Color> Colors
		{
			get { return _colors; }
		}


		/// <summary>Recolors the whole text now.</summary>
		public void Highlight()
		{
			_timer.Stop();
			if (_highlighting || !_textBox.IsHandleCreated) {
				return;
			}
			_highlighting = true;
			int selectionStart = _textBox.SelectionStart;
			int selectionLength = _textBox.SelectionLength;
			POINT scroll = new POINT();
			SendMessage(_textBox.Handle, EM_GETSCROLLPOS, IntPtr.Zero, ref scroll);
			SendMessage(_textBox.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
			try {
				// RichTextBox.Text stores line breaks as \n, so token positions match Select positions.
				string text = _textBox.Text;
				_textBox.SelectAll();
				_textBox.SelectionColor = _textBox.ForeColor;
				foreach (SqlToken token in SqlTokenizer.Tokenize(text)) {
					_textBox.Select(token.Start, token.Length);
					_textBox.SelectionColor = _colors[token.Type];
				}
			} finally {
				_textBox.Select(selectionStart, selectionLength);
				SendMessage(_textBox.Handle, EM_SETSCROLLPOS, IntPtr.Zero, ref scroll);
				SendMessage(_textBox.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
				_textBox.Invalidate();
				_highlighting = false;
			}
		}


		private void OnTextChanged(object sender, EventArgs e)
		{
			if (_highlighting) {
				return;
			}
			_timer.Stop();
			_timer.Start();
		}


		/// <summary>Pastes plain text, so SQL copied from a web page or Word keeps the editor's font.</summary>
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			bool paste = (e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert);
			if (paste) {
				_textBox.Paste(DataFormats.GetFormat(DataFormats.UnicodeText));
				e.SuppressKeyPress = true;
			}
		}


		private void OnTimerTick(object sender, EventArgs e)
		{
			Highlight();
		}


		/// <summary>Stops highlighting and releases the timer.</summary>
		public void Dispose()
		{
			_textBox.TextChanged -= new EventHandler(OnTextChanged);
			_textBox.KeyDown -= new KeyEventHandler(OnKeyDown);
			_timer.Dispose();
		}
	}
}
