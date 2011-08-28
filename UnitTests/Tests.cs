/* 
 * Copyright 2007 Justin Dearing
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
 * Date: 8/6/2007
 * Time: 11:43 AM
 */

using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

using NUnit.Framework;
using PlaneDisaster.Dba;

namespace UnitTests
{
	//TODO: Refactor This tests class into an abstract base class that gets inherited once per DBA type. 
	[TestFixture]
	public sealed class Tests
	{
		private struct tblTestRow {
			internal int Id;
			internal string Description;
			
			internal tblTestRow(int id,string description) {
				Id = id;
				Description = description;
			}
		}
			
		private static readonly string _tempDirectory = Path.GetTempPath();
		private static readonly string _tempFilePrefix = string.Format("PlaneDisaster-UnitTest-{0}", Guid.NewGuid());
		
		#region SQL Strings
		
		//tblTest SQL
		private static readonly string _sqlCreateTable = "CREATE TABLE tblTest (id INTEGER PRIMARY KEY, description varchar(255))";
		private static readonly string _sqlDropTable = "DROP TABLE tblTest";
		private static readonly string _sqlInsertRow = "INSERT INTO tblTest (id, description) VALUES (@id, @description)";
		//v_test SQL
		//TODO: Create a test where views are created selected, dropped etc.
		private static readonly string _sqlCreateView = "CREATE VIEW v_test SELECT id, description FROM tblTest";
		private static readonly string _sqlDropView = "DROP VIEW v_test";

        #endregion SQL Strings

        /// <summary>
        /// Creates a JetSQL database.
        /// </summary>
        [Test]
        public void TestCompactMdb()
        {
            string fileName = Path.Combine(_tempDirectory, _tempFilePrefix + ".mdb");
            OleDba oleDba = null;

            try
            {
                CreateMdb(fileName);

                oleDba = new OleDba();
                oleDba.ConnectMDB(fileName);
                PopulateOleDba(oleDba);
                oleDba.Disconnect();

                JetSqlUtil.CompactMDB(fileName);

                oleDba = new OleDba();
                oleDba.ConnectMDB(fileName);
                PopulateOleDba(oleDba);
                oleDba.Disconnect();
            }
            finally
            {
                if (oleDba != null && oleDba.Connected)
                {
                    oleDba.Disconnect();
                    oleDba.Dispose();
                }
                File.Delete(fileName);
                Assert.IsFalse(File.Exists(fileName), "Failed to delete " + fileName);
            }

        }

        /// <summary>
        /// Creates a JetSQL database.
        /// </summary>
        [Test]
        public void TestCreateMdb()
        {
            string fileName = Path.Combine(_tempDirectory, _tempFilePrefix + ".mdb");
            OleDba oleDba = null;

            try
            {
                CreateMdb(fileName);

                oleDba = new OleDba();
                oleDba.ConnectMDB(fileName);
                PopulateOleDba(oleDba);
                oleDba.Disconnect();
            }
            finally
            {
                if (oleDba != null && oleDba.Connected)
                {
                    oleDba.Disconnect();
                    oleDba.Dispose();
                }
                File.Delete(fileName);
                Assert.IsFalse(File.Exists(fileName), "Failed to delete " + fileName);
            }

        }

        /// <summary>
        /// Creates a JetSQL database, creates a table, inserts some rows,
        /// deletes some rows, and deletes the database.
        /// </summary>
        [Test]
        public void TestMdb()
        {
            string fileName = Path.Combine(_tempDirectory, _tempFilePrefix + ".mdb");
            OleDba oleDba = null;

            try
            {
                CreateMdb(fileName);

                oleDba = new OleDba();
                oleDba.ConnectMDB(fileName);
                PopulateOleDba(oleDba);
                oleDba.Disconnect();

                CompactAndRepairMdb(fileName);

                oleDba.ConnectMDB(fileName);
                oleDba.ExecuteSqlCommand(_sqlDropTable);
                oleDba.Disconnect();
            }
            finally
            {
                if (oleDba != null && oleDba.Connected)
                {
                    oleDba.Disconnect();
                    oleDba.Dispose();
                }
                File.Delete(fileName);
                Assert.IsFalse(File.Exists(fileName), "Failed to delete " + fileName);
            }

        }

