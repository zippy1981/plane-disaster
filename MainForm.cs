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
 * Created:		08/01/2006
 * Description  This is the presentation code for PlaneDisaster.NET
 */

 
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows.Forms;

using PlaneDisaster.Configuration;

namespace PlaneDisaster
{
	/// <summary>
	/// PlaneDisaster.NET main form.
	/// </summary>
	public sealed partial class MainForm
	{
		private System.Configuration.Configuration Config;
		private PlaneDisasterSection oPlaneDisasterSection;
		private dba dbcon = null;
				
		#region Properties
		
		/// <summary>The Results of the query in CSV format.</summary>
		internal string CSV {
			get { return this.txtResults.Text; }
			set { this.txtResults.Text = value; }
		}
		
		
		/// <summary>
		/// The maximum rows to display in the query window.
		/// This only applies to tables and views, not custom queries.
		/// </summary>
		private int MaxRowDisplayCount {
			get { return 500; }
		}
		
		
		/// <summary>The contents of the Query Text Area.</summary>
		internal string Query {
			get { return this.txtSQL.Text; }
			set { this.txtSQL.Text = value; }
		}
		
		#endregion
		
		/// <summary>
		/// Default constructor.
		/// </summary>
		internal MainForm()
		{
			InitializeComponent();
			
			/* ListBox Double Click event handlers */
			lstColumns.DoubleClick += new System.EventHandler(this.lst_DblClick);
			lstProcedures.DoubleClick += new System.EventHandler(this.lst_DblClick);
			lstTables.DoubleClick += new System.EventHandler(this.lst_DblClick);
			lstViews.DoubleClick += new System.EventHandler(this.lst_DblClick);
			
			/* ListBox Right Click event handlers */
			lstProcedures.MouseDown += new MouseEventHandler(this.Lst_RightClickSelect);
			lstTables.MouseDown += new MouseEventHandler(this.Lst_RightClickSelect);
			lstViews.MouseDown += new MouseEventHandler(this.Lst_RightClickSelect);
			
			gridResults.DataError += new DataGridViewDataErrorEventHandler(this.EvtDataGridError);
			
			Config = ConfigurationManager.OpenExeConfiguration
				(ConfigurationUserLevel.PerUserRoaming);
			oPlaneDisasterSection = (PlaneDisasterSection)Config.GetSection("planeDisaster");
			if (oPlaneDisasterSection == null) {
				oPlaneDisasterSection = new PlaneDisasterSection();
				oPlaneDisasterSection.SectionInformation.AllowExeDefinition =
					ConfigurationAllowExeDefinition.MachineToLocalUser;
				Config.Sections.Add("planeDisaster", oPlaneDisasterSection);
			}
			
			GenerateOpenRecentMenu();
		}

		#region Events
		
		#region Button Events
		
		void CmdSaveCsvClick(object sender, System.EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			string FileName;
			dlg.Filter = "Comma Seperated Value (*.csv)|*.csv|All Files|";
			
			if(dlg.ShowDialog() == DialogResult.OK ) {
				FileName = dlg.FileName;
				using (StreamWriter sw = File.CreateText(FileName))
				{
					sw.Write(this.CSV);
	            }  
			}
		}
		
		
		void CmdSQLClick(object sender, System.EventArgs e)
		{
			LoadQueryResults();
		}
		
		
		void CmdStatusClick(object sender, System.EventArgs e)
		{
			MessageBox.Show(GetDatabaseStatus());
		}
		
		#endregion
				
		#region DataGridView Events
		
		void EvtDataGridError(object sender, DataGridViewDataErrorEventArgs e) {
			if ((e.Context & DataGridViewDataErrorContexts.Display) == DataGridViewDataErrorContexts.Display) {
				//Its ok its just not a picture
			} else { e.ThrowException = true;}
		}
		
		#endregion
		
		#region Form Events
		
