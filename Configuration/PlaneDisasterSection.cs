/*
 * Created by SharpDevelop.
 * Author:		Justin Dearing <zippy1981@gmail.com>
 * Date: 1/9/2007
 * Time: 12:48 AM
 */

using System;
using System.Configuration;

namespace PlaneDisaster.Configuration
{
	/// <summary>
	/// Configuration settings for PlaneDisaster.
	/// </summary>
	public sealed class PlaneDisasterSection : ConfigurationSection
	{
		/// <summary>
		/// A collection that stores a list of the 
		/// <c>this["recentFileCount"]</c> most recently opened files.
		/// </summary>
		[ConfigurationProperty("recentFiles", IsDefaultCollection = true)]
		public RecentFilesCollection RecentFiles
		{
			get { return (RecentFilesCollection) base["recentFiles"]; }
		}
		
		public PlaneDisasterSection () : base () {
			this.SectionInformation.AllowExeDefinition =
				ConfigurationAllowExeDefinition.MachineToLocalUser;
		}
	}
}

