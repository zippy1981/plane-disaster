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
			this.txtJetSqlFile = new System.Windows.Forms.TextBox();
			this.cmdBrowseJetSql = new System.Windows.Forms.Button();
			this.cmdSQLiteFile = new System.Windows.Forms.Button();
			this.txtSQLiteFile = new System.Windows.Forms.TextBox();
			this.cmdConvert = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtJetSqlFile
			// 
			this.txtJetSqlFile.Location = new System.Drawing.Point(3, 27);
			this.txtJetSqlFile.Name = "txtJetSqlFile";
			this.txtJetSqlFile.Size = new System.Drawing.Size(486, 20);
			this.txtJetSqlFile.TabIndex = 7;
			// 
			// cmdBrowseJetSql
			// 
			this.cmdBrowseJetSql.Location = new System.Drawing.Point(358, 53);
			this.cmdBrowseJetSql.Name = "cmdBrowseJetSql";
			this.cmdBrowseJetSql.Size = new System.Drawing.Size(131, 23);
			this.cmdBrowseJetSql.TabIndex = 8;
			this.cmdBrowseJetSql.Text = "Browse For &JetSQL File";
			this.cmdBrowseJetSql.UseVisualStyleBackColor = true;
			this.cmdBrowseJetSql.Click += new System.EventHandler(this.CmdBrowseJetSqlClick);
			// 
			// cmdSQLiteFile
			// 
			this.cmdSQLiteFile.Location = new System.Drawing.Point(358, 109);
			this.cmdSQLiteFile.Name = "cmdSQLiteFile";
			this.cmdSQLiteFile.Size = new System.Drawing.Size(131, 23);
			this.cmdSQLiteFile.TabIndex = 10;
			this.cmdSQLiteFile.Text = "Browse For &SQLite File";
			this.cmdSQLiteFile.UseVisualStyleBackColor = true;
			this.cmdSQLiteFile.Click += new System.EventHandler(this.CmdSQLiteFileClick);
			// 
			// txtSQLiteFile
			// 
			this.txtSQLiteFile.Location = new System.Drawing.Point(3, 82);
			this.txtSQLiteFile.Name = "txtSQLiteFile";
			this.txtSQLiteFile.Size = new System.Drawing.Size(486, 20);
			this.txtSQLiteFile.TabIndex = 9;
			// 
			// cmdConvert
			// 
			this.cmdConvert.Location = new System.Drawing.Point(3, 138);
			this.cmdConvert.Name = "cmdConvert";
			this.cmdConvert.Size = new System.Drawing.Size(486, 23);
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
			this.menuStrip1.Size = new System.Drawing.Size(492, 24);
			this.menuStrip1.TabIndex = 12;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
			this.aboutToolStripMenuItem.Text = "&About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
			// 
			// txtLog
			// 
			this.txtLog.Location = new System.Drawing.Point(3, 111);
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.Size = new System.Drawing.Size(352, 20);
			this.txtLog.TabIndex = 13;
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.exitToolStripMenuItem.Text = "&Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(492, 168);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.cmdConvert);
			this.Controls.Add(this.cmdSQLiteFile);
			this.Controls.Add(this.txtSQLiteFile);
			this.Controls.Add(this.cmdBrowseJetSql);
			this.Controls.Add(this.txtJetSqlFile);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MainMenuStrip = this.menuStrip1;
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
