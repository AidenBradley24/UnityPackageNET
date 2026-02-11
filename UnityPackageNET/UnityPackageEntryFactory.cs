namespace UnityPackageNET
{
	/// <summary>
	/// Create UnityPackageEntry instance.
	/// </summary>
	public static class UnityPackageEntryFactory
	{
		/// <summary>
		/// Combine a YAML-based UnityAsset and its corresponding UnityAssetMetadata into a single UnityPackageEntry.
		/// </summary>
		public static UnityPackageEntry Combine(UnityAsset asset, UnityAssetMetadata metadata)
		{
			var entry = new UnityPackageEntry(metadata.Guid)
			{
				DataStream = asset.ToStream(),
				Metadata = metadata
			};
			return entry;
		}

		/// <summary>
		/// Create an empty UnityPackageEntry with a new GUID and specified pathname.
		/// </summary>
		/// <param name="pathname">The asset's file path in a Unity project including the file name. i.e. "Assets/Textures/MyTexture.png"</param>
		public static UnityPackageEntry MakeEmptyEntry(string pathname)
		{
			var entry = new UnityPackageEntry(Guid.NewGuid());
			entry.Metadata = new UnityAssetMetadata(entry.GUID)
			{
				PathName = pathname
			};
			return entry;
		}
		
		/// <summary>
		/// Create a UnityPackageEntry from a metadata stream and specified pathname.
		/// </summary>
		/// <param name="pathname">The asset's file path in a Unity project including the file name. i.e. "Assets/Textures/MyTexture.png"</param>
		/// <param name="metadataStream">A stream containing the asset's metadata.</param>
		public static UnityPackageEntry FromMetadataStream(string pathname, Stream metadataStream)
		{
			var assetMetadata = UnityAssetMetadata.CreateFromStream(metadataStream);
			assetMetadata.PathName = pathname;
			var entry = new UnityPackageEntry(assetMetadata.Guid)
			{
				Metadata = assetMetadata
			};
			return entry;
		}
	}
}
