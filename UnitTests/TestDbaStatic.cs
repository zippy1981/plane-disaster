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

using NUnit.Framework;
using PlaneDisaster.Dba;

namespace UnitTests
{
	/// <summary>
	/// The DataTable helpers on <see cref="dba"/> that need no database: the CSV and
	/// INSERT exports behind the results pane, and the column helpers.
	/// </summary>
	[TestFixture]
	public sealed class TestDbaStatic
	{
		private static readonly string NL = Environment.NewLine;

		private static DataTable People() {
			DataTable dt = new DataTable("people");
			dt.Columns.Add("id", typeof(int));
			dt.Columns.Add("name", typeof(string));
			dt.Rows.Add(new object[] {1, "Ann"});
			dt.Rows.Add(new object[] {2, "O'Brien, Pat"});
			dt.Rows.Add(new object[] {3, DBNull.Value});
			return dt;
		}


		[Test]
		public void DataTable2CSVWritesAHeaderAndOneLinePerRow() {
			DataTable dt = new DataTable("t");
			dt.Columns.Add("a", typeof(int));
			dt.Columns.Add("b", typeof(string));
			dt.Rows.Add(new object[] {1, "x"});
			dt.Rows.Add(new object[] {2, "y"});

			Assert.AreEqual("a,b" + NL + "1,x" + NL + "2,y" + NL, dba.DataTable2CSV(dt));
		}


		[Test]
		public void DataTable2CSVQuotesFieldsThatNeedIt() {
			DataTable dt = new DataTable("t");
			dt.Columns.Add("text", typeof(string));
			dt.Rows.Add(new object[] {"a, b"});
			dt.Rows.Add(new object[] {"say \"hi\""});
			dt.Rows.Add(new object[] {"two" + "\n" + "lines"});
			dt.Rows.Add(new object[] {DBNull.Value});

			Assert.AreEqual(
				"text" + NL + "\"a, b\"" + NL + "\"say \"\"hi\"\"\"" + NL + "\"two\nlines\"" + NL + NL,
				dba.DataTable2CSV(dt));
		}


		[Test]
		public void DataTable2CSVUsesTheGivenSeparator() {
			DataTable dt = new DataTable("t");
			dt.Columns.Add("a", typeof(string));
			dt.Columns.Add("b", typeof(string));
			dt.Rows.Add(new object[] {"1,5", "tab\there"});

			Assert.AreEqual("a\tb" + NL + "1,5\t\"tab\there\"" + NL, dba.DataTable2CSV(dt, "\t"));
		}


		[Test]
		public void DataTable2DMLEscapesQuotesAndWritesNulls() {
			Assert.AreEqual(
				"INSERT INTO [people] ([id], [name]) VALUES('1', 'Ann');" + NL +
				"INSERT INTO [people] ([id], [name]) VALUES('2', 'O''Brien, Pat');" + NL +
				"INSERT INTO [people] ([id], [name]) VALUES('3', NULL);" + NL,
				dba.DataTable2DML(People()));
		}


		[Test]
		public void DataTable2DMLAllowsBracesInNames() {
			DataTable dt = new DataTable("Sales {2026}");
			dt.Columns.Add("total {usd}", typeof(int));
			dt.Rows.Add(new object[] {5});

			Assert.AreEqual(
				"INSERT INTO [Sales {2026}] ([total {usd}]) VALUES('5');" + NL,
				dba.DataTable2DML(dt));
		}


		[Test]
		public void GetColumnAsStringArrayReturnsEveryRow() {
			DataTable dt = People();
			dt.Rows.Add(new object[] {1, "Ann"});

			Assert.AreEqual(new string[] {"1", "2", "3", "1"}, dba.GetColumnAsStringArray(dt, "id"));
			Assert.AreEqual(new string[] {"1", "2", "3", "1"}, dba.GetColumnAsStringArray(dt, "id", false));
		}


		[Test]
		public void GetColumnAsStringArrayDistinctWorksOnAnyColumnType() {
			DataTable dt = People();
			dt.Rows.Add(new object[] {1, "Ann"});

			Assert.AreEqual(new string[] {"1", "2", "3"}, dba.GetColumnAsStringArray(dt, "id", true));
			Assert.AreEqual(new string[] {"", "Ann", "O'Brien, Pat"}, dba.GetColumnAsStringArray(dt, "name", true));
		}
	}
}
