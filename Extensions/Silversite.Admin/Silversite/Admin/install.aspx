<%@ Page Title="" Language="C#" MasterPageFile="~/Silversite/Admin/master/admin.master" AutoEventWireup="true" CodeBehind="install.aspx.cs" Inherits="Silversite.silversite.admin.InstallWizard" %>
<%@ Register TagPrefix="ss" Namespace="Silversite.Web.UI" Assembly="Silversite.Core" %>

<asp:Content ID="Content2" ContentPlaceHolderID="page" runat="server">
	
	<ss:Head runat="server">
		<style type="text/css">
			td { padding 0px 4px 0px 4px; }
		</style>
	</ss:Head>

	<asp:Wizard runat="server" ID="wiz" ActiveStepIndex="1" Width="504px" OnActiveStepChanged="StepChanged" OnNextButtonClick="NextClick" >
		<WizardSteps>
			<asp:TemplatedWizardStep ID="dbstep">
				<ContentTemplate>
					<h1>Select a Database</h1>
					<p>
						<table>
							<tr><td>Database:</td>
								<td>
									<asp:DropDownList runat="server" ID="dbtype">
										<asp:ListItem Text="SQL Server" Value="MSSql" />
										<asp:ListItem Text="SQL CE 4" Value="MSSqlCE" />
										<asp:ListItem Text="MySql" Value="MySql" />
									</asp:DropDownList>
								</td>
							</tr>
							<tr><td>Database Server:</td>
								<td><asp:TextBox runat="server" ID="dbserver" Width="100%" /></td>
							</tr>
							<tr><td>Database Username:</td>
								<td><asp:TextBox runat="server" ID="dbuser" Width="100%" /></td>
							</tr>
							<tr><td>Database Password:</td>
								<td><asp:TextBox runat="server" ID="dbpassword" TextMode="Password" Width="100%" /></td>
							</tr>
							<tr runat="server" ID="appdatarow"><td>Store in App_Data Folder:</td>
								<td><asp:CheckBox runat="server" ID="appdata" /></td>
							</tr>
							<tr><td>Database Name:</td>
								<td><asp:TextBox runat="server" ID="db" Width="100%" /></td>
							</tr>
							<tr><td><ss:SaveIcon runat="server" ID="dbok" Visible="false" /><ss:CancelIcon runat="server" ID="dberror" Visible="false" /></td>
								<td><asp:Button runat="server" Text="Test Connection..." /></td>
							</tr>
						</table>
					</p>
				</ContentTemplate>
			</asp:TemplatedWizardStep>
			<asp:TemplatedWizardStep ID="sastep">
				<ContentTemplate>
					<h1>Create Admin User</h1>
					<table>
						<tr><td>Email:</td>
							<td width="70%"><asp:TextBox runat="server" ID="saemail" Width="100%" /></td>
						</tr>
						<tr><td>Password:</td>
							<td><asp:TextBox runat="server" ID="sapassword" Width="100%" TextMode="Password" /></td>
						</tr>
						<tr><td>Retype Password: </td>
							<td><asp:TextBox runat="server" ID="sapwdretype" Width="100%" TextMode="Password" /></td>
						</tr>
					</table>
				</ContentTemplate>
			</asp:TemplatedWizardStep>
		</WizardSteps>
	</asp:Wizard>
</asp:Content>
