/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Collections.Generic;

namespace ProtonVPN.Vpn.WireGuard;

public class RingLogger
{
    private bool _started;

    private readonly struct UnixTimestamp
    {
        private readonly long _ns;

        public UnixTimestamp(long ns)
        {
            _ns = ns;
        }

        public bool IsEmpty => _ns == 0;

        public override string ToString()
        {
            return DateTimeOffset.FromUnixTimeSeconds(_ns / 1000000000).LocalDateTime
                .ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'") + (_ns % 1000000000 + "00000").Substring(0, 6);
        }
    }

    private readonly struct Line
    {
        private const int MAX_LINE_LENGTH = 512;
        private const int OFFSET_TIME_NS = 0;
        private const int OFFSET_LINE = 8;

        private readonly MemoryMappedViewAccessor _view;
        private readonly int _start;

        public Line(MemoryMappedViewAccessor view, uint index)
        {
            (_view, _start) = (view, (int)(Log.HeaderBytes + index * Bytes));
        }

        public static int Bytes => MAX_LINE_LENGTH + OFFSET_LINE;

        public UnixTimestamp Timestamp => new(_view.ReadInt64(_start + OFFSET_TIME_NS));

        private string? Text
        {
            get
            {
                byte[] textBytes = new byte[MAX_LINE_LENGTH];
                _view.ReadArray(_start + OFFSET_LINE, textBytes, 0, textBytes.Length);
                int nullByte = Array.IndexOf<byte>(textBytes, 0);
                if (nullByte <= 0)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(textBytes, 0, nullByte);
            }
        }

        public override string? ToString()
        {
            UnixTimestamp time = Timestamp;
            if (time.IsEmpty)
            {
                return null;
            }

            string? text = Text;
            if (text == null)
            {
                return null;
            }

            return $"{time}: {text}";
        }
    }

    private struct Log
    {
        private const uint MAX_LINES = 2048;
        private const int OFFSET_NEXT_INDEX = 4;
        private const int OFFSET_LINES = 8;

        private readonly MemoryMappedViewAccessor _view;

        public Log(MemoryMappedViewAccessor view)
        {
            _view = view;
        }

        public static int HeaderBytes => OFFSET_LINES;
        public static int Bytes => (int)(HeaderBytes + Line.Bytes * MAX_LINES);

        public uint NextIndex => _view.ReadUInt32(OFFSET_NEXT_INDEX);

        public uint LineCount => MAX_LINES;
        public Line this[uint i] => new(_view, i % MAX_LINES);
    }

    private Log _log;
    private MemoryMappedFile? _memoryMappedFile;
    private MemoryMappedViewAccessor? _memoryMappedViewAccessor;

    private readonly string _filename;

    public RingLogger(string filename)
    {
        _filename = filename;
    }

    public static readonly uint CursorAll = uint.MaxValue;

    public void Start()
    {
        DeleteFile();
        FileStream file = File.Open(_filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
        file.SetLength(Log.Bytes);
        
        _memoryMappedFile = MemoryMappedFile.CreateFromFile(file, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
        _memoryMappedViewAccessor = _memoryMappedFile.CreateViewAccessor(0, Log.Bytes, MemoryMappedFileAccess.ReadWrite);
        _log = new Log(_memoryMappedViewAccessor);
        _started = true;
    }

    public void Stop()
    {
        _started = false;
        _memoryMappedViewAccessor?.Dispose();
        _memoryMappedFile?.Dispose();
    }

    private void DeleteFile()
    {
        if (File.Exists(_filename))
        {
            try
            {
                File.Delete(_filename);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    public List<string> FollowFromCursor(ref uint cursor)
    {
        if (!_started)
        {
            return [];
        }

        List<string> lines = new((int)_log.LineCount);
        uint i = cursor;
        bool all = cursor == CursorAll;
        if (all)
        {
            i = _log.NextIndex;
        }

        for (uint l = 0; l < _log.LineCount; ++l, ++i)
        {
            if (!all && i % _log.LineCount == _log.NextIndex % _log.LineCount)
            {
                break;
            }

            Line entry = _log[i];
            if (entry.Timestamp.IsEmpty)
            {
                if (all)
                {
                    continue;
                }

                break;
            }

            cursor = (i + 1) % _log.LineCount;

            string? entryString = entry.ToString();
            if (!string.IsNullOrEmpty(entryString))
            {
                lines.Add(entryString);
            }
        }

        return lines;
    }
}