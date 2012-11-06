/* 
 * Copyright 2006-2012 Justin Dearing
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
using System.IO;
using System.Text;
using System.Windows.Forms;

using PlaneDisaster.Configuration;
using PlaneDisaster.Dba;

namespace PlaneDisaster
{
	/// <summary>
	/// PlaneDisaster.NET main form.
	/// </summary>
	public sealed partial class MainForm
	{
		private PlaneDisasterSettings _oPlaneDisasterSettings;
		private dba _dbcon = null;

        private const string FILTER_ALL_FILES = "All Files|*.*";
        private const string FILTER_ALL_DBFORMATS = "All supported database types|*.accdb;*.mdb;*.mde;*.db;*.db3;*.sqlite";
        private const string FILTER_CSV = "Comma Seperated Value (*.csv)|*.csv";
        private const string FILTER_JETSQL = "Microsoft Access (*.accdb;*.mdb;*.mde)|*.accdb;*.mdb;*.mde";
        private const string FILTER_SQLITE = "SQLite3 (*.db;*.db3;*.sqlite)|*.db;*.db3;*.sqlite";
        private const string FILTER_SQL_SCRIPTS = "SQL Scripts (*.sql)|*.sql";

        private readonly List<string> _JetSqlExtensions = new List<string>
	        {
	            ".accdb",
                ".mde",
                ".mdb"
	        };
        private readonly List<string> _SqliteExtensions = new List<string>
	        {
	            ".db",
                ".db3",
                ".sqlite"
	        };
		
		#region Properties

        private string CurrentFile { get; set; }
		
		/// <summary>The Results of the query in CSV format.</summary>
        private string CSV { get; set; }
		
		
		/// <summary>The Results of the query in InsertStatements format.</summary>
        private string InsertStatements { get; set; }
		
		
		/// <summary>
		/// The maximum rows to display in the query window.
		/// This only applies to tables and views, not custom queries.
		/// </summary>
		private int MaxRowDisplayCount {
			//TODO make this number configurable.
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
			
			/* ListBox Right Click event handlers */
			lstProcedures.MouseDown += new MouseEventHandler(this.Lst_RightClickSelect);
			lstTables.MouseDown += new MouseEventHandler(this.Lst_RightClickSelect);
			lstViews.MouseDown += new MouseEventHandler(this.Lst_RightClickSelect);
			
			gridResults.DataError += new DataGridViewDataErrorEventHandler(this.EvtDataGridError);
			
			_oPlaneDisasterSettings = PlaneDisasterSettings.GetSection(ConfigurationUserLevel.PerUserRoamingAndLocal);
			_oPlaneDisasterSettings.RecentFiles.GenerateOpenRecentMenu
				(openRecentToolStripMenuItem,
				 menuOpenRecent_Click);
		}

		#region Events
		
		#region Button Events
		
		void CmdRefreshClick(object sender, EventArgs e)
		{
			DisplayDataSource();
		}
		
		
		void CmdSaveCsvClick(object sender, System.EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			string FileName;
			dlg.Filter = FILTER_CSV + "|" + FILTER_ALL_FILES;
			
			if(dlg.ShowDialog() == DialogResult.OK ) {
				FileName = dlg.FileName;
				using (StreamWriter sw = File.CreateText(FileName))
				{
					sw.Write(CSV);
				}
			}
		}
		
		
		void CmdSavSqlClick(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			string FileName;
			dlg.Filter = FILTER_SQL_SCRIPTS;
			
			if(dlg.ShowDialog() == DialogResult.OK ) {
				FileName = dlg.FileName;
				using (StreamWriter sw = File.CreateText(FileName))
				{
					sw.Write(InsertStatements);
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
			this.radCSV.Top = this.ClientSize.Height -65;
			this.radInsert.Top = this.ClientSize.Height -45;
			
			this.ResumeLayout();
		}
		
		#endregion

		
		#region ListBox Events
		
		void lst_DblClick(object sender, EventArgs  e) {
			ListBox lst = (ListBox) sender;
			
			if (lst.Name == "lstColumns") {
				//I dont know what the default action for the columm list should be
			} else if (lst.Text != ""){
				try {
					int RowCount = _dbcon.GetTableRowCount(lst.Text);
					if (RowCount > this.MaxRowDisplayCount) {
						string Message;
						string SQL;
						
                        //TODO: Make ths not suck
						if (this._dbcon.IsAccessDatabase) {
							SQL = String.Format
								("SELECT TOP {1} * FROM [{0}]",
								 lst.Text, MaxRowDisplayCount);
						} else {
							SQL = String.Format
								("SELECT * FROM [{0}] LIMIT {1}",
								 lst.Text, MaxRowDisplayCount);
						}
						Message = String.Format
							("Row count is {0}. Displaying the first {1} rows.",
							 _dbcon.GetTableRowCount(lst.Text),
							 MaxRowDisplayCount);
						MessageBox.Show(Message, "Too Many Rows!");
						LoadQueryResults(SQL, lst.Text);
					} else {
						LoadTableResults(lst.Text);
					}
				} catch (DbException ex) {
					switch (ex.ErrorCode) {
						case -2147217865:
							MessageBox.Show(ex.Message, "Query Error");
							break;
						case -2147217904:
							MessageBox.Show
								(String.Format
								 ("Query {0} requires parameters. Please execute it with parameters via the SQL prompt.",
								  lst.Text));
							break;
						case -2147217911:
							MessageBox.Show(ex.Message, "Permission Error");
							break;
						default:
							throw ex;
					}
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
				lstColumns.DataSource = _dbcon.GetColumnNames(lst.Text);
			}
			catch (System.Data.OleDb.OleDbException ex) {
				MessageBox.Show ("OleDbException: " + ex.Message + "\r\nCode: " + ex.ErrorCode);
			}
		}
		
		#endregion
		
		
		#region Menu Events
		
		void menuAbout_Click (object sender, System.EventArgs e) {
			//TODO: write a proper about box
			StringBuilder Msg = new StringBuilder();
			
			Msg.AppendFormat
				("{0} version {1}.\n",
				 Application.ProductName,
				 Application.ProductVersion);
			Msg.AppendLine
				("Copyright 2006 Justin Dearing <zippy1981@gmail.com>");
			MessageBox.Show(Msg.ToString(), "About PlaneDisaster.NET");
		}
		
		
		void menuClose_Click(object sender, System.EventArgs e)
		{
			DisconnectDataSource();
		}
		
		
		void menuCompactDatabase_Click (object sender, System.EventArgs e)
		{
			FileDialog dlg = new OpenFileDialog();
			dlg.Filter = FILTER_JETSQL;
			
			try {
				if (dlg.ShowDialog() == DialogResult.OK) {
					if (dlg.FileName == CurrentFile) {
						DisconnectDataSource();
						JetSqlUtil.CompactMDB(dlg.FileName);
						OpenMDB(CurrentFile);
					} else {
						JetSqlUtil.CompactMDB(dlg.FileName); }
				}
			} catch (ApplicationException) {
				StringBuilder ErrorMessage = new StringBuilder();
				ErrorMessage.AppendFormat
					("There seems to be a problem compacting {0}.\n", dlg.FileName);
				ErrorMessage.AppendLine
					("Perhaps the file is opened by another process.");
				MessageBox.Show(ErrorMessage.ToString(),
					 "PlaneDisaster.NET");
			}
			dlg.Dispose();
		}
		
		
		void menuDatabaseSchema_Click(object sender, System.EventArgs e)
		{
			this.gridResults.DataSource = _dbcon.GetSchema();
		}
		
		
		void menuDrop_Click (object sender, System.EventArgs e) {
			MenuItem mnu = (MenuItem) sender;
			
			if (mnu.Name == "menuDropProcedure") {
				_dbcon.DropProcedure('[' + (string) lstProcedures.SelectedItem + ']');
				lstProcedures.DataSource = _dbcon.GetProcedures();
			} else if (mnu.Name == "menuDropTable") {
				_dbcon.DropTable('[' + (string) lstTables.SelectedItem + ']');
				lstTables.DataSource = _dbcon.GetTables();
			} else if (mnu.Name == "menuDropView") {
				_dbcon.DropView('[' + (string) lstViews.SelectedItem + ']');
				lstViews.DataSource = _dbcon.GetViews();
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
			FileFilter.AppendFormat("|{0}", FILTER_SQLITE);
			dlg.Filter = FileFilter.ToString();
			
			if (dlg.ShowDialog() == DialogResult.OK) {
				switch (dlg.FilterIndex) {
					case 1:
						JetSqlUtil.CreateMDB(dlg.FileName);
						OpenMDB(dlg.FileName);
						break;
					case 2:
						JetSqlUtil.CreateMDB(dlg.FileName, AccessDbVersion.Access95);
						OpenMDB(dlg.FileName);
						break;
					case 3:
						JetSqlUtil.CreateMDB(dlg.FileName, AccessDbVersion.Access2000);
						OpenMDB(dlg.FileName);
						break;
					case 4:
						System.Data.SQLite.SQLiteConnection.CreateFile
							(dlg.FileName);
						OpenSQLite(dlg.FileName);
						break;
				}
				AddRecentFile(dlg.FileName);
				_oPlaneDisasterSettings.RecentFiles.GenerateOpenRecentMenu
					(openRecentToolStripMenuItem,
					 menuOpenRecent_Click);
				InitContextMenues();
			}
			dlg.Dispose();
		}

		
		void menuOpen_Click (object sender, System.EventArgs e) {
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new OpenFileDialog();
            FileFilter.AppendFormat("{0}|", FILTER_ALL_DBFORMATS);
            FileFilter.AppendFormat("{0}|", FILTER_JETSQL);
            FileFilter.AppendFormat("{0}|", FILTER_SQLITE);
            FileFilter.Append(FILTER_ALL_FILES);
			dlg.Filter = FileFilter.ToString();
			
			if(dlg.ShowDialog() == DialogResult.OK) {
				switch (dlg.FilterIndex) {
					case 1:
						string Extension =
							Path.GetExtension(dlg.FileName).ToLower();
                        if (_JetSqlExtensions.Contains(Extension))
                        {
							OpenMDB(dlg.FileName);
						} else if (_SqliteExtensions.Contains(Extension)) {
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
				_oPlaneDisasterSettings.RecentFiles.GenerateOpenRecentMenu
					(openRecentToolStripMenuItem,
					 menuOpenRecent_Click);
				InitContextMenues();
			}
			dlg.Dispose();
		}
		
		
		void menuOpenRecent_Click (object sender, System.EventArgs e) {
			string FileName = Path.GetFullPath(((ToolStripItem)sender).Text);
			
			OpenDatabaseFile(FileName);
			InitContextMenues();
		}


		void menuRepairDatabase_Click (object sender, System.EventArgs e)
		{
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new OpenFileDialog();
            FileFilter.Append(FILTER_JETSQL);
			dlg.Filter = FileFilter.ToString();
			
			try {
				if (dlg.ShowDialog() == DialogResult.OK) {
					if (dlg.FileName == CurrentFile) {
						DisconnectDataSource();
                        JetSqlUtil.RepairMDB(dlg.FileName);
						OpenMDB(CurrentFile);
					} else {
						JetSqlUtil.RepairMDB(dlg.FileName); }
				}
			} catch (ApplicationException) {
				StringBuilder ErrorMessage = new StringBuilder();
				ErrorMessage.AppendFormat
					("There seems to be a problem repairing {0}.\n", dlg.FileName);
				ErrorMessage.AppendLine
					("Perhaps the file is opened by another process.");
				MessageBox.Show
					(ErrorMessage.ToString(),
					 "PlaneDisaster.NET");
			}
			dlg.Dispose();
		}
		
		
		void menuSchema_Click (object sender, System.EventArgs e) {
			MenuItem mnu = (MenuItem) sender;
			
			if (mnu.Name == "menuProcedureSchema") {
				gridResults.DataSource = _dbcon.GetColumnSchema(lstProcedures.Text);
			} else if (mnu.Name == "menuTableSchema") {
				gridResults.DataSource = _dbcon.GetColumnSchema(lstTables.Text);
			} else if (mnu.Name == "menuViewSchema") {
				gridResults.DataSource = _dbcon.GetColumnSchema(lstViews.Text);
			} else {
				throw new ArgumentException
					("sender for menu_Click must be one of " +
					 "menuProcedures, menuTables, or menuViews.");
			}
		}
		
		
		void menuScript_Click (object sender, System.EventArgs e) {
			MenuItem mnu = (MenuItem) sender;
			
			if (mnu.Name == "menuScriptProcedure") {
				Query = _dbcon.GetProcedureSQL(lstProcedures.Text);
			} else if (mnu.Name == "menuScriptTable") {
				Query = ((SQLiteDba)_dbcon).GetTableSQL(lstTables.Text);
			} else if (mnu.Name == "menuScriptView") {
				Query = _dbcon.GetViewSQL(lstViews.Text);
			}
		}
		
		
		void menuShow_Click (object sender, EventArgs e) {
			MenuItem mnu = (MenuItem) sender;
			
			if (mnu.Name == "menuShowProcedure") {
				lst_DblClick(lstProcedures, e);
			} else if (mnu.Name == "menuShowTable") {
				lst_DblClick(lstTables, e);
			} else if (mnu.Name == "menuShowView") {
				lst_DblClick(lstViews, e);
			} else {
				throw new ArgumentException
					("sender for menuShow_Click must be one of " +
					 "menuProcedures, menuTables, or menuViews.");
			}
		}
		
		#endregion

		#region Radio Events
		

		void RadCSVCheckedChanged(object sender, System.EventArgs e)
		{
			gridResults.Hide();
			txtResults.Show();
			txtResults.Text = CSV;
		}
		

		void RadGridCheckedChanged(object sender, System.EventArgs e)
		{
			txtResults.Hide();
			gridResults.Show();
		}
		

		void RadInsertCheckedChanged(object sender, System.EventArgs e)
		{
			gridResults.Hide();
			txtResults.Show();
			txtResults.Text = InsertStatements;
		}
		

		#endregion

		#endregion
		

		private void AddRecentFile (string FileName) {
			FileName = Path.GetFullPath(FileName);
			
			_oPlaneDisasterSettings.RecentFiles.Add(FileName);
			_oPlaneDisasterSettings.Save();
		}
		
		
		private void ClearRecentFileMenu() {
			openRecentToolStripMenuItem.DropDownItems.Clear();
		}
		
		
		/// <summary>
		/// Disconnects from the data source and updates the GUI appropiatly.
		/// </summary>
		internal void DisconnectDataSource() {
			
			/* ListBox Double Click event handlers */
			lstColumns.DoubleClick -= lst_DblClick;
			lstProcedures.DoubleClick -= this.lst_DblClick;
			lstTables.DoubleClick -= lst_DblClick;
			lstViews.DoubleClick -= lst_DblClick;
			
			lstProcedures.DataSource = null;
			lstTables.DataSource = null;
			lstViews.DataSource = null;
			/*
			 * We must clear this last otherwise, events firing from
			 * the first three might repopulate this.
			 */
			lstColumns.DataSource = null;
			
			lstColumns.ContextMenu = null;
			lstProcedures.ContextMenu = null;
			lstTables.ContextMenu = null;
			lstViews.ContextMenu = null;
			
			txtResults.Text = "";
			CSV = "";
			gridResults.DataSource = null;
			
			databaseSchemaToolStripMenuItem.Enabled = false;
			_dbcon.Disconnect();
			_dbcon = null;
			Text = "PlaneDisaster.NET";
			databaseSchemaToolStripMenuItem.Enabled = false;
			closeToolStripMenuItem.Enabled = false;
			queryToolStripMenuItem.Enabled = false;
			cmdRefresh.Enabled = false;
		    CurrentFile = null;
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
			lstProcedures.DataSource = _dbcon.GetProcedures();
			lstTables.DataSource = _dbcon.GetTables();
			lstViews.DataSource = _dbcon.GetViews();
			
			txtResults.Text = "";
			gridResults.DataSource = null;
			
			databaseSchemaToolStripMenuItem.Enabled = true;
			this.closeToolStripMenuItem.Enabled = true;
			queryToolStripMenuItem.Enabled = true;
			cmdRefresh.Enabled = true;
		}
		
		
		private string GetDatabaseStatus() {
			return _dbcon.GetStatus();
		}
		
		
		internal void InitContextMenues () {
			ContextMenu ctxProcedure, ctxTable, ctxView;
			MenuItem menuDropProcedure, menuDropTable, menuDropView;
			MenuItem menuScriptProcedure, menuScriptTable, menuScriptView;
			MenuItem menuShowProcedure, menuShowTable, menuShowView;
			MenuItem menuTableSchema, menuViewSchema;
			
			if (!(_dbcon is SQLiteDba)) {	
				lstProcedures.DoubleClick += new EventHandler(this.lst_DblClick);
				
				menuDropProcedure = new MenuItem("Drop");
				menuDropProcedure.Click += new System.EventHandler(menuDrop_Click);
				menuDropProcedure.Name = "menuDropProcedure";
				
				menuScriptProcedure = new MenuItem("Script");
				menuScriptProcedure.Click += new System.EventHandler(menuScript_Click);
				menuScriptProcedure.Name = "menuScriptProcedure";
				
				menuShowProcedure = new MenuItem("Show");
				menuShowProcedure.Click += new EventHandler(menuShow_Click);
				menuShowProcedure.Name = "menuShowProcedure";
				
				ctxProcedure = new ContextMenu(new MenuItem[] {menuShowProcedure, menuScriptProcedure, menuDropProcedure});
				this.lstProcedures.ContextMenu = ctxProcedure;
			}
			
			lstTables.DoubleClick += new EventHandler(this.lst_DblClick);
			
			menuDropTable = new MenuItem("Drop");
			menuDropTable.Click += new System.EventHandler(menuDrop_Click);
			menuDropTable.Name = "menuDropTable";
			
			menuScriptTable = new MenuItem("Script");
			menuScriptTable.Click += new System.EventHandler(menuScript_Click);
			menuScriptTable.Name = "menuScriptTable";
			
			menuShowTable = new MenuItem("Show");
			menuShowTable.Click += new EventHandler(menuShow_Click);
			menuShowTable.Name = "menuShowTable";
			
			menuTableSchema = new MenuItem("Schema");
			menuTableSchema.Click += new System.EventHandler(this.menuSchema_Click);
			menuTableSchema.Name = "menuTableSchema";
			
			if (_dbcon is SQLiteDba) {
				ctxTable = new ContextMenu(new MenuItem[] {menuShowTable, menuScriptTable, menuTableSchema, menuDropTable});
			} else {
				ctxTable = new ContextMenu(new MenuItem[] {menuShowTable, menuTableSchema, menuDropTable});
			}
			this.lstTables.ContextMenu = ctxTable;
			
			lstViews.DoubleClick += new EventHandler(this.lst_DblClick);
			
			menuDropView = new MenuItem("Drop");
			menuDropView.Click += new System.EventHandler(menuDrop_Click);
			menuDropView.Name = "menuDropView";

			menuScriptView = new MenuItem("Script");
			menuScriptView.Click += new System.EventHandler(menuScript_Click);
			menuScriptView.Name = "menuScriptView";
			
			menuShowView = new MenuItem("Show");
			menuShowView.Click += new EventHandler(menuShow_Click);
			menuShowView.Name = "menuShowView";
			
			menuViewSchema = new MenuItem("Schema");
			menuViewSchema.Click += new System.EventHandler(this.menuSchema_Click);
			menuViewSchema.Name = "menuViewSchema";

			ctxView = new ContextMenu(new MenuItem[] {menuShowView, menuViewSchema, menuScriptView, menuDropView});
			this.lstViews.ContextMenu = ctxView;
		}
		
		
		internal void LoadDataTable(DataTable dt) {
			if (dt != null) {
				this.CSV = dba.DataTable2CSV(dt);
				this.InsertStatements = dba.DataTable2DML(dt);
				gridResults.DataSource = dt;
			}
			// Assume that if no rows were returned, then the schema was altered.
			else {
				DisplayDataSource();
				this.CSV = null;
				this.InsertStatements = null;
			}
			if (radCSV.Checked) {
				this.txtResults.Text = CSV;
			} else if (radInsert.Checked) {
				this.txtResults.Text = InsertStatements;
			}
		}
		
		
		private void LoadQueryResults() {
			if (txtSQL.SelectionLength > 1) {
				LoadQueryResults(txtSQL.SelectedText, null);
			} else {
				LoadQueryResults(txtSQL.Text, null);
			}
		}
		
		
		private void LoadQueryResults(string SQL, string TableName) {
			System.Data.DataTable dt;
			
			/* 
			 * Don't do anything if the query window is empty or we
			 * are not connected to a database.
			 */
			if (SQL == "" || _dbcon == null) { return; }
			
			try {
				dt = _dbcon.ExecuteScript(SQL);
				if (TableName != null ) {
					dt.TableName = TableName;
				}
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
			if (Table == "" || _dbcon == null) { return; }
			
			try {
				dt = _dbcon.GetTableAsDataTable(Table);
			} catch (System.Data.Common.DbException ex) {
				MessageBox.Show
					(String.Format("Problem loading table {0}\r\nError Message: {1}", Table, ex.Message));
				return;
			}
			
			LoadDataTable(dt);
		}
		
		
		internal void NewDatabaseFile(string FileName) {
			string Extension = Path.GetExtension(FileName);
            //TODO: Make use of _JetSqlExtensions and _SqliteExtensions
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
			this.queryToolStripMenuItem.Enabled = true;
			Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", Path.GetFileName(FileName), FileName);
		}
		
		
		internal void OpenDatabaseFile (string FileName) {
			try {
				this.DisconnectDataSource();
			} catch (NullReferenceException) {}
			
			string Extension = Path.GetExtension(FileName).ToLower();
			if (_JetSqlExtensions.Contains(Extension)) {
				OpenMDB(FileName);
			} else if (_SqliteExtensions.Contains(Extension)) {
				OpenSQLite(FileName);
			} else {throw new ApplicationException("Unknown file type.");}
			AddRecentFile(FileName);
			_oPlaneDisasterSettings.RecentFiles.GenerateOpenRecentMenu
				(openRecentToolStripMenuItem,
				 menuOpenRecent_Click);
			this.queryToolStripMenuItem.Enabled = true;
		}
		
		
 		internal void OpenMDB (string FileName) {
 			DialogResult Result;
 			
			this._dbcon = new OleDba();
			
			try {
				((OleDba) _dbcon).ConnectMDB(FileName);
			} catch (OleDbException ex) {
				//TODO: this is the error code for incorrect access password. Make this a constant.
				if (ex.ErrorCode == -2147217843) {
					InputDialog GetPassword = new InputDialog();
					Result = GetPassword.ShowDialog("Enter the password for the database");
					if (Result == DialogResult.OK) {
						try {
							((OleDba) _dbcon).ConnectMDB(FileName, GetPassword.Input);
						} catch (OleDbException exSecond) {
							if (exSecond.ErrorCode == -2147217843) {
								MessageBox.Show("Incorrect Password");
							} else {
								throw exSecond;
							}
							return;
						} finally { GetPassword.Dispose(); }
					}
				} else if (ex.ErrorCode == -2147467259) {
					Text = "PlaneDisaster.NET";
                    //TODO: Apparently this error code is also returned if you try to open an .accdb with the Jet 4.0 driver
					string Msg = String.Format("File [{0}] not found.", FileName);
					MessageBox.Show(Msg, "Error Opening File");
					return;
				} else {
					throw ex;
				}
			}
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error Opening database");
                return;
            }
			Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", Path.GetFileName(FileName), FileName);
 		    CurrentFile = FileName;
			this.DisplayDataSource();
		}
		
		
		internal void OpenSQLite (string FileName) {
			this._dbcon = new SQLiteDba();
			
			((SQLiteDba) _dbcon).Connect(FileName);
			Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(FileName), FileName);

            CurrentFile = FileName;
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
