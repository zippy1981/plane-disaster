/*
 * Created by SharpDevelop.
 * User: justin.dearing
 * Date: 8/13/2007
 * Time: 3:55 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using NUnit.Framework;
using System;
using System.IO;
using PlaneDisaster.Dba;

namespace UnitTests
{
	[TestFixture]
	public abstract class TestDbaBase
	{
		protected struct tblTestRow {
			internal int Id;
			internal string Description;
			
			internal tblTestRow(int id,string description) {
				Id = id;
				Description = description;
			}
		}
			
		protected static readonly string _tempDirectory = Path.GetTempPath();
		protected static readonly string _tempFilePrefix = string.Format("PlaneDisaster-UnitTest-{0}", Guid.NewGuid());
		
		#region SQL Strings
		
		//tblTest SQL
		protected static readonly string _sqlCreateTable = "CREATE TABLE tblTest (id INTEGER PRIMARY KEY, description varchar(255))";
		protected static readonly string _sqlDropTable = "DROP TABLE tblTest";
		protected static readonly string _sqlInsertRow = "INSERT INTO tblTest (id, description) VALUES (@id, @description)";
		//v_test SQL
		//TODO: Create a test where views are created selected, dropped etc.
		protected static readonly string _sqlCreateView = "CREATE VIEW v_test SELECT id, description FROM tblTest";
		protected static readonly string _sqlDropView = "DROP VIEW v_test";
		
		#endregion SQL Strings
		
		//[Test]
		public abstract void TestDbOperations();
		
		[Test]
		public abstract void TestProcedureSupport();
		
		[Test]
		public abstract void TestViewSupport();
		
		protected abstract void CreateDb(string fileName);
		
		protected abstract void PopulateDb(dba dataAccessor);
	}
}
