/*/ 
 * Copyright 2006 Justin Dearing
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
/*/

/*/
 * Created by SharpDevelop.
 * Author:		Justin Dearing <zippy1981@gmail.com>
 * Created:		8/5/2006
 * Description  This is supposed to be a simple input dialog box. Its not done yet.
/*/


using System;
using System.Drawing;
using System.Windows.Forms;

namespace PlaneDisaster
{
	/// <summary>
	/// A simple input dialog box.
	/// </summary>
	public class InputDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button cmd;
		private System.Windows.Forms.TextBox txt;
		
		
		/// <summary>
		/// The text entered into the input box.
		/// </summary>
		public string  Input {
			get { return txt.Text; }
		}
		
		
		/// <summary>
		/// Default constructor.
		/// </summary>
		public InputDialog()
		{
			InitializeComponent();
		}

		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
			this.txt = new System.Windows.Forms.TextBox();
			this.cmd = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txt
			// 
			this.txt.Location = new System.Drawing.Point(4, 4);
			this.txt.Name = "txt";
			this.txt.Size = new System.Drawing.Size(342, 23);
			this.txt.TabIndex = 0;
			// 
			// cmd
			// 
			this.cmd.Location = new System.Drawing.Point(352, 4);
			this.cmd.Name = "cmd";
			this.cmd.Size = new System.Drawing.Size(36, 23);
			this.cmd.TabIndex = 1;
			this.cmd.Text = "&Ok";
			this.cmd.Click += new System.EventHandler(this.CmdClick);
			// 
			// InputDialog
			// 
			this.AcceptButton = this.cmd;
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
			this.ClientSize = new System.Drawing.Size(392, 30);
			this.ControlBox = false;
			this.Controls.Add(this.cmd);
			this.Controls.Add(this.txt);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InputDialog";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "InputDialog";
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		
		void CmdClick(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
		
		
		/// <summary>
		/// Displays the InoutBox Form with the title given.
		/// </summary>
		/// <param name="Title">The title of the Form.</param>
		/// <returns>
		/// Result of the dialog.
		/// </returns>
		public DialogResult  ShowDialog (string Title) {
			this.Text = Title;
			return this.ShowDialog();
		}
		
	}
}
