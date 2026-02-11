using UnityPackageNET;
using Xunit.Abstractions;

namespace UnityPackageNET.Tests
{
	public class TestUnityAsset
	{
		private readonly ITestOutputHelper _output;

		public TestUnityAsset(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void TestCreateAndSave()
		{
			var asset = new UnityAsset();
			asset.Root.Add("key", "value");
			asset.AssetType = 1;
			asset.ObjectID = "&12345678";
			using var ms = new MemoryStream();
			asset.Save(ms, leaveOpen: true);
			ms.Seek(0, SeekOrigin.Begin);
			using var reader = new StreamReader(ms);
			var content = reader.ReadToEnd();

			_output.WriteLine("Asset stream content:");
			_output.WriteLine(content);

			Assert.Contains("key: value", content);
		}

		[Fact]
		public void TestMakeScriptableObject()
		{
			var (asset, metadata, monoBehaviourNode) = UnityAssetFactory.MakeScriptableObject("Assets/MyScriptableObject.asset", Guid.NewGuid());
			Assert.Equal((uint)AssetType.ScriptableObject, asset.AssetType);
			Assert.Equal("&11400000", asset.ObjectID);
			Assert.True(monoBehaviourNode.Children.ContainsKey("m_Script"));
			Assert.Equal("Assets/MyScriptableObject.asset", metadata.PathName);
			Assert.True(metadata.DocRoot.Children.ContainsKey("NativeFormatImporter"));

			var ms = new MemoryStream();
			asset.Save(ms, leaveOpen: true);
			ms.Seek(0, SeekOrigin.Begin);

			_output.WriteLine("ScriptableObject asset stream content:");
			using var reader = new StreamReader(ms);
			var content = reader.ReadToEnd();
			_output.WriteLine(content);
		}
	}
}
