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

namespace PlaneDisaster.Dba
{
	/// <summary>
	/// Extension methods for <see cref="Type"/>.
	/// </summary>
	internal static class TypeExtensions
	{
		/// <summary>
		/// The SQLite column type for a DataColumn's type. The declared type also tells
		/// System.Data.SQLite which .NET type to read the column back as.
		/// </summary>
		/// <exception cref="DataException">No SQLite column type holds this type.</exception>
		public static string ToSQLiteColumnType(this Type type) {
			if (type == typeof(string) || type == typeof(char) || type == typeof(TimeSpan)) {
				return "TEXT";
			} else if (type == typeof(DateTime)) {
				return "DATETIME";
			} else if (type == typeof(bool)) {
				return "BOOLEAN";
			} else if (type == typeof(Guid)) {
				return "GUID";
			} else if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)
			           || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong)) {
				return "INTEGER";
			} else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) {
				// Not NUMERIC for decimal: SQLite stores it as a double either way, and reading it back
				// as decimal shows 19.989999999999998436805981328 on .NET 11, whose double-to-decimal
				// conversion became exact. As REAL it reads back as the double 19.99.
				return "REAL";
			} else if (type == typeof(byte[])) {
				return "BLOB";
			}
			throw new DataException
				(String.Concat("DataTable2SQLiteTable() doesn't know how to map columns of type ", type.ToString()));
		}
	}
}

#if NET20
namespace System.Runtime.CompilerServices
{
	/// <summary>
	/// The compiler marks extension methods with this attribute. .NET Framework 2.0 has no
	/// System.Core to supply it, so the net20 build declares its own.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
	internal sealed class ExtensionAttribute : Attribute
	{
	}
}
#endif