		void MainFormResize(object sender, System.EventArgs e) {
			this.SuspendLayout();
			
			//Width
			this.gridResults.Width = this.Width - 15;
			this.txtResults.Width = this.Width - 15;
			this.cmdSQL.Left = this.Width - 35;
			this.txtSQL.Width = this.Width - 45;
			
			// Height
			// Output display controls
			this.gridResults.Height = this.ClientSize.Height - 185;
			this.txtResults.Height = this.ClientSize.Height - 185;
			// The Labels
			this.lblColumns.Top = this.ClientSize.Height - 85;
			this.lblProcedures.Top = this.ClientSize.Height - 85;
			this.lblTables.Top = this.ClientSize.Height - 85;
			this.lblViews.Top = this.ClientSize.Height - 85;
			// The List boxes
			this.lstColumns.Top = this.ClientSize.Height - 65;
			this.lstProcedures.Top = this.ClientSize.Height - 65;
			this.lstTables.Top = this.ClientSize.Height - 65;
			this.lstViews.Top = this.ClientSize.Height - 65;
			// Radio Buttons
			this.radGrid.Top = this.ClientSize.Height -85;
			this.radText.Top = this.ClientSize.Height -60;
			// Buttons
			this.cmdStatus.Top = this.ClientSize.Height - 85;
			this.cmdSaveCsv.Top = this.ClientSize.Height -60;
			
			this.ResumeLayout();
		}
		
		#endregion

		
		#region ListBox Events
	
		void lst_DblClick(object sender, System.EventArgs e) {
			ListBox lst = (ListBox) sender;
			if (lst.Name == "lstColumns") {
				//I dont know what the default action for the columm list should be
			} else {
				try {
				int RowCount = dbcon.GetTableRowCount(lst.Text);
				if (RowCount > this.MaxRowDisplayCount) {
					string Message;
					string SQL = String.Format
						("SELECT TOP {1} * FROM {0}",
						 lst.Text, MaxRowDisplayCount);
					Message = String.Format
						("Row count is {0}. Displaying the first {1} rows.", 
						dbcon.GetTableRowCount(lst.Text),
						MaxRowDisplayCount);
					MessageBox.Show(Message, "Too Many Rows!");
					LoadQueryResults(SQL);
				} else {
					LoadTableResults(lst.Text);
				}
				} catch (DbException ex) {
					if (ex.ErrorCode == -2147217865) {
						MessageBox.Show(ex.Message, "Query Error");
					}
					else { throw ex; }
				}
			}
		}
		
		
		/// <summary>
		/// If you want to be able to select an item in your listbox via right click,
		/// add this function as an event handler to the listbox's mousedown event.
		/// </summary>
		/// <param name="sender">The listbox being right clicked.</param>
		/// <param name="e">The MouseEventArgs object.</param>
		void Lst_RightClickSelect(object sender, MouseEventArgs  e) {
			if (e.Button == MouseButtons.Right) {
				ListBox lst = (ListBox) sender;
				int Index = lst.IndexFromPoint(e.X, e.Y);
			
				if (Index >= 0 && Index < lst.Items.Count) {
            	    lst.SelectedIndex = Index;
            	}
            	lst.Refresh();
			}
		}
		
		
		void Lst_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ListBox lst = (ListBox) sender;
			try {
				lstColumns.DataSource = dbcon.GetColumnNames(lst.Text);
			}
			catch (System.Data.OleDb.OleDbException ex) {
				MessageBox.Show ("OleDbException: " + ex.Message + "\r\nCode: " + ex.ErrorCode);
			}
		}
		
		#endregion
		
		
		#region Menu Events
		
