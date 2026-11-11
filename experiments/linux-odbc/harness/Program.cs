// Probes an ODBC driver through PlaneDisaster.Dba.OdbcDba, the way PlaneDisaster.NET uses it, and
// prints one Markdown table row per operation. Every step runs even if an earlier one failed.
//
// Usage: OdbcHarness --name <label> --connection <odbc connection string>
//                    [--mode rw|ro] [--table <name>] [--select-name <sql name>] [--setup <sql>]...
//   rw (default): create tblTest, insert three rows with ? parameters, then read it back.
//   ro: read an existing table (--table) from a fixture.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using PlaneDisaster.Dba;

namespace OdbcHarness
{
	/// <summary>OdbcDba with its connection's driver name exposed.</summary>
	internal sealed class ProbeDba : OdbcDba
	{
		public string Driver { get { return ((OdbcConnection)Cn).Driver; } }
		public string DriverVersion { get { return ((OdbcConnection)Cn).ServerVersion; } }
	}

	internal static class Program
	{
		private static int _passed;
		private static int _failed;

		private static int Main(string[] args)
		{
			string name = "driver", connection = null, mode = "rw", table = "tblTest", selectName = null;
			List<string> setup = new List<string>();
			for (int i = 0; i + 1 < args.Length; i += 2) {
				switch (args[i]) {
					case "--name": name = args[i + 1]; break;
					case "--connection": connection = args[i + 1]; break;
					case "--mode": mode = args[i + 1]; break;
					case "--table": table = args[i + 1]; break;
					case "--select-name": selectName = args[i + 1]; break;
					case "--setup": setup.Add(args[i + 1]); break;
					default: Console.Error.WriteLine("Unknown option " + args[i]); return 2;
				}
			}
			// How plain SQL names the table; a name with spaces needs the driver's quoting.
			if (selectName == null) selectName = table;
			if (connection == null) {
				Console.Error.WriteLine("--connection is required");
				return 2;
			}

			Console.WriteLine("### " + name);
			Console.WriteLine();
			Console.WriteLine("| Step | Result | Detail |");
			Console.WriteLine("| --- | --- | --- |");

			ProbeDba db = new ProbeDba();
			if (!Step("connect", delegate { db.ConnectDSN(connection); return db.Driver + " " + db.DriverVersion; })) {
				Summary();
				return 1;
			}
			foreach (string sql in setup) {
				Step("setup: " + sql, delegate { db.ExecuteSqlCommand(sql); return ""; });
			}

			if (mode == "rw") {
				Step("CREATE TABLE", delegate {
					db.ExecuteSqlCommand("CREATE TABLE tblTest (id INTEGER PRIMARY KEY, description VARCHAR(255))");
					return "";
				});
				Step("INSERT with ? parameters", delegate {
					string[] rows = { "Number one.", "Number two.", "Number three." };
					for (int i = 0; i < rows.Length; i++) {
						db.ExecuteSqlCommand("INSERT INTO tblTest (id, description) VALUES (?, ?)",
							new DbParameter[] { new OdbcParameter("id", i + 1), new OdbcParameter("description", rows[i]) });
					}
					return "3 rows";
				});
			}

			Step("GetTables (schema)", delegate {
				string[] tables = db.GetTables();
				if (Array.IndexOf(tables, table) < 0) throw new Exception(table + " not in [" + string.Join(", ", tables) + "]");
				return tables.Length + " tables";
			});
			Step("GetColumnNames (schema)", delegate { return string.Join(", ", db.GetColumnNames(table)); });
			Step("SELECT without brackets", delegate {
				return db.GetSqlAsDataTable("SELECT * FROM " + selectName).Rows.Count + " rows";
			});
			Step("GetTableRowCount ([brackets])", delegate { return db.GetTableRowCount(table) + " rows"; });
			Step("GetTableAsDataTable ([brackets])", delegate { return db.GetTableAsDataTable(table).Rows.Count + " rows"; });
			Step("GetSQLAsCSV", delegate {
				string csv = db.GetSQLAsCSV("SELECT * FROM " + selectName);
				return csv.Split('\n')[0].Trim();
			});
			Step("SupportsViews", delegate { return db.SupportsViews.ToString(); });
			if (mode == "rw") {
				Step("DROP TABLE", delegate { db.DropTable("tblTest"); return ""; });
			}
			db.Disconnect();
			Summary();
			return _failed == 0 ? 0 : 1;
		}

		private delegate string Probe();

		private static bool Step(string step, Probe probe)
		{
			try {
				string detail = probe();
				Console.WriteLine("| {0} | pass | {1} |", step, Escape(detail));
				_passed++;
				return true;
			} catch (Exception ex) {
				Console.WriteLine("| {0} | FAIL | {1}: {2} |", step, ex.GetType().Name, Escape(ex.Message));
				_failed++;
				return false;
			}
		}

		private static string Escape(string text)
		{
			text = text.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ").Trim();
			// Drivers repeat a message once per diagnostic record; the first 200 characters say it.
			return text.Length > 200 ? text.Substring(0, 200) + "..." : text;
		}

		private static void Summary()
		{
			Console.WriteLine();
			Console.WriteLine("{0} passed, {1} failed.", _passed, _failed);
		}
	}
}
