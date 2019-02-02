﻿using System;
using System.IO;

namespace InvertedTomato.Compression.Integers {
    /// <summary>
    ///     Reader for Elias Gamma universal coding adapted for signed values.
    /// </summary>
    public class EliasGammaSignedReader : ISignedReader {
        /// <summary>
        ///     The underlying unsigned reader.
        /// </summary>
        private readonly EliasGammaUnsignedReader Underlying;

        /// <summary>
        ///     Standard instantiation.
        /// </summary>
        /// <param name="input"></param>
        public EliasGammaSignedReader(Stream input) {
			Underlying = new EliasGammaUnsignedReader(input);
		}

        /// <summary>
        ///     If it's disposed.
        /// </summary>
        public Boolean IsDisposed { get; private set; }

        /// <summary>
        ///     Read the next value.
        /// </summary>
        /// <returns></returns>
        public Int64 Read() {
			return ZigZag.Decode(Underlying.Read());
		}

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose() {
			Dispose(true);
		}

        /// <summary>
        ///     Read first value from a byte array.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Int64 ReadOneDefault(Byte[] input) {
			if (null == input) {
				throw new ArgumentNullException(nameof(input));
			}

			using (var stream = new MemoryStream(input)) {
				using (var reader = new EliasGammaSignedReader(stream)) {
					return reader.Read();
				}
			}
		}

        /// <summary>
        ///     Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(Boolean disposing) {
			if (IsDisposed) {
				return;
			}

			IsDisposed = true;

			Underlying.Dispose();

			if (disposing) {
				// Dispose managed state (managed objects)
				Underlying?.Dispose();
			}
		}
	}
}