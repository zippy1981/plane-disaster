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
 * Created:		08/01/2006
 * Description  This is the presentation code for PlaneDisaster.NET
 */

 
using System;
using System.IO;
using System.Windows.Forms;

namespace PlaneDisaster
{
	/// <summary>
	/// PlaneDisaster.NET main class.
	/// </summary>
	public sealed class PlaneDisaster
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		[STAThread]
		public static void Main(string[] args)
		{
			MainForm frm = new MainForm();
			/*
			 * From the infalliable tomes of the msdn http://msdn2.microsoft.com/en-us/library/acy3edy3.aspx
			 *    Unlike C and C++, the name of the program is not treated as the first command line argument.
			 *    WHY WHY WHY!!!!!! Lets just have 1 based arrays.
			 */
			if (args.Length > 0) {
				string FileName = args[0];
				if (File.Exists(FileName)) {
					frm.OpenDatabaseFile(FileName);
					frm.InitContextMenues();
				} else if (Directory.Exists(Path.GetDirectoryName(FileName))) {
					frm.NewDatabaseFile(FileName);
					frm.InitContextMenues();
				} else {
					MessageBox.Show(String.Format("File {0} is not a real file.", FileName));
				}
			}
			Application.Run(frm);
		}

	}
}
