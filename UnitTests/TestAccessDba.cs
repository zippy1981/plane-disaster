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
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;

using NUnit.Framework;
using PlaneDisaster.Dba;

namespace UnitTests
{
	/// <summary>
	/// <see cref="OleDba"/>, <see cref="OdbcDba"/> and <see cref="JetSqlUtil"/> against a real Access file.
	/// Needs Windows with Jet (x86) or the Access Database Engine (x64).
	/// </summary>
	[TestFixture]
	public sealed class TestAccessDba
	{
		private const string Password = "s3cret";
		private string _fileName;
		private OleDba _db;


		[SetUp]
		public void SetUp() {
			_fileName = Path.Combine(Path.GetTempPath(), string.Format("PlaneDisaster-UnitTest-{0}.mdb", Guid.NewGuid()));
			JetSqlUtil.CreateMDB(_fileName);
			_db = new OleDba();
			_db.ConnectMDB(_fileName);
			_db.ExecuteSqlCommand("CREATE TABLE tblTest (id INTEGER PRIMARY KEY, description varchar(255))");
			string [] descriptions = new string[] {"Number one.", "Number two.", "Number three."};
			for (int i = 0; i < descriptions.Length; i++) {
				_db.ExecuteSqlCommand(
					"INSERT INTO tblTest (id, description) VALUES (?, ?)",
					new DbParameter[] {new OleDbParameter("id", i + 1), new OleDbParameter("description", descriptions[i])});
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


		/// <summary>Sets a database password, which needs an exclusive connection.</summary>
		private void SetPassword() {
			string provider = new OleDbConnectionStringBuilder(_db.ConnectionString).Provider;
			_db.Disconnect();
			OleDba exclusive = new OleDba();
			exclusive.ConnectDSN(string.Format("Provider={0};Data Source={1};Mode=Share Exclusive;", provider, _fileName));
			exclusive.ExecuteSqlCommand(string.Format("ALTER DATABASE PASSWORD [{0}] NULL", Password));
			exclusive.Disconnect();
			exclusive.Dispose();
		}


		[Test]
		public void AccessIsAnAccessDatabase() {
			Assert.IsTrue(_db.IsAccessDatabase);
			Assert.IsTrue(_db.SupportsViews);
			Assert.IsTrue(_db.SupportsProcedures);
		}


		[Test]
		public void TablesAndColumns() {
			CollectionAssert.Contains(_db.GetTables(), "tblTest");
			Assert.AreEqual(new string[] {"description", "id"}, Sorted(_db.GetColumnNames("tblTest")));
			Assert.AreEqual(3, _db.GetTableRowCount("tblTest"));

			DataTable dt = _db.GetTableAsDataTable("tblTest");
			Assert.AreEqual("tblTest", dt.TableName);
			Assert.AreEqual(3, dt.Rows.Count);
		}


		[Test]
		public void Procedures() {
			// Jet lists a SELECT without parameters as a view; action queries are procedures.
			_db.CreateProcedure("qryDelete", "DELETE FROM tblTest WHERE id = 1");
			CollectionAssert.Contains(_db.GetProcedures(), "qryDelete");
			StringAssert.Contains("tblTest", _db.GetProcedureSQL("qryDelete"));

			// Creating it again replaces it.
			_db.CreateProcedure("qryDelete", "DELETE FROM tblTest WHERE id = 2");
			_db.ExecuteSqlCommand("EXECUTE qryDelete");
			Assert.AreEqual(new string[] {"1", "3"}, _db.GetColumnAsStringArray("tblTest", "id"));

			_db.DropProcedure("qryDelete");
			CollectionAssert.DoesNotContain(_db.GetProcedures(), "qryDelete");
		}


		[Test]
		public void Views() {
			_db.CreateView("vTest", "SELECT id FROM tblTest WHERE id > 1");
			CollectionAssert.Contains(_db.GetViews(), "vTest");
			StringAssert.Contains("tblTest", _db.GetViewSQL("vTest"));
			Assert.AreEqual(2, _db.GetSqlAsDataTable("SELECT * FROM vTest").Rows.Count);
			_db.DropView("vTest");
			CollectionAssert.DoesNotContain(_db.GetViews(), "vTest");
		}


		[Test]
		public void ExecuteScriptRunsEveryStatement() {
			DataTable dt = _db.ExecuteScript(
				"INSERT INTO tblTest (id, description) VALUES (4, 'a;b'); SELECT description FROM tblTest WHERE id = 4");
			Assert.AreEqual("a;b", dt.Rows[0]["description"]);
		}


		[Test]
		public void SqlAsCsvAndDml() {
			Assert.AreEqual(
				"id,description" + Environment.NewLine + "2,Number two." + Environment.NewLine,
				_db.GetSQLAsCSV("SELECT id, description FROM tblTest WHERE id = 2"));

			DataTable dt = _db.GetTableAsDataTable("tblTest");
			_db.ExecuteSqlCommand("DELETE FROM tblTest");
			foreach (string insert in dba.DataTable2DML(dt).Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)) {
				_db.ExecuteSqlCommand(insert);
			}
			Assert.AreEqual(3, _db.GetTableRowCount("tblTest"));
		}


		[Test]
		public void OleDbOpensAPasswordProtectedDatabase() {
			SetPassword();
			OleDba db = new OleDba();
			db.ConnectMDB(_fileName, Password);
			Assert.IsTrue(db.Connected);
			Assert.AreEqual(3, db.GetTableRowCount("tblTest"));
			db.Disconnect();
			db.Dispose();
		}


		[Test]
		public void OdbcOpensAPasswordProtectedDatabase() {
			SetPassword();
			OdbcDba db = new OdbcDba();
			db.ConnectMDB(_fileName, Password);
			Assert.IsTrue(db.Connected);
			Assert.AreEqual(3, db.GetTableRowCount("tblTest"));
			db.Disconnect();
			db.Dispose();
		}


		[Test]
		public void OdbcReadsTheSameDatabase() {
			_db.Disconnect();
			OdbcDba db = new OdbcDba();
			db.ConnectMDB(_fileName);
			Assert.IsTrue(db.IsAccessDatabase);
			Assert.IsTrue(db.SupportsViews);
			CollectionAssert.Contains(db.GetTables(), "tblTest");
			Assert.AreEqual(new string[] {"description", "id"}, Sorted(db.GetColumnNames("tblTest")));
			Assert.AreEqual(3, db.GetTableAsDataTable("tblTest").Rows.Count);
			db.Disconnect();
			db.Dispose();
		}


		[Test]
		public void CompactKeepsTheData() {
			_db.Disconnect();
			JetSqlUtil.CompactMDB(_fileName);
			_db.ConnectMDB(_fileName);
			Assert.AreEqual(3, _db.GetTableRowCount("tblTest"));
		}


		[Test]
		public void CreateMdbReplacesAnExistingFile() {
			string fileName = _fileName + ".new.mdb";
			File.WriteAllText(fileName, "not a database");
			try {
				JetSqlUtil.CreateMDB(fileName, AccessDbVersion.Access2000);
				OleDba db = new OleDba();
				db.ConnectMDB(fileName);
				Assert.AreEqual(0, db.GetTables().Length);
				db.Disconnect();
				db.Dispose();
			} finally {
				File.Delete(fileName);
			}
		}


		[Test]
		public void CompactingAMissingFileFails() {
			Assert.Catch(delegate { JetSqlUtil.CompactMDB(_fileName + ".missing.mdb"); });
		}


		private static string [] Sorted(string [] values) {
			Array.Sort(values, StringComparer.Ordinal);
			return values;
		}
	}
}
