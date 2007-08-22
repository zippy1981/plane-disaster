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
 * Description:	This is the database access code for PlaneDisaster.NET
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;


namespace PlaneDisaster.Dba
{

	/// <summary>
	/// dba stands for database access. This is the data access layer for 
	/// the PlaneDisaster project.
	/// </summary>
	public abstract class dba : IDisposable
	{
		private DbConnection _Cn = null;
		
		#region Properties
		
		/// <summary>
		/// Returns true if there is an active database connection.
		/// </summary>
		public bool Connected {
			get {
				if (Cn == null)
				{
					return false;
				} 
				else
				{
					return Cn.State == ConnectionState.Open;
				}
			}
		}
		
		
		/// <summary>The ADO.NET database connection</summary>
		protected virtual DbConnection Cn {
			get {
				return this._Cn;
			}
			set {
				this._Cn = value;
			}
		}
		
		
		/// <summary>The Database Connection string</summary>
		public string ConnectionString {
			get { return this.Cn.ConnectionString; }
		}
		
		
		/// <summary>
		/// Returns true if the underlying database provider supports procedures.
		/// </summary>
		public abstract bool SupportsProcedures { get; }
		
		
		/// <summary>
		/// Returns true if the underlying database provider supports views.
		/// </summary>
		public abstract bool SupportsViews { get; }
		
		#endregion Properties
		
		/// <summary>
		/// Factory method to create a new DataAdapter of the appropiate type.
		/// </summary>
		/// <param name="cmd">The select fommand for the data adapter.</param>
		/// <returns>A populated DataAdapter of the appropiate type.</returns>
		protected abstract DataAdapter CreateDataAdapter(DbCommand cmd);
		
				
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
			DbCommand cmd = Cn.CreateCommand();
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
			DbCommand cmd = Cn.CreateCommand();
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
			DbCommand cmd = Cn.CreateCommand();
			cmd.CommandText = String.Format("DROP PROCEDURE {0}", Name);
			cmd.ExecuteNonQuery();
		}
		
		
		/// <summary>
		/// Drops the given table from the database.
		/// </summary>
		/// <param name="Name">The name of the Table.</param>
		public void DropTable(string Name) {
			DbCommand cmd = Cn.CreateCommand();
			cmd.CommandText = String.Format("DROP TABLE {0}", Name);
			cmd.ExecuteNonQuery();
		}
		
		
		/// <summary>
		/// Drops the given view from the database.
		/// </summary>
		/// <param name="Name">The name of the View.</param>
		public void DropView(string Name) {
			DbCommand cmd = Cn.CreateCommand();
			cmd.CommandText = String.Format("DROP VIEW {0}", Name);
			cmd.ExecuteNonQuery();
		}

		
		/// <summary>Disconnect from the database.</summary>
		public void Disconnect () {
			this.Cn.Close();
		}
		

		/// <summary>
		/// Performs disposal.
		/// </summary>
		public void Dispose() {
			Cn.Dispose();
		}
		
