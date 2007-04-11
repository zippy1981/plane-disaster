/*
 * Copyright 2006 Justin Dearing
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

/*
 * Created by SharpDevelop.
 * Author:		Justin Dearing <zippy1981@gmail.com>
 * Date: 9/5/2006
 * Time: 12:25 PM
 */

using Heyes;
using PlaneDisaster;
using System;
using System.Collections.Generic;
using System.IO;

using PlaneDisaster.Dba;

namespace jsql_admin
{
	class jsql_admin
	{
		public static void Main(string[] args)
		{
			GetOpt oGetOpt = new GetOpt(args);
			string MdbFile = "";
			
			try {
            	oGetOpt.SetOpts(new string[] {"c", "d=", "s="});
            	oGetOpt.Parse();
            	//DEBUG: Console.WriteLine("Successfully parsed arguments.");
            } catch (ArgumentException) {
				Console.Error.WriteLine("ERROR: arguments not supplied");
            	//TODO: Write usage info function
            	oGetOpt.Args.ToString();
            	Console.WriteLine();
            	System.Environment.Exit(666);
            }

			if (!oGetOpt.IsDefined("d")) {
					Console.Error.WriteLine("Must specify the database.");
					System.Environment.Exit(666);
			} else {
				MdbFile = oGetOpt.GetOptionArg("d");
			}
			
			Console.WriteLine("Database: {0}", MdbFile);
			try {
				if (oGetOpt.HasArgument("s")) {
					Console.WriteLine("SQL Script: {0}", oGetOpt.GetOptionArg("s"));
				}
			} catch (ArgumentNullException) { }
			
			if (oGetOpt.IsDefined("c")) {
				if (File.Exists(MdbFile)) {
					Console.Error.WriteLine("JetSQL file \"{0}\" already exists!", MdbFile);
				} else {
					JetSqlUtil.CreateMDB(MdbFile);
				}
			}
			
			//If the Access file doesn't exist at this point we can't go on
			if (!File.Exists(MdbFile)) {
				Console.Error.WriteLine("JetSQL file \"{0}\" does not exist!", MdbFile);
			}
			
			if (oGetOpt.IsDefined("s")) {
			    OdbcDba dbconn = new OdbcDba();
			    dbconn.ConnectMDB(MdbFile);
			    dbconn.ExecuteSqlFile(oGetOpt.GetOptionArg("s"));
			    dbconn.Disconnect();
			}
		}
	}
}
