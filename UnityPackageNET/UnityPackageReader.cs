using System.Diagnostics.CodeAnalysis;
using System.Formats.Tar;
using System.IO.Compression;

namespace UnityPackageNET
{
	/// <summary>
	/// Read Unity package (.unitypackage) files.
	/// </summary>
	public class UnityPackageReader : IDisposable
	{
		private readonly bool _leaveOpen;
		private bool _disposed;

		private readonly TarReader _tarReader;
		private readonly GZipStream _gzipStream;
		private readonly Stream _backingStream;

		UnityPackageEntry? currentEntry = null;

		/// <summary>
		/// Initializes a new instance of the UnityPackageReader class to read Unity package data from the specified stream.
		/// </summary>
		/// <remarks>The UnityPackageReader decompresses the provided stream using GZip and reads the contents as a
		/// tar archive. Ensure the stream remains valid for the lifetime of the reader if leaveOpen is set to true.</remarks>
		/// <param name="stream">The stream from which to read the Unity package. The stream must support reading and be positioned at the start of
		/// the Unity package data.</param>
		/// <param name="leaveOpen">true to leave the underlying stream open after the UnityPackageReader is disposed; otherwise, false.</param>
		public UnityPackageReader(Stream stream, bool leaveOpen = false)
		{
			_backingStream = stream;
			_gzipStream = new GZipStream(_backingStream, CompressionMode.Decompress, leaveOpen: true);
			_tarReader = new TarReader(_gzipStream, leaveOpen: true);
			_leaveOpen = leaveOpen;
		}

		private bool TryGetRegularFileEntry(Guid expectedGuid, string expectedName, [NotNullWhen(true)] out TarEntry? entry)
		{
			entry = _tarReader.GetNextEntry();
			if (entry == null || entry.EntryType != TarEntryType.RegularFile)
			{
				throw new InvalidDataException($"Expected a regular file entry.");
			}

			if (!entry.Name.StartsWith(expectedGuid.ToString("N") + "/") || !entry.Name.EndsWith("/" + expectedName))
			{
				return false;
			}

			return true;
		}
		
		/// <summary>
		/// Get the next entry in the Unity package.
		/// </summary>
		/// <returns>The next UnityPackageEntry if available; otherwise, null if the end of the package is reached.</returns>
		public UnityPackageEntry? GetNextEntry()
		{
			ObjectDisposedException.ThrowIf(_disposed, this);

			TarEntry? guidEntry;
			while (true)
			{
				var tarEntry = _tarReader.GetNextEntry();
				if (tarEntry == null) return null;
				if (tarEntry.EntryType == TarEntryType.Directory)
				{
					// this is the start of a new entry, we can break out of the loop and process it
					guidEntry = tarEntry;
					break;
				}
				// otherwise, this is some unexpected entry that we should skip
			}

			var guid = Guid.Parse(guidEntry.Name.AsSpan()[..^1]);

			if (!TryGetRegularFileEntry(guid, "asset", out var assetFile))
			{
				// is likely a folder metadata entry, those are redundant
				return GetNextEntry();
			}

			var entry = new UnityPackageEntry(guid)
			{
				DataStream = assetFile.DataStream
			};

			currentEntry = entry;
			return entry;
		}

		/// <summary>
		/// Retrieves the metadata associated with the specified Unity package entry.
		/// </summary>
		/// <remarks>This method requires that the entry provided is the current entry returned by GetNextEntry. It
		/// loads the metadata from the corresponding data streams.</remarks>
		/// <param name="entry">The UnityPackageEntry for which metadata is to be retrieved. Must be the current entry returned by GetNextEntry.</param>
		/// <returns>An instance of UnityAssetMetadata containing the metadata for the specified entry.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the specified entry is not the current entry or if the entry is null.</exception>
		/// <exception cref="InvalidDataException">Thrown if the expected 'asset.meta' or 'pathname' entry for the specified GUID is not found.</exception>
		public UnityAssetMetadata GetMetadata(UnityPackageEntry entry)
		{
			if (currentEntry != entry || entry == null)
			{
				throw new InvalidOperationException("Metadata can only be read for the current entry returned by GetNextEntry.");
			}
			;

			entry.DataStream = null;

			if (!TryGetRegularFileEntry(entry.GUID, "asset.meta", out var metadataEntry))
			{
				throw new InvalidDataException($"Expected an 'asset.meta' entry for GUID {entry.GUID}.");
			}

			var metadata = new UnityAssetMetadata(entry.GUID);
			metadata.LoadStream(metadataEntry.DataStream!);

			if (!TryGetRegularFileEntry(entry.GUID, "pathname", out var pathnameEntry))
			{
				throw new InvalidDataException($"Expected a 'pathname' entry for GUID {entry.GUID}.");
			}

			using var streamReader = new StreamReader(pathnameEntry.DataStream!, leaveOpen: true);
			var pathName = streamReader.ReadToEnd().TrimEnd('\n', '\r');
			metadata.PathName = pathName;
			entry.Metadata = metadata;
			return metadata;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			if (_disposed) return;
			_disposed = true;

			_tarReader.Dispose();
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
