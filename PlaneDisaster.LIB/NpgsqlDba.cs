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

using Npgsql;
using System;
using System.Data;
using System.Data.Common;
using System.Text;

namespace PlaneDisaster
{
	/// <summary>
	/// Description of NpgsqlDba.
	/// </summary>
	public class NpgsqlDba : dba
	{
		

		/// <summary>The Npgsql database connection</summary>
		protected new NpgsqlConnection cn;
		private string strConnStr;
		
		
		/// <summary>The Npgsql Connection string</summary>
		protected string ConnStr {
			get { return this.strConnStr; }
			set { this.strConnStr = value; }
		}
		
		
		/// <summary>
		/// Connect to the previously defined connection string.
		/// </summary>
		public void Connect (){
			this.cn = new NpgsqlConnection(ConnStr);
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
		/// Gets a column from a table and returns it as a string of arrays.
		/// </summary>
		/// <param name="Table">The name of the table or view.</param>
		/// <param name="strCol">The name of the column.</param>
		/// <returns>The contents of the column as a string of arrays.</returns>
		public override string [] GetColumnAsStringArray (string Table, string strCol) {
			string SQL;
			NpgsqlCommand cmd;
			NpgsqlDataReader rdr;
			long numRows;
			string [] Rows;
			Int64 curRowNum = 0;	

			NpgsqlTransaction trans = this.cn.BeginTransaction();
			SQL = string.Concat("SELECT COUNT(*) FROM ", Table);
			cmd =new NpgsqlCommand(SQL, this.cn);
			rdr = cmd.ExecuteReader();
			rdr.Read();
			
			numRows = rdr.GetInt64(0);
			rdr.Close();
			rdr = null;
			cmd = null;
			if (numRows == 0) {
				return new string [] {""};
			}
			Rows = new string [numRows];	
			
			SQL = string.Concat("SELECT ", strCol, " FROM ", Table);
			cmd =new NpgsqlCommand(SQL, this.cn);
			cmd.Parameters.Add("Column", strCol);
			rdr = cmd.ExecuteReader();
				
			while (rdr.Read()) {
				Rows[curRowNum] = (string) rdr[strCol];
				curRowNum++;
			}
			rdr.Close();
			trans.Commit();
			return Rows;
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
			
			try {
				dt = cn.GetSchema("Columns");
				numCols = dt.Rows.Count;
				Tables = new string[numCols];
				for (int i = 0; i < numCols; i++) {
					Tables[i] = (string) dt.Rows[i]["COLUMN_NAME"];
				}
			}
			
			catch (Exception e)
			{
				System.Diagnostics.Debug.Write(e.Message);
				System.Windows.Forms.MessageBox.Show(e.Message);
				Tables = null;
			}
			return Tables;
		}
		
		
		/// <summary>
		/// Executes a SQL query and returns the rtesults as a comma seperated file.
		/// </summary>
		/// <param name="SQL">The SQL statement to execute.</param>
		/// <returns>A string containing a CSV</returns>
		public override string GetSQLAsCSV (string SQL) {
			return GetSQLAsCSV (SQL, ",");
		}
		
		
		/// <summary>
		/// Executes a SQL query and returns the Results as a 
		/// <code>Seperator</code> seperated file.
		/// </summary>
		/// <param name="SQL">The SQL statement to execute.</param>
		/// <param name="strSeperator">
		/// The field seperator character(s).
		/// </param>
		/// <returns>A string containing the table with rows seperated by 
		/// newlines and fields seperated by <code>Seperator</code>.
		/// </returns>
		public override string GetSQLAsCSV (string SQL, string strSeperator) {
			NpgsqlCommand cmd = new NpgsqlCommand(SQL, this.cn);
			NpgsqlDataReader rdr;
			int numFields;
			string [] strFields;
			StringBuilder  CSV = new StringBuilder();
			
			try {
				rdr = cmd.ExecuteReader();
				
				numFields = rdr.FieldCount;
				strFields = new string[numFields];
				for (int i = 0; i < numFields; i++) {
					strFields[i] = rdr.GetName(i);
					CSV.AppendFormat("{0}{1}", strFields[i], strSeperator);
				}
				CSV.AppendLine();
				while (rdr.Read()) {
					foreach (string strField in strFields) {
						CSV.AppendFormat("{0}{1}", rdr[strField], strSeperator);
					}
					CSV.AppendLine();
				}
				rdr.Close();
			}
			catch (DbException e)
			{
				dba.DbaException(e);
			}
			
			return CSV.ToString();
		}
			
		
		/// <summary>
		/// Executes a SQL statement and returns the results in a 
		/// <code>System.DataGridView</code>
		/// </summary>
		/// <param name="SQL">The SQL Statement</param>
		/// <returns>A DataGridView containing the result set.</returns>
		public override DataSet GetSqlAsDataSet(string SQL) {
			DataSet ds = new DataSet();
			NpgsqlDataAdapter da = new NpgsqlDataAdapter(SQL, this.cn);
			
			da.Fill(ds, "qryTemp");
			return ds;
		}
		

		/// <summary>
		/// Gets a list of tables in the database.
		/// </summary>
		/// <returns>
		/// A list of table names as an array of strings.
		/// </returns>
		public override string [] GetTables() {			
			return this.GetColumnAsStringArray
				("information_schema.tables", "table_name");
		}
	
	
		/// <summary>
		/// Performs the SQL query specified and returns an xml 
		/// serialized datatable of the retultset.
		/// </summary>
		/// <param name="SQL">The SQL query to execute.</param>
		/// <returns>
		/// A string containing an XML serialized data tablr.
		/// </returns>
		public override string SerializeQuery (string SQL) {
			NpgsqlDataAdapter da = new NpgsqlDataAdapter(SQL, this.cn);
		 	DataSet ds = new DataSet();
		 	da.Fill(ds);
		 	return ds.GetXml();
		}
		
		
		/// <summary>
		/// Performs the SQL query specified and writes the xml serialized
		/// datatable as a file.
		/// </summary>
		/// <param name="SQL">The SQL query to execute.</param>
		/// <param name="strFile">
		/// The name of the file to write the xml to.
		/// </param>
		public override void SerializeQuery (string SQL, string strFile) {
			NpgsqlDataAdapter da = new NpgsqlDataAdapter(SQL, this.cn);
			DataSet ds = new DataSet();
		 	da.Fill(ds);
		 	ds.WriteXml(strFile, XmlWriteMode.WriteSchema);
		}
	}
}
