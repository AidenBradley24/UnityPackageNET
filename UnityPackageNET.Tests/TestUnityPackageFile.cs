using Xunit.Abstractions;

namespace UnityPackageNET.Tests
{
	public class TestUnityPackageFile
	{
		private readonly ITestOutputHelper _output;

		public TestUnityPackageFile(ITestOutputHelper output)
		{
			_output = output;
		}

		[Fact]
		public void TestRoundTrip()
		{
			using var ms = new MemoryStream();
			var meta = new UnityAssetMetadata(Guid.NewGuid());
			meta.SaveToStream(ms);

			ms.Position = 0;
			using (var reader = new StreamReader(ms, leaveOpen: true))
			{
				var text = reader.ReadToEnd();
				_output.WriteLine(text);
			}

			ms.Position = 0;
			var loaded = UnityAssetMetadata.CreateFromStream(ms);
		}

		[Fact]
		public void TestExtractToDirectory()
		{
			var source = File.OpenRead("test.unitypackage");
			var destDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			UnityPackageFile.ExtractToDirectory(source, destDir, overwriteFiles: true);
			var expectedFiles = new[]
			{
				Path.Combine(destDir, "Assets/Sprites/Square.png"),
				Path.Combine(destDir, "Assets/Sprites/Hexagon.png"),
				Path.Combine(destDir, "Assets/Sprites/Square.png.meta"),
				Path.Combine(destDir, "Assets/Sprites/Hexagon.png.meta")
			};
			foreach (var file in expectedFiles)
			{
				Assert.True(File.Exists(file), $"Expected file {file} to exist after extraction.");
			}
			Directory.Delete(destDir, recursive: true);
		}

		[Fact]
		public void TestCreateFromDirectory()
		{
			var sourceDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			using (var source = File.OpenRead("test.unitypackage"))
			{
				UnityPackageFile.ExtractToDirectory(source, sourceDir, overwriteFiles: true);
			}

			var destFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".unitypackage");

			using var destStream = File.Create(destFile);
			UnityPackageFile.CreateFromDirectory(sourceDir, destStream);
			Assert.True(File.Exists(destFile), $"Expected destination file {destFile} to be created.");
			var fileInfo = new FileInfo(destFile);
			Assert.True(fileInfo.Length > 0, $"Expected destination file {destFile} to have non-zero length.");

			File.Delete(destFile);
			Directory.Delete(sourceDir, recursive: true);
			File.Delete(destFile);
		}
	}
}
