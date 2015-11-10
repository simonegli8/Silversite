using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;

namespace Silversite.Web {

	public class ThumbnailHandler: IHttpHandler, IHttpModule {

		public const string CacheFolder = Services.Paths.Cache + "/Thumbnails";
		public const string DefaultExtension = ".thumbnail";
		public const int DefaultWidth = 165;
		public const int DefaultHeight = 99;

		public Dictionary<HttpRequest, string> ThumbnailRequests = new Dictionary<HttpRequest, string>();

		public string CacheFile {
			get {
				string query = HttpContext.Current.Request.Url.PathAndQuery;
				return CacheFolder + "/" + Services.Hash.Compute(query) + "." + Services.Paths.FileWithoutExtension(query);
			}
		}

		public void ProcessRequest(HttpContext context) {
			var Response = context.Response;

			var cache = CacheFile;
			if (Services.Files.FileExists(cache)) {
				Services.Files.Serve(cache);
				return;
			}

			string ImagePath;
			if (!ThumbnailRequests.TryGetValue(context.Request, out ImagePath)) {
				ImagePath = Services.Paths.WithoutExtension(Services.Paths.Normalize(HttpUtility.UrlDecode(context.Request.Url.AbsolutePath)));
			} else {
				ThumbnailRequests.Remove(context.Request);
			}
	
			int width = -1, height = -1;
			bool keepRatio;

			string wstr = context.Request.QueryString["width"];
			string hstr = context.Request.QueryString["height"];
			string rstr = context.Request.QueryString["keepratio"];

			if (!string.IsNullOrEmpty(wstr)) int.TryParse(wstr, out width);
			if (!string.IsNullOrEmpty(hstr))	int.TryParse(hstr, out height);
	
			if (width == -1) {
				if (height != -1) width = int.MaxValue;
				else width = DefaultWidth;
			}
			if (height == -1) {
				if (width != -1) height = int.MaxValue;
				else height = DefaultHeight;
			}

			keepRatio = string.Compare("false", rstr, true) != 0;


			using (var file = Services.Files.OpenVirtual(ImagePath)) {
				Bitmap bmp = (Bitmap)Bitmap.FromStream(file);
				ImageThumbnailer thumbnailer = new ImageThumbnailer();
				thumbnailer.CreateThumbnail(bmp, width, height, keepRatio);
				thumbnailer.Save(Response, cache);
			}
		}

		public bool IsReusable { get { return true; } }

		public void MapHandler(object sender, EventArgs e) {
			HttpApplication app = (HttpApplication)sender;
			string appPath = app.Context.Request.ApplicationPath;
			int p0, p1;
			p1 = appPath.LastIndexOf('.');
			p0 = appPath.LastIndexOf('.', p1-1);
			if (p0 > 0 && p1 > 0 && appPath.Substring(p0, p1) == DefaultExtension) {
				var imagePath = appPath.Remove(p0, p1-p0);
				ThumbnailRequests[app.Context.Request] = imagePath;
				app.Context.RemapHandler(this);
			}
		}

		public void Dispose() { }
		public void Init(HttpApplication context) {
			context.PostMapRequestHandler += MapHandler;
		}
	}


	public class ImageThumbnailer {
		private Bitmap _thumb = null;

		/// <summary>
		/// Creates the thumbnail.
		/// </summary>
		/// <param name="SourceImage">The source image.</param>
		/// <param name="Width">The width.</param>
		/// <param name="Height">The height.</param>
		/// <param name="KeepRatio">if set to <c>true</c> [keep ratio].</param>
		/// <returns></returns>
		public Bitmap CreateThumbnail(Bitmap SourceImage,
			 int Width, int Height, Boolean KeepRatio) {
			// if Source Bitmap smaller than designated thumbnail => Return Original
			if (SourceImage.Width < Width && SourceImage.Height < Height)
				return _thumb = SourceImage;

			try {
				int _Width = 0;
				int _Height = 0;

				_Width = Width;
				_Height = Height;

				double fx = (double)Width / SourceImage.Width;
				double fy = (double)Height / SourceImage.Height;

				double f = Math.Min(fx, fy);

				if (KeepRatio) {
					if (fx < fy) {
						_Width = Width;
						_Height = (int)(SourceImage.Height * fx + 0.5);
					} else {
						_Height = Height;
						_Width = (int)(SourceImage.Width * fy + 0.5);
					}
				}

				_thumb = new Bitmap(_Width, _Height);
				using (Graphics g = Graphics.FromImage(_thumb)) {
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.FillRectangle(Brushes.White, 0, 0, _Width, _Height);
					g.DrawImage(SourceImage, 0, 0, _Width, _Height);
				}
			} catch {
				_thumb = null;
			}
			return _thumb;
		}

		/// <summary>
		/// Saves the Bitmap to the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public void Save(HttpResponse response, string filename) {
			if (_thumb != null) {
				var ext = Services.Paths.Extension(filename).ToLower();
				if (ext == "jpg" || ext == "jpeg" || ext == "jpe") {
					// JPEG Optimizing
					EncoderParameters encoderParams = new EncoderParameters();
					long[] quality = new long[1];
					quality[0] = 75;
					EncoderParameter encoderParam = new EncoderParameter(Encoder.Quality, quality);
					encoderParams.Param[0] = encoderParam;

					ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
					ImageCodecInfo codec = encoders.Where(
						 p => p.FormatDescription.Equals("JPEG")).SingleOrDefault();
					// Save to Specified Stream
					response.ContentType = "image/jpeg";
					using (var m = new MemoryStream()) {
						_thumb.Save(m, codec, encoderParams);
						m.Seek(0, SeekOrigin.Begin);
						m.CopyTo(response.OutputStream);
						Services.Files.Save(m, filename); // cache result
					}
				} else {
					response.ContentType  = "image/png";
					using (var m = new MemoryStream()) {
						_thumb.Save(m, ImageFormat.Png);
						m.Seek(0, SeekOrigin.Begin);
						m.CopyTo(response.OutputStream);
						Services.Files.Save(m, filename); // cache result
					}
				}
			}
		}
	}
}
