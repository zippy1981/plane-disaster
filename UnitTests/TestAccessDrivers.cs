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
using System.Text;

using NUnit.Framework;
using PlaneDisaster.Dba;

namespace UnitTests
{
	/// <summary>
	/// How an Access driver is chosen for a file, and what the user is told when there is none.
	/// These tests need no driver and no Windows: they describe the installed drivers themselves.
	/// </summary>
	[TestFixture]
	public sealed class TestAccessDrivers
	{
		private string _fileName;


		[TearDown]
		public void TearDown() {
			if (_fileName != null) {
				File.Delete(_fileName);
				_fileName = null;
			}
		}


		/// <summary>
		/// Builds drivers from names: "jet" is Jet 4.0 and "ace16" is the Access Database Engine 16.
		/// </summary>
		private static List<AccessDriver> Drivers(string names, bool is64Bit) {
			List<AccessDriver> drivers = new List<AccessDriver>();
			foreach (string name in names.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)) {
				int version = name == "jet" ? AccessDriver.JetEngineVersion : int.Parse(name.Substring(3));
				drivers.Add(new AccessDriver(AccessDriverKind.OleDb, name, version, is64Bit, null));
			}
			return drivers;
		}


		[TestCase("Standard Jet DB", 0, ".mdb", AccessFileFormat.Jet3)]
		[TestCase("Standard Jet DB", 1, ".mdb", AccessFileFormat.Jet4)]
		[TestCase("Standard ACE DB", 2, ".accdb", AccessFileFormat.Ace)]
		[TestCase("Standard ACE DB", 5, ".accdb", AccessFileFormat.Ace)]
		// The header wins over the extension: an .accdb renamed to .mdb still needs an ACE driver.
		[TestCase("Standard ACE DB", 2, ".mdb", AccessFileFormat.Ace)]
		[TestCase("Standard Jet DB", 0, ".accdb", AccessFileFormat.Jet3)]
		[TestCase("not an Access db", 0, ".mdb", AccessFileFormat.Unknown)]
		[TestCase("not an Access db", 0, ".accdb", AccessFileFormat.Ace)]
		public void DetectFormatReadsTheHeader(string signature, int version, string extension, AccessFileFormat expected) {
			_fileName = Path.Combine(Path.GetTempPath(), string.Format("PlaneDisaster-UnitTest-{0}{1}", Guid.NewGuid(), extension));
			byte[] header = new byte[32];
			header[1] = 1;
			Encoding.ASCII.GetBytes(signature, 0, 15, header, 4);
			header[20] = (byte) version;
			File.WriteAllBytes(_fileName, header);

			Assert.AreEqual(expected, AccessDrivers.DetectFormat(_fileName));
		}


		[TestCase(".mdb", AccessFileFormat.Unknown)]
		[TestCase(".accdb", AccessFileFormat.Ace)]
		[TestCase(".accde", AccessFileFormat.Ace)]
		public void DetectFormatGoesByTheExtensionOfAMissingFile(string extension, AccessFileFormat expected) {
			string missing = Path.Combine(Path.GetTempPath(), string.Format("PlaneDisaster-UnitTest-{0}{1}", Guid.NewGuid(), extension));
			Assert.AreEqual(expected, AccessDrivers.DetectFormat(missing));
		}


		[Test]
		public void DetectFormatGoesByTheExtensionOfATruncatedFile() {
			_fileName = Path.Combine(Path.GetTempPath(), string.Format("PlaneDisaster-UnitTest-{0}.mdb", Guid.NewGuid()));
			File.WriteAllBytes(_fileName, new byte[] {0, 1, 0, 0});
			Assert.AreEqual(AccessFileFormat.Unknown, AccessDrivers.DetectFormat(_fileName));
		}


		[TestCase("jet", AccessFileFormat.Jet3, true)]
		[TestCase("jet", AccessFileFormat.Jet4, true)]
		[TestCase("jet", AccessFileFormat.Ace, false)]
		[TestCase("jet", AccessFileFormat.Unknown, true)]
		[TestCase("ace12", AccessFileFormat.Jet3, true)]
		[TestCase("ace14", AccessFileFormat.Jet3, true)]
		[TestCase("ace15", AccessFileFormat.Jet3, false)]
		[TestCase("ace16", AccessFileFormat.Jet3, false)]
		[TestCase("ace16", AccessFileFormat.Jet4, true)]
		[TestCase("ace16", AccessFileFormat.Ace, true)]
		[TestCase("ace16", AccessFileFormat.Unknown, true)]
		public void ReadsSaysWhichFormatsADriverOpens(string driver, AccessFileFormat format, bool expected) {
			Assert.AreEqual(expected, Drivers(driver, false)[0].Reads(format));
		}


