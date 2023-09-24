using System.Collections.Generic;
using System.Linq;

namespace RidgeDrawer
{
	public static class StringExtension
	{
		public static IEnumerable<string> Chunk(this string s, int chunkSize)
		{
			var chunks = s.Length / chunkSize + (s.Length % chunkSize == 0 ? 0 : 1);
			return Enumerable.Range(0, chunks)
				.Select(i => (i == chunks - 1) ? s.Substring(i * chunkSize) : s.Substring(i * chunkSize, chunkSize));
		}
	}
}
