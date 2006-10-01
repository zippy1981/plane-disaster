/*/
 * Created by SharpDevelop.
 * User: EddingtonAndAssoc
 * Date: 8/13/2006
 * Time: 6:00 PM
/*/
 	
namespace PlaneDisaster
{
	partial class ConnectionStringDialog : System.Windows.Forms.Form
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.txtUser = new System.Windows.Forms.TextBox();
			this.txtPasswd = new System.Windows.Forms.TextBox();
			this.txtDatabase = new System.Windows.Forms.TextBox();
			this.lblDatabase = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.cmdConnect = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txtUser
			// 
			this.txtUser.Location = new System.Drawing.Point(94, 41);
			this.txtUser.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtUser.Name = "txtUser";
			this.txtUser.Size = new System.Drawing.Size(200, 23);
			this.txtUser.TabIndex = 2;
			// 
			// txtPasswd
			// 
			this.txtPasswd.Location = new System.Drawing.Point(94, 68);
			this.txtPasswd.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtPasswd.Name = "txtPasswd";
			this.txtPasswd.Size = new System.Drawing.Size(200, 23);
			this.txtPasswd.TabIndex = 3;
			this.txtPasswd.UseSystemPasswordChar = true;
			// 
			// txtDatabase
			// 
			this.txtDatabase.Location = new System.Drawing.Point(94, 14);
			this.txtDatabase.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.txtDatabase.Name = "txtDatabase";
			this.txtDatabase.Size = new System.Drawing.Size(200, 23);
			this.txtDatabase.TabIndex = 1;
			// 
			// lblDatabase
			// 
			this.lblDatabase.Location = new System.Drawing.Point(13, 14);
			this.lblDatabase.Name = "lblDatabase";
			this.lblDatabase.Size = new System.Drawing.Size(75, 23);
			this.lblDatabase.TabIndex = 0;
			this.lblDatabase.Text = "Database:";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(13, 41);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Username:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(13, 68);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(75, 23);
			this.label2.TabIndex = 0;
			this.label2.Text = "Password:";
			// 
			// cmdConnect
			// 
			this.cmdConnect.Location = new System.Drawing.Point(143, 96);
			this.cmdConnect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.cmdConnect.Name = "cmdConnect";
			this.cmdConnect.Size = new System.Drawing.Size(75, 23);
			this.cmdConnect.TabIndex = 4;
			this.cmdConnect.Text = "C&onnect";
			this.cmdConnect.UseVisualStyleBackColor = true;
			this.cmdConnect.Click += new System.EventHandler(this.CmdConnectClick);
			// 
			// cmdCancel
			// 
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Location = new System.Drawing.Point(219, 95);
			this.cmdCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(75, 23);
			this.cmdCancel.TabIndex = 5;
			this.cmdCancel.Text = "&Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			this.cmdCancel.Click += new System.EventHandler(this.CmdCancelClick);
			// 
			// ConnectionStringDialog
			// 
			this.AcceptButton = this.cmdConnect;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(304, 132);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdConnect);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblDatabase);
			this.Controls.Add(this.txtDatabase);
			this.Controls.Add(this.txtPasswd);
			this.Controls.Add(this.txtUser);
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "ConnectionStringDialog";
			this.Text = "ConnectionStringDialog";
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.TextBox txtUser;
		private System.Windows.Forms.TextBox txtPasswd;
		private System.Windows.Forms.TextBox txtDatabase;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Button cmdConnect;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblDatabase;
	}
}
