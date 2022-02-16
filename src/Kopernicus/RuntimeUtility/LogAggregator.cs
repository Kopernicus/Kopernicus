/**
 * Kopernicus Planetary System Modifier
 * ------------------------------------------------------------- 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Kopernicus.ConfigParser;
using UnityEngine;
using Version = Kopernicus.Constants.Version;

namespace Kopernicus.RuntimeUtility
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class LogAggregator : DescriptionAttribute
    {
        public LogAggregator(String description) : base(description) { }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class LogAggregatorWorker : MonoBehaviour
    {
        private Dictionary<String, String[]> _files;

        private void Awake()
        {
            // Create the storage 
            _files = new Dictionary<String, String[]>();

            // Select the Assembly that should run
            Type type = Parser.ModTypes.Where(t => t.Name == "LogAggregator")
                .OrderByDescending(t => Version.BuiltTime(t.Assembly)).FirstOrDefault();

            // Are we the type?
            if (type != typeof(LogAggregator))
            {
                Destroy(this);
                return;
            }

            // This aggregator got selected to run
            Assembly[] assemblies = Parser.ModTypes.Select(t => t.Assembly).ToArray();
            for (Int32 i = 0; i < assemblies.Length; i++)
            {
                // Check if the assemblies have description attributes
                if (!(assemblies[i].GetCustomAttributes(typeof(DescriptionAttribute), true) is DescriptionAttribute[]
                    descriptionAttributes))
                {
                    continue;
                }

                descriptionAttributes = descriptionAttributes.Where(d => d.GetType().Name == "LogAggregator")
                    .ToArray();

                // Read the files
                List<String> filesToBackup = new List<String>();
                for (Int32 j = 0; j < descriptionAttributes.Length; j++)
                {
                    String file = descriptionAttributes[j].Description;

                    // Prevent breaking out of KSPs folder
                    if (file.Contains("../") || file.Contains("/..") || file.Contains("\\..") || file.Contains("..\\"))
                    {
                        Debug.Log("[LAG] Found an attempt to break out of KSPs directory! Assembly: " + assemblies[i] +
                                  ", Path: " + file);
                        continue;
                    }

                    // Add the file
                    filesToBackup.Add(file);
                }

                if (!_files.ContainsKey(assemblies[i].GetName().Name) && filesToBackup.Count > 0)
                {
                    _files.Add(assemblies[i].GetName().Name, filesToBackup.ToArray());
                }
            }

            // Keep this behaviour alive
            DontDestroyOnLoad(this);
            GameEvents.onCrash.Add(AggregateLogs);
        }

        private void OnApplicationQuit()
        {
            AggregateLogs(null);
        }

        private void AggregateLogs(EventReport report)
        {
            foreach (KeyValuePair<String, String[]> kvP in _files)
            {
                String modName = kvP.Key;
                String[] filePaths = kvP.Value;

                // Does the file already exist?
                String path = KSPUtil.ApplicationRootPath + "Logs/" + "Logs-" + modName + ".zip";
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using (ZipStorer zip = ZipStorer.Create(path, ""))
                {
                    for (Int32 i = 0; i < filePaths.Length; i++)
                    {
                        String fullPath = Path.Combine(KSPUtil.ApplicationRootPath, filePaths[i]);

                        // Is it a file?
                        if (File.Exists(fullPath))
                        {
                            // Make a temporary copy to avoid IO errors
                            File.Copy(fullPath, "tmpfile", true);

                            using (FileStream stream = File.OpenRead("tmpfile"))
                            {
                                zip.AddStream(ZipStorer.Compression.Deflate, filePaths[i], stream, DateTime.Now, "");
                            }

                            // Remove the temporary file
                            File.Delete("tmpfile");
                        }

                        // Is it a directory?
                        if (!Directory.Exists(fullPath))
                        {
                            continue;
                        }

                        foreach (String file in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
                        {
                            String nameInFile = file.Substring(KSPUtil.ApplicationRootPath.Length);
                            if (nameInFile.StartsWith("/") || nameInFile.StartsWith("\\"))
                            {
                                nameInFile = nameInFile.Substring(1);
                            }

                            // Make a temporary copy to avoid IO errors
                            File.Copy(file, "tmpfile", true);

                            using (FileStream stream = File.OpenRead("tmpfile"))
                            {
                                zip.AddStream(ZipStorer.Compression.Deflate, nameInFile, stream, DateTime.Now, "");
                            }

                            // Remove the temporary file
                            File.Delete("tmpfile");
                        }
                    }
                }
            }

            // I don't know if both events might fire so clean the storage to be sure the other method doesn't run
            _files.Clear();
        }
    }
}

#region ZipStorer
// ZipStorer, by Jaime Olivares
// Website: http://github.com/jaime-olivares/zipstorer
// Version: 3.4.0 (August 4, 2017)
namespace System.IO.Compression
{
    /// <summary>
    /// Unique class for compression/decompression file. Represents a Zip file.
    /// </summary>
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class ZipStorer : IDisposable
    {
        /// <summary>
        /// Compression method enumeration
        /// </summary>
        public enum Compression : ushort
        {
            /// <summary>Uncompressed storage</summary> 
            Store = 0,
            /// <summary>Deflate compression method</summary>
            Deflate = 8
        }

        /// <summary>
        /// Represents an entry in Zip file directory
        /// </summary>
        public struct ZipFileEntry
        {
            /// <summary>Compression method</summary>
            public Compression Method;
            /// <summary>Full path and filename as stored in Zip</summary>
            public String FilenameInZip;
            /// <summary>Original file size</summary>
            public UInt32 FileSize;
            /// <summary>Compressed file size</summary>
            public UInt32 CompressedSize;
            /// <summary>Offset of header information inside Zip storage</summary>
            public UInt32 HeaderOffset;
            /// <summary>Offset of file inside Zip storage</summary>
            public UInt32 FileOffset;
            /// <summary>32-bit checksum of entire file</summary>
            public UInt32 Crc32;
            /// <summary>Last modification time of file</summary>
            public DateTime ModifyTime;
            /// <summary>User comment for file</summary>
            public String Comment;
            /// <summary>True if UTF8 encoding for filename and comments, false if default (CP 437)</summary>
            public Boolean EncodeUTF8;

            /// <summary>Overriden method</summary>
            /// <returns>Filename in Zip</returns>
            public override String ToString()
            {
                return FilenameInZip;
            }
        }

        #region Public fields
        /// <summary>True if UTF8 encoding for filename and comments, false if default (CP 437)</summary>
        // ReSharper disable ConvertToConstant.Global
        public Boolean EncodeUTF8 = false;
        /// <summary>Force deflate algorithm even if it inflates the stored file. Off by default.</summary>
        public Boolean ForceDeflating = false;
        // ReSharper restore ConvertToConstant.Global
        #endregion

        #region Private fields
        // List of files to store
        private readonly List<ZipFileEntry> Files = new List<ZipFileEntry>();
        // Filename of storage file
        private String FileName;
        // Stream object of storage file
        private Stream ZipFileStream;
        // General comment
        private String Comment = "";
        // Central dir image
        private Byte[] CentralDirImage;
        // Existing files in zip
        private UInt16 ExistingFiles;
        // File access for Open method
        private FileAccess Access;
        // leave the stream open after the ZipStorer object is disposed
        private Boolean leaveOpen;
        // Static CRC32 Table
        private static readonly UInt32[] CrcTable;
        // Default filename encoder
        private static readonly Encoding DefaultEncoding = Encoding.GetEncoding(437);
        #endregion

        #region Public methods
        // Static constructor. Just invoked once in order to create the CRC32 lookup table.
        static ZipStorer()
        {
            // Generate CRC32 table
            CrcTable = new UInt32[256];
            for (Int32 i = 0; i < CrcTable.Length; i++)
            {
                UInt32 c = (UInt32)i;
                for (Int32 j = 0; j < 8; j++)
                {
                    if ((c & 1) != 0)
                    {
                        c = 3988292384 ^ (c >> 1);
                    }
                    else
                    {
                        c >>= 1;
                    }
                }
                CrcTable[i] = c;
            }
        }
        /// <summary>
        /// Method to create a new storage file
        /// </summary>
        /// <param name="_filename">Full path of Zip file to create</param>
        /// <param name="_comment">General comment for Zip file</param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Create(String _filename, String _comment)
        {
            Stream stream = new FileStream(_filename, FileMode.Create, FileAccess.ReadWrite);

            ZipStorer zip = Create(stream, _comment);
            zip.Comment = _comment;
            zip.FileName = _filename;

            return zip;
        }
        /// <summary>
        /// Method to create a new zip storage in a stream
        /// </summary>
        /// <param name="_stream"></param>
        /// <param name="_comment"></param>
        /// <param name="_leaveOpen">true to leave the stream open after the ZipStorer object is disposed; otherwise, false (default).</param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Create(Stream _stream, String _comment, Boolean _leaveOpen = false)
        {
            ZipStorer zip = new ZipStorer
            {
                Comment = _comment,
                ZipFileStream = _stream,
                Access = FileAccess.Write,
                leaveOpen = _leaveOpen
            };
            return zip;
        }
        /// <summary>
        /// Method to open an existing storage file
        /// </summary>
        /// <param name="_filename">Full path of Zip file to open</param>
        /// <param name="_access">File access mode as used in FileStream constructor</param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Open(String _filename, FileAccess _access)
        {
            Stream stream = new FileStream(_filename, FileMode.Open, _access == FileAccess.Read ? FileAccess.Read : FileAccess.ReadWrite);

            ZipStorer zip = Open(stream, _access);
            zip.FileName = _filename;

            return zip;
        }
        /// <summary>
        /// Method to open an existing storage from stream
        /// </summary>
        /// <param name="_stream">Already opened stream with zip contents</param>
        /// <param name="_access">File access mode for stream operations</param>
        /// <param name="_leaveOpen">true to leave the stream open after the ZipStorer object is disposed; otherwise, false (default).</param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Open(Stream _stream, FileAccess _access, Boolean _leaveOpen = false)
        {
            if (!_stream.CanSeek && _access != FileAccess.Read)
            {
                throw new InvalidOperationException("Stream cannot seek");
            }

            ZipStorer activeZip = null;
            using (ZipStorer zip = new ZipStorer
            {
                ZipFileStream = _stream,
                Access = _access,
                leaveOpen = _leaveOpen
            })
            {
                //zip.FileName = _filename;

                if (zip.ReadFileInfo())
                {
                    activeZip = zip;
                }
            }
            if (activeZip != null) return activeZip;
            else activeZip.Dispose();
            throw new InvalidDataException();
        }
        /// <summary>
        /// Add full contents of a file into the Zip storage
        /// </summary>
        /// <param name="_method">Compression method</param>
        /// <param name="_pathname">Full path of file to add to Zip storage</param>
        /// <param name="_filenameInZip">Filename and path as desired in Zip directory</param>
        /// <param name="_comment">Comment for stored file</param>        
        public void AddFile(Compression _method, String _pathname, String _filenameInZip, String _comment)
        {
            if (Access == FileAccess.Read)
            {
                throw new InvalidOperationException("Writing is not allowed");
            }

            using (FileStream stream = new FileStream(_pathname, FileMode.Open, FileAccess.Read))
            {
                AddStream(_method, _filenameInZip, stream, File.GetLastWriteTime(_pathname), _comment);
            }
        }
        /// <summary>
        /// Add full contents of a stream into the Zip storage
        /// </summary>
        /// <param name="_method">Compression method</param>
        /// <param name="_filenameInZip">Filename and path as desired in Zip directory</param>
        /// <param name="_source">Stream object containing the data to store in Zip</param>
        /// <param name="_modTime">Modification time of the data to store</param>
        /// <param name="_comment">Comment for stored file</param>
        public void AddStream(Compression _method, String _filenameInZip, Stream _source, DateTime _modTime, String _comment)
        {
            if (Access == FileAccess.Read)
            {
                throw new InvalidOperationException("Writing is not allowed");
            }

            // Prepare the file info
            ZipFileEntry zfe = new ZipFileEntry
            {
                Method = _method,
                EncodeUTF8 = EncodeUTF8,
                FilenameInZip = NormalizedFilename(_filenameInZip),
                Comment = _comment ?? "",
                Crc32 = 0,
                HeaderOffset = (UInt32) ZipFileStream.Position,
                ModifyTime = _modTime
            };

            // Even though we write the header now, it will have to be rewritten, since we don't know compressed size or crc.
            // to be updated later
            // offset within file of the start of this local record

            // Write local header
            WriteLocalHeader(ref zfe);
            zfe.FileOffset = (UInt32)ZipFileStream.Position;

            // Write file to zip (store)
            Store(ref zfe, _source);
            _source.Close();

            UpdateCrcAndSizes(ref zfe);

            Files.Add(zfe);
        }
        /// <summary>
        /// Updates central directory (if pertinent) and close the Zip storage
        /// </summary>
        /// <remarks>This is a required step, unless automatic dispose is used</remarks>
        public void Close()
        {
            if (Access != FileAccess.Read)
            {
                UInt32 centralOffset = (UInt32)ZipFileStream.Position;
                UInt32 centralSize = 0;

                if (CentralDirImage != null)
                {
                    ZipFileStream.Write(CentralDirImage, 0, CentralDirImage.Length);
                }

                for (Int32 i = 0; i < Files.Count; i++)
                {
                    Int64 pos = ZipFileStream.Position;
                    WriteCentralDirRecord(Files[i]);
                    centralSize += (UInt32)(ZipFileStream.Position - pos);
                }

                if (CentralDirImage != null)
                {
                    WriteEndRecord(centralSize + (UInt32)CentralDirImage.Length, centralOffset);
                }
                else
                {
                    WriteEndRecord(centralSize, centralOffset);
                }
            }

            if (ZipFileStream == null || leaveOpen)
            {
                return;
            }
            ZipFileStream.Flush();
            ZipFileStream.Dispose();
            ZipFileStream = null;
        }
        /// <summary>
        /// Read all the file records in the central directory 
        /// </summary>
        /// <returns>List of all entries in directory</returns>
        public IEnumerable<ZipFileEntry> ReadCentralDir()
        {
            if (CentralDirImage == null)
            {
                throw new InvalidOperationException("Central directory currently does not exist");
            }

            List<ZipFileEntry> result = new List<ZipFileEntry>();

            for (Int32 pointer = 0; pointer < CentralDirImage.Length;)
            {
                UInt32 signature = BitConverter.ToUInt32(CentralDirImage, pointer);
                if (signature != 0x02014b50)
                {
                    break;
                }

                Boolean encodeUTF8 = (BitConverter.ToUInt16(CentralDirImage, pointer + 8) & 0x0800) != 0;
                UInt16 method = BitConverter.ToUInt16(CentralDirImage, pointer + 10);
                UInt32 modifyTime = BitConverter.ToUInt32(CentralDirImage, pointer + 12);
                UInt32 crc32 = BitConverter.ToUInt32(CentralDirImage, pointer + 16);
                UInt32 comprSize = BitConverter.ToUInt32(CentralDirImage, pointer + 20);
                UInt32 fileSize = BitConverter.ToUInt32(CentralDirImage, pointer + 24);
                UInt16 filenameSize = BitConverter.ToUInt16(CentralDirImage, pointer + 28);
                UInt16 extraSize = BitConverter.ToUInt16(CentralDirImage, pointer + 30);
                UInt16 commentSize = BitConverter.ToUInt16(CentralDirImage, pointer + 32);
                UInt32 headerOffset = BitConverter.ToUInt32(CentralDirImage, pointer + 42);

                Encoding encoder = encodeUTF8 ? Encoding.UTF8 : DefaultEncoding;

                ZipFileEntry zfe = new ZipFileEntry
                {
                    Method = (Compression) method,
                    FilenameInZip = encoder.GetString(CentralDirImage, pointer + 46, filenameSize),
                    FileOffset = GetFileOffset(headerOffset),
                    FileSize = fileSize,
                    CompressedSize = comprSize,
                    HeaderOffset = headerOffset,
                    Crc32 = crc32,
                    ModifyTime = DosTimeToDateTime(modifyTime) ?? DateTime.Now
                };

                if (commentSize > 0)
                {
                    zfe.Comment = encoder.GetString(CentralDirImage, pointer + 46 + filenameSize + extraSize, commentSize);
                }

                result.Add(zfe);
                pointer += 46 + filenameSize + extraSize + commentSize;
            }

            return result;
        }
        /// <summary>
        /// Copy the contents of a stored file into a physical file
        /// </summary>
        /// <param name="_zfe">Entry information of file to extract</param>
        /// <param name="_filename">Name of file to store uncompressed data</param>
        /// <returns>True if success, false if not.</returns>
        /// <remarks>Unique compression methods are Store and Deflate</remarks>
        public Boolean ExtractFile(ZipFileEntry _zfe, String _filename)
        {
            // Make sure the parent directory exist
            String path = Path.GetDirectoryName(_filename);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Check it is directory. If so, do nothing
            if (Directory.Exists(_filename))
            {
                return true;
            }

            Boolean result;
            using (FileStream output = new FileStream(_filename, FileMode.Create, FileAccess.Write))
            {
                result = ExtractFile(_zfe, output);
            }

            if (!result)
            {
                return result;
            }
            File.SetCreationTime(_filename, _zfe.ModifyTime);
            File.SetLastWriteTime(_filename, _zfe.ModifyTime);

            return result;
        }
        /// <summary>
        /// Copy the contents of a stored file into an opened stream
        /// </summary>
        /// <param name="_zfe">Entry information of file to extract</param>
        /// <param name="_stream">Stream to store the uncompressed data</param>
        /// <returns>True if success, false if not.</returns>
        /// <remarks>Unique compression methods are Store and Deflate</remarks>
        public Boolean ExtractFile(ZipFileEntry _zfe, Stream _stream)
        {
            if (!_stream.CanWrite)
            {
                throw new InvalidOperationException("Stream cannot be written");
            }

            // check signature
            Byte[] signature = new Byte[4];
            ZipFileStream.Seek(_zfe.HeaderOffset, SeekOrigin.Begin);
            ZipFileStream.Read(signature, 0, 4);
            if (BitConverter.ToUInt32(signature, 0) != 0x04034b50)
            {
                return false;
            }

            // Select input stream for inflating or just reading
            Stream inStream;
            switch (_zfe.Method)
            {
                case Compression.Store:
                    inStream = ZipFileStream;
                    break;
                case Compression.Deflate:
                    inStream = new DeflateStream(ZipFileStream, CompressionMode.Decompress, true);
                    break;
                default:
                    return false;
            }

            // Buffered copy
            Byte[] buffer = new Byte[16384];
            ZipFileStream.Seek(_zfe.FileOffset, SeekOrigin.Begin);
            UInt32 bytesPending = _zfe.FileSize;
            while (bytesPending > 0)
            {
                Int32 bytesRead = inStream.Read(buffer, 0, (Int32)Math.Min(bytesPending, buffer.Length));
                _stream.Write(buffer, 0, bytesRead);
                bytesPending -= (UInt32)bytesRead;
            }
            _stream.Flush();

            //if (_zfe.Method == Compression.Deflate)
            //{
            inStream.Dispose();
            //}
            return true;
        }

        /// <summary>
        /// Copy the contents of a stored file into a byte array
        /// </summary>
        /// <param name="_zfe">Entry information of file to extract</param>
        /// <param name="_file">Byte array with uncompressed data</param>
        /// <returns>True if success, false if not.</returns>
        /// <remarks>Unique compression methods are Store and Deflate</remarks>
        public Boolean ExtractFile(ZipFileEntry _zfe, out Byte[] _file)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                if (ExtractFile(_zfe, ms))
                {
                    _file = ms.ToArray();
                    return true;
                }

                _file = null;
                return false;
            }
        }
        /// <summary>
        /// Removes one of many files in storage. It creates a new Zip file.
        /// </summary>
        /// <param name="_zip">Reference to the current Zip object</param>
        /// <param name="_zfes">List of Entries to remove from storage</param>
        /// <returns>True if success, false if not</returns>
        /// <remarks>This method only works for storage of type FileStream</remarks>
        public static Boolean RemoveEntries(ref ZipStorer _zip, List<ZipFileEntry> _zfes)
        {
            if (!(_zip.ZipFileStream is FileStream))
            {
                throw new InvalidOperationException("RemoveEntries is allowed just over streams of type FileStream");
            }


            //Get full list of entries
            IEnumerable<ZipFileEntry> fullList = _zip.ReadCentralDir();

            //In order to delete we need to create a copy of the zip file excluding the selected items
            String tempZipName = Path.GetTempFileName();
            String tempEntryName = Path.GetTempFileName();

            try
            {
                using (ZipStorer tempZip = Create(tempZipName, String.Empty))
                {
                    foreach (ZipFileEntry zfe in fullList)
                    {
                        if (_zfes.Contains(zfe))
                        {
                            continue;
                        }
                        if (_zip.ExtractFile(zfe, tempEntryName))
                        {
                            tempZip.AddFile(zfe.Method, tempEntryName, zfe.FilenameInZip, zfe.Comment);
                        }
                    }
                    _zip.Close();
                    tempZip.Close();

                    File.Delete(_zip.FileName);
                    File.Move(tempZipName, _zip.FileName);

                    _zip = Open(_zip.FileName, _zip.Access);
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (File.Exists(tempZipName))
                {
                    File.Delete(tempZipName);
                }

                if (File.Exists(tempEntryName))
                {
                    File.Delete(tempEntryName);
                }
            }
            return true;
        }
        #endregion

        #region Private methods
        // Calculate the file offset by reading the corresponding local header
        private UInt32 GetFileOffset(UInt32 _headerOffset)
        {
            Byte[] buffer = new Byte[2];

            ZipFileStream.Seek(_headerOffset + 26, SeekOrigin.Begin);
            ZipFileStream.Read(buffer, 0, 2);
            UInt16 filenameSize = BitConverter.ToUInt16(buffer, 0);
            ZipFileStream.Read(buffer, 0, 2);
            UInt16 extraSize = BitConverter.ToUInt16(buffer, 0);

            return (UInt32)(30 + filenameSize + extraSize + _headerOffset);
        }
        /* Local file header:
            local file header signature     4 bytes  (0x04034b50)
            version needed to extract       2 bytes
            general purpose bit flag        2 bytes
            compression method              2 bytes
            last mod file time              2 bytes
            last mod file date              2 bytes
            crc-32                          4 bytes
            compressed size                 4 bytes
            uncompressed size               4 bytes
            filename length                 2 bytes
            extra field length              2 bytes

            filename (variable size)
            extra field (variable size)
        */
        private void WriteLocalHeader(ref ZipFileEntry _zfe)
        {
            Encoding encoder = _zfe.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
            Byte[] encodedFilename = encoder.GetBytes(_zfe.FilenameInZip);

            ZipFileStream.Write(new Byte[] { 80, 75, 3, 4, 20, 0 }, 0, 6); // No extra header
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)(_zfe.EncodeUTF8 ? 0x0800 : 0)), 0, 2); // filename and comment encoding 
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)_zfe.Method), 0, 2);  // zipping method
            ZipFileStream.Write(BitConverter.GetBytes(DateTimeToDosTime(_zfe.ModifyTime)), 0, 4); // zipping date and time
            ZipFileStream.Write(new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 12); // unused CRC, un/compressed size, updated later
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)encodedFilename.Length), 0, 2); // filename length
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)0), 0, 2); // extra length

            ZipFileStream.Write(encodedFilename, 0, encodedFilename.Length);
        }
        /* Central directory's File header:
            central file header signature   4 bytes  (0x02014b50)
            version made by                 2 bytes
            version needed to extract       2 bytes
            general purpose bit flag        2 bytes
            compression method              2 bytes
            last mod file time              2 bytes
            last mod file date              2 bytes
            crc-32                          4 bytes
            compressed size                 4 bytes
            uncompressed size               4 bytes
            filename length                 2 bytes
            extra field length              2 bytes
            file comment length             2 bytes
            disk number start               2 bytes
            internal file attributes        2 bytes
            external file attributes        4 bytes
            relative offset of local header 4 bytes

            filename (variable size)
            extra field (variable size)
            file comment (variable size)
        */
        private void WriteCentralDirRecord(ZipFileEntry _zfe)
        {
            Encoding encoder = _zfe.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
            Byte[] encodedFilename = encoder.GetBytes(_zfe.FilenameInZip);
            Byte[] encodedComment = encoder.GetBytes(_zfe.Comment);

            ZipFileStream.Write(new Byte[] { 80, 75, 1, 2, 23, 0xB, 20, 0 }, 0, 8);
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)(_zfe.EncodeUTF8 ? 0x0800 : 0)), 0, 2); // filename and comment encoding 
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)_zfe.Method), 0, 2);  // zipping method
            ZipFileStream.Write(BitConverter.GetBytes(DateTimeToDosTime(_zfe.ModifyTime)), 0, 4);  // zipping date and time
            ZipFileStream.Write(BitConverter.GetBytes(_zfe.Crc32), 0, 4); // file CRC
            ZipFileStream.Write(BitConverter.GetBytes(_zfe.CompressedSize), 0, 4); // compressed file size
            ZipFileStream.Write(BitConverter.GetBytes(_zfe.FileSize), 0, 4); // uncompressed file size
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)encodedFilename.Length), 0, 2); // Filename in zip
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)0), 0, 2); // extra length
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)encodedComment.Length), 0, 2);

            ZipFileStream.Write(BitConverter.GetBytes((UInt16)0), 0, 2); // disk=0
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)0), 0, 2); // file type: binary
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)0), 0, 2); // Internal file attributes
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)0x8100), 0, 2); // External file attributes (normal/readable)
            ZipFileStream.Write(BitConverter.GetBytes(_zfe.HeaderOffset), 0, 4);  // Offset of header

            ZipFileStream.Write(encodedFilename, 0, encodedFilename.Length);
            ZipFileStream.Write(encodedComment, 0, encodedComment.Length);
        }
        /* End of central dir record:
            end of central dir signature    4 bytes  (0x06054b50)
            number of this disk             2 bytes
            number of the disk with the
            start of the central directory  2 bytes
            total number of entries in
            the central dir on this disk    2 bytes
            total number of entries in
            the central dir                 2 bytes
            size of the central directory   4 bytes
            offset of start of central
            directory with respect to
            the starting disk number        4 bytes
            zip file comment length          2 bytes
            zip file comment (variable size)
        */
        private void WriteEndRecord(UInt32 _size, UInt32 _offset)
        {
            Encoding encoder = EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
            Byte[] encodedComment = encoder.GetBytes(Comment);

            ZipFileStream.Write(new Byte[] { 80, 75, 5, 6, 0, 0, 0, 0 }, 0, 8);
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)Files.Count + ExistingFiles), 0, 2);
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)Files.Count + ExistingFiles), 0, 2);
            ZipFileStream.Write(BitConverter.GetBytes(_size), 0, 4);
            ZipFileStream.Write(BitConverter.GetBytes(_offset), 0, 4);
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)encodedComment.Length), 0, 2);
            ZipFileStream.Write(encodedComment, 0, encodedComment.Length);
        }
        // Copies all source file into storage file
        private void Store(ref ZipFileEntry _zfe, Stream _source)
        {
            while (true)
            {
                Byte[] buffer = new Byte[16384];
                Int32 bytesRead;
                UInt32 totalRead = 0;

                Int64 posStart = ZipFileStream.Position;
                Int64 sourceStart = _source.CanSeek ? _source.Position : 0;

                using (Stream outStream = _zfe.Method == Compression.Store ? ZipFileStream : new DeflateStream(ZipFileStream, CompressionMode.Compress, true))
                {
                    _zfe.Crc32 = 0 ^ 0xffffffff;

                    do
                    {
                        bytesRead = _source.Read(buffer, 0, buffer.Length);
                        totalRead += (UInt32)bytesRead;
                        if (bytesRead <= 0)
                        {
                            continue;
                        }
                        outStream.Write(buffer, 0, bytesRead);

                        for (UInt32 i = 0; i < bytesRead; i++)
                        {
                            _zfe.Crc32 = CrcTable[(_zfe.Crc32 ^ buffer[i]) & 0xFF] ^ (_zfe.Crc32 >> 8);
                        }
                    } while (bytesRead > 0);

                    outStream.Flush();

                    if (_zfe.Method == Compression.Deflate)
                    {
                        outStream.Dispose();
                    }

                    _zfe.Crc32 ^= 0xffffffff;
                    _zfe.FileSize = totalRead;
                    _zfe.CompressedSize = (UInt32)(ZipFileStream.Position - posStart);

                    // Verify for real compression
                    if (_zfe.Method == Compression.Deflate && !ForceDeflating && _source.CanSeek && _zfe.CompressedSize > _zfe.FileSize)
                    {
                        // Start operation again with Store algorithm
                        _zfe.Method = Compression.Store;
                        ZipFileStream.Position = posStart;
                        ZipFileStream.SetLength(posStart);
                        _source.Position = sourceStart;
                        continue;
                    }

                    break;
                }
            }
        }

        /* DOS Date and time:
            MS-DOS date. The date is a packed value with the following format. Bits Description 
                0-4 Day of the month (131) 
                5-8 Month (1 = January, 2 = February, and so on) 
                9-15 Year offset from 1980 (add 1980 to get actual year) 
            MS-DOS time. The time is a packed value with the following format. Bits Description 
                0-4 Second divided by 2 
                5-10 Minute (059) 
                11-15 Hour (023 on a 24-hour clock) 
        */
        private static UInt32 DateTimeToDosTime(DateTime _dt)
        {
            return (UInt32)(
                (_dt.Second / 2) | (_dt.Minute << 5) | (_dt.Hour << 11) |
                (_dt.Day << 16) | (_dt.Month << 21) | ((_dt.Year - 1980) << 25));
        }
        private static DateTime? DosTimeToDateTime(UInt32 _dt)
        {
            Int32 year = (Int32)(_dt >> 25) + 1980;
            Int32 month = (Int32)(_dt >> 21) & 15;
            Int32 day = (Int32)(_dt >> 16) & 31;
            Int32 hours = (Int32)(_dt >> 11) & 31;
            Int32 minutes = (Int32)(_dt >> 5) & 63;
            Int32 seconds = (Int32)(_dt & 31) * 2;

            if (month == 0 || day == 0)
            {
                return null;
            }

            return new DateTime(year, month, day, hours, minutes, seconds);
        }

        /* CRC32 algorithm
          The 'magic number' for the CRC is 0xdebb20e3.  
          The proper CRC pre and post conditioning is used, meaning that the CRC register is
          pre-conditioned with all ones (a starting value of 0xffffffff) and the value is post-conditioned by
          taking the ones complement of the CRC residual.
          If bit 3 of the general purpose flag is set, this field is set to zero in the local header and the correct
          value is put in the data descriptor and in the central directory.
        */
        private void UpdateCrcAndSizes(ref ZipFileEntry _zfe)
        {
            Int64 lastPos = ZipFileStream.Position;  // remember position

            ZipFileStream.Position = _zfe.HeaderOffset + 8;
            ZipFileStream.Write(BitConverter.GetBytes((UInt16)_zfe.Method), 0, 2);  // zipping method

            ZipFileStream.Position = _zfe.HeaderOffset + 14;
            ZipFileStream.Write(BitConverter.GetBytes(_zfe.Crc32), 0, 4);  // Update CRC
            ZipFileStream.Write(BitConverter.GetBytes(_zfe.CompressedSize), 0, 4);  // Compressed size
            ZipFileStream.Write(BitConverter.GetBytes(_zfe.FileSize), 0, 4);  // Uncompressed size

            ZipFileStream.Position = lastPos;  // restore position
        }
        // Replaces backslashes with slashes to store in zip header
        private static String NormalizedFilename(String _filename)
        {
            String filename = _filename.Replace('\\', '/');

            Int32 pos = filename.IndexOf(':');
            if (pos >= 0)
            {
                filename = filename.Remove(0, pos + 1);
            }

            return filename.Trim('/');
        }
        // Reads the end-of-central-directory record
        private Boolean ReadFileInfo()
        {
            if (ZipFileStream.Length < 22)
            {
                return false;
            }

            try
            {
                ZipFileStream.Seek(-17, SeekOrigin.End);
                using (BinaryReader br = new BinaryReader(ZipFileStream))
                {
                    do
                    {
                        ZipFileStream.Seek(-5, SeekOrigin.Current);
                        UInt32 sig = br.ReadUInt32();
                        if (sig != 0x06054b50)
                        {
                            continue;
                        }
                        ZipFileStream.Seek(6, SeekOrigin.Current);

                        UInt16 entries = br.ReadUInt16();
                        Int32 centralSize = br.ReadInt32();
                        UInt32 centralDirOffset = br.ReadUInt32();
                        UInt16 commentSize = br.ReadUInt16();

                        // check if comment field is the very last data in file
                        if (ZipFileStream.Position + commentSize != ZipFileStream.Length)
                        {
                            return false;
                        }

                        // Copy entire central directory to a memory buffer
                        ExistingFiles = entries;
                        CentralDirImage = new Byte[centralSize];
                        ZipFileStream.Seek(centralDirOffset, SeekOrigin.Begin);
                        ZipFileStream.Read(CentralDirImage, 0, centralSize);

                        // Leave the pointer at the beginning of central dir, to append new files
                        ZipFileStream.Seek(centralDirOffset, SeekOrigin.Begin);
                        return true;
                    } while (ZipFileStream.Position > 0);
                }
            }
            catch
            {
                // Ignore
            }

            return false;
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Closes the Zip file stream
        /// </summary>
        public void Dispose()
        {
            Close();
        }
        #endregion
    }
}
#endregion
