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
using System.Data.Common;
using System.Data.SQLite;
using System.Text;

namespace PlaneDisaster.LIB
{
	/// <summary>
	/// Description of SQLitea.
	/// </summary>
	public class SQLiteDba : dba
	{
		private SQLiteConnection _Cn;
		private string _ConnStr;
		
		
		/// <summary>The SQLite Connection string.</summary>
		protected string ConnStr {
			get { return this._ConnStr; }
			set { this._ConnStr = value; }
		}
		
		
		/// <summary>The SQLite database connection</summary>
		protected override System.Data.Common.DbConnection Cn {
			get {
				return this._Cn;
			}
			set {
				this._Cn = (SQLiteConnection) value;
			}
		}
		
		
		/// <summary>
		/// Connect to the previously defined connection string.
		/// </summary>
		public void Connect (){
			this.Cn = new SQLiteConnection(ConnStr);
			Cn.Open();
		}
		

		/// <summary>
		/// Connect to the specified database
		/// </summary>
		/// <param name="FileName">Database to connect to.</param>
		public void Connect(string FileName) {
			this.ConnStr = String.Format("Data Source={0}", FileName);
			this.Connect();
		}
		
		
		/// <summary>
		/// Factory method to create a new DataAdapter of the SQLiteDataAdapter type.
		/// </summary>
		/// <param name="cmd">The select fommand for the data adapter.</param>
		/// <returns>A populated DataAdapter of the SQLiteDataAdapter type.</returns>
		protected override DataAdapter CreateDataAdapter(DbCommand cmd) { return new SQLiteDataAdapter((SQLiteCommand) cmd);}

		
		/// <summary>
		/// Gets the names of the columns in a table
		/// </summary>
		/// <param name="Table">The Name of the table</param>
		/// <returns>The column names as an array of strings.</returns>
		public override string [] GetColumnNames (string Table) {
			DataTable dt = new DataTable();
			int numCols;
			string [] Tables;
			
			dt = Cn.GetSchema("Columns", new string [] {null, null, Table, null});
			numCols = dt.Rows.Count;
			Tables = new string[numCols];
			for (int i = 0; i < numCols; i++) {
				Tables[i] = (string) dt.Rows[i]["COLUMN_NAME"];
			}
			
			return Tables;
		}
		
		
		/// <summary>
		/// Gets a list of procedures in the database.
		/// </summary>
		/// <returns>
		/// A list of procedure names as an array of strings.
		/// </returns>
		public override string [] GetProcedures() {
			return null;
		}
		
		
		/// <summary>
		/// Gets the SQL executed by a given TABLE.
		/// </summary>
		/// <remarks>
		/// Posted by Rasha in http://sqlite.phxsoftware.com/forums/thread/2272.aspx 
		/// </remarks>
		/// <returns>
		/// The DDL of the given table.
		/// </returns>
		public virtual string GetTableSQL(string Table) {
			using (SQLiteCommand cmd = (SQLiteCommand)Cn.CreateCommand()) {
				cmd.CommandText =  "SELECT sql FROM sqlite_master " +
					"WHERE name = @tablename";
				cmd.Parameters.Add("@tablename", DbType.String);
				cmd.Parameters["@tablename"].Value = Table;
				return (string) cmd.ExecuteScalar();
			}
		}
	}
}
