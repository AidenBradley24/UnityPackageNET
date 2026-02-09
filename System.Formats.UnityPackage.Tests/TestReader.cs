using System.Diagnostics;

namespace System.Formats.UnityPackage.Tests
{
	public class TestReader
	{
		static UnityPackageReader OpenReader()
		{
			var fs = File.OpenRead("test.unitypackage");
			return new UnityPackageReader(fs);
		}


		[Fact]
		public void TestFile()
		{
			using var reader = OpenReader();

			List<string> expectedPaths =
			[
				"Assets/Sprites/Square.png",
				"Assets/Sprites/Hexagon.png"
			];

			AssetMetadata? metadata = null;
			UnityPackageEntry? entry = reader.GetNextEntry();
			int i = 0;
			while (entry != null)
			{
				metadata = reader.GetMetadata(entry);
				Assert.NotNull(metadata);
				Assert.NotNull(entry.Metadata);
				Assert.Null(entry.DataStream);
				Assert.Equal(metadata, entry.Metadata);
				Assert.Equal(entry.GUID, metadata.Guid);
				Assert.Equal(expectedPaths[i++], metadata.PathName);

				entry = reader.GetNextEntry();
			}
		}
	}
}