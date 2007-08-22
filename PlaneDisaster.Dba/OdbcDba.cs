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
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Text;

namespace PlaneDisaster.Dba
{
	/// <summary>
	/// Description of OdbcDba.
	/// </summary>
	public class OdbcDba : dba
	{	
		private OdbcConnection _Cn;
		//private bool _supportsProcedures;
		//private bool _supportsViews;
		
		#region Properties
		
		/// <summary>The Odbc database connection</summary>
		protected override System.Data.Common.DbConnection Cn {
			get {
				return this._Cn;
			}
			set {
				this._Cn = (OdbcConnection) value;
			}
		}
		
		
		/// <summary>
		/// Returns true if the connected database provider supports procedures.
		/// </summary>
		/// <exception cref="NotImplementedException">
		/// Thrown for databases whose procedure support is not explicitly known.
		/// Currently this mean everything but MS Access databases.
		/// </exception>
		public override bool SupportsProcedures {
			get {
				if (Connected) {
					if (_Cn.Driver != "odbcjt32.dll") {
						string msg = string.Format ("Currently the OdbcDba.SupportsProcedures property may only be called when a Microsft Access database is being connected. You are connected with the {0} driver", _Cn.Driver);
						throw new NotImplementedException(msg);
					}
					return true;
					//return _supportsProcedures;
				} else {
					throw new InvalidOperationException("The value of OdbcDba.SupportsProcedures depends on the database that it is connected to.");
				}
			}
		}
		

		/// <summary>
		/// Returns true if the connected database provider supports views.
		/// </summary>
		/// <exception cref="NotImplementedException">
		/// Thrown for databases whose view support is not explicitly known.
		/// Currently this mean everything but MS Access databases.
		/// </exception>
		public override bool SupportsViews {
			get {
				if (Connected) {
					if (_Cn.Driver != "odbcjt32.dll") {
						string msg = string.Format ("Currently the OdbcDba.SupportsViews property may only be called when a Microsft Access database is being connected. You are connected with the {0} driver", _Cn.Driver);
						throw new NotImplementedException(msg);
					}
					return true;
					//return _supportsViews;
				} else {
					throw new InvalidOperationException("The value of OdbcDba.SupportsViews depends on the database that it is connected to.");
				}
			}
		}
		
		#endregion Properties

		/// <summary>
		/// Connect to the specified DSN
		/// </summary>
		/// <param name="ConnStr">DSN to connect to.</param>
		public void ConnectDSN(string ConnStr) {
			Cn = new OdbcConnection(ConnStr);
			Cn.Open();
		}
		

		/// <summary>
		/// Connect to the specified MDB file.
		/// </summary>
		/// <param name="File">MDB file to connect to.</param>
		public void ConnectMDB(string File) {
			Cn = new OdbcConnection();
			Cn.ConnectionString = String.Format
				("Driver={{Microsoft Access Driver (*.mdb)}};Dbq={0};Uid=Admin;Pwd=;", File);
			Cn.Open();
		}
		
		
		/// <summary>
		/// Connect to the specified MDB file with the specified passwd.
		/// </summary>
		/// <param name="File">MDB file to connect to.</param>
		/// <param name="Password">
		/// The password to connecto to the database as.
		/// </param>
		public void ConnectMDB(string File, string Password) {
			Cn = new OdbcConnection();
			Cn.ConnectionString = String.Format
					("Driver={{Microsoft Access Driver (*.mdb)}};Dbq={0};Uid=Admin;Pwd={1};", File, Password);
			Cn.Open();
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
			
			dt = Cn.GetSchema("Columns", new string [] {null, null, Table, null});
			numCols = dt.Rows.Count;
			Tables = new string[numCols];
			for (int i = 0; i < numCols; i++) {
				Tables[i] = (string) dt.Rows[i]["COLUMN_NAME"];
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
			cmd.CommandText = String.Format("SELECT * FROM [{0}]", Table);
			OdbcDataAdapter da = new OdbcDataAdapter(cmd);
			da.Fill(ret);
			ret.TableName = Table;
			return ret;
		}
		
	}
}
