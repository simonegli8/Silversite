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
    
#region Public Enums
    /// <summary>
    /// Enumeration representing type of file transfer mode.
    /// </summary>
    public enum TransferType : int
    {
        /// <summary>
        /// No transfer type.
        /// </summary>
        None,
        /// <summary>
        /// Transfer mode of type 'A' (ascii).
        /// </summary>
        Ascii, 
        /// <summary>
        /// Transfer mode of type 'I' (image or binary)
        /// </summary>
        Binary // TYPE I
    }

    /// <summary>
    /// Enumeration representing the three types of actions that FTP supports when
    /// uploading or 'putting' a file on an FTP server from the FTP client.  
    /// </summary>
    public enum FileAction : int
    {
        /// <summary>
        /// No action.
        /// </summary>
        None,
        /// <summary>
        /// Create a new file or overwrite an existing file.
        /// </summary>
        Create,
        /// <summary>
        /// Create a new file.  Do not overwrite an existing file.
        /// </summary>
        CreateNew,
        /// <summary>
        /// Create a new file or append an existing file.
        /// </summary>
        CreateOrAppend,
        /// <summary>
        /// Resume a file transfer.
        /// </summary>
        Resume,
        /// <summary>
        /// Resume a file transfer if the file already exists.  Otherwise, create a new file.
        /// </summary>
        ResumeOrCreate
    }

	/// <summary>
	/// Defines the possible versions of FtpSecurityProtocol.
	/// </summary>
	public enum FtpSecurityProtocol: int {
		/// <summary>
		/// No security protocol specified.
		/// </summary>
		None,
		/// <summary>
		/// Specifies Transport Layer Security (TLS) version 1.0 is required to secure communciations.  The TLS protocol is defined in IETF RFC 2246 and supercedes the SSL 3.0 protocol.
		/// </summary>
		/// <remarks>
		/// The AUTH TLS command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL protcol and is the security protocol that should be used whenever possible.
		/// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
		/// </remarks>
		Tls1Explicit,
		/// <summary>
		/// Specifies Transport Layer Security (TLS) version 1.0. or Secure Socket Layer (SSL) version 3.0 is acceptable to secure communications in explicit mode.
		/// </summary>
		/// <remarks>
		/// The AUTH SSL command is sent to the FTP server to secure the connection but the security protocol is negotiated between the server and client.  
		/// TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
		/// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
		/// </remarks>
		Tls1OrSsl3Explicit,
		/// <summary>
		/// Specifies Secure Socket Layer (SSL) version 3.0 is required to secure communications in explicit mode.  SSL 3.0 has been superseded by the TLS protocol
		/// and is provided for backward compatibility only
		/// </summary>
		/// <remarks>
		/// The AUTH SSL command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
		/// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
		/// Some FTP server do not implement TLS or understand the command AUTH TLS.  In those situations you should specify the security
		/// protocol Ssl3, otherwise specify Tls1.
		/// </remarks>
		Ssl3Explicit,
		/// <summary>
		/// Specifies Secure Socket Layer (SSL) version 2.0 is required to secure communications in explicit mode.  SSL 2.0 has been superseded by the TLS protocol
		/// and is provided for backward compatibility only.  SSL 2.0 has several weaknesses and should only be used with legacy FTP server that require it.
		/// </summary>
		/// <remarks>
		/// The AUTH SSL command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
		/// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
		/// Some FTP server do not implement TLS or understand the command AUTH TLS.  In those situations you should specify the security
		/// protocol Ssl3, otherwise specify Tls1.
		/// </remarks>
		Ssl2Explicit,
		/// <summary>
		/// Specifies Transport Layer Security (TLS) version 1.0 is required to secure communciations in explicit mode.  The TLS protocol is defined in IETF RFC 2246 and supercedes the SSL 3.0 protocol.
		/// </summary>
		/// <remarks>
		/// The AUTH TLS command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL protcol and is the security protocol that should be used whenever possible.
		/// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
		/// </remarks>
		Tls1Implicit,
		/// <summary>
		/// Specifies Transport Layer Security (TLS) version 1.0. or Secure Socket Layer (SSL) version 3.0 is acceptable to secure communications in implicit mode.
		/// </summary>
		/// <remarks>
		/// The AUTH SSL command is sent to the FTP server to secure the connection but the security protocol is negotiated between the server and client.  
		/// TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
		/// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
		/// </remarks>
		Tls1OrSsl3Implicit,
		/// <summary>
		/// Specifies Secure Socket Layer (SSL) version 3.0 is required to secure communications in implicit mode.  SSL 3.0 has been superseded by the TLS protocol
		/// and is provided for backward compatibility only
		/// </summary>
		/// <remarks>
		/// The AUTH SSL command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
		/// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
		/// Some FTP server do not implement TLS or understand the command AUTH TLS.  In those situations you should specify the security
		/// protocol Ssl3, otherwise specify Tls1.
		/// </remarks>
		Ssl3Implicit,
		/// <summary>
		/// Specifies Secure Socket Layer (SSL) version 2.0 is required to secure communications in implicit mode.  SSL 2.0 has been superseded by the TLS protocol
		/// and is provided for backward compatibility only.  SSL 2.0 has several weaknesses and should only be used with legacy FTP server that require it.
		/// </summary>
		/// <remarks>
		/// The AUTH SSL command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
		/// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
		/// Some FTP server do not implement TLS or understand the command AUTH TLS.  In those situations you should specify the security
		/// protocol Ssl3, otherwise specify Tls1.
		/// </remarks>
		Ssl2Implicit
	}

	/// <summary>
	/// The type of data transfer mode (e.g. Active or Passive).
	/// </summary>
	/// <remarks>
	/// The default setting is Passive data transfer mode.  This mode is widely used as a
	/// firewall friendly setting for the FTP clients operating behind a firewall.
	/// </remarks>
	public enum TransferMode: int {
		/// <summary>
		/// Active transfer mode.  In this mode the FTP server initiates a connection to the client when transfering data.
		/// </summary>
		/// <remarks>This transfer mode may not work when the FTP client is behind a firewall and is accessing a remote FTP server.</remarks>
		Active,
		/// <summary>
		/// Passive transfer mode.  In this mode the FTP client initiates a connection to the server when transfering data.
		/// </summary>
		/// <remarks>
		/// This transfer mode is "firewall friendly" and generally allows an FTP client behind a firewall to access a remote FTP server.
		/// This mode is recommended for most data transfers.
		/// </remarks>
		Passive
	}
	/// <summary>
	/// Enumeration representing the type of integrity algorithm used to verify the integrity of the file after transfer and storage.
	/// </summary>
	public enum HashingFunction : int {
		/// <summary>
		/// No algorithm slected.
		/// </summary>
		None,
		/// <summary>
		/// Cyclic redundancy check (CRC).  A CRC can be used in the same way as a checksum to detect accidental 
		/// alteration of data during transmission or storage.
		/// </summary>
		/// <remarks>
		/// It is often falsely assumed that when a message and its CRC are transmitted over an open channel, then when it arrives 
		/// if the CRC matches the message's calculated CRC then the message can not have been altered in transit.
		/// For this reason it is recommended to use SHA1 whenever possible.
		/// </remarks>
		/// <seealso cref="Sha1"/>
		Crc32,
		/// <summary>
		/// Message-Digest algorithm 5 (MD5).  Hashing function used to produce a 'unique' signature to detect 
		/// alternation of data during transmission or storage.
		/// </summary>
		/// <remarks>
		/// MD5 is a weak algorithm and has been show to produce collisions.  For this reason it is recommended to use SHA1 whenere possible.
		/// </remarks>
		/// <seealso cref="Sha1"/>
		Md5,
		/// <summary>
		/// Secure Hash Algorithm (SHA).  cryptographic hash functions designed by the National Security Agency (NSA) and published by the NIST as a U.S. Federal Information Processing Standard.
		/// </summary>
		/// <remarks>
		/// SHA1 is the recommended integrity check algorithm.  Even a small change in the message will, with overwhelming probability, result in a completely different hash due to the avalanche effect.
		/// </remarks>
		Sha1
	}


