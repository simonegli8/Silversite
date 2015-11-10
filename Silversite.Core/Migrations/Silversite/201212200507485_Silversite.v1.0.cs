namespace Silversite.Migrations.Silversite {
	using System;
	using System.Data.Entity.Migrations;

	public partial class Silversitev10 : DbMigration {
		public override void Up() {
			CreateTable(
				 "dbo.Silversite_Services_Person",
				 c => new {
					 Key = c.Int(nullable: false, identity: true),
					 Title = c.String(maxLength: 64),
					 FirstName = c.String(maxLength: 128),
					 LastName = c.String(maxLength: 128),
					 Company = c.String(maxLength: 128),
					 Address = c.String(maxLength: 128),
					 Zip = c.String(maxLength: 32),
					 City = c.String(maxLength: 128),
					 Country = c.String(maxLength: 64),
					 State = c.String(maxLength: 64),
					 TimeZone = c.String(maxLength: 32),
					 Phone = c.String(maxLength: 128),
					 Email = c.String(maxLength: 128),
					 CultureName = c.String(maxLength: 8),
					 UserName = c.String(maxLength: 128),
					 RegistrationDate = c.DateTime(nullable: false),
					 DiskSpaceUsed = c.Long(nullable: false),
					 Providers = c.String(),
					 CustomData = c.Binary(),
					 Discriminator = c.String(nullable: false, maxLength: 128),
				 })
				.PrimaryKey(t => t.Key)
				.Index(t => t.UserName, unique: true)
				.Index(t => t.Email);

			CreateTable(
				 "dbo.Silversite_Services_Document",
				 c => new {
					 Key = c.Int(nullable: false, identity: true),
					 ContentKey = c.Int(nullable: false),
					 Path = c.String(maxLength: 512),
					 Domain = c.String(maxLength: 255),
					 Title = c.String(maxLength: 255),
					 Notes = c.String(),
					 Tags = c.String(),
					 Revision = c.Int(nullable: false),
					 IsCurrentRevision = c.Boolean(nullable: false),
					 IsPreviewRevision = c.Boolean(nullable: false),
					 Categories = c.String(),
					 Published = c.DateTime(nullable: false),
					 Text = c.String(),
					 Author_Key = c.Int(),
				 })
				.PrimaryKey(t => t.Key)
				.ForeignKey("dbo.Silversite_Services_Person", t => t.Author_Key, cascadeDelete: false)
				.Index(t => t.Author_Key)
				.Index(t => new { t.ContentKey, t.IsCurrentRevision });

			CreateTable(
				 "dbo.CompanyCategories",
				 c => new {
					 Key = c.Int(nullable: false, identity: true),
					 Name = c.String(maxLength: 128),
					 WebAddress_Key = c.Int(),
				 })
				.PrimaryKey(t => t.Key)
				.ForeignKey("dbo.Silversite_Services_Person", t => t.WebAddress_Key)
				.Index(t => t.WebAddress_Key);

			CreateTable(
				 "dbo.Silversite_Services_EditRights",
				 c => new {
					 UserOrRole = c.String(nullable: false, maxLength: 128),
					 IsUser = c.Boolean(nullable: false),
					 DocumentCategories = c.String(),
				 })
				 .PrimaryKey(t => t.UserOrRole);

			CreateTable(
				 "dbo.Silversite_Web_Providers_User",
				 c => new {
					 UserId = c.Guid(nullable: false),
					 UserName = c.String(nullable: false, maxLength: 128),
					 Email = c.String(maxLength: 128),
					 ClearPassword = c.String(maxLength: 128),
					 HashedPassword = c.String(maxLength: 128),
					 EncryptedPassword = c.Binary(maxLength: 255),
					 IsConfirmed = c.Boolean(nullable: false),
					 PasswordFailuresSinceLastSuccess = c.Int(nullable: false),
					 AnswerFailuresSinceLastSuccess = c.Int(nullable: false),
					 LastPasswordFailureDate = c.DateTime(),
					 LastAnswerFailureDate = c.DateTime(),
					 ConfirmationToken = c.String(maxLength: 64),
					 CreationDate = c.DateTime(nullable: false),
					 PasswordChangedDate = c.DateTime(nullable: false),
					 PasswordVerificationToken = c.String(maxLength: 64),
					 PasswordVerificationTokenExpirationDate = c.DateTime(),
					 LastLockoutDate = c.DateTime(),
					 LastLoginDate = c.DateTime(),
					 LastActivityDate = c.DateTime(nullable: false),
					 IsApproved = c.Boolean(nullable: false),
					 IsDeleted = c.Boolean(nullable: false),
					 Comment = c.String(maxLength: 255),
					 PasswordQuestion = c.String(maxLength: 255),
					 PasswordAnswer = c.String(maxLength: 255),
					 IsLockedOut = c.Boolean(nullable: false),
					 Application_Key = c.Int(nullable: false),
				 })
				.PrimaryKey(t => t.UserId)
				.ForeignKey("dbo.Silversite_Web_Providers_Application", t => t.Application_Key, cascadeDelete: true)
				.Index(t => t.Application_Key)
				.Index(t => t.UserName)
				.Index(t => t.Email);

			CreateTable(
				 "dbo.Silversite_Web_Providers_Application",
				 c => new {
					 Key = c.Int(nullable: false, identity: true),
					 Name = c.String(nullable: false, maxLength: 255),
				 })
				 .PrimaryKey(t => t.Key)
				.Index(t => new { t.Name });

			CreateTable(
				 "dbo.Silversite_Web_Providers_Role",
				 c => new {
					 RoleId = c.Guid(nullable: false),
					 ApplicationKey = c.Int(nullable: false),
					 RoleName = c.String(nullable: false, maxLength: 128),
					 Description = c.String(maxLength: 255),
				 })
					.PrimaryKey(t => t.RoleId)
					.ForeignKey("dbo.Silversite_Web_Providers_Application", t => t.ApplicationKey, cascadeDelete: true)
			.Index(t => t.ApplicationKey)
			.Index(t => t.RoleName);

			CreateTable(
				 "dbo.Silversite_Services_LogMessage",
				 c => new {
					 Key = c.Long(nullable: false, identity: true),
					 Date = c.DateTime(nullable: false),
					 Text = c.String(),
					 Category = c.String(maxLength: 128),
					 ExceptionData = c.Binary(),
					 CustomData = c.Binary(),
					 SourceText = c.String(),
					 SourceFile = c.String(maxLength: 512),
					 SourceLine = c.Int(nullable: false),
				 })
			.PrimaryKey(t => t.Key)
			.Index(t => t.Date)
			.Index(t => t.Category);

			CreateTable(
				 "dbo.Silversite_Data_SentMail",
				 c => new {
					 Key = c.Guid(nullable: false),
					 Sent = c.DateTime(nullable: false),
					 Priority = c.Int(nullable: false),
					 To = c.String(),
					 From = c.String(maxLength: 128),
					 CC = c.String(),
					 Subject = c.String(maxLength: 128),
					 PlainText_Key = c.Int(),
					 HtmlText_Key = c.Int(),
					 Personal_Key = c.Int(),
					 SmtpServer_Key = c.Int(),
				 })
					.PrimaryKey(t => t.Key)
					.ForeignKey("dbo.Silversite_Data_MailText", t => t.PlainText_Key)
					.ForeignKey("dbo.Silversite_Data_MailText", t => t.HtmlText_Key)
					.ForeignKey("dbo.Silversite_Services_Person", t => t.Personal_Key)
					.ForeignKey("dbo.Silversite_Data_SmtpServer", t => t.SmtpServer_Key)
					.Index(t => t.PlainText_Key)
					.Index(t => t.HtmlText_Key)
					.Index(t => t.Personal_Key)
					.Index(t => t.SmtpServer_Key)
					.Index(t => t.Sent);

			CreateTable(
				 "dbo.Silversite_Data_MailText",
				 c => new {
					 Key = c.Int(nullable: false, identity: true),
					 Hash = c.Int(nullable: false),
					 Text = c.String(nullable: false),
				 })
			.PrimaryKey(t => t.Key)
			.Index(t => t.Hash);

			CreateTable(
				 "dbo.Silversite_Data_MailResource",
				 c => new {
					 Key = c.Guid(nullable: false),
					 Hash = c.Int(nullable: false),
					 SentMail_Key = c.Guid(),
					 ScheduledMail_Key = c.Guid(),
				 })
					.PrimaryKey(t => t.Key)
					.ForeignKey("dbo.Silversite_Data_SentMail", t => t.SentMail_Key)
					.ForeignKey("dbo.Silversite_Data_ScheduledMail", t => t.ScheduledMail_Key)
					.Index(t => t.SentMail_Key)
					.Index(t => t.ScheduledMail_Key)
				.Index(t => t.Hash);

			CreateTable(
				 "dbo.Silversite_Data_MailSubstitution",
				 c => new {
					 Key = c.Int(nullable: false, identity: true),
					 Hash = c.Int(nullable: false),
					 Token = c.String(nullable: false),
					 Text = c.String(nullable: false),
					 SentMail_Key = c.Guid(),
					 ScheduledMail_Key = c.Guid(),
				 })
					.PrimaryKey(t => t.Key)
					.ForeignKey("dbo.Silversite_Data_SentMail", t => t.SentMail_Key)
					.ForeignKey("dbo.Silversite_Data_ScheduledMail", t => t.ScheduledMail_Key)
					.Index(t => t.SentMail_Key)
					.Index(t => t.ScheduledMail_Key)
				.Index(t => t.Hash);

			CreateTable(
				 "dbo.Silversite_Data_SmtpServer",
				 c => new {
					 Key = c.Int(nullable: false, identity: true),
					 Hash = c.Int(nullable: false),
					 Host = c.String(nullable: false, maxLength: 128),
					 Port = c.Int(nullable: false),
					 User = c.String(nullable: false, maxLength: 128),
					 Password = c.String(nullable: false, maxLength: 128),
				 })
			.PrimaryKey(t => t.Key)
			.Index(t => t.Hash);

			CreateTable(
				 "dbo.Silversite_Data_ScheduledMail",
				 c => new {
					 Key = c.Guid(nullable: false),
					 Sent = c.DateTime(nullable: false),
					 Priority = c.Int(nullable: false),
					 To = c.String(),
					 From = c.String(maxLength: 128),
					 CC = c.String(),
					 Subject = c.String(maxLength: 128),
					 PlainText_Key = c.Int(),
					 HtmlText_Key = c.Int(),
					 Personal_Key = c.Int(),
					 SmtpServer_Key = c.Int(),
				 })
					.PrimaryKey(t => t.Key)
					.ForeignKey("dbo.Silversite_Data_MailText", t => t.PlainText_Key)
					.ForeignKey("dbo.Silversite_Data_MailText", t => t.HtmlText_Key)
					.ForeignKey("dbo.Silversite_Services_Person", t => t.Personal_Key)
					.ForeignKey("dbo.Silversite_Data_SmtpServer", t => t.SmtpServer_Key)
					.Index(t => t.PlainText_Key)
					.Index(t => t.HtmlText_Key)
					.Index(t => t.Personal_Key)
			.Index(t => t.SmtpServer_Key)
			.Index(t => t.Priority)
			.Index(t => t.Sent);

			CreateTable(
				 "dbo.Silversite_Services_UptimeRecord",
				 c => new {
					 Key = c.Int(nullable: false, identity: true),
					 Time = c.DateTime(nullable: false),
					 Running = c.Boolean(nullable: false),
					 Test = c.Boolean(nullable: false),
					 Process = c.Int(nullable: false),
					 AppDomain = c.Int(nullable: false),
				 })
			.PrimaryKey(t => t.Key)
			.Index(t => t.Time);

			CreateTable(
				 "dbo.Silversite_Services_StopwatchRecord",
				 c => new {
					 Id = c.Int(nullable: false, identity: true),
					 Page = c.String(maxLength: 512),
					 Name = c.String(maxLength: 128),
					 Date = c.DateTime(nullable: false),
					 MeanTicks = c.Double(nullable: false),
					 N = c.Int(nullable: false),
					 Test = c.Boolean(nullable: false),
					 Time_Value = c.Long(nullable: false),
					 Max_Value = c.Long(nullable: false),
					 Min_Value = c.Long(nullable: false),
					 Start = c.DateTime(nullable: false),
					 ParentPage_Id = c.Int(),
				 })
					.PrimaryKey(t => t.Id)
					.ForeignKey("dbo.Silversite_Services_StopwatchRecord", t => t.ParentPage_Id)
			.Index(t => t.ParentPage_Id)
			.Index(t => t.Date)
			.Index(t => t.Page)
			.Index(t => t.Name);

			CreateTable(
				 "dbo.Silversite_Data_Language",
				 c => new {
					 Culture = c.String(nullable: false, maxLength: 16),
				 })
				 .PrimaryKey(t => t.Culture);

			CreateTable(
				 "dbo.Silversite_Data_Currency",
				 c => new {
					 Name = c.String(nullable: false, maxLength: 16),
					 DisplayName = c.String(maxLength: 128),
					 Symbol = c.String(maxLength: 16),
					 MinimalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
					 RoundTo = c.Decimal(nullable: false, precision: 18, scale: 2),
					 Rate = c.Double(nullable: false),
					 CurrentRate = c.Double(nullable: false),
					 IsBaseCurrency = c.Boolean(nullable: false),
				 })
				 .PrimaryKey(t => t.Name);

			CreateTable(
				 "dbo.Silversite_Web_Providers_UserRoles",
				 c => new {
					 Role = c.Guid(nullable: false),
					 User = c.Guid(nullable: false),
				 })
					.PrimaryKey(t => new { t.Role, t.User })
					.ForeignKey("dbo.Silversite_Web_Providers_User", t => t.Role, cascadeDelete: false)
					.ForeignKey("dbo.Silversite_Web_Providers_Role", t => t.User, cascadeDelete: false)
					.Index(t => t.Role)
					.Index(t => t.User);

		}

		public override void Down() {
			DropIndex("dbo.Silversite_Services_StopwatchRecord", new[] { "Date" });
			DropIndex("dbo.Silversite_Services_StopwatchRecord", new[] { "Page" });
			DropIndex("dbo.Silversite_Services_StopwatchRecord", new[] { "Name" });
			DropIndex("dbo.Silversite_Services_UptimeRecord", new[] { "Time" });
			DropIndex("dbo.Silversite_Data_ScheduledMail", new[] { "Priority" });
			DropIndex("dbo.Silversite_Data_ScheduledMail", new[] { "Sent" });
			DropIndex("dbo.Silversite_Data_SmtpServer", new[] { "Hash" });
			DropIndex("dbo.Silversite_Data_MailSubstitution", new[] { "Hash" });
			DropIndex("dbo.Silversite_Data_MailResource", new[] { "Hash" });
			DropIndex("dbo.Silversite_Data_MailText", new[] { "Hash" });
			DropIndex("dbo.Silversite_Data_SentMail", new[] { "Sent" });
			DropIndex("dbo.Silversite_Services_LogMessage", new[] { "Category" });
			DropIndex("dbo.Silversite_Services_LogMessage", new[] { "Date" });
			DropIndex("dbo.Silversite_Web_Providers_Role", new[] { "RoleName" });
			DropIndex("dbo.Silversite_Web_Providers_Application", new[] { "Name" });
			DropIndex("dbo.Silversite_Web_Providers_User", new[] { "UserName" });
			DropIndex("dbo.Silversite_Web_PRoviders_User", new[] { "Email" });
			DropIndex("dbo.Silversite_Services_Document", new[] { "ContentKey", "IsCurrentRevision" });
			DropIndex("dbo.Silversite_Services_Person", new[] { "UserName" });
			DropIndex("dbo.Silversite_Services_Person", new[] { "Email" });

			DropIndex("dbo.Silversite_Web_Providers_UserRoles", new[] { "User" });
			DropIndex("dbo.Silversite_Web_Providers_UserRoles", new[] { "Role" });
			DropIndex("dbo.Silversite_Services_StopwatchRecord", new[] { "ParentPage_Id" });
			DropIndex("dbo.Silversite_Data_ScheduledMail", new[] { "SmtpServer_Key" });
			DropIndex("dbo.Silversite_Data_ScheduledMail", new[] { "Personal_Key" });
			DropIndex("dbo.Silversite_Data_ScheduledMail", new[] { "HtmlText_Key" });
			DropIndex("dbo.Silversite_Data_ScheduledMail", new[] { "PlainText_Key" });
			DropIndex("dbo.Silversite_Data_MailSubstitution", new[] { "ScheduledMail_Key" });
			DropIndex("dbo.Silversite_Data_MailSubstitution", new[] { "SentMail_Key" });
			DropIndex("dbo.Silversite_Data_MailResource", new[] { "ScheduledMail_Key" });
			DropIndex("dbo.Silversite_Data_MailResource", new[] { "SentMail_Key" });
			DropIndex("dbo.Silversite_Data_SentMail", new[] { "SmtpServer_Key" });
			DropIndex("dbo.Silversite_Data_SentMail", new[] { "Personal_Key" });
			DropIndex("dbo.Silversite_Data_SentMail", new[] { "HtmlText_Key" });
			DropIndex("dbo.Silversite_Data_SentMail", new[] { "PlainText_Key" });
			DropIndex("dbo.Silversite_Web_Providers_Role", new[] { "ApplicationKey" });
			DropIndex("dbo.Silversite_Web_Providers_User", new[] { "Application_Key" });
			DropIndex("dbo.Silversite_Services_Document", new[] { "Author_Key" });

			DropForeignKey("dbo.Silversite_Web_Providers_UserRoles", "User", "dbo.Silversite_Web_Providers_Role");
			DropForeignKey("dbo.Silversite_Web_Providers_UserRoles", "Role", "dbo.Silversite_Web_Providers_User");
			DropForeignKey("dbo.Silversite_Services_StopwatchRecord", "ParentPage_Id", "dbo.Silversite_Services_StopwatchRecord");
			DropForeignKey("dbo.Silversite_Data_ScheduledMail", "SmtpServer_Key", "dbo.Silversite_Data_SmtpServer");
			DropForeignKey("dbo.Silversite_Data_ScheduledMail", "Personal_Key", "dbo.Silversite_Services_Person");
			DropForeignKey("dbo.Silversite_Data_ScheduledMail", "HtmlText_Key", "dbo.Silversite_Data_MailText");
			DropForeignKey("dbo.Silversite_Data_ScheduledMail", "PlainText_Key", "dbo.Silversite_Data_MailText");
			DropForeignKey("dbo.Silversite_Data_MailSubstitution", "ScheduledMail_Key", "dbo.Silversite_Data_ScheduledMail");
			DropForeignKey("dbo.Silversite_Data_MailSubstitution", "SentMail_Key", "dbo.Silversite_Data_SentMail");
			DropForeignKey("dbo.Silversite_Data_MailResource", "ScheduledMail_Key", "dbo.Silversite_Data_ScheduledMail");
			DropForeignKey("dbo.Silversite_Data_MailResource", "SentMail_Key", "dbo.Silversite_Data_SentMail");
			DropForeignKey("dbo.Silversite_Data_SentMail", "SmtpServer_Key", "dbo.Silversite_Data_SmtpServer");
			DropForeignKey("dbo.Silversite_Data_SentMail", "Personal_Key", "dbo.Silversite_Services_Person");
			DropForeignKey("dbo.Silversite_Data_SentMail", "HtmlText_Key", "dbo.Silversite_Data_MailText");
			DropForeignKey("dbo.Silversite_Data_SentMail", "PlainText_Key", "dbo.Silversite_Data_MailText");
			DropForeignKey("dbo.Silversite_Web_Providers_Role", "ApplicationKey", "dbo.Silversite_Web_Providers_Application");
			DropForeignKey("dbo.Silversite_Web_Providers_User", "Application_Key", "dbo.Silversite_Web_Providers_Application");
			DropForeignKey("dbo.CompanyCategories", "WebAddress_Key", "dbo.Silversite_Services_Person");
			DropForeignKey("dbo.Silversite_Services_Document", "Author_Key", "dbo.Silversite_Services_Person");
			DropTable("dbo.Silversite_Web_Providers_UserRoles");
			DropTable("dbo.Silversite_Data_Currency");
			DropTable("dbo.Silversite_Data_Language");
			DropTable("dbo.Silversite_Services_StopwatchRecord");
			DropTable("dbo.Silversite_Services_UptimeRecord");
			DropTable("dbo.Silversite_Data_ScheduledMail");
			DropTable("dbo.Silversite_Data_SmtpServer");
			DropTable("dbo.Silversite_Data_MailSubstitution");
			DropTable("dbo.Silversite_Data_MailResource");
			DropTable("dbo.Silversite_Data_MailText");
			DropTable("dbo.Silversite_Data_SentMail");
			DropTable("dbo.Silversite_Services_LogMessage");
			DropTable("dbo.Silversite_Web_Providers_Role");
			DropTable("dbo.Silversite_Web_Providers_Application");
			DropTable("dbo.Silversite_Web_Providers_User");
			DropTable("dbo.Silversite_Services_EditRights");
			DropTable("dbo.CompanyCategories");
			DropTable("dbo.Silversite_Services_Document");
			DropTable("dbo.Silversite_Services_Person");
		}
	}
}
