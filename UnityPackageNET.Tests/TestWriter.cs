using System.Text;

namespace UnityPackageNET.Tests
{
	public class TestWriter
	{
		[Fact]
		public void TestWriteEntry()
		{
			using var ms = new MemoryStream();
			using (var writer = new UnityPackageWriter(ms, leaveOpen: true))
			{
				var entry = UnityPackageEntryFactory.MakeEmptyEntry("Assets/test.txt");
				entry.DataStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, Unity Package!"));
				writer.WriteEntry(entry);
			}

			ms.Position = 0;
			using var reader = new UnityPackageReader(ms);
			var readEntry = reader.GetNextEntry();
			Assert.NotNull(readEntry);
			Assert.NotNull(readEntry.DataStream);

			using (var sr = new StreamReader(readEntry.DataStream, leaveOpen: true))
			{
				var content = sr.ReadToEnd();
				Assert.Equal("Hello, Unity Package!", content);
			}

			reader.GetMetadata(readEntry);
			Assert.NotNull(readEntry.Metadata);
			Assert.Equal("Assets/test.txt", readEntry.Metadata.PathName);

		}
	}
}