		[TestCase("jet", AccessFileFormat.Jet4, "jet")]
		[TestCase("jet", AccessFileFormat.Ace, null)]
		[TestCase("ace16", AccessFileFormat.Jet3, null)]
		[TestCase("", AccessFileFormat.Jet4, null)]
		// 32 bit with the Access Database Engine installed: each format goes to the driver that opens it.
		[TestCase("ace16,ace12,jet", AccessFileFormat.Jet3, "jet")]
		[TestCase("ace16,ace12,jet", AccessFileFormat.Jet4, "ace16")]
		[TestCase("ace16,ace12,jet", AccessFileFormat.Ace, "ace16")]
		[TestCase("ace16,ace12,jet", AccessFileFormat.Unknown, "ace16")]
		// The order they are found in doesn't matter.
		[TestCase("jet,ace12,ace16", AccessFileFormat.Jet4, "ace16")]
		[TestCase("jet,ace14", AccessFileFormat.Jet3, "jet")]
		[TestCase("ace14,jet", AccessFileFormat.Jet3, "jet")]
		[TestCase("ace14", AccessFileFormat.Jet3, "ace14")]
		public void PickChoosesTheDriverForAFormat(string installed, AccessFileFormat format, string expected) {
			AccessDriver driver = AccessDrivers.Pick(Drivers(installed, false), format);
			Assert.AreEqual(expected, driver == null ? null : driver.Name);
		}


		[Test]
		public void GuidanceFor64BitAndAccess97PointsAtThe32BitBuild() {
			string message = AccessDrivers.Guidance(AccessDriverKind.OleDb, AccessFileFormat.Jet3, true,
				Drivers("ace16", true), Drivers("jet", false));

			StringAssert.Contains("This is the 64 bit PlaneDisaster.NET", message);
			StringAssert.Contains("Access 97", message);
			StringAssert.Contains("The 32 bit PlaneDisaster.NET can open it with jet, which ships with Windows", message);
			StringAssert.Contains("x86 build from " + AccessDrivers.ReleasesUrl, message);
			// No current 64 bit driver opens Access 97, so there is nothing to tell the user to install.
			StringAssert.DoesNotContain(AccessDrivers.RedistributableUrl, message);
		}


		[Test]
		public void GuidanceFor32BitAndAccdbOffersThe64BitBuildAndTheDriver() {
			string message = AccessDrivers.Guidance(AccessDriverKind.Odbc, AccessFileFormat.Ace, false,
				Drivers("jet", false), Drivers("ace16", true));

			StringAssert.Contains("no 32 bit ODBC driver", message);
			StringAssert.Contains("The 64 bit PlaneDisaster.NET can open it with ace16.", message);
			StringAssert.Contains("x64 build", message);
			StringAssert.Contains("Or install the 32 bit Microsoft Access Database Engine 2016 Redistributable from " + AccessDrivers.RedistributableUrl, message);
		}


		[TestCase(true, 64)]
		[TestCase(false, 32)]
		public void GuidanceWithNoDriverAnywhereSaysWhatToInstall(bool is64BitProcess, int bits) {
			string message = AccessDrivers.Guidance(AccessDriverKind.OleDb, AccessFileFormat.Jet4, is64BitProcess,
				Drivers("", is64BitProcess), Drivers("", !is64BitProcess));

			StringAssert.Contains(string.Format("Install the {0} bit Microsoft Access Database Engine 2016 Redistributable", bits), message);
			StringAssert.DoesNotContain("can open it with", message);
			StringAssert.DoesNotContain("Installed:", message);
		}


		[Test]
		public void GuidanceListsEachInstalledDriverOnce() {
			string message = AccessDrivers.Guidance(AccessDriverKind.OleDb, AccessFileFormat.Jet3, true,
				Drivers("ace16,ace16", true), Drivers("", false));

			StringAssert.Contains("Installed: ace16 (64 bit, Access Database Engine 16).", message);
		}
	}
}
