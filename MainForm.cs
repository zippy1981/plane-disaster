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
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
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
		private bool _Connected = false;
		
		#region Properties
		
		private bool Connected {
			get { return _Connected; }
			set { _Connected = value; }
		}
		
		
		/// <summary>The Results of the query in CSV format.</summary>
		private string CSV {
			get { return this.txtResults.Text; }
		}
		
		#endregion
		
		/// <summary>
		/// Default constructor.
		/// </summary>
		private MainForm()
		{
			InitializeComponent();
			
			/* ListBox Double Click event handlers */
			lstProcedures.DoubleClick += new System.EventHandler(this.lst_DblClick);
			lstTables.DoubleClick += new System.EventHandler(this.lst_DblClick);
			lstViews.DoubleClick += new System.EventHandler(this.lst_DblClick);
			
			/* ListBox Right Click event handlers */
			lstProcedures.MouseDown += new MouseEventHandler(this.ListBox_RightClickSelect);
			lstTables.MouseDown += new MouseEventHandler(this.ListBox_RightClickSelect);
			lstViews.MouseDown += new MouseEventHandler(this.ListBox_RightClickSelect);
			
			//TODO: figure out the event fired whe enter is pressed. Its not Enter
		}


		/// <summary>
		/// Program entry point.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		[STAThread]
		public static void Main(string[] args)
		{
			Application.Run(new MainForm());
		}

		
		
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
			LoadQueryResults(this.txtSQL.Text);
		}

		
		void CmdStatusClick(object sender, System.EventArgs e)
		{
			MessageBox.Show (dbcon.GetStatus());
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
			LoadTableResults(lst.Text);	
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
		
		
		/// <summary>
		/// If you want to be able to select an item in your listbox via right click,
		/// add this function as an event handler to the listbox's mousedown event.
		/// </summary>
		/// <param name="sender">The listbox being right clicked.</param>
		/// <param name="e">The MouseEventArgs object.</param>
		void ListBox_RightClickSelect(object sender, MouseEventArgs  e) {
			if (e.Button == MouseButtons.Right) {
				ListBox lst = (ListBox) sender;
				int Index = lst.IndexFromPoint(e.X, e.Y);
			
				if (Index >= 0 && Index < lst.Items.Count) {
            	    lst.SelectedIndex = Index;
            	}
            	lst.Refresh();
			}
		}
		
		#endregion			

		
		#region Menu Events	
		
		private void menuAbout_Click (object sender, System.EventArgs e) {
			//TODO: write an about box
			string strMsg;
			
			strMsg = "PlaneDisaster.NET database viewer\nCopyright 2006 Justin Dearing\nzippy1981@gmail.com";
			//TODO: Bitch about this problems on the SharpDevelop forum 
			//Possible reason: http://community.sharpdevelop.net/forums/thread/6750.aspx
			/*
			Assembly exe = typeof(MainForm).Assembly;
			ResourceManager rm = new ResourceManager
				("PlaneDisaster.MainForm", exe);
			strMsg = rm.GetString("AboutMsg");
			*/
			MessageBox.Show(strMsg, "About PlaneDisaster.NET");
		}
		
		
		void menuCompactDatabase_Click (object sender, System.EventArgs e)
		{
			//TODO: make sure you are not compacting the currently open database. 
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new OpenFileDialog();
			FileFilter.Append("Microsoft Access (*.mdb)|*.mdb");
			dlg.Filter = FileFilter.ToString();
			
			if (dlg.ShowDialog() == DialogResult.OK) {
				JetSqlUtil.CompactMDB(dlg.FileName);
			}
			dlg.Dispose();
		}
		
		
		void menuDatabaseSchema_Click(object sender, System.EventArgs e)
		{
			this.gridResults.DataSource = ((OleDba)this.dbcon).GetSchema();
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
				dbcon.DropTable('[' + (string) lstViews.SelectedItem + ']');
				lstViews.DataSource = ((OleDba)dbcon).GetViews();
			} else {
				throw new ArgumentException
					("sender for menuDrop_Click must be one of " +
					 "menuProcedures, menuTables, or menuViews.");
			}
		}
		
		
		void menuNew_Click (object sender, System.EventArgs e)
		{
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new SaveFileDialog();
			FileFilter.Append("Microsoft Access (*.mdb)|*.mdb");
			FileFilter.Append("|SQLite3 (*.db;*.db3;*.sqlite)|*.db;*.db3;*.sqlite");
			dlg.Filter = FileFilter.ToString();
			
			if (dlg.ShowDialog() == DialogResult.OK) {
				switch (dlg.FilterIndex) {
					case 1:
						JetSqlUtil.CreateMDB(dlg.FileName);
						this.OpenMDB(dlg.FileName);
						break;
					case 2:
						System.Data.SQLite.SQLiteConnection.CreateFile
							(dlg.FileName);
						this.OpenSQLite(dlg.FileName);
						break;
						
				}
			}
			dlg.Dispose();
			this.Connected = true;
			InitContextMenues();
		}

		
		private void menuOpen_Click (object sender, System.EventArgs e) {
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new OpenFileDialog();
			FileFilter.Append("Microsoft Access (*.mdb)|*.mdb");
			FileFilter.Append("|SQLite3 (*.db;*.db3;*.sqlite)|*.db;*.db3;*.sqlite");
			dlg.Filter = FileFilter.ToString();
			
			if(dlg.ShowDialog() == DialogResult.OK) {
				switch (dlg.FilterIndex) {
					case 1:
						this.OpenMDB(dlg.FileName);
						break;
					case 2:
						this.OpenSQLite(dlg.FileName);
						break;
						
				}
			}
			dlg.Dispose();
			this.Connected = true;
			InitContextMenues();
		}

		
		private void menuOpenMsSql_Click (object sender, System.EventArgs e) {
			MsSqlConnStrDialog dlg;
			using (dlg = new MsSqlConnStrDialog()) {
				if (dlg.ShowDialog() == DialogResult.OK) {
					MessageBox.Show(dlg.ConnectionString);
					((OleDba) dbcon).ConnectDSN(dlg.ConnectionString);
					this.DisplayDataSource();
				}
			}
		}
		
		
		private void menuOpenMySQL_Click (object sender, System.EventArgs e) {
			MySQLConnStrDialog dlg;
			using (dlg = new MySQLConnStrDialog()) {
				if (dlg.ShowDialog() == DialogResult.OK) {
					MessageBox.Show(dlg.ConnectionString);
					((OleDba) dbcon).ConnectDSN(dlg.ConnectionString);
					this.DisplayDataSource();
				}
			}
		}
		
		
		private void menuOpenPostgresql_Click (object sender, System.EventArgs e) {
			PostgresqlConnStrDialog dlg;
			using (dlg = new PostgresqlConnStrDialog(PostgresqlConnStrDialog.DbDriver.OleDb)) {
				if (dlg.ShowDialog() == DialogResult.OK) {
					((OleDba) dbcon).ConnectDSN(dlg.ConnectionString);
					this.DisplayDataSource();
				}
			}
		}

		
		private void menuExit_Click (object sender, System.EventArgs e) {
			this.Close();
		}


		void menuRepairDatabase_Click (object sender, System.EventArgs e)
		{
			StringBuilder FileFilter = new StringBuilder();
			FileDialog dlg = new OpenFileDialog();
			FileFilter.Append("Microsoft Access (*.mdb)|*.mdb");
			dlg.Filter = FileFilter.ToString();
			
			if (dlg.ShowDialog() == DialogResult.OK) {
				JetSqlUtil.RepairMDB(dlg.FileName);
			}
			dlg.Dispose();
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


		/// <summary>
		/// Populates the listbox that hold the names of tables in the
		/// database.
		/// </summary>
		/// <remarks>
		/// Anything that Connects to a Datasource should call this to 
		/// refresh the form.
		/// </remarks>
		private void DisplayDataSource() {
			lstProcedures.DataSource = dbcon.GetProcedures();
			lstTables.DataSource = dbcon.GetTables();
			lstViews.DataSource = dbcon.GetViews();
			this.txtResults.Text = "";
			this.gridResults.DataSource = null;
		}
		
		
		private void InitContextMenues () {
			ContextMenu ctxDropProcedure, ctxDropTable, ctxDropView;
			MenuItem menuDropProcedure, menuDropTable, menuDropView;
			
			menuDropProcedure = new MenuItem("Drop");
			menuDropProcedure.Click += new System.EventHandler(menuDrop_Click);
			menuDropProcedure.Name = "menuDropProcedure";
			ctxDropProcedure = new ContextMenu(new MenuItem[] {menuDropProcedure});
			this.lstProcedures.ContextMenu = ctxDropProcedure;
				
			menuDropTable = new MenuItem("Drop");
			menuDropTable.Click += new System.EventHandler(menuDrop_Click);
			menuDropTable.Name = "menuDropTable";
			ctxDropTable = new ContextMenu(new MenuItem[] {menuDropTable});
			this.lstTables.ContextMenu = ctxDropTable;
			
			menuDropView = new MenuItem("Drop");
			menuDropView.Click += new System.EventHandler(menuDrop_Click);
			menuDropView.Name = "menuDropView";
			ctxDropView = new ContextMenu(new MenuItem[] {menuDropView});
			
			this.lstViews.ContextMenu = ctxDropView;
		}
		
		
		private void LoadQueryResults(string SQL) {
			System.Data.DataSet ds;
			
			//Don't do anything if the query window is empty or we are not connected to a database.
			if (SQL == "" || dbcon == null) { return; }
			
			try {
				ds = dbcon.ExecuteSql(SQL);
			} catch (System.Data.Common.DbException ex) {
				MessageBox.Show
					(String.Format("Problem loading query {0}\r\nError Message: {1}", SQL, ex.Message));
				return;
			}
			
			if (ds.Tables.Count == 1) {
				txtResults.Text = dba.DataTable2CSV(ds.Tables[0]);
				gridResults.DataSource = ds.Tables[0];
			}
			// Assume that if no rows were returned, then the schema was altered.
			else {
				DisplayDataSource();
			}
		}
		
		
		private void LoadTableResults(string Table) {
			//TODO: Scrub or escape this table name
			this.LoadQueryResults(String.Format("SELECT * FROM [{0}]", Table));
		}
		
		private void OpenMDB (string FileName) {
			DialogResult Result;
			this.dbcon = new OleDba();
			
			try {
				((OleDba) dbcon).ConnectMDB(FileName);
			} catch (OleDbException ex) {
				//TODO: this is the error code for incorrect access password. Make this a constant.
				if (ex.ErrorCode == -2147217843) {
					InputDialog GetPasswd = new InputDialog();
					Result = GetPasswd.ShowDialog("Enter the password for the database");
					if (Result == DialogResult.OK) {
						try {
							((OleDba) dbcon).ConnectMDB(FileName, GetPasswd.Input);
						} catch (OleDbException exSecond) {
							if (exSecond.ErrorCode == -2147217843) {
								MessageBox.Show("Incorrect Password");
							} else {
								throw exSecond;
							}
							return;
						} finally { GetPasswd.Dispose(); }
					} 
				}
				else { 
					throw ex;
				}
			} 
			this.DisplayDataSource();
		}
		
		
		private void OpenSQLite (string FileName) {
			this.dbcon = new SQLiteDba();
			
			((SQLiteDba) dbcon).Connect(FileName);
			this.DisplayDataSource();
		}
		
		
		void MainFormLoad(object sender, System.EventArgs e)
		{
			
		}
	}
}
