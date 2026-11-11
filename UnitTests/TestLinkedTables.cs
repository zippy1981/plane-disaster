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
using System.IO;
using NUnit.Framework;
using PlaneDisaster.Dba;

namespace UnitTests
{
	/// <summary>Adding, listing and removing linked tables between two Access databases.</summary>
	[TestFixture]
	public class TestLinkedTables
	{
		private string _sourceFile;
		private string _targetFile;
		private OleDba _source;
		private OleDba _target;

		[SetUp]
		public void SetUp()
		{
			string prefix = Path.Combine(Path.GetTempPath(), "PlaneDisaster-LinkTest-" + Guid.NewGuid());
			_sourceFile = prefix + "-source.mdb";
			_targetFile = prefix + "-target.mdb";
			JetSqlUtil.CreateMDB(_sourceFile);
			JetSqlUtil.CreateMDB(_targetFile);

			_source = new OleDba();
			_source.ConnectMDB(_sourceFile);
			_source.ExecuteSqlCommand("CREATE TABLE tblSource (id INTEGER PRIMARY KEY, description VARCHAR(255))");
			_source.ExecuteSqlCommand("INSERT INTO tblSource (id, description) VALUES (1, 'one')");
			_source.ExecuteSqlCommand("INSERT INTO tblSource (id, description) VALUES (2, 'two')");
			_source.ExecuteSqlCommand("INSERT INTO tblSource (id, description) VALUES (3, 'three')");

			_target = new OleDba();
			_target.ConnectMDB(_targetFile);
		}

		[TearDown]
		public void TearDown()
		{
			foreach (OleDba db in new OleDba[] { _source, _target }) {
				if (db != null && db.Connected) {
					db.Disconnect();
				}
			}
			foreach (string file in new string[] { _sourceFile, _targetFile }) {
				if (file != null && File.Exists(file)) {
					File.Delete(file);
				}
			}
		}

		[Test]
		public void NewDatabaseHasNoLinks()
		{
			Assert.AreEqual(0, _target.GetLinkedTableNames().Length);
			Assert.AreEqual(0, _target.GetLinkedTables().Length);
		}

		[Test]
		public void LinkReadsTheSourceTable()
		{
			_target.LinkTable("lnkSource", _sourceFile, "tblSource");

			Assert.AreEqual(new string[] { "lnkSource" }, _target.GetLinkedTableNames());
			Assert.IsTrue(_target.IsLinkedTable("LNKSOURCE"), "Link names are case insensitive");
			Assert.AreEqual(3, _target.GetTableAsDataTable("lnkSource").Rows.Count);

			LinkedTableInfo[] links = _target.GetLinkedTables();
			Assert.AreEqual(1, links.Length);
			Assert.AreEqual("lnkSource", links[0].Name);
			Assert.AreEqual("tblSource", links[0].SourceTable);
			Assert.AreEqual(Path.GetFullPath(_sourceFile), Path.GetFullPath(links[0].SourceDatabase));
		}

		[Test]
		public void LinkedTablesAreNotInGetTables()
		{
			_target.LinkTable("lnkSource", _sourceFile, "tblSource");
			Assert.AreEqual(-1, Array.IndexOf(_target.GetTables(), "lnkSource"));
			Assert.AreNotEqual(-1, Array.IndexOf(_target.GetTablesAndLinks(), "lnkSource"));
		}

		[Test]
		public void UnlinkRemovesOnlyTheLink()
		{
			_target.LinkTable("lnkSource", _sourceFile, "tblSource");
			_target.UnlinkTable("lnkSource");

			Assert.AreEqual(0, _target.GetLinkedTableNames().Length);
			Assert.AreEqual(3, _source.GetTableAsDataTable("tblSource").Rows.Count, "The source table must survive");
		}

		[Test]
		public void UnlinkRefusesARealTable()
		{
			_target.ExecuteSqlCommand("CREATE TABLE tblLocal (id INTEGER)");
			try {
				_target.UnlinkTable("tblLocal");
				Assert.Fail("UnlinkTable dropped a real table");
			} catch (ArgumentException) {
			}
			Assert.AreNotEqual(-1, Array.IndexOf(_target.GetTables(), "tblLocal"));
		}

		[Test]
		public void LinkRefusesAnExistingName()
		{
			_target.ExecuteSqlCommand("CREATE TABLE tblLocal (id INTEGER)");
			try {
				_target.LinkTable("tblLocal", _sourceFile, "tblSource");
				Assert.Fail("LinkTable replaced an existing table");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void LinkingAMissingTableFails()
		{
			try {
				_target.LinkTable("lnkMissing", _sourceFile, "tblDoesNotExist");
				Assert.Fail("Linked to a table that does not exist");
			} catch (AssertionException) {
				throw;
			} catch (ArgumentException) {
				throw;
			} catch (Exception) {
				// The provider reports the missing table; any provider error is fine here.
			}
			Assert.AreEqual(0, _target.GetLinkedTableNames().Length);
		}
	}
}
