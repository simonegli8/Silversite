﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

[assembly:Silversite.Services.DependsOn(typeof(Silversite.Services.WindowsMimeTypeProvider))]
[assembly:Silversite.Services.DependsOn(typeof(Silversite.Services.StaticMimeTypeProvider))]

namespace Silversite.Services {

	/// <summary>
	/// A class to retrieve MIME Types for files.
	/// </summary>
	/// 

	public class MimeType : StaticService<MimeType, MimeTypeProvider> {
		/// <summary>
		/// Ensures that the file (also virtual) exists and retrieves the content type. 
		/// </summary>
		/// <param name='strfileName'>The path to the file.</param>
		/// <returns>Returns the Content MimeType.</returns>
		public static string OfFile(string filename) { return Provider.OfFile(filename); }
		/// <summary>
		/// Ensures that the file (also virtual) exists and retrieves the content type. 
		/// </summary>
		/// <param name='strfileName'>The path to the file.</param>
		/// <returns>Returns the Content MimeType.</returns>
		public static string OfFile(System.IO.Stream stream, string filename) { return Provider.OfFile(stream, filename); }
		public static string OfExtension(string filename) { return Provider.OfExtension(filename); }
	}

	public abstract class MimeTypeProvider : Provider<MimeType> {
		/// <summary>
		/// Ensures that the file (also virtual) exists and retrieves the content type. 
		/// </summary>
		/// <param name='strfileName'>The path to the file.</param>
		/// <returns>Returns the Content MimeType.</returns>
		public abstract string OfFile(string filename);
		/// <summary>
		/// Ensures that the file (also virtual) exists and retrieves the content type. 
		/// </summary>
		/// <param name='strfileName'>The path to the file.</param>
		/// <returns>Returns the Content MimeType.</returns>
		public abstract string OfFile(System.IO.Stream stream, string filename); 

		public abstract string OfExtension(string filename);
	}

	public class WindowsMimeTypeProvider: MimeTypeProvider {

		public override void Startup() {
			if (Runtime.IsMicrosoft) {
				Modules.DependsOn<StaticMimeTypeProvider>();
				Providers.Register(this);
			}
		}

		[System.Runtime.InteropServices.DllImport("urlmon.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
		static extern int FindMimeFromData(IntPtr pBC,
			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pwzUrl,
			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPArray, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I1, SizeParamIndex = 3)] 
			byte[] pBuffer, int cbSize,
			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pwzMimeProposed, int dwMimeFlags, out IntPtr ppwzMimeOut, int dwReserved);

		/// <summary>
		/// Ensures that the file (also virtual) exists and retrieves the content type. 
		/// </summary>
		/// <param name='strfileName'>The path to the file.</param>
		/// <returns>Returns the Content MimeType.</returns>
		public override string OfFile(string strFileName) {
			IntPtr mimeout;
			if (!Files.ExistsVirtual(strFileName))
				throw new System.IO.FileNotFoundException(strFileName + " not found");

			int result = 0;
			using (var s = Files.OpenVirtual(strFileName)) {
				return OfFile(s, strFileName);
			}
		}

		/// <summary>
		/// Ensures that the file (also virtual) exists and retrieves the content type. 
		/// </summary>
		/// <param name='strfileName'>The path to the file.</param>
		/// <returns>Returns the Content MimeType.</returns>
		public override string OfFile(System.IO.Stream stream, string strFileName) {
			IntPtr mimeout;

			int result = 0;
			int MaxContent = (int)(Math.Max(4096, stream.Length));
			var buf = new byte[MaxContent];
			stream.Seek(0, System.IO.SeekOrigin.Begin);
			stream.Read(buf, 0, MaxContent);
			stream.Seek(0, System.IO.SeekOrigin.Begin);
			result = FindMimeFromData(IntPtr.Zero, strFileName, buf, MaxContent, null, 0, out mimeout, 0);

			if (result != 0)
				throw System.Runtime.InteropServices.Marshal.GetExceptionForHR(result);

			string mime = System.Runtime.InteropServices.Marshal.PtrToStringUni(mimeout);
			System.Runtime.InteropServices.Marshal.FreeCoTaskMem(mimeout);
			return mime;
		}
		public override string OfExtension(string filename) {
			var p = new StaticMimeTypeProvider();
			return p.OfExtension(filename);
		}
	}

