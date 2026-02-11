# UnityPackageNET

[![NuGet Version](https://img.shields.io/nuget/v/UnityPackageNET)](https://www.nuget.org/packages/UnityPackageNET/)

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
		entry.DataStream!.CopyTo(ms);
		ms.Position = 0;

		// you must call reader.GetMetadata to access the metadata afterward
		UnityAssetMetadata? metadata = reader.GetMetadata(entry);

		Console.WriteLine($"Found entry: {entry.PathName}");
		entry = reader.GetNextEntry();
	}

```

### Create a Unity Package file
```csharp
	var fs = File.Create("output.unitypackage");
	using var writer = new UnityPackageWriter(fs);

	var entry = UnityPackageEntryFactory.MakeEmptyEntry("Assets/MyAsset.txt");

	// access metadata YAML
	entry.Metadata.Root.Add("myKey", "myValue");

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

### Creating a Unity Package file from a directory
```csharp
	using var fs = File.Create("output.unitypackage");
	UnityPackageFile.CreateFromDirectory("C:\\SRC_DIR", fs);
```

### Extracting a Unity Package file to a directory	
```csharp
	using var fs = File.OpenRead("output.unitypackage");
	UnityPackageFile.ExtractToDirectory(fs, "C:\\TARGET_DIR", overwriteFiles: true);
```

### Create YAML-based Asset Files
- ScriptableObject instances
- Prefabs
- Materials
- etc.
```csharp
	Guid scriptGuid = Guid.Parse("...unity-script-guid-here...");;
	var (asset, metadata, monoBehaviorNode) = UnityAssetFactory.MakeScriptableObject("Assets/MyScriptableObject.asset", scriptGuid);
	UnityPackageEntry entry = UnityPackageEntryFactory.Combine(asset, metadata);
	// then you can write the entry to a Unity Package file as shown above
```

You can create other types of assets seperately and attach the data stream to the UnityPackageEntry.
