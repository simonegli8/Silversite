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

namespace Silversite.Services.Ftp {
    
	public abstract class FtpProvider: Provider<FtpClient> {

		#region Contructors

		public abstract FtpProvider Client();
		public abstract FtpProvider Client(string host);
		public abstract FtpProvider Client(string host, int port);
		public abstract FtpProvider Client(string host, int port, FtpSecurityProtocol securityProtocol);

		#endregion

		#region Public Properties
 
		public abstract TransferType FileTransferType { get; set; }
		public abstract int FxpTransferTimeout { get; set; }
		public abstract string CurrentDirectory { get; }
		public abstract bool CompatibleMode { get; set; }
		public abstract IFtpItemParser ItemParser { get; set; }
		public abstract int Port { get; set; }
		public abstract string Host { get; set; }
		public abstract FtpSecurityProtocol SecurityProtocol { get; set; }
		public abstract X509CertificateCollection SecurityCertificates { get; }
		public abstract bool IsCompressionEnabled { get; set; }
		public abstract int MaxUploadSpeed { get; set; }
		public abstract int MaxDownloadSpeed { get; set; }
		public abstract int ServerTimeOffset { get; set;  }
		public abstract string ServerTimeString { get; }
		public abstract string RootDirectory { get; }
		public abstract TransferMode DataTransferMode { get; set; }
		public abstract HashingFunction AutoChecksumValidation { get; set; }
		#endregion

		#region Public Methods

		public abstract void Open(string user, string password);
		public abstract void Close();
		public abstract void ChangeDirectory(string path);
		public abstract string GetWorkingDirectory();
		public abstract void DeleteFile(string path);
		public abstract void Abort();
		public abstract void MakeDirectory(string path);
		public abstract void MoveFile(string fromPath, string toPath);
		public abstract void DeleteDirectory(string path);
		public abstract string GetHelp();
		public abstract DateTime GetFileDateTime(string fileName, bool adjustToLocalTime);
		public abstract void SetDateTime(string path, DateTime dateTime);
		public abstract string GetStatus();
		public abstract void ChangeDirectoryUp();
		public abstract long GetFileSize(string path);
		public abstract string GetFeatures();
		public abstract string GetStatus(string path);
		public abstract void AllocateStorage(long size);
		public abstract string GetSystemType();
		public abstract void PutFileUnique(string localPath);
		public abstract void PutFileUnique(Stream inputStream);
		public abstract void GetFile(string remotePath, string localPath);
		public abstract void GetFile(string remotePath, string localPath, FileAction action);
		public abstract void GetFile(string remotePath, Stream outStream, bool restart);
		public abstract bool Exists(string path);
		public abstract string GetNameList();
		public abstract string GetNameList(string path);
		public abstract string GetDirListAsText();
		public abstract string GetDirListAsText(string path);
		public abstract FtpItemCollection GetDirList();
		public abstract FtpItemCollection GetDirList(string path);
		public abstract FtpItemCollection GetDirListDeep(string path);
		public abstract void Rename(string name, string newName);
		public abstract string Quote(string command);
		public abstract void NoOperation();
		public abstract void ChangeMode(string path, int octalValue);
		public abstract void Site(string argument);
		public abstract void PutFile(string localPath, string remotePath, FileAction action);
		public abstract void PutFile(string localPath, string remotePath);
		public abstract void PutFile(string localPath, FileAction action);
		public abstract void PutFile(string localPath);
		public abstract void PutFile(Stream inputStream, string remotePath, FileAction action);
		public abstract void FxpCopy(string fileName, FtpClient destination);
     
#endregion

    }
}




