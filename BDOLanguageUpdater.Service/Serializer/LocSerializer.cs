using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDOLanguageUpdater.Service.Serializer;

public static class LocSerializer
{
    private const string Utf16LeBom = "\ufeff";

    private static Task<string> TrimStart(string str, string chars)
    {
        if (str.StartsWith(chars))
        {
            str = str.Substring(chars.Length);
        }

        return Task.FromResult(str);
    }

    private static string AddSingleQuote(string str)
    {
        if (new[] { '+', '=', '-' }.All(ch => !str.StartsWith(ch)))
        {
            str = $"'{str}";
        }

        return str;
    }

    private static async Task<byte[]> FileToBuffer(string filePath)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var buffer = new byte[stream.Length];
        await stream.ReadAsync(buffer, 0, (int)stream.Length);
        return buffer;
    }

    private static async Task<byte[]> ZlibDecompress(string filePath)
    {
        var buffer = await FileToBuffer(filePath);
        using var memoryStream = new MemoryStream(buffer, 6, buffer.Length - 6);
        await using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        await deflateStream.CopyToAsync(resultStream);
        return resultStream.ToArray();
    }

    private static byte[] ZlibCompress(byte[] buffer)
    {
        using var memoryStream = new MemoryStream();
        using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true);
        deflateStream.Write(buffer, 0, buffer.Length);
        return memoryStream.ToArray();
    }

    private static byte[] Encrypt(string filePath)
    {
        var lines = new List<string>();
        var utf16LeBomBytes = Encoding.Unicode.GetBytes(Utf16LeBom);

        foreach (var line in File.ReadLines(filePath, Encoding.Unicode))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split('\t');
            var content = new List<object>();

            for (var i = 0; i < parts.Length; i++)
            {
                if (i > 4)
                {
                    content.Add(
                        TrimStart(parts[i].Replace("<lf>", "\n").Replace("\"", "").Replace("<quot>", "\""), "'"));
                }
                else
                {
                    content.Add(int.Parse(parts[i]));
                }
            }

            var strSize = Encoding.Unicode.GetByteCount((string)content[5]);
            var size = 4 + 4 + 4 + 2 + 1 + 1 + (strSize * 2) + 4;
            var buffer = new byte[size];
            var index = 0;

            BitConverter.GetBytes(strSize).CopyTo(buffer, index);
            index += 4;
            BitConverter.GetBytes((int)content[0]).CopyTo(buffer, index);
            index += 4;
            BitConverter.GetBytes((int)content[1]).CopyTo(buffer, index);
            index += 4;
            BitConverter.GetBytes((short)content[2]).CopyTo(buffer, index);
            index += 2;
            buffer[index] = (byte)(int)content[3];
            index += 1;
            buffer[index] = (byte)(int)content[4];
            index += 1;
            Encoding.Unicode.GetBytes((string)content[5]).CopyTo(buffer, index);
            index += strSize * 2;
            BitConverter.GetBytes(strSize).CopyTo(buffer, index);

            lines.Add(Encoding.Unicode.GetString(buffer));
        }

        var result = string.Join("\n", lines);
        return Encoding.Unicode.GetBytes(Utf16LeBom + result);
    }

    private static string Decrypt(byte[] buffer)
    {
        var result = new List<string>();
        var index = 0;

        while (index < buffer.Length)
        {
            var strSize = BitConverter.ToUInt32(buffer, index);
            index += 4;
            var strType = BitConverter.ToUInt32(buffer, index);
            index += 4;
            var strID1 = BitConverter.ToUInt32(buffer, index);
            index += 4;
            var strID2 = BitConverter.ToUInt16(buffer, index);
            index += 2;
            var strID3 = buffer[index];
            index += 1;
            var strID4 = buffer[index];
            index += 1;
            var strBytes = new byte[strSize * 2];
            Array.Copy(buffer, index, strBytes, 0, strSize * 2);
            index += (int)strSize * 2;
            var str = AddSingleQuote(Encoding.Unicode.GetString(strBytes).Replace("\n", "<lf>")
                .Replace("\"", "<quot>"));
            index += 4;

            result.Add($"{strType}\t{strID1}\t{strID2}\t{strID3}\t{strID4}\t{str}");
        }

        return Utf16LeBom + string.Join("\n", result);
    }

    public static async Task<string> Decompress(string source = "./languagedata_en.loc")
    {
        try
        {
            var dataDecompress = await ZlibDecompress(source);
            var dataDecrypt = Decrypt(dataDecompress);
            return dataDecrypt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static async Task<byte[]> Compress(string source = "./languagedata_en.tsv")
    {
        try
        {
            var dataEncrypt = Encrypt(source);
            var dataCompress = ZlibCompress(dataEncrypt);
            return dataCompress;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}