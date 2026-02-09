using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPackageNET
{
	public class UnityPackageBuilder
	{
		private readonly List<UnityPackageEntry> _entries = [];

		public UnityPackageEntry MakeEmptyEntry(string assetFileName)
		{
			var entry = new UnityPackageEntry(Guid.NewGuid());
			entry.Metadata = new AssetMetadata(entry.GUID)
			{
				PathName = assetFileName
			};
			_entries.Add(entry);
			return entry;
		}

		public IEnumerable<UnityPackageEntry> GetAllEntries()
		{
			return _entries;
		}
	}
}
