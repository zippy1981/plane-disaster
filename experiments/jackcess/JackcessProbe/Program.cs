// Can .NET on Linux create, write and query Access files with Jackcess compiled by IKVM?
// 1. Jackcess directly: create .accdb and .mdb files, add a table and rows, read them back,
//    and read Northwind (written by Access itself).
// 2. UCanAccess, Jackcess plus HSQLDB behind JDBC: SQL DDL, ? parameters, SELECT, against the
//    same files, then check the result with Jackcess.
// Prints one Markdown table row per step; every step runs even if an earlier one failed.
//
// Usage: JackcessProbe <work dir> [nwind.mdb]

using System;
using System.Collections.Generic;
using System.IO;
using com.healthmarketscience.jackcess;
using JFile = java.io.File;

namespace JackcessProbe
{
	internal static class Program
	{
		private static int _failed;

		private static int Main(string[] args)
		{
			string work = args.Length > 0 ? args[0] : Path.GetTempPath();
			string nwind = args.Length > 1 ? args[1] : null;
			Directory.CreateDirectory(work);

			Console.WriteLine("| Step | Result | Detail |");
			Console.WriteLine("| --- | --- | --- |");

			foreach (KeyValuePair<string, Database.FileFormat> format in new[] {
				new KeyValuePair<string, Database.FileFormat>("new.accdb", Database.FileFormat.V2010),
				new KeyValuePair<string, Database.FileFormat>("new.mdb", Database.FileFormat.V2003),
			}) {
				string path = Path.Combine(work, format.Key);
				File.Delete(path);
				JFile file = new JFile(path);

				Step("Jackcess: create " + format.Key, delegate {
					Database db = new DatabaseBuilder(file).setFileFormat(format.Value).create();
					Table table = new TableBuilder("tblTest")
						.addColumn(new ColumnBuilder("id", DataType.LONG))
						.addColumn(new ColumnBuilder("description", DataType.TEXT))
						.toTable(db);
					string[] rows = { "Number one.", "Number two.", "Number three." };
					for (int i = 0; i < rows.Length; i++) {
						table.addRow(java.lang.Integer.valueOf(i + 1), rows[i]);
					}
					db.close();
					return new FileInfo(path).Length + " bytes";
				});

				Step("Jackcess: read " + format.Key, delegate {
					Database db = DatabaseBuilder.open(file);
					try {
						Table table = db.getTable("tblTest");
						return db.getFileFormat() + ", " + table.getRowCount() + " rows, first: " + table.getNextRow().get("description");
					} finally {
						db.close();
					}
				});

				string url = "jdbc:ucanaccess://" + path + ";memory=false";
				Step("UCanAccess: SQL on " + format.Key, delegate {
					java.sql.Connection cn = new net.ucanaccess.jdbc.UcanaccessDriver().connect(url, new java.util.Properties());
					try {
						java.sql.Statement st = cn.createStatement();
						st.executeUpdate("CREATE TABLE tblSql (id INTEGER PRIMARY KEY, description VARCHAR(255))");
						java.sql.PreparedStatement insert = cn.prepareStatement("INSERT INTO tblSql (id, description) VALUES (?, ?)");
						for (int i = 1; i <= 3; i++) {
							insert.setInt(1, i);
							insert.setString(2, "Row " + i + "; with 'quotes'");
							insert.executeUpdate();
						}
						java.sql.ResultSet rs = st.executeQuery(
							"SELECT COUNT(*), MAX(t.description) FROM tblSql s INNER JOIN tblTest t ON s.id = t.id");
						rs.next();
						return rs.getInt(1) + " joined rows, max: " + rs.getString(2);
					} finally {
						cn.close();
					}
				});

				Step("Jackcess: UCanAccess's table is in " + format.Key, delegate {
					Database db = DatabaseBuilder.open(file);
					try {
						return "tables: " + db.getTableNames() + ", tblSql rows: " + db.getTable("tblSql").getRowCount();
					} finally {
						db.close();
					}
				});
			}

			if (nwind != null) {
				Step("Jackcess: read Northwind (written by Access)", delegate {
					Database db = new DatabaseBuilder(new JFile(nwind)).setReadOnly(true).open();
					try {
						return db.getFileFormat() + ", " + db.getTableNames().size() + " tables, Customers: " + db.getTable("Customers").getRowCount() + " rows";
					} finally {
						db.close();
					}
				});
				Step("UCanAccess: query Northwind", delegate {
					java.sql.Connection cn = new net.ucanaccess.jdbc.UcanaccessDriver().connect(
						"jdbc:ucanaccess://" + nwind + ";memory=true;openExclusive=false;readOnly=true", new java.util.Properties());
					try {
						java.sql.ResultSet rs = cn.createStatement().executeQuery(
							"SELECT TOP 1 c.CompanyName, COUNT(o.OrderID) AS n FROM Customers c INNER JOIN Orders o ON c.CustomerID = o.CustomerID GROUP BY c.CompanyName ORDER BY 2 DESC");
						rs.next();
						return "most orders: " + rs.getString(1) + " (" + rs.getInt(2) + ")";
					} finally {
						cn.close();
					}
				});
			}

			Console.WriteLine();
			Console.WriteLine(_failed == 0 ? "All steps passed." : _failed + " steps failed.");
			return _failed == 0 ? 0 : 1;
		}

		private delegate string Probe();

		private static void Step(string step, Probe probe)
		{
			try {
				Console.WriteLine("| {0} | pass | {1} |", step, Escape(probe()));
			} catch (Exception ex) {
				_failed++;
				Console.WriteLine("| {0} | FAIL | {1}: {2} |", step, ex.GetType().Name, Escape(ex.Message));
			}
		}

		private static string Escape(string text)
		{
			text = (text ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ").Trim();
			return text.Length > 200 ? text.Substring(0, 200) + "..." : text;
		}
	}
}
