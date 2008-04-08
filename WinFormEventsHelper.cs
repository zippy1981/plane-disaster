/*
 * Copyright 2006-2008 Justin Dearing
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
 * Date: 3/8/2008
 * Time: 10:04 PM
 */

using System;
using System.Windows.Forms;

namespace PlaneDisaster
{
	/// <summary>
	/// These are generic event handlers for windows events.
	/// </summary>
	/// <remarks>
	/// This class should eventually be moved to a seperate assembly.
	/// </remarks>
	/// <seealso cref="ListBox"/>
	public static class WinFormEventsHelper
	{
		/// <summary>
		/// Add this to a listboxes Click event too allow right clicks to change the highlighted item.
		/// </summary>
		/// <remarks>
		/// If you want to be able to select an item in your ListBox via right click,
		/// add this function as an event handler to the listbox's mousedown event.
		/// </remarks>
		/// <param name="sender">The listbox being right clicked.</param>
		/// <param name="e">The MouseEventArgs object.</param>
		/// <seealso cref="ListBox" />
		public static void ListBoxRightClickSelect(object sender, MouseEventArgs  e) {
			if (e.Button == MouseButtons.Right) {
				ListBox lst = sender as ListBox;
				if (lst == null) {
					string typeName = sender.GetType().Name;
					string msg = string.Format
						("Cannot use the ListBoxRightClickSelect() event handler on objects of type (0)", typeName);
					throw new InvalidCastException(msg);
				}
				int Index = lst.IndexFromPoint(e.X, e.Y);
				
				if (Index >= 0 && Index < lst.Items.Count) {
					lst.SelectedIndex = Index;
				}
				lst.Refresh();
			}
		}
		
		
		/// <summary>
		/// Add this to a TreeViews Click event too allow right clicks to change the highlighted item.
		/// </summary>
		/// <remarks>
		/// <para>If you want to be able to select an item in your TreeView via right click,
		/// add this function as an event handler to the TreeView's mousedown event.</para>
		/// <para>Source: http://www.syncfusion.com/FAQ/windowsforms/faq_c91c.aspx#q807q.</para>
		/// </remarks>
		/// <param name="sender">The TreeView being right clicked.</param>
		/// <param name="e">The MouseEventArgs object.</param>
		/// <seealso cref="TreeView" />
		public static void TreeViewRightClickSelect(object sender, MouseEventArgs  e) {
			if (e.Button == MouseButtons.Right) {
				TreeView node = (TreeView) sender;
				node.SelectedNode = node.GetNodeAt (e.X ,e.Y );
			}
		}		
	}
}
