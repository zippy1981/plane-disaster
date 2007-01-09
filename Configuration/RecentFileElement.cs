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
	/// An xml representation of a recent element.
	/// </summary>
	public sealed class RecentFileElement : ConfigurationElement
	{
		/// <summary>
		/// The full path of the recently opened file.
		/// </summary>
		[ConfigurationProperty("fileName", IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (string)this["fileName"]; }
			set { this["fileName"] = value; }
		}
		
		/*
		/// <summary>
		/// The order in which the files apprear in the recent files collection.
		/// </summary>
		[ConfigurationProperty("order", IsRequired = true)]
		public short Order
		{
			get { return (short)this["order"]; }
			set { this["order"] = value; }
		}
		*/
		
		internal RecentFileElement() : base()
		{}
		
		internal RecentFileElement(string FileName)  : base()
		{
			this.Name = FileName;
		}
	}
	
}

