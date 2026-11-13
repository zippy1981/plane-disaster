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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PlaneDisaster.Dba
{
	/// <summary>No installed Access driver of this process's bitness opens the database.</summary>
	/// <remarks>The message says what would: the other bitness of PlaneDisaster.NET, or a driver to install.</remarks>
	public sealed class AccessDriverNotFoundException : InvalidOperationException
	{
		private readonly AccessFileFormat _format;

		/// <summary>Creates the exception.</summary>
		public AccessDriverNotFoundException(string message, AccessFileFormat format) : base(message)
		{
			_format = format;
		}

		/// <summary>The format no driver opens.</summary>
		public AccessFileFormat Format { get { return _format; } }
	}


	/// <summary>
	/// Finds the Access drivers that are installed and picks the one to open a database with.
	/// </summary>
	/// <remarks>
	/// A driver only loads into a process of its own bitness, and no driver opens every format: Jet 4.0
	/// is 32 bit only and doesn't open .accdb, and the Access Database Engine 2013 and later don't open
	/// Access 97 files. So the choice depends on the file and on whether this is the 32 or 64 bit build.
	/// </remarks>
	public static class AccessDrivers
	{
		/// <summary>Where to get the x86 and x64 builds.</summary>
		public const string ReleasesUrl = "https://github.com/zippy1981/plane-disaster/releases/latest";

		/// <summary>The Microsoft Access Database Engine 2016 Redistributable, which has a 32 and a 64 bit installer.</summary>
		public const string RedistributableUrl = "https://www.microsoft.com/en-us/download/details.aspx?id=54920";

		/// <summary>The OLE DB ProgIDs for Access.</summary>
		private static readonly string[] OleDbProgIds = {
			"Microsoft.ACE.OLEDB.16.0",
			"Microsoft.ACE.OLEDB.15.0",
			"Microsoft.ACE.OLEDB.14.0",
			"Microsoft.ACE.OLEDB.12.0",
			JetOleDbProgId,
		};

		private const string JetOleDbProgId = "Microsoft.Jet.OLEDB.4.0";
		private const string AceOdbcDriverName = "Microsoft Access Driver (*.mdb, *.accdb)";
		private const string JetOdbcDriverName = "Microsoft Access Driver (*.mdb)";

		/// <summary>What the engine version is taken to be when the driver's DLL can't be read.</summary>
		private const int NewestEngineVersion = 16;

		/// <summary>The signature at offset 4 of an Access 2007 and later file. Older files say "Standard Jet DB".</summary>
		private const string AceSignature = "Standard ACE DB";
		private const string JetSignature = "Standard Jet DB";
		private const int SignatureOffset = 4;
		/// <summary>The byte that holds the format version: 0 is Jet 3, 1 is Jet 4, 2 and up are .accdb.</summary>
		private const int VersionOffset = 20;

		#region Registry

		private static readonly UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
		private const int KEY_QUERY_VALUE = 0x0001;
		private const int KEY_WOW64_64KEY = 0x0100;
		private const int KEY_WOW64_32KEY = 0x0200;

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
		private static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int options, int samDesired, out IntPtr result);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
		private static extern int RegQueryValueEx(IntPtr hKey, string valueName, IntPtr reserved, out int type, byte[] data, ref int dataSize);

		[DllImport("advapi32.dll")]
		private static extern int RegCloseKey(IntPtr hKey);

		/// <summary>
		/// Reads a string from HKEY_LOCAL_MACHINE as 32 or 64 bit programs see it. RegistryKey can't
		/// choose the view before .NET Framework 4, and the net20 build is the one that ships.
		/// </summary>
		/// <returns>The value with environment variables expanded, or null when it isn't there.</returns>
		private static string ReadMachineString(string subKey, string valueName, bool is64Bit)
		{
			IntPtr key;
			int access = KEY_QUERY_VALUE | (is64Bit ? KEY_WOW64_64KEY : KEY_WOW64_32KEY);
			if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, subKey, 0, access, out key) != 0) {
				return null;
			}
			try {
				int type;
				int size = 0;
				if (RegQueryValueEx(key, valueName, IntPtr.Zero, out type, null, ref size) != 0 || size == 0) {
					return null;
				}
				byte[] data = new byte[size];
				if (RegQueryValueEx(key, valueName, IntPtr.Zero, out type, data, ref size) != 0) {
					return null;
				}
				string value = Encoding.Unicode.GetString(data, 0, size).TrimEnd('\0');
				return value.Length == 0 ? null : Environment.ExpandEnvironmentVariables(value);
			} finally {
				RegCloseKey(key);
			}
		}

		#endregion Registry


		/// <summary>True when this process is 64 bit.</summary>
		public static bool Is64BitProcess { get { return IntPtr.Size == 8; } }

		/// <summary>True when Windows is 64 bit, so drivers of both bitnesses can be installed.</summary>
		private static bool Is64BitWindows {
			// Windows sets PROCESSOR_ARCHITEW6432 only for a 32 bit process on 64 bit Windows.
			get { return Is64BitProcess || Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432") != null; }
		}

		private static bool IsWindows {
			get { return Environment.OSVersion.Platform == PlatformID.Win32NT; }
		}


		/// <summary>
		/// Reads the format from the file's header. Falls back to the extension when the file is
		/// missing or unreadable, so a driver can still be chosen and report the real error.
		/// </summary>
		public static AccessFileFormat DetectFormat(string fileName)
		{
			byte[] header = new byte[VersionOffset + 1];
			int read = 0;
			try {
				using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
					read = stream.Read(header, 0, header.Length);
				}
			} catch (IOException) {
			} catch (UnauthorizedAccessException) {
			}
			if (read == header.Length) {
				string signature = Encoding.ASCII.GetString(header, SignatureOffset, JetSignature.Length);
				if (signature == AceSignature) {
					return AccessFileFormat.Ace;
				}
				if (signature == JetSignature) {
					switch (header[VersionOffset]) {
						case 0: return AccessFileFormat.Jet3;
						case 1: return AccessFileFormat.Jet4;
						default: return AccessFileFormat.Ace;
					}
				}
			}
			string extension = Path.GetExtension(fileName);
			bool accdb = extension != null && extension.StartsWith(".accd", StringComparison.OrdinalIgnoreCase);
			return accdb ? AccessFileFormat.Ace : AccessFileFormat.Unknown;
		}


		/// <summary>The Access drivers that are registered for 32 or for 64 bit processes.</summary>
		/// <param name="kind">OLE DB or ODBC.</param>
		/// <param name="is64Bit">Which bitness to look at. It need not be this process's.</param>
		/// <returns>The drivers, or none off Windows and for 64 bit on 32 bit Windows.</returns>
		public static AccessDriver[] GetInstalled(AccessDriverKind kind, bool is64Bit)
		{
			List<AccessDriver> drivers = new List<AccessDriver>();
			if (!IsWindows || (is64Bit && !Is64BitWindows)) {
				return drivers.ToArray();
			}
			if (kind == AccessDriverKind.OleDb) {
				foreach (string progId in OleDbProgIds) {
					// A ProgID is shared by both bitnesses; the CLSID's server is what a process can load.
					string clsid = ReadMachineString("SOFTWARE\\Classes\\" + progId + "\\CLSID", null, is64Bit);
					string dll = clsid == null ? null
						: ReadMachineString("SOFTWARE\\Classes\\CLSID\\" + clsid + "\\InprocServer32", null, is64Bit);
					if (dll != null) {
						bool jet = progId == JetOleDbProgId;
						// Microsoft.ACE.OLEDB.16.0 is engine 16.
						int version = jet ? AccessDriver.JetEngineVersion : int.Parse(progId.Substring(progId.Length - 4, 2));
						AddDriver(drivers, kind, progId, jet, version, is64Bit, dll);
					}
				}
			} else {
				foreach (string name in new string[] { AceOdbcDriverName, JetOdbcDriverName }) {
					string dll = ReadMachineString("SOFTWARE\\ODBC\\ODBCINST.INI\\" + name, "Driver", is64Bit);
					if (dll != null) {
						AddDriver(drivers, kind, name, name == JetOdbcDriverName, NewestEngineVersion, is64Bit, dll);
					}
				}
			}
			return drivers.ToArray();
		}


		/// <summary>Adds a registered driver, unless it is for this process and its DLL is gone.</summary>
		/// <param name="fallbackVersion">The Access Database Engine version to assume when the DLL can't be read.</param>
		private static void AddDriver(List<AccessDriver> drivers, AccessDriverKind kind, string name, bool jet,
			int fallbackVersion, bool is64Bit, string dll)
		{
			dll = dll.Trim('"');
			int engineVersion = jet ? AccessDriver.JetEngineVersion : fallbackVersion;
			if (File.Exists(dll)) {
				if (!jet) {
					// Every Access Database Engine registers Microsoft.ACE.OLEDB.12.0 and the same ODBC
					// driver name, so only the DLL says which engine it is.
					engineVersion = FileVersionInfo.GetVersionInfo(dll).FileMajorPart;
				}
			} else if (is64Bit == Is64BitProcess) {
				// A 32 bit driver in System32 is out of a 64 bit process's sight, so only this bitness is checked.
				return;
			}
			drivers.Add(new AccessDriver(kind, name, engineVersion, is64Bit, dll));
		}


		/// <summary>
		/// Picks the driver to open a database of <paramref name="format"/> with: Jet 4.0 for Access 97,
		/// otherwise the newest Access Database Engine, then Jet 4.0.
		/// </summary>
		/// <returns>The driver, or null when none of <paramref name="drivers"/> reads the format.</returns>
		public static AccessDriver Pick(IEnumerable<AccessDriver> drivers, AccessFileFormat format)
		{
			AccessDriver best = null;
			foreach (AccessDriver driver in drivers) {
				if (!driver.Reads(format)) {
					continue;
				}
				if (best == null) {
					best = driver;
				} else if (format == AccessFileFormat.Jet3) {
					// Jet 4.0 is the engine that is known to open Jet 3 files, and it is always there.
					if (driver.IsJet && !best.IsJet) {
						best = driver;
					}
				} else if (driver.EngineVersion > best.EngineVersion) {
					best = driver;
				}
			}
			return best;
		}


		/// <summary>The driver this process should open <paramref name="fileName"/> with.</summary>
		/// <exception cref="AccessDriverNotFoundException">No driver of this bitness opens it.</exception>
		public static AccessDriver Find(AccessDriverKind kind, string fileName)
		{
			return Find(kind, DetectFormat(fileName));
		}


		/// <summary>The driver this process should use for databases of <paramref name="format"/>.</summary>
		/// <exception cref="AccessDriverNotFoundException">No driver of this bitness opens it.</exception>
		public static AccessDriver Find(AccessDriverKind kind, AccessFileFormat format)
		{
			AccessDriver[] installed = GetInstalled(kind, Is64BitProcess);
			AccessDriver driver = Pick(installed, format);
			if (driver == null) {
				AccessDriver[] otherBitness = GetInstalled(kind, !Is64BitProcess);
				throw new AccessDriverNotFoundException
					(Guidance(kind, format, Is64BitProcess, installed, otherBitness), format);
			}
			return driver;
		}


		/// <summary>What to tell the user when no installed driver of the process's bitness reads a format.</summary>
		/// <param name="kind">OLE DB or ODBC.</param>
		/// <param name="format">The database's format.</param>
		/// <param name="is64BitProcess">The bitness of the process that needs the driver.</param>
		/// <param name="installed">The drivers of that bitness. None of them reads the format.</param>
		/// <param name="otherBitness">The drivers of the other bitness.</param>
		public static string Guidance(AccessDriverKind kind, AccessFileFormat format, bool is64BitProcess,
			IEnumerable<AccessDriver> installed, IEnumerable<AccessDriver> otherBitness)
		{
			int bits = is64BitProcess ? 64 : 32;
			string what;
			switch (format) {
				case AccessFileFormat.Jet3: what = "Access 97 databases"; break;
				case AccessFileFormat.Ace: what = "Access 2007 and later (.accdb) databases"; break;
				default: what = "Access databases"; break;
			}
			StringBuilder message = new StringBuilder();
			message.AppendFormat("This is the {0} bit PlaneDisaster.NET, and no {0} bit {1} on this computer opens {2}.",
				bits, kind == AccessDriverKind.OleDb ? "OLE DB provider" : "ODBC driver", what);

			List<string> names = new List<string>();
			foreach (AccessDriver driver in installed) {
				if (!names.Contains(driver.ToString())) {
					names.Add(driver.ToString());
				}
			}
			if (names.Count > 0) {
				message.AppendFormat(" Installed: {0}.", string.Join(", ", names.ToArray()));
			}

			AccessDriver other = Pick(otherBitness, format);
			if (other != null) {
				message.AppendLine().AppendLine();
				message.AppendFormat("The {0} bit PlaneDisaster.NET can open it with {1}{2}. Get the {3} build from {4}",
					is64BitProcess ? 32 : 64, other.Name, other.IsJet ? ", which ships with Windows" : "",
					is64BitProcess ? "x86" : "x64", ReleasesUrl);
			}
			message.AppendLine().AppendLine();
			if (format != AccessFileFormat.Jet3) {
				message.AppendFormat("{0}nstall the {1} bit Microsoft Access Database Engine 2016 Redistributable from {2}",
					other != null ? "Or i" : "I", bits, RedistributableUrl);
			} else if (is64BitProcess) {
				message.Append("Access 97 files need Jet 4.0, which is 32 bit only. The 64 bit drivers Microsoft still offers, the Access Database Engine 2013 and later, don't open them.");
			} else {
				message.Append("Access 97 files need Jet 4.0, which ships with Windows, or the Access Database Engine 2010 or earlier.");
			}
			return message.ToString();
		}
	}
}
