using System.Text.Json;
using YamlDotNet.RepresentationModel;

namespace UnityPackageNET.Metadata
{
	/// <summary>
	/// Represents metadata used for a built-in asset importer in Unity. Can be used in conjunction with Unity's AssetPostprocessor.
	/// <br/>
	/// Custom data can be written with <see cref="WriteUserData(IReadOnlyDictionary{string, object})"/>
	/// <br/>
	/// <br/>
	/// <a href="https://docs.unity3d.com/Manual/ScriptedImporters.html">Unity Importer Documentation</a>	
	/// </summary>
	public class AssetImporter
	{
		/// <summary>
		/// The YAML document root of the asset importer. Add properties of the built-in importer to this node.
		/// </summary>
		public YamlMappingNode Root { get; }

		/// <summary>
		/// Initializes a new instance of the AssetImporter class using the specified asset metadata and built-in type key.
		/// </summary>
		/// <param name="metadata">The metadata object of the asset</param>
		/// <param name="builtInType">The type of built-in importer. i.e. TextureImporter</param>
		public AssetImporter(UnityAssetMetadata metadata, string builtInType)
		{
			ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));
			ArgumentNullException.ThrowIfNullOrWhiteSpace(builtInType, nameof(builtInType));
			Root = new YamlMappingNode();
			metadata.Root.Add(builtInType, Root);
		}

		/// <summary>
		/// Writes custom data to the userData property of the asset importer in a JSON format.
		/// </summary>
		/// <param name="userData">A dictionary containing user data to write. The keys represent property names, and the values represent
		/// corresponding property values.</param>
		public void WriteUserData(IReadOnlyDictionary<string, object> userData)
		{
			ArgumentNullException.ThrowIfNull(userData, nameof(userData));
			string s = JsonSerializer.Serialize(userData);
			Root.Add("userData", new YamlScalarNode(s));
		}
	}
}
