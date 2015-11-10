// davidegli

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Silversite.Services {

	/// <summary>
	/// A class that provides asymmetric encryption of strings.
	/// </summary>
	public static class AsymmetricEncryptionUtility {

		/// <summary>
		/// Generates a key file with an asymetric key.
		/// </summary>
		/// <param name="targetFile">The file name of the key file to generate.</param>
		/// <returns></returns>
		public static string GenerateKey(string targetFile) {
			RSACryptoServiceProvider Algorithm = new RSACryptoServiceProvider();

			// Save the private key
			string CompleteKey = Algorithm.ToXmlString(true);
			byte[] KeyBytes = Encoding.UTF8.GetBytes(CompleteKey);

			KeyBytes = ProtectedData.Protect(KeyBytes,
								 null, DataProtectionScope.LocalMachine);

			using (FileStream fs = new FileStream(targetFile, FileMode.Create)) {
				fs.Write(KeyBytes, 0, KeyBytes.Length);
			}

			// Return the public key
			return Algorithm.ToXmlString(false);
		}

		/// <summary>
		/// Reads a key file
		/// </summary>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="keyFile">The filename of the key file.</param>
		private static void ReadKey(RSACryptoServiceProvider algorithm, string keyFile) {
			byte[] KeyBytes;

			using (FileStream fs = new FileStream(keyFile, FileMode.Open)) {
				KeyBytes = new byte[fs.Length];
				fs.Read(KeyBytes, 0, (int)fs.Length);
			}

			KeyBytes = ProtectedData.Unprotect(KeyBytes,
								 null, DataProtectionScope.LocalMachine);

			algorithm.FromXmlString(Encoding.UTF8.GetString(KeyBytes));
		}

		/// <summary>
		/// Encrypts a string using a public key string.
		/// </summary>
		/// <param name="data">The string to encrypt.</param>
		/// <param name="publicKey">The public key.</param>
		/// <returns>A byte array containing the encrypted data.</returns>
		public static byte[] EncryptData(string data, string publicKey) {
			// Create the algorithm based on the key
			RSACryptoServiceProvider Algorithm = new RSACryptoServiceProvider();
			Algorithm.FromXmlString(publicKey);

			// Now encrypt the data
			return Algorithm.Encrypt(
							Encoding.UTF8.GetBytes(data), true);
		}

		/// <summary>
		/// Decrypts a byte array into a string using a key file.
		/// </summary>
		/// <param name="data">The byte array with the encrypted data.</param>
		/// <param name="keyFile">The key file.</param>
		/// <returns>The decrypted string.</returns>
		public static string DecryptData(byte[] data, string keyFile) {
			RSACryptoServiceProvider Algorithm = new RSACryptoServiceProvider();
			ReadKey(Algorithm, keyFile);

			byte[] ClearData = Algorithm.Decrypt(data, true);
			return Convert.ToString(
							Encoding.UTF8.GetString(ClearData));
		}
	}
}
