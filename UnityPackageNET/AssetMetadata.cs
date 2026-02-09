using System;
using System.Collections.Generic;
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
		}

		internal void LoadFromStream(Stream stream)
		{
			_yamlStream = new YamlStream();
			using var streamReader = new StreamReader(stream, leaveOpen: true);
			_yamlStream.Load(streamReader);
		}

		internal void SaveToStream(Stream stream)
		{
			using var writer = new StreamWriter(stream, leaveOpen: true);
			_yamlStream.Save(writer);
		}
	}
}
