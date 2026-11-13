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

namespace PlaneDisaster.Dba
{
	/// <summary>The on-disk format of an Access database, which decides the drivers that can open it.</summary>
	public enum AccessFileFormat
	{
		/// <summary>Not an Access file, or the file could not be read. Any driver may try it.</summary>
		Unknown,
		/// <summary>Access 95 and 97 (Jet 3).</summary>
		Jet3,
		/// <summary>Access 2000 to 2003 (Jet 4), the usual .mdb.</summary>
		Jet4,
		/// <summary>Access 2007 and later (.accdb).</summary>
		Ace
	}


	/// <summary>The two ways PlaneDisaster.NET talks to Access.</summary>
	public enum AccessDriverKind
	{
		/// <summary>An OLE DB provider, named by its ProgID.</summary>
		OleDb,
		/// <summary>An ODBC driver, named as in the ODBC administrator.</summary>
		Odbc
	}


	/// <summary>An Access OLE DB provider or ODBC driver that is installed on this machine.</summary>
	public sealed class AccessDriver
	{
		/// <summary>The engine version of Jet 4.0. The Access Database Engine (ACE) starts at 12.</summary>
		public const int JetEngineVersion = 4;

		/// <summary>The first Access Database Engine that no longer opens Access 97 files: Office 2013.</summary>
		private const int FirstEngineWithoutJet3 = 15;

		private readonly AccessDriverKind _kind;
		private readonly string _name;
		private readonly int _engineVersion;
		private readonly bool _is64Bit;
		private readonly string _fileName;

		/// <summary>Describes a driver.</summary>
		/// <param name="kind">OLE DB or ODBC.</param>
		/// <param name="name">The ProgID or the ODBC driver name.</param>
		/// <param name="engineVersion">4 for Jet 4.0, or the Access Database Engine's major version.</param>
		/// <param name="is64Bit">True when 64 bit processes load it, false for 32 bit ones.</param>
		/// <param name="fileName">The driver's DLL, or null when it is not known.</param>
		public AccessDriver(AccessDriverKind kind, string name, int engineVersion, bool is64Bit, string fileName)
		{
			_kind = kind;
			_name = name;
			_engineVersion = engineVersion;
			_is64Bit = is64Bit;
			_fileName = fileName;
		}

		/// <summary>OLE DB or ODBC.</summary>
		public AccessDriverKind Kind { get { return _kind; } }

		/// <summary>The ProgID for a connection string's Provider, or the ODBC driver name for its Driver.</summary>
		public string Name { get { return _name; } }

		/// <summary>4 for Jet 4.0. Otherwise the Access Database Engine's major version: 12 is 2007, 14 is 2010, 15 is 2013, 16 is 2016 and Microsoft 365.</summary>
		public int EngineVersion { get { return _engineVersion; } }

		/// <summary>True when 64 bit processes load this driver, false for 32 bit ones.</summary>
		public bool Is64Bit { get { return _is64Bit; } }

		/// <summary>The driver's DLL, or null when it is not known.</summary>
		public string FileName { get { return _fileName; } }

		/// <summary>True for Jet 4.0, false for the Access Database Engine.</summary>
		public bool IsJet { get { return _engineVersion == JetEngineVersion; } }

		/// <summary>True when the driver ships inside a Click-to-Run Office, such as Microsoft 365.</summary>
		public bool IsClickToRun {
			get { return _fileName != null && _fileName.IndexOf("\\root\\", StringComparison.OrdinalIgnoreCase) >= 0; }
		}

		/// <summary>
		/// True when this driver opens databases of <paramref name="format"/>. Jet 4.0 doesn't open
		/// .accdb, and the Access Database Engine dropped Access 97 files in Office 2013.
		/// </summary>
		public bool Reads(AccessFileFormat format)
		{
			switch (format) {
				case AccessFileFormat.Jet3:
					return _engineVersion < FirstEngineWithoutJet3;
				case AccessFileFormat.Ace:
					return !IsJet;
				default:
					return true;
			}
		}

		/// <summary>For example "Microsoft.ACE.OLEDB.16.0 (64 bit, Access Database Engine 16)".</summary>
		public override string ToString()
		{
			return string.Format("{0} ({1} bit, {2})", _name, _is64Bit ? 64 : 32,
				IsJet ? "Jet 4.0" : "Access Database Engine " + _engineVersion);
		}
	}
}
