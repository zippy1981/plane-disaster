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
 * Date: 8/13/2006
 * Time: 10:09 PM
/*/

using System;

namespace PlaneDisaster
{
	/// <summary>
	/// Description of PostgresqlConnStrDialog.
	/// </summary>
	public sealed class PostgresqlConnStrDialog : ConnectionStringDialog, IConnectionStringDialog
	{
		/// <summary>Type Of Database driver to use.</summary>
		public enum DbDriver {
			/// <summary>Odbc database driver.</summary>
			Odbc,
			/// <summary>OleDb database driver.</summary>
			OleDb,
			/// <summary>Npgsql .Net data provider.</summary>
			Npgsql
		};
		
		DbDriver ProviderType;
		
		/// <summary>
		/// Returns a properly formatted Npgsql (PostgresSQL .NET data provider) 
		/// Connection String.
		/// </summary>
		public override string ConnectionString {
			get {
				switch (ProviderType) {
					case DbDriver.OleDb:
						return this.OleDbConnectionString;
						/*/
						 * OK this is absolutely fucking bullshit.
						 * I can pretend to accept that you are forced
						 * by the compiler to break all your switch 
						 * statements. However, it should require the break 
						 * even if I return or at least repress the 
						 * unreachable code detected warning.
						 * -- Zippy
						/*/
						//break;
					case DbDriver.Npgsql:
						return this.NpgsqlConnectionString;
						//break;
					default:
						//TODO: implement PlaneDisasterException
						//TODO: make this throw a better exception
						throw new ApplicationException ("You must chose a provider type");
				}
			}
		}
		
		/// <summary>
		/// Returns a properly formatted Npgsql (PostgresSQL .NET data provider) 
		/// Connection String.
		/// </summary>
		public string NpgsqlConnectionString {
			get {
				return String.Format
					("Server=localhost;Database={0};User ID={1};password={2}", 
					Database, User, Passwd);
			}
		}
		
		
		/// <summary>
		/// Returns a properly formatted PgOleDb Connection String.
		/// </summary>
		public string OleDbConnectionString {
			get {
				return String.Format
					("Provider={0};Data Source=localhost;location={1};User ID={2};password={3}", 
					Provider, Database, User, Passwd);
			}
		}
		
		
		/// <summary>
		/// Chose the type of provider.
		/// </summary>
		public PostgresqlConnStrDialog(DbDriver ProviderType) :base("PostgreSQL") {
			this.ProviderType = ProviderType;
		}
		
		
		
		
	}
}
