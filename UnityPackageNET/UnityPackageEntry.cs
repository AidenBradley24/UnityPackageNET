namespace UnityPackageNET
{
	/// <summary>
	/// A combination of a Unity asset file and its corresponding metadata file. This is the main data structure used when working with Unity packages in this library.
	/// </summary>
	public class UnityPackageEntry
	{
		/// <summary>
		/// The unique identifier (GUID) of the Unity asset.
		/// </summary>
		public Guid GUID { get; }

		/// <summary>
		/// A stream defining the contents of the Unity asset file.
		/// </summary>
		public Stream? DataStream { get; set; }

		/// <summary>
		/// The asset's metadata, which includes the GUID, file path, and other import settings. This corresponds to the .meta file in a Unity project.
		/// </summary>
		public UnityAssetMetadata? Metadata { get; set; }

		internal UnityPackageEntry(Guid guid)
		{
			GUID = guid;
		}
	}
}
