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
using System.Data.Odbc;
using System.Text;

namespace PlaneDisaster
{
	/// <summary>
	/// Description of OdbcDba.
	/// </summary>
	public class OdbcDba : dba
	{	
		private OdbcConnection _cn;
		private string strConnStr;

		
		/// <summary>The Odbc database connection</summary>
		protected override System.Data.Common.DbConnection cn {
			get {
				return this._cn;
			}
			set {
				this._cn = (OdbcConnection) value;
			}
		}
		
		
		/// <summary>The Odbc Connection string</summary>
		protected string ConnStr {
			get { return this.strConnStr; }
			set { this.strConnStr = value; }
		}
		
		
		/// <summary>
		/// Connect to the previously defined connection string.
		/// </summary>
		public void Connect (){
			this.cn = new OdbcConnection(ConnStr);
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
		/// Gets the names of the columns in a table
		/// </summary>
		/// <param name="Table">The Name of the table</param>
		/// <returns>The column names as an array of strings.</returns>
		public override string [] GetColumnNames (string Table) {
			DataTable dt = new DataTable();
			int numCols;
			string [] Tables;
			
			try {
				dt = cn.GetSchema("Columns", new string [] {null, null, Table, null});
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
		/// Executes a SQL statement and returns the results in a 
		/// <code>System.DataGridView</code>
		/// </summary>
		/// <param name="SQL">The SQL Statement</param>
		/// <returns>A DataGridView containing the result set.</returns>
		public override DataSet GetSqlAsDataSet(string SQL) {
			DataSet ds = new DataSet();
			OdbcDataAdapter da = new OdbcDataAdapter(SQL, (OdbcConnection)this.cn);
			
			da.Fill(ds, "qryTemp");
			return ds;
		}
	}
}
