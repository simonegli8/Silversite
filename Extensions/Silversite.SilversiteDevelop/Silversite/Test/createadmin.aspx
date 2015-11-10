<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Silversite/Test/master/test.master" CodeBehind="createadmin.aspx.cs"
	Inherits="Silversite.silversite.test.CreateAdmin" %>

<asp:Content ContentPlaceHolderID="content" runat="server">
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
		>
		<WizardSteps>
			<asp:CreateUserWizardStep ID="CreateUserWizardStep" runat="server">
				<ContentTemplate>
					<h3 class="title">Administrator - Registrierung</h3>
					<table class="EingabeTabelle" border="0">
						<tr>
							<td class="label" align="right" >
								<asp:Label ID="EmailLabel" runat="server" AssociatedControlID="UserName">E-Mail</asp:Label>
							</td>
							<td>
								<asp:TextBox ID="UserName" runat="server" CssClass="textbox"/>
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
							<td>
								<asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="textbox"></asp:TextBox>
								<asp:RequiredFieldValidator ID="PasswordRequired" runat="server" 
									ControlToValidate="Password" ErrorMessage="Sie müssen ein Passwort angeben." 
									ToolTip="Sie müssen ein Passwort angeben." ValidationGroup="CreateUserWizard">*</asp:RequiredFieldValidator>
							</td>
						</tr>
						<tr>
							<td class="label" align="right">
								<asp:Label ID="ConfirmPasswordLabel" runat="server" AssociatedControlID="ConfirmPassword">Passwort bestätigen</asp:Label>
							</td>
							<td>
								<asp:TextBox ID="ConfirmPassword" runat="server" TextMode="Password" CssClass="textbox" />
								<asp:RequiredFieldValidator ID="ConfirmPasswordRequired" runat="server" 
									ControlToValidate="ConfirmPassword" 
									ErrorMessage="Sie müssen das Passwort bestätigen." 
									ToolTip="Sie müssen das Passwort bestätigen." ValidationGroup="CreateUserWizard">*</asp:RequiredFieldValidator>
								<asp:CompareValidator ID="PasswordCompare" runat="server" 
									ControlToCompare="Password" ControlToValidate="ConfirmPassword" 
									ErrorMessage="Das Passwort und die Bestätigung stimmen nicht überein." 
									ValidationGroup="CreateUserWizard" Text="*" />
							</td>
						</tr>
						<tr>

						</tr>
						<tr>
							<td colspan="2" style="color:Red;">
								<asp:ValidationSummary ID="RegisterValidationSummary" runat="server" ValidationGroup="CreateUserWizard" />
							</td>
						</tr>
						<tr>
							<td colspan="2" style="color:Red;">
								<asp:Literal ID="ErrorMessage" runat="server" EnableViewState="False"></asp:Literal>
							</td>
						</tr>
					</table>
				</ContentTemplate>
			</asp:CreateUserWizardStep>
			<asp:CompleteWizardStep ID="CompleteWizardStep" runat="server">
				<ContentTemplate>
					<h3 class="title">Sie haben den Administrator erfolgreich registriert.</h3>
					<table class="EingabeTabelle" border="0">
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
</asp:Content>
