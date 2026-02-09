# UnityPackageNET

Read and write Unity Package files in .NET.

### Read a Unity Package file
```csharp
	var fs = File.OpenRead("test.unitypackage");
	using var reader = new UnityPackageReader(fs);
	UnityPackageEntry? entry = reader.GetNextEntry();
	while (entry != null)
	{
		// access the asset data stream first
		// in order to use the asset data later, you must copy elsewhere
		using var ms = new MemoryStream();
		reader.DataStream.CopyTo(ms);
		ms.Position = 0;

		// you must call reader.GetMetadata to access the metadata afterward
		UnityAssetMetadata? metadata = reader.GetMetadata(entry);

		Console.WriteLine($"Found entry: {entry.Name}");
		entry = reader.GetNextEntry();
	}

```

### Create a Unity Package file
```csharp
	var fs = File.Create("output.unitypackage");
	using var writer = new UnityPackageWriter(fs);

	var entry = UnityPackageFactory.MakeEmptyEntry("Assets/MyAsset.txt");
	
	// assign metadata
	entry.Metadata = new UnityAssetMetadata(Guid.NewGuid());

	// access metadata YAML file
	entry.Metadata.Document

	// write asset data stream
	using (var ms = new MemoryStream())
	{
		using var sw = new StreamWriter(ms);
		sw.WriteLine("This is my asset data.");
		sw.Flush();
		ms.Position = 0;
		entry.DataStream = ms;
		writer.WriteEntry(entry);
	}
```
