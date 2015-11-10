// davidegli

using System;
using System.Collections.Generic;
using System.Text;

namespace Silversite.Services {

	/// <summary>
	/// A class for Encoding/Decoding to Hex notation.
	/// </summary>
	public static class HexEncoding {

		/// <summary>
		/// Converts a data byte array into a string of a hex number corresponding to the data.
		/// </summary>
		/// <param name="data">The data byte array</param>
		/// <returns>A string with a hex number representing the data.</returns>
		public static string GetString(byte[] data) {
			StringBuilder Results = new StringBuilder();
			foreach (byte b in data) {
				Results.Append(b.ToString("X2"));
			}

			return Results.ToString();
		}

		/// <summary>
		/// Converts a string hex number to a byte array.
		/// </summary>
		/// <param name="data">The hex number string.</param>
		/// <returns>The data byte array.</returns>
		public static byte[] GetBytes(string data) {
			// GetString encodes the hex-numbers with two digits
			byte[] Results = new byte[data.Length / 2];
			for (int i = 0; i < data.Length; i += 2) {
				Results[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);
			}

			return Results;
		}
	}
}
