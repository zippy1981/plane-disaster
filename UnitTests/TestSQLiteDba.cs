/*
 * Copyright 2026 Justin Dearing
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

using NUnit.Framework;
using PlaneDisaster.Dba;

namespace UnitTests
{
	/// <summary>
	/// Exercises <see cref="SQLiteDba"/> and, through it, the shared <see cref="dba"/> code against a
	/// real SQLite file. Runs everywhere, Mono included.
	/// </summary>
	[TestFixture]
	public sealed class TestSQLiteDba
	{
		private static readonly string NL = Environment.NewLine;
		private string _fileName;
		private SQLiteDba _db;


		[SetUp]
		public void SetUp() {
			_fileName = Path.Combine(Path.GetTempPath(), string.Format("PlaneDisaster-UnitTest-{0}.sqlite", Guid.NewGuid()));
			SQLiteConnection.CreateFile(_fileName);
			_db = new SQLiteDba();
			_db.Connect(_fileName);
			_db.ExecuteSqlCommand("CREATE TABLE tblTest (id INTEGER PRIMARY KEY, description varchar(255))");
			string [] descriptions = new string[] {"Number one.", "Number two.", "Number three."};
			for (int i = 0; i < descriptions.Length; i++) {
				_db.ExecuteSqlCommand(
					"INSERT INTO tblTest (id, description) VALUES (@id, @description)",
					new DbParameter[] {new SQLiteParameter("@id", i + 1), new SQLiteParameter("@description", descriptions[i])});
			}
		}


		[TearDown]
		public void TearDown() {
			if (_db.Connected) {
				_db.Disconnect();
			}
			_db.Dispose();
			File.Delete(_fileName);
		}


		[Test]
		public void SQLiteIsNotAnAccessDatabase() {
			// MainForm picks SELECT TOP (Access) or LIMIT (everything else) from this.
			Assert.IsFalse(_db.IsAccessDatabase);
		}


		[Test]
		public void Capabilities() {
			Assert.IsTrue(_db.SupportsViews);
			Assert.IsFalse(_db.SupportsProcedures);
			Assert.IsNull(_db.GetProcedures());
		}


		[Test]
		public void ConnectedFollowsTheConnection() {
			SQLiteDba db = new SQLiteDba();
			Assert.IsFalse(db.Connected);
			db.Connect(_fileName);
			Assert.IsTrue(db.Connected);
			StringAssert.Contains("Open", db.GetStatus());
			StringAssert.Contains(_fileName, db.ConnectionString);
			db.Disconnect();
			Assert.IsFalse(db.Connected);
			db.Dispose();
		}


		[Test]
		public void DisposeWithoutConnecting() {
			SQLiteDba db = new SQLiteDba();
			db.Dispose();
		}


		[Test]
		public void TablesAndColumns() {
			CollectionAssert.Contains(_db.GetTables(), "tblTest");
			Assert.AreEqual(new string[] {"id", "description"}, _db.GetColumnNames("tblTest"));
			Assert.AreEqual(2, _db.GetColumnSchema("tblTest").Rows.Count);
			StringAssert.StartsWith("CREATE TABLE tblTest", _db.GetTableSQL("tblTest"));
			Assert.IsNull(_db.GetTableSQL("noSuchTable"));
			Assert.AreEqual(3, _db.GetTableRowCount("tblTest"));
			Assert.AreEqual(3, _db.GetTableAsDataTable("tblTest").Rows.Count);
			Assert.Greater(_db.GetSchema().Rows.Count, 0);
			Assert.Greater(_db.GetSchema("Tables").Rows.Count, 0);
		}


		[Test]
		public void ColumnAsStringArray() {
			_db.ExecuteSqlCommand("INSERT INTO tblTest (id, description) VALUES (4, 'Number one.')");
			Assert.AreEqual(4, _db.GetColumnAsStringArray("tblTest", "description").Length);
			Assert.AreEqual(3, _db.GetColumnAsStringArray("tblTest", "description", true).Length);
		}


		[Test]
		public void SqlAsDataTable() {
			DataTable dt = _db.GetSqlAsDataTable(
				"SELECT description FROM tblTest WHERE id = @id",
				new DbParameter[] {new SQLiteParameter("@id", 2)});
			Assert.AreEqual(1, dt.Rows.Count);
			Assert.AreEqual("Number two.", dt.Rows[0]["description"]);

			Assert.AreEqual("named", _db.GetSqlAsDataTable("SELECT * FROM tblTest", "named").TableName);
		}


		[Test]
		public void SqlAsDataTableReturnsNullForAStatementWithNoResults() {
			Assert.IsNull(_db.GetSqlAsDataTable("UPDATE tblTest SET description = 'x' WHERE id = 1"));
			Assert.IsNull(_db.GetSqlAsDataTable("UPDATE tblTest SET description = 'y' WHERE id = 1", "named"));
		}


		[Test]
		public void SqlAsDataReader() {
			int rows = 0;
			using (DbDataReader rdr = _db.GetSqlAsDataReader("SELECT id FROM tblTest ORDER BY id")) {
				while (rdr.Read()) {
					rows++;
					Assert.AreEqual(rows, Convert.ToInt32(rdr[0]));
				}
			}
			Assert.AreEqual(3, rows);
		}


		[Test]
		public void Views() {
			_db.CreateView("v_test", "SELECT id FROM tblTest WHERE id > 1");
			CollectionAssert.Contains(_db.GetViews(), "v_test");
			StringAssert.Contains("SELECT id FROM tblTest", _db.GetViewSQL("v_test"));
			Assert.AreEqual(2, _db.GetTableRowCount("v_test"));

			// Creating it again replaces it.
			_db.CreateView("v_test", "SELECT id FROM tblTest");
			Assert.AreEqual(3, _db.GetTableRowCount("v_test"));

			_db.DropView("v_test");
			CollectionAssert.DoesNotContain(_db.GetViews(), "v_test");
		}


		[Test]
		public void DropTable() {
			_db.DropTable("tblTest");
			CollectionAssert.DoesNotContain(_db.GetTables(), "tblTest");
		}


		[Test]
		public void ExecuteScriptRunsEveryStatementAndReturnsTheLastResult() {
			DataTable dt = _db.ExecuteScript(
				"CREATE TABLE t (a INTEGER);" + NL +
				"INSERT INTO t VALUES (1);" + NL +
				"INSERT INTO t VALUES (2);" + NL +
				"SELECT COUNT(*) AS n FROM t;" + NL);
			Assert.AreEqual(2, Convert.ToInt32(dt.Rows[0]["n"]));
		}


		[Test]
		public void ExecuteScriptKeepsSemicolonsInsideStrings() {
			DataTable dt = _db.ExecuteScript(
				"CREATE TABLE t (a TEXT); INSERT INTO t VALUES ('x;y'); SELECT a FROM t");
			Assert.AreEqual("x;y", dt.Rows[0]["a"]);
		}


		[Test]
		public void ExecuteScriptIgnoresSemicolonsInComments() {
			DataTable dt = _db.ExecuteScript(
				"-- count the rows; all of them" + NL +
				"/* a block; comment */ SELECT COUNT(*) AS n FROM tblTest");
			Assert.AreEqual(3, Convert.ToInt32(dt.Rows[0]["n"]));
		}


		[Test]
		public void ExecuteScriptEndingInAStatementWithNoResults() {
			Assert.IsNull(_db.ExecuteScript("DELETE FROM tblTest WHERE id = 1; DELETE FROM tblTest WHERE id = 2;"));
			Assert.AreEqual(1, _db.GetTableRowCount("tblTest"));
		}


		[Test]
		public void ExecuteSqlFile() {
			string script = _fileName + ".sql";
			File.WriteAllText(script, "UPDATE tblTest SET description = 'changed' WHERE id = 3;\nSELECT description FROM tblTest WHERE id = 3;");
			try {
				Assert.AreEqual("changed", _db.ExecuteSqlFile(script).Rows[0][0]);
			} finally {
				File.Delete(script);
			}
		}


		[Test]
		public void SqlAsCsv() {
			_db.ExecuteSqlCommand("INSERT INTO tblTest (id, description) VALUES (4, 'Four, or \"4\"')");
			Assert.AreEqual(
				"id,description" + NL + "1,Number one." + NL,
				_db.GetSQLAsCSV("SELECT id, description FROM tblTest WHERE id = 1"));
			Assert.AreEqual(
				"id;description" + NL + "4;\"Four, or \"\"4\"\"\"" + NL,
				_db.GetSQLAsCSV("SELECT id, description FROM tblTest WHERE id = 4", ";"));
			StringAssert.StartsWith("id,description" + NL + "1,Number one." + NL, _db.GetTableAsCSV("tblTest"));
		}


		[Test]
		public void SerializeQueryAndTable() {
			StringAssert.Contains("Number two.", _db.SerializeQuery("SELECT * FROM tblTest"));
			StringAssert.Contains("Number three.", _db.SerializeTable("tblTest"));

			string xml = _fileName + ".xml";
			try {
				_db.SerializeTable("tblTest", xml);
				DataSet ds = new DataSet();
				ds.ReadXml(xml);
				Assert.AreEqual(3, ds.Tables[0].Rows.Count);
			} finally {
				File.Delete(xml);
			}
		}


		[Test]
		public void InsertStatementsFromDataTable2DMLRun() {
			_db.ExecuteSqlCommand("CREATE TABLE people (id INTEGER, name TEXT)");
			DataTable dt = new DataTable("people");
			dt.Columns.Add("id", typeof(int));
			dt.Columns.Add("name", typeof(string));
			dt.Rows.Add(new object[] {1, "O'Brien"});
			dt.Rows.Add(new object[] {2, DBNull.Value});

			_db.ExecuteScript(dba.DataTable2DML(dt));

			DataTable copy = _db.GetSqlAsDataTable("SELECT id, name FROM people ORDER BY id");
			Assert.AreEqual("O'Brien", copy.Rows[0]["name"]);
			Assert.AreEqual(DBNull.Value, copy.Rows[1]["name"]);
		}


		[Test]
		public void DataTable2SQLiteTableNeedsATableName() {
			Assert.Throws<ArgumentException>(delegate { _db.DataTable2SQLiteTable(new DataTable()); });
		}


		[Test]
		public void DataTable2SQLiteTableCopiesEveryRow() {
			DataTable dt = new DataTable("copy");
			dt.Columns.Add("id", typeof(int));
			dt.Columns.Add("two words", typeof(string));
			dt.Columns.Add("dash-name", typeof(string));
			for (int i = 1; i <= 5; i++) {
				dt.Rows.Add(new object[] {i, "row " + i, "dash " + i});
			}

			_db.DataTable2SQLiteTable(dt);

			DataTable copy = _db.GetSqlAsDataTable("SELECT * FROM copy ORDER BY id");
			Assert.AreEqual(5, copy.Rows.Count);
			Assert.AreEqual("row 5", copy.Rows[4]["two words"]);
			Assert.AreEqual("dash 5", copy.Rows[4]["dash-name"]);
		}


		/// <summary>
		/// Jet2SQLite copies Access tables through DataTable2SQLiteTable, so every type the Access
		/// providers return has to map to a SQLite column that keeps the value.
		/// </summary>
		[Test]
		public void DataTable2SQLiteTableKeepsAccessColumnTypes() {
			Guid guid = Guid.NewGuid();
			DateTime when = new DateTime(2026, 11, 10, 13, 14, 15);
			DataTable dt = new DataTable("types");
			dt.Columns.Add("byteCol", typeof(byte));
			dt.Columns.Add("shortCol", typeof(short));
			dt.Columns.Add("intCol", typeof(int));
			dt.Columns.Add("longCol", typeof(long));
			dt.Columns.Add("singleCol", typeof(float));
			dt.Columns.Add("doubleCol", typeof(double));
			dt.Columns.Add("decimalCol", typeof(decimal));
			dt.Columns.Add("boolCol", typeof(bool));
			dt.Columns.Add("dateCol", typeof(DateTime));
			dt.Columns.Add("guidCol", typeof(Guid));
			dt.Columns.Add("textCol", typeof(string));
			dt.Columns.Add("blobCol", typeof(byte[]));
			dt.Rows.Add(new object[] {(byte)200, (short)-12345, 123456789, 1234567890123L, 1.25f, 0.5, 19.99m, true, when, guid, "text", new byte[] {1, 2, 3}});
			dt.Rows.Add(new object[] {DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value});

			_db.DataTable2SQLiteTable(dt);

			DataTable copy = _db.GetTableAsDataTable("types");
			Assert.AreEqual(2, copy.Rows.Count);
			DataRow row = copy.Rows[0];
			Assert.AreEqual(200, Convert.ToInt32(row["byteCol"]));
			Assert.AreEqual(-12345, Convert.ToInt32(row["shortCol"]));
			Assert.AreEqual(123456789, Convert.ToInt32(row["intCol"]));
			Assert.AreEqual(1234567890123L, Convert.ToInt64(row["longCol"]));
			Assert.AreEqual(1.25, Convert.ToDouble(row["singleCol"]));
			Assert.AreEqual(0.5, Convert.ToDouble(row["doubleCol"]));
			Assert.AreEqual(19.99m, Convert.ToDecimal(row["decimalCol"]));
			Assert.IsTrue(Convert.ToBoolean(row["boolCol"]));
			Assert.AreEqual(when, Convert.ToDateTime(row["dateCol"]));
			Assert.AreEqual(guid, new Guid(row["guidCol"].ToString()));
			Assert.AreEqual("text", row["textCol"]);
			Assert.AreEqual(new byte[] {1, 2, 3}, row["blobCol"]);
			foreach (object value in copy.Rows[1].ItemArray) {
				Assert.AreEqual(DBNull.Value, value);
			}
		}
	}
}
