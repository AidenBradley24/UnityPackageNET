using YamlDotNet.RepresentationModel;

namespace UnityPackageNET
{
	/// <summary>
	/// The metadata of a Unity asset, stored in the .meta file. Contains the asset's GUID and other import settings.
	/// </summary>
	public class UnityAssetMetadata
	{
		/// <summary>
		/// An asset's unique identifier
		/// </summary>
		public Guid Guid { get; }

		/// <summary>
		/// The asset's file path in a Unity project including the file name. i.e. "Assets/Textures/MyTexture.png"
		/// </summary>
		public string PathName { get; set; } = "";

		/// <summary>
		/// The YAML document root of the metadata file. This is where you can add properties and values to define the asset's import settings and other metadata.
		/// </summary>
		public YamlMappingNode Root { get; set; }

		/// <summary>
		/// Create a blank metadata file.
		/// </summary>
		public UnityAssetMetadata(Guid guid)
		{
			Guid = guid;

			Root = new YamlMappingNode
			{
				{ "fileFormatVersion", new YamlScalarNode("2") },
				{ "guid", new YamlScalarNode(Guid.ToString("N")) }
			};
		}

		internal void LoadStream(Stream stream)
		{
			using var streamReader = new StreamReader(stream, leaveOpen: true);

			var yamlStream = new YamlStream();
			yamlStream.Load(streamReader);

			if (yamlStream.Documents.Count == 0)
			{
				var root = new YamlMappingNode
				{
					{ "fileFormatVersion", new YamlScalarNode("2") },
					{ "guid", new YamlScalarNode(Guid.ToString("N")) }
				};

				yamlStream.Add(new YamlDocument(root));
				return;
			}

			if (yamlStream.Documents.Count == 0 || yamlStream.Documents[0].RootNode is not YamlMappingNode mapping)
			{
				throw new InvalidDataException("Invalid YAML format: Root node is not a mapping node.");
			}

			mapping.Children["fileFormatVersion"] = new YamlScalarNode("2");
			mapping.Children["guid"] = new YamlScalarNode(Guid.ToString("N"));
			Root = mapping;
		}

		/// <summary>
		/// Save the metadata to a stream in the YAML format expected by Unity. This will include the GUID and any other properties defined in the Root node.
		/// </summary>
		public void SaveToStream(Stream stream)
		{
			using var writer = new StreamWriter(stream, leaveOpen: true);
			var yamlStream = new YamlStream(new YamlDocument(Root));
			yamlStream.Save(writer, assignAnchors: false);
			writer.Flush();
		}

		/// <summary>
		/// Creates a UnityAssetMetadata instance from an existing metadata file stream.
		/// </summary>
		public static UnityAssetMetadata CreateFromStream(Stream stream)
		{
			YamlStream yamlStream;
			using (var streamReader = new StreamReader(stream, leaveOpen: true))
			{
				yamlStream = new YamlStream();
				yamlStream.Load(streamReader);
			}

			if (yamlStream.Documents.Count == 0)
			{
				var root = new YamlMappingNode
				{
					{ "fileFormatVersion", new YamlScalarNode("2") },
					{ "guid", new YamlScalarNode(Guid.NewGuid().ToString("N")) }
				};

				yamlStream.Add(new YamlDocument(root));
			}


			if (yamlStream.Documents[0].RootNode is not YamlMappingNode mapping)
			{
				throw new InvalidDataException("Invalid YAML format: Root node is not a mapping node.");
			}

			if (!mapping.Children.ContainsKey("guid"))
			{
				throw new InvalidDataException("Invalid YAML format: Missing 'guid' field.");
			}

			if (!mapping.Children.ContainsKey("fileFormatVersion"))
			{
				mapping.Children["fileFormatVersion"] = new YamlScalarNode("2");
			}

			Guid guid = Guid.Parse((string)mapping.Children["guid"]!);
			var assetMetadata = new UnityAssetMetadata(guid);
			assetMetadata.Root = mapping;
			return assetMetadata;
		}
	}
}
