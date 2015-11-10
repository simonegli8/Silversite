using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using Srvc = Silversite.Services;

namespace Silversite.WebServices {

	[ServiceContract(Namespace = "Silversite.WebServices")]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class FileManager {

		public const string FullFileSystemAccessRole = "RootAccess";

		// To use HTTP GET, add [WebGet] attribute. (Default ResponseFormat is WebMessageFormat.Json)
		// To create an operation that returns XML,
		//     add [WebGet(ResponseFormat=WebMessageFormat.Xml)],
		//     and include the following line in the operation body:
		//         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
		[OperationContract]
		[WebInvoke]
		public FileObjectInfo GetInfo(string path, bool getSize, bool showThumbs) {
			var user = Srvc.Persons.Current;
			if (user != null) return new FileObjectInfo(path, getSize, showThumbs);
			else return null;
		}

		[OperationContract]
		[WebInvoke]
		public FileObjectInfo GetInfo(string path) {
			return GetInfo(path, true, true);
		}

		[OperationContract]
		[WebInvoke]
		public DirectoryInfo GetDirectory(string path, bool getSize, bool showThumbs, int depth) {
			var user = Srvc.Persons.Current;
			if (user != null) return new DirectoryInfo(string.Empty, path, getSize, showThumbs, depth);
			else return null;
		}

		[OperationContract]
		[WebInvoke]
		public DirectoryInfo GetDirectory(string path, int depth) {
			return GetDirectory(path, true, true, depth);
		}

		[OperationContract]
		[WebInvoke]
		public Message Move(string oldpath, string newpath) {
			var user = Srvc.Persons.Current;
			if (user != null) {
				Srvc.Files.Move(user.AbsolutePath(oldpath), user.AbsolutePath(newpath));
				return new Message();
			}
			return new Message { Code = 1, Error = "Not logged in." };
		}

		[OperationContract]
		[WebInvoke]
		public Message Copy(string oldpath, string newpath) {
			var user = Srvc.Persons.Current;
			if (user != null) {
				Srvc.Files.Move(user.AbsolutePath(oldpath), user.AbsolutePath(newpath));
				return new Message();
			}
			return new Message { Code = 1, Error = "Not logged in." };
		}

		[OperationContract]
		[WebInvoke]
		public Message Delete(string path) {
			var user = Srvc.Persons.Current;
			if (user != null) {
				Srvc.Files.Delete(user.AbsolutePath(path));
				return new Message();
			}
			return new Message { Code = 1, Error = "Not logged in." };
		}

		[OperationContract]
		[WebInvoke]
		public Message CreateDirectory(string path) {
			var user = Srvc.Persons.Current;
			if (user != null) {
				Srvc.Files.CreateDirectory(user.AbsolutePath(path));
				return new Message();
			}
			return new Message { Code = 1, Error = "Not logged in." };
		}


		[OperationContract]
		[WebInvoke]
		public Text ReadText(string path) {
			var user = Srvc.Persons.Current;
			if (user != null) {
				var text = Srvc.Files.Load(user.AbsolutePath(path));
				return new Text { Content = text };
			}
			return new Text { Code = 1, Error = "Not logged in.", Content = "" };
		}

	}
}
