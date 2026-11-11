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

namespace PlaneDisaster.Dba
{
	/// <summary>The kinds of token <see cref="SqlTokenizer"/> reports.</summary>
	public enum SqlTokenType
	{
		/// <summary>A reserved word such as SELECT or WHERE.</summary>
		Keyword,
		/// <summary>A built in function such as COUNT or IIF.</summary>
		Function,
		/// <summary>A quoted string literal.</summary>
		String,
		/// <summary>A number literal.</summary>
		Number,
		/// <summary>A -- line comment or a /* block */ comment.</summary>
		Comment,
		/// <summary>An identifier quoted with [brackets], "double quotes" or `backticks`.</summary>
		QuotedIdentifier,
		/// <summary>A parameter such as @id, :id or ?.</summary>
		Parameter,
	}


	/// <summary>A span of SQL text and what kind of token it is.</summary>
	public struct SqlToken
	{
		/// <summary>Where the token starts in the text.</summary>
		public readonly int Start;
		/// <summary>The token's length in characters.</summary>
		public readonly int Length;
		/// <summary>What kind of token it is.</summary>
		public readonly SqlTokenType Type;

		/// <summary>Creates a token.</summary>
		public SqlToken(int start, int length, SqlTokenType type)
		{
			Start = start;
			Length = length;
			Type = type;
		}

		/// <summary>Formats the token for test failure messages.</summary>
		public override string ToString()
		{
			return string.Format("{0}@{1}+{2}", Type, Start, Length);
		}
	}


	/// <summary>
	/// Splits JetSQL and SQLite text into the tokens a syntax highlighter colors. Plain identifiers,
	/// operators and whitespace are not reported. It never throws: unterminated strings and comments
	/// run to the end of the text, so it is safe to call on every keystroke.
	/// </summary>
	public static class SqlTokenizer
	{
		private static readonly Dictionary<string, SqlTokenType> Words = BuildWords();

		private static Dictionary<string, SqlTokenType> BuildWords()
		{
			string[] keywords = {
				"ADD", "ALL", "ALTER", "AND", "AS", "ASC", "AUTOINCREMENT", "BETWEEN", "BY", "CASCADE",
				"CASE", "CHECK", "COLUMN", "CONSTRAINT", "COUNTER", "CREATE", "CROSS", "DATABASE", "DEFAULT",
				"DELETE", "DESC", "DISTINCT", "DISTINCTROW", "DROP", "ELSE", "END", "ESCAPE", "EXCEPT", "EXEC",
				"EXECUTE", "EXISTS", "FOREIGN", "FROM", "FULL", "GLOB", "GROUP", "HAVING", "IF", "IN", "INDEX",
				"INNER", "INSERT", "INTERSECT", "INTO", "IS", "JOIN", "KEY", "LEFT", "LIKE", "LIMIT", "NOT",
				"NULL", "OFFSET", "ON", "OR", "ORDER", "OUTER", "PARAMETERS", "PIVOT", "PRAGMA", "PRIMARY",
				"PROCEDURE", "REFERENCES", "RIGHT", "SELECT", "SET", "TABLE", "THEN", "TOP", "TRANSFORM",
				"UNION", "UNIQUE", "UPDATE", "VALUES", "VIEW", "WHEN", "WHERE", "WITH",
				// Types
				"BINARY", "BIT", "BLOB", "BOOLEAN", "BYTE", "CHAR", "CURRENCY", "DATETIME", "DECIMAL",
				"DOUBLE", "FLOAT", "GUID", "IMAGE", "INT", "INTEGER", "LONG", "LONGBINARY", "LONGTEXT",
				"MEMO", "MONEY", "NUMERIC", "REAL", "SHORT", "SINGLE", "SMALLINT", "TEXT", "TINYINT",
				"VARBINARY", "VARCHAR", "YESNO",
			};
			string[] functions = {
				"ABS", "AVG", "CCUR", "CDATE", "CDBL", "CINT", "CLNG", "COALESCE", "COUNT", "CSTR", "DATE",
				"DATEADD", "DATEDIFF", "DATEPART", "DAY", "FIRST", "FORMAT", "IFNULL", "IIF", "INSTR",
				"ISNULL", "LAST", "LCASE", "LEN", "LENGTH", "LOWER", "LTRIM", "MAX", "MID", "MIN", "MONTH",
				"NOW", "NZ", "REPLACE", "ROUND", "RTRIM", "STDEV", "SUBSTR", "SUM", "SWITCH", "TRIM",
				"UCASE", "UPPER", "VAL", "VAR", "YEAR",
			};
			Dictionary<string, SqlTokenType> words =
				new Dictionary<string, SqlTokenType>(StringComparer.OrdinalIgnoreCase);
			foreach (string word in keywords) {
				words[word] = SqlTokenType.Keyword;
			}
			foreach (string word in functions) {
				words[word] = SqlTokenType.Function;
			}
			return words;
		}


