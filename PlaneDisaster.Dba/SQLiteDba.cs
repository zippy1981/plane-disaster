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
 * Date: 8/15/2006
 * Time: 6:07 PM
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace PlaneDisaster.Dba
{
	/// <summary>
	/// Description of SQLiteDba.
	/// </summary>
	public class SQLiteDba : dba
	{
		private SQLiteConnection _Cn;
		private string _ConnStr;
		
		#region Properties
		
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


        /// <summary>Returns false: SQLite is not a Microsoft Access database.</summary>
        public override bool IsAccessDatabase
        {
            get { return false; }
        }
		
		
		/// <summary>
		/// Returns false as SQLite does not support procedures.
		/// </summary>
		public override bool SupportsProcedures {
			get { return false; }
		}
		

		/// <summary>
		/// Returns false as SQLite does support views.
		/// </summary>
		public override bool SupportsViews {
			get { return true; }
		}
		
		#endregion Properties
		
		
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
		protected override DataAdapter CreateDataAdapter(DbCommand cmd) {
			return new SQLiteDataAdapter((SQLiteCommand) cmd);
		}
		
		
		/// <summary>
		/// Takes the passed DataTable <c>dt</c> and creates a table in the currently
		/// open SQLite database with equivilant schama and values. If the currently 
		/// open database  has a table of the same name, attempts to append the rows
		/// in the DataTable to it.
		/// </summary>
		/// <remarks>
		/// <para>This qualifies as one of the greatest dirty hacks that just works.
		/// I don't scrub parameters, and it will probably only work with DataTables
		/// from Microsoft Access Databases. I am not all emcompasing with
		/// data types. I simple kept feeding it the data I needed to feed it until
		/// it worked. If it encounters a datatype it doesn't know how to deal with it
		/// panics and throws an exception.</para>
		/// <para>Columns get a declared type System.Data.SQLite reads back as the same
		/// .NET type: whole numbers are INTEGER, float and double are REAL, decimal is
		/// NUMERIC, and bool, DateTime and Guid are BOOLEAN, DATETIME and GUID. Dates are
		/// stored as ISO 8601 text, which SQLite's date functions read. Any other type
		/// throws a <see cref="DataException"/>.</para>
		/// <para>All INSERT statements are done as one transaction. The reason for this
		/// is that no writes are performed to the database until a transaction is
		/// committed and therefore the difference in execution time between 
		/// inserting a thousand rows in one transaction and inserting a thousand rows 
		/// without transactions is greater than a thousand fold. This has been tested on
		/// a table containing 179442 rows with 5 numeric column and one DateTime column
		/// that was inserted as a text column. Tests much be performed on even larger 
		/// datasets to see if there is a point of noticeable performance degradation.
		/// </para>
		/// </remarks>
		/// <param name="dt">The DataTable to place in a new SQLite database.</param>
		public void DataTable2SQLiteTable(DataTable dt) {
			/* Check to make sure that the DataTable has a name */
			string TableNameError =
				"DataTable passed to DataTable2SQLiteTable() must have the TableName Property " +
				"set to a non null, non empty string (\"\") value.";
			if (dt.TableName == null) {
				throw new ArgumentNullException (TableNameError);
			} else if (dt.TableName == "") {
				throw new ArgumentException (TableNameError);
			}
			
			/* Create the table */
			using (SQLiteCommand cmd = (SQLiteCommand) Cn.CreateCommand()) {
				StringBuilder DDL = new StringBuilder();
				
				DDL.Append("CREATE TABLE [").Append(dt.TableName).Append("] (");
				List<string> Cols = new List<string>();

				/* Figure out what datatypes to assign to the columns */
				foreach (DataColumn col in dt.Columns) {
					Cols.Add(String.Concat("[", col.ColumnName, "] ", SQLiteColumnType(col.DataType)));
				}
				DDL.Append(String.Join(", ", Cols.ToArray()));
				DDL.Append(")");
				cmd.CommandText = DDL.ToString();
				cmd.ExecuteNonQuery();
			}
			
			using (SQLiteCommand cmd = (SQLiteCommand) Cn.CreateCommand()) {
				/* Create the INSERT INTO statement. */
				StringBuilder DML = new StringBuilder();
				
				DML.Append("INSERT INTO [").Append(dt.TableName).Append("] ([");
				List<string> ColumnName = new List<string>();
				List<string> ParameterName = new List<string>();
				foreach (DataColumn col in dt.Columns) {
					ColumnName.Add(col.ColumnName);
					// Positional names, so a column name needn't be a valid parameter name.
					ParameterName.Add("@p" + ParameterName.Count);
				}
				DML.Append(String.Join("], [", ColumnName.ToArray()));
				DML.Append("]) VALUES (");
				DML.Append(String.Join(", ", ParameterName.ToArray()));
				DML.Append(")");
				cmd.CommandText = DML.ToString();

				/* One parameter per column; each row sets their values. */
				foreach (string Name in ParameterName) {
					cmd.Parameters.Add(new SQLiteParameter(Name));
				}

				/* It is much faster if this is done as one transaction. */
				using (SQLiteTransaction Transaction = (SQLiteTransaction) Cn.BeginTransaction()) {
					cmd.Transaction = Transaction;
					DataTableReader rdr = dt.CreateDataReader();
					while(rdr.Read()) {
						for (int i = 0; i < rdr.FieldCount; i++) {
							cmd.Parameters[i].Value = rdr[i];
						}
						cmd.ExecuteNonQuery();
					}
					Transaction.Commit();
				}
			}
		}


		/// <summary>
		/// The SQLite column type for a DataColumn's type. The declared type also tells
		/// System.Data.SQLite which .NET type to read the column back as.
		/// </summary>
		private static string SQLiteColumnType(Type type) {
			if (type == typeof(string) || type == typeof(char) || type == typeof(TimeSpan)) {
				return "TEXT";
			} else if (type == typeof(DateTime)) {
				return "DATETIME";
			} else if (type == typeof(bool)) {
				return "BOOLEAN";
			} else if (type == typeof(Guid)) {
				return "GUID";
			} else if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)
			           || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong)) {
				return "INTEGER";
			} else if (type == typeof(float) || type == typeof(double)) {
				return "REAL";
			} else if (type == typeof(decimal)) {
				return "NUMERIC";
			} else if (type == typeof(byte[])) {
				return "BLOB";
			}
			throw new DataException
				(String.Concat("DataTable2SQLiteTable() doesn't know how to map columns of type ", type.ToString()));
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