        /// <summary>
        /// Creates a JetSQL database.
        /// </summary>
        [Test]
        public void TestRepairMdb()
        {
            string fileName = Path.Combine(_tempDirectory, _tempFilePrefix + ".mdb");
            OleDba oleDba = null;

            try
            {
                CreateMdb(fileName);

                oleDba = new OleDba();
                oleDba.ConnectMDB(fileName);
                PopulateOleDba(oleDba);
                oleDba.Disconnect();
                
                JetSqlUtil.RepairMDB(fileName);

                oleDba = new OleDba();
                oleDba.ConnectMDB(fileName);
                PopulateOleDba(oleDba);
                oleDba.Disconnect();
            }
            finally
            {
                if (oleDba != null && oleDba.Connected)
                {
                    oleDba.Disconnect();
                    oleDba.Dispose();
                }
                File.Delete(fileName);
                Assert.IsFalse(File.Exists(fileName), "Failed to delete " + fileName);
            }

        }
		
		
		/// <summary>
		/// Creates a SQLite database, creates a table, inserts some rows,
		/// deletes some rows, and deletes the database.
		/// </summary>
		[Test]
		public void TestSQLite ()
		{
			string fileName = Path.Combine(_tempDirectory, _tempFilePrefix + ".sqlite");
			try {	
				CreateSQLite(fileName);
				
				SQLiteDba sqliteDba = new SQLiteDba();
				sqliteDba.Connect(fileName);
				
				PopulateSQLite(sqliteDba);
				
				sqliteDba.ExecuteSqlCommand(_sqlDropTable);
				
				sqliteDba.Disconnect();
			}
			finally 
			{
				File.Delete(fileName);
				Assert.IsFalse(File.Exists(fileName), "Failed to delete " + fileName);
			}
		}
		
		
		[Test]
		public void TestProcedureSupport()
		{
			string fileBaseName = Path.Combine(_tempDirectory, _tempFilePrefix);
		    OleDba oleDba = null;

			try {
				CreateMdb(fileBaseName + ".mdb");
				
				oleDba = new OleDba();
				try {
					Assert.IsTrue(oleDba.SupportsProcedures);
					Assert.Fail("OleDba.SupportsProcedures should throw a InvalidOperationException if no database is connected.");
				}
				catch(InvalidOperationException) {}
					
				oleDba.ConnectMDB(fileBaseName + ".mdb");
				Assert.IsTrue(oleDba.SupportsProcedures);;
			}
			finally 
			{
                if (oleDba != null && oleDba.Connected)
                {
                    oleDba.Disconnect();
                    oleDba.Dispose();
                }
				File.Delete(fileBaseName + ".mdb");
				Assert.IsFalse(File.Exists(fileBaseName + ".mdb"), "Failed to delete " + fileBaseName + ".mdb");
			}
		}
		
		
		private static void CompactAndRepairMdb(string fileName) {
			JetSqlUtil.CompactMDB(fileName);
			JetSqlUtil.RepairMDB(fileName);
		}
		
		
		private static void CreateMdb(string fileName) {
			JetSqlUtil.CreateMDB(fileName);
			Assert.IsTrue(File.Exists(fileName), "Failed to create " + fileName);
		}
		
		
		private static void CreateSQLite(string fileName) {
			SQLiteConnection.CreateFile(fileName);
			Assert.IsTrue(File.Exists(fileName), "Failed to create " + fileName);
		}
		
		
		private static void PopulateOleDba(OleDba oleDba) {
			tblTestRow [] rows = new tblTestRow [] {
				new tblTestRow(1, "Number one."),
				new tblTestRow(2, "Number two."),
				new tblTestRow(3, "Number three.")
			};
			
			oleDba.ExecuteSqlCommand(_sqlCreateTable);
			
			foreach(tblTestRow row in rows) {
				DbParameter[] parameters = new DbParameter [] {
					new OleDbParameter("@id", row.Id),
					new OleDbParameter("@description", row.Description)
				};
				oleDba.ExecuteSqlCommand(_sqlInsertRow, parameters);
			}
			string [] columnData = oleDba.GetColumnAsStringArray("tblTest", "description");
			Assert.AreEqual(columnData.Length, 3, "Inserted 3 rows and retrieved {0}", new object[] {columnData.Length});
		}
		
		
		private static void PopulateSQLite(SQLiteDba sqliteDba) {
			tblTestRow [] rows = new tblTestRow [] {
				new tblTestRow(1, "Number one."),
				new tblTestRow(2, "Number two."),
				new tblTestRow(3, "Number three.")
			};
			
			sqliteDba.ExecuteSqlCommand(_sqlCreateTable);
			
			foreach(tblTestRow row in rows) {
				DbParameter[] parameters = new DbParameter [] {
					new SQLiteParameter("@id", row.Id),
					new SQLiteParameter("@description", row.Description)
				};
				sqliteDba.ExecuteSqlCommand(_sqlInsertRow, parameters);
			}
			string [] columnData = sqliteDba.GetColumnAsStringArray("tblTest", "description");
			Assert.AreEqual(columnData.Length, 3, "Inserted 3 rows and retrieved {0}", new object[] {columnData.Length});
		}
	}
}
