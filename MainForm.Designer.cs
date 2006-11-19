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
 * Created:		08/01/2006
 * Description  This is the presentation code for PlaneDisaster.NET
/*/

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
			this.lstTables = new System.Windows.Forms.ListBox();
			this.radText = new System.Windows.Forms.RadioButton();
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
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.utilitiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.microsoftAccessJetSQLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.compactJetSQLmdbFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.repairDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.databaseSchemaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.lstViews = new System.Windows.Forms.ListBox();
			this.lblViews = new System.Windows.Forms.Label();
			this.lblTables = new System.Windows.Forms.Label();
			this.lblColumns = new System.Windows.Forms.Label();
			this.lblProcedures = new System.Windows.Forms.Label();
			this.lstProcedures = new System.Windows.Forms.ListBox();
			((System.ComponentModel.ISupportInitialize)(this.gridResults)).BeginInit();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lstTables
			// 
			this.lstTables.Location = new System.Drawing.Point(336, 400);
			this.lstTables.Name = "lstTables";
			this.lstTables.Size = new System.Drawing.Size(160, 56);
			this.lstTables.TabIndex = 5;
			this.lstTables.SelectedIndexChanged += new System.EventHandler(this.Lst_SelectedIndexChanged);
			// 
			// radText
			// 
			this.radText.Location = new System.Drawing.Point(681, 404);
			this.radText.Name = "radText";
			this.radText.Size = new System.Drawing.Size(95, 25);
			this.radText.TabIndex = 7;
			this.radText.TabStop = true;
			this.radText.Text = "Text (CSV)";
			this.radText.CheckedChanged += new System.EventHandler(this.RadTextCheckedChanged);
			// 
			// radGrid
			// 
			this.radGrid.Checked = true;
			this.radGrid.Location = new System.Drawing.Point(681, 377);
			this.radGrid.Name = "radGrid";
			this.radGrid.Size = new System.Drawing.Size(95, 25);
			this.radGrid.TabIndex = 8;
			this.radGrid.TabStop = true;
			this.radGrid.Text = "DataGridView";
			this.radGrid.CheckedChanged += new System.EventHandler(this.RadGridCheckedChanged);
			// 
			// cmdSQL
			// 
			this.cmdSQL.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdSQL.ForeColor = System.Drawing.Color.Red;
			this.cmdSQL.Location = new System.Drawing.Point(537, 24);
			this.cmdSQL.Name = "cmdSQL";
			this.cmdSQL.Size = new System.Drawing.Size(23, 35);
			this.cmdSQL.TabIndex = 2;
			this.cmdSQL.Text = "!";
			this.cmdSQL.Click += new System.EventHandler(events.CmdSQLClick);
			// 
			// lstColumns
			// 
			this.lstColumns.Location = new System.Drawing.Point(502, 400);
			this.lstColumns.Name = "lstColumns";
			this.lstColumns.Size = new System.Drawing.Size(160, 56);
			this.lstColumns.TabIndex = 6;
			// 
			// txtSQL
			// 
			this.txtSQL.Location = new System.Drawing.Point(4, 24);
			this.txtSQL.Multiline = true;
			this.txtSQL.Name = "txtSQL";
			this.txtSQL.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtSQL.Size = new System.Drawing.Size(527, 71);
			this.txtSQL.TabIndex = 1;
			// 
			// txtResults
			// 
			this.txtResults.Location = new System.Drawing.Point(4, 96);
			this.txtResults.Multiline = true;
			this.txtResults.Name = "txtResults";
			this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtResults.Size = new System.Drawing.Size(583, 275);
			this.txtResults.TabIndex = 3;
			this.txtResults.Visible = false;
			// 
			// cmdStatus
			// 
			this.cmdStatus.Location = new System.Drawing.Point(782, 375);
			this.cmdStatus.Name = "cmdStatus";
			this.cmdStatus.Size = new System.Drawing.Size(82, 21);
			this.cmdStatus.TabIndex = 10;
			this.cmdStatus.Text = "&Status";
			this.cmdStatus.Click += new System.EventHandler(events.CmdStatusClick);
			// 
			// gridResults
			// 
			this.gridResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gridResults.Location = new System.Drawing.Point(4, 96);
			this.gridResults.Name = "gridResults";
			this.gridResults.Size = new System.Drawing.Size(535, 19);
			this.gridResults.TabIndex = 3;
			// 
			// cmdSaveCsv
			// 
			this.cmdSaveCsv.Location = new System.Drawing.Point(782, 402);
			this.cmdSaveCsv.Name = "cmdSaveCsv";
			this.cmdSaveCsv.Size = new System.Drawing.Size(82, 21);
			this.cmdSaveCsv.TabIndex = 12;
			this.cmdSaveCsv.Text = "Save &CSV";
			this.cmdSaveCsv.Click += new System.EventHandler(events.CmdSaveCsvClick);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.fileToolStripMenuItem,
									this.helpToolStripMenuItem,
									this.utilitiesToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(876, 24);
			this.menuStrip1.TabIndex = 13;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.newToolStripMenuItem,
									this.openToolStripMenuItem,
									this.closeToolStripMenuItem,
									this.toolStripSeparator1,
									this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// newToolStripMenuItem
			// 
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
			this.newToolStripMenuItem.Text = "&New";
			this.newToolStripMenuItem.Click += new System.EventHandler(this.menuNew_Click);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// closeToolStripMenuItem
			// 
			this.closeToolStripMenuItem.Enabled = false;
			this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
			this.closeToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
			this.closeToolStripMenuItem.Text = "&Close";
			this.closeToolStripMenuItem.Click += new System.EventHandler(this.menuClose_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(108, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.menuExit_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
			this.aboutToolStripMenuItem.Text = "&About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.menuAbout_Click);
			// 
			// utilitiesToolStripMenuItem
			// 
			this.utilitiesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.microsoftAccessJetSQLToolStripMenuItem,
									this.databaseSchemaToolStripMenuItem});
			this.utilitiesToolStripMenuItem.Name = "utilitiesToolStripMenuItem";
			this.utilitiesToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
			this.utilitiesToolStripMenuItem.Text = "&Utilities";
			// 
			// microsoftAccessJetSQLToolStripMenuItem
			// 
			this.microsoftAccessJetSQLToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.compactJetSQLmdbFileToolStripMenuItem,
									this.repairDatabaseToolStripMenuItem});
			this.microsoftAccessJetSQLToolStripMenuItem.Name = "microsoftAccessJetSQLToolStripMenuItem";
			this.microsoftAccessJetSQLToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.microsoftAccessJetSQLToolStripMenuItem.Text = "Microsoft Access (JetSQL)";
			// 
			// compactJetSQLmdbFileToolStripMenuItem
			// 
			this.compactJetSQLmdbFileToolStripMenuItem.Name = "compactJetSQLmdbFileToolStripMenuItem";
			this.compactJetSQLmdbFileToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
			this.compactJetSQLmdbFileToolStripMenuItem.Text = "&Compact Database";
			this.compactJetSQLmdbFileToolStripMenuItem.Click += new System.EventHandler(this.menuCompactDatabase_Click);
			// 
			// repairDatabaseToolStripMenuItem
			// 
			this.repairDatabaseToolStripMenuItem.Name = "repairDatabaseToolStripMenuItem";
			this.repairDatabaseToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
			this.repairDatabaseToolStripMenuItem.Text = "&Repair Database";
			this.repairDatabaseToolStripMenuItem.Click += new System.EventHandler(this.menuRepairDatabase_Click);
			// 
			// databaseSchemaToolStripMenuItem
			// 
			this.databaseSchemaToolStripMenuItem.Enabled = false;
			this.databaseSchemaToolStripMenuItem.Name = "databaseSchemaToolStripMenuItem";
			this.databaseSchemaToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.databaseSchemaToolStripMenuItem.Text = "Database &Schema";
			this.databaseSchemaToolStripMenuItem.Click += new System.EventHandler(this.menuDatabaseSchema_Click);
			// 
			// lstViews
			// 
			this.lstViews.Location = new System.Drawing.Point(4, 400);
			this.lstViews.Name = "lstViews";
			this.lstViews.Size = new System.Drawing.Size(160, 56);
			this.lstViews.TabIndex = 14;
			this.lstViews.SelectedIndexChanged += new System.EventHandler(this.Lst_SelectedIndexChanged);
			// 
			// lblViews
			// 
			this.lblViews.Location = new System.Drawing.Point(4, 379);
			this.lblViews.Name = "lblViews";
			this.lblViews.Size = new System.Drawing.Size(100, 19);
			this.lblViews.TabIndex = 15;
			this.lblViews.Text = "Views";
			// 
			// lblTables
			// 
			this.lblTables.Location = new System.Drawing.Point(336, 381);
			this.lblTables.Name = "lblTables";
			this.lblTables.Size = new System.Drawing.Size(100, 19);
			this.lblTables.TabIndex = 16;
			this.lblTables.Text = "Tables";
			// 
			// lblColumns
			// 
			this.lblColumns.Location = new System.Drawing.Point(502, 379);
			this.lblColumns.Name = "lblColumns";
			this.lblColumns.Size = new System.Drawing.Size(100, 19);
			this.lblColumns.TabIndex = 17;
			this.lblColumns.Text = "Columns";
			// 
			// lblProcedures
			// 
			this.lblProcedures.Location = new System.Drawing.Point(170, 379);
			this.lblProcedures.Name = "lblProcedures";
			this.lblProcedures.Size = new System.Drawing.Size(100, 19);
			this.lblProcedures.TabIndex = 19;
			this.lblProcedures.Text = "Procedures";
			// 
			// lstProcedures
			// 
			this.lstProcedures.Location = new System.Drawing.Point(170, 400);
			this.lstProcedures.Name = "lstProcedures";
			this.lstProcedures.Size = new System.Drawing.Size(160, 56);
			this.lstProcedures.TabIndex = 18;
			// 
			// MainForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(876, 464);
			this.Controls.Add(this.lblProcedures);
			this.Controls.Add(this.lstProcedures);
			this.Controls.Add(this.lblColumns);
			this.Controls.Add(this.lblTables);
			this.Controls.Add(this.lblViews);
			this.Controls.Add(this.lstViews);
			this.Controls.Add(this.cmdSaveCsv);
			this.Controls.Add(this.radGrid);
			this.Controls.Add(this.radText);
			this.Controls.Add(this.cmdSQL);
			this.Controls.Add(this.txtResults);
			this.Controls.Add(this.lstColumns);
			this.Controls.Add(this.txtSQL);
			this.Controls.Add(this.lstTables);
			this.Controls.Add(this.cmdStatus);
			this.Controls.Add(this.gridResults);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.MinimumSize = new System.Drawing.Size(884, 300);
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
		private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
		private System.Windows.Forms.ListBox lstProcedures;
		private System.Windows.Forms.Label lblProcedures;
		private System.Windows.Forms.Label lblColumns;
		private System.Windows.Forms.Label lblTables;
		private System.Windows.Forms.Label lblViews;
		private System.Windows.Forms.ToolStripMenuItem databaseSchemaToolStripMenuItem;
		private System.Windows.Forms.ListBox lstViews;
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
		private System.Windows.Forms.RadioButton radText;
		private System.Windows.Forms.ListBox lstTables;
		#endregion
	}
}