		/// <summary>
		/// Executes the SQL command(s) passed as a string.
		/// </summary>
		/// <param name="SQL">One or more SQL commands semicolon delimited.</param>
		/// <returns>A dataset generated from the last SQL command in the script.</returns>
		public DataTable ExecuteScript(string SQL) {
			DataTable ret;
			DbCommand cmd = Cn.CreateCommand();
			SQL = SQL.Trim();
			SQL = SQL.TrimEnd(new char [] {';'});
			string [] Statements = SqlScript2Statements(SQL);
			
			for (int i = 0; i < Statements.Length - 1; i++) {
				if (Statements[i] != "") {
					cmd.CommandText = Statements[i];
					cmd.ExecuteNonQuery();
				}
			}
			cmd.Dispose();
			ret = this.GetSqlAsDataTable(Statements[Statements.Length - 1]);
			return ret;
		}
		
		
		/// <summary>
		/// Executes the SQL command passed as a string.
		/// </summary>
		/// <param name="SQL">One or more SQL commands semicolon delimited.</param>
		public void ExecuteSqlCommand (string SQL) {
			using (DbCommand cmd = Cn.CreateCommand()) {
				cmd.CommandText = SQL;
				cmd.ExecuteNonQuery();
			}
		}
		
		
		/// <summary>
		/// Executes the SQL command passed as a string.
		/// </summary>
		/// <param name="SQL">One or more SQL commands semicolon delimited.</param>
		/// <param name="paramaters">The parameters to pass to the SQL.</param>
		public virtual void ExecuteSqlCommand (string SQL, DbParameter [] parameters) {
			using (DbCommand cmd = Cn.CreateCommand()) {
				cmd.CommandText = SQL;
				cmd.Parameters.AddRange(parameters);
				cmd.ExecuteNonQuery();
			}
		}
		
		
		/// <summary>
		/// Executes the SQL command(s) in a file.
		/// </summary>
		/// <param name="Script">
		/// A text file containing one or more SQL commands semicolon delimited.
		/// </param>
		/// <returns>A dataset generated from the last SQL command in the script.</returns>
		public DataTable ExecuteSqlFile(string Script) {
			return this.ExecuteScript(System.IO.File.ReadAllText(Script));
		}
		
		
		/// <summary>
		/// Gets a column from a table and returns it as a string of arrays.
		/// </summary>
		/// <param name="Table">The name of the table or view.</param>
		/// <param name="Col">The name of the column.</param>
		/// <param name="Distinct">Set to true to only return unique values.</param>
		/// <returns>The contents of the column as a string of arrays.</returns>
		public virtual string [] GetColumnAsStringArray (string Table, string Col, bool Distinct) {
			string SQL;
			DbCommand cmd;
			DbDataReader rdr;
			ArrayList Rows = new ArrayList();
			
			using (cmd = Cn.CreateCommand()) {
				Rows = new ArrayList();
				
				SQL = Distinct ?
					string.Format("SELECT DISTINCT [{1}] FROM [{0}]", Table, Col) :
					string.Format("SELECT [{1}] FROM [{0}]", Table, Col);
				cmd.CommandText = SQL;
				rdr = cmd.ExecuteReader();
				
				while (rdr.Read()) {
					Rows.Add(rdr[Col].ToString());
				}
				rdr.Close();
				rdr.Dispose();
			}
			return (string []) Rows.ToArray(typeof(System.String));
		}
		
		
		/// <summary>
		/// Gets a column from a table and returns it as a string of arrays.
		/// </summary>
		/// <param name="Table">The name of the table or view.</param>
		/// <param name="Col">The name of the column.</param>
		/// <returns>The contents of the column as a string of arrays.</returns>
		public string [] GetColumnAsStringArray (string Table, string Col) {
			return this.GetColumnAsStringArray(Table, Col, false);
		}
		 
		 	
		/// <summary>
		/// Gets the names of the columns in a table
		/// </summary>
		/// <param name="Table">The Name of the table</param>
		/// <returns>The column names as an array of strings.</returns>
		public abstract string [] GetColumnNames (string Table);
		
		
		/// <summary>
		/// Gets information about columns for a given table, view or procedure.
		/// </summary>
		/// <param name="Object">
		/// Table, view or procedure to get information about.
		/// </param>
		/// <returns>A Datatable with the column schema for the given object.</returns>
		public DataTable GetColumnSchema (string Object) {
			string [] Restrictions = new string[4]{ null, null, Object, null };
			return Cn.GetSchema("Columns", Restrictions);
		}
		
		
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

			dt = Cn.GetSchema("procedures");
			numCols = dt.Rows.Count;
			Tables = new string[numCols];
			for (i = 0; i < numCols; i++) {
				Tables[i] = (string) dt.Rows[i]["PROCEDURE_NAME"];
			}

