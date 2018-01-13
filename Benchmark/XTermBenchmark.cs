﻿// Copyright 2011-2017 The Poderosa Project.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

using Poderosa.Commands;
using Poderosa.Protocols;
using Poderosa.Sessions;
using Poderosa.Terminal;

namespace Poderosa.Benchmark {

    internal enum XTermBenchmarkPattern {
        /// <summary>
        /// Only ASCII letter and digits.
        /// </summary>
        ASCII,

        /// <summary>
        /// Only Japanese Kanji characters. (UTF-8)
        /// </summary>
        KANJI,

        /// <summary>
        /// ASCII and KANJI alternately
        /// </summary>
        ASCII_KANJI,

        /// <summary>
        /// Only ASCII letter and digits.
        /// 16 colors.
        /// </summary>
        ASCII_COLOR16,

        /// <summary>
        /// Only Japanese Kanji characters. (UTF-8)
        /// 16 colors.
        /// </summary>
        KANJI_COLOR16,

        /// <summary>
        /// ASCII and KANJI alternately.
        /// 16 colors.
        /// </summary>
        ASCII_KANJI_COLOR16,

        /// <summary>
        /// Only ASCII letter and digits.
        /// 256 colors.
        /// </summary>
        ASCII_COLOR256,

        /// <summary>
        /// Only Japanese Kanji characters. (UTF-8)
        /// 256 colors.
        /// </summary>
        KANJI_COLOR256,

        /// <summary>
        /// ASCII and KANJI alternately.
        /// 256 colors.
        /// </summary>
        ASCII_KANJI_COLOR256,

        /// <summary>
        /// Only ASCII letter and digits.
        /// 24 bit colors.
        /// </summary>
        ASCII_COLOR24,

        /// <summary>
        /// Only Japanese Kanji characters. (UTF-8)
        /// 24 bit colors.
        /// </summary>
        KANJI_COLOR24,

        /// <summary>
        /// ASCII and KANJI alternately.
        /// 24 bit colors.
        /// </summary>
        ASCII_KANJI_COLOR24,
    }

    /// <summary>
    /// XTerm Benchmark
    /// </summary>
    internal class XTermBenchmark : AbstractTerminalBenchmark {

        private readonly MockSocket _socket;
        private readonly MockTerminalConnection _connection;
        private readonly XTermBenchmarkPattern _pattern;
        private ITerminalEmulatorOptions _options;
        private ITerminalSession _session;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="target">target object</param>
        /// <param name="pattern">benchmark pattern</param>
        public XTermBenchmark(ICommandTarget target, XTermBenchmarkPattern pattern)
            : base(target) {
            _socket = new MockSocket();
            _connection = new MockTerminalConnection("xterm", _socket);
            _pattern = pattern;
        }

        /// <summary>
        /// Returns terminal's caption
        /// </summary>
        protected override string GetTerminalCaption() {
            return "XTerm Benchmark";
        }

        /// <summary>
        /// Returns ITerminalConnection instance from the derived class
        /// </summary>
        /// <returns>connection object</returns>
        protected override ITerminalConnection GetTerminalConnection() {
            return _connection;
        }

        /// <summary>
        /// Starts benchmark thread int the derived class
        /// </summary>
        protected override void StartBenchmarkThread(ITerminalEmulatorOptions options, ITerminalSession session) {
            _options = options;
            _session = session;
            Thread thread = new Thread(new ThreadStart(BenchmarkThread));
            thread.Name = "Poderosa.Benchmark.XTermBenchmark";
            thread.Start();
        }

        private const int TIMEOUT = 5000;
        private const string NEWLINE = "\r\n";

