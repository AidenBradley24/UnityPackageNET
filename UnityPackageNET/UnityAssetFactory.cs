using YamlDotNet.RepresentationModel;

namespace UnityPackageNET
{
	/// <summary>
	/// Common factory methods for creating Unity assets. 
	/// These methods create the asset's YAML structure and metadata with the necessary properties and values for Unity to recognize and import the asset correctly. 
	/// You can further customize the returned UnityAsset and UnityAssetMetadata objects to add additional properties or modify existing ones as needed.
	/// </summary>
	public static class UnityAssetFactory
	{
		/// <summary>
		/// Make a scriptable object instance. Note that the returned UnityAsset's Root will contain a "MonoBehaviour" node. 
		/// This is where you add any additional properties specific to your scriptable object.
		/// </summary>
		/// <param name="pathname">The asset's file path in a Unity project including the file name. 
		/// i.e. "Assets/Fruits/Apple.asset" where "Apple" is the scriptable object's name.</param>
		/// <param name="scriptGuid">The GUID of the ScriptableObject script</param>
		/// <returns></returns>
		public static (UnityAsset asset, UnityAssetMetadata metadata, YamlMappingNode monoBehaviorRoot) MakeScriptableObject(string pathname, Guid scriptGuid)
		{
			string name = Path.GetFileNameWithoutExtension(pathname);

			var asset = new UnityAsset
			{
				AssetType = (uint)AssetType.ScriptableObject,
				ObjectID = "&11400000"
			};

			var monoBehaviourNode = new YamlMappingNode
			{
				{ "m_ObjectHideFlags", "0" },
				{ "m_CorrespondingSourceObject", new YamlMappingNode { { "fileID", "0" } } },
				{ "m_PrefabInstance", new YamlMappingNode { { "fileID", "0" } } },
				{ "m_PrefabAsset", new YamlMappingNode { { "fileID", "0" } } },
				{ "m_GameObject", new YamlMappingNode { { "fileID", "0" } } },
				{ "m_Enabled", "1" },
				{ "m_EditorHideFlags", "0" },
				{ "m_Script", new YamlMappingNode
					{
						{ "fileID", "11500000" },
						{ "guid", scriptGuid.ToString("N") },
						{ "type", "3" }
					}
				},
				{ "m_Name", name },
				{ "m_EditorClassIdentifier", "" }
			};
			asset.Root.Add("MonoBehaviour", monoBehaviourNode);

			var metadata = new UnityAssetMetadata(Guid.NewGuid())
			{
				PathName = pathname
			};
			metadata.Root.Add("NativeFormatImporter", new YamlMappingNode
			{
				{ "externalObjects", new YamlMappingNode() },
				{ "mainObjectFileID", "11400000" },
				{ "userData", string.Empty },
				{ "assetBundleName", string.Empty },
				{ "assetBundleVariant", string.Empty }
			});

			return (asset, metadata, monoBehaviourNode);
		}
	}
}
