/*
 * Copyright 2006-2007 Justin Dearing
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
 * Created:		08/01/2006
 * Description  This is the presentation code for PlaneDisaster.NET
 */

namespace PlaneDisaster
{
	partial class MainForm : System.Windows.Forms.Form
	{
		
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

		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.radCSV = new System.Windows.Forms.RadioButton();
			this.radGrid = new System.Windows.Forms.RadioButton();
			this.cmdSQL = new System.Windows.Forms.Button();
			this.lstColumns = new System.Windows.Forms.ListBox();
			this.txtSQL = new System.Windows.Forms.TextBox();
			this.txtResults = new System.Windows.Forms.TextBox();
			this.cmdStatus = new System.Windows.Forms.Button();
			this.gridResults = new System.Windows.Forms.DataGridView();
			this.cmdSaveCsv = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openRecentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.queryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.executeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.utilitiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.microsoftAccessJetSQLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.compactJetSQLmdbFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.repairDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.databaseSchemaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.lblColumns = new System.Windows.Forms.Label();
			this.radInsert = new System.Windows.Forms.RadioButton();
			this.cmdRefresh = new System.Windows.Forms.Button();
			this.cmdSavSql = new System.Windows.Forms.Button();
			this.treeObjects = new System.Windows.Forms.TreeView();
			((System.ComponentModel.ISupportInitialize)(this.gridResults)).BeginInit();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// radCSV
			// 
			this.radCSV.Location = new System.Drawing.Point(668, 400);
			this.radCSV.Name = "radCSV";
			this.radCSV.Size = new System.Drawing.Size(130, 25);
			this.radCSV.TabIndex = 14;
			this.radCSV.TabStop = true;
			this.radCSV.Text = "Text (CSV)";
			this.radCSV.CheckedChanged += new System.EventHandler(this.RadCSVCheckedChanged);
			// 
			// radGrid
			// 
			this.radGrid.Checked = true;
			this.radGrid.Location = new System.Drawing.Point(668, 377);
			this.radGrid.Name = "radGrid";
			this.radGrid.Size = new System.Drawing.Size(130, 25);
			this.radGrid.TabIndex = 13;
			this.radGrid.TabStop = true;
			this.radGrid.Text = "DataGridView";
			this.radGrid.CheckedChanged += new System.EventHandler(this.RadGridCheckedChanged);
			// 
			// cmdSQL
			// 
			this.cmdSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdSQL.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdSQL.ForeColor = System.Drawing.Color.Red;
			this.cmdSQL.Location = new System.Drawing.Point(855, 29);
			this.cmdSQL.Name = "cmdSQL";
			this.cmdSQL.Size = new System.Drawing.Size(35, 35);
			this.cmdSQL.TabIndex = 2;
			this.cmdSQL.Text = "!";
			this.cmdSQL.Click += new System.EventHandler(this.CmdSQLClick);
			// 
			// lstColumns
			// 
			this.lstColumns.ItemHeight = 16;
			this.lstColumns.Location = new System.Drawing.Point(0, 404);
			this.lstColumns.Name = "lstColumns";
			this.lstColumns.Size = new System.Drawing.Size(160, 52);
			this.lstColumns.TabIndex = 12;
			// 
			// txtSQL
			// 
			this.txtSQL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.txtSQL.Location = new System.Drawing.Point(4, 29);
			this.txtSQL.Multiline = true;
			this.txtSQL.Name = "txtSQL";
			this.txtSQL.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtSQL.Size = new System.Drawing.Size(845, 71);
			this.txtSQL.TabIndex = 1;
			// 
			// txtResults
			// 
			this.txtResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.txtResults.Location = new System.Drawing.Point(166, 101);
			this.txtResults.Multiline = true;
			this.txtResults.Name = "txtResults";
			this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtResults.Size = new System.Drawing.Size(724, 275);
			this.txtResults.TabIndex = 4;
			this.txtResults.Visible = false;
			// 
			// cmdStatus
			// 
			this.cmdStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdStatus.Location = new System.Drawing.Point(804, 383);
			this.cmdStatus.Name = "cmdStatus";
			this.cmdStatus.Size = new System.Drawing.Size(86, 21);
			this.cmdStatus.TabIndex = 16;
			this.cmdStatus.Text = "&Status";
			this.cmdStatus.Click += new System.EventHandler(this.CmdStatusClick);
			// 
			// gridResults
			// 
			this.gridResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.gridResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gridResults.Location = new System.Drawing.Point(232, 101);
			this.gridResults.Name = "gridResults";
			this.gridResults.RowTemplate.Height = 24;
			this.gridResults.Size = new System.Drawing.Size(658, 275);
			this.gridResults.TabIndex = 3;
			// 
			// cmdSaveCsv
			// 
			this.cmdSaveCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdSaveCsv.Location = new System.Drawing.Point(804, 410);
			this.cmdSaveCsv.Name = "cmdSaveCsv";
			this.cmdSaveCsv.Size = new System.Drawing.Size(86, 21);
			this.cmdSaveCsv.TabIndex = 17;
			this.cmdSaveCsv.Text = "Save &CSV";
			this.cmdSaveCsv.Click += new System.EventHandler(this.CmdSaveCsvClick);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.fileToolStripMenuItem,
									this.helpToolStripMenuItem,
									this.queryToolStripMenuItem,
									this.utilitiesToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.menuStrip1.Size = new System.Drawing.Size(902, 26);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.newToolStripMenuItem,
									this.openToolStripMenuItem,
									this.openRecentToolStripMenuItem,
									this.closeToolStripMenuItem,
									this.toolStripSeparator1,
									this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(40, 22);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// newToolStripMenuItem
			// 
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.newToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
			this.newToolStripMenuItem.Text = "&New";
			this.newToolStripMenuItem.Click += new System.EventHandler(this.menuNew_Click);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// openRecentToolStripMenuItem
			// 
			this.openRecentToolStripMenuItem.Enabled = false;
			this.openRecentToolStripMenuItem.Name = "openRecentToolStripMenuItem";
			this.openRecentToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
			this.openRecentToolStripMenuItem.Text = "Open &Recent";
			// 
			// closeToolStripMenuItem
			// 
			this.closeToolStripMenuItem.Enabled = false;
			this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
			this.closeToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
			this.closeToolStripMenuItem.Text = "&Close";
			this.closeToolStripMenuItem.Click += new System.EventHandler(this.menuClose_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(174, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.menuExit_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(48, 22);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.aboutToolStripMenuItem.Text = "&About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.menuAbout_Click);
			// 
			// queryToolStripMenuItem
			// 
			this.queryToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.executeToolStripMenuItem});
			this.queryToolStripMenuItem.Enabled = false;
			this.queryToolStripMenuItem.Name = "queryToolStripMenuItem";
			this.queryToolStripMenuItem.Size = new System.Drawing.Size(60, 22);
			this.queryToolStripMenuItem.Text = "&Query";
			// 
			// executeToolStripMenuItem
			// 
			this.executeToolStripMenuItem.Name = "executeToolStripMenuItem";
			this.executeToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.executeToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
			this.executeToolStripMenuItem.Text = "&Execute";
			this.executeToolStripMenuItem.Click += new System.EventHandler(this.CmdSQLClick);
			// 
			// utilitiesToolStripMenuItem
			// 
			this.utilitiesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.microsoftAccessJetSQLToolStripMenuItem,
									this.databaseSchemaToolStripMenuItem});
			this.utilitiesToolStripMenuItem.Name = "utilitiesToolStripMenuItem";
			this.utilitiesToolStripMenuItem.Size = new System.Drawing.Size(63, 22);
			this.utilitiesToolStripMenuItem.Text = "&Utilities";
			// 
			// microsoftAccessJetSQLToolStripMenuItem
			// 
			this.microsoftAccessJetSQLToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.compactJetSQLmdbFileToolStripMenuItem,
									this.repairDatabaseToolStripMenuItem});
			this.microsoftAccessJetSQLToolStripMenuItem.Name = "microsoftAccessJetSQLToolStripMenuItem";
			this.microsoftAccessJetSQLToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
			this.microsoftAccessJetSQLToolStripMenuItem.Text = "Microsoft Access (JetSQL)";
			// 
			// compactJetSQLmdbFileToolStripMenuItem
			// 
			this.compactJetSQLmdbFileToolStripMenuItem.Name = "compactJetSQLmdbFileToolStripMenuItem";
			this.compactJetSQLmdbFileToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
			this.compactJetSQLmdbFileToolStripMenuItem.Text = "&Compact Database";
			this.compactJetSQLmdbFileToolStripMenuItem.Click += new System.EventHandler(this.menuCompactDatabase_Click);
			// 
			// repairDatabaseToolStripMenuItem
			// 
			this.repairDatabaseToolStripMenuItem.Name = "repairDatabaseToolStripMenuItem";
			this.repairDatabaseToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
			this.repairDatabaseToolStripMenuItem.Text = "&Repair Database";
			this.repairDatabaseToolStripMenuItem.Click += new System.EventHandler(this.menuRepairDatabase_Click);
			// 
			// databaseSchemaToolStripMenuItem
			// 
			this.databaseSchemaToolStripMenuItem.Enabled = false;
			this.databaseSchemaToolStripMenuItem.Name = "databaseSchemaToolStripMenuItem";
			this.databaseSchemaToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
			this.databaseSchemaToolStripMenuItem.Text = "Database &Schema";
			this.databaseSchemaToolStripMenuItem.Click += new System.EventHandler(this.menuDatabaseSchema_Click);
			// 
			// lblColumns
			// 
			this.lblColumns.Location = new System.Drawing.Point(0, 383);
			this.lblColumns.Name = "lblColumns";
			this.lblColumns.Size = new System.Drawing.Size(100, 19);
			this.lblColumns.TabIndex = 11;
			this.lblColumns.Text = "Columns";
			// 
			// radInsert
			// 
			this.radInsert.Location = new System.Drawing.Point(668, 427);
			this.radInsert.Name = "radInsert";
			this.radInsert.Size = new System.Drawing.Size(130, 25);
			this.radInsert.TabIndex = 15;
			this.radInsert.TabStop = true;
			this.radInsert.Text = "Text (INSERTS)";
			this.radInsert.CheckedChanged += new System.EventHandler(this.RadInsertCheckedChanged);
			// 
			// cmdRefresh
			// 
			this.cmdRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdRefresh.Enabled = false;
			this.cmdRefresh.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdRefresh.ForeColor = System.Drawing.Color.Red;
			this.cmdRefresh.Location = new System.Drawing.Point(855, 65);
			this.cmdRefresh.Name = "cmdRefresh";
			this.cmdRefresh.Size = new System.Drawing.Size(35, 35);
			this.cmdRefresh.TabIndex = 3;
			this.cmdRefresh.Text = "&R";
			this.cmdRefresh.Click += new System.EventHandler(this.CmdRefreshClick);
			// 
			// cmdSavSql
			// 
			this.cmdSavSql.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdSavSql.Location = new System.Drawing.Point(804, 437);
			this.cmdSavSql.Name = "cmdSavSql";
			this.cmdSavSql.Size = new System.Drawing.Size(86, 21);
			this.cmdSavSql.TabIndex = 18;
			this.cmdSavSql.Text = "Save &SQL";
			this.cmdSavSql.Click += new System.EventHandler(this.CmdSavSqlClick);
			// 
			// treeObjects
			// 
			this.treeObjects.Location = new System.Drawing.Point(4, 101);
			this.treeObjects.Name = "treeObjects";
			this.treeObjects.Size = new System.Drawing.Size(156, 279);
			this.treeObjects.TabIndex = 19;
			this.treeObjects.MouseDown += new System.Windows.Forms.MouseEventHandler(WinFormEventsHelper.TreeViewRightClickSelect);
			// 
			// MainForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(902, 464);
			this.Controls.Add(this.treeObjects);
			this.Controls.Add(this.cmdSavSql);
			this.Controls.Add(this.cmdRefresh);
			this.Controls.Add(this.radInsert);
			this.Controls.Add(this.lblColumns);
			this.Controls.Add(this.cmdSaveCsv);
			this.Controls.Add(this.radGrid);
			this.Controls.Add(this.radCSV);
			this.Controls.Add(this.cmdSQL);
			this.Controls.Add(this.txtResults);
			this.Controls.Add(this.lstColumns);
			this.Controls.Add(this.txtSQL);
			this.Controls.Add(this.cmdStatus);
			this.Controls.Add(this.gridResults);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.MinimumSize = new System.Drawing.Size(910, 300);
			this.Name = "MainForm";
			this.Text = "Plane Disaster.NET";
			this.SizeChanged += new System.EventHandler(this.MainFormResize);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.MainFormResize);
			((System.ComponentModel.ISupportInitialize)(this.gridResults)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.TreeView treeObjects;
		private System.Windows.Forms.Button cmdSavSql;
		private System.Windows.Forms.ToolStripMenuItem executeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem queryToolStripMenuItem;
		private System.Windows.Forms.Button cmdRefresh;
		private System.Windows.Forms.RadioButton radInsert;
		private System.Windows.Forms.RadioButton radCSV;
		private System.Windows.Forms.ToolStripMenuItem openRecentToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
		private System.Windows.Forms.Label lblColumns;
		private System.Windows.Forms.ToolStripMenuItem databaseSchemaToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem repairDatabaseToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem microsoftAccessJetSQLToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem compactJetSQLmdbFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem utilitiesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.Button cmdSaveCsv;
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.DataGridView gridResults;
		private System.Windows.Forms.Button cmdStatus;
		private System.Windows.Forms.TextBox txtResults;
		private System.Windows.Forms.TextBox txtSQL;
		private System.Windows.Forms.ListBox lstColumns;
		private System.Windows.Forms.Button cmdSQL;
		private System.Windows.Forms.RadioButton radGrid;
		#endregion
	}
}
