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

using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using PlaneDisaster.Dba;

namespace UnitTests
{
	[TestFixture]
	public class TestSqlTokenizer
	{
		/// <summary>Renders the tokens as "Type:text" pairs so failures show what was matched.</summary>
		private static string Describe(string sql)
		{
			StringBuilder sb = new StringBuilder();
			foreach (SqlToken token in SqlTokenizer.Tokenize(sql)) {
				if (sb.Length > 0) {
					sb.Append(' ');
				}
				sb.AppendFormat("{0}:{1}", token.Type, sql.Substring(token.Start, token.Length));
			}
			return sb.ToString();
		}

		[Test]
		public void KeywordsAreCaseInsensitiveAndIdentifiersAreSkipped()
		{
			Assert.AreEqual("Keyword:select Keyword:FROM Keyword:Where Number:1",
				Describe("select name FROM tblTest Where id = 1"));
		}

		[Test]
		public void FunctionsAreSeparateFromKeywords()
		{
			Assert.AreEqual("Keyword:SELECT Function:COUNT Keyword:FROM",
				Describe("SELECT COUNT(*) FROM t"));
		}

		[Test]
		public void StringsWithDoubledQuotes()
		{
			Assert.AreEqual("Keyword:SELECT String:'it''s'", Describe("SELECT 'it''s'"));
		}

		[Test]
		public void UnterminatedStringRunsToTheEnd()
		{
			Assert.AreEqual("Keyword:SELECT String:'abc", Describe("SELECT 'abc"));
		}

		[Test]
		public void LineAndBlockComments()
		{
			Assert.AreEqual("Comment:-- hi Keyword:SELECT Comment:/* a\nb */ Number:2",
				Describe("-- hi\nSELECT /* a\nb */ 2"));
		}

		[Test]
		public void UnterminatedBlockCommentRunsToTheEnd()
		{
			Assert.AreEqual("Comment:/* open", Describe("/* open"));
		}

		[Test]
		public void QuotedIdentifiersAreNotKeywords()
		{
			Assert.AreEqual("Keyword:SELECT QuotedIdentifier:[Order Date] QuotedIdentifier:\"select\" QuotedIdentifier:`from`",
				Describe("SELECT [Order Date], \"select\", `from`"));
		}

		[Test]
		public void QualifiedNamesAreNotKeywords()
		{
			Assert.AreEqual("Keyword:SELECT Keyword:FROM", Describe("SELECT t.date FROM t"));
		}

		[Test]
		public void Parameters()
		{
			Assert.AreEqual("Keyword:WHERE Parameter:@id Keyword:AND Parameter:? Keyword:AND Parameter::name",
				Describe("WHERE id = @id AND b = ? AND c = :name"));
		}

		[Test]
		public void Numbers()
		{
			Assert.AreEqual("Number:1 Number:2.5 Number:.5 Number:1e10 Number:3E-2",
				Describe("1, 2.5, .5, 1e10, 3E-2, col1, 9abc"));
		}

		[Test]
		public void JetDateLiterals()
		{
			Assert.AreEqual("Keyword:WHERE String:#2026-11-10#", Describe("WHERE d > #2026-11-10#"));
		}

		[Test]
		public void EmptyAndNullAreEmpty()
		{
			Assert.AreEqual(0, SqlTokenizer.Tokenize("").Count);
			Assert.AreEqual(0, SqlTokenizer.Tokenize(null).Count);
		}

		[Test]
		public void TokensAreInOrderAndDoNotOverlap()
		{
			string sql = "SELECT a, 'b' FROM [c] -- d\nWHERE e = @f /* g */";
			List<SqlToken> tokens = SqlTokenizer.Tokenize(sql);
			int end = 0;
			foreach (SqlToken token in tokens) {
				Assert.GreaterOrEqual(token.Start, end, token.ToString());
				end = token.Start + token.Length;
			}
			Assert.LessOrEqual(end, sql.Length);
		}
	}
}
