namespace UnityPackageNET
{
	public class UnityPackageEntry
	{
		public Guid GUID { get; }

		public Stream? DataStream { get; set; }

		public UnityAssetMetadata? Metadata { get; set; }

		internal UnityPackageEntry(Guid guid)
		{
			GUID = guid;
		}
	}
}
