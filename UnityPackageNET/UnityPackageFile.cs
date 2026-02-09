using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace UnityPackageNET
{
	public static class UnityPackageFile
	{
		public const string FileExtension = ".unitypackage";

		public static void CreateFromDirectory(string sourceDirectory, Stream destination)
		{

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
