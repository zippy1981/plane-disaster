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

 
using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PlaneDisaster
{
	/// <summary>
	/// PlaneDisaster.NET main form.
	/// </summary>
	public sealed partial class MainForm
	{
		
		private dba dbcon = null;
		MainFormEvents events;
				
		#region Properties
		
		/// <summary>The Results of the query in CSV format.</summary>
		internal string CSV {
			get { return this.txtResults.Text; }
			set { this.txtResults.Text = value; }
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
		private MainForm()
		{
			events = new MainFormEvents();
			InitializeComponent();
			
			/* ListBox Double Click event handlers */
			lstColumns.DoubleClick += new System.EventHandler(events.lst_DblClick);
			lstProcedures.DoubleClick += new System.EventHandler(events.lst_DblClick);
			lstTables.DoubleClick += new System.EventHandler(events.lst_DblClick);
			lstViews.DoubleClick += new System.EventHandler(events.lst_DblClick);
			
			/* ListBox Right Click event handlers */
			lstProcedures.MouseDown += new MouseEventHandler(events.ListBox_RightClickSelect);
			lstTables.MouseDown += new MouseEventHandler(events.ListBox_RightClickSelect);
			lstViews.MouseDown += new MouseEventHandler(events.ListBox_RightClickSelect);
			
			gridResults.DataError += new DataGridViewDataErrorEventHandler(events.EvtDataGridError);
			
			//TODO: figure out the event fired whe enter is pressed. Its not Enter
		}


		/// <summary>
		/// Program entry point.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		[STAThread]
		public static void Main(string[] args)
		{
			MainForm frm = new MainForm();
			/*
			 * From the infalliable tomes of the msdn http://msdn2.microsoft.com/en-us/library/acy3edy3.aspx
			 *    Unlike C and C++, the name of the program is not treated as the first command line argument.
			 *    WHY WHY WHY!!!!!! Lets just have 1 bases arrays.
			 */
			if (args.Length > 0) {
				string FileName = args[0];
				if (File.Exists(FileName)) {
					frm.OpenDatabaseFile(FileName);
					frm.Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(FileName), FileName);
				} else if (Directory.Exists(Path.GetDirectoryName(FileName))) {
					frm.NewDatabaseFile(FileName);
				} else {
					MessageBox.Show(String.Format("File {0} is not a real file.", FileName));
				}
			}
			Application.Run(frm);
		}

		#region Events
		
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
		
		
		/* 
		 * I am having trouble getting the container form from a toolstrip. Other wise
		 * these menu items could be ported over.
		 */
		#region Migrated Menu Events
		
		internal void menuAbout_Click (object sender, System.EventArgs e) {
			//TODO: write a proper about box
			string Msg;
			
			Msg = "PlaneDisaster.NET database viewer\nCopyright 2006 Justin Dearing\nzippy1981@gmail.com";
			MessageBox.Show(Msg, "About PlaneDisaster.NET");
		}
		
		
		internal void menuClose_Click(object sender, System.EventArgs e)
		{
			GetMainForm(sender).DisconnectDataSource();
		}
		
		
		internal void menuCompactDatabase_Click (object sender, System.EventArgs e)
		{
			string CurrentFile = GetMainForm(sender).GetFileName();
			string FileFilter = "Microsoft Access (*.mdb)|*.mdb";
			FileDialog dlg = new OpenFileDialog();
			dlg.Filter = FileFilter;
			
			if (dlg.ShowDialog() == DialogResult.OK) {
				if (dlg.FileName == CurrentFile) {
					GetMainForm(sender).DisconnectDataSource();
					JetSqlUtil.CompactMDB(dlg.FileName);
					GetMainForm(sender).OpenMDB(CurrentFile);
				} else { 
					JetSqlUtil.CompactMDB(dlg.FileName); }
			}
			dlg.Dispose();
		}

		
		internal void menuExit_Click (object sender, System.EventArgs e) {
			GetMainForm(sender).Close();
		}
		
		
		internal void menuNew_Click (object sender, System.EventArgs e)
		{
			MainForm frm = GetMainForm(((ToolStripItem)sender).Owner);
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new SaveFileDialog();
			dlg.Title = "New Database";
			FileFilter.Append("Microsoft Access (*.mdb)|*.mdb");
			FileFilter.Append("|SQLite3 (*.db;*.db3;*.sqlite)|*.db;*.db3;*.sqlite");
			dlg.Filter = FileFilter.ToString();
			
			if (dlg.ShowDialog() == DialogResult.OK) {
				switch (dlg.FilterIndex) {
					case 1:
						JetSqlUtil.CreateMDB(dlg.FileName);
						frm.OpenMDB(dlg.FileName);
						break;
					case 2:
						System.Data.SQLite.SQLiteConnection.CreateFile
							(dlg.FileName);
						frm.OpenSQLite(dlg.FileName);
						break;
				}
				frm.Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(dlg.FileName), dlg.FileName);
			}
			dlg.Dispose();
			frm.InitContextMenues();
		}

		
		internal void menuOpen_Click (object sender, System.EventArgs e) {
			MainForm frm = GetMainForm(sender);
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new OpenFileDialog();
			FileFilter.Append("All supported database types|*.mdb;*.db;*.db3;*.sqlite");
			FileFilter.Append("|Microsoft Access (*.mdb)|*.mdb");
			FileFilter.Append("|SQLite3 (*.db;*.db3;*.sqlite)|*.db;*.db3;*.sqlite");
			dlg.Filter = FileFilter.ToString();
			
			if(dlg.ShowDialog() == DialogResult.OK) {
				switch (dlg.FilterIndex) {
					case 1:
						string Extension = 
							System.IO.Path.GetExtension(dlg.FileName).ToLower();
						if (Extension == ".mdb") {
							frm.OpenMDB(dlg.FileName);
						} else if (Extension == ".db" || Extension == ".db3" || Extension == ".sqlite") {
							frm.OpenSQLite(dlg.FileName);
						} else {throw new ApplicationException("Unknown file type.");}
						break;
					case 2:
						frm.OpenMDB(dlg.FileName);
						break;
					case 3:
						frm.OpenSQLite(dlg.FileName);
						break;
				}
				frm.Text = string.Format("{0} - ({1}) - PlaneDisaster.NET", System.IO.Path.GetFileName(dlg.FileName), dlg.FileName);
			}
			dlg.Dispose();
			frm.InitContextMenues();
		}


		internal void menuRepairDatabase_Click (object sender, System.EventArgs e)
		{
			MainForm frm = GetMainForm(sender);
			string CurrentFile = frm.GetFileName();
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new OpenFileDialog();
			FileFilter.Append("Microsoft Access (*.mdb)|*.mdb");
			dlg.Filter = FileFilter.ToString();
			
			if (dlg.ShowDialog() == DialogResult.OK) {
				if (dlg.FileName == CurrentFile) {
					frm.DisconnectDataSource();
					JetSqlUtil.RepairMDB(dlg.FileName);
					frm.OpenMDB(CurrentFile);
				} else { 
					JetSqlUtil.RepairMDB(dlg.FileName); }
			}
			dlg.Dispose();
		}
		
		#endregion

		
		#region Unmigrated Menu Events	
		
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
		
		
		internal void menuSchema_Click (object sender, System.EventArgs e) {
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
				events.lst_DblClick(lstProcedures, e);
			} else if (mnu.Name == "menuShowTable") {
				events.lst_DblClick(lstTables, e);
			} else if (mnu.Name == "menuShowView") {
				events.lst_DblClick(lstViews, e);
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
		
		
		internal string GetDatabaseStatus() {
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
		
		
		[Obsolete("This is a workaround until I can fing the owning form of a ToolStripItem")]
		private MainForm GetMainForm(object oControl) {
			return this;
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
			
			if (dt != null) {
				txtResults.Text = dba.DataTable2CSV(dt);
				gridResults.DataSource = dt;
			}
			// Assume that if no rows were returned, then the schema was altered.
			else {
				DisplayDataSource();
			}
		}
		
		
		internal void LoadTableResults(string Table) {
			//TODO: Scrub or escape this table name
			this.LoadQueryResults(String.Format("SELECT * FROM [{0}]", Table));
		}
		
		
		private void NewDatabaseFile(string FileName) {
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
			if (Extension == ".mdb") {
				this.OpenMDB(FileName);
			} else if (Extension == ".db" || Extension == ".db3" || Extension == ".sqlite") {
				this.OpenSQLite(FileName);
			} else {throw new ApplicationException("Unknown file type.");}
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
