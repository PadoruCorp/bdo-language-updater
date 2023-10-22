using System;

namespace BDOLanguageUpdater.Service.Serializer;

public static class BitConverterHelper
{
    public static void WriteUInt32LE(byte[] buffer, int offset, uint value)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (offset < 0 || offset + 4 > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        buffer[offset] = (byte)(value);
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
        buffer[offset + 3] = (byte)(value >> 24);
    }
    
    public static void WriteUInt16LE(byte[] buffer, int offset, ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        Buffer.BlockCopy(bytes, 0, buffer, offset, sizeof(ushort));
    }
}