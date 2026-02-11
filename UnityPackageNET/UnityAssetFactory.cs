using YamlDotNet.RepresentationModel;

namespace UnityPackageNET
{
	public static class UnityAssetFactory
	{
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
			metadata.DocRoot.Add("NativeFormatImporter", new YamlMappingNode
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
