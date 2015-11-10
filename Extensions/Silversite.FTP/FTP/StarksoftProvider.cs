/*
 *  Authors:  Benton Stark
 * 
 *  Copyright (c) 2007-2009 Starksoft, LLC (http://www.starksoft.com) 
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;
using System.Globalization;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using Starksoft.Net.Ftp;
using Silversite.Services.Ftp;


namespace Silversite.Ftp {

	public class StarksoftProvider: FtpProvider, IDisposable {

		public Starksoft.Net.Ftp.FtpClient FtpClient { get; set; }

		#region Contructors

		public override FtpProvider Client() { return new StarksoftProvider { FtpClient = new Starksoft.Net.Ftp.FtpClient() }; }
		public override FtpProvider Client(string host) { return new StarksoftProvider { FtpClient = new Starksoft.Net.Ftp.FtpClient(host) }; }
		public override FtpProvider Client(string host, int port) { return new StarksoftProvider { FtpClient = new Starksoft.Net.Ftp.FtpClient(host, port) }; }
		public override FtpProvider Client(string host, int port, FtpSecurityProtocol securityProtocol) { return new StarksoftProvider { FtpClient = new Starksoft.Net.Ftp.FtpClient(host, port, securityProtocol) }; }
		
		#endregion

		#region Public Properties
 
		public override TransferType FileTransferType { get { return FtpClient.FileTransferType; } set { FtpClient.FileTransferType = value; } }
		public override int FxpTransferTimeout { get { return FtpClient.FxpTransferTimeout; } set { FtpClient.FxpTransferTimeout = value; } }
		public override string CurrentDirectory { get { return FtpClient.CurrentDirectory; } }
		public override bool CompatibleMode { get; set; }
		public override int Port { get; set; }
		public override string Host { get; set; }
		public override FtpSecurityProtocol SecurityProtocol { get { return FtpClient.SecurityProtocol; } set { FtpClient.SecurityProtocol = value; } }
		public override X509CertificateCollection SecurityCertificates { get { return FtpClient.SecurityCertificates; } }
		public override bool IsCompressionEnabled { get { return FtpClient.IsCompressionEnabled; } set { FtpClient.IsCompressionEnabled = value; } }
		public override int MaxUploadSpeed { get { return FtpClient.MaxUploadSpeed; } set { FtpClient.MaxUploadSpeed = value; } }
		public override int MaxDownloadSpeed { get { return FtpClient.MaxDownloadSpeed; } set { FtpClient.MaxDownloadSpeed = value; } }
		public override int ServerTimeOffset { get { return FtpClient.ServerTimeOffset; } set { FtpClient.ServerTimeOffset = value; } }
		public override string ServerTimeString { get { return FtpClient.ServerTimeString; } }
		public override string RootDirectory { get { return FtpClient.RootDirectory; } }
		public override TransferMode DataTransferMode { get { return FtpClient.DataTransferMode; } set { FtpClient.DataTransferMode = value; } }
		public override HashingFunction AutoChecksumValidation { get { return FtpClient.AutoChecksumValidation; } set { FtpClient.AutoChecksumValidation = value; } }

		#endregion

		#region Public Methods

		public override void Open(string user, string password) { FtpClient.Open(user, password); }
		public override void Close() { FtpClient.Close(); }
		public override void ChangeDirectory(string path) { if (CompatibleMode) FtpClient.ChangeDirectoryMultiPath(path); else FtpClient.ChangeDirectory(path); }
		public override string GetWorkingDirectory() { return FtpClient.GetWorkingDirectory(); }
		public override void DeleteFile(string path) { FtpClient.DeleteFile(path); }
		public override void Abort() { FtpClient.Abort(); }
		public override void MakeDirectory(string path) { FtpClient.MakeDirectory(path); }
		public override void MoveFile(string fromPath, string toPath) { FtpClient.MoveFile(fromPath, toPath); }
		public override void DeleteDirectory(string path) { FtpClient.DeleteDirectory(path); }
		public override string GetHelp() { return FtpClient.GetHelp(); }
		public override DateTime GetFileDateTime(string fileName, bool adjustToLocalTime) { return FtpClient.GetFileDateTime(fileName, adjustToLocalTime); }
		public override void SetDateTime(string path, DateTime dateTime) { FtpClient.SetDateTime(path, dateTime); }
		public override string GetStatus() { return FtpClient.GetStatus(); }
		public override void ChangeDirectoryUp() { FtpClient.ChangeDirectoryUp(); }
		public override long GetFileSize(string path) { return FtpClient.GetFileSize(path); }
		public override string GetFeatures() { return FtpClient.GetFeatures(); }
		public override string GetStatus(string path) { return FtpClient.GetStatus(path); }
		public override void AllocateStorage(long size) { FtpClient.AllocateStorage(size); }
		public override string GetSystemType() { return FtpClient.GetSystemType(); }
		public override void PutFileUnique(string localPath) { FtpClient.PutFileUnique(localPath); }
		public override void PutFileUnique(Stream inputStream) { FtpClient.PutFileUnique(inputStream); }
		public override void GetFile(string remotePath, string localPath) { GetFile(remotePath, localPath, FileAction.CreateNew); }
		public override void GetFile(string remotePath, string localPath, FileAction action) { FtpClient.GetFile(remotePath, localPath, action); }
		public override void GetFile(string remotePath, Stream outStream, bool restart) { FtpClient.GetFile(remotePath, outStream, restart); }
		public override bool Exists(string path) { return FtpClient.Exists(path); }
		public override string GetNameList() { return FtpClient.GetNameList(); }
		public override string GetNameList(string path) { return FtpClient.GetNameList(path); }
		public override string GetDirListAsText() { return FtpClient.GetDirListAsText(); }
		public override string GetDirListAsText(string path) { return FtpClient.GetDirListAsText(path); }
		public override FtpItemCollection GetDirList() { return FtpClient.GetDirList(); }
		public override FtpItemCollection GetDirList(string path) { return FtpClient.GetDirList(path); }
		public override FtpItemCollection GetDirListDeep(string path) { return FtpClient.GetDirListDeep(path); }
		public override void Rename(string name, string newName) { FtpClient.Rename(name, newName); }
		public override string Quote(string command) { return FtpClient.Quote(command); }
		public override void NoOperation() { FtpClient.NoOperation(); }
		public override void ChangeMode(string path, int octalValue) { FtpClient.ChangeMode(path, octalValue); }
		public override void Site(string argument) { FtpClient.Site(argument); }
		public override void PutFile(string localPath, string remotePath, FileAction action) { FtpClient.PutFile(localPath, remotePath, action); }
		public override void PutFile(string localPath, string remotePath) { FtpClient.PutFile(localPath, remotePath); }
		public override void PutFile(string localPath, FileAction action) { FtpClient.PutFile(localPath, action); }
		public override void PutFile(string localPath) { FtpClient.PutFile(localPath); }
		public override void PutFile(Stream inputStream, string remotePath, FileAction action) { FtpClient.PutFile(inputStream, remotePath, action); }
		public override void FxpCopy(string fileName, Silversite.Services.Ftp.FtpClient destination) { FtpClient.FxpCopy(fileName, ((StarksoftProvider)destination.Provider).FtpClient); }
		public override IFtpItemParser ItemParser { get { return FtpClient.ItemParser; } set { FtpClient.ItemParser = value; } }

#endregion

#region Destructors

		public virtual void Dispose() {
			if (FtpClient != null) {
				FtpClient.Dispose();
				FtpClient = null;
			}
		}

		~StarksoftProvider() { Dispose(); }

#endregion
    }
}




