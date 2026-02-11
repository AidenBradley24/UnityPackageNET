using YamlDotNet.RepresentationModel;

namespace UnityPackageNET
{
	/// <summary>
	/// Any YAML-based Unity asset file, such as a .prefab, .mat, .asset, .controller, etc.
	/// </summary>
	public class UnityAsset
	{
		public YamlMappingNode Root { get; }

		public uint AssetType { get; set; }

		public string ObjectID { get; set; } = "";

		public UnityAsset()
		{
			Root = new YamlMappingNode();
		}

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

		public MemoryStream ToStream() 
		{ 
			var ms = new MemoryStream();
			Save(ms, leaveOpen: true);
			ms.Position = 0;
			return ms;
		}
	}

	public enum AssetType : uint
	{
		GameObject = 1,
		Material = 21,
		ScriptableObject = 114,
		AnimationClip = 74,
		// non exhaustive list, add more as needed
	}
}
