// davidegli

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;

namespace Silversite.Services {

	public enum TextContentClass { Html, Aspx, RazorCSharp, RazorVisualBasic, CSharp, VisualBasic, FSharp, Phyton, Ruby, Xml, Xaml, Css, Php, Sql, Wiki, Text, Email, EmailAspx, Other };

	// TODO move management of old documents & ContentKey from Database to a Provider implementing a gzip file storage.

	/// <summary>
	/// Common properties of Documents, for Documents that are stored in the Database and for the DocumentInfo Control that get's stored in the .aspx page.
	/// </summary>
	public interface IDocumentInfo {
		/// <summary>
		/// An integer key that determines the document.
		/// </summary>
		int ContentKey { get; set; }
		/// <summary>
		/// A document title.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// The revision of the document.
		/// </summary>
		int Revision { get; set; }
		/// <summary>
		/// The document's author.
		/// </summary>
		Person Author { get; set; }
		/// <summary>
		/// Notes about the document.
		/// </summary>
		string Notes { get; set; }
		/// <summary>
		/// Document tags.
		/// </summary>
		string Tags { get; set; }
		/// <summary>
		/// The categories the document belogns to, separated by commas or semicolons.
		/// </summary>
		string Categories { get; set; }
		/// <summary>
		/// The date when the document was published.
		/// </summary>
		DateTime Published { get; set; }
	}
	/// <summary>
	/// A Document as it get's stored in the database. 
	/// </summary>
	public class Document : IDocumentInfo {

		public const int None = int.MinValue; // the ContentKey & Key for no document.
		public const int Preview = int.MaxValue; // the Revision for a preview document.