		/// <summary>Returns the tokens in <paramref name="sql"/>, in order.</summary>
		public static List<SqlToken> Tokenize(string sql)
		{
			List<SqlToken> tokens = new List<SqlToken>();
			if (string.IsNullOrEmpty(sql)) {
				return tokens;
			}
			int i = 0;
			int n = sql.Length;
			while (i < n) {
				char c = sql[i];
				char next = i + 1 < n ? sql[i + 1] : '\0';
				int start = i;
				if (c == '-' && next == '-') {
					while (i < n && sql[i] != '\n' && sql[i] != '\r') {
						i++;
					}
					tokens.Add(new SqlToken(start, i - start, SqlTokenType.Comment));
				} else if (c == '/' && next == '*') {
					int end = sql.IndexOf("*/", i + 2, StringComparison.Ordinal);
					i = end < 0 ? n : end + 2;
					tokens.Add(new SqlToken(start, i - start, SqlTokenType.Comment));
				} else if (c == '\'') {
					i = SkipQuoted(sql, i, '\'');
					tokens.Add(new SqlToken(start, i - start, SqlTokenType.String));
				} else if (c == '"' || c == '`') {
					i = SkipQuoted(sql, i, c);
					tokens.Add(new SqlToken(start, i - start, SqlTokenType.QuotedIdentifier));
				} else if (c == '[') {
					int end = sql.IndexOf(']', i + 1);
					i = end < 0 ? n : end + 1;
					tokens.Add(new SqlToken(start, i - start, SqlTokenType.QuotedIdentifier));
				} else if (c == '#' && IsJetDateLiteral(sql, i)) {
					// JetSQL date literal: #2026-11-10#
					i = sql.IndexOf('#', i + 1) + 1;
					tokens.Add(new SqlToken(start, i - start, SqlTokenType.String));
				} else if (c == '?') {
					i++;
					tokens.Add(new SqlToken(start, 1, SqlTokenType.Parameter));
				} else if ((c == '@' || c == ':' || c == '$') && IsWordStart(next)) {
					i++;
					while (i < n && IsWordPart(sql[i])) {
						i++;
					}
					tokens.Add(new SqlToken(start, i - start, SqlTokenType.Parameter));
				} else if (char.IsDigit(c) || (c == '.' && char.IsDigit(next))) {
					while (i < n && (char.IsDigit(sql[i]) || sql[i] == '.')) {
						i++;
					}
					if (i < n && (sql[i] == 'e' || sql[i] == 'E')) {
						int j = i + 1;
						if (j < n && (sql[j] == '+' || sql[j] == '-')) {
							j++;
						}
						if (j < n && char.IsDigit(sql[j])) {
							i = j;
							while (i < n && char.IsDigit(sql[i])) {
								i++;
							}
						}
					}
					// 1abc is an identifier, not a number.
					if (i < n && IsWordPart(sql[i])) {
						while (i < n && IsWordPart(sql[i])) {
							i++;
						}
					} else {
						tokens.Add(new SqlToken(start, i - start, SqlTokenType.Number));
					}
				} else if (IsWordStart(c)) {
					while (i < n && IsWordPart(sql[i])) {
						i++;
					}
					SqlTokenType type;
					// A word after a dot is a column name, even when it is spelled like a keyword.
					bool qualified = start > 0 && sql[start - 1] == '.';
					if (!qualified && Words.TryGetValue(sql.Substring(start, i - start), out type)) {
						tokens.Add(new SqlToken(start, i - start, type));
					}
				} else {
					i++;
				}
			}
			return tokens;
		}


		/// <summary>Returns the index just past a quoted run; a doubled quote is an escaped quote.</summary>
		private static int SkipQuoted(string sql, int start, char quote)
		{
			int i = start + 1;
			while (i < sql.Length) {
				if (sql[i] == quote) {
					if (i + 1 < sql.Length && sql[i + 1] == quote) {
						i += 2;
						continue;
					}
					return i + 1;
				}
				i++;
			}
			return sql.Length;
		}


		private static bool IsJetDateLiteral(string sql, int start)
		{
			int end = sql.IndexOf('#', start + 1);
			if (end < 0 || end - start > 32) {
				return false;
			}
			for (int i = start + 1; i < end; i++) {
				char c = sql[i];
				if (!(char.IsLetterOrDigit(c) || c == '/' || c == '-' || c == ':' || c == ' ' || c == '.')) {
					return false;
				}
			}
			return end > start + 1;
		}


		private static bool IsWordStart(char c)
		{
			return char.IsLetter(c) || c == '_';
		}


		private static bool IsWordPart(char c)
		{
			return char.IsLetterOrDigit(c) || c == '_';
		}
	}
}
