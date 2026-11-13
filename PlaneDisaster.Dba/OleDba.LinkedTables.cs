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
using System.Data.OleDb;

namespace PlaneDisaster.Dba
{
	/// <summary>A table in an Access database that is a link to a table somewhere else.</summary>
	public sealed class LinkedTableInfo
	{
		private readonly string _name;
		private readonly string _sourceDatabase;
		private readonly string _sourceTable;
		private readonly string _providerString;

		/// <summary>Creates a description of a linked table.</summary>
		public LinkedTableInfo(string name, string sourceDatabase, string sourceTable, string providerString)
		{
			_name = name;
			_sourceDatabase = sourceDatabase;
			_sourceTable = sourceTable;
			_providerString = providerString;
		}

		/// <summary>The link's name in this database.</summary>
		public string Name { get { return _name; } }

		/// <summary>The file the link points at, or empty for an ODBC link.</summary>
		public string SourceDatabase { get { return _sourceDatabase; } }

		/// <summary>The table's name in the source.</summary>
		public string SourceTable { get { return _sourceTable; } }

		/// <summary>The Jet connect string, such as "ODBC;DSN=..." or "Excel 12.0 Xml;". Empty for Access.</summary>
		public string ProviderString { get { return _providerString; } }
	}


	/// <summary>Linked tables: listing, adding and removing links in a Jet or ACE database.</summary>
	/// <remarks>
	/// Jet SQL has no statement that creates a link, so links are created through ADOX, late bound.
	/// ADOX ships with Windows in 32 and 64 bit, and uses the same OleDb provider as the connection.
	/// Removing a link is a DROP TABLE, which deletes only the link, never the source table.
	/// </remarks>
	public partial class OleDba
	{
		private const string LINK_DATASOURCE = "Jet OLEDB:Link Datasource";
		private const string REMOTE_TABLE_NAME = "Jet OLEDB:Remote Table Name";
		private const string LINK_PROVIDER_STRING = "Jet OLEDB:Link Provider String";
		private const string CREATE_LINK = "Jet OLEDB:Create Link";

		/// <summary>The TABLE_TYPE values the Jet provider reports for links.</summary>
		private static readonly string[] LinkTableTypes = { "LINK", "PASS-THROUGH" };


		/// <summary>The names of the linked tables, from the OleDb schema.</summary>
		public string[] GetLinkedTableNames()
		{
			List<string> names = new List<string>();
			foreach (string tableType in LinkTableTypes) {
				DataTable dt = ((OleDbConnection)Cn).GetOleDbSchemaTable
					(OleDbSchemaGuid.Tables, new object[] { null, null, null, tableType });
				foreach (DataRow row in dt.Rows) {
					names.Add((string)row["TABLE_NAME"]);
				}
			}
			names.Sort(StringComparer.OrdinalIgnoreCase);
			return names.ToArray();
		}


		/// <summary>
		/// The local tables and the linked tables, sorted together. <see cref="GetTables"/> stays local
		/// only, because callers such as Jet2SQLite copy what it returns.
		/// </summary>
		public string[] GetTablesAndLinks()
		{
			List<string> names = new List<string>(GetTables());
			names.AddRange(GetLinkedTableNames());
			names.Sort(StringComparer.OrdinalIgnoreCase);
			return names.ToArray();
		}


		/// <summary>True when <paramref name="tableName"/> is a linked table in this database.</summary>
		public bool IsLinkedTable(string tableName)
		{
			foreach (string name in GetLinkedTableNames()) {
				if (string.Equals(name, tableName, StringComparison.OrdinalIgnoreCase)) {
					return true;
				}
			}
			return false;
		}


		/// <summary>The linked tables with where each one points.</summary>
		public LinkedTableInfo[] GetLinkedTables()
		{
			List<LinkedTableInfo> links = new List<LinkedTableInfo>();
			string[] names = GetLinkedTableNames();
			if (names.Length == 0) {
				return links.ToArray();
			}
			object catalog = OpenCatalog();
			try {
				object tables = LateBound.Get(catalog, "Tables");
				try {
					foreach (string name in names) {
						object table = LateBound.Get(tables, "Item", name);
						try {
							links.Add(new LinkedTableInfo(name,
								GetTableProperty(table, LINK_DATASOURCE),
								GetTableProperty(table, REMOTE_TABLE_NAME),
								GetTableProperty(table, LINK_PROVIDER_STRING)));
						} finally {
							LateBound.Release(table);
						}
					}
				} finally {
					LateBound.Release(tables);
				}
			} finally {
				LateBound.Release(catalog);
			}
			return links.ToArray();
		}


