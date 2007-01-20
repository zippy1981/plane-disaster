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
 * Date: 8/15/2006
 * Time: 6:07 PM
 */

using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Text;

namespace PlaneDisaster.LIB
{
	/// <summary>
	/// Description of OleDba.
	/// </summary>
	public class OleDba : dba
	{
		private OleDbConnection _Cn;
		private string _ConnStr;
		private string MDB;
		private string Password;
		
		
		/// <summary>The OleDb database connection</summary>
		protected override System.Data.Common.DbConnection Cn {
			get {
				return this._Cn;
			}
			set {
				this._Cn = (OleDbConnection) value;
			}
		}
		
		
		/// <summary>The OleDb Connection string</summary>
		protected string ConnStr {
			get { return this._ConnStr; }
			set { this._ConnStr = value; }
		}
		
		
		/// <summary>
		/// Connect to the previously defined connection string.
		/// </summary>
		public void Connect (){
			this.Cn = new OleDbConnection(ConnStr);
			Cn.Open();
		}
		

		/// <summary>
		/// Connect to the specified DSN
		/// </summary>
		/// <param name="ConnStr">DSN to connect to.</param>
		public void ConnectDSN(string ConnStr) {
			this.ConnStr = ConnStr;
			this.Connect();
		}
		

		/// <summary>
		/// Connect to the previously defined MDB.
		/// </summary>
		public void ConnectMDB() {
			ConnStr = String.Format
				("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};", MDB);
			this.Connect();
		}
		

		/// <summary>
		/// Connect to the specified MDB file.
		/// </summary>
		/// <param name="File">MDB file to connect to.</param>
		public void ConnectMDB(string File) {
			MDB = File;
			this.ConnectMDB();
		}
		
		
		/// <summary>
		/// Connect to the specified MDB file with the specified passwd.
		/// </summary>
		/// <param name="File">MDB file to connect to.</param>
		/// <param name="Password">
		/// The password to connecto to the database as.
		/// </param>
		public void ConnectMDB(string File, string Password) {
			MDB = File;
			this.Password = Password;
			ConnStr = String.Format
					("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Database Password={1};", MDB, Password);
			this.Connect();
		}
		
		
		/// <summary>
		/// Factory method to create a new DataAdapter of the OleDbDataAdapter type.
		/// </summary>
		/// <param name="cmd">The select fommand for the data adapter.</param>
		/// <returns>A populated DataAdapter of the OleDbDataAdapter type.</returns>
		protected override DataAdapter CreateDataAdapter(DbCommand cmd) { return new OleDbDataAdapter((OleDbCommand) cmd);}
		

		/// <summary>
		/// Gets the names of the columns in a table
		/// </summary>
		/// <param name="Table">The Name of the table</param>
		/// <returns>The column names as an array of strings.</returns>
		public override string [] GetColumnNames (string Table) {
			DataTable dt = new DataTable();
			int numCols;
			string [] Tables;
			
			dt = ((OleDbConnection)Cn).GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object [] {null, null, Table, null});
			numCols = dt.Rows.Count;
			Tables = new string[numCols];
			for (int i = 0; i < numCols; i++) {
				Tables[i] = (string) dt.Rows[i][3];
			}
			return Tables;
		}
		
		
		/// <summary>
		/// Gets the SQL executed by a given procedure.
		/// </summary>
		/// <returns>
		/// The source of the given procedure.
		/// </returns>
		public override string GetProcedureSQL(string Procedure) {
			DataTable dt;
			dt = ((OleDbConnection)Cn).GetOleDbSchemaTable
				(System.Data.OleDb.OleDbSchemaGuid.Procedures, 
				 new object[] {null, null, Procedure, null});
			return (string) dt.Rows[0]["PROCEDURE_DEFINITION"];
		}
		
		
		/// <summary>
		/// Returns all rows in a table in a 
		/// <code>System.DataGridView</code>.
		/// </summary>
		/// <param name="TableName">The name of the table</param>
		/// <returns>A DataGridView containing the result set.</returns>
		public override DataTable GetTableAsDataTable (string TableName) {
			DataTable ret = new DataTable();
			OleDbCommand cmd = (OleDbCommand)this.Cn.CreateCommand();
			cmd.CommandType = CommandType.TableDirect;
			cmd.CommandText = TableName;
			OleDbDataAdapter da = new OleDbDataAdapter(cmd);
			da.Fill(ret);
			ret.TableName = TableName;
			return ret;
		}
		
		
		/// <summary>
		/// Gets a list of tables in the database.
		/// </summary>
		/// <remarks>
		/// This is overwritten here because when I use the GetSchema method in 
		/// dba.GetTables(), I get all the database objects. I believe this to be 
		/// more of an Access thing than an OleDb thing. Futher expirimentation
		/// is neccessary.
		/// </remarks>
		/// <returns>
		/// A list of table names as an array of strings.
		/// </returns>
		public override string [] GetTables() {
			int numCols;
			int i = 0;
			string [] Tables;
			DataTable dt = null;
			
			dt = ((OleDbConnection)Cn).GetOleDbSchemaTable
				(OleDbSchemaGuid.Tables,
				 new Object[] {null, null, null, "TABLE"});
			numCols = dt.Rows.Count;
			Tables = new string[numCols];
			for (i = 0; i < numCols; i++) {
				Tables[i] = (string) dt.Rows[i]["TABLE_NAME"];
			}
			return Tables;
		}
		
		
		/// <summary>
		/// Gets the SQL executed by a given VIEW.
		/// </summary>
		/// <returns>
		/// The source of the given view.
		/// </returns>
		public override string GetViewSQL(string View) {
			DataTable dt;
			dt = ((OleDbConnection)Cn).GetOleDbSchemaTable
				(System.Data.OleDb.OleDbSchemaGuid.Views, 
				 new object[] {null, null, View});
			return (string) dt.Rows[0]["VIEW_DEFINITION"];
		}
	}
}
