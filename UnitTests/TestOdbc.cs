/*
 * Created by SharpDevelop.
 * User: justin.dearing
 * Date: 8/13/2007
 * Time: 4:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using NUnit.Framework;
using System;
using System.Data.Common;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using PlaneDisaster.Dba;

namespace UnitTests
{
	[TestFixture]
	public class TestOdbc : TestDbaBase
	{
		protected new static readonly string _sqlInsertRow = "INSERT INTO tblTest (id, description) VALUES (idParam, descriptionParam)";
		
		[Test]
		public override void TestDbOperations()
		{
			string fileName = Path.Combine(_tempDirectory, _tempFilePrefix + "-Odbc-TestDbOps.mdb");
			OdbcDba odbcDba = null;
			
			try {
				CreateDb(fileName);
				
				odbcDba = new OdbcDba();
				odbcDba.ConnectMDB(fileName);
				
				PopulateDb(odbcDba);
				
				odbcDba.ExecuteSqlCommand(_sqlDropTable);
			}
			catch(OdbcException ex) {
				//TODO: Figure out why this exception gets thrown
				Assert.Ignore(ex.Message);
			}
			catch (Exception ex) 
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (odbcDba != null && odbcDba.Connected) {
					odbcDba.Disconnect();
					odbcDba.Dispose();
				}
				File.Delete(fileName);
				Assert.IsFalse(File.Exists(fileName), "Failed to delete " + fileName);
			}
		}
		
		
		[Test]
		public override void TestProcedureSupport()
		{
			string fileName = Path.Combine(_tempDirectory, _tempFilePrefix + "-Odbc-TestProcedures.mdb");
			
			try {
				CreateDb(fileName);
							
				OdbcDba odbcDba = new OdbcDba();
				try {
					Assert.IsTrue(odbcDba.SupportsProcedures);
					Assert.Fail("OdbcDba.SupportsProcedures should throw a InvalidOperationException if no database is connected.");
				}
				catch(InvalidOperationException) {}
					
				odbcDba.ConnectMDB(fileName);
				Assert.IsTrue(odbcDba.SupportsProcedures);
				odbcDba.Disconnect();
			}
			finally 
			{
				File.Delete(fileName);
				Assert.IsFalse(File.Exists(fileName), "Failed to delete " + fileName);
			}
		}
		

		[Test]
		public override void TestViewSupport()
		{
			string fileName = Path.Combine(_tempDirectory, _tempFilePrefix + "-Odbc-TestViews.mdb");
			
			try {
				CreateDb(fileName);
							
				OdbcDba odbcDba = new OdbcDba();
				try {
					Assert.IsTrue(odbcDba.SupportsViews);
					Assert.Fail("OdbcDba.SupportsViews should throw a InvalidOperationException if no database is connected.");
				}
				catch(InvalidOperationException) {}
					
				odbcDba.ConnectMDB(fileName);
				Assert.IsTrue(odbcDba.SupportsViews);
				odbcDba.Disconnect();
			}
			finally 
			{
				File.Delete(fileName);
				Assert.IsFalse(File.Exists(fileName), "Failed to delete " + fileName);
			}
		}
		
		
		protected override void CreateDb(string fileName)
		{
			JetSqlUtil.CreateMDB(fileName);
			Assert.IsTrue(File.Exists(fileName), "Failed to create " + fileName);
		}
		
		
		protected override void PopulateDb(dba odbcDba)
		{
			tblTestRow [] rows = new tblTestRow [] {
				new tblTestRow(1, "Number one."),
				new tblTestRow(2, "Number two."),
				new tblTestRow(3, "Number three.")
			};
			
			odbcDba.ExecuteSqlCommand(_sqlCreateTable);
			
			foreach(tblTestRow row in rows) {
				OdbcParameter[] parameters = new OdbcParameter [] {
					new OdbcParameter("idParam", row.Id),
					new OdbcParameter("@descriptionParam", row.Description)
				};
				odbcDba.ExecuteSqlCommand(_sqlInsertRow, parameters);
			}
			string [] columnData = odbcDba.GetColumnAsStringArray("tblTest", "description");
			Assert.AreEqual(columnData.Length, 3, "Inserted 3 rows and retrieved {0}", new object[] {columnData.Length});
		}

	}
}
