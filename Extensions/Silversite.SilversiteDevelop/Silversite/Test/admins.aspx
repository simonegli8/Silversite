<%@ Page Language="C#" MasterPageFile="~/Silversite/Test/master/test.master" AutoEventWireup="true" CodeBehind="admins.aspx.cs" Inherits="Silversite.silversite.test.Admins" Title="Administratoren" %>

<asp:Content ContentPlaceHolderID="content" runat="server">

	<ss:Head runat="server">
		<style type="text/css">
			.textbox {
				width: 200px;
			}
		</style>
	</ss:Head>
	
	<asp:Panel runat="server" style="position:relative">
		<asp:Panel ID="popup" CssClass="editor" runat="server" style="position:absolute;">
			<asp:ImageButton ID="ImageButton1" runat="server" ImageUrl="~/Silversite/Images/cancel.gif" OnClick="Finish" AlternateText="Schliessen" Style="position:absolute; right: 8px; top: 8px;"/>

			<asp:CreateUserWizard ID="CreateUserWizard" runat="server"
				CancelButtonText="Abbrechen" 
				CreateUserButtonText="Administrator Registrieren" 
				DuplicateEmailErrorMessage="Diese E-Mail Adresse ist bereits registriert." 
				DuplicateUserNameErrorMessage="Diese E-Mail Adresse ist bereits registriert." 
				FinishCompleteButtonText="Abschliessen" FinishPreviousButtonText="Zurück" 
				InvalidPasswordErrorMessage="Minimale Passwort Länge: {0}. Mindest Nicht-Alphanumerische Zeichen: {1}." 
				StartNextButtonText="Weiter" StepNextButtonText="Weiter" 
				StepPreviousButtonText="Zurück" 
				UnknownErrorMessage="Ihr Benutzerkonto konnte nicht erstellt werden. Bitte versuchen Sie es erneut."
				OnCreatingUser="CreatingUser"
				OnCancelButtonClick="Finish"
				>
				<WizardSteps>
					<asp:CreateUserWizardStep ID="CreateUserWizardStep" runat="server">
						<ContentTemplate>
							<h3 class="title">Administrator - Registrierung</h3>
							<table class="inputbox" border="0">
								<tr>
									<td class="label" align="right" >
										<asp:Label ID="EmailLabel" runat="server" AssociatedControlID="UserName">E-Mail</asp:Label>
									</td>
									<td class="inputbox">
										<asp:TextBox ID="UserName" runat="server" CssClass="textbox" Width="200" />
									</td>
									<td>
										<asp:RequiredFieldValidator ID="UserNameRequired" runat="server" 
											ControlToValidate="UserName"
											ErrorMessage="Sie müssen eine E-Mail Adresse angeben." 
											ToolTip="Sie müssen eine E-Mail Adresse angeben." ValidationGroup="CreateUserWizard">
											*
										</asp:RequiredFieldValidator>
										<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server"
											ControlToValidate="UserName" ErrorMessage="Dies ist keine gültige E-Mail Adresse."
											ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
											ValidationGroup="CreateUserWizard" >
											*
										</asp:RegularExpressionValidator>
										<!-- Verstecktes Feld Email -->
										<asp:TextBox ID="Email" runat="server" Visible="false" />
									</td>
								</tr>
								<tr>
									<td class="label" align="right">
										<asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Gewünschtes Passwort</asp:Label>
									</td>
									<td class="inputbox">
										<asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="textbox" Width="200"/>
									</td><td>
										<asp:RequiredFieldValidator ID="PasswordRequired" runat="server" 
											ControlToValidate="Password" ErrorMessage="Sie müssen ein Passwort angeben." 
											ToolTip="Sie müssen ein Passwort angeben." ValidationGroup="CreateUserWizard">*</asp:RequiredFieldValidator>
									</td>
								</tr>
								<tr>
									<td class="label" align="right">
										<asp:Label ID="ConfirmPasswordLabel" runat="server" AssociatedControlID="ConfirmPassword">Passwort bestätigen</asp:Label>
									</td>
									<td class="inputbox">
										<asp:TextBox ID="ConfirmPassword" runat="server" TextMode="Password" CssClass="textbox" Width="200"/>
									</td><td>
										<asp:RequiredFieldValidator ID="ConfirmPasswordRequired" runat="server" 
											ControlToValidate="ConfirmPassword" 
											ErrorMessage="Sie müssen das Passwort bestätigen." 
											ToolTip="Sie müssen das Passwort bestätigen." ValidationGroup="CreateUserWizard">*</asp:RequiredFieldValidator>
										<asp:CompareValidator ID="PasswordCompare" runat="server" 
											ControlToCompare="Password" ControlToValidate="ConfirmPassword" 
											ErrorMessage="Das Passwort und die Bestätigung stimmen nicht überein." 
											ValidationGroup="CreateUserWizard">*</asp:CompareValidator>
									</td>
								</tr>
								<tr>
									<td colspan="3" style="color:Red;">
										<asp:ValidationSummary ID="RegisterValidationSummary" runat="server" ValidationGroup="CreateUserWizard" />
									</td>
								</tr>
								<tr>
									<td colspan="3" style="color:Red;">
										<asp:Literal ID="ErrorMessage" runat="server" EnableViewState="False"></asp:Literal>
									</td>
								</tr>
							</table>
						</ContentTemplate>
					</asp:CreateUserWizardStep>
					<asp:CompleteWizardStep ID="CompleteWizardStep" runat="server">
						<ContentTemplate>
							<h3 class="title">Sie haben den Administrator erfolgreich registriert.</h3>
							<table class="inputbox" border="0">
								<tr>
									<td align="right" colspan="2">
										<asp:Button ID="ContinueButton" runat="server" CausesValidation="False"  Text="Fertig" OnClick="Finish" />
									</td>
								</tr>
							</table>
						</ContentTemplate>
					</asp:CompleteWizardStep>
				</WizardSteps>
			</asp:CreateUserWizard>
		</asp:Panel>

		<h1>Administratoren verwalten</h1>
		<p>
			<asp:Button runat="server" Text="Neuen Administrator erstellen" OnClick="NewAdmin" /> <br />
			<asp:Label ID="msg" runat="server" EnableViewState="False" />
		</p>
	
		<asp:ObjectDataSource ID="source" runat="server" EnablePaging="true" 
			TypeName="Silversite.Web.Users" SelectMethod="All" SelectCountMethod="AllCount"
			DeleteMethod="DeleteUser" >
			<DeleteParameters>
				<asp:Parameter Type="String" Name="username" />
			</DeleteParameters>
		</asp:ObjectDataSource>
    
		 <asp:GridView ID="grid" runat="server" DataSourceID="source" AllowPaging="true" PageSize="20" DataKeyNames="UserName"
			CssClass="admins"
			OnRowCommand="RowCommand" AutoGenerateColumns="false" >
			<Columns>
				<asp:TemplateField>	
				  <ItemTemplate>
						<asp:ImageButton runat="server"
							OnClientClick='return confirm("Wollen Sie diesen Administrator wirklich löschen?");' 
							AlternateText="Löschen" ImageUrl="~/Silversite/Images/del.gif" ToolTip = "Löschen"
							 CommandName="Delete" />
					</ItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="Email" SortExpression="Email" >
					<ItemTemplate>
						<asp:HyperLink runat="server" Text='<%# Eval("Email") %>' NavigateUrl='<%# Eval("Email", "mailto:{0}")  %>' />
					</ItemTemplate>
				</asp:TemplateField>
				<asp:TemplateField HeaderText="Sperre">
					<ItemTemplate>
						<asp:LinkButton runat="server" CommandArgument='<%# Eval("UserName") %>' CommandName="Unlock" Text="Sperre aufheben"
							Visible='<%# Eval("IsLockedOut") %>' />
					</ItemTemplate>
				</asp:TemplateField>
			</Columns>
		 </asp:GridView>
	</asp:Panel>
</asp:Content>