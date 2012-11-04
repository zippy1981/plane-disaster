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
 * Date: 8/21/2006
 * Time: 2:00 AM
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Win32;

namespace PlaneDisaster.Dba
{
    /// <summary>
	/// JetSQL is the "code name" for the sql engine behind access.
	/// It's auctually built into windows. Microsoft Access is just a fancy
	/// front end.
	/// </summary>
	public class JetSqlUtil
    {
        #region PInvoke
        private enum ODBC_Constants : int {
    		ODBC_ADD_DSN = 1,
    		ODBC_CONFIG_DSN,
    		ODBC_REMOVE_DSN,
    		ODBC_ADD_SYS_DSN,
    		ODBC_CONFIG_SYS_DSN,
    		ODBC_REMOVE_SYS_DSN,
    		ODBC_REMOVE_DEFAULT_DSN,
		}
        
        private enum SQL_RETURN_CODE : int
        {
            SQL_ERROR = -1,
            SQL_INVALID_HANDLE = -2,
            SQL_SUCCESS = 0,
            SQL_SUCCESS_WITH_INFO = 1,
            SQL_STILL_EXECUTING = 2,
            SQL_NEED_DATA = 99,
            SQL_NO_DATA = 100
        }
		
		[DllImport("ODBCCP32.DLL",CharSet=CharSet.Unicode, SetLastError=true)]
		private static extern int SQLConfigDataSource (int hwndParent, ODBC_Constants fRequest, string lpszDriver, string lpszAttributes);

        [DllImport("ODBCCP32.DLL", CharSet = CharSet.Auto)]
        private static extern SQL_RETURN_CODE SQLInstallerError(int iError, ref int pfErrorCode, StringBuilder lpszErrorMsg, int cbErrorMsgMax, ref int pcbErrorMsg);
        #endregion

        private static string OdbcProviderName = null;
        private static string OleDbProviderName = null;

        /// <summary>The url for downloading the MS Access Redistributable.</summary>
        /// <remarks>This has changed on me in the past.</remarks>
        private const string ACCESS_REDISTRIBUTABLE_URL = "http://www.microsoft.com/en-us/download/details.aspx?id=13255";

		/// <summary>
		/// Compacts an access database
		/// </summary>
		/// <param name="fileName">The name of the databse to compact.</param>
		public static void CompactMDB (string fileName) {
			string attributes = 
				String.Format("COMPACT_DB=\"{0}\" \"{0}\" General\0", Path.GetFullPath(fileName));
			int retCode = SQLConfigDataSource
				(0, ODBC_Constants.ODBC_ADD_DSN,
                GetOdbcProviderName(), attributes);
			if (retCode == 0) {
                int errorCode = 0;
                int resizeErrorMesg = 0;
                var sbError = new StringBuilder(512);
                SQLInstallerError(1, ref errorCode, sbError, sbError.MaxCapacity, ref resizeErrorMesg);
                throw new ApplicationException(string.Format("Can not compact database:: {0}. Error: {1}", fileName, sbError));
			}
		}


        /// <summary>
        /// Creates an Access 2003 database. If the filename specified exists it is 
        /// overwritten.
        /// </summary>
        /// <param name="fileName">The name of the databse to create.</param>
        /// <param name="version">The version of the database to create.</param>
        public static void CreateMDB (string fileName, AccessDbVersion version = AccessDbVersion.Access2003) {
			;
			if (File.Exists(fileName)) {
				File.Delete(fileName);
			}

            string command = "";
            switch (version)
            {
                case AccessDbVersion.Access95:
                    command = "CREATE_DBV3";
                    break;
                case AccessDbVersion.Access2000:
                    command = "CREATE_DBV4";
                    break;
                case AccessDbVersion.Access2003:
                    command = "CREATE_DB";
                    break;
            }

			string attributes = String.Format("{0}=\"{1}\" General\0", command, fileName);
            int retCode = SQLConfigDataSource 
                (0, ODBC_Constants.ODBC_ADD_DSN,
                 GetOdbcProviderName(), attributes);
			if (retCode == 0)
			{
			    int errorCode = 0 ;
                int  resizeErrorMesg = 0 ;
			    var sbError = new StringBuilder(512);
			    SQLInstallerError(1, ref errorCode, sbError, sbError.MaxCapacity, ref resizeErrorMesg);
				throw new ApplicationException(string.Format("Cannot create file: {0}. Error: {1}", fileName, sbError));
			}
		}

