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


using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
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
		private PlaneDisasterSettings _planeDisasterSettings;
		private dba _dbcon = null;
		ContextMenu ctxProcedure, ctxTable, ctxView;
		private string _CSV;
		private string _InsertStatements;
		
		#region Properties
		
		/// <summary>The Results of the query in CSV format.</summary>
		private string CSV {
			get { return _CSV; }
			set { _CSV = value; }
		}
		
		
		/// <summary>The Results of the query in InsertStatements format.</summary>
		private string InsertStatements {
			get { return _InsertStatements; }
			set { _InsertStatements = value; }
		}
		
		
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
			get { return txtSQL.Text; }
			set { txtSQL.Text = value; }
		}
		
		#endregion
		
		/// <summary>
		/// Default constructor.
		/// </summary>
		internal MainForm()
		{
			InitializeComponent();
			
			gridResults.DataError += this.EvtDataGridError;
			
			_planeDisasterSettings = PlaneDisasterSettings.GetSection(ConfigurationUserLevel.PerUserRoamingAndLocal);
			_planeDisasterSettings.RecentFiles.GenerateOpenRecentMenu
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
			dlg.Filter = "Comma Seperated Value (*.csv)|*.csv|All Files|";
			
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
			dlg.Filter = "SQL Scripts (*.sql)|*.sql|All Files|";
			
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
			// The List boxes
			this.lstColumns.Top = this.ClientSize.Height - 65;
			// Radio Buttons
			this.radGrid.Top = this.ClientSize.Height -85;
			this.radCSV.Top = this.ClientSize.Height -65;
			this.radInsert.Top = this.ClientSize.Height -45;
			
			this.ResumeLayout();
		}
		
		#endregion

		
		#region ListBox Events
		
		private void DisplayTable(string tableName) {
			try {
				long RowCount = _dbcon.GetTableRowCount(tableName);
				if (RowCount > this.MaxRowDisplayCount) {
					string Message;
					string SQL;
					
					if (this._dbcon is OdbcDba) {
						SQL = String.Format
							("SELECT TOP {1} * FROM {0}",
							 tableName, MaxRowDisplayCount);
					} else {
						SQL = String.Format
							("SELECT * FROM {0} LIMIT {1}",
							 tableName, MaxRowDisplayCount);
					}
					Message = String.Format
						("Row count is {0}. Displaying the first {1} rows.",
						 _dbcon.GetTableRowCount(tableName),
						 MaxRowDisplayCount);
					MessageBox.Show(Message, "Too Many Rows!");
					LoadQueryResults(SQL, tableName);
				} else {
					LoadTableResults(tableName);
				}
			}
			catch (DbException ex)
			{
				switch (ex.ErrorCode) {
					case -2147467259:
						string msg = string.Format("Connection error viewing database object {0}. Its possible that {0} is a reference to a database object in a remote database.", tableName);
						MessageBox.Show(msg);
						break;
					case -2147217865:
						MessageBox.Show(ex.Message, "Query Error");
						break;
					case -2147217904:
						MessageBox.Show
							(string.Format
							 ("Query {0} requires parameters. Please execute it with parameters via the SQL prompt.",
							  tableName));
						break;
					case -2147217911:
						MessageBox.Show(ex.Message, "Permission Error");
						break;
					default:
						throw ex;
				}
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
			string CurrentFile = GetFileName();
			string FileFilter = "Microsoft Access (*.mdb;*.mde)|*.mdb;*,mde";
			FileDialog dlg = new OpenFileDialog();
			dlg.Filter = FileFilter;
			
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
			// TODO: Figure out how to select the node from the menu.
			TreeNode node = treeObjects.SelectedNode;
			
			if (mnu.Name == "menuDropProcedure") {
				_dbcon.DropProcedure('[' + node.Text + ']');
			} else if (mnu.Name == "menuDropTable") {
				_dbcon.DropTable('[' + node.Text + ']');
			} else if (mnu.Name == "menuDropView") {
				_dbcon.DropView('[' + node.Text + ']');
			} else {
				throw new ArgumentException
					("sender for menuDrop_Click must be one of " +
					 "menuProcedures, menuTables, or menuViews.");
			}
			treeObjects.SelectedNode.Remove();
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
				AddRecentFile(dlg.FileName);
				_planeDisasterSettings.RecentFiles.GenerateOpenRecentMenu
					(openRecentToolStripMenuItem,
					 menuOpenRecent_Click);
				InitContextMenues();
			}
			dlg.Dispose();
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
				_planeDisasterSettings.RecentFiles.GenerateOpenRecentMenu
					(openRecentToolStripMenuItem,
					 menuOpenRecent_Click);
				InitContextMenues();
			}
			dlg.Dispose();
		}
		
		
		void menuOpenRecent_Click (object sender, System.EventArgs e) {
			ToolStripItem menuItem = (ToolStripItem)sender;
			string fileName = Path.GetFullPath(menuItem.Text);
			
			if (File.Exists(fileName))
			{
				OpenDatabaseFile(fileName);
				InitContextMenues();
			}
			else
			{
				menuItem.Owner.Items.Remove(menuItem);
				menuItem.Dispose();
				
				_planeDisasterSettings.RecentFiles.Remove(fileName);
				_planeDisasterSettings.Save();
				
				string msg = 
					string.Format("File {0} no longer exists. Removing from Open Recent list.", fileName);
				MessageBox.Show(msg);
			}
		}


		void menuRepairDatabase_Click (object sender, System.EventArgs e)
		{
			string CurrentFile = GetFileName();
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new OpenFileDialog();
			FileFilter.Append("Microsoft Access (*.mdb)|*.mdb");
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
			// TODO: Figure out how to select the node from the menu.
			TreeNode node = treeObjects.SelectedNode;
			
			if (mnu.Name == "menuProcedureSchema") {
				gridResults.DataSource = _dbcon.GetColumnSchema(node.Text);
			} else if (mnu.Name == "menuTableSchema") {
				gridResults.DataSource = _dbcon.GetColumnSchema(node.Text);
			} else if (mnu.Name == "menuViewSchema") {
				gridResults.DataSource = _dbcon.GetColumnSchema(node.Text);
			} else {
				throw new ArgumentException
					("sender for menu_Click must be one of " +
					 "menuProcedureSchema, menuTableSchema, or menuViewSchema.");
			}
		}
		
		
		void menuScript_Click (object sender, System.EventArgs e) {
			MenuItem mnu = (MenuItem)sender;
			// TODO: Figure out how to select the node from the menu.
			TreeNode node = treeObjects.SelectedNode;
			
			if (mnu.Name == "menuScriptProcedure") {
				Query = _dbcon.GetProcedureSQL(node.Text);
			} else if (mnu.Name == "menuScriptTable") {
				Query = ((SQLiteDba)_dbcon).GetTableSQL(node.Text);
			} else if (mnu.Name == "menuScriptView") {
				Query = _dbcon.GetViewSQL(node.Text);
			}
		}
		
		
		void menuShow_Click (object sender, EventArgs e) {
			MenuItem mnu = (MenuItem) sender;
			TreeNode treeNode = treeObjects.SelectedNode;
			
			if (mnu.Name == "menuShowProcedure")
			{
				DisplayTable(treeNode.Text);
			}
			else if (mnu.Name == "menuShowTable")
			{
				DisplayTable(treeNode.Text);
			}
			else if (mnu.Name == "menuShowView")
			{
				DisplayTable(treeNode.Text);
			}
			else
			{
				throw new ArgumentException
					("Sender for menuShow_Click must be one of " +
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
			
			_planeDisasterSettings.RecentFiles.Add(FileName);
			_planeDisasterSettings.Save();
		}
		
		
		/// <summary>
		/// Disconnects from the data source and updates the GUI appropiatly.
		/// </summary>
		internal void DisconnectDataSource() {
			
			/*
			 * We must clear this last otherwise, events firing from
			 * the first three might repopulate this.
			 */
			lstColumns.DataSource = null;
			
			lstColumns.ContextMenu = null;
			
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
		
		
		/// <summary>
		/// Gets the file name of the currently open database.
		/// </summary>
		/// <returns>The file name of the currently open database.</returns>
		internal string GetFileName() {
			DbConnectionStringBuilder ConStr;
			
			if (_dbcon is OleDba) {
				ConStr = new OleDbConnectionStringBuilder(((OleDba)_dbcon).ConnectionString);
				//For some reason FileName is blank.
				return (string)((OleDbConnectionStringBuilder)ConStr)["Dbq"];
			} else if (_dbcon is SQLiteDba) {
				ConStr = new SQLiteConnectionStringBuilder(((SQLiteDba)_dbcon).ConnectionString);
				return ((SQLiteConnectionStringBuilder)ConStr).DataSource;
			} else { return ""; }
		}
		
		
		private void InitContextMenues () {
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
			menuShowProcedure.Click += new EventHandler(menuShow_Click);
			menuShowProcedure.Name = "menuShowProcedure";
			
			ctxProcedure = new ContextMenu(new MenuItem[] {menuShowProcedure, menuScriptProcedure, menuDropProcedure});
			
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
			InitDbObjTree();
		}
		
		
		internal void InitDbObjTree()
		{
			if (!this._dbcon.Connected) {
				return;
			}
			
			TreeNode [] nodes = new TreeNode [] {
				new TreeNode("Tables"),
				new TreeNode("Views"),
				new TreeNode("Procedures")
			};
			
			nodes[0].Name = "Tables";
			nodes[1].Name = "Views";
			nodes[2].Name = "Procedures";
			
			treeObjects.Nodes.Clear();
			treeObjects.Nodes.AddRange(nodes);
			foreach (string tableName in _dbcon.GetTables())
			{
				TreeNode node = new TreeNode(tableName);
				node.ContextMenu = this.ctxTable;
				treeObjects.Nodes["Tables"].Nodes.Add(node);
			}
			foreach (string viewName in _dbcon.GetViews())
			{
				TreeNode node = new TreeNode(viewName);
				node.ContextMenu = this.ctxView;
				treeObjects.Nodes["Views"].Nodes.Add(node);
			}
			foreach (string procedureName in _dbcon.GetProcedures())
			{
				TreeNode node = new TreeNode(procedureName);
				node.ContextMenu = this.ctxProcedure;
				treeObjects.Nodes["Procedures"].Nodes.Add(node);
			}
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
			Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(FileName), FileName);
		}
		
		
		internal void OpenDatabaseFile (string FileName) {
			try {
				this.DisconnectDataSource();
			} catch (NullReferenceException) {}
			
			string Extension =
				System.IO.Path.GetExtension(FileName).ToLower();
			if (Extension == ".mdb" || Extension == ".mde") {
				OpenMDB(FileName);
			} else if (Extension == ".db" || Extension == ".db3" || Extension == ".sqlite") {
				OpenSQLite(FileName);
			} else {throw new ApplicationException("Unknown file type.");}
			AddRecentFile(FileName);
			_planeDisasterSettings.RecentFiles.GenerateOpenRecentMenu
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
					string Msg = String.Format("File '{0}' not found.", FileName);
					MessageBox.Show(Msg, "Error Opening File");
					return;
				} else {
					throw ex;
				}
			}
			Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(FileName), FileName);
			this.DisplayDataSource();
		}
		
		
		internal void OpenSQLite (string FileName) {
			this._dbcon = new SQLiteDba();
			
			((SQLiteDba) _dbcon).Connect(FileName);
			Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(FileName), FileName);
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
