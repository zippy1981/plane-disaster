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
using System.Data.Odbc;
using System.Text;

namespace PlaneDisaster
{
	/// <summary>
	/// Description of OdbcDba.
	/// </summary>
	public class OdbcDba : dba
	{	
		private OdbcConnection _Cn;
		private string _ConnStr;

		
		/// <summary>The Odbc database connection</summary>
		protected override System.Data.Common.DbConnection Cn {
			get {
				return this._Cn;
			}
			set {
				this._Cn = (OdbcConnection) value;
			}
		}
		
		
		/// <summary>The Odbc Connection string</summary>
		protected string ConnStr {
			get { return this._ConnStr; }
			set { this._ConnStr = value; }
		}
		
		
		/// <summary>
		/// Connect to the previously defined connection string.
		/// </summary>
		public void Connect (){
			this.Cn = new OdbcConnection(ConnStr);
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
		/// Factory method to create a new DataAdapter of the OdbcDataAdapter type.
		/// </summary>
		/// <param name="cmd">The select fommand for the data adapter.</param>
		/// <returns>A populated DataAdapter of the OdbcDataAdapter type.</returns>
		protected override DataAdapter CreateDataAdapter(DbCommand cmd) { return new OdbcDataAdapter((OdbcCommand) cmd);}

		
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
				dt = Cn.GetSchema("Columns", new string [] {null, null, Table, null});
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
		/// Returns all rows in a table in a 
		/// <code>System.DataGridView</code>.
		/// </summary>
		/// <param name="Table">The name of the table</param>
		/// <returns>A DataGridView containing the result set.</returns>
		public override DataTable GetTableAsDataTable (string Table) {
			DataTable ret = new DataTable();
			OdbcCommand cmd = (OdbcCommand)Cn.CreateCommand();
			cmd.CommandType = CommandType.TableDirect;
			cmd.CommandText = Table;
			OdbcDataAdapter da = new OdbcDataAdapter(cmd);
			da.Fill(ret);
			return ret;
		}
		
	}
}
