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
 * Date: 8/6/2007
 * Time: 11:43 AM
 */

using NUnit.Framework;
using PlaneDisaster.Dba;
using System;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;


namespace UnitTests
{
	[TestFixture]
	public class Tests
	{
		private struct tblTestRow {
			internal int Id;
			internal string Description;
			
			internal tblTestRow(int id,string description) {
				Id = id;
				Description = description;
			}
		}
			
		private readonly string _tempDirectory = Path.GetTempPath();
		private readonly string _tempFilePrefix = string.Format("PlaneDIsaster-unittest-{0}", Guid.NewGuid());
		
		private readonly string _sqlCreateTable = "CREATE TABLE tblTest (id INTEGER PRIMARY KEY, description varchar(255))";
		private readonly string _sqlDropTable = "DROP TABLE tblTest";
		private readonly string _sqlInsertRow = "INSERT INTO tblTest (id, description) VALUES (@id, @description)";
		
		/// <summary>
		/// Creates a JetSQL database, creates a table, inserts some rows,
		/// deletes some rows, and deletes the database.
		/// </summary>
		[Test]
		public void CreateMdb ()
		{
			string fileName = Path.Combine(_tempDirectory, _tempFilePrefix + ".mdb");
			tblTestRow [] rows = new tblTestRow [] {
				new tblTestRow(1, "Number one."),
				new tblTestRow(2, "Number two."),
				new tblTestRow(3, "Number three.")
			};
			
			JetSqlUtil.CreateMDB(fileName);
			
			OleDba oleDba = new OleDba();
			oleDba.ConnectMDB(fileName);
			
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
			
			oleDba.ExecuteSqlCommand(_sqlDropTable);
			
			oleDba.Disconnect();
			File.Delete(fileName);
			
		}
	}
}
