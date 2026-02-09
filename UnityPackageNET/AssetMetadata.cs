using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Formats.UnityPackage
{
	public class AssetMetadata
	{
		public Guid Guid { get; }
		public string PathName { get; set; }

		internal AssetMetadata(Guid guid)
		{
			Guid = guid;
		}
	}
}
