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
 * Description:	This is the database access code for PlaneDisaster.NET
/*/


using System;
using System.Data;
using System.Data.Common;
using System.Text;


namespace PlaneDisaster
{

	/// <summary>
	/// dba stands for database access. This is the data access layer for 
	/// the PlaneDisaster project.
	/// </summary>
	public abstract class dba
	{
		private DbConnection _cn;
		
		/// <summary>The ADO.NET database connection</summary>
		protected virtual DbConnection cn {
			get {
				return this._cn;
			}
			set {
				this._cn = value;
			}
		}
		
		
		/// <summary>
		/// Creates a procedure with a given name and sql statement if a procedure of the same name exists it 
		/// replaces the procedure.
		/// </summary>
		/// <param name="Name">The name to give to the procedure.</param>
		/// <param name="SQL">The SQL statement that generates the PROCEDURE.</param>
		public void CreateProcedure(string Name, string SQL) {
			this.CreateProcedure(Name, SQL, true);
		}
		
		
		/// <summary>
		/// Creates a procedure with a given name and sql. You can specify to replace an 
		/// exisiting query of the same name.
		/// </summary>
		/// <param name="Name">The name to give to the procedure.</param>
		/// <param name="SQL">The SQL statement that generates the PROCEDURE.</param>
		/// <param name="ReplaceQuery">
		/// Set this to true to replace the current procedure.
		/// </param>
		public void CreateProcedure(string Name, string SQL, bool ReplaceQuery) {
			DbCommand cmd = cn.CreateCommand();
			if (ReplaceQuery) {
				try {
					cmd.CommandText = String.Format("DROP PROCEDURE {0}", Name);
					cmd.ExecuteNonQuery();
				} catch (DbException) {}
			}
			cmd.CommandText = String.Format("CREATE PROCEDURE {0} AS {1}", Name, SQL);
			cmd.ExecuteNonQuery();
		}
		
		
		/// <summary>
		/// Creates a view with a given name and sql if a view of the same name exists it 
		/// replaces the view.
		/// </summary>
		/// <param name="Name">The name to give to the view.</param>
		/// <param name="SQL">The SQL statement that generates the VIEW.</param>
		public void CreateView(string Name, string SQL) {
			this.CreateView(Name, SQL, true);
		}
		
		
		/// <summary>
		/// Creates a view with a given name and sql. You can specify to replace an 
		/// exisiting query of the same name.
		/// </summary>
		/// <param name="Name">The name to give to the view.</param>
		/// <param name="SQL">The SQL statement that generates the VIEW.</param>
		/// <param name="ReplaceQuery">
		/// Set this to true to replace the current view.
		/// </param>
		public void CreateView(string Name, string SQL, bool ReplaceQuery) {
			DbCommand cmd = cn.CreateCommand();
			if (ReplaceQuery) {
				try {
					cmd.CommandText = String.Format("DROP VIEW {0}", Name);
					cmd.ExecuteNonQuery();
				} catch (DbException) {}
			}
			cmd.CommandText = String.Format("CREATE VIEW {0} AS {1}", Name, SQL);
			cmd.ExecuteNonQuery();
		}
		
		
		/// <summary>
		/// Drops the given procedure from the database.
		/// </summary>
		/// <param name="Name">The name of the View.</param>
		public void DropProcedure(string Name) {
			DbCommand cmd = cn.CreateCommand();
			cmd.CommandText = String.Format("DROP PROCEDURE {0}", Name);
			cmd.ExecuteNonQuery();
		}
		
		
		/// <summary>
		/// Drops the given table from the database.
		/// </summary>
		/// <param name="Name">The name of the Table.</param>
		public void DropTable(string Name) {
			DbCommand cmd = cn.CreateCommand();
			cmd.CommandText = String.Format("DROP TABLE {0}", Name);
			cmd.ExecuteNonQuery();
		}
		
		
		/// <summary>
		/// Drops the given view from the database.
		/// </summary>
		/// <param name="Name">The name of the View.</param>
		public void DropView(string Name) {
			DbCommand cmd = cn.CreateCommand();
			cmd.CommandText = String.Format("DROP VIEW {0}", Name);
			cmd.ExecuteNonQuery();
		}

		
		/// <summary>Disconnect from the database.</summary>
		public void Disconnect () {
			this.cn.Close();
		}
		
		
		/// <summary>
		/// Executes the SQL command(s) passed as a string.
		/// </summary>
		/// <param name="SQL">One or more SQL commands semicolon delimited.</param>
		/// <returns>A dataset generated from the last SQL command in the script.</returns>
		public DataSet ExecuteSql(string SQL) {
			DataSet ret;
			DbCommand cmd = cn.CreateCommand();
			//cmd.Transaction = cn.BeginTransaction();
			//TODO: perhaps move this trimming over to dba::ExecuteSQL.
			SQL = SQL.Trim();
			SQL = SQL.TrimEnd(new char [] {';'});
			string [] Statements = SqlScript2Statements(SQL);
			
			for (int i = 0; i < Statements.Length - 1; i++) {
				if (Statements[i] != "") {
					cmd.CommandText = Statements[i];
					cmd.ExecuteNonQuery();
				}
			}
			ret = this.GetSqlAsDataSet(Statements[Statements.Length - 1]);
			//cmd.Transaction.Commit();
			return ret;
		}
		
		
		/// <summary>
		/// Executes the SQL command(s) in a file.
		/// </summary>
		/// <param name="Script">
		/// A text file containing one or more SQL commands semicolon delimited.
		/// </param>
		/// <returns>A dataset generated from the last SQL command in the script.</returns>
		public DataSet ExecuteSqlScript(string Script) {
			return this.ExecuteSql(System.IO.File.ReadAllText(Script));
		}
		
		
		/// <summary>
		/// Gets a column from a table and returns it as a string of arrays.
		/// </summary>
		/// <param name="Table">The name of the table or view.</param>
		/// <param name="Col">The name of the column.</param>
		/// <returns>The contents of the column as a string of arrays.</returns>
		public string [] GetColumnAsStringArray (string Table, string Col) {
			string SQL;
			DbCommand cmd;
			DbDataReader rdr;
			int numRows;
			string [] Rows;
			int curRowNum = 0;	
			
			cmd = cn.CreateCommand();
			//cmd.Transaction = this.cn.BeginTransaction();;

			try {
				SQL = string.Concat("SELECT COUNT(*) FROM ", Table);
				cmd.CommandText = SQL;
				rdr = cmd.ExecuteReader();
				rdr.Read();
				numRows = rdr.GetInt32(0);
				rdr.Close();
				rdr = null;
				cmd = null;
				if (numRows == 0) {
					return new string [] {""};
				}
				Rows = new string [numRows];	
				
				SQL = string.Concat("SELECT ", Col, " FROM ", Table);
				cmd.CommandText = SQL;
				cmd.Parameters.Clear();
				cmd.Parameters.Add(Col);
				rdr = cmd.ExecuteReader();
				
				while (rdr.Read()) {
					Rows[curRowNum] = (string) rdr[Col];
					curRowNum++;
				}
				rdr.Close();
				//cmd.Transaction.Commit();
			} catch {
				Rows = new string [1] {""};
				cmd.Transaction.Rollback();
				
			}
			return Rows;
		}
		 
		 	
		/// <summary>
		/// Gets the names of the columns in a table
		/// </summary>
		/// <param name="Table">The Name of the table</param>
		/// <returns>The column names as an array of strings.</returns>
		public abstract string [] GetColumnNames (string Table);
		
		
		/// <summary>
		/// Gets a list of Procedures in the database.
		/// </summary>
		/// <returns>
		/// A list of Procedures names as an array of strings.
		/// </returns>
		public virtual string [] GetProcedures() {
			int numCols;
			int i = 0;
			string [] Tables;
			
			DataTable dt = null;			

			dt = cn.GetSchema("procedures");
			numCols = dt.Rows.Count;
			Tables = new string[numCols];
			for (i = 0; i < numCols; i++) {
				Tables[i] = (string) dt.Rows[i]["PROCEDURE_NAME"];
			}

			return Tables;
		}
		
		
		/// <summary>
		/// Gets database schema.
		/// </summary>
		/// <returns>
		/// A datatable of what I believe to be useless information. I believe
		/// you can use the first column to determins the schema types you can 
		/// return.
		/// </returns>
		public virtual DataTable GetSchema() {
			//TODO: See if there is a good call to GetOleDbSchemaTable() to get info like this
			return cn.GetSchema();
		}

		
		/// <summary>
		/// Executes a SQL query and returns the rtesults as a comma seperated file.
		/// </summary>
		/// <param name="SQL">The SQL statement to execute.</param>
		/// <returns>A string containing a CSV</returns>
		public string GetSQLAsCSV (string SQL) {
			return this.GetSQLAsCSV(SQL, ",");
		}
		
		
		/// <summary>
		/// Executes a SQL query and returns the Results as a 
		/// <code>Seperator</code> seperated file.
		/// </summary>
		/// <param name="SQL">The SQL statement to execute.</param>
		/// <param name="Seperator">
		/// The field seperator character(s).
		/// </param>
		/// <returns>A string containing the table with rows seperated by 
		/// newlines and fields seperated by <code>Seperator</code>.
		/// </returns>
		public string GetSQLAsCSV (string SQL, string Seperator) {
			DbCommand cmd = cn.CreateCommand();
			cmd.CommandText = SQL;
			DbDataReader rdr;
			int numFields;
			string [] Fields;
			StringBuilder  CSV = new StringBuilder();
			
			rdr = cmd.ExecuteReader();
			
			numFields = rdr.FieldCount;
			Fields = new string[numFields];
			for (int i = 0; i < numFields; i++) {
				Fields[i] = rdr.GetName(i);
				CSV.AppendFormat("{0}{1}", Fields[i], Seperator);
			}
			CSV.AppendLine();
			while (rdr.Read()) {
				foreach (string Field in Fields) {
					CSV.AppendFormat("{0}{1}", rdr[Field], Seperator);
				}
				CSV.AppendLine();
			}
			rdr.Close();
						
			return CSV.ToString();
		}
		
		
		/// <summary>
		/// Executes a SQL statement and returns the results in a 
		/// <code>System.DataGridView</code>
		/// </summary>
		/// <param name="SQL">The SQL Statement</param>
		/// <returns>A DataGridView containing the result set.</returns>
		public abstract DataSet GetSqlAsDataSet(string SQL);
		//TODO: find a way to write GetSqlAsDataSet(string SQL) once
		

		/// <summary>
		/// Returns a message describing the Connection state.
		/// </summary>
		/// <returns>
		/// A string in the format 
		/// <code>ConnectionString\nState</code>
		/// </returns>
		public string GetStatus() {
			string Status;
			
			Status = String.Concat(
				cn.ConnectionString,
				"\n",
				cn.State.ToString()
			);
			return Status;
		}

		
		/// <summary>
		/// Returns all rows in a table in a string in CSV format.
		/// </summary>
		/// <param name="Table">The name of the table</param>
		/// <returns>A string containing a CSV</returns>
		public string GetTableAsCSV (string Table) {
			return this.GetSQLAsCSV("SELECT * FROM " + Table);
		}
		

		/// <summary>
		/// Returns all rows in a table in a 
		/// <code>System.DataGridView</code>.
		/// </summary>
		/// <param name="Table">The name of the table</param>
		/// <returns>A DataGridView containing the result set.</returns>
		public DataSet GetTableAsDataSet (string Table) {
			DataSet ret = this.GetSqlAsDataSet("SELECT * FROM " + Table);
			ret.Tables[0].TableName = Table;
			
			return ret;
		}
		

		/// <summary>
		/// Gets a list of tables in the database.
		/// </summary>
		/// <returns>
		/// A list of table names as an array of strings.
		/// </returns>
		public virtual string [] GetTables() {
			int numCols;
			int i = 0;
			string [] Tables;
			DataTable dt = null;
			
			dt = cn.GetSchema("tables");
			numCols = dt.Rows.Count;
			Tables = new string[numCols];
			for (i = 0; i < numCols; i++) {
				Tables[i] = (string) dt.Rows[i]["TABLE_NAME"];
			}
			return Tables;
		}
		
		
		/// <summary>
		/// Gets a list of Views in the database.
		/// </summary>
		/// <returns>
		/// A list of views names as an array of strings.
		/// </returns>
		public string [] GetViews() {
			int numCols;
			int i = 0;
			string [] Tables;
			
			DataTable dt = null;			

			dt = cn.GetSchema("views");
			numCols = dt.Rows.Count;
			Tables = new string[numCols];
			for (i = 0; i < numCols; i++) {
				Tables[i] = (string) dt.Rows[i]["TABLE_NAME"];
			}

			return Tables;
		}
	
	
		/// <summary>
		/// Performs the SQL query specified and returns an xml 
		/// serialized datatable of the retultset.
		/// </summary>
		/// <param name="SQL">The SQL query to execute.</param>
		/// <returns>
		/// A string containing an XML serialized data tablr.
		/// </returns>
		public string SerializeQuery (string SQL) {
			return this.GetSqlAsDataSet(SQL).GetXml();
		}
		
		
		/// <summary>
		/// Performs the SQL query specified and writes the xml serialized
		/// datatable as a file.
		/// </summary>
		/// <param name="SQL">The SQL query to execute.</param>
		/// <param name="File">
		/// The name of the file to write the xml to.
		/// </param>
		public void SerializeQuery (string SQL, string File) {
			this.GetSqlAsDataSet(SQL).WriteXml(File, XmlWriteMode.WriteSchema);
		}
		
		
		/// <summary>
		/// Execute a SELECT * FROM Table and returns the results as
		/// an XML serialized datatable.
		/// </summary>
		/// <param name="Table">
		/// The name of the table to serialize.
		/// </param>
		/// <returns>
		/// The table as an XML serialized datatable.
		/// </returns>
		public string SerializeTable (string Table) {
		 	string SQL = "SELECT * FROM " + Table;
		 	return this.SerializeQuery(SQL);
		}

		
		/// <summary>
		/// Execute a SELECT * FROM Table and writes the results to a file
		/// as an XML serialized datatable.
		/// </summary>
		/// <param name="Table">
		/// The name of the table to serialize.
		/// </param>
		/// <param name="File"></param>
		public void SerializeTable (string Table, string File) {
			string SQL = "SELECT * FROM " + Table;
		 	this.SerializeQuery(SQL, File);
		}
		
		#region Private Members
		
		private string [] SqlScript2Statements (string Script) {
			//TODO: write a parser that can handle semi colons inside of quotes.
			Script = Script.TrimEnd(';');
			return Script.Split(';');
		}
		
		#endregion
		
		#region Static Members
		
		
		/// <summary>
		/// Prints out an exception in a messagee box.
		/// This is my catchall for dealing with exceptions.
		/// </summary>
		/// <param name="e">The Exception I am dealing with.</param>
		protected static void DbaException (DbException e) {
			string Title;
		
			Title = String.Format("DBA Exception (HRESULT: {0})", e.ErrorCode);
			System.Windows.Forms.MessageBox.Show
				(e.ToString() , Title);
		}
		
		
		/// <summary>
		/// Converts a DataTable to a string in CSV format.
		/// </summary>
		/// <param name="dt">The datatable</param>
		/// <returns>
		/// A string with the fields of the datatable seperated by commas
		/// and the rows seperated by newlines.
		/// </returns>
		public static string DataTable2CSV(DataTable dt) {
			return DataTable2CSV(dt, ",");
		}
		
		
		/// <summary>
		/// Converts a DataTable to a string in CSV format.
		/// </summary>
		/// <param name="dt">The datatable</param>
		/// <param name="Seperator">The column seperator.</param>
		/// <returns>
		/// A string with the fields of the datatable seperated by Seperator
		/// and the rows seperated by newlines.
		/// </returns>
		public static string DataTable2CSV(DataTable dt, string Seperator) {
			int numFields;
			string [] Fields;
			StringBuilder CSV = new StringBuilder();
			
			numFields = dt.Columns.Count;
			Fields = new string[numFields];
			for (int i = 0; i < numFields; i++) {
				Fields[i] = dt.Columns[i].ColumnName;
				CSV.AppendFormat("{0}{1}", Fields[i], Seperator);
			}
			CSV.AppendLine();
			foreach(DataRow row in dt.Rows) {
				foreach (string Field in Fields) {
					CSV.AppendFormat("{0}{1}", row[Field], Seperator);
				}
				CSV.AppendLine();
			}

			return CSV.ToString();
		}
				
		#endregion
	}
}
