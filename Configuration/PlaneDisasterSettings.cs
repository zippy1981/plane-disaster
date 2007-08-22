/*
 * Copyright 2006-2007 Justin Dearing
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
 * Date: 4/30/2007
 * Time: 4:47 AM
 */

using System;   
using System.Configuration;

namespace PlaneDisaster.Configuration
{
	/// <summary>
	/// Configuration section &lt;PlaneDisaster&gt;
	/// </summary>
	/// <remarks>
	/// Assign properties to your child class that has the attribute 
	/// <c>[ConfigurationProperty]</c> to store said properties in the xml.
	/// </remarks>
	public sealed class PlaneDisasterSettings : ConfigurationSection
	{
		System.Configuration.Configuration _Config;


		#region Configuration Properties
		
		/*
		 *  Uncomment the following section and add a Configuration Collection 
		 *  from the with the file named PlaneDisaster.cs
		 */
		// /// <summary>
		// /// A custom XML section for an application's configuration file.
		// /// </summary>
		// [ConfigurationProperty("customSection", IsDefaultCollection = true)]
		// public PlaneDisasterCollection PlaneDisaster
		// {
		// 	get { return (PlaneDisasterCollection) base["customSection"]; }
		// }

		/// <summary>
		/// Represents attribute <c>exampleAttribute</c> of &lt;PlaneDisaster&gt;
		/// </summary>
		[ConfigurationProperty("exampleAttribute", DefaultValue="exampleValue")]
		public string ExampleAttribute {
			get { return (string) this["exampleAttribute"]; }
			set { this["exampleAttribute"] = value; }
		}
		
		
		/// <summary>
		/// A collection that stores a list of the 
		/// <c>this["recentFileCount"]</c> most recently opened files.
		/// </summary>
		[ConfigurationProperty("recentFiles", IsDefaultCollection = true)]
		public RecentFilesCollection RecentFiles
		{
			get { return (RecentFilesCollection) base["recentFiles"]; }
		}

		#endregion

		/// <summary>
		/// Private Constructor used by our factory method.
		/// </summary>
		private PlaneDisasterSettings () : base () {
			// Allow this section to be stored in user.app. By default this is forbidden.
			this.SectionInformation.AllowExeDefinition =
				ConfigurationAllowExeDefinition.MachineToLocalUser;
		}

		#region Public Methods
		
		/// <summary>
		/// Saves the configuration to the config file.
		/// </summary>
		public void Save() {
			_Config.Save();
		}
		
		#endregion
		
		#region Static Members
		
		/// <summary>
		/// Gets the current applications &lt;PlaneDisaster&gt; section.
		/// </summary>
		/// <param name="ConfigLevel">
		/// The &lt;ConfigurationUserLevel&gt; that the config file
		/// is retrieved from.
		/// </param>
		/// <returns>
		/// The configuration file's &lt;PlaneDisaster&gt; section.
		/// </returns>
		public static PlaneDisasterSettings GetSection (ConfigurationUserLevel ConfigLevel) {
			/* 
			 * This class is setup using a factory pattern that forces you to
			 * name the section &lt;PlaneDisaster&gt; in the config file.
			 * If you would prefer to be able to specify the name of the section,
			 * then remove this method and mark the constructor public.
			 */ 
			System.Configuration.Configuration Config = ConfigurationManager.OpenExeConfiguration
				(ConfigLevel);
			PlaneDisasterSettings oPlaneDisasterSettings;
			
			oPlaneDisasterSettings =
				(PlaneDisasterSettings)Config.GetSection("PlaneDisasterSettings");
			if (oPlaneDisasterSettings == null) {
				oPlaneDisasterSettings = new PlaneDisasterSettings();
				Config.Sections.Add("PlaneDisasterSettings", oPlaneDisasterSettings);
			}
			oPlaneDisasterSettings._Config = Config;
			
			return oPlaneDisasterSettings;
		}
		
		#endregion
	}
}

