﻿using System;

namespace InvertedTomato.Compression.Integers {
	public interface IUnsignedWriter : IDisposable {
		void Write(UInt64 value);

		// static byte[] Write (params ulong value);
		// static byte[] Write (IEnumerable<ulong> values);
	}
}