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
 * Date: 8/15/2006
 * Time: 6:07 PM
/*/

using System;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace PlaneDisaster
{
	/// <summary>
	/// Description of OleDba.
	/// </summary>
	public class OleDba : dba
	{
		private OleDbConnection _cn;
		private string _ConnStr;
		private string MDB;
		private string Passwd;
		
		
		/// <summary>The OleDb database connection</summary>
		protected override System.Data.Common.DbConnection cn {
			get {
				return this._cn;
			}
			set {
				this._cn = (OleDbConnection) value;
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
			this.cn = new OleDbConnection(ConnStr);
			cn.Open();
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
		/// <param name="Passwd">
		/// The password to connecto to the database as.
		/// </param>
		public void ConnectMDB(string File, string Passwd) {
			MDB = File;
			this.Passwd = Passwd;
			ConnStr = String.Format
					("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Database Password={1};", MDB, Passwd);
			this.Connect();
		}
		

		/// <summary>
		/// Gets the names of the columns in a table
		/// </summary>
		/// <param name="Table">The Name of the table</param>
		/// <returns>The column names as an array of strings.</returns>
		public override string [] GetColumnNames (string Table) {
			DataTable dt = new DataTable();
			int numCols;
			string [] Tables;
			
			dt = ((OleDbConnection)cn).GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object [] {null, null, Table, null});
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
		public string GetProcedureSQL(string Procedure) {
			DataTable dt;
			dt = ((OleDbConnection)cn).GetOleDbSchemaTable
				(System.Data.OleDb.OleDbSchemaGuid.Procedures, 
				 new object[] {null, null, Procedure, null});
			return (string) dt.Rows[0]["PROCEDURE_DEFINITION"];
		}
			
		
		/// <summary>
		/// Executes a SQL statement and returns the results in a 
		/// <code>System.DataGridView</code>
		/// </summary>
		/// <param name="SQL">The SQL Statement</param>
		/// <returns>A DataGridView containing the result set.</returns>
		public override DataSet GetSqlAsDataSet(string SQL) {
			DataSet ds = new DataSet();
			OleDbDataAdapter da = new OleDbDataAdapter(SQL, (OleDbConnection) this.cn);
			//OleDbTransaction trans = (OleDbTransaction) cn.BeginTransaction();
			//try {
				da.Fill(ds, "qryTemp");
			//} catch (OleDbException) {
				//I probally threw it a DDL query.
			//}
			//trans.Commit();
			return ds;
		}
		
		
		/// <summary>
		/// Gets the SQL executed by a given VIEW.
		/// </summary>
		/// <returns>
		/// The source of the given view.
		/// </returns>
		public string GetViewSQL(string View) {
			DataTable dt;
			dt = ((OleDbConnection)cn).GetOleDbSchemaTable
				(System.Data.OleDb.OleDbSchemaGuid.Views, 
				 new object[] {null, null, View});
			return (string) dt.Rows[0]["VIEW_DEFINITION"];
		}
	}
}
