// davidegli

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Web.Hosting;

namespace Silversite.Services {

	/// <summary>
	/// A class that provides symmetric encryption of strings.
	/// </summary>
	public static class SymmetricEncryptionUtility {

		private static bool _ProtectKey;
		private static string _AlgorithmName = "DES";

		public static string AlgorithmName {
			get { return _AlgorithmName; }
			set { _AlgorithmName = value; }
		}

		public static bool ProtectKey {
			get { return _ProtectKey; }
			set { _ProtectKey = value; }
		}

		/// <summary>
		/// Creates a key file.
		/// </summary>
		/// <param name="targetFile">The key file's filename.</param>
		public static void GenerateKey(string targetFile) {
			// Create the algorithm
			SymmetricAlgorithm Algorithm = SymmetricAlgorithm.Create(AlgorithmName);
			Algorithm.GenerateKey();

			// No get the key
			byte[] Key = Algorithm.Key;

			if (ProtectKey) {
				// Use DPAPI to encrypt key
				Key = ProtectedData.Protect(
					 Key, null, DataProtectionScope.LocalMachine);
			}

			// Store the key in a file called key.config
			using (FileStream fs = new FileStream(targetFile, FileMode.Create)) {
				fs.Write(Key, 0, Key.Length);
			}
		}

		/// <summary>
		/// Reads a key file.
		/// </summary>
		/// <param name="algorithm">The encryption algortihm.</param>
		/// <param name="keyFile">The key file.</param>
		public static void ReadKey(SymmetricAlgorithm algorithm, string keyFile) {
			byte[] Key;

			using (FileStream fs = new FileStream(keyFile, FileMode.Open)) {
				Key = new byte[fs.Length];
				fs.Read(Key, 0, (int)fs.Length);
			}

			if (ProtectKey)
				algorithm.Key = ProtectedData.Unprotect(Key, null, DataProtectionScope.LocalMachine);
			else
				algorithm.Key = Key;
		}

		/// <summary>
		/// Encrypts a string using a key file.
		/// </summary>
		/// <param name="data">The string to encrypt.</param>
		/// <param name="keyFile">The key file.</param>
		/// <returns>A byte array with the encrypted data.</returns>
		public static byte[] EncryptData(string data, string keyFile) {
			// Convert string data to byte array
			byte[] ClearData = Encoding.UTF8.GetBytes(data);

			// Now Create the algorithm
			SymmetricAlgorithm Algorithm = SymmetricAlgorithm.Create(AlgorithmName);
			ReadKey(Algorithm, keyFile);

			// Encrypt information
			MemoryStream Target = new MemoryStream();

			// Append IV
			Algorithm.GenerateIV();
			Target.Write(Algorithm.IV, 0, Algorithm.IV.Length);

			// Encrypt actual data
			CryptoStream cs = new CryptoStream(Target, Algorithm.CreateEncryptor(), CryptoStreamMode.Write);
			cs.Write(ClearData, 0, ClearData.Length);
			cs.FlushFinalBlock();

			// Output the bytes of the encrypted array to the textbox
			return Target.ToArray();
		}

		/// <summary>
		/// Decrypts a byte array of enctrypted data to a string using a key file.
		/// </summary>
		/// <param name="data">The encrypted data array.</param>
		/// <param name="keyFile">The key file.</param>
		/// <returns>The decrypted string.</returns>
		public static string DecryptData(byte[] data, string keyFile) {
			// Now create the algorithm
			SymmetricAlgorithm Algorithm = SymmetricAlgorithm.Create(AlgorithmName);
			ReadKey(Algorithm, keyFile);

			// Decrypt information
			MemoryStream Target = new MemoryStream();

			// Read IV
			int ReadPos = 0;
			byte[] IV = new byte[Algorithm.IV.Length];
			Array.Copy(data, IV, IV.Length);
			Algorithm.IV = IV;
			ReadPos += Algorithm.IV.Length;

			CryptoStream cs = new CryptoStream(Target, Algorithm.CreateDecryptor(), CryptoStreamMode.Write);
			cs.Write(data, ReadPos, data.Length - ReadPos);
			cs.FlushFinalBlock();

			// Get the bytes from the memory stream and convert them to text
			return Encoding.UTF8.GetString(Target.ToArray());
		}

		/// <summary>
		/// The default key file app relative path.
		/// </summary>
		public const string DefaultKeyPath = Configuration.Section.ConfigRoot + "/key.config";

		/// <summary>
		/// The default key file filesystem path.
		/// </summary>
		public static string DefaultKeyFile { get { return HostingEnvironment.MapPath(DefaultKeyPath); } }

		/// <summary>
		/// Asserts there is a key file by creating one if it does not exist.
		/// </summary>
		public static void AssertKey() {
			var keyfile = DefaultKeyFile;
			if (!File.Exists(keyfile)) GenerateKey(keyfile);
		}

		/// <summary>
		/// Decrypts data using the default key.
		/// </summary>
		/// <param name="data">The data to decrypt.</param>
		/// <returns>The decrypted string.</returns>
		public static string DecryptData(byte[] data) {
			AssertKey();
			return DecryptData(data, DefaultKeyFile);
		}

		/// <summary>
		/// Encrypts data using the default key.
		/// </summary>
		/// <param name="data">The string to encrypt.</param>
		/// <returns>A byte array with the encrypted data.</returns>
		public static byte[] EncryptData(string data) {
			AssertKey();
			return EncryptData(data, DefaultKeyFile);
		}

	}
}