        /// <summary>
        /// Benchmark thread
        /// </summary>
        private void BenchmarkThread() {
            Thread.Sleep(2000);

            try {
                OnPaintTimeStatistics onPaintStats = new OnPaintTimeStatistics();

                long mem1 = GC.GetTotalMemory(true);

                _session.TerminalControl.SetOnPaintTimeObserver(
                    delegate(Stopwatch s) {
                        onPaintStats.Update(s);
                    }
                );

                SockWriteLine("Start XTerm Benchmark.");

                const int DATA_CHUNK_SIZE = 200;

                const int TRANSMIT_SECONDS = 30;

                DateTime utcLimit = DateTime.UtcNow.AddSeconds(TRANSMIT_SECONDS);

                Stopwatch swTotal;

                switch (_pattern) {
                    case XTermBenchmarkPattern.ASCII:
                        swTotal = BenchmarkAscii(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.KANJI:
                        swTotal = BenchmarkKanji(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.ASCII_KANJI:
                        swTotal = BenchmarkAsciiKanji(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.ASCII_COLOR16:
                        swTotal = BenchmarkAsciiColor16(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.KANJI_COLOR16:
                        swTotal = BenchmarkKanjiColor16(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.ASCII_KANJI_COLOR16:
                        swTotal = BenchmarkAsciiKanjiColor16(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.ASCII_COLOR256:
                        swTotal = BenchmarkAsciiColor256(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.KANJI_COLOR256:
                        swTotal = BenchmarkKanjiColor256(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.ASCII_KANJI_COLOR256:
                        swTotal = BenchmarkAsciiKanjiColor256(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.ASCII_COLOR24:
                        swTotal = BenchmarkAsciiColor24(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.KANJI_COLOR24:
                        swTotal = BenchmarkKanjiColor24(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    case XTermBenchmarkPattern.ASCII_KANJI_COLOR24:
                        swTotal = BenchmarkAsciiKanjiColor24(DATA_CHUNK_SIZE, utcLimit);
                        break;
                    default:
                        swTotal = Stopwatch.StartNew();
                        swTotal.Stop();
                        break;
                }

                SockWriteLine("\u001b[0m");
                SockWriteLine("End XTerm Benchmark.");

                _session.TerminalControl.SetOnPaintTimeObserver(null);

                long mem2 = GC.GetTotalMemory(true);

                TerminalDocument doc = (TerminalDocument)_session.Terminal.IDocument.GetAdapter(typeof(TerminalDocument));

                SockWriteLine("---------------------------------------");
                SockWriteLine(String.Format(NumberFormatInfo.InvariantInfo,
                            "Terminal Size : {0} x {1}", doc.TerminalWidth, doc.TerminalHeight));
                SockWriteLine(String.Format(NumberFormatInfo.InvariantInfo,
                            "Terminal Buffer Size : {0}", _options.TerminalBufferSize));
                SockWriteLine("---------------------------------------");
                SockWriteLine(String.Format("OnPaint {0} samples", onPaintStats.GetSampleCount()));
                SockWriteLine(String.Format("        Max  {0} msec", onPaintStats.GetMaxTimeMilliseconds()));
                SockWriteLine(String.Format("        Min  {0} msec", onPaintStats.GetMinTimeMilliseconds()));
                SockWriteLine(String.Format("        Avg  {0} msec", onPaintStats.GetAverageTimeMilliseconds()));
                SockWriteLine("---------------------------------------");
                ReportBenchmark("Total          ", swTotal);
                SockWriteLine("---------------------------------------");

                SockWriteLine(String.Format("Increase of Managed Memory : {0} bytes", mem2 - mem1));
                SockWriteLine("---------------------------------------");
            }
            catch (MockSocketTimeoutException) {
            }
        }

        private void SockWriteLine() {
            _socket.FeedData(new byte[][] { Encoding.UTF8.GetBytes(NEWLINE) }, TIMEOUT);
        }

        private void SockWriteLine(string text) {
            _socket.FeedData(new byte[][] { Encoding.UTF8.GetBytes(text + NEWLINE) }, TIMEOUT);
        }

        private void ReportBenchmark(string title, Stopwatch w) {
            SockWriteLine(String.Format(NumberFormatInfo.InvariantInfo,
                "{0} : {1}.{2:D3} sec", title, w.ElapsedMilliseconds / 1000, w.ElapsedMilliseconds % 1000));
        }




        private static readonly string[] COLOR16 = {
            "\u001b[103m\u001b[30m",
            "\u001b[104m\u001b[31m",
            "\u001b[105m\u001b[32m",
            "\u001b[106m\u001b[33m",
            "\u001b[107m\u001b[34m",
            "\u001b[100m\u001b[35m",
            "\u001b[101m\u001b[36m",
            "\u001b[102m\u001b[37m",
            "\u001b[43m\u001b[90m",
            "\u001b[44m\u001b[91m",
            "\u001b[45m\u001b[92m",
            "\u001b[46m\u001b[93m",
            "\u001b[47m\u001b[94m",
            "\u001b[40m\u001b[95m",
            "\u001b[41m\u001b[96m",
            "\u001b[42m\u001b[97m",
        };

        private static string GetAsciiPattern() {
            StringBuilder pattern = new StringBuilder();
            for (int i = 0x21; i <= 0x7e; i++) {
                pattern.Append((char)i);
            }
            return pattern.ToString();
        }

        private static string GetKanjiPattern() {
            return "\u5143\u5144\u5145\u5146\u5148\u5149\u514b\u514d\u5165\u5168"
                 + "\u516b\u516c\u516d\u5171\u5175\u5176\u5177\u5178\u517c\u5180"
                 + "\u518d\u5192\u5195\u51a0\u51ac\u51b7\u51bd\u51c4\u51c6\u51c9"
                 + "\u51cb\u51cc\u51dd\u51e1\u51f6\u51f8\u51f9\u51fa\u51fd\u5200"
                 + "\u5203\u5206\u5207\u5208\u520a\u520e\u5211\u5217\u521d\u5224";
        }

        private static string GetAsciiKanjiPattern() {
            StringBuilder pattern = new StringBuilder();
            string kanjiPattern = GetKanjiPattern();
            string asciiPattern = GetAsciiPattern();
            for (int i = 0; i < kanjiPattern.Length; i++) {
                pattern.Append(kanjiPattern[i]).Append(asciiPattern[i % asciiPattern.Length]);
            }
            return pattern.ToString();
        }

        private byte[] GetPalette256() {
            StringBuilder palette = new StringBuilder();
            string[] val = { "00", "33", "66", "99", "cc", "ff" };
            for (int r = 0; r < 6; r++) {
                for (int g = 0; g < 6; g++) {
                    for (int b = 0; b < 6; b++) {
                        palette.Append("\u001b]4;")
                               .Append(16 + r * 36 + g * 6 + b)
                               .Append(";rgb:")
                               .Append(val[r]).Append('/').Append(val[g]).Append('/').Append(val[b])
                               .Append("\u001b\\");
                    }
                }
            }
            for (int i = 0; i < 24; i++) {
                string level = String.Format("{0:X2}", 8 + i * 10);
                palette.Append("\u001b]4;")
                       .Append(232 + i)
                       .Append(";rgb:")
                       .Append(level).Append('/').Append(level).Append('/').Append(level)
                       .Append("\u001b\\");
            }

            return Encoding.UTF8.GetBytes(palette.ToString());
        }

        private void FillBuffer(byte[] buff, byte[] data, ref int offset) {
            int dataSize = data.Length;
            int dataOffset = offset;
            int buffSize = buff.Length;
            int buffOffset = 0;

            while (buffOffset < buffSize) {
                int copySize = Math.Min(buffSize - buffOffset, dataSize - dataOffset);
                Buffer.BlockCopy(data, dataOffset, buff, buffOffset, copySize);
                buffOffset += copySize;
                dataOffset += copySize;
                if (dataOffset >= dataSize)
                    dataOffset = 0;
            }
            offset = dataOffset;
        }

        private byte[] GetBlock(byte[] data, int offset) {
            int size = data.Length - offset;
            byte[] buff = new byte[size];
            Buffer.BlockCopy(data, offset, buff, 0, size);
            return buff;
        }

        private IEnumerable<byte[]> BenchmarkDataGenerator(int chunk, DateTime utcLimit, byte[] source) {
            byte[] buff = new byte[chunk];

            int offset = 0;
            while (DateTime.UtcNow < utcLimit) {
                FillBuffer(buff, source, ref offset);
                yield return buff;
            }

            if (offset > 0) {
                yield return GetBlock(source, offset);
            }
        }

        private Stopwatch BenchmarkAscii(int chunk, DateTime utcLimit) {
            byte[] source = Encoding.UTF8.GetBytes(GetAsciiPattern());
            Stopwatch sw = Stopwatch.StartNew();
            _socket.FeedData(BenchmarkDataGenerator(chunk, utcLimit, source), TIMEOUT);
            sw.Stop();
            return sw;
        }

        private Stopwatch BenchmarkKanji(int chunk, DateTime utcLimit) {
            byte[] source = Encoding.UTF8.GetBytes(GetKanjiPattern());
            Stopwatch sw = Stopwatch.StartNew();
            _socket.FeedData(BenchmarkDataGenerator(chunk, utcLimit, source), TIMEOUT);
            sw.Stop();
            return sw;
        }

        private Stopwatch BenchmarkAsciiKanji(int chunk, DateTime utcLimit) {
            byte[] source = Encoding.UTF8.GetBytes(GetAsciiKanjiPattern());
            Stopwatch sw = Stopwatch.StartNew();
            _socket.FeedData(BenchmarkDataGenerator(chunk, utcLimit, source), TIMEOUT);
            sw.Stop();
            return sw;
        }

        private Stopwatch BenchmarkAsciiColor16(int chunk, DateTime utcLimit) {
            string asciiPattern = GetAsciiPattern();
            StringBuilder pattern = new StringBuilder();
            for (int i = 0; i < asciiPattern.Length; i++) {
                pattern.Append(COLOR16[i % COLOR16.Length]).Append(asciiPattern[i]);
            }
            byte[] source = Encoding.UTF8.GetBytes(pattern.ToString());

            Stopwatch sw = Stopwatch.StartNew();
            _socket.FeedData(BenchmarkDataGenerator(chunk, utcLimit, source), TIMEOUT);
            sw.Stop();
            return sw;
        }

        private Stopwatch BenchmarkKanjiColor16(int chunk, DateTime utcLimit) {
            string kanjiPattern = GetKanjiPattern();
            StringBuilder pattern = new StringBuilder();
            for (int i = 0; i < kanjiPattern.Length; i++) {
                pattern.Append(COLOR16[i % COLOR16.Length]).Append(kanjiPattern[i]);
            }
            byte[] source = Encoding.UTF8.GetBytes(pattern.ToString());

            Stopwatch sw = Stopwatch.StartNew();
            _socket.FeedData(BenchmarkDataGenerator(chunk, utcLimit, source), TIMEOUT);
            sw.Stop();
            return sw;
        }

        private Stopwatch BenchmarkAsciiKanjiColor16(int chunk, DateTime utcLimit) {
            string asciiKanjiPattern = GetAsciiKanjiPattern();
            StringBuilder pattern = new StringBuilder();
            for (int i = 0; i < asciiKanjiPattern.Length; i++) {
                pattern.Append(COLOR16[i % COLOR16.Length]).Append(asciiKanjiPattern[i]);
            }
            byte[] source = Encoding.UTF8.GetBytes(pattern.ToString());

            Stopwatch sw = Stopwatch.StartNew();
            _socket.FeedData(BenchmarkDataGenerator(chunk, utcLimit, source), TIMEOUT);
            sw.Stop();
            return sw;
        }

        private Stopwatch BenchmarkColor256(int chunk, DateTime utcLimit, string charPattern) {
            byte[] palette = GetPalette256();

            StringBuilder pattern = new StringBuilder();
            for (int i = 16; i < 256; i++) {
                pattern.Append("\u001b[48;5;").Append(i).Append('m')
                       .Append("\u001b[38;5;").Append(255 + 16 - i).Append('m')
                       .Append(charPattern[i % charPattern.Length]);
            }
            byte[] source = Encoding.UTF8.GetBytes(pattern.ToString());

            Stopwatch sw = Stopwatch.StartNew();
            _socket.FeedData(new byte[][] { palette }, TIMEOUT);
            _socket.FeedData(BenchmarkDataGenerator(chunk, utcLimit, source), TIMEOUT);
            sw.Stop();
            return sw;
        }

        private Stopwatch BenchmarkAsciiColor256(int chunk, DateTime utcLimit) {
            string asciiPattern = GetAsciiPattern();
            return BenchmarkColor256(chunk, utcLimit, asciiPattern);
        }

        private Stopwatch BenchmarkKanjiColor256(int chunk, DateTime utcLimit) {
            string kanjiPattern = GetKanjiPattern();
            return BenchmarkColor256(chunk, utcLimit, kanjiPattern);
        }

        private Stopwatch BenchmarkAsciiKanjiColor256(int chunk, DateTime utcLimit) {
            string asciiKanjiPattern = GetAsciiKanjiPattern();
            return BenchmarkColor256(chunk, utcLimit, asciiKanjiPattern);
        }

        private Stopwatch BenchmarkColor24(int chunk, DateTime utcLimit, string charPattern) {
            StringBuilder pattern = new StringBuilder();
            int letterIndex = 0;
            for (int i = 0; i <= 256; i += 16) {
                int r = Math.Min(i, 255);
                int g = 0;
                int b = 0;
                pattern.Append("\u001b[48;2;").Append(r).Append(';').Append(g).Append(';').Append(b).Append('m')
                       .Append("\u001b[97m") // white
                       .Append(charPattern[letterIndex++ % charPattern.Length]);
            }
            for (int i = 0; i <= 256; i += 16) {
                int r = 0;
                int g = Math.Min(i, 255);
                int b = 0;
                pattern.Append("\u001b[48;2;").Append(r).Append(';').Append(g).Append(';').Append(b).Append('m')
                       .Append("\u001b[95m") // Magenta
                       .Append(charPattern[letterIndex++ % charPattern.Length]);
            }
            for (int i = 0; i <= 256; i += 16) {
                int r = 0;
                int g = 0;
                int b = Math.Min(i, 255);
                pattern.Append("\u001b[48;2;").Append(r).Append(';').Append(g).Append(';').Append(b).Append('m')
                       .Append("\u001b[93m") // Yellow
                       .Append(charPattern[letterIndex++ % charPattern.Length]);
            }
            for (int i = 0; i <= 256; i += 16) {
                int r = Math.Min(i, 255);
                int g = 0;
                int b = 0;
                pattern.Append("\u001b[107m") // white
                       .Append("\u001b[38;2;").Append(r).Append(';').Append(g).Append(';').Append(b).Append('m')
                       .Append(charPattern[letterIndex++ % charPattern.Length]);
            }
            for (int i = 0; i <= 256; i += 16) {
                int r = 0;
                int g = Math.Min(i, 255);
                int b = 0;
                pattern.Append("\u001b[105m") // Magenta
                       .Append("\u001b[38;2;").Append(r).Append(';').Append(g).Append(';').Append(b).Append('m')
                       .Append(charPattern[letterIndex++ % charPattern.Length]);
            }
            for (int i = 0; i <= 256; i += 16) {
                int r = 0;
                int g = 0;
                int b = Math.Min(i, 255);
                pattern.Append("\u001b[103m") // Yellow
                       .Append("\u001b[38;2;").Append(r).Append(';').Append(g).Append(';').Append(b).Append('m')
                       .Append(charPattern[letterIndex++ % charPattern.Length]);
            }
            byte[] source = Encoding.UTF8.GetBytes(pattern.ToString());

            Stopwatch sw = Stopwatch.StartNew();
            _socket.FeedData(BenchmarkDataGenerator(chunk, utcLimit, source), TIMEOUT);
            sw.Stop();
            return sw;
        }

        private Stopwatch BenchmarkAsciiColor24(int chunk, DateTime utcLimit) {
            string asciiPattern = GetAsciiPattern();
            return BenchmarkColor24(chunk, utcLimit, asciiPattern);
        }

        private Stopwatch BenchmarkKanjiColor24(int chunk, DateTime utcLimit) {
            string kanjiPattern = GetKanjiPattern();
            return BenchmarkColor24(chunk, utcLimit, kanjiPattern);
        }

        private Stopwatch BenchmarkAsciiKanjiColor24(int chunk, DateTime utcLimit) {
            string asciiKanjiPattern = GetAsciiKanjiPattern();
            return BenchmarkColor24(chunk, utcLimit, asciiKanjiPattern);
        }

    }
}
