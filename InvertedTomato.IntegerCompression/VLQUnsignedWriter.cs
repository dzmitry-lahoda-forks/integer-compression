﻿using InvertedTomato.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace InvertedTomato.IntegerCompression {
    /// <summary>
    /// Writer for Variable-length Quantity (VLQ) unsigned numbers.
    /// </summary>
    public class VLQUnsignedWriter : IUnsignedWriter {
        /// <summary>
        /// Write all given values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static byte[] WriteAll(IEnumerable<ulong> values) {
            using (var stream = new MemoryStream()) {
                using (var writer = new VLQUnsignedWriter(stream)) {
                    foreach (var value in values) {
                        writer.Write(value);
                    }
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Calculate the length of an encoded value in bits.
        /// </summary>
        /// <param name="packetSize">The number of bits to include in each packet.</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int CalculateBitLength(int packetSize, ulong value) {
            var packets = (int)Math.Ceiling((float)Bits.CountUsed(value) / (float)packetSize);

            return packets * (packetSize + 1);
        }

        /// <summary>
        /// If disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }


        private readonly byte PacketSize;

        /// <summary>
        /// The stream to output encoded bytes to.
        /// </summary>
        private readonly BitWriter Output;

        /// <summary>
        /// Standard instantiation.
        /// </summary>
        /// <param name="output"></param>
        public VLQUnsignedWriter(Stream output) : this(output, 7) { }

        /// <summary>
        /// Instantiate with options
        /// </summary>
        /// <param name="output"></param>
        /// <param name="packetSize">The number of bits to include in each packet.</param>
        public VLQUnsignedWriter(Stream output, int packetSize) {
            if (null == output) {
                throw new ArgumentNullException("output");
            }

            // Store
            Output = new BitWriter(output);
            PacketSize = (byte)packetSize;
        }

        /// <summary>
        /// Append value to stream.
        /// </summary>
        /// <param name="value"></param>
        public void Write(ulong value) {
            if (IsDisposed) {
                throw new ObjectDisposedException("this");
            }

            // Calculate size of non-final packet
            var min = ulong.MaxValue >> 64 - PacketSize;

            // Iterate through input, taking X bits of data each time, aborting when less than X bits left
            while (value > min) {
                // Write continuity header - more packets following
                Output.Write(0, 1);

                // Write payload
                Output.Write(value, PacketSize);

                // Offset value for next cycle
                value >>= PacketSize;
                value--;
            }

            // Write continuity header - no packets following
            Output.Write(1, 1);

            // Write final payload
            Output.Write(value, PacketSize);
        }

        /// <summary>
        /// Flush any unwritten bits and dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (IsDisposed) {
                return;
            }
            IsDisposed = true;

            if (disposing) {
                // Dispose managed state (managed objects).
            }
        }

        /// <summary>
        /// Flush any unwritten bits and dispose.
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose() {
            Dispose(true);
        }
    }
}
