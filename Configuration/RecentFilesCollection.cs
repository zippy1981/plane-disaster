/*
 * Created by SharpDevelop.
 * Author:		Justin Dearing <zippy1981@gmail.com>
 * Date: 1/9/2007
 * Time: 12:48 AM
 */

using System;
using System.Configuration;
using System.IO;


namespace PlaneDisaster.Configuration
{
	/// <summary>
	/// ConfigurationSection with PlaneDisaster settings.
	/// </summary>
	public sealed class RecentFilesCollection : ConfigurationElementCollection
	{
		#region Properties

		/// <summary>
		/// Gets the CollectionType of the ConfigurationElementCollection.
		/// </summary>
		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.BasicMap; }
		}
	   

		/// <summary>
		/// Gets the Name of Elements of the collection.
		/// </summary>
		protected override string ElementName
		{
		get { return "PlaneDisaster"; }
		}
			   
	   
		/// <summary>
		/// Retrieve and item in the collection by index.
		/// </summary>
		public RecentFileElement this[int index]
		{
			get   { return (RecentFileElement)BaseGet(index); }
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}
		
		
		/// <summary>
		/// The maximum number of RecentFileElements to store
		/// </summary>
		[ConfigurationProperty("maxCount", IsKey=true, DefaultValue=(short)5)]
		public short MaxCount {
			get { return (short)this["maxCount"]; }
			set { this["maxCount"] = value; }
		}


		#endregion

		/// <summary>
		/// Adds a PlaneDisasterElement to the configuration file.
		/// </summary>
		public void Add(RecentFileElement element)
		{
			BaseAdd(element);
		}
		
		
		/// <summary>
		/// Adds a PlaneDisasterElement to the configuration file.
		/// </summary>
		public void Add(string FileName)
		{
			Add (new RecentFileElement(Path.GetFullPath(FileName)));
		}
		
		/// <summary>
		/// Adds a group of to the configuration file.
		/// </summary>
		public void AddRange(string [] Files) {
			foreach (string File in Files) {
				RecentFileElement RecentFile = 
					new RecentFileElement(File);
				this.Add(RecentFile);
			}
		}
		
		
		/// <summary>
		/// Clears all PlaneDisasterElements to the collection.
		/// </summary>
		public void Clear()
		{
			BaseClear();
		}
	   
	   
		/// <summary>
		/// Creates a new PlaneDisasterElement.
		/// </summary>
		/// <returns>A new <c>PlaneDisasterElement</c></returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new RecentFileElement();
		}
			   
	   
		/// <summary>
		/// Checks for the existance of a given file in the
		/// <c>RecentFilesCollection</c>.
		/// </summary>
		/// <param name="FileName">The name of the file.</param>
		/// <returns>True if the file exists. False otherwise.</returns>
		public  bool FileExists(string FileName)
		{
			FileName = Path.GetFullPath(FileName);
			foreach (RecentFileElement File in this) {
				if (File.Name == FileName) return true;
			} return false;
			
		}
		
		
		/// <summary>
		/// Gets the key of an element based on it's Id.
		/// </summary>
		/// <param name="element">Element to get the key of.</param>
		/// <returns>The key of <c>element</c>.</returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((RecentFileElement)element).Name;
		}
	   
	   
		/// <summary>
		/// Removes a PlaneDisasterElement with the given name.
		/// </summary>
		/// <param name="name">The name of the PlaneDisasterElement to remove.</param>
		public void Remove (string name) {
			base.BaseRemove(name);
		}

	}
}

