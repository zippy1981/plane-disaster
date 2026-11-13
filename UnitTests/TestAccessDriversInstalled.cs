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
using System.IO;

using NUnit.Framework;
using PlaneDisaster.Dba;

namespace UnitTests
{
	/// <summary>
	/// The Access drivers that are really installed, against the sample files in TestData. Needs Windows.
	/// </summary>
	/// <remarks>
	/// CI sets PLANEDISASTER_EXPECT_ACCESS_FORMATS on each leg to the formats its drivers must open,
	/// such as "Jet4,Ace", so a leg can't pass by skipping what it was set up to test.
	/// PLANEDISASTER_EXPECT_ACCESS_ENGINE names the Access Database Engine it installed: "msi" or "clicktorun".
	/// </remarks>
	[TestFixture]
	public sealed class TestAccessDriversInstalled
	{
		private const string ExpectFormatsVariable = "PLANEDISASTER_EXPECT_ACCESS_FORMATS";
		private const string ExpectEngineVariable = "PLANEDISASTER_EXPECT_ACCESS_ENGINE";

		private static readonly AccessDriverKind[] Kinds = { AccessDriverKind.OleDb, AccessDriverKind.Odbc };

		private string _fileName;


		/// <summary>The formats this machine's drivers are expected to open. Empty when nothing was promised.</summary>
		internal static List<AccessFileFormat> ExpectedFormats {
			get {
				List<AccessFileFormat> formats = new List<AccessFileFormat>();
				string value = Environment.GetEnvironmentVariable(ExpectFormatsVariable) ?? "";
				foreach (string name in value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)) {
					formats.Add((AccessFileFormat) Enum.Parse(typeof(AccessFileFormat), name.Trim(), true));
				}
				return formats;
			}
		}


		[TearDown]
		public void TearDown() {
			if (_fileName != null) {
				File.Delete(_fileName);
				_fileName = null;
			}
		}


		/// <summary>Copies a file from TestData to the temp directory, where Access can write its lock file.</summary>
		private string Copy(string sample) {
			string directory = Path.GetDirectoryName(typeof(TestAccessDriversInstalled).Assembly.Location);
			string source = Path.Combine(Path.Combine(directory, "TestData"), sample);
			_fileName = Path.Combine(Path.GetTempPath(), string.Format("PlaneDisaster-UnitTest-{0}{1}", Guid.NewGuid(), Path.GetExtension(sample)));
			File.Copy(source, _fileName);
			File.SetAttributes(_fileName, FileAttributes.Normal);
			return _fileName;
		}


		[TestCase("census-pl2000-access97.mdb", AccessFileFormat.Jet3)]
		[TestCase("nist-albatross.mdb", AccessFileFormat.Jet4)]
		[TestCase("nist-albatross.accdb", AccessFileFormat.Ace)]
		public void DetectFormatKnowsTheSampleFiles(string sample, AccessFileFormat expected) {
			Assert.AreEqual(expected, AccessDrivers.DetectFormat(Copy(sample)));
		}


		/// <summary>The test log shows which drivers each CI leg really ran against.</summary>
		[TestCase(AccessDriverKind.OleDb)]
		[TestCase(AccessDriverKind.Odbc)]
		public void InstalledDriversOfThisBitnessHaveTheirDll(AccessDriverKind kind) {
			foreach (bool is64Bit in new bool[] {false, true}) {
				foreach (AccessDriver driver in AccessDrivers.GetInstalled(kind, is64Bit)) {
					Console.WriteLine("{0}: {1} [{2}]", kind, driver, driver.FileName);
					Assert.AreEqual(is64Bit, driver.Is64Bit);
					if (is64Bit == AccessDrivers.Is64BitProcess) {
						Assert.IsTrue(File.Exists(driver.FileName), driver.FileName);
					}
				}
			}
		}


		[Test]
		public void A32BitProcessAlwaysHasJet() {
			if (AccessDrivers.Is64BitProcess) {
				Assert.Ignore("Jet 4.0 is 32 bit only.");
			}
			foreach (AccessDriverKind kind in Kinds) {
				Assert.IsTrue(AccessDrivers.Find(kind, AccessFileFormat.Jet3).IsJet, kind.ToString());
			}
		}


		[Test]
		public void TheExpectedFormatsHaveADriver() {
			foreach (AccessFileFormat format in ExpectedFormats) {
				foreach (AccessDriverKind kind in Kinds) {
					AccessDriver driver = AccessDrivers.Find(kind, format);
					Console.WriteLine("{0} {1}: {2}", format, kind, driver);
					Assert.IsTrue(driver.Reads(format));
					Assert.AreEqual(AccessDrivers.Is64BitProcess, driver.Is64Bit);
				}
			}
		}


		[Test]
		public void TheExpectedAccessDatabaseEngineIsTheOneInUse() {
			string expected = Environment.GetEnvironmentVariable(ExpectEngineVariable);
			if (string.IsNullOrEmpty(expected)) {
				return;
			}
			foreach (AccessDriverKind kind in Kinds) {
				AccessDriver driver = AccessDrivers.Find(kind, AccessFileFormat.Ace);
				Assert.AreEqual(expected == "clicktorun", driver.IsClickToRun, string.Format("{0} [{1}]", driver, driver.FileName));
			}
		}


		/// <summary>
		/// Every sample either opens with the driver that was picked for it, or the error says which
		/// build of PlaneDisaster.NET or which driver would open it.
		/// </summary>
		[TestCase("census-pl2000-access97.mdb", 5)]
		[TestCase("nist-albatross.mdb", 21)]
		[TestCase("nist-albatross.accdb", 21)]
		public void EachSampleOpensOrSaysWhatWould(string sample, int tables) {
			string fileName = Copy(sample);
			AccessFileFormat format = AccessDrivers.DetectFormat(fileName);
			int bits = AccessDrivers.Is64BitProcess ? 64 : 32;

			using (OleDba oleDb = new OleDba()) {
				try {
					oleDb.ConnectMDB(fileName);
					Assert.AreEqual(tables, oleDb.GetTables().Length);
					oleDb.Disconnect();
				} catch (AccessDriverNotFoundException ex) {
					CollectionAssert.DoesNotContain(ExpectedFormats, format, ex.Message);
					StringAssert.Contains(string.Format("This is the {0} bit PlaneDisaster.NET", bits), ex.Message);
				}
			}
			using (OdbcDba odbc = new OdbcDba()) {
				try {
					odbc.ConnectMDB(fileName);
					Assert.GreaterOrEqual(odbc.GetTables().Length, tables);
					odbc.Disconnect();
				} catch (AccessDriverNotFoundException ex) {
					CollectionAssert.DoesNotContain(ExpectedFormats, format, ex.Message);
					StringAssert.Contains(string.Format("no {0} bit ODBC driver", bits), ex.Message);
				}
			}
		}


		[Test]
		public void CreatingAnAccess95DatabaseNeedsJet() {
			_fileName = Path.Combine(Path.GetTempPath(), string.Format("PlaneDisaster-UnitTest-{0}.mdb", Guid.NewGuid()));
			try {
				JetSqlUtil.CreateMDB(_fileName, AccessDbVersion.Access95);
			} catch (AccessDriverNotFoundException ex) {
				CollectionAssert.DoesNotContain(ExpectedFormats, AccessFileFormat.Jet3, ex.Message);
				Assert.IsFalse(File.Exists(_fileName));
				_fileName = null;
				return;
			}
			Assert.AreEqual(AccessFileFormat.Jet3, AccessDrivers.DetectFormat(_fileName));
		}
	}
}
