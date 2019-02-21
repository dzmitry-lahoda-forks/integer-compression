using System;
using System.Collections.Generic;
using System.IO;

namespace InvertedTomato.Compression.Integers.Wave3 {
	public class ListByteWrapper : IByteWriter {
		private readonly List<Byte> Underlying;

		public ListByteWrapper(List<Byte> underlying) {
			if (null == underlying) {
				throw new ArgumentNullException(nameof(underlying));
			}

			Underlying = underlying;
		}

		public void WriteByte(Byte value) {
			Underlying.Add(value);
		}
	}
}