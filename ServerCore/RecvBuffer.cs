namespace ServerCore;

public class RecvBuffer
{
    private ArraySegment<byte> _buffer;
    private int _readPos;
    private int _writePos;

    public RecvBuffer(int bufferSize)
    {
        _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
    }

    public int DataSize => _writePos - _readPos;
    public int FreeSize => _buffer.Count - _writePos;

    public ArraySegment<byte> ReadSegment => new(_buffer.Array, _buffer.Offset + _readPos, DataSize);

    public ArraySegment<byte> WriteSegment => new(_buffer.Array, _buffer.Offset + _writePos, FreeSize);

    public void Clear()
    {
        int dataSize = DataSize;
        
        if (dataSize == 0)
            _readPos = _writePos = 0;
        
        else
        {
            Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
            _readPos = 0;
            _writePos = dataSize;
        }
    }

    public bool OnRead(int numOfByte)
    {
        if (numOfByte > DataSize)
            return false;
        
        _readPos += numOfByte;
        return true;
    }

    public bool OnWrite(int numOfByte)
    {
        if (numOfByte > FreeSize)
            return false;
        
        _writePos += numOfByte;
        return true;
    }
}