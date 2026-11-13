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
using System.Data.OleDb;
using System.Data.SQLite;
using System.IO;

using NUnit.Framework;
using PlaneDisaster.Dba;

namespace UnitTests
{
	/// <summary>
	/// Smoke tests against Access databases that other people published: the files in TestData,
	/// whose README lists where each came from. Needs Windows with Jet (x86) or the Access
	/// Database Engine. A test is ignored when no installed provider of this bitness reads the
	/// file's format: Jet 4.0 doesn't read accdb, and the Access Database Engine 2013 and later
	/// don't read Access 97.
	/// </summary>
	[TestFixture]
	public sealed class TestAccessSampleFiles
	{
		/// <summary>Census 2000 redistricting file structure. Access 97 (Jet 3), 149 column tables.</summary>
		private const string Census97 = "census-pl2000-access97.mdb";
		/// <summary>NIST albatross contaminant database. Access 2000 (Jet 4), with queries.</summary>
		private const string AlbatrossMdb = "nist-albatross.mdb";
		/// <summary>The same NIST database as an Access 2007 and later (ACE) file.</summary>
		private const string AlbatrossAccdb = "nist-albatross.accdb";

		private string _fileName;
		private string _sqliteFileName;
		private OleDba _db;


		[TearDown]
		public void TearDown() {
			if (_db != null) {
				if (_db.Connected) {
					_db.Disconnect();
				}
				_db.Dispose();
				_db = null;
			}
			if (_fileName != null) {
				File.Delete(_fileName);
			}
			if (_sqliteFileName != null) {
				File.Delete(_sqliteFileName);
			}
		}


		/// <summary>
		/// Connects <see cref="_db"/> to a copy of a file from TestData, so the lock file Access
		/// writes lands in the temp directory. Ignores the test when the provider can't read the format.
		/// </summary>
		private void Open(string sample) {
			string directory = Path.GetDirectoryName(typeof(TestAccessSampleFiles).Assembly.Location);
			string source = Path.Combine(Path.Combine(directory, "TestData"), sample);
			string extension = Path.GetExtension(sample);
			_fileName = Path.Combine(Path.GetTempPath(), string.Format("PlaneDisaster-UnitTest-{0}{1}", Guid.NewGuid(), extension));
			File.Copy(source, _fileName);
			File.SetAttributes(_fileName, FileAttributes.Normal);

			_db = new OleDba();
			try {
				_db.ConnectMDB(_fileName);
			} catch (AccessDriverNotFoundException ex) {
				// A CI leg that says its drivers open this format must not skip the test.
				if (TestAccessDriversInstalled.ExpectedFormats.Contains(ex.Format)) {
					throw;
				}
				Assert.Ignore(string.Format("{0}: {1}", sample, ex.Message));
			}
		}


		[TestCase(Census97, 5)]
		[TestCase(AlbatrossMdb, 21)]
		[TestCase(AlbatrossAccdb, 21)]
		public void ListsTheTablesAndTheirColumns(string sample, int tables) {
			Open(sample);
			string [] names = _db.GetTables();
			Assert.AreEqual(tables, names.Length, string.Join(", ", names));
			foreach (string table in names) {
				Assert.Greater(_db.GetColumnNames(table).Length, 0, table);
				Assert.Greater(_db.GetColumnSchema(table).Rows.Count, 0, table);
			}
		}