		/// <summary>Links to a table in another Access database.</summary>
		/// <param name="linkName">The name the link gets in this database.</param>
		/// <param name="sourceDatabase">The full path of the .mdb or .accdb holding the table.</param>
		/// <param name="sourceTable">The table's name in that database.</param>
		public void LinkTable(string linkName, string sourceDatabase, string sourceTable)
		{
			LinkTable(linkName, sourceDatabase, sourceTable, null);
		}


		/// <summary>Links to a table in another database.</summary>
		/// <param name="linkName">The name the link gets in this database.</param>
		/// <param name="sourceDatabase">The source file, or null for an ODBC link.</param>
		/// <param name="sourceTable">The table's name in the source.</param>
		/// <param name="providerString">
		/// The Jet connect string, such as "ODBC;DSN=MyDsn;" or "Excel 12.0 Xml;HDR=YES;". Null for Access.
		/// </param>
		/// <exception cref="ArgumentException">A table or link named <paramref name="linkName"/> exists.</exception>
		public void LinkTable(string linkName, string sourceDatabase, string sourceTable, string providerString)
		{
			if (string.IsNullOrEmpty(linkName)) {
				throw new ArgumentException("A link needs a name.", "linkName");
			}
			if (string.IsNullOrEmpty(sourceTable)) {
				throw new ArgumentException("A link needs a source table.", "sourceTable");
			}
			if (Array.IndexOf(GetTables(), linkName) >= 0 || IsLinkedTable(linkName)) {
				throw new ArgumentException(string.Format("There is already a table named {0}.", linkName), "linkName");
			}

			object catalog = OpenCatalog();
			object table = null;
			object tables = null;
			try {
				table = LateBound.Create("ADOX.Table");
				LateBound.Set(table, "Name", linkName);
				LateBound.SetRef(table, "ParentCatalog", catalog);
				SetTableProperty(table, CREATE_LINK, true);
				if (!string.IsNullOrEmpty(sourceDatabase)) {
					SetTableProperty(table, LINK_DATASOURCE, sourceDatabase);
				}
				SetTableProperty(table, REMOTE_TABLE_NAME, sourceTable);
				if (!string.IsNullOrEmpty(providerString)) {
					SetTableProperty(table, LINK_PROVIDER_STRING, providerString);
				}
				tables = LateBound.Get(catalog, "Tables");
				LateBound.Call(tables, "Append", table);
			} finally {
				LateBound.Release(tables, table, catalog);
			}
			Reopen();
		}


		/// <summary>Closes and reopens the connection, so it sees what ADOX wrote through its own.</summary>
		/// <remarks>
		/// Jet caches what a connection has read and refreshes the cache every page timeout, 5 seconds by
		/// default. Until then this connection lists no new link and cannot select from it.
		/// </remarks>
		private void Reopen()
		{
			Cn.Close();
			Cn.Open();
		}


		/// <summary>Removes a link. The table it points at is not touched.</summary>
		/// <exception cref="ArgumentException"><paramref name="linkName"/> is not a linked table.</exception>
		public void UnlinkTable(string linkName)
		{
			// Check first: DROP TABLE on a real table would delete its data.
			if (!IsLinkedTable(linkName)) {
				throw new ArgumentException(string.Format("{0} is not a linked table.", linkName), "linkName");
			}
			ExecuteSqlCommand(string.Format("DROP TABLE [{0}]", linkName.Replace("]", "]]")));
		}


		private object OpenCatalog()
		{
			object catalog = LateBound.Create("ADOX.Catalog");
			try {
				// A string ActiveConnection makes ADOX open its own connection with the same provider.
				LateBound.Set(catalog, "ActiveConnection", ConnectionString);
			} catch {
				LateBound.Release(catalog);
				throw;
			}
			return catalog;
		}


		private static string GetTableProperty(object table, string name)
		{
			object properties = LateBound.Get(table, "Properties");
			object property = null;
			try {
				property = LateBound.Get(properties, "Item", name);
				object value = LateBound.Get(property, "Value");
				return value == null || value is DBNull ? string.Empty : value.ToString();
			} finally {
				LateBound.Release(property, properties);
			}
		}


		private static void SetTableProperty(object table, string name, object value)
		{
			object properties = LateBound.Get(table, "Properties");
			object property = null;
			try {
				property = LateBound.Get(properties, "Item", name);
				LateBound.Set(property, "Value", value);
			} finally {
				LateBound.Release(property, properties);
			}
		}
	}
}
