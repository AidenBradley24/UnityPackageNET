using System.Diagnostics.CodeAnalysis;
using System.Formats.Tar;
using System.IO.Compression;
using YamlDotNet.RepresentationModel;

namespace UnityPackageNET
{
    public class UnityPackageReader : IDisposable
    {
        private readonly bool _leaveOpen;
        private bool _disposed;

        private readonly TarReader _tarReader;
        private readonly GZipStream _gzipStream;
        private readonly Stream _backingStream;

        UnityPackageEntry? currentEntry = null;

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

        public AssetMetadata GetMetadata(UnityPackageEntry entry)
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

            var metadata = new AssetMetadata(entry.GUID);
            metadata.LoadFromStream(metadataEntry.DataStream!);

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
