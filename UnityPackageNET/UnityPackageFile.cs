namespace UnityPackageNET
{
	public static class UnityPackageFile
	{
		public const string FileExtension = ".unitypackage";

		public static void CreateFromDirectory(string sourceDirectory, Stream destination)
		{
			using var writer = new UnityPackageWriter(destination);
			var dir = new DirectoryInfo(sourceDirectory);
			if (!dir.Exists) throw new DirectoryNotFoundException($"Source directory '{sourceDirectory}' does not exist.");

			HashSet<string> filePaths = [];
			HashSet<string> metaPaths = [];
			dir.EnumerateFiles("*", SearchOption.AllDirectories).ToList().ForEach(file =>
			{
				var pathname = Path.GetRelativePath(sourceDirectory, file.FullName).Replace(Path.DirectorySeparatorChar, '/');
				var paths = pathname.EndsWith(".meta") ? metaPaths : filePaths;
				if (paths.Contains(pathname))
				{
					throw new InvalidOperationException($"Duplicate file path detected: '{pathname}'. Unity packages cannot contain duplicate paths.");
				}
				paths.Add(pathname);
			});

			foreach (var filePath in filePaths)
			{
				if (!metaPaths.Contains(filePath + ".meta"))
				{
					throw new InvalidOperationException($"Missing .meta file for '{filePath}'. Every asset file must have a corresponding .meta file in a Unity package.");
				}

				UnityPackageEntry entry;

				var realMetaFilePath = Path.Combine([sourceDirectory, .. filePath.Split('/')]) + ".meta";
				using (var metaFs = new FileStream(realMetaFilePath, FileMode.Open, FileAccess.Read))
				{
					entry = UnityPackageFactory.FromMetadataStream(filePath, metaFs);
				}
				var realFilePath = Path.Combine([sourceDirectory, .. filePath.Split('/')]);
				using var fs = new FileStream(realFilePath, FileMode.Open, FileAccess.Read);
				entry.DataStream = fs;
				writer.WriteEntry(entry);
			}
		}

		public static void ExtractToDirectory(Stream source, string destinationDirectoryName, bool overwriteFiles)
		{
			using var reader = new UnityPackageReader(source);
			var dir = new DirectoryInfo(destinationDirectoryName);
			if (!dir.Exists)
			{
				dir.Create();
			}

			while (true)
			{
				var entry = reader.GetNextEntry();
				if (entry == null)
				{
					break;
				}

				var tmpFile = new FileInfo(Path.GetTempFileName());
				using (var fs = tmpFile.OpenWrite())
				{
					entry.DataStream!.CopyTo(fs);
				}

				var metadata = reader.GetMetadata(entry);
				var filePath = Path.Combine(dir.FullName, metadata.PathName.Replace('/', Path.DirectorySeparatorChar));
				var fileDir = Path.GetDirectoryName(filePath);
				if (fileDir != null && !Directory.Exists(fileDir))
				{
					Directory.CreateDirectory(fileDir);
				}

				tmpFile.MoveTo(filePath, overwriteFiles);

				var metadataFile = filePath + ".meta";
				using (var fs = new FileStream(metadataFile, overwriteFiles ? FileMode.Create : FileMode.CreateNew, FileAccess.Write))
				{
					metadata.SaveToStream(fs);
				}
			}
		}
	}
}
