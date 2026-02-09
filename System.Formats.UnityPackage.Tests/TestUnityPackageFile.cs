using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPackageNET.Tests
{
	public class TestUnityPackageFile
	{
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
	}
}