	public class StaticMimeTypeProvider : MimeTypeProvider {

		public XElement Config = XElement.Parse(@"
			<!-- Mime Types copied from IISExpress -->
			<staticContent lockAttributes='isDocFooterFileName'>
				<mimeMap fileExtension='.323' mimeType='text/h323' />
				<mimeMap fileExtension='.3g2' mimeType='video/3gpp2' />
				<mimeMap fileExtension='.3gp2' mimeType='video/3gpp2' />
				<mimeMap fileExtension='.3gp' mimeType='video/3gpp' />
				<mimeMap fileExtension='.3gpp' mimeType='video/3gpp' />
				<mimeMap fileExtension='.7z' mimeType='application/x-7z-compressed' />
				<mimeMap fileExtension='.aac' mimeType='audio/aac' />
				<mimeMap fileExtension='.aaf' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.aca' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.accdb' mimeType='application/msaccess' />
				<mimeMap fileExtension='.accde' mimeType='application/msaccess' />
				<mimeMap fileExtension='.accdt' mimeType='application/msaccess' />
				<mimeMap fileExtension='.acx' mimeType='application/internet-property-stream' />
				<mimeMap fileExtension='.adt' mimeType='audio/vnd.dlna.adts' />
				<mimeMap fileExtension='.adts' mimeType='audio/vnd.dlna.adts' />
				<mimeMap fileExtension='.afm' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.ai' mimeType='application/postscript' />
				<mimeMap fileExtension='.aif' mimeType='audio/x-aiff' />
				<mimeMap fileExtension='.aifc' mimeType='audio/aiff' />
				<mimeMap fileExtension='.aiff' mimeType='audio/aiff' />
				<mimeMap fileExtension='.application' mimeType='application/x-ms-application' />
				<mimeMap fileExtension='.art' mimeType='image/x-jg' />
				<mimeMap fileExtension='.asd' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.asf' mimeType='video/x-ms-asf' />
				<mimeMap fileExtension='.asi' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.asm' mimeType='text/plain' />
				<mimeMap fileExtension='.asr' mimeType='video/x-ms-asf' />
				<mimeMap fileExtension='.asx' mimeType='video/x-ms-asf' />
				<mimeMap fileExtension='.atom' mimeType='application/atom+xml' />
				<mimeMap fileExtension='.au' mimeType='audio/basic' />
				<mimeMap fileExtension='.avi' mimeType='video/x-msvideo' />
				<mimeMap fileExtension='.axs' mimeType='application/olescript' />
				<mimeMap fileExtension='.bas' mimeType='text/plain' />
				<mimeMap fileExtension='.bcpio' mimeType='application/x-bcpio' />
				<mimeMap fileExtension='.bin' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.bmp' mimeType='image/bmp' />
				<mimeMap fileExtension='.c' mimeType='text/plain' />
				<mimeMap fileExtension='.cab' mimeType='application/vnd.ms-cab-compressed' />
				<mimeMap fileExtension='.calx' mimeType='application/vnd.ms-office.calx' />
				<mimeMap fileExtension='.cat' mimeType='application/vnd.ms-pki.seccat' />
				<mimeMap fileExtension='.cdf' mimeType='application/x-cdf' />
				<mimeMap fileExtension='.chm' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.class' mimeType='application/x-java-applet' />
				<mimeMap fileExtension='.clp' mimeType='application/x-msclip' />
				<mimeMap fileExtension='.cmx' mimeType='image/x-cmx' />
				<mimeMap fileExtension='.cnf' mimeType='text/plain' />
				<mimeMap fileExtension='.cod' mimeType='image/cis-cod' />
				<mimeMap fileExtension='.cpio' mimeType='application/x-cpio' />
				<mimeMap fileExtension='.cpp' mimeType='text/plain' />
				<mimeMap fileExtension='.crd' mimeType='application/x-mscardfile' />
				<mimeMap fileExtension='.crl' mimeType='application/pkix-crl' />
				<mimeMap fileExtension='.crt' mimeType='application/x-x509-ca-cert' />
				<mimeMap fileExtension='.csh' mimeType='application/x-csh' />
				<mimeMap fileExtension='.css' mimeType='text/css' />
				<mimeMap fileExtension='.csv' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.cur' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.dcr' mimeType='application/x-director' />
				<mimeMap fileExtension='.deploy' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.der' mimeType='application/x-x509-ca-cert' />
				<mimeMap fileExtension='.dib' mimeType='image/bmp' />
				<mimeMap fileExtension='.dir' mimeType='application/x-director' />
				<mimeMap fileExtension='.disco' mimeType='text/xml' />
				<mimeMap fileExtension='.dll' mimeType='application/x-msdownload' />
				<mimeMap fileExtension='.dll.config' mimeType='text/xml' />
				<mimeMap fileExtension='.dlm' mimeType='text/dlm' />
				<mimeMap fileExtension='.doc' mimeType='application/msword' />
				<mimeMap fileExtension='.docm' mimeType='application/vnd.ms-word.document.macroEnabled.12' />
				<mimeMap fileExtension='.docx' mimeType='application/vnd.openxmlformats-officedocument.wordprocessingml.document' />
				<mimeMap fileExtension='.dot' mimeType='application/msword' />
				<mimeMap fileExtension='.dotm' mimeType='application/vnd.ms-word.template.macroEnabled.12' />
				<mimeMap fileExtension='.dotx' mimeType='application/vnd.openxmlformats-officedocument.wordprocessingml.template' />
				<mimeMap fileExtension='.dsp' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.dtd' mimeType='text/xml' />
				<mimeMap fileExtension='.dvi' mimeType='application/x-dvi' />
				<mimeMap fileExtension='.dvr-ms' mimeType='video/x-ms-dvr' />
				<mimeMap fileExtension='.dwf' mimeType='drawing/x-dwf' />
				<mimeMap fileExtension='.dwp' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.dxr' mimeType='application/x-director' />
				<mimeMap fileExtension='.eml' mimeType='message/rfc822' />
				<mimeMap fileExtension='.emz' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.eot' mimeType='application/vnd.ms-fontobject' />
				<mimeMap fileExtension='.eps' mimeType='application/postscript' />
				<mimeMap fileExtension='.etx' mimeType='text/x-setext' />
				<mimeMap fileExtension='.evy' mimeType='application/envoy' />
				<mimeMap fileExtension='.exe' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.exe.config' mimeType='text/xml' />
				<mimeMap fileExtension='.fdf' mimeType='application/vnd.fdf' />
				<mimeMap fileExtension='.fif' mimeType='application/fractals' />
				<mimeMap fileExtension='.fla' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.flr' mimeType='x-world/x-vrml' />
				<mimeMap fileExtension='.flv' mimeType='video/x-flv' />
				<mimeMap fileExtension='.gif' mimeType='image/gif' />
				<mimeMap fileExtension='.gtar' mimeType='application/x-gtar' />
				<mimeMap fileExtension='.gz' mimeType='application/x-gzip' />
				<mimeMap fileExtension='.h' mimeType='text/plain' />
				<mimeMap fileExtension='.hdf' mimeType='application/x-hdf' />
				<mimeMap fileExtension='.hdml' mimeType='text/x-hdml' />
				<mimeMap fileExtension='.hhc' mimeType='application/x-oleobject' />
				<mimeMap fileExtension='.hhk' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.hhp' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.hlp' mimeType='application/winhlp' />
				<mimeMap fileExtension='.hqx' mimeType='application/mac-binhex40' />
				<mimeMap fileExtension='.hta' mimeType='application/hta' />
				<mimeMap fileExtension='.htc' mimeType='text/x-component' />
				<mimeMap fileExtension='.htm' mimeType='text/html' />
				<mimeMap fileExtension='.html' mimeType='text/html' />
				<mimeMap fileExtension='.htt' mimeType='text/webviewhtml' />
				<mimeMap fileExtension='.hxt' mimeType='text/html' />
				<mimeMap fileExtension='.ical' mimeType='text/calendar' />
				<mimeMap fileExtension='.icalendar' mimeType='text/calendar' />
				<mimeMap fileExtension='.ico' mimeType='image/x-icon' />
				<mimeMap fileExtension='.ics' mimeType='text/calendar' />
				<mimeMap fileExtension='.ief' mimeType='image/ief' />
				<mimeMap fileExtension='.ifb' mimeType='text/calendar' />
				<mimeMap fileExtension='.iii' mimeType='application/x-iphone' />
				<mimeMap fileExtension='.inf' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.ins' mimeType='application/x-internet-signup' />
				<mimeMap fileExtension='.isp' mimeType='application/x-internet-signup' />
				<mimeMap fileExtension='.IVF' mimeType='video/x-ivf' />
				<mimeMap fileExtension='.jar' mimeType='application/java-archive' />
				<mimeMap fileExtension='.java' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.jck' mimeType='application/liquidmotion' />
				<mimeMap fileExtension='.jcz' mimeType='application/liquidmotion' />
				<mimeMap fileExtension='.jfif' mimeType='image/pjpeg' />
				<mimeMap fileExtension='.jpb' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.jpe' mimeType='image/jpeg' />
				<mimeMap fileExtension='.jpeg' mimeType='image/jpeg' />
				<mimeMap fileExtension='.jpg' mimeType='image/jpeg' />
				<mimeMap fileExtension='.js' mimeType='application/javascript' />
				<mimeMap fileExtension='.jsx' mimeType='text/jscript' />
				<mimeMap fileExtension='.latex' mimeType='application/x-latex' />
				<mimeMap fileExtension='.lit' mimeType='application/x-ms-reader' />
				<mimeMap fileExtension='.lpk' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.lsf' mimeType='video/x-la-asf' />
				<mimeMap fileExtension='.lsx' mimeType='video/x-la-asf' />
				<mimeMap fileExtension='.lzh' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.m13' mimeType='application/x-msmediaview' />
				<mimeMap fileExtension='.m14' mimeType='application/x-msmediaview' />
				<mimeMap fileExtension='.m1v' mimeType='video/mpeg' />
				<mimeMap fileExtension='.m2ts' mimeType='video/vnd.dlna.mpeg-tts' />
				<mimeMap fileExtension='.m3u' mimeType='audio/x-mpegurl' />
				<mimeMap fileExtension='.m4a' mimeType='audio/mp4' />
				<mimeMap fileExtension='.m4v' mimeType='video/mp4' />
				<mimeMap fileExtension='.man' mimeType='application/x-troff-man' />
				<mimeMap fileExtension='.manifest' mimeType='application/x-ms-manifest' />
				<mimeMap fileExtension='.map' mimeType='text/plain' />
				<mimeMap fileExtension='.mdb' mimeType='application/x-msaccess' />
				<mimeMap fileExtension='.mdp' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.me' mimeType='application/x-troff-me' />
				<mimeMap fileExtension='.mht' mimeType='message/rfc822' />
				<mimeMap fileExtension='.mhtml' mimeType='message/rfc822' />
				<mimeMap fileExtension='.mid' mimeType='audio/mid' />
				<mimeMap fileExtension='.midi' mimeType='audio/mid' />
				<mimeMap fileExtension='.mix' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.mmf' mimeType='application/x-smaf' />
				<mimeMap fileExtension='.mno' mimeType='text/xml' />
				<mimeMap fileExtension='.mny' mimeType='application/x-msmoney' />
				<mimeMap fileExtension='.mov' mimeType='video/quicktime' />
				<mimeMap fileExtension='.movie' mimeType='video/x-sgi-movie' />
				<mimeMap fileExtension='.mp2' mimeType='video/mpeg' />
				<mimeMap fileExtension='.mp3' mimeType='audio/mpeg' />
				<mimeMap fileExtension='.mp4' mimeType='video/mp4' />
				<mimeMap fileExtension='.mp4v' mimeType='video/mp4' />
				<mimeMap fileExtension='.mpa' mimeType='video/mpeg' />
				<mimeMap fileExtension='.mpe' mimeType='video/mpeg' />
				<mimeMap fileExtension='.mpeg' mimeType='video/mpeg' />
				<mimeMap fileExtension='.mpg' mimeType='video/mpeg' />
				<mimeMap fileExtension='.mpp' mimeType='application/vnd.ms-project' />
				<mimeMap fileExtension='.mpv2' mimeType='video/mpeg' />
				<mimeMap fileExtension='.ms' mimeType='application/x-troff-ms' />
				<mimeMap fileExtension='.msi' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.mso' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.mvb' mimeType='application/x-msmediaview' />
				<mimeMap fileExtension='.mvc' mimeType='application/x-miva-compiled' />
				<mimeMap fileExtension='.nc' mimeType='application/x-netcdf' />
				<mimeMap fileExtension='.nsc' mimeType='video/x-ms-asf' />
				<mimeMap fileExtension='.nws' mimeType='message/rfc822' />
				<mimeMap fileExtension='.ocx' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.oda' mimeType='application/oda' />
				<mimeMap fileExtension='.odc' mimeType='text/x-ms-odc' />
				<mimeMap fileExtension='.ods' mimeType='application/oleobject' />
				<mimeMap fileExtension='.oga' mimeType='audio/ogg' />
				<mimeMap fileExtension='.ogg' mimeType='video/ogg' />
				<mimeMap fileExtension='.ogv' mimeType='video/ogg' />
				<mimeMap fileExtension='.ogx' mimeType='application/ogg' />
				<mimeMap fileExtension='.one' mimeType='application/onenote' />
				<mimeMap fileExtension='.onea' mimeType='application/onenote' />
				<mimeMap fileExtension='.onetoc' mimeType='application/onenote' />
				<mimeMap fileExtension='.onetoc2' mimeType='application/onenote' />
				<mimeMap fileExtension='.onetmp' mimeType='application/onenote' />
				<mimeMap fileExtension='.onepkg' mimeType='application/onenote' />
				<mimeMap fileExtension='.osdx' mimeType='application/opensearchdescription+xml' />
				<mimeMap fileExtension='.otf' mimeType='font/otf' />
				<mimeMap fileExtension='.p10' mimeType='application/pkcs10' />
				<mimeMap fileExtension='.p12' mimeType='application/x-pkcs12' />
				<mimeMap fileExtension='.p7b' mimeType='application/x-pkcs7-certificates' />
				<mimeMap fileExtension='.p7c' mimeType='application/pkcs7-mime' />
				<mimeMap fileExtension='.p7m' mimeType='application/pkcs7-mime' />
				<mimeMap fileExtension='.p7r' mimeType='application/x-pkcs7-certreqresp' />
				<mimeMap fileExtension='.p7s' mimeType='application/pkcs7-signature' />
				<mimeMap fileExtension='.pbm' mimeType='image/x-portable-bitmap' />
				<mimeMap fileExtension='.pcx' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.pcz' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.pdf' mimeType='application/pdf' />
				<mimeMap fileExtension='.pfb' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.pfm' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.pfx' mimeType='application/x-pkcs12' />
				<mimeMap fileExtension='.pgm' mimeType='image/x-portable-graymap' />
				<mimeMap fileExtension='.pko' mimeType='application/vnd.ms-pki.pko' />
				<mimeMap fileExtension='.pma' mimeType='application/x-perfmon' />
				<mimeMap fileExtension='.pmc' mimeType='application/x-perfmon' />
				<mimeMap fileExtension='.pml' mimeType='application/x-perfmon' />
				<mimeMap fileExtension='.pmr' mimeType='application/x-perfmon' />
				<mimeMap fileExtension='.pmw' mimeType='application/x-perfmon' />
				<mimeMap fileExtension='.png' mimeType='image/png' />
				<mimeMap fileExtension='.pnm' mimeType='image/x-portable-anymap' />
				<mimeMap fileExtension='.pnz' mimeType='image/png' />
				<mimeMap fileExtension='.pot' mimeType='application/vnd.ms-powerpoint' />
				<mimeMap fileExtension='.potm' mimeType='application/vnd.ms-powerpoint.template.macroEnabled.12' />
				<mimeMap fileExtension='.potx' mimeType='application/vnd.openxmlformats-officedocument.presentationml.template' />
				<mimeMap fileExtension='.ppam' mimeType='application/vnd.ms-powerpoint.addin.macroEnabled.12' />
				<mimeMap fileExtension='.ppm' mimeType='image/x-portable-pixmap' />
				<mimeMap fileExtension='.pps' mimeType='application/vnd.ms-powerpoint' />
				<mimeMap fileExtension='.ppsm' mimeType='application/vnd.ms-powerpoint.slideshow.macroEnabled.12' />
				<mimeMap fileExtension='.ppsx' mimeType='application/vnd.openxmlformats-officedocument.presentationml.slideshow' />
				<mimeMap fileExtension='.ppt' mimeType='application/vnd.ms-powerpoint' />
				<mimeMap fileExtension='.pptm' mimeType='application/vnd.ms-powerpoint.presentation.macroEnabled.12' />
				<mimeMap fileExtension='.pptx' mimeType='application/vnd.openxmlformats-officedocument.presentationml.presentation' />
				<mimeMap fileExtension='.prf' mimeType='application/pics-rules' />
				<mimeMap fileExtension='.prm' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.prx' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.ps' mimeType='application/postscript' />
				<mimeMap fileExtension='.psd' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.psm' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.psp' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.pub' mimeType='application/x-mspublisher' />
				<mimeMap fileExtension='.qt' mimeType='video/quicktime' />
				<mimeMap fileExtension='.qtl' mimeType='application/x-quicktimeplayer' />
				<mimeMap fileExtension='.qxd' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.ra' mimeType='audio/x-pn-realaudio' />
				<mimeMap fileExtension='.ram' mimeType='audio/x-pn-realaudio' />
				<mimeMap fileExtension='.rar' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.ras' mimeType='image/x-cmu-raster' />
				<mimeMap fileExtension='.rf' mimeType='image/vnd.rn-realflash' />
				<mimeMap fileExtension='.rgb' mimeType='image/x-rgb' />
				<mimeMap fileExtension='.rm' mimeType='application/vnd.rn-realmedia' />
				<mimeMap fileExtension='.rmi' mimeType='audio/mid' />
				<mimeMap fileExtension='.roff' mimeType='application/x-troff' />
				<mimeMap fileExtension='.rpm' mimeType='audio/x-pn-realaudio-plugin' />
				<mimeMap fileExtension='.rtf' mimeType='application/rtf' />
				<mimeMap fileExtension='.rtx' mimeType='text/richtext' />
				<mimeMap fileExtension='.scd' mimeType='application/x-msschedule' />
				<mimeMap fileExtension='.sct' mimeType='text/scriptlet' />
				<mimeMap fileExtension='.sea' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.setpay' mimeType='application/set-payment-initiation' />
				<mimeMap fileExtension='.setreg' mimeType='application/set-registration-initiation' />
				<mimeMap fileExtension='.sgml' mimeType='text/sgml' />
				<mimeMap fileExtension='.sh' mimeType='application/x-sh' />
				<mimeMap fileExtension='.shar' mimeType='application/x-shar' />
				<mimeMap fileExtension='.sit' mimeType='application/x-stuffit' />
				<mimeMap fileExtension='.sldm' mimeType='application/vnd.ms-powerpoint.slide.macroEnabled.12' />
				<mimeMap fileExtension='.sldx' mimeType='application/vnd.openxmlformats-officedocument.presentationml.slide' />
				<mimeMap fileExtension='.smd' mimeType='audio/x-smd' />
				<mimeMap fileExtension='.smi' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.smx' mimeType='audio/x-smd' />
				<mimeMap fileExtension='.smz' mimeType='audio/x-smd' />
				<mimeMap fileExtension='.snd' mimeType='audio/basic' />
				<mimeMap fileExtension='.snp' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.spc' mimeType='application/x-pkcs7-certificates' />
				<mimeMap fileExtension='.spl' mimeType='application/futuresplash' />
				<mimeMap fileExtension='.spx' mimeType='audio/ogg' />
				<mimeMap fileExtension='.src' mimeType='application/x-wais-source' />
				<mimeMap fileExtension='.ssm' mimeType='application/streamingmedia' />
				<mimeMap fileExtension='.sst' mimeType='application/vnd.ms-pki.certstore' />
				<mimeMap fileExtension='.stl' mimeType='application/vnd.ms-pki.stl' />
				<mimeMap fileExtension='.sv4cpio' mimeType='application/x-sv4cpio' />
				<mimeMap fileExtension='.sv4crc' mimeType='application/x-sv4crc' />
				<mimeMap fileExtension='.svg' mimeType='image/svg+xml' />
				<mimeMap fileExtension='.svgz' mimeType='image/svg+xml' />
				<mimeMap fileExtension='.swf' mimeType='application/x-shockwave-flash' />
				<mimeMap fileExtension='.t' mimeType='application/x-troff' />
				<mimeMap fileExtension='.tar' mimeType='application/x-tar' />
				<mimeMap fileExtension='.tcl' mimeType='application/x-tcl' />
				<mimeMap fileExtension='.tex' mimeType='application/x-tex' />
				<mimeMap fileExtension='.texi' mimeType='application/x-texinfo' />
				<mimeMap fileExtension='.texinfo' mimeType='application/x-texinfo' />
				<mimeMap fileExtension='.tgz' mimeType='application/x-compressed' />
				<mimeMap fileExtension='.thmx' mimeType='application/vnd.ms-officetheme' />
				<mimeMap fileExtension='.thn' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.tif' mimeType='image/tiff' />
				<mimeMap fileExtension='.tiff' mimeType='image/tiff' />
				<mimeMap fileExtension='.toc' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.tr' mimeType='application/x-troff' />
				<mimeMap fileExtension='.trm' mimeType='application/x-msterminal' />
				<mimeMap fileExtension='.ts' mimeType='video/vnd.dlna.mpeg-tts' />
				<mimeMap fileExtension='.tsv' mimeType='text/tab-separated-values' />
				<mimeMap fileExtension='.ttf' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.tts' mimeType='video/vnd.dlna.mpeg-tts' />
				<mimeMap fileExtension='.txt' mimeType='text/plain' />
				<mimeMap fileExtension='.u32' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.uls' mimeType='text/iuls' />
				<mimeMap fileExtension='.ustar' mimeType='application/x-ustar' />
				<mimeMap fileExtension='.vbs' mimeType='text/vbscript' />
				<mimeMap fileExtension='.vcf' mimeType='text/x-vcard' />
				<mimeMap fileExtension='.vcs' mimeType='text/plain' />
				<mimeMap fileExtension='.vdx' mimeType='application/vnd.ms-visio.viewer' />
				<mimeMap fileExtension='.vml' mimeType='text/xml' />
				<mimeMap fileExtension='.vsd' mimeType='application/vnd.visio' />
				<mimeMap fileExtension='.vss' mimeType='application/vnd.visio' />
				<mimeMap fileExtension='.vst' mimeType='application/vnd.visio' />
				<mimeMap fileExtension='.vsto' mimeType='application/x-ms-vsto' />
				<mimeMap fileExtension='.vsw' mimeType='application/vnd.visio' />
				<mimeMap fileExtension='.vsx' mimeType='application/vnd.visio' />
				<mimeMap fileExtension='.vtx' mimeType='application/vnd.visio' />
				<mimeMap fileExtension='.wav' mimeType='audio/wav' />
				<mimeMap fileExtension='.wax' mimeType='audio/x-ms-wax' />
				<mimeMap fileExtension='.wbmp' mimeType='image/vnd.wap.wbmp' />
				<mimeMap fileExtension='.wcm' mimeType='application/vnd.ms-works' />
				<mimeMap fileExtension='.wdb' mimeType='application/vnd.ms-works' />
				<mimeMap fileExtension='.webm' mimeType='video/webm' />
				<mimeMap fileExtension='.wks' mimeType='application/vnd.ms-works' />
				<mimeMap fileExtension='.wm' mimeType='video/x-ms-wm' />
				<mimeMap fileExtension='.wma' mimeType='audio/x-ms-wma' />
				<mimeMap fileExtension='.wmd' mimeType='application/x-ms-wmd' />
				<mimeMap fileExtension='.wmf' mimeType='application/x-msmetafile' />
				<mimeMap fileExtension='.wml' mimeType='text/vnd.wap.wml' />
				<mimeMap fileExtension='.wmlc' mimeType='application/vnd.wap.wmlc' />
				<mimeMap fileExtension='.wmls' mimeType='text/vnd.wap.wmlscript' />
				<mimeMap fileExtension='.wmlsc' mimeType='application/vnd.wap.wmlscriptc' />
				<mimeMap fileExtension='.wmp' mimeType='video/x-ms-wmp' />
				<mimeMap fileExtension='.wmv' mimeType='video/x-ms-wmv' />
				<mimeMap fileExtension='.wmx' mimeType='video/x-ms-wmx' />
				<mimeMap fileExtension='.wmz' mimeType='application/x-ms-wmz' />
				<mimeMap fileExtension='.woff' mimeType='font/x-woff' />
				<mimeMap fileExtension='.wps' mimeType='application/vnd.ms-works' />
				<mimeMap fileExtension='.wri' mimeType='application/x-mswrite' />
				<mimeMap fileExtension='.wrl' mimeType='x-world/x-vrml' />
				<mimeMap fileExtension='.wrz' mimeType='x-world/x-vrml' />
				<mimeMap fileExtension='.wsdl' mimeType='text/xml' />
				<mimeMap fileExtension='.wtv' mimeType='video/x-ms-wtv' />
				<mimeMap fileExtension='.wvx' mimeType='video/x-ms-wvx' />
				<mimeMap fileExtension='.x' mimeType='application/directx' />
				<mimeMap fileExtension='.xaf' mimeType='x-world/x-vrml' />
				<mimeMap fileExtension='.xaml' mimeType='application/xaml+xml' />
				<mimeMap fileExtension='.xap' mimeType='application/x-silverlight-app' />
				<mimeMap fileExtension='.xbap' mimeType='application/x-ms-xbap' />
				<mimeMap fileExtension='.xbm' mimeType='image/x-xbitmap' />
				<mimeMap fileExtension='.xdr' mimeType='text/plain' />
				<mimeMap fileExtension='.xht' mimeType='application/xhtml+xml' />
				<mimeMap fileExtension='.xhtml' mimeType='application/xhtml+xml' />
				<mimeMap fileExtension='.xla' mimeType='application/vnd.ms-excel' />
				<mimeMap fileExtension='.xlam' mimeType='application/vnd.ms-excel.addin.macroEnabled.12' />
				<mimeMap fileExtension='.xlc' mimeType='application/vnd.ms-excel' />
				<mimeMap fileExtension='.xlm' mimeType='application/vnd.ms-excel' />
				<mimeMap fileExtension='.xls' mimeType='application/vnd.ms-excel' />
				<mimeMap fileExtension='.xlsb' mimeType='application/vnd.ms-excel.sheet.binary.macroEnabled.12' />
				<mimeMap fileExtension='.xlsm' mimeType='application/vnd.ms-excel.sheet.macroEnabled.12' />
				<mimeMap fileExtension='.xlsx' mimeType='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' />
				<mimeMap fileExtension='.xlt' mimeType='application/vnd.ms-excel' />
				<mimeMap fileExtension='.xltm' mimeType='application/vnd.ms-excel.template.macroEnabled.12' />
				<mimeMap fileExtension='.xltx' mimeType='application/vnd.openxmlformats-officedocument.spreadsheetml.template' />
				<mimeMap fileExtension='.xlw' mimeType='application/vnd.ms-excel' />
				<mimeMap fileExtension='.xml' mimeType='text/xml' />
				<mimeMap fileExtension='.xof' mimeType='x-world/x-vrml' />
				<mimeMap fileExtension='.xpm' mimeType='image/x-xpixmap' />
				<mimeMap fileExtension='.xps' mimeType='application/vnd.ms-xpsdocument' />
				<mimeMap fileExtension='.xsd' mimeType='text/xml' />
				<mimeMap fileExtension='.xsf' mimeType='text/xml' />
				<mimeMap fileExtension='.xsl' mimeType='text/xml' />
				<mimeMap fileExtension='.xslt' mimeType='text/xml' />
				<mimeMap fileExtension='.xsn' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.xtp' mimeType='application/octet-stream' />
				<mimeMap fileExtension='.xwd' mimeType='image/x-xwindowdump' />
				<mimeMap fileExtension='.z' mimeType='application/x-compress' />
				<mimeMap fileExtension='.zip' mimeType='application/x-zip-compressed' />
			</staticContent>");

		private Dictionary<string, string> dictionary = new Dictionary<string,string>();

		public StaticMimeTypeProvider() {
			foreach (XElement child in Config.Nodes()) dictionary.Add(child.Attribute("fileExtension").Value.Substring(1).ToLower(), child.Attribute("mimeType").Value);
		}

		public override string OfFile(string filename) {
			var ext = Paths.Extension(filename).ToLower();
			string mime = null;
			if (!dictionary.TryGetValue(ext, out mime)) return "application/octet-stream";
			return mime;
		}

		public override string OfFile(System.IO.Stream stream, string filename) { return OfFile(filename); }

		public override string OfExtension(string filename) { return OfFile(filename); }
	}

}
