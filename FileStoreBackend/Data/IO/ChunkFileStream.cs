using FileStoreBackend.Data.Models;
using FileStoreBackend.Data.Providers;

namespace FileStoreBackend.Data.IO;
public class ChunkFileStream : Stream
{
    private readonly FileModel _file;
    private readonly PostgresProvider _dbProvider;
    private readonly long _length;
    private byte[] _currentChunk;
    private long _previousChunksRead;
    private long _position;
    private int _index;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _length;

    public override long Position { get => _position; set => _position = value; }

    public ChunkFileStream(FileModel file, PostgresProvider dbProvider)
    {
        _file = file;
        _dbProvider = dbProvider;
        _length = _dbProvider.GetOverallLength(file);
        _previousChunksRead = 0;
        _position = 0;
        _index = 0;
        _currentChunk = _dbProvider.GetChunk(_file, _index).Data;
    }

    public override void Flush()
    {
        _currentChunk = Array.Empty<byte>();
    }

    // count = 65536, chunkSize = 81920, position = 0
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (offset + count > buffer.Length)
        {
            throw new ArgumentOutOfRangeException();
        }

        int readBytes = 0;

        while (readBytes < count && _position < _length)
        {
            int bytesToRead = count - readBytes;
            int positionInCurrentChunk = (int)(_position - _previousChunksRead);
            if (bytesToRead < _currentChunk.Length - positionInCurrentChunk)
            {
                Array.Copy(_currentChunk, positionInCurrentChunk, buffer, offset + readBytes, bytesToRead);
                readBytes += bytesToRead;
                _position += bytesToRead;
            }
            else
            {
                Array.Copy(_currentChunk, positionInCurrentChunk, buffer, offset + readBytes, _currentChunk.Length - positionInCurrentChunk);
                readBytes += _currentChunk.Length - positionInCurrentChunk;
                _position += _currentChunk.Length - positionInCurrentChunk;
                _previousChunksRead += _currentChunk.Length;
                _currentChunk = _dbProvider.GetChunk(_file, ++_index)?.Data;

                if(_currentChunk is null)
                {
                    break;
                }
            }
        }

        return readBytes;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                _position = offset;
                break;
            case SeekOrigin.Current:
                _position += offset;
                break;
            case SeekOrigin.End:
                _position -= offset;
                break;
        }
        return _position;
    }

    // We don't have to edit stream
    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    // We don't have to edit stream
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}