		void menuAbout_Click (object sender, System.EventArgs e) {
			//TODO: write a proper about box
			string Msg;
			
			Msg = "PlaneDisaster.NET database viewer\nCopyright 2006 Justin Dearing\nzippy1981@gmail.com";
			MessageBox.Show(Msg, "About PlaneDisaster.NET");
		}
		
		
		void menuClose_Click(object sender, System.EventArgs e)
		{
			DisconnectDataSource();
		}
		
		
		void menuCompactDatabase_Click (object sender, System.EventArgs e)
		{
			string CurrentFile = GetFileName();
			string FileFilter = "Microsoft Access (*.mdb;*.mde)|*.mdb;*,mde";
			FileDialog dlg = new OpenFileDialog();
			dlg.Filter = FileFilter;
			
			if (dlg.ShowDialog() == DialogResult.OK) {
				if (dlg.FileName == CurrentFile) {
					DisconnectDataSource();
					JetSqlUtil.CompactMDB(dlg.FileName);
					OpenMDB(CurrentFile);
				} else { 
					JetSqlUtil.CompactMDB(dlg.FileName); }
			}
			dlg.Dispose();
		}
		
		
		void menuDatabaseSchema_Click(object sender, System.EventArgs e)
		{
			this.gridResults.DataSource = dbcon.GetSchema();
		}
		
		
		void menuDrop_Click (object sender, System.EventArgs e) {
			MenuItem mnu = (MenuItem) sender;
			
			if (mnu.Name == "menuDropProcedure") {
				dbcon.DropProcedure('[' + (string) lstProcedures.SelectedItem + ']');
				lstProcedures.DataSource = dbcon.GetProcedures();
			} else if (mnu.Name == "menuDropTable") {
				dbcon.DropTable('[' + (string) lstTables.SelectedItem + ']');
				lstTables.DataSource = dbcon.GetTables();
			} else if (mnu.Name == "menuDropView") {
				dbcon.DropView('[' + (string) lstViews.SelectedItem + ']');
				lstViews.DataSource = dbcon.GetViews();
			} else {
				throw new ArgumentException
					("sender for menuDrop_Click must be one of " +
					 "menuProcedures, menuTables, or menuViews.");
			}
		}

		
		void menuExit_Click (object sender, System.EventArgs e) {
			Close();
		}
		
		
		void menuNew_Click (object sender, System.EventArgs e)
		{
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new SaveFileDialog();
			dlg.Title = "New Database";
			FileFilter.Append("Microsoft Access (*.mdb)|*.mdb");
			FileFilter.Append("|Microsoft Access 95 (*.mdb)|*.mdb");
			FileFilter.Append("|Microsoft Access 2000 (*.mdb)|*.mdb");
			FileFilter.Append("|SQLite3 (*.db;*.db3;*.sqlite)|*.db;*.db3;*.sqlite");
			dlg.Filter = FileFilter.ToString();
			
			if (dlg.ShowDialog() == DialogResult.OK) {
				switch (dlg.FilterIndex) {
					case 1:
						JetSqlUtil.CreateMDB(dlg.FileName);
						OpenMDB(dlg.FileName);
						break;
					case 2:
						JetSqlUtil.CreateMDBv3(dlg.FileName);
						OpenMDB(dlg.FileName);
						break;
					case 3:
						JetSqlUtil.CreateMDBv4(dlg.FileName);
						OpenMDB(dlg.FileName);
						break;
					case 4:
						System.Data.SQLite.SQLiteConnection.CreateFile
							(dlg.FileName);
						OpenSQLite(dlg.FileName);
						break;
				}
				Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(dlg.FileName), dlg.FileName);
			}
			AddRecentFile(dlg.FileName);
			GenerateOpenRecentMenu();
			dlg.Dispose();
			InitContextMenues();
		}

		
		void menuOpen_Click (object sender, System.EventArgs e) {
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new OpenFileDialog();
			FileFilter.Append("All supported database types|*.mdb;*.mde;*.db;*.db3;*.sqlite");
			FileFilter.Append("|Microsoft Access (*.mdb;*.mde)|*.mdb;*.mde");
			FileFilter.Append("|SQLite3 (*.db;*.db3;*.sqlite)|*.db;*.db3;*.sqlite");
			dlg.Filter = FileFilter.ToString();
			
			if(dlg.ShowDialog() == DialogResult.OK) {
				switch (dlg.FilterIndex) {
					case 1:
						string Extension = 
							System.IO.Path.GetExtension(dlg.FileName).ToLower();
						if (Extension == ".mdb" || Extension == ".mde") {
							OpenMDB(dlg.FileName);
						} else if (Extension == ".db" || Extension == ".db3" || Extension == ".sqlite") {
							OpenSQLite(dlg.FileName);
						} else {throw new ApplicationException("Unknown file type.");}
						break;
					case 2:
						OpenMDB(dlg.FileName);
						break;
					case 3:
						OpenSQLite(dlg.FileName);
						break;
				}
				AddRecentFile(dlg.FileName);
				GenerateOpenRecentMenu();
				Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(dlg.FileName), dlg.FileName);
			}
			dlg.Dispose();
			InitContextMenues();
			
			
		}
		
		
		void menuOpenRecent_Click (object sender, System.EventArgs e) {
			string FileName = Path.GetFullPath(((ToolStripItem)sender).Text);			
			string Extension =
				System.IO.Path.GetExtension(FileName).ToLower();
			
			if (Extension == ".mdb" || Extension == ".mde") {
				OpenMDB(FileName);
			} else if (Extension == ".db" || Extension == ".db3" || Extension == ".sqlite") {
				OpenSQLite(FileName);
			} else {throw new ApplicationException("Unknown file type.");}
			AddRecentFile(FileName);	//Put this here to bump the file to the top of the list.
			GenerateOpenRecentMenu();	//Regenerate the open recent menu
			Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(FileName), FileName);
		}


		void menuRepairDatabase_Click (object sender, System.EventArgs e)
		{
			string CurrentFile = GetFileName();
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new OpenFileDialog();
			FileFilter.Append("Microsoft Access (*.mdb)|*.mdb");
			dlg.Filter = FileFilter.ToString();
			
			if (dlg.ShowDialog() == DialogResult.OK) {
				if (dlg.FileName == CurrentFile) {
					DisconnectDataSource();
					JetSqlUtil.RepairMDB(dlg.FileName);
					OpenMDB(CurrentFile);
				} else { 
					JetSqlUtil.RepairMDB(dlg.FileName); }
			}
			dlg.Dispose();
		}
		
		
		void menuSchema_Click (object sender, System.EventArgs e) {
			MenuItem mnu = (MenuItem) sender;
			
			if (mnu.Name == "menuProcedureSchema") {
				this.gridResults.DataSource = dbcon.GetColumnSchema(lstProcedures.Text);
			} else if (mnu.Name == "menuTableSchema") {
				this.gridResults.DataSource = dbcon.GetColumnSchema(lstTables.Text);
			} else if (mnu.Name == "menuViewSchema") {
				this.gridResults.DataSource = dbcon.GetColumnSchema(lstViews.Text);
			} else {
				throw new ArgumentException
					("sender for menu_Click must be one of " +
					 "menuProcedures, menuTables, or menuViews.");
			}
		}
		
		
		void menuScript_Click (object sender, System.EventArgs e) {
			MenuItem mnu = (MenuItem) sender;
			
			if (mnu.Name == "menuScriptProcedure") {
				this.Query = dbcon.GetProcedureSQL(lstProcedures.Text);
			} else if (mnu.Name == "menuScriptTable") {
				this.Query = ((SQLiteDba)dbcon).GetTableSQL(lstTables.Text);
			} else if (mnu.Name == "menuScriptView") {
				this.Query = dbcon.GetViewSQL(lstViews.Text);
			} else {
				throw new ArgumentException
					("sender for menuScript_Click must be one of " +
					 "menuProcedures, menuTables, or menuViews.");
			}
		}
		
		
		void menuShow_Click (object sender, System.EventArgs e) {
			MenuItem mnu = (MenuItem) sender;
			
			if (mnu.Name == "menuShowProcedure") {
				this.lst_DblClick(lstProcedures, e);
			} else if (mnu.Name == "menuShowTable") {
				this.lst_DblClick(lstTables, e);
			} else if (mnu.Name == "menuShowView") {
				this.lst_DblClick(lstViews, e);
			} else {
				throw new ArgumentException
					("sender for menuShow_Click must be one of " +
					 "menuProcedures, menuTables, or menuViews.");
			}
		}
		
		#endregion

		#region Radio Events
		

		void RadGridCheckedChanged(object sender, System.EventArgs e)
		{
			txtResults.Hide();
			gridResults.Show();
		}
		

		void RadTextCheckedChanged(object sender, System.EventArgs e)
		{
			gridResults.Hide();
			txtResults.Show();
		}
		

		#endregion

		#endregion
		

		private void AddRecentFile (string FileName) {
			FileName = Path.GetFullPath(FileName);
			short FileCount;			
			
			try {
				FileCount = oPlaneDisasterSection.RecentFiles.MaxCount;
			} catch (NullReferenceException) {
				MessageBox.Show("There is no spoon");
				oPlaneDisasterSection = new PlaneDisasterSection();
				Config.Sections.Remove("planeDisaster");
				Config.Sections.Add("planeDisaster", oPlaneDisasterSection);
				FileCount = oPlaneDisasterSection.RecentFiles.MaxCount;
			}

			//TODO: all this should be done by PlaneDisasterSection.Add
			List<string> Files = new List<string>();
			Files.Add(FileName);
			int i = 0;
			foreach (RecentFileElement curFile in oPlaneDisasterSection.RecentFiles) {
				if (curFile.Name != FileName) {
					Files.Add(curFile.Name);
					i++;
					if (i >= FileCount) break;
				}
			}
			oPlaneDisasterSection.RecentFiles.Clear();
			oPlaneDisasterSection.RecentFiles.AddRange(Files.ToArray());
			Config.Save();
		}
		
		
		private void AddRecentFileToMenu (string FileName) {
			openRecentToolStripMenuItem.Enabled = true;
			ToolStripMenuItem RecentFileMenu = new ToolStripMenuItem(FileName);
			RecentFileMenu.Click  += menuOpenRecent_Click;
			this.openRecentToolStripMenuItem.DropDownItems.Add
				(RecentFileMenu);
		}
		
		private void ClearRecentFileMenu() {
			this.openRecentToolStripMenuItem.DropDownItems.Clear();
		}
		
		/// <summary>
		/// Disconnects from the data source and updates the GUI appropiatly.
		/// </summary>
		internal void DisconnectDataSource() {
			lstColumns.DataSource = null;
			lstProcedures.DataSource = null;
			lstTables.DataSource = null;
			lstViews.DataSource = null;
			
			lstColumns.ContextMenu = null;
			lstProcedures.ContextMenu = null;
			lstTables.ContextMenu = null;
			lstViews.ContextMenu = null;
			
			txtResults.Text = "";
			gridResults.DataSource = null;
			
			databaseSchemaToolStripMenuItem.Enabled = false;
			dbcon.Disconnect();
			dbcon = null;
			Text = string.Format("PlaneDisaster.NET");
			databaseSchemaToolStripMenuItem.Enabled = false;
			this.closeToolStripMenuItem.Enabled = false;
			
		}
		
		
		/// <summary>
		/// Populates the listbox that hold the names of tables in the
		/// database.
		/// </summary>
		/// <remarks>
		/// Anything that Connects to a Datasource should call this to 
		/// refresh the form.
		/// </remarks>
		private void DisplayDataSource() {
			lstColumns.DataSource = null;
			lstProcedures.DataSource = dbcon.GetProcedures();
			lstTables.DataSource = dbcon.GetTables();
			lstViews.DataSource = dbcon.GetViews();
			
			txtResults.Text = "";
			gridResults.DataSource = null;
			
			databaseSchemaToolStripMenuItem.Enabled = true;
			this.closeToolStripMenuItem.Enabled = true;
		}
		
		private void GenerateOpenRecentMenu() {
			try {
				// Ensure that this value is explicitly written in the xml file
				oPlaneDisasterSection.RecentFiles.MaxCount =
					oPlaneDisasterSection.RecentFiles.MaxCount;
				ClearRecentFileMenu();
				foreach (RecentFileElement RecentFile in oPlaneDisasterSection.RecentFiles) {
					AddRecentFileToMenu(RecentFile.Name);
				}
			} 
			catch (NullReferenceException) {
				oPlaneDisasterSection = new PlaneDisasterSection();
				oPlaneDisasterSection.RecentFiles.MaxCount =
					oPlaneDisasterSection.RecentFiles.MaxCount;
				Config.Sections.Remove("planeDisaster");
				Config.Sections.Add("planeDisaster", oPlaneDisasterSection);
				Config.Save();
			}
		}
		
		
		private string GetDatabaseStatus() {
			return dbcon.GetStatus();
		}
		
		
		/// <summary>
		/// Gets the file name of the currently open database.
		/// </summary>
		/// <returns>The file name of the currently open database.</returns>
		internal string GetFileName() {
			DbConnectionStringBuilder ConStr;
			
			if (dbcon is OleDba) {
				ConStr = new OleDbConnectionStringBuilder(dbcon.ConnectionString);
				//For some reason FileName is blank.
				return ((OleDbConnectionStringBuilder)ConStr).DataSource;
			} else if (dbcon is SQLiteDba) {
				ConStr = new SQLiteConnectionStringBuilder(dbcon.ConnectionString);
				return ((SQLiteConnectionStringBuilder)ConStr).DataSource;
			} else { return ""; }
		}
		
		
		internal void InitContextMenues () {
			ContextMenu ctxProcedure, ctxTable, ctxView;
			MenuItem menuDropProcedure, menuDropTable, menuDropView;
			MenuItem menuScriptProcedure, menuScriptTable, menuScriptView;
			MenuItem menuShowProcedure, menuShowTable, menuShowView;
			MenuItem menuTableSchema, menuViewSchema;
			
			menuDropProcedure = new MenuItem("Drop");
			menuDropProcedure.Click += new System.EventHandler(menuDrop_Click);
			menuDropProcedure.Name = "menuDropProcedure";
			
			menuScriptProcedure = new MenuItem("Script");
			menuScriptProcedure.Click += new System.EventHandler(menuScript_Click);
			menuScriptProcedure.Name = "menuScriptProcedure";
			
			menuShowProcedure = new MenuItem("Show");
			menuShowProcedure.Click += new System.EventHandler(menuShow_Click);
			menuShowProcedure.Name = "menuShowProcedure";
			
			ctxProcedure = new ContextMenu(new MenuItem[] {menuShowProcedure, menuScriptProcedure, menuDropProcedure});
			this.lstProcedures.ContextMenu = ctxProcedure;
				
			menuDropTable = new MenuItem("Drop");
			menuDropTable.Click += new System.EventHandler(menuDrop_Click);
			menuDropTable.Name = "menuDropTable";
			
			menuScriptTable = new MenuItem("Script");
			menuScriptTable.Click += new System.EventHandler(menuScript_Click);
			menuScriptTable.Name = "menuScriptTable";
			
			menuShowTable = new MenuItem("Show");
			menuShowTable.Click += new System.EventHandler(menuShow_Click);
			menuShowTable.Name = "menuShowTable";
			
			menuTableSchema = new MenuItem("Schema");
			menuTableSchema.Click += new System.EventHandler(this.menuSchema_Click);
			menuTableSchema.Name = "menuTableSchema";
			
			if (dbcon is SQLiteDba) {
				ctxTable = new ContextMenu(new MenuItem[] {menuShowTable, menuScriptTable, menuTableSchema, menuDropTable});
			} else {
				ctxTable = new ContextMenu(new MenuItem[] {menuShowTable, menuTableSchema, menuDropTable});
			}
			this.lstTables.ContextMenu = ctxTable;
			
			menuDropView = new MenuItem("Drop");
			menuDropView.Click += new System.EventHandler(menuDrop_Click);
			menuDropView.Name = "menuDropView";

			menuScriptView = new MenuItem("Script");
			menuScriptView.Click += new System.EventHandler(menuScript_Click);
			menuScriptView.Name = "menuScriptView";
			
			menuShowView = new MenuItem("Show");
			menuShowView.Click += new System.EventHandler(menuShow_Click);
			menuShowView.Name = "menuShowView";
			
			menuViewSchema = new MenuItem("Schema");
			menuViewSchema.Click += new System.EventHandler(this.menuSchema_Click);
			menuViewSchema.Name = "menuViewSchema";

			ctxView = new ContextMenu(new MenuItem[] {menuShowView, menuViewSchema, menuScriptView, menuDropView});
			this.lstViews.ContextMenu = ctxView;
		}
		
		
		internal void LoadQueryResults() {
			this.LoadQueryResults(this.Query);
		}
		
		
		internal void LoadDataTable(DataTable dt) {
			if (dt != null) {
				txtResults.Text = dba.DataTable2CSV(dt);
				gridResults.DataSource = dt;
			}
			// Assume that if no rows were returned, then the schema was altered.
			else {
				DisplayDataSource();
			}
		}
		
		
		internal void LoadQueryResults(string SQL) {
			System.Data.DataTable dt;
			
			//Don't do anything if the query window is empty or we are not connected to a database.
			if (SQL == "" || dbcon == null) { return; }
			
			try {
				dt = dbcon.ExecuteSql(SQL);
			} catch (System.Data.Common.DbException ex) {
				MessageBox.Show
					(String.Format("Problem loading query {0}\r\nError Message: {1}", SQL, ex.Message));
				return;
			}
			
			LoadDataTable(dt);
		}
		
		
		private void LoadTableResults(string Table) {
			System.Data.DataTable dt;
			
			//Don't do anything if we are not connected to a database or no table is specified.
			if (Table == "" || dbcon == null) { return; }
			
			try {
				dt = dbcon.GetTableAsDataTable(Table);
			} catch (System.Data.Common.DbException ex) {
				MessageBox.Show
					(String.Format("Problem loading table {0}\r\nError Message: {1}", Table, ex.Message));
				return;
			}
			
			LoadDataTable(dt);
		}
		
		
		internal void NewDatabaseFile(string FileName) {
			string Extension = Path.GetExtension(FileName);
			switch (Extension) {
				case ".mdb":
						JetSqlUtil.CreateMDB(FileName);
						this.OpenMDB(FileName);
						break;
				case ".db":
				case ".db3":
				case ".sqlite":
						System.Data.SQLite.SQLiteConnection.CreateFile(FileName);
						this.OpenSQLite(FileName);
						break;
				}
				this.Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(FileName), FileName);
		}
		
		
		internal void OpenDatabaseFile (string FileName) {
			try {
				this.DisconnectDataSource(); 
			} catch (NullReferenceException) {}
			
			string Extension =
				System.IO.Path.GetExtension(FileName).ToLower();
			if (Extension == ".mdb" || Extension == ".mde") {
				this.OpenMDB(FileName);
			} else if (Extension == ".db" || Extension == ".db3" || Extension == ".sqlite") {
				this.OpenSQLite(FileName);
			} else {throw new ApplicationException("Unknown file type.");}
			AddRecentFile(FileName);
			GenerateOpenRecentMenu();
		}
		
		
		internal void OpenMDB (string FileName) {
			DialogResult Result;
			
			this.dbcon = new OleDba();
			
			try {
				((OleDba) dbcon).ConnectMDB(FileName);
			} catch (OleDbException ex) {
				//TODO: this is the error code for incorrect access password. Make this a constant.
				if (ex.ErrorCode == -2147217843) {
					InputDialog GetPassword = new InputDialog();
					Result = GetPassword.ShowDialog("Enter the password for the database");
					if (Result == DialogResult.OK) {
						try {
							((OleDba) dbcon).ConnectMDB(FileName, GetPassword.Input);
						} catch (OleDbException exSecond) {
							if (exSecond.ErrorCode == -2147217843) {
								MessageBox.Show("Incorrect Password");
							} else {
								throw exSecond;
							}
							return;
						} finally { GetPassword.Dispose(); }
					} 
				}
				else { 
					throw ex;
				}
			} 
			this.DisplayDataSource();
		}
		
		
		internal void OpenSQLite (string FileName) {
			this.dbcon = new SQLiteDba();
			
			((SQLiteDba) dbcon).Connect(FileName);
			this.DisplayDataSource();
		}
		
		
		/// <summary>
		/// Process keypresses.
		/// </summary>
		/// <param name="msg">
		/// The window message that represents the keypress.
		/// </param>
		/// <param name="keyData">The kepress performed.</param>
		/// <returns></returns>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (Control.FromHandle(msg.HWnd) is ListBox) {
				if ((keyData & Keys.Enter) == Keys.Enter) {
					ListBox lst = (ListBox) ListBox.FromHandle(msg.HWnd);
					if (lst.Name != "lstColumns") {
						this.LoadTableResults(lst.Text);
					}
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
