namespace UnityPackageNET
{
	public static class UnityPackageFactory
	{
		public static UnityPackageEntry Combine(UnityAsset asset, UnityAssetMetadata metadata)
		{
			var entry = new UnityPackageEntry(metadata.Guid)
			{
				DataStream = asset.ToStream(),
				Metadata = metadata
			};
			return entry;
		}

		public static UnityPackageEntry MakeEmptyEntry(string assetFileName)
		{
			var entry = new UnityPackageEntry(Guid.NewGuid());
			entry.Metadata = new UnityAssetMetadata(entry.GUID)
			{
				PathName = assetFileName
			};
			return entry;
		}

		public static UnityPackageEntry FromMetadataStream(string assetFileName, Stream metadataStream)
		{
			var assetMetadata = UnityAssetMetadata.LoadFromStream(metadataStream);
			assetMetadata.PathName = assetFileName;
			var entry = new UnityPackageEntry(assetMetadata.Guid)
			{
				Metadata = assetMetadata
			};
			return entry;
		}
	}
}
