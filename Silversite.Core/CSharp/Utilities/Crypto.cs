using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Text.RegularExpressions;

namespace Silversite.Services {

	public class Crypto {

		private const int TOKEN_SIZE_IN_BYTES = 16;
		private const int PBKDF2_ITER_COUNT = 1000;
		// default for Rfc2898DeriveBytes
		private const int PBKDF2_SUBKEY_LENGTH = 256 / 8;
		// 256 bits
		private const int SALT_SIZE = 128 / 8;
		// 128 bits
		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "byte", Justification = "It really is a byte length")]
		internal static byte[] GenerateSaltInternal(int byteLength = SALT_SIZE) {
			byte[] buf = new byte[byteLength];
			using (var rng = new RNGCryptoServiceProvider()) {
				rng.GetBytes(buf);
			}
			return buf;
		}

		static internal string GenerateToken() {
			byte[] tokenBytes = new byte[TOKEN_SIZE_IN_BYTES];
			using (RNGCryptoServiceProvider prng = new RNGCryptoServiceProvider()) {
				prng.GetBytes(tokenBytes);
				return Convert.ToBase64String(tokenBytes);
			}
		}

		public static string GenerateSalt(int byteLength = SALT_SIZE) {
			return Convert.ToBase64String(GenerateSaltInternal(byteLength));
		}

		public static string Hash(string input, string algorithm = "sha256") {
			if (input == null) {
				throw new ArgumentNullException("input");
			}

			return Hash(Encoding.UTF8.GetBytes(input), algorithm);
		}

		public static string Hash(byte[] input, string algorithm = "sha256") {
			if (input == null) {
				throw new ArgumentNullException("input");
			}

			using (HashAlgorithm alg = HashAlgorithm.Create(algorithm)) {
				if (alg != null) {
					byte[] hashData = alg.ComputeHash(input);
					return BinaryToHex(hashData);
				} else {
					throw new InvalidOperationException(String.Format(string.Format("Not supported hash algorhitm {0}", algorithm)));
				}
			}
		}

		public static string SHA1(string input) {
			return Hash(input, "sha1");
		}

		public static string SHA256(string input) {
			return Hash(input, "sha256");
		}

		// =======================
		//         * HASHED PASSWORD FORMATS
		//         * =======================
		//         * 
		//         * Version 0:
		//         * PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.
		//         * (See also: SDL crypto guidelines v5.1, Part III)
		//         * Format: { 0x00, salt, subkey }
		//         


		public static string HashPassword(string password) {
			if (password == null) {
				throw new ArgumentNullException("password");
			}

			// Produce a version 0 (see comment above) password hash.
			byte[] salt;
			byte[] subkey;
			using (var deriveBytes = new Rfc2898DeriveBytes(password, SALT_SIZE, PBKDF2_ITER_COUNT)) {
				salt = deriveBytes.Salt;
				subkey = deriveBytes.GetBytes(PBKDF2_SUBKEY_LENGTH);
			}

			byte[] outputBytes = new byte[1 + SALT_SIZE + PBKDF2_SUBKEY_LENGTH];
			Buffer.BlockCopy(salt, 0, outputBytes, 1, SALT_SIZE);
			Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SALT_SIZE, PBKDF2_SUBKEY_LENGTH);
			return Convert.ToBase64String(outputBytes);
		}

		// hashedPassword must be of the format of HashWithPassword (salt + Hash(salt+input)
		public static bool VerifyHashedPassword(string hashedPassword, string password) {
			if (hashedPassword == null) {
				throw new ArgumentNullException("hashedPassword");
			}
			if (password == null) {
				throw new ArgumentNullException("password");
			}

			byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

			// Verify a version 0 (see comment above) password hash.

			if (hashedPasswordBytes.Length != (1 + SALT_SIZE + PBKDF2_SUBKEY_LENGTH) || hashedPasswordBytes[0] != (byte)0x00) {
				// Wrong length or version header.
				return false;
			}

			byte[] salt = new byte[SALT_SIZE];
			Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, SALT_SIZE);
			byte[] storedSubkey = new byte[PBKDF2_SUBKEY_LENGTH];
			Buffer.BlockCopy(hashedPasswordBytes, 1 + SALT_SIZE, storedSubkey, 0, PBKDF2_SUBKEY_LENGTH);

			byte[] generatedSubkey;
			using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, PBKDF2_ITER_COUNT)) {
				generatedSubkey = deriveBytes.GetBytes(PBKDF2_SUBKEY_LENGTH);
			}
			return ByteArraysEqual(storedSubkey, generatedSubkey);
		}

		internal static string BinaryToHex(byte[] data) {
			char[] hex = new char[data.Length * 2];

			for (int iter = 0; iter < data.Length; iter++) {
				byte hexChar = ((byte)(data[iter] >> 4));
				hex[iter * 2] = (char)(hexChar > 9 ? hexChar + 0x37 : hexChar + 0x30);
				hexChar = ((byte)(data[iter] & 0xF));
				hex[iter * 2 + 1] = (char)(hexChar > 9 ? hexChar + 0x37 : hexChar + 0x30);
			}
			return new string(hex);
		}

		// Compares two byte arrays for equality. The method is specifically written so that the loop is not optimized.
		[MethodImpl(MethodImplOptions.NoOptimization)]
		private static bool ByteArraysEqual(byte[] a, byte[] b) {
			if (Object.ReferenceEquals(a, b)) {
				return true;
			}

			if (a == null || b == null || a.Length != b.Length) {
				return false;
			}

			bool areSame = true;
			for (int i = 0; i <= a.Length - 1; i++) {
				areSame = areSame & (a[i] == b[i]);
			}
			return areSame;
		}

		public static byte[] EncryptPassword(string password) {
			return SymmetricEncryptionUtility.EncryptData(password);
		}

		public static string DecryptPassword(byte[] data) {
			return SymmetricEncryptionUtility.DecryptData(data);
		}


		[Serializable]
		class EncryptedObject {
			public object Object { get; set; }
			public string[] Keys { get; set; }
		}

		public static string Encrypt(object obj, params string[] keys) {
			return HttpUtility.UrlEncode(Convert.ToBase64String(Services.Hash.ToBytes(MachineKey.Encode(Services.Hash.ToBytes(new EncryptedObject { Object = obj, Keys = keys }), MachineKeyProtection.All))));
		}

		public static object Decrypt(string text, params string[] keys) {
			try {
				var eobj = (EncryptedObject)Services.Hash.ToObject(MachineKey.Decode((string)Services.Hash.ToObject(Convert.FromBase64String(HttpUtility.UrlDecode(text))), MachineKeyProtection.All));
				if (eobj.Keys.All(key => keys.Any(key2 => key == key2))) return eobj.Object;
			} catch (Exception ex) { }
			return null;
		}

		public static string Secret(TimeSpan maxAge = default(TimeSpan), params string[] keys) { return Encrypt(maxAge == default(TimeSpan) ? DateTime.MaxValue : DateTime.Now + maxAge, keys); }

		public static bool Secure(string secret, params string[] keys) {
			var validUntil = (DateTime?)Decrypt(secret, keys);
			return validUntil.HasValue && validUntil.Value >= DateTime.Now;
		}

		public static string SecureUrl(string url, TimeSpan maxAge = default(TimeSpan), params string[] keys) {
			url = url.Replace("&amp;", "&");
			return url + (url.Contains('?') ? "&" : "?") + "!!" + Encrypt(maxAge == default(TimeSpan) ? DateTime.MaxValue : DateTime.Now + maxAge, keys.Prepend(url).ToArray());
		}

		static Regex securetokens = new Regex("(\\?|&|&amp;)!![^&]+$");

		public static bool SecureRequest(params string[] keys) {
			try {
				var url = HttpContext.Current.Request.Url.AbsoluteUri;
				var match = securetokens.Match(url);
				if (!match.Success) return false;
				var rawurl = url.Replace(match.Value, "");
				var secret = match.Value.Substring(3);
				return Secure(secret, keys.Prepend(rawurl).ToArray());
			} catch {
				return false;
			}
		}

	}
}