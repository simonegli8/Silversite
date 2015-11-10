using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;

namespace Silversite.Services {
	
	public class OCR: StaticService<OCR, OCRProvider> {

		public string Recognise(string image, string lang, double scale = 1.0, double contrast = 0.0, string whitelist = null) {
			if (HasProvider) return Provider.Recognise(image, lang, scale, contrast);
			throw new NotSupportedException("OCR has no provider installed.");
		}
	
	}

	public abstract class OCRProvider : Provider<OCR> {
		public abstract string Recognise(string image, string lang, double scale, double contrast, string whitelist = null);
	}

}