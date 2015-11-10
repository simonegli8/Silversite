// http://aspnetupload.com
// Copyright � 2009 Krystalware, Inc.
//
// This work is licensed under a Creative Commons Attribution-Share Alike 3.0 United States License
// http://creativecommons.org/licenses/by-sa/3.0/us/

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.IO;

namespace Silversite.Html
{
	/// <summary>
	/// A helper class to upload files in a html form.
	/// </summary>
    internal class HttpUploadHelper
    {
        private HttpUploadHelper()
        { }
		  /// <summary>
		  /// Uploads files & form values
		  /// </summary>
		  /// <param name="url">The form action url.</param>
		  /// <param name="files">The files to upload.</param>
		  /// <param name="form">The form values.</param>
		  /// <returns>Returns the http response.</returns>
		 public static string Upload(string url, UploadFile[] files, NameValueCollection form) {
			  HttpWebRequest r = (HttpWebRequest)WebRequest.Create(url);

			  Upload(r, files, form);

			  WebResponse resp = r.GetResponse();

			  using (Stream s = resp.GetResponseStream())
			  using (StreamReader sr = new StreamReader(s)) {
				  return sr.ReadToEnd();
			  }
		  }
		 /// <summary>
		 /// Uploads files & form values
		 /// </summary>
		 /// <param name="req">A web request.</param>
		 /// <param name="files">Files to upload.</param>
		 /// <param name="form">Form values to upload.</param>
        public static void Upload(HttpWebRequest req, UploadFile[] files, NameValueCollection form)
        {
            List<MimePart> mimeParts = new List<MimePart>();

            try
            {
                foreach (string key in form.AllKeys)
                {
                    StringMimePart part = new StringMimePart();

                    part.Headers["Content-Disposition"] = "form-data; name=\"" + key + "\"";
                    part.StringData = form[key];

                    mimeParts.Add(part);
                }

                int nameIndex = 0;

                foreach (UploadFile file in files)
                {
                    StreamMimePart part = new StreamMimePart();

                    if (string.IsNullOrEmpty(file.FieldName))
                        file.FieldName = "file" + nameIndex++;

                    part.Headers["Content-Disposition"] = "form-data; name=\"" + file.FieldName + "\"; filename=\"" + file.FileName + "\"";
                    part.Headers["Content-Type"] = file.ContentType;

                    part.SetStream(file.Data);

                    mimeParts.Add(part);
                }

                string boundary = "----------" + DateTime.Now.Ticks.ToString("x");

                req.ContentType = "multipart/form-data; boundary=" + boundary;
                req.Method = "POST";

                long contentLength = 0;

                byte[] _footer = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");

                foreach (MimePart part in mimeParts)
                {
                    contentLength += part.GenerateHeaderFooterData(boundary);
                }

                req.ContentLength = contentLength + _footer.Length;

                byte[] buffer = new byte[8192];
                byte[] afterFile = Encoding.UTF8.GetBytes("\r\n");
                int read;

                using (Stream s = req.GetRequestStream())
                {
                    foreach (MimePart part in mimeParts)
                    {
                        s.Write(part.Header, 0, part.Header.Length);

                        while ((read = part.Data.Read(buffer, 0, buffer.Length)) > 0)
                            s.Write(buffer, 0, read);

                        part.Data.Dispose();

                        s.Write(afterFile, 0, afterFile.Length);
                    }

                    s.Write(_footer, 0, _footer.Length);
                }

                // return (HttpWebResponse)req.GetResponse();
            }
            catch
            {
                foreach (MimePart part in mimeParts)
                    if (part.Data != null)
                        part.Data.Dispose();

                throw;
            }
        }
    }
}