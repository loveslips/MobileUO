using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace ClassicUO.IO
{
    public unsafe class MMFileReader : FileReader
    {
        // MobileUO: removed accessor for stream
        private readonly MemoryMappedViewStream _stream;
        private readonly MemoryMappedFile _mmf;
        private readonly BinaryReader _file;

        // MINIMAL FIX: Add accessor and pointer
        private MemoryMappedViewAccessor _accessor;
        private byte* _startPtr;

        public MMFileReader(FileStream stream) : base(stream)
        {
            if (Length <= 0)
                return;

            _mmf = MemoryMappedFile.CreateFromFile
            (
                stream,
                null,
                0,
                MemoryMappedFileAccess.Read,
                HandleInheritability.None,
                false
            );

            // MobileUO: replaced unsafe call
            _stream = _mmf.CreateViewStream(0, Length, MemoryMappedFileAccess.Read);
            _file = new BinaryReader(_stream);

            // MINIMAL FIX: Acquire pointer
            _accessor = _mmf.CreateViewAccessor(0, Length, MemoryMappedFileAccess.Read);
            _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _startPtr);
        }

        public override BinaryReader Reader => _file;

        // MINIMAL FIX: Expose pointer (cast to ulong to avoid managed type issues)
        public ulong StartAddressAsUInt64 => (ulong)_startPtr;

        public override void Dispose()
        {
            // MobileUO: added dispose
            // MINIMAL FIX: Release pointer
            if (_startPtr != null)
            {
                _accessor?.SafeMemoryMappedViewHandle.ReleasePointer();
                _startPtr = null;
            }

            _accessor?.Dispose();
            _file?.Dispose();
            _stream?.Dispose();
            _mmf?.Dispose();

            base.Dispose();
        }
    }
}