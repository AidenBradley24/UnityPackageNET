using YamlDotNet.RepresentationModel;

namespace UnityPackageNET
{
	/// <summary>
	/// Any YAML-based Unity asset file, such as a .prefab, .mat, .asset, .controller, etc.
	/// </summary>
	public class UnityAsset
	{
		/// <summary>
		/// The root YAML node of the asset file. This is where you can add properties and values to define the asset's data. When saving, this will be serialized into the YAML format expected by Unity.
		/// </summary>
		public YamlMappingNode Root { get; set; }

		/// <summary>
		/// Gets or sets the type of asset represented by this instance.
		/// </summary>
		public uint AssetType { get; set; }

		/// <summary>
		/// A number starting with '&amp;' that uniquely identifies the asset within the YAML document. This is used in the YAML header to reference the asset's data.
		/// </summary>
		public string ObjectID { get; set; } = "";

		/// <summary>
		/// Create an empty Unity asset. You must set the AssetType and ObjectID properties, 
		/// and populate the Root node with the appropriate structure and values for the type of asset you are creating before saving.
		/// </summary>
		public UnityAsset()
		{
			Root = new YamlMappingNode();
		}

		/// <summary>
		/// Save the asset to a stream in the YAML format expected by Unity. 
		/// The AssetType and ObjectID properties must be set before calling this method, 
		/// and the Root node should be populated with the appropriate structure and values for the type of asset you are saving.
		/// </summary>
		/// <exception cref="Exception"></exception>
		public void Save(Stream stream, bool leaveOpen = false)
		{
			if (string.IsNullOrWhiteSpace(ObjectID) || !ObjectID.StartsWith('&'))
			{
				throw new Exception("ObjectID must be set and start with '&'");
			}

			using var writer = new StreamWriter(stream, leaveOpen: leaveOpen);

			writer.WriteLine("%YAML 1.1");
			writer.WriteLine("%TAG !u! tag:unity3d.com,2011:");
			writer.WriteLine($"--- !u!{AssetType} {ObjectID}");

			var yamlStream = new YamlStream(new YamlDocument(Root));
			yamlStream.Save(writer, assignAnchors: false);
		}

		/// <summary>
		/// Load the contents of an asset from a stream containing YAML data in the format expected by Unity.
		/// </summary>
		/// <exception cref="Exception"></exception>
		public void Load(Stream stream, bool leaveOpen = false)
		{
			using var reader = new StreamReader(stream, leaveOpen: leaveOpen);
			reader.ReadLine(); // skip %YAML header
			reader.ReadLine(); // skip %TAG header
			string s = reader.ReadLine() ?? throw new Exception();
			string[] bits = s.Trim().Split(' ');
			if (bits.Length != 3 || !bits[0].Equals("---") || !bits[1].StartsWith("!u!") || !bits[1].StartsWith('&'))
			{
				throw new Exception("Invalid YAML format: Expected header line in the format '--- !u!{AssetType} &{ObjectID}'");
			}
			AssetType = uint.Parse(bits[1][3..]);
			ObjectID = bits[2];

			var yamlStream = new YamlStream();
			yamlStream.Load(reader);
		}

		/// <summary>
		/// Get a MemoryStream representing the asset. Used to write the asset data to a Unity package entry.
		/// Note that disposing the MemoryStream is not necessary as it will be handled by the garbage collector.
		/// </summary>
		public MemoryStream ToStream()
		{
			var ms = new MemoryStream();
			Save(ms, leaveOpen: true);
			ms.Position = 0;
			return ms;
		}
	}

#pragma warning disable CS1591

	/// <summary>
	/// A unique number for each type of asset in Unity. This is used in the YAML header to indicate the type of asset being defined.
	/// </summary>
	public enum AssetType : uint
	{
		GameObject = 1,
		Material = 21,
		ScriptableObject = 114,
		AnimationClip = 74,
		// non exhaustive list, add more as needed

	}

#pragma warning restore CS1591

}
