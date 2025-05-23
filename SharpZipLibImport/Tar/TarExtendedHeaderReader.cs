﻿using System.Text;

namespace SharpZipLib.Tar
{
    /// <summary>
    /// Reads the extended header of a Tar stream
    /// </summary>
    public class TarExtendedHeaderReader
    {
        private const byte LENGTH = 0;
        private const byte KEY = 1;
        private const byte VALUE = 2;
        private const byte END = 3;

        private readonly Dictionary<string, string> headers = [];

        private string[] headerParts = new string[3];

        private int bbIndex;
        private byte[] byteBuffer;
        private char[] charBuffer;

        private readonly StringBuilder sb = new();
        private readonly Decoder decoder = Encoding.UTF8.GetDecoder();

        private int state = LENGTH;

        private int currHeaderLength;
        private int currHeaderRead;

        private static readonly byte[] StateNext = [(byte)' ', (byte)'=', (byte)'\n'];

        /// <summary>
        /// Creates a new <see cref="TarExtendedHeaderReader"/>.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public TarExtendedHeaderReader()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            ResetBuffers();
        }

        /// <summary>
        /// Read <paramref name="length"/> bytes from <paramref name="buffer"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        public void Read(byte[] buffer, int length)
        {
            for (int i = 0; i < length; i++)
            {
                byte next = buffer[i];

                var foundStateEnd = state == VALUE
                    ? currHeaderRead == currHeaderLength - 1
                    : next == StateNext[state];

                if (foundStateEnd)
                {
                    Flush();
                    headerParts[state] = sb.ToString();
                    sb.Clear();

                    if (++state == END)
                    {
                        if (!headers.ContainsKey(headerParts[KEY]))
                        {
                            headers.Add(headerParts[KEY], headerParts[VALUE]);
                        }

                        headerParts = new string[3];
                        currHeaderLength = 0;
                        currHeaderRead = 0;
                        state = LENGTH;
                    }
                    else
                    {
                        currHeaderRead++;
                    }


                    if (state != VALUE) continue;

                    if (int.TryParse(headerParts[LENGTH], out var vl))
                    {
                        currHeaderLength = vl;
                    }
                }
                else
                {
                    byteBuffer[bbIndex++] = next;
                    currHeaderRead++;
                    if (bbIndex == 4)
                        Flush();
                }
            }
        }

        private void Flush()
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            decoder.Convert(byteBuffer, 0, bbIndex, charBuffer, 0, 4, false, out int bytesUsed, out int charsUsed, out bool completed);
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            sb.Append(charBuffer, 0, charsUsed);
            ResetBuffers();
        }

        private void ResetBuffers()
        {
            charBuffer = new char[4];
            byteBuffer = new byte[4];
            bbIndex = 0;
        }

        /// <summary>
        /// Returns the parsed headers as key-value strings
        /// </summary>
        public Dictionary<string, string> Headers
        {
            get
            {
                // TODO: Check for invalid state? -NM 2018-07-01
                return headers;
            }
        }
    }
}
