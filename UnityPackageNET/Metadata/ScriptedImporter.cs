using YamlDotNet.RepresentationModel;

namespace UnityPackageNET.Metadata
{
	/// <summary>
	/// Represents metadata used for a custom asset importer in Unity. 
	/// Allows you to define the import settings and other metadata for a custom asset type that is imported using a ScriptedImporter in Unity.
	/// When using built-in asset type like a texture, model, audio file, etc., use the <see cref="AssetImporter"/> class instead.
	/// <br/>
	/// <br/>
	/// <a href="https://docs.unity3d.com/Manual/ScriptedImporters.html">Unity Importer Documentation</a>
	/// </summary>
	public class ScriptedImporter
	{
		/// <summary>
		/// The YAML document root of the asset importer. Add properties of the built-in importer to this node.
		/// </summary>
		public YamlMappingNode Root { get; }

		/// <summary>
		/// Initializes a new instance of the ScriptedImporter class using the specified asset metadata and importer script GUID.
		/// </summary>
		/// <param name="metadata">The metadata object of the asset</param>
		/// <param name="importerScriptGuid">The GUID of the importer script</param>
		public ScriptedImporter(UnityAssetMetadata metadata, Guid importerScriptGuid)
		{
			ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));

			Root = new YamlMappingNode
			{
				{ "externalObjects", new YamlMappingNode() },
				{ "userData", new YamlScalarNode("") },
				{ "assetBundleName", new YamlScalarNode("") },
				{ "assetBundleVariant", new YamlScalarNode("") },
				{
					"script", new YamlMappingNode
					{
						{ "fileID", new YamlScalarNode("11500000") },
						{ "guid", new YamlScalarNode(importerScriptGuid.ToString("N")) }
					}
				}
			};

			metadata.Root.Add("ScriptedImporter", Root);
		}
	}
}
