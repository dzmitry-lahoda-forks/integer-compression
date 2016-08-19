﻿using System;
using System.Collections.Generic;
using System.IO;

namespace InvertedTomato.IntegerCompression {
    /// <summary>
    /// Reader for Variable-length Quantity (VLQ) unsigned numbers.
    /// </summary>
    public class VLQUnsignedReader : IUnsignedReader {
        /// <summary>
        /// Read all values in a byte array.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<ulong> ReadAll(byte[] input) {
            return ReadAll(0, input);
        }

        /// <summary>
        /// Read all values in a byte array with options.
        /// </summary>
        /// <param name="expectedMinValue">The expected minimum value to optimize encoded values for. To match standard use 0.</param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<ulong> ReadAll(ulong expectedMinValue, byte[] input) {
            if (null == input) {
                throw new ArgumentNullException("input");
            }

            using (var stream = new MemoryStream(input)) {
                using (var reader = new VLQUnsignedReader(stream, expectedMinValue)) {
                    ulong value;
                    while (reader.TryRead(out value)) {
                        yield return value;
                    }
                }
            }
        }

        /// <summary>
        /// Mask to extract the data from a byte.
        /// </summary>
        const int PAYLOAD_MASK = 0x7F; // 0111 1111  - this is an int32 to save later casting

        /// <summary>
        /// Mask to extract the 'continuity' bit from a byte.
        /// </summary>
        const int CONTINUITY_MASK = 0x80; // 1000 0000  - this is an int32 to save later casting

        /// <summary>
        /// If disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// The number of full 8-bit bytes at the start of each value. Derived from MinBytes.
        /// </summary>
        private readonly int PrefixBytes;

        /// <summary>
        /// The underlying stream to be reading from.
        /// </summary>
        private readonly Stream Input;

        /// <summary>
        /// The current byte being worked with.
        /// </summary>
        private int CurrentByte;

        /// <summary>
        /// Standard instantiation.
        /// </summary>
        /// <param name="input"></param>
        public VLQUnsignedReader(Stream input) : this(input, 0) { }

        /// <summary>
        /// Instantiate with options.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expectedMinValue">The expected minimum value to optimize encoded values for. To match standard use 0.</param>
        public VLQUnsignedReader(Stream input, ulong expectedMinValue) {
            if (null == input) {
                throw new ArgumentNullException("input");
            }
           

            // Store
            Input = input;

            // Calculate number of prefix bytes for max efficiency
            while(expectedMinValue > byte.MaxValue) {
                PrefixBytes++;
                expectedMinValue /= byte.MaxValue;
            }
        }

        /// <summary>
        /// Attempt to read the next value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>If a read was successful.</returns>
        public bool TryRead(out ulong value) {
            if (IsDisposed) {
                throw new ObjectDisposedException("this");
            }

            // Setup offset
            var outputPosition = 0;

            // Set value to 0
            value = 0;
            
            // Read any full bytes per min-bytes requirements
            for (var i = 0; i < PrefixBytes; i++) {
                // Read next byte
                if (!ReadByte()) {
                    if (i > 0) {
                        throw new InvalidOperationException("Missing initial byte (" + i + ").");
                    }
                    return false;
                }

                // Add bits to value
                value += (ulong)CurrentByte << outputPosition;
                outputPosition += 8;
            }

            // Read next byte
            if (!ReadByte()) {
                if (PrefixBytes > 0) {
                    throw new InvalidOperationException("Missing initial body byte.");
                }
                return false;
            }

            // Add bits to value
            value += (ulong)((CurrentByte & PAYLOAD_MASK)) << outputPosition;

            while ((CurrentByte & CONTINUITY_MASK) == 0) {
                // Update target offset
                outputPosition += 7;

                // Read next byte
                if (!ReadByte()) {
                    throw new InvalidOperationException("Missing body byte.");
                }

                // Add bits to value
                value += (ulong)((CurrentByte & PAYLOAD_MASK) + 1) << outputPosition;
            }

            return true;
        }

        /// <summary>
        /// Read the next value. 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">No value was available.</exception>
        public ulong Read() {
            ulong value;
            if (!TryRead(out value)) {
                throw new EndOfStreamException();
            }
            return value;
        }

        /// <summary>
        /// Read a byte from the input stream.
        /// </summary>
        /// <returns>TRUE if successful.</returns>
        private bool ReadByte() {
            // Get next byte
            CurrentByte = Input.ReadByte();
            if (CurrentByte < 0) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Dispose.
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
        /// Dispose.
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }
    }
}
