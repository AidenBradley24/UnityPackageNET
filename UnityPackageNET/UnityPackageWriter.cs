using System.Formats.Tar;
using System.IO.Compression;
using System.Text;

namespace UnityPackageNET
{
	/// <summary>
	/// Write Unity package (.unitypackage) files.
	/// </summary>
	public class UnityPackageWriter : IDisposable
	{
		private readonly bool _leaveOpen;
		private bool _disposed;

		private readonly TarWriter _tarWriter;
		private readonly GZipStream _gzipStream;
		private readonly Stream _backingStream;

		/// <summary>
		/// Initializes a new instance of the UnityPackageWriter class that writes a compressed Unity package archive to the
		/// specified stream.
		/// </summary>
		/// <remarks>This constructor creates a GZip-compressed tar archive for writing Unity package data. The
		/// leaveOpen parameter allows control over the lifetime of the underlying stream when disposing the writer.</remarks>
		/// <param name="stream">The output stream to which the Unity package archive will be written. The stream must be writable.</param>
		/// <param name="leaveOpen">true to leave the provided stream open after the UnityPackageWriter is disposed; otherwise, false.</param>
		public UnityPackageWriter(Stream stream, bool leaveOpen = false)
		{
			_backingStream = stream;
			_leaveOpen = leaveOpen;
			_gzipStream = new GZipStream(_backingStream, CompressionMode.Compress, leaveOpen: true);
			_tarWriter = new TarWriter(_gzipStream, leaveOpen: true);
		}

		/// <summary>
		/// Write an entry to the Unity package.
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		public void WriteEntry(UnityPackageEntry entry)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			ArgumentNullException.ThrowIfNull(entry, nameof(entry));
			if (entry.Metadata is null) throw new ArgumentException("Entry has no metadata!", nameof(entry));
			if (entry.DataStream is null) throw new ArgumentException("Entry has no data stream!", nameof(entry));
			if (entry.GUID != entry.Metadata.Guid)
			{
				throw new ArgumentException("Entry GUID does not match metadata GUID!", nameof(entry));
			}

			var dirTarEntry = new GnuTarEntry(TarEntryType.Directory, entry.GUID.ToString("N") + "/");
			_tarWriter.WriteEntry(dirTarEntry);

			var assetTarEntry = new GnuTarEntry(TarEntryType.RegularFile, entry.GUID.ToString("N") + "/asset")
			{
				DataStream = entry.DataStream
			};
			_tarWriter.WriteEntry(assetTarEntry);


			using (var ms = new MemoryStream())
			{
				entry.Metadata.SaveToStream(ms);
				ms.Position = 0;
				var metaTarEntry = new GnuTarEntry(TarEntryType.RegularFile, entry.GUID.ToString("N") + "/asset.meta")
				{
					DataStream = ms
				};
				_tarWriter.WriteEntry(metaTarEntry);
			}

			using (var ms = new MemoryStream())
			{
				ms.Write(Encoding.UTF8.GetBytes(entry.Metadata.PathName));
				ms.Position = 0;
				var pathnameEntry = new GnuTarEntry(TarEntryType.RegularFile, entry.GUID.ToString("N") + "/pathname")
				{
					DataStream = ms
				};
				_tarWriter.WriteEntry(pathnameEntry);
			}

		}

		/// <inheritdoc/>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			if (_disposed) return;
			_disposed = true;

			_tarWriter.Dispose();
			_gzipStream.Dispose();
			if (!_leaveOpen)
			{
				_backingStream.Dispose();
			}
			else
			{
				_backingStream.Flush();
			}
		}
	}
}
