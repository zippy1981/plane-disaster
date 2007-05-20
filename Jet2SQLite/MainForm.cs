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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using PlaneDisaster.Dba;

namespace PlaneDisaster.Jet2SQLite
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Maximun number rows to insert into the database at a time.
		/// </summary>
		const int InsertRowCount = 1000;
		
		#region Properties
		
		private string JetSqlFile {
			get {
				return txtJetSqlFile.Text;
			}
			
			set {
				if (File.Exists(value)) {
					txtJetSqlFile.Text = value;
				} else {
					throw new FileNotFoundException("JetSql filw dows not exist", value);
				}
			}
		}
		
		private string SQLiteFile {
			get {
				return this.txtSQLiteFile.Text;
			}
			
			set {
				if (Directory.Exists(Path.GetDirectoryName(value))) {
					txtSQLiteFile.Text = value;
				} else {
					throw new FileNotFoundException("JetSql file does not exist", value);
				}
			}
		}
		
		#endregion
		
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
		public MainForm()
		{
			InitializeComponent();
		}
		
		#region Events
		
		
		void AboutToolStripMenuItemClick(object sender, EventArgs e)
		{
			StringBuilder Msg = new StringBuilder();
			Msg.AppendLine("Jet2SQLite");
			Msg.AppendLine("Microsoft Access to SQLite Database converter ");
			Msg.Append("by Justin Dearing <zippy1981@gmail.com>.");
			MessageBox.Show(Msg.ToString());
		}
		
		
		void CmdConvertClick(object sender, EventArgs e)
		{
			txtLog.Text = String.Format("Start Time: {0}", DateTime.Now.ToLongTimeString());
			OdbcDba JetDb = new OdbcDba();
			SQLiteDba SQLiteDb = new SQLiteDba();
			
			try {
				File.Delete(this.SQLiteFile);
			} catch (IOException) {
				string Msg = String.Format("Cannot delete the existing SQLite file {0}", SQLiteFile);
				MessageBox.Show(Msg);
				return;
			}
			try {
				JetDb.ConnectMDB(this.JetSqlFile);
			} catch (OdbcException ex) {
				//TODO: this is the error code for incorrect access password. Make this a constant.
				if (ex.ErrorCode == -2147217843 || ex.ErrorCode == -2146232009) {
					DialogResult Result;
					InputDialog GetPassword = new InputDialog();
					Result = GetPassword.ShowDialog("Enter the password for the database");
					if (Result == DialogResult.OK) {
						try {
							((OdbcDba) JetDb).ConnectMDB(JetSqlFile, GetPassword.Input);
						} catch (OdbcException exSecond) {
							if (ex.ErrorCode == -2147217843 || ex.ErrorCode == -2146232009) {
								MessageBox.Show("Incorrect Password");
							} else {
								throw exSecond;
							}
							return;
						} finally { GetPassword.Dispose(); }
					}
				} else if (ex.ErrorCode == -2147467259) {
					Text = "PlaneDisaster.NET";
					string Msg = String.Format("File [{0}] not found.", JetSqlFile);
					MessageBox.Show(Msg, "Error Opening File");
					return;
				} else {
					throw ex;
				}
			}
			
			SQLiteDb.Connect(this.SQLiteFile);
			
			string [] Tables = JetDb.GetTables();
			
			foreach (string Table in Tables ) {
				try {
					SQLiteDb.DataTable2SQLiteTable(JetDb.GetTableAsDataTable(Table));
				} catch (OdbcException ex) {
					string Message = String.Format("{0}: {1}\n", Table, ex.Message);
					File.AppendAllText(String.Concat(SQLiteFile, ".log"), Message);
				}
			}
			JetDb.Disconnect();
			SQLiteDb.Disconnect();
			MessageBox.Show("Database Successfully converted.");
			txtLog.Text += String.Format(" End Time: {0}", DateTime.Now.ToLongTimeString());
		} 
		
		void CmdBrowseJetSqlClick(object sender, EventArgs e)
		{
			StringBuilder FileFilter = new StringBuilder();
			OpenFileDialog dlg = new OpenFileDialog();
			
			FileFilter.Append("Microsoft Access (*.mdb;*.mde)|*.mdb;*.mde");
			FileFilter.Append("|All Files (*.*)|*.*");
			dlg.Filter = FileFilter.ToString();
			
			if(dlg.ShowDialog() == DialogResult.OK ) {
				this.JetSqlFile = dlg.FileName;
			} 
		}
		
		void CmdSQLiteFileClick(object sender, EventArgs e)
		{
			StringBuilder FileFilter = new StringBuilder();
			SaveFileDialog dlg = new SaveFileDialog();
			
			FileFilter.Append("SQLite3 (*.db;*.db3;*.sqlite)|*.db;*.db3;*.sqlite");
			FileFilter.Append("|All Files (*.*)|*.*");
			dlg.Filter = FileFilter.ToString();
			
			if(dlg.ShowDialog() == DialogResult.OK ) {
				this.SQLiteFile= dlg.FileName;
			}
		}
		
		
		void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			this.Close();
		}
		
		#endregion
	}
}
