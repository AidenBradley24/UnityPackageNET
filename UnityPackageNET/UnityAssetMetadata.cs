using YamlDotNet.RepresentationModel;

namespace UnityPackageNET
{
	public class UnityAssetMetadata
	{
		YamlStream _yamlStream;

		public Guid Guid { get; }
		public string PathName { get; set; }

		public YamlDocument Document => _yamlStream.Documents[0];

		public UnityAssetMetadata(Guid guid)
		{
			Guid = guid;
			_yamlStream = new YamlStream();

			var root = new YamlMappingNode
			{
				{ "fileFormatVersion", new YamlScalarNode("2") },
				{ "guid", new YamlScalarNode(Guid.ToString("N")) }
			};

			_yamlStream.Add(new YamlDocument(root));
		}

		internal void LoadStream(Stream stream)
		{
			using var streamReader = new StreamReader(stream, leaveOpen: true);
			_yamlStream.Load(streamReader);

			if (_yamlStream.Documents.Count == 0)
			{
				var root = new YamlMappingNode
				{
					{ "fileFormatVersion", new YamlScalarNode("2") },
					{ "guid", new YamlScalarNode(Guid.ToString("N")) }
				};

				_yamlStream.Add(new YamlDocument(root));
				return;
			}

			if (_yamlStream.Documents[0].RootNode is YamlMappingNode mapping)
			{
				mapping.Children["fileFormatVersion"] = new YamlScalarNode("2");
				mapping.Children["guid"] = new YamlScalarNode(Guid.ToString("N"));
			}
		}

		public void SaveToStream(Stream stream)
		{
			using var writer = new StreamWriter(stream, leaveOpen: true);
			_yamlStream.Save(writer, assignAnchors: false);
			writer.Flush();
		}

		public static UnityAssetMetadata LoadFromStream(Stream stream)
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
				throw new Exception("Invalid YAML format: Root node is not a mapping node.");
			}

			Guid guid = Guid.Parse((string)mapping.Children["guid"]!);
			var assetMetadata = new UnityAssetMetadata(guid)
			{
				_yamlStream = yamlStream
			};
			return assetMetadata;
		}
	}
}
