/*/ 
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
/*/

/*/
 * Created by SharpDevelop.
 * Author:		Justin Dearing <zippy1981@gmail.com>
 * Date: 8/13/2006
 * Time: 10:21 PM
/*/

using System;

namespace PlaneDisaster
{
	/// <summary>
	/// Description of MySQLConnStrDialog.
	/// </summary>
	public class MySQLConnStrDialog : ConnectionStringDialog, IConnectionStringDialog
	{
		/// <summary>
		/// Returns a properly formatted MYOLEDB Connection String
		/// </summary>
		public override string ConnectionString {
			get {
				return String.Format
					("Provider={0};Data Source={1};User Id={2};Password={3}", 
					Provider, Database, User, Password);
			}
		}
		
		/// <summary>
		/// Default constructor.
		/// </summary>
		public MySQLConnStrDialog() :base ("MySQLProv") { }
	}
}
