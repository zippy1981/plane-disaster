/*
 * Created by SharpDevelop.
 * User: EddingtonAndAssoc
 * Date: 11/18/2006
 * Time: 2:47 AM
 */

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PlaneDisaster
{
	/// <summary>
	/// Description of MainFormEvents.
	/// </summary>
	internal class MainFormEvents
	{
		internal MainFormEvents() :base() {}
		
		
		
		
		internal void CmdSQLClick(object sender, System.EventArgs e)
		{
			((MainForm)((Control)sender).FindForm()).LoadQueryResults();
		}
	}
}
