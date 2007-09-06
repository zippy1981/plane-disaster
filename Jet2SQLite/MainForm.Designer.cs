/*
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
 */

/*
 * Created by SharpDevelop.
 * Author:		Justin Dearing <zippy1981@gmail.com>
 * Date: 4/5/2007
 * Time: 9:33 AM
 */
namespace PlaneDisaster.Jet2SQLite
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.txtJetSqlFile = new System.Windows.Forms.TextBox();
			this.cmdBrowseJetSql = new System.Windows.Forms.Button();
			this.cmdSQLiteFile = new System.Windows.Forms.Button();
			this.txtSQLiteFile = new System.Windows.Forms.TextBox();
			this.cmdConvert = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtJetSqlFile
			// 
			this.txtJetSqlFile.Location = new System.Drawing.Point(4, 33);
			this.txtJetSqlFile.Margin = new System.Windows.Forms.Padding(4);
			this.txtJetSqlFile.Name = "txtJetSqlFile";
			this.txtJetSqlFile.Size = new System.Drawing.Size(647, 22);
			this.txtJetSqlFile.TabIndex = 7;
			// 
			// cmdBrowseJetSql
			// 
			this.cmdBrowseJetSql.Location = new System.Drawing.Point(477, 65);
			this.cmdBrowseJetSql.Margin = new System.Windows.Forms.Padding(4);
			this.cmdBrowseJetSql.Name = "cmdBrowseJetSql";
			this.cmdBrowseJetSql.Size = new System.Drawing.Size(175, 28);
			this.cmdBrowseJetSql.TabIndex = 8;
			this.cmdBrowseJetSql.Text = "Browse For &JetSQL File";
			this.cmdBrowseJetSql.UseVisualStyleBackColor = true;
			this.cmdBrowseJetSql.Click += new System.EventHandler(this.CmdBrowseJetSqlClick);
			// 
			// cmdSQLiteFile
			// 
			this.cmdSQLiteFile.Location = new System.Drawing.Point(477, 134);
			this.cmdSQLiteFile.Margin = new System.Windows.Forms.Padding(4);
			this.cmdSQLiteFile.Name = "cmdSQLiteFile";
			this.cmdSQLiteFile.Size = new System.Drawing.Size(175, 28);
			this.cmdSQLiteFile.TabIndex = 10;
			this.cmdSQLiteFile.Text = "Browse For &SQLite File";
			this.cmdSQLiteFile.UseVisualStyleBackColor = true;
			this.cmdSQLiteFile.Click += new System.EventHandler(this.CmdSQLiteFileClick);
			// 
			// txtSQLiteFile
			// 
			this.txtSQLiteFile.Location = new System.Drawing.Point(4, 101);
			this.txtSQLiteFile.Margin = new System.Windows.Forms.Padding(4);
			this.txtSQLiteFile.Name = "txtSQLiteFile";
			this.txtSQLiteFile.Size = new System.Drawing.Size(647, 22);
			this.txtSQLiteFile.TabIndex = 9;
			// 
			// cmdConvert
			// 
			this.cmdConvert.Location = new System.Drawing.Point(4, 170);
			this.cmdConvert.Margin = new System.Windows.Forms.Padding(4);
			this.cmdConvert.Name = "cmdConvert";
			this.cmdConvert.Size = new System.Drawing.Size(648, 28);
			this.cmdConvert.TabIndex = 11;
			this.cmdConvert.Text = "&Convert";
			this.cmdConvert.UseVisualStyleBackColor = true;
			this.cmdConvert.Click += new System.EventHandler(this.CmdConvertClick);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.helpToolStripMenuItem,
									this.fileToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.menuStrip1.Size = new System.Drawing.Size(656, 26);
			this.menuStrip1.TabIndex = 12;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(48, 22);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.aboutToolStripMenuItem.Text = "&About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(40, 22);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
			this.exitToolStripMenuItem.Text = "&Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
			// 
			// txtLog
			// 
			this.txtLog.Location = new System.Drawing.Point(4, 137);
			this.txtLog.Margin = new System.Windows.Forms.Padding(4);
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.Size = new System.Drawing.Size(468, 22);
			this.txtLog.TabIndex = 13;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(656, 207);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.cmdConvert);
			this.Controls.Add(this.cmdSQLiteFile);
			this.Controls.Add(this.txtSQLiteFile);
			this.Controls.Add(this.cmdBrowseJetSql);
			this.Controls.Add(this.txtJetSqlFile);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.Text = "Jet2SQLite";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.TextBox txtJetSqlFile;
		private System.Windows.Forms.Button cmdConvert;
		private System.Windows.Forms.TextBox txtSQLiteFile;
		private System.Windows.Forms.Button cmdSQLiteFile;
		private System.Windows.Forms.Button cmdBrowseJetSql;
	}
}