        /// <summary>
        /// Gets the name of the best Microsoft Access Odbc provider to use.
        /// </summary>
        /// <returns>
        /// One of the following Access providers (in order of preference):
        /// <list type="value">
        /// <value>Microsoft Access Driver (*.mdb, *.accdb)</value>
        /// <value>Microsoft Access Driver (*.mdb)</value>
        /// </list>
        /// </returns>
        internal static string GetOdbcProviderName()
        {
            if (string.IsNullOrEmpty(OdbcProviderName))
            {
                var odbcRegKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\ODBC\\ODBCINST.INI\\ODBC Drivers", false);
                var drivers = new List<string>(odbcRegKey.GetValueNames());
                if (drivers.Contains("Microsoft Access Driver (*.mdb, *.accdb)"))
                {
                    OdbcProviderName = "Microsoft Access Driver (*.mdb, *.accdb)";
                }
                else if (drivers.Contains("Microsoft Access Driver (*.mdb)"))
                {
                    OdbcProviderName = "Microsoft Access Driver (*.mdb)";
                }
                else
                {
                    //TODO: Condider checking for 32 versus 64 bit.
                    //TODO: Find a better exception type. http://stackoverflow.com/questions/7221703/what-is-the-proper-exception-to-throw-if-an-odbc-driver-cannot-be-found
                    string msg = string.Format("Cannot find an ODBC driver for Microsoft Access. Please download the Microsoft Access Database Engine 2010 Redistributable. {0}", ACCESS_REDISTRIBUTABLE_URL);
                    throw new InvalidOperationException(msg);
                }
            }
            return OdbcProviderName;
        }

        /// <summary>
        /// Gets the name of the best Microsoft Access OleDb provider to use.
        /// </summary>
        /// <returns>
        /// One of the following Access providers (in order of preference):
        /// <list type="value">
        /// <value>Microsoft Access Driver (*.mdb, *.accdb)</value>
        /// <value>Microsoft Access Driver (*.mdb)</value>
        /// </list>
        /// </returns>
        internal static string GetOleDbProviderName()
        {
            if (string.IsNullOrEmpty(OleDbProviderName))
            {
                var clsidRegKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\CLSID", false);
                var oleDbProviderNames = new List<string>();
                foreach (var subKeyName in clsidRegKey.GetSubKeyNames())
                {
                    var subKey = clsidRegKey.OpenSubKey(subKeyName);
                    var oleDbSubKey = subKey.OpenSubKey("OLE DB Provider");
                    if (subKey.GetValue("OLEDB_SERVICES") != null && oleDbSubKey != null)
                    {
                        oleDbProviderNames.Add((string) oleDbSubKey.GetValue(""));
                    }
                }
                if (oleDbProviderNames.Contains("Microsoft Office 12.0 Access Database Engine OLE DB Provider"))
                {
                    OleDbProviderName = "Microsoft Office 12.0 Access Database Engine OLE DB Provider";
                }
                else if (oleDbProviderNames.Contains("Microsoft Jet 4.0 OLE DB Provider"))
                {
                    OleDbProviderName = "Microsoft Jet 4.0 OLE DB Provider";
                }
                else
                {
                    //TODO: Condider checking for 32 versus 64 bit.
                    //TODO: Find a better exception type. http://stackoverflow.com/questions/7221703/what-is-the-proper-exception-to-throw-if-an-OleDb-driver-cannot-be-found
                    string msg = string.Format("Cannot find an OleDb driver for Microsoft Access. Please download the Microsoft Access Database Engine 2010 Redistributable. {0}", ACCESS_REDISTRIBUTABLE_URL);
                    throw new InvalidOperationException(msg);
                }
            }
            return OleDbProviderName;
        }
		
		/// <summary>
		/// Repairs an access database
		/// </summary>
		/// <param name="fileName">The name of the databse to repair.</param>
		public static void RepairMDB (string fileName) {
			string attributes = 
				String.Format("REPAIR_DB=\"{0}\"\0", fileName);
			int retCode = SQLConfigDataSource
				(0, ODBC_Constants.ODBC_ADD_DSN, 
				GetOdbcProviderName(), attributes);
			if (retCode == 0) {
                int errorCode = 0;
                int resizeErrorMesg = 0;
                var sbError = new StringBuilder(512);
                SQLInstallerError(1, ref errorCode, sbError, sbError.MaxCapacity, ref resizeErrorMesg);
                throw new ApplicationException(string.Format("Cannot repair database: {0}. Error: {1}", fileName, sbError));
			}
		}

	}
}