#endregion


	public class FtpClient: Silversite.Services.Service<FtpProvider>, IDisposable {

		public new FtpProvider Provider { get; set; }

		#region Contructors

      /// <summary>
      /// FtpClient default constructor.
      /// </summary>
		public FtpClient() : base() { Provider =  base.Provider.Client(); }
		public FtpClient(string host) : base() { Provider =  base.Provider.Client(host); }
		public FtpClient(string host, int port) : base() { Provider =  base.Provider.Client(host, port); }
		public FtpClient(string host, int port, FtpSecurityProtocol securityProtocol): base() { Provider = base.Provider.Client(host, port, securityProtocol); }
		
		#endregion

		#region Public Properties
 
		public TransferType FileTransferType { get { return Provider.FileTransferType; } set { Provider.FileTransferType = value; } }
		public int FxpTransferTimeout { get { return Provider.FxpTransferTimeout; } set { Provider.FxpTransferTimeout = value; } }
		public string CurrentDirectory { get { return Provider.CurrentDirectory; } }
		public bool CompatibleMode { get { return Provider.CompatibleMode; } set { Provider.CompatibleMode = value; } }
		public IFtpItemParser ItemParser { get { return Provider.ItemParser; } set { Provider.ItemParser = value; } }
		public int Port { get { return Provider.Port; } set { Provider.Port = value; } }
		public string Host { get { return Provider.Host; } set { Provider.Host = value; } }
		public FtpSecurityProtocol SecurityProtocol { get { return Provider.SecurityProtocol; } set { Provider.SecurityProtocol = value; } }
		/// <summary>
		/// Get Client certificate collection used when connection with a secured SSL/TSL protocol.  Add your client certificates 
		/// if required to connect to the remote FTP server.
		/// </summary>
		/// <remarks>Returns a X509CertificateCollection list contains X.509 security certificates.</remarks>
		/// <seealso cref="SecurityProtocol"/>
		/// <seealso cref="ValidateServerCertificate" />
		public X509CertificateCollection SecurityCertificates {
			get { return Provider.SecurityCertificates; }
		}

		/// <summary>
		/// Gets or sets a value indicating that the client will use compression when uploading and downloading
		/// data.
		/// </summary>
		/// <remarks>
		/// This value turns on or off the compression algorithm DEFLATE to facility FTP data compression which is compatible with
		/// FTP servers that implement compression via the zLib compression software library.  The default value is 'False'.  
		/// This setting can only be changed when the system is not busy conducting other operations.  
		/// 
		/// Returns True if compression is enabled; otherwise False;
		/// </remarks>
		public bool IsCompressionEnabled {
			get { return Provider.IsCompressionEnabled; }
			set { Provider.IsCompressionEnabled = value; }
		}

		/// <summary>
		/// Gets or sets an Integer value representing the maximum upload speed allowed 
		/// for data transfers in kilobytes per second.
		/// </summary>
		/// <remarks>
		/// Set this value when you would like to throttle back any upload data transfers.
		/// A value of zero means there is no restriction on how fast data uploads are 
		/// conducted.  The default value is zero.  This setting is used to throttle data traffic so the FtpClient does
		/// not consume all available network bandwidth.
		/// </remarks>
		/// <seealso cref="MaxDownloadSpeed"/>
		public int MaxUploadSpeed {
			get { return Provider.MaxUploadSpeed; }
			set { Provider.MaxUploadSpeed = value; }
		}

		/// <summary>
		/// Gets or sets an Integer value representing the maximum download speed allowed 
		/// for data transfers in kilobytes per second.
		/// </summary>
		/// <remarks>
		/// Set this value when you would like to throttle back any download data transfers.
		/// A value of zero means there is no restriction on how fast data uploads are 
		/// conducted.  The default value is zero.  This setting is used to throttle data traffic so the FtpClient does
		/// not consume all available network bandwidth.
		/// </remarks>
		/// <seealso cref="MaxUploadSpeed"/>
		public int MaxDownloadSpeed {
			get { return Provider.MaxDownloadSpeed; }
			set { Provider.MaxDownloadSpeed = value; }	
		}

		public int ServerTimeOffset { get { return Provider.ServerTimeOffset; } set { Provider.ServerTimeOffset = value; } }
		public string ServerTimeString { get { return Provider.ServerTimeString; } }

		public virtual string RootDirectory { get { return Provider.RootDirectory; } }

		   /// <summary>
        /// Gets or sets the data transfer mode to either Active or Passive.
        /// </summary>
        /// <seealso cref="ActivePortRangeMin"/>
        /// <seealso cref="ActivePortRangeMax"/>
		public TransferMode DataTransferMode {
			get { return Provider.DataTransferMode; }
			set { Provider.DataTransferMode = value; }
		}
		public HashingFunction AutoChecksumValidation {
			get { return Provider.AutoChecksumValidation; }
			set { Provider.AutoChecksumValidation = value; }
		}
		#endregion

		#region Public Methods

        /// <summary>
        /// Opens a connection to the remote FTP server and log in with user name and password credentials.
        /// </summary>
        /// <param name="user">User name.  Many public ftp allow for this value to 'anonymous'.</param>
        /// <param name="password">Password.  Anonymous public ftp servers generally require a valid email address for a password.</param>
        /// <remarks>Use the Close() method to log off and close the connection to the FTP server.</remarks>
        /// <seealso cref="OpenAsync"/>
        /// <seealso cref="Close"/>
        /// <seealso cref="Reopen"/>
        public void Open(string user, string password) { Provider.Open(user, password); }
        
        /// <summary>
        /// Closes connection to the FTP server.
        /// </summary>
        /// <seealso cref="FtpBase.ConnectionClosed"/>
        /// <seealso cref="Reopen"/>
        /// <seealso cref="Open"/>
        public void Close() { Provider.Close(); }

        /// <summary>
        /// Changes the current working directory on the server.
        /// </summary>
        /// <param name="path">Path of the new directory to change to.</param>
        /// <remarks>Accepts both foward slash '/' and back slash '\' path names.</remarks>
        /// <seealso cref="ChangeDirectoryMultiPath(string)"/>
        /// <seealso cref="GetWorkingDirectory"/>
		public void ChangeDirectory(string path) { Provider.ChangeDirectory(path); }
        

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        /// <returns>A string value containing the current full working directory path on the FTP server.</returns>
        /// <seealso cref="ChangeDirectory"/>
        /// <seealso cref="ChangeDirectoryUp"/>
		public string GetWorkingDirectory() { return Provider.GetWorkingDirectory(); }
	

        /// <summary>
        /// Deletes a file on the remote FTP server.  
        /// </summary>
        /// <param name="path">The path name of the file to delete.</param>
        /// <remarks>
        /// The file is deleted in the current working directory if no path information 
        /// is specified.  Otherwise a full absolute path name can be specified.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to delete the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="DeleteDirectory"/>
		public void DeleteFile(string path) { Provider.DeleteFile(path); }

        /// <summary>
        /// Aborts an action such as transferring a file to or from the server.  
        /// </summary>
        /// <remarks>
        /// The abort command is sent up to the server signaling the server to abort the current activity.
        /// </remarks>
        public void Abort() { Provider.Abort(); }
    
        /// <summary>
        /// Creates a new directory on the remote FTP server.  
        /// </summary>
        /// <param name="path">The name of a new directory or an absolute path name for a new directory.</param>
        /// <remarks>
        /// If a directory name is given for path then the server will create a new subdirectory 
        /// in the current working directory.  Optionally, a full absolute path may be given.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to make the subdirectory using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
		public void MakeDirectory(string path) { Provider.MakeDirectory(path); }

        /// <summary>
        /// Moves a file on the remote FTP server from one directory to another.  
        /// </summary>
        /// <param name="fromPath">Path and/or file name to be moved.</param>
        /// <param name="toPath">Destination path specifying the directory where the file will be moved to.</param>
        /// <remarks>
        /// This method actually results in several FTP commands being issued to the server to perform the physical file move.  
        /// This method is available for your convenience when performing common tasks such as moving processed files out of a pickup directory
        /// and into a archive directory.
        /// Note that some older FTP server implementations will not accept a full path to a filename.  On those systems this method may not work
        /// properly.
        /// </remarks>
		public void MoveFile(string fromPath, string toPath) { Provider.MoveFile(fromPath, toPath); }
        
        /// <summary>
        /// Deletes a directory from the FTP server.
        /// </summary>
        /// <param name="path">Directory to delete.</param>
        /// <remarks>
        /// The path can be either a specific subdirectory relative to the 
        /// current working directory on the server or an absolute path to 
        /// the directory to remove.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the parent directory of the directory you wish to delete using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="DeleteFile"/>
		public void DeleteDirectory(string path) { Provider.DeleteDirectory(path); }
	
        /// <summary>
        /// Executes the specific help dialog on the FTP server.  
        /// </summary>
        /// <returns>
        /// A string contains the help dialog from the FTP server.
        /// </returns>
        /// <remarks>
        /// Every FTP server supports a different set of commands and this commands 
        /// can be obtained by the FTP HELP command sent to the FTP server.  The information sent
        /// back is not parsed or processed in any way by the FtpClient object.  
        /// </remarks>
		public string GetHelp() { return Provider.GetHelp(); }
	
        /// <summary>
        /// Retrieves the data and time for a specific file on the ftp server as a Coordinated Universal Time (UTC) value (formerly known as GMT). 
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="adjustToLocalTime">Specifies if modified date and time as reported on the FTP server should be adjusted to the local time zone with daylight savings on the client.</param>
        /// <returns>
        /// A date time value.
        /// </returns>
        /// <remarks>
        /// This function uses the MDTM command which is an additional feature command and therefore not supported
        /// by all FTP servers.
        /// </remarks>
        /// <seealso cref="GetFileSize"/>
        public DateTime GetFileDateTime(string fileName, bool adjustToLocalTime) { return Provider.GetFileDateTime(fileName, adjustToLocalTime); }
      

        /// <summary>
        /// Set the date and time for a specific file or directory on the server.
        /// </summary>
        /// <param name="path">The path or name of the file or directory.</param>
        /// <param name="dateTime">New date to set on the file or directory.</param>
        /// <remarks>
        /// This function uses the MDTM command which is an additional feature command and therefore not supported
        /// by all FTP servers.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory which has the file you wish to set the date and time using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="Rename"/>
        public void SetDateTime(string path, DateTime dateTime) { Provider.SetDateTime(path, dateTime); }

        /// <summary>
        /// Retrieves the specific status for the FTP server.  
        /// </summary>
        /// <remarks>
        /// Each FTP server may return different status dialog information.  The status information sent
        /// back is not parsed or processed in any way by the FtpClient object. 
        /// </remarks>
        /// <returns>
        /// A string containing the status of the FTP server.
        /// </returns>
		public string GetStatus() { return Provider.GetStatus(); }
	
        /// <summary>
        /// Changes the current working directory on the FTP server to the parent directory.  
        /// </summary>
        /// <remarks>
        /// If there is no parent directory then ChangeDirectoryUp() will not have 
        /// any affect on the current working directory.
        /// </remarks>
        /// <seealso cref="ChangeDirectory"/>
        /// <seealso cref="GetWorkingDirectory"/>
        public void ChangeDirectoryUp() { Provider.ChangeDirectoryUp(); }
	
        /// <summary>
        /// Get the file size for a file on the remote FTP server.  
        /// </summary>
        /// <param name="path">The name and/or path to the file.</param>
        /// <returns>An integer specifying the file size.</returns>
        /// <remarks>
        /// The path can be file name relative to the current working directory or an absolute path.  This command is an additional feature 
        /// that is not supported by all FTP servers.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to get the file size using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetFileDateTime"/>
		public long GetFileSize(string path) { return Provider.GetFileSize(path); }

        /// <summary>
        /// Get the additional features supported by the remote FTP server.  
        /// </summary>
        /// <returns>A string containing the additional features beyond the RFC 959 standard supported by the FTP server.</returns>
        /// <remarks>
        /// This command is an additional feature beyond the RFC 959 standard and therefore is not supported by all FTP servers.        
        /// </remarks>
        public string GetFeatures() { return Provider.GetFeatures(); }
  
        /// <summary>
        /// Retrieves the specific status for a file on the FTP server.  
        /// </summary>
        /// <param name="path">
        /// The path to the file.
        /// </param>
        /// <returns>
        /// A string containing the status for the file.
        /// </returns>
        /// <remarks>
        /// Each FTP server may return different status dialog information.  The status information sent
        /// back is not parsed or processed in any way by the FtpClient object. 
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to get the status of the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
		public string GetStatus(string path) { return Provider.GetStatus(path); }
        
        /// <summary>
        /// Allocates storage for a file on the FTP server prior to data transfer from the FTP client.  
        /// </summary>
        /// <param name="size">
        /// The storage size to allocate on the FTP server.
        /// </param>
        /// <remarks>
        /// Some FTP servers may return the client to specify the storage size prior to data transfer from the FTP client to the FTP server.
        /// </remarks>
        public void AllocateStorage(long size) { Provider.AllocateStorage(size); }

        /// <summary>
        /// Retrieves a string identifying the remote FTP system.  
        /// </summary>
        /// <returns>
        /// A string contains the server type.
        /// </returns>
        /// <remarks>
        /// The string contains the word "Type:", and the default transfer type 
        /// For example a UNIX FTP server will return 'UNIX Type: L8'.  A Windows 
        /// FTP server will return 'WINDOWS_NT'.
        /// </remarks>
		public string GetSystemType() { return Provider.GetSystemType();}

        /// <summary>
        /// Uploads a local file specified in the path parameter to the remote FTP server.  
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server.  
        /// A unique file name is created by the server.    
        /// </remarks>
        public void PutFileUnique(string localPath) { Provider.PutFileUnique(localPath); }
     
        /// <summary>
        /// Uploads any stream object to the remote FTP server and stores the data under a unique file name assigned by the FTP server.  
        /// </summary>
        /// <param name="inputStream">Any stream object on the local client machine.</param>
        /// <remarks>
        /// The stream is uploaded to the current working directory on the remote server.  
        /// A unique file name is created by the server to store the data uploaded from the stream.
        /// </remarks>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="GetFile(string, string)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>  
        public void PutFileUnique(Stream inputStream) { Provider.PutFileUnique(inputStream); }
   
        /// <summary>
        /// Retrieves a remote file from the FTP server and writes the data to a local file
        /// specfied in the localPath parameter.  If the local file already exists a System.IO.IOException is thrown.
        /// </summary>
        /// <remarks>
        /// To retrieve a remote file that you need to overwrite an existing file with or append to an existing file
        /// see the method GetFile(string, string, FileAction).
        /// </remarks>
        /// <param name="remotePath">A path of the remote file.</param>
        /// <param name="localPath">A fully qualified local path to a file on the local machine.</param>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>
        public void GetFile(string remotePath, string localPath) { GetFile(remotePath, localPath, FileAction.CreateNew); }

        /// <summary>
        /// Retrieves a remote file from the FTP server and writes the data to a local file
        /// specfied in the localPath parameter.
        /// </summary>
        /// <remarks>
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to get the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <param name="remotePath">A path and/or file name to the remote file.</param>
        /// <param name="localPath">A fully qualified local path to a file on the local machine.</param>
        /// <param name="action">The type of action to take.</param>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>
        public void GetFile(string remotePath, string localPath, FileAction action) { Provider.GetFile(remotePath, localPath, action); }

        /// <summary>
        /// Retrieves a remote file from the FTP server and writes the data to a local stream object
        /// specfied in the outStream parameter.
        /// </summary> 
        /// <param name="remotePath">A path and/or file name to the remote file.</param>
        /// <param name="outStream">An output stream object used to stream the remote file to the local machine.</param>
        /// <param name="restart">A true/false value to indicate if the file download needs to be restarted due to a previous partial download.</param>
        /// <remarks>
        /// If the remote path is a file name then the file is downloaded from the FTP server current working directory.  Otherwise a fully qualified
        /// path for the remote file may be specified.  The output stream must be writeable and can be any stream object.  Finally, the restart parameter
        /// is used to send a restart command to the FTP server.  The FTP server is instructed to restart the download process at the last position of
        /// of the output stream.  Not all FTP servers support the restart command.  If the FTP server does not support the restart (REST) command,
        /// an FtpException error is thrown.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to get the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>
        public void GetFile(string remotePath, Stream outStream, bool restart) { Provider.GetFile(remotePath, outStream, restart); }
        
        /// <summary>
        /// Tests to see if a file or directory exists on the remote server.  The current working directory must be the
        /// parent or root directory of the file or directory whose existence is being tested.  For best results, 
        /// call this method from the root working directory ("/").
        /// </summary>
        /// <param name="path">The full path to the remote file or directory relative to the current working directory, or the filename 
        /// or directory in the current working directory.</param>
        /// <returns>Boolean value indicating if file exists or not.</returns>
        /// <remarks>This method will execute a change working directory (CWD) command prior to testing to see if the  
        /// file or direcotry exists.  The original working directory will be changed back to the original value
        /// after this method has completed.  This method may not work on systems where the directory listing is not being
        /// parsed correctly.  If the method call GetDirList() does not work properly with your FTP server, this method may not
        /// produce reliable results.  This method will also not produce reliable results if the directory or file is hidden on the
        /// remote FTP server.</remarks>
        /// <seealso cref="GetDirList()"/>
        public bool Exists(string path) { return Provider.Exists(path); }

        /// <summary>
        /// Retrieves a file name listing of the current working directory from the 
        /// remote FTP server using the NLST command.
        /// </summary>
        /// <returns>A string containing the file listing from the current working directory.</returns>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListDeep"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        public string GetNameList() { return Provider.GetNameList(); }

        /// <summary>
        /// Retrieves a file name listing of the current working directory from the 
        /// remote FTP server using the NLST command.
        /// </summary>
        /// <param name="path">The path to a directory on the remote FTP server.</param>
        /// <returns>A string containing the file listing from the current working directory.</returns>
        /// <remarks>
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the parent directory you wish to get the name list using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListDeep"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        public string GetNameList(string path) { return Provider.GetNameList(path); }

        /// <summary>
        /// Retrieves a directory listing of the current working directory from the 
        /// remote FTP server using the LIST command.
        /// </summary>
        /// <returns>A string containing the directory listing of files from the current working directory.</returns>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList(string)"/>
        public string GetDirListAsText() { return Provider.GetDirListAsText(); }

        /// <summary>
        /// Retrieves a directory listing of the current working directory from the 
        /// remote FTP server using the LIST command.
        /// </summary>
        /// <param name="path">The path to a directory on the remote FTP server.</param>
        /// <returns>A string containing the directory listing of files from the current working directory.</returns>
        /// <remarks>
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the parent directory you wish to get the name list using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep"/>
        /// <seealso cref="GetDirListDeepAsync()"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public string GetDirListAsText(string path) { return Provider.GetDirListAsText(path); }

        /// <summary>
        /// Retrieves a list of the files from current working directory on the remote FTP 
        /// server using the LIST command.  
        /// </summary>
        /// <returns>FtpItemList collection object.</returns>
        /// <remarks>
        /// This method returns a FtpItemList collection of FtpItem objects.
        /// </remarks>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListDeep"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList(string)"/>        
        public FtpItemCollection GetDirList() { return Provider.GetDirList(); }

        /// <summary>
        /// Retrieves a list of the files from a specified path on the remote FTP 
        /// server using the LIST command. 
        /// </summary>
        /// <param name="path">The path to a directory on the remote FTP server.</param>
        /// <returns>FtpFileCollection collection object.</returns>
        /// <remarks>
        /// This method returns a FtpFileCollection object containing a collection of 
        /// FtpItem objects.  Some FTP server implementations will not accept a full path to a resource.  On those
        /// systems it is best to change the working directory using the ChangeDirectoryMultiPath(string) method and then call
        /// the method GetDirList().
        /// </remarks>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListDeep"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList(string)"/>        
        public FtpItemCollection GetDirList(string path) { return Provider.GetDirList(path); }

        /// <summary>
        /// Deeply retrieves a list of all files and all sub directories from a specified path on the remote FTP 
        /// server using the LIST command. 
        /// </summary>
        /// <param name="path">The path to a directory on the remote FTP server.</param>
        /// <returns>FtpFileCollection collection object.</returns>
        /// <remarks>
        /// This method returns a FtpFileCollection object containing a collection of 
        /// FtpItem objects.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the parent directory you wish to get the directory list using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetNameList(string)"/>
        public FtpItemCollection GetDirListDeep(string path) { return Provider.GetDirListDeep(path); }

        /// <summary>
        /// Renames a file or directory on the remote FTP server.
        /// </summary>
        /// <param name="name">The name or absolute path of the file or directory you want to rename.</param>
        /// <param name="newName">The new name or absolute path of the file or directory.</param>
        /// <seealso cref="SetDateTime"/>
        public void Rename(string name, string newName) { Provider.Rename(name, newName); }

        /// <summary>
        /// Send a raw FTP command to the server.
        /// </summary>
        /// <param name="command">A string containing a valid FTP command value such as SYST.</param>
        /// <returns>The raw textual response from the server.</returns>
        /// <remarks>
        /// This is an advanced feature of the FtpClient class that allows for any custom or specialized
        /// FTP command to be sent to the FTP server.  Some FTP server support custom commands outside of
        /// the standard FTP command list.  The following commands are not supported: PASV, RETR, STOR, and STRU.
        /// </remarks>
        /// <example>
        /// <code>
        /// FtpClient ftp = new FtpClient("ftp.microsoft.com");
        /// ftp.Open("anonymous", "myemail@server.com");
        /// string response = ftp.Quote("SYST");
        /// System.Diagnostics.Debug.WriteLine(response);
        /// ftp.Close();
        /// </code>
        /// </example>
        public string Quote(string command) { return Provider.Quote(command); }

        /// <summary>
        /// Sends a NOOP or no operation command to the FTP server.  This can be used to prevent some servers from logging out the
        /// interactive session during file transfer process.
        /// </summary>
        public void NoOperation() { Provider.NoOperation(); }

        /// <summary>
        /// Issues a site specific change file mode (CHMOD) command to the server.  Not all servers implement this command.
        /// </summary>
        /// <param name="path">The path to the file or directory you want to change the mode on.</param>
        /// <param name="octalValue">The CHMOD octal value.</param>
        /// <remarks>
        /// Common CHMOD values used on web servers.
        /// 
        ///       Value 	User 	Group 	Other
        ///         755 	rwx 	r-x 	r-x
        ///         744 	rwx 	r--	    r--
        ///         766 	rwx 	rw- 	rw-
        ///         777 	rwx 	rwx 	rwx
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory containing the file or directory you wish to change the mode on by using with the 
        /// ChangeDirectory() or ChangeDirectoryMultiPath() method.
        /// </remarks>
        /// <seealso cref="SetDateTime"/>
        /// <seealso cref="Rename"/>
        public void ChangeMode(string path, int octalValue) { Provider.ChangeMode(path, octalValue); }

        /// <summary>
        /// Issue a SITE command to the FTP server for site specific implementation commands.
        /// </summary>
        /// <param name="argument">One or more command arguments</param>
        /// <remarks>
        /// For example, the CHMOD command is issued as a SITE command.
        /// </remarks>
        public void Site(string argument) { Provider.Site(argument); }
   
        /// <summary>
        /// Uploads a local file specified in the path parameter to the remote FTP server.   
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <param name="remotePath">Filename or full path to file on the remote FTP server.</param>
        /// <param name="action">The type of put action taken.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server.  The remotePath
        /// parameter is used to specify the path and file name used to store the file on the remote server.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to put the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>    
        public void PutFile(string localPath, string remotePath, FileAction action) { Provider.PutFile(localPath, remotePath, action); }

        /// <summary>
        /// Uploads a local file specified in the path parameter to the remote FTP server.   
        /// An FtpException is thrown if the file already exists.
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <param name="remotePath">Filename or full path to file on the remote FTP server.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server.  The remotePath
        /// parameter is used to specify the path and file name used to store the file on the remote server.
        /// To overwrite an existing file see the method PutFile(string, string, FileAction) and specify the 
        /// FileAction Create.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to put the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>            
        public void PutFile(string localPath, string remotePath) { Provider.PutFile(localPath, remotePath); }

        /// <summary>
        /// Uploads a local file specified in the path parameter to the remote FTP server.   
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <param name="action">The type of put action taken.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server. 
        /// </remarks>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>            
        public void PutFile(string localPath, FileAction action) { Provider.PutFile(localPath, action); }

        /// <summary>
        /// Uploads a local file specified in the path parameter to the remote FTP server.   
        /// An FtpException is thrown if the file already exists.
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server. 
        /// </remarks>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>    
        public void PutFile(string localPath) { Provider.PutFile(localPath); }

        /// <summary>
        /// Uploads stream data specified in the inputStream parameter to the remote FTP server.   
        /// </summary>
        /// <param name="inputStream">Any open stream object on the local client machine.</param>
        /// <param name="remotePath">Filename or path and filename of the file stored on the remote FTP server.</param>
        /// <param name="action">The type of put action taken.</param>
        /// <remarks>
        /// The stream is uploaded to the current working directory on the remote server.  The remotePath
        /// parameter is used to specify the path and file name used to store the file on the remote server.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to put the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>        
        public void PutFile(Stream inputStream, string remotePath, FileAction action) { Provider.PutFile(inputStream, remotePath, action); }

        /// <summary>
        /// File Exchange Protocol (FXP) allows server-to-server transfer which can greatly speed up file transfers.
        /// </summary>
        /// <param name="fileName">The name of the file to transfer.</param>
        /// <param name="destination">The destination FTP server which must be supplied as an open and connected FtpClient object.</param>
        /// <remarks>
        /// Both servers must support and have FXP enabled before you can transfer files between two remote servers using FXP.  One FTP server must support PASV mode and the other server must allow PORT commands from a foreign address.  Finally, firewall settings may interfer with the ability of one server to access the other server.
        /// Starksoft FtpClient will coordinate the FTP negoitaion and necessary PORT and PASV transfer commands.
        /// </remarks>
        /// <seealso cref="FxpTransferTimeout"/>
        /// <seealso cref="FxpCopyAsync"/> 
        public void FxpCopy(string fileName, FtpClient destination) { Provider.FxpCopy(fileName, destination); }
     
#endregion

	}
}




