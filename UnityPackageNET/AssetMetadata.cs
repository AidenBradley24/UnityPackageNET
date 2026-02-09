using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.RepresentationModel;

namespace UnityPackageNET
{
	public class AssetMetadata
	{
		YamlStream _yamlStream;

		public Guid Guid { get; }
		public string PathName { get; set; }

		public YamlDocument Document => _yamlStream.Documents[0];

		internal AssetMetadata(Guid guid)
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

		internal void LoadFromStream(Stream stream)
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

		internal void SaveToStream(Stream stream)
		{
			using var writer = new StreamWriter(stream, leaveOpen: true);
			_yamlStream.Save(writer);
		}
	}
}