		[TestCase(Census97, "header", 3, 80)]
		[TestCase(Census97, "tables", 3, 299)]
		[TestCase(Census97, "PL_Geo_Header", 80, 0)]
		[TestCase(Census97, "PL_Part1", 149, 0)]
		[TestCase(AlbatrossMdb, "Paste Errors", 3, 1)]
		[TestCase(AlbatrossMdb, "tbl_species", 6, 26)]
		[TestCase(AlbatrossMdb, "tbl_Procite", 11, 170)]
		[TestCase(AlbatrossMdb, "tbl_analysis_contaminants", 18, 5284)]
		[TestCase(AlbatrossAccdb, "Paste Errors", 3, 1)]
		[TestCase(AlbatrossAccdb, "tbl_species", 6, 26)]
		[TestCase(AlbatrossAccdb, "tbl_Procite", 11, 170)]
		[TestCase(AlbatrossAccdb, "tbl_analysis_contaminants", 18, 5284)]
		public void ReadsATable(string sample, string table, int columns, int rows) {
			Open(sample);
			Assert.AreEqual(columns, _db.GetColumnNames(table).Length);
			Assert.AreEqual(rows, _db.GetTableRowCount(table));
			DataTable dt = _db.GetTableAsDataTable(table);
			Assert.AreEqual(columns, dt.Columns.Count);
			Assert.AreEqual(rows, dt.Rows.Count);
		}


		[TestCase(Census97)]
		[TestCase(AlbatrossMdb)]
		[TestCase(AlbatrossAccdb)]
		public void ExportsEveryTable(string sample) {
			Open(sample);
			foreach (string table in _db.GetTables()) {
				DataTable dt = _db.GetTableAsDataTable(table);
				StringAssert.StartsWith(dt.Columns[0].ColumnName, dba.DataTable2CSV(dt), table);
				string dml = dba.DataTable2DML(dt);
				if (dt.Rows.Count > 0) {
					StringAssert.StartsWith(string.Concat("INSERT INTO [", table, "]"), dml, table);
				}
				Assert.IsNotEmpty(_db.SerializeTable(table), table);
			}
		}


		/// <summary>What Jet2SQLite does: copy every table into a new SQLite file.</summary>
		[TestCase(Census97)]
		[TestCase(AlbatrossMdb)]
		[TestCase(AlbatrossAccdb)]
		public void CopiesEveryTableToSQLite(string sample) {
			Open(sample);
			_sqliteFileName = _fileName + ".sqlite";
			SQLiteConnection.CreateFile(_sqliteFileName);
			using (SQLiteDba sqlite = new SQLiteDba()) {
				sqlite.Connect(_sqliteFileName);
				try {
					string [] tables = _db.GetTables();
					foreach (string table in tables) {
						DataTable dt = _db.GetTableAsDataTable(table);
						sqlite.DataTable2SQLiteTable(dt);
						Assert.AreEqual(dt.Rows.Count, sqlite.GetTableRowCount(table), table);
						Assert.AreEqual(dt.Columns.Count, sqlite.GetColumnNames(table).Length, table);
					}
					Assert.AreEqual(tables.Length, sqlite.GetTables().Length);
				} finally {
					sqlite.Disconnect();
				}
			}
		}


		[TestCase(AlbatrossMdb)]
		[TestCase(AlbatrossAccdb)]
		public void ViewsAndProceduresHaveSql(string sample) {
			Open(sample);
			string [] views = _db.GetViews();
			Assert.Greater(views.Length, 0);
			foreach (string view in views) {
				Assert.IsNotEmpty(_db.GetViewSQL(view), view);
			}
			string [] procedures = _db.GetProcedures();
			Assert.Greater(procedures.Length, 0);
			foreach (string procedure in procedures) {
				Assert.IsNotEmpty(_db.GetProcedureSQL(procedure), procedure);
			}
		}


		[TestCase(AlbatrossMdb)]
		[TestCase(AlbatrossAccdb)]
		public void RunsASavedQuery(string sample) {
			Open(sample);
			CollectionAssert.Contains(_db.GetViews(), "qry_list_species");
			Assert.AreEqual(24, _db.GetSqlAsDataTable("SELECT * FROM qry_list_species").Rows.Count);
		}


		[Test]
		public void TheCensusFileHasNoQueries() {
			Open(Census97);
			Assert.AreEqual(0, _db.GetViews().Length);
			Assert.AreEqual(0, _db.GetProcedures().Length);
		}
	}
}
