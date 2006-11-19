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
		
		
		private MainForm GetMainForm(object oControl) {
			return (MainForm)((Control)oControl).FindForm();
		}
		
		
		#region Events
		
		#region Button Events
		
		internal void CmdSaveCsvClick(object sender, System.EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			string FileName;
			dlg.Filter = "Comma Seperated Value (*.csv)|*.csv|All Files|";
			
			if(dlg.ShowDialog() == DialogResult.OK ) {
				FileName = dlg.FileName;
				using (StreamWriter sw = File.CreateText(FileName))
				{
					sw.Write(GetMainForm(sender).CSV);
	            }  
			}
		}
		
		internal void CmdStatusClick(object sender, System.EventArgs e)
		{
			MessageBox.Show(GetMainForm(sender).GetDatabaseStatus());
		}
		
		#endregion
				
		#region DataGridView Events
		
		internal void EvtDataGridError(object sender, DataGridViewDataErrorEventArgs e) {
			if ((e.Context & DataGridViewDataErrorContexts.Display) == DataGridViewDataErrorContexts.Display) {
				//Its ok its just not a picture
			} else { e.ThrowException = true;}
		}
		
		#endregion
		
		internal void CmdSQLClick(object sender, System.EventArgs e)
		{
			GetMainForm(sender).LoadQueryResults();
		}
		
		#region ListBox Events
	
		internal void lst_DblClick(object sender, System.EventArgs e) {
			ListBox lst = (ListBox) sender;
			if (lst.Name == "lstColumns") {
				//I dont know what the default action for the columm list should be
			} else {
				GetMainForm(sender).LoadTableResults(lst.Text);
			}
		}
		
		
		/// <summary>
		/// If you want to be able to select an item in your listbox via right click,
		/// add this function as an event handler to the listbox's mousedown event.
		/// </summary>
		/// <param name="sender">The listbox being right clicked.</param>
		/// <param name="e">The MouseEventArgs object.</param>
		internal void ListBox_RightClickSelect(object sender, MouseEventArgs  e) {
			if (e.Button == MouseButtons.Right) {
				ListBox lst = (ListBox) sender;
				int Index = lst.IndexFromPoint(e.X, e.Y);
			
				if (Index >= 0 && Index < lst.Items.Count) {
            	    lst.SelectedIndex = Index;
            	}
            	lst.Refresh();
			}
		}
		
		#endregion
		
		#endregion
	}
}
