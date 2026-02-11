namespace UnityPackageNET
{
	/// <summary>
	/// Common operations for Unity package (.unitypackage) files.
	/// </summary>
	public static class UnityPackageFile
	{
		/// <summary>
		/// The file extension used for Unity package files.
		/// </summary>
		public const string FileExtension = ".unitypackage";

		/// <summary>
		/// Creates a Unity package archive from the specified source directory and writes it to the provided destination
		/// stream.
		/// </summary>
		/// <remarks>Each asset file in the source directory must have a corresponding .meta file for the Unity
		/// package to be valid. Duplicate file paths are not allowed and will result in an exception. The method ensures
		/// package integrity by enforcing these requirements.</remarks>
		/// <param name="sourceDirectory">The path to the directory containing the asset files and their corresponding .meta files to include in the Unity
		/// package. The directory must exist and contain all required files.</param>
		/// <param name="destination">A writable stream to which the Unity package will be written.</param>
		/// <exception cref="DirectoryNotFoundException">Thrown if the specified source directory does not exist.</exception>
		/// <exception cref="InvalidOperationException">Thrown if duplicate file paths are detected or if any asset file is missing its required .meta file.</exception>
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
					entry = UnityPackageEntryFactory.FromMetadataStream(filePath, metaFs);
				}
				var realFilePath = Path.Combine([sourceDirectory, .. filePath.Split('/')]);
				using var fs = new FileStream(realFilePath, FileMode.Open, FileAccess.Read);
				entry.DataStream = fs;
				writer.WriteEntry(entry);
			}
		}

		/// <summary>
		/// Extracts the contents of a Unity package from the specified stream to a directory, creating the directory if it
		/// does not exist and optionally overwriting existing files.
		/// </summary>
		/// <param name="source">The input stream containing the Unity package data to extract. The stream must be readable and positioned at the
		/// start of the package data.</param>
		/// <param name="destinationDirectoryName">The path to the directory where the package contents will be extracted. If the directory does not exist, it will
		/// be created.</param>
		/// <param name="overwriteFiles">A value indicating whether to overwrite existing files in the destination directory. If set to <see
		/// langword="true"/>, existing files will be replaced; otherwise, an exception is thrown if a file already exists.</param>
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
