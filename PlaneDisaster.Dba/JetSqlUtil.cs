/*
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
 */

/*
 * Created by SharpDevelop.
 * Author:		Justin Dearing <zippy1981@gmail.com>
 * Date: 8/21/2006
 * Time: 2:00 AM
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PlaneDisaster.Dba
{
	/// <summary>
	/// JetSQL is the "code name" for the sql engine behind access.
	/// It's auctually built into windows. Microsoft Access is just a fancy
	/// front end.
	/// </summary>
	public class JetSqlUtil
	{
		internal enum ODBC_Constants : int {
    		ODBC_ADD_DSN = 1,
    		ODBC_CONFIG_DSN,
    		ODBC_REMOVE_DSN,
    		ODBC_ADD_SYS_DSN,
    		ODBC_CONFIG_SYS_DSN,
    		ODBC_REMOVE_SYS_DSN,
    		ODBC_REMOVE_DEFAULT_DSN,
		}
		
		[DllImport("ODBCCP32.DLL",CharSet=CharSet.Unicode, SetLastError=true)]
		internal static extern int SQLConfigDataSource (int hwndParent, ODBC_Constants fRequest, string lpszDriver, string lpszAttributes);
		
		
		/// <summary>
		/// Compacts an access database
		/// </summary>
		/// <param name="FileName">The name of the databse to compact.</param>
		public static void CompactMDB (string FileName) {
			int retCode;
			string Attributes = 
				String.Format("COMPACT_DB=\"{0}\" \"{0}\" General\0", FileName);
			retCode = SQLConfigDataSource
				(0, ODBC_Constants.ODBC_ADD_DSN, 
				"Microsoft Access Driver (*.mdb)", Attributes);
			if (retCode == 0) {
				throw new ApplicationException("Cannot compact database: " + FileName);
			}
		}
		
		
		/// <summary>
		/// Creates an access database. If the filename specified exists it is 
		/// overwritten.
		/// </summary>
		/// <param name="FileName">The name of the databse to create.</param>
		public static void CreateMDB (string FileName) {
			int retCode;
			if (File.Exists(FileName)) {
				File.Delete(FileName);
			}
			string Attributes = 
				String.Format("CREATE_DB=\"{0}\" General\0", FileName);
			retCode = SQLConfigDataSource
				(0, ODBC_Constants.ODBC_ADD_DSN, 
				"Microsoft Access Driver (*.mdb)", Attributes);
			if (retCode == 0) {
				throw new ApplicationException("Cannot create file: " + FileName);
			}
		}
		
		
		/// <summary>
		/// Creates an Access 95 database. If the filename specified exists it is 
		/// overwritten.
		/// </summary>
		/// <param name="FileName">The name of the databse to create.</param>
		public static void CreateMDBv3 (string FileName) {
			int retCode;
			if (File.Exists(FileName)) {
				File.Delete(FileName);
			}
			string Attributes = 
				String.Format("CREATE_DBV3=\"{0}\" General\0", FileName);
			retCode = SQLConfigDataSource
				(0, ODBC_Constants.ODBC_ADD_DSN, 
				"Microsoft Access Driver (*.mdb)", Attributes);
			if (retCode == 0) {
				throw new ApplicationException("Cannot create file: " + FileName);
			}
		}
		
		
		/// <summary>
		/// Creates an Access 2000 database. If the filename specified exists it is 
		/// overwritten.
		/// </summary>
		/// <param name="FileName">The name of the databse to create.</param>
		public static void CreateMDBv4 (string FileName) {
			int retCode;
			if (File.Exists(FileName)) {
				File.Delete(FileName);
			}
			string Attributes = 
				String.Format("CREATE_DBV4=\"{0}\" General\0", FileName);
			retCode = SQLConfigDataSource
				(0, ODBC_Constants.ODBC_ADD_DSN, 
				"Microsoft Access Driver (*.mdb)", Attributes);
			if (retCode == 0) {
				throw new ApplicationException("Cannot create file: " + FileName);
			}
		}
		
		
		
		/// <summary>
		/// Repairs an access database
		/// </summary>
		/// <param name="FileName">The name of the databse to repair.</param>
		public static void RepairMDB (string FileName) {
			int retCode;
			string Attributes = 
				String.Format("REPAIR_DB=\"{0}\"\0", FileName);
			retCode = SQLConfigDataSource
				(0, ODBC_Constants.ODBC_ADD_DSN, 
				"Microsoft Access Driver (*.mdb)", Attributes);
			if (retCode == 0) {
				throw new ApplicationException("Cannot repair database: " + FileName);
			}
		}

	}
}
