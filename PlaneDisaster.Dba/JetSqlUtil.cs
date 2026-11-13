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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

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

		/// <summary>
		/// Compacts an access database
		/// </summary>
		/// <remarks>
		/// The Access Database Engine (ACE) driver can't compact a file onto itself, so this compacts
		/// into a temporary file next to the database and then replaces the database with it.
		/// </remarks>
		/// <param name="fileName">The name of the databse to compact.</param>
		public static void CompactMDB (string fileName) {
			string fullName = Path.GetFullPath(fileName);
			string tempName = fullName + ".compacting" + Path.GetExtension(fullName);
			if (File.Exists(tempName)) {
				File.Delete(tempName);
			}
			// The ACE ODBC driver fails COMPACT_DB, so compact through ACE's DAO engine when it is there.
			AccessDriver driver = AccessDrivers.Find(AccessDriverKind.Odbc, fullName);
			Type daoEngineType = driver.IsJet ? null : Type.GetTypeFromProgID("DAO.DBEngine.120");
			if (daoEngineType != null) {
				object dbEngine = Activator.CreateInstance(daoEngineType);
				try {
					daoEngineType.InvokeMember("CompactDatabase", BindingFlags.InvokeMethod, null, dbEngine,
						new object[] { fullName, tempName });
				} finally {
					Marshal.ReleaseComObject(dbEngine);
				}
				File.Delete(fullName);
				File.Move(tempName, fullName);
				return;
			}
			string attributes =
				String.Format("COMPACT_DB=\"{0}\" \"{1}\" General\0", fullName, tempName);
			int retCode = SQLConfigDataSource
				(0, ODBC_Constants.ODBC_ADD_DSN,
                driver.Name, attributes);
			if (retCode == 0) {
                int errorCode = 0;
                int resizeErrorMesg = 0;
                var sbError = new StringBuilder(512);
                SQLInstallerError(1, ref errorCode, sbError, sbError.MaxCapacity, ref resizeErrorMesg);
                throw new ApplicationException(string.Format("Can not compact database:: {0}. Error: {1}", fileName, sbError));
			}
			File.Delete(fullName);
			File.Move(tempName, fullName);
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

            // The driver has to write the format, and Access 95 files are Jet 3, which newer drivers dropped.
            AccessFileFormat format = version == AccessDbVersion.Access95 ? AccessFileFormat.Jet3 : AccessFileFormat.Jet4;
            AccessDriver driver = AccessDrivers.Find(AccessDriverKind.Odbc, format);
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
                 driver.Name, attributes);
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
		/// Repairs an access database
		/// </summary>
		/// <remarks>
		/// The Access Database Engine (ACE) driver has no REPAIR_DB command because compacting repairs
		/// the database, so with that driver this compacts it.
		/// </remarks>
		/// <param name="fileName">The name of the databse to repair.</param>
		public static void RepairMDB (string fileName) {
			AccessDriver driver = AccessDrivers.Find(AccessDriverKind.Odbc, fileName);
			if (!driver.IsJet) {
				CompactMDB(fileName);
				return;
			}
			string attributes =
				String.Format("REPAIR_DB=\"{0}\"\0", fileName);
			int retCode = SQLConfigDataSource
				(0, ODBC_Constants.ODBC_ADD_DSN, 
				driver.Name, attributes);
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
