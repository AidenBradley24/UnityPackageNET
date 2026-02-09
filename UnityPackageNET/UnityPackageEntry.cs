using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPackageNET
{
	public class UnityPackageEntry
	{
		public Guid GUID { get; }
		
		public Stream? DataStream { get; set; }

		public AssetMetadata? Metadata { get; set; }

		internal UnityPackageEntry(Guid guid)
		{
			GUID = guid;
		}
	}
}
