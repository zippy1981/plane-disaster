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
 * Date: 8/13/2006
 * Time: 6:00 PM
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PlaneDisaster
{
	/// <summary>
	/// Form that can be use to build a connection string to a local 
	/// Postgresql database.
	/// </summary>
	public abstract partial class ConnectionStringDialog
	{
		
		string _Provider;
		
		
		/// <summary>
		/// Returns the connection string.
		/// </summary>
		public abstract string ConnectionString {
			get;
		}
		
		
		/// <summary>
		/// The name of the database.
		/// </summary>
		public string Database {
			get { return txtDatabase.Text; }
		}
		
		
		/// <summary>
		/// The password to connect to the database with.
		/// </summary>
		public string Password {
			get { return txtPassword.Text; }
		}
		
		
		/// <summary>
		/// The name of the OleDB Provider. Set by the constructor.
		/// </summary>
		/// <remarks>
		/// The contract of the abstract class requires its children to 
		/// pass the name of the provider in the constructor.
		/// </remarks>
		protected string Provider {
			get { return this._Provider; }
		}
		
		
		/// <summary>
		/// The User Name to connect to the database as.
		/// </summary>
		protected string User {
			get { return txtUser.Text; }
		}
		
		/// <summary>
		/// Default Constructor.
		/// </summary>
		public ConnectionStringDialog(string Provider)
		{
			_Provider = Provider;
			InitializeComponent();
		}
		
		#region Button Events

		void CmdCancelClick(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		
		void CmdConnectClick(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
		
		#endregion
	}
}