			return Tables;
		}
		
		
		/// <summary>
		/// Gets the SQL executed by a given procedure.
		/// </summary>
		/// <returns>
		/// The source of the given procedure.
		/// </returns>
		public virtual string GetProcedureSQL(string Procedure) {
			DataTable dt;
			dt = Cn.GetSchema
				("Procedures", new string[] {null, null, Procedure, null});
			return (string) dt.Rows[0]["PROCEDURE_DEFINITION"];
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
			return Cn.GetSchema();
		}
		
		
		/// <summary>
		/// Gets database schema.
		/// </summary>
		/// <param name="Collection">The name of the collection to retrieve.</param>
		/// <returns>
		/// A datatable with schema information about the database
		/// </returns>
		public virtual DataTable GetSchema(string Collection) {
			return Cn.GetSchema(Collection);
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
			DbCommand cmd = Cn.CreateCommand();
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
		/// <code>DbDataReader</code>
		/// </summary>
		/// <param name="SQL">The SQL Statement</param>
		/// <returns>A DbDataReader containing the result set.</returns>
		/// <remarks>Remember to close te reader when you are done with it.</remarks>
		public DbDataReader GetSqlAsDataReader(string SQL) {
			DbCommand cmd;
			DbDataReader rdr;
			
			using (cmd = Cn.CreateCommand()) {
				cmd.CommandText = SQL;
				rdr = cmd.ExecuteReader();
			}
			
			return rdr;
		}
		
		
		/// <summary>
		/// Executes a SQL statement and returns the results in a 
		/// <code>System.DataGridView</code>
		/// </summary>
		/// <param name="SQL">The SQL Statement</param>
		/// <returns>A DataGridView containing the result set.</returns>
		public virtual DataTable GetSqlAsDataTable(string SQL) {
			DbCommand cmd;
			DataSet ds = new DataSet();
			DataAdapter da;
			
			using (cmd = Cn.CreateCommand()) {
				cmd.CommandText = SQL;
				da = this.CreateDataAdapter(cmd);
				da.Fill(ds);
			}
			
			return (ds.Tables.Count == 0) ? null : ds.Tables[0];
		}
		
		
		/// <summary>
		/// Executes a SQL statement and returns the results in a
		/// <code>"System.DataTable"</code>
		/// </summary>
		/// <param name="SQL">The SQL Statement</param>
		/// <param name="TableName">The title of the DataTable</param>
		/// <returns>A DaTatable containing the result set.</returns>
		public DataTable GetSqlAsDataTable(string SQL, string TableName) {
			DataTable ret = this.GetSqlAsDataTable(SQL);
			ret.TableName = TableName;
			return ret;
		}
		
		
		/// <summary>
		/// Executes a SQL statement applying the given array of parametera 
		/// and returns the results in a <code>"System.DataTable"</code>.
		/// </summary>
		/// <param name="SQL">The SQL Statement</param>
		/// <param name="Parameters">
		/// The Parameters to apply to the SQL statement.
		/// </param>
		/// <returns>A DaTatable containing the result set.</returns>
		public virtual DataTable GetSqlAsDataTable(string SQL, DbParameter[] Parameters) {
			DbCommand cmd;
			DataSet ds = new DataSet();
			
			DataAdapter da;
			
			
			using (cmd = Cn.CreateCommand()) {
				cmd.CommandText = SQL;
				cmd.Parameters.AddRange(Parameters);
				da = this.CreateDataAdapter(cmd);
				da.Fill(ds);
			}
			
			return ds.Tables[0];
		}
		

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
				Cn.ConnectionString,
				"\n",
				Cn.State.ToString()
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
		public virtual DataTable GetTableAsDataTable (string Table) {
			return this.GetSqlAsDataTable(
				String.Format("SELECT * FROM [{0}]", Table)
			);
		}
		
		
		/// <summary>
		/// Retrieves the number of rows in a table.
		/// </summary>
		/// <param name="Table">The name of the table.</param>
		/// <returns>The number of tables of an integer.</returns>
		public virtual int GetTableRowCount(String Table) {
			IDbCommand cmd;
			IDataReader rdr;
			int ret;
			using (cmd = Cn.CreateCommand()) {
				cmd.CommandText = 
					String.Format("SELECT COUNT(*) FROM [{0}]", Table);
				rdr = cmd.ExecuteReader();
				rdr.Read();
				try {
					ret = (int) rdr[0];
				} 
				/* 
				 * The previous works with access databases, but trips an 
				 * InvalidCastException in SQLite databases. Its probably that whole, 
				 * "Lets make a loosley typed database." mentality of Dr. Hib.
				 */
				catch (InvalidCastException) {
					ret = int.Parse(rdr[0].ToString());
				}
				rdr.Close();
			}
			return ret;
		}
		

		/// <summary>
		/// Gets a list of tables in the database.
		/// </summary>
		/// <returns>
		/// A list of table names as an array of strings.
		/// </returns>
		public virtual string [] GetTables() {
			int RowCount;
			int i = 0;
			string [] Tables;
			DataTable dt = null;
			
			dt = Cn.GetSchema("tables");
			RowCount = dt.Rows.Count;
			Tables = new string[RowCount];
			for (i = 0; i < RowCount; i++) {
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

			dt = Cn.GetSchema("views");
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
		public virtual string GetViewSQL(string View) {
			DataTable dt;
			dt = Cn.GetSchema
				("Views", new string[] {null, null, View});
			return (string) dt.Rows[0]["VIEW_DEFINITION"];
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
			return this.GetSqlAsDataTable(SQL).DataSet.GetXml();
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
			this.GetSqlAsDataTable(SQL).DataSet.WriteXml(File, XmlWriteMode.WriteSchema);
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
		/*
		protected static void DbaException (DbException e) {
			string Title;
		
			Title = String.Format("DBA Exception (HRESULT: {0})", e.ErrorCode);
			System.Windows.Forms.MessageBox.Show
				(e.ToString() , Title);
		}
		*/
		
		
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
		
		
		/// <summary>
		/// Converts a DataTable to the INSERT queries neccessary
		/// to recreate it.
		/// </summary>
		/// <param name="dt">The datatable</param>
		/// <returns>
		/// The INSERT statements neccessary to reproduce the existing query.
		/// <remarks>
		/// The DDL (CREATE TABLE) statement neccessary to define the table
		/// is not created.
		/// </remarks>
		/// </returns>
		public static string DataTable2DML(DataTable dt) {
			int numFields;
			string [] Fields;
			string [] FieldValues;
			StringBuilder DML = new StringBuilder();
			StringBuilder InsertFormatter = new StringBuilder();
			
			numFields = dt.Columns.Count;
			Fields = new string[numFields];
			FieldValues = new string[numFields];
			//TODO: I need to scrub output here.
			for (int i = 0; i < numFields; i++) {
				Fields[i] = dt.Columns[i].ColumnName;
			}
			
			InsertFormatter.AppendFormat
				("INSERT INTO [{0}] ([{1}]) VALUES(", 
				 dt.TableName,
				 String.Join("], [", Fields));
			for (int i = 0; i < numFields - 1; i++) {
				InsertFormatter.AppendFormat(@"'{{{0}}}', ", i);
			}
			InsertFormatter.AppendFormat(@"'{{{0}}}');", numFields - 1);
			
			foreach(DataRow row in dt.Rows) {
				for (int i = 0; i < numFields; i++) {
					FieldValues[i] = row[Fields[i]].ToString();
				}
				DML.AppendFormat
					(InsertFormatter.ToString(), FieldValues);
				DML.AppendLine();
			}

			return DML.ToString();
		}
		
		
		/// <summary>
		/// Gets a column from a DataTable and returns it as a string of arrays.
		/// </summary>
		/// <param name="Table">The DataTable to get the data from.</param>
		/// <param name="Column">The name of the column.</param>
		/// <returns>The contents of the column as a string of arrays</returns>
		public static string [] GetColumnAsStringArray (DataTable Table, string Column) {
			ArrayList Rows = new ArrayList();
			foreach(DataRow Row in Table.Rows) {
				Rows.Add(Row[Column].ToString());
			}
			return (string []) Rows.ToArray(typeof(System.String));
		}
		
		
		/// <summary>
		/// Gets a column from a table and returns it as a string of arrays.
		/// </summary>
		/// <param name="Table">The DataTable to get the data from.</param>
		/// <param name="Column">The name of the column.</param>
		/// <param name="Distinct">Set to true to only return unique values.</param>
		/// <returns>The contents of the column as a string of arrays.</returns>
		public static string [] GetColumnAsStringArray (DataTable Table, string Column, bool Distinct) {
			if (!Distinct) {
				return GetColumnAsStringArray (Table, Column);
			} else {
				List<string> ColumnValues = new List<string>();
				string Val;
				foreach(DataRow Row in Table.Rows) {
					Val = (string)Row[Column];
					if (!ColumnValues.Contains(Val)) {
						ColumnValues.Add(Val);
					}
				}
				ColumnValues.Sort();
				return ColumnValues.ToArray();
			}
			
		}
				
		#endregion
	}
}