		public Document() {
			Author = Persons.Current;
			Published = DateTime.Now;
			Categories = Title = Tags = Notes = Text = Path = string.Empty;
			Domain = Services.Domains.Default;
			Revision = 1;
			IsCurrentRevision = true;
		}
		/// <summary>
		/// The document's key.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Key { get; set; }
		/// <summary>
		/// An key that determines the document and it's revisions.
		/// </summary>
		public int ContentKey { get; set; }
		/// <summary>
		/// The path to the .aspx page or control that contains the document.
		/// </summary>
		[MaxLength(512)]
		public string Path { get; set; }
		/// <summary>
		/// The domain the document belongs to.
		/// </summary>
		[Required, MaxLength(255)]
		public string DomainPattern { get; set; }
		[NotMapped]
		public Services.Domain Domain { get { return (Services.Domain)DomainPattern; } set { DomainPattern = value.Pattern; } }
		/// <summary>
		/// The document's title.
		/// </summary>
		[MaxLength(255)]
		public string Title { get; set; }
		/// <summary>
		/// The dcoument's author.
		// </summary>
		public virtual Person Author { get; set; }
		/// <summary>
		/// Notes about the document.
		/// </summary>
		[MaxLength]
		public string Notes { get; set; }
		/// <summary>
		/// Document tags.
		/// </summary>
		[MaxLength]
		public string Tags { get; set; }
		/// <summary>
		/// The Document's revision.
		/// </summary>
		public int Revision { get; set; }
		/// <summary>
		/// True if this document is the current revision.
		/// </summary>
		public bool IsCurrentRevision { get; set; }
		/// <summary>
		/// True if this document is the preview revision.
		/// </summary>
		public bool IsPreviewRevision { get; set; }
		/// <summary>
		/// The categories the document belongs to, separated by commas or semicolons.
		/// </summary>
		[MaxLength]
		public string Categories { get; set; }
		/// <summary>
		/// The date when the document was published.
		/// </summary>
		public DateTime Published { get; set; }
		/// <summary>
		/// The domcument's text.
		/// </summary>
		[MaxLength]
		public string Text { get; set; }
		/// <summary>
		/// Copies the document for storing as old version of the document.
		/// </summary>
		/// <returns></returns>
		public Document Old() {
			var old = new Document();
			CopyInfo(this, old);
			old.Path = Path;
			old.Domain = Domain;
			old.Text = Text;
			old.IsCurrentRevision = false;
			return old;
		}
		/// <summary>
		/// Copies DocumentInfo's.
		/// </summary>
		/// <param name="source">The source IDocumentInfo.</param>
		/// <param name="dest">The destination IDocumentInfo.</param>
		public static void CopyInfo(IDocumentInfo source, IDocumentInfo dest) {
			dest.Author = source.Author;
			dest.Categories = source.Categories;
			dest.Title = source.Title;
			dest.Notes = source.Notes;
			dest.Published = source.Published;
			dest.ContentKey = source.ContentKey;
			dest.Revision = source.Revision;
			dest.Tags = source.Tags;
		}
		/// <summary>
		/// Copies IDocumentInfos.
		/// </summary>
		/// <param name="doc">The IDocumentInfo to copy from.</param>
		public void CopyFrom(IDocumentInfo doc) { CopyInfo(doc, this); }
	}
	/// <summary>
	/// A class to manage persistent document. This class serves as an interface to the database.
	/// </summary>
	public class Documents {
		/// <summary>
		/// Creates a new document and returns the document key.
		/// </summary>
		/// <param name="path">The path of the .aspx page or control containing the document.</param>
		/// <returns></returns>
		public static IDocumentInfo Create(string path) {
			using (var db = new Silversite.Context()) {
				var doc = new Document { Author = null, Path = path, Domain = Domains.Current };
				db.Documents.Add(doc);
				db.SaveChanges();
				doc.ContentKey = doc.Key;
				doc.Revision = 1;
				return doc;
			}
		}
		/// <summary>
		/// Gets the document with the supplied key.
		/// </summary>
		/// <param name="key">The document key to look for.</param>
		/// <returns>The Document with the given key.</returns>
		public static Document Current(int contentKey) { using (var db = new Silversite.Context()) { return Current(db, contentKey); } }
		/// <summary>
		/// Gets the document with the supplied key.
		/// </summary>
		/// <param name="db">The Context to use.</param>
		/// <param name="key">The document to look for.</param>
		/// <returns>The Document with the given key.</returns>
		public static Document Current(Silversite.Context db, int contentKey) { return db.Documents.FirstOrDefault(d => (d.ContentKey == contentKey || d.Key == contentKey) && d.IsCurrentRevision == true); }
		/// <summary>
		/// Publishes a new document.
		/// </summary>
		/// <param name="doc">The IDocumentInfo for the document, including the document key.</param>
		/// <param name="text">The document's text.</param>
		public static void Publish(IDocumentInfo doc, string text) {
			using (var db = new Silversite.Context()) {
				var d = Current(db, doc.ContentKey);
				if (d == null) throw new ArgumentException("invalid key");
				if (d.Text != string.Empty) db.Documents.Add(d.Old());
				d.CopyFrom(doc);
				d.Text = text;
				db.SaveChanges();
			}
		}
		/// <summary>
		/// Returns theDocument that corresponds to the supplied revision.
		/// </summary>
		/// <param name="key">The Document key.</param>
		/// <param name="revision">The document revision.</param>
		/// <returns>The revision of the document.</returns>
		public static Document Revision(Nullable<int> key, int revision) {
			if (key == null) return null;
			using (var db = new Silversite.Context()) {
				return db.Documents.FirstOrDefault(doc => doc.ContentKey == key && doc.Revision == revision);
			}
		}
		/// <summary>
		/// Returns all revisions of a document. 
		/// </summary>
		/// <param name="key">The document key.</param>
		/// <returns>A IQueryable of all revisions of that document.</returns>
		public static IQueryable<Document> Revisions(Nullable<int> key) {
			if (key == null) return new Document[0].AsQueryable();
			using (var db = new Silversite.Context()) {
				return db.Documents.Where(doc => doc.ContentKey == key);
			}
		}
		/// <summary>
		/// Sets the IDocumentInfo of the corresponding document in the database.
		/// </summary>
		/// <param name="doc">The IDocumentInfo of the document.</param>
		public static void Modify(IDocumentInfo doc) {
			using (var db = new Silversite.Context()) {
				var d = Current(db, doc.ContentKey); if (d == null) return;
				d.CopyFrom(doc);
				db.SaveChanges();
			}
		}

	}
}