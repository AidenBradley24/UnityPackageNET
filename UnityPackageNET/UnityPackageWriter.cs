using System.Formats.Tar;
using System.IO.Compression;
using System.Text;

namespace UnityPackageNET
{
	public class UnityPackageWriter : IDisposable
	{
		private readonly bool _leaveOpen;
		private bool _disposed;

		private readonly TarWriter _tarWriter;
		private readonly GZipStream _gzipStream;
		private readonly Stream _backingStream;

		public UnityPackageWriter(Stream stream, bool leaveOpen = false)
		{
			_backingStream = stream;
			_leaveOpen = leaveOpen;
			_gzipStream = new GZipStream(_backingStream, CompressionMode.Compress, leaveOpen: true);
			_tarWriter = new TarWriter(_gzipStream, leaveOpen: true);
		}

		public void WriteEntry(UnityPackageEntry entry)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			ArgumentNullException.ThrowIfNull(entry);

			var dirTarEntry = new GnuTarEntry(TarEntryType.Directory, entry.GUID.ToString("N") + "/");
			_tarWriter.WriteEntry(dirTarEntry);

			if (entry.DataStream == null) throw new Exception("Entry has no data stream!");
			var assetTarEntry = new GnuTarEntry(TarEntryType.RegularFile, entry.GUID.ToString("N") + "/asset")
			{
				DataStream = entry.DataStream
			};
			_tarWriter.WriteEntry(assetTarEntry);

			if (entry.Metadata == null) throw new Exception("Entry has no metadata!");

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
