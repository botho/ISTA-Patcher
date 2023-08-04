// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2022-2023 TautCony

// ReSharper disable InconsistentNaming, StringLiteralTypo, CommentTypo
namespace ISTA_Patcher;

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Serilog;

public static class IntegrityManager
{
    private static readonly byte[] _salt = { 0xd, 0xca, 0x32, 0xe0, 0x7f, 0xa4, 0xdf, 0xf1 };

    private const int _iterations = 1100;

    private static readonly byte[] _password =
    {
        0x33, 0x2f, 0x33, 0x48, 0x65, 0x78, 0x62, 0x4b, 0x4b, 0x46, 0x73, 0x34, 0x4c, 0x71, 0x70, 0x69,
        0x43, 0x53, 0x67, 0x4b, 0x41, 0x58, 0x47, 0x55, 0x59, 0x43, 0x74, 0x71, 0x6a, 0x6f, 0x46, 0x63,
        0x68, 0x66, 0x50, 0x69, 0x74, 0x41, 0x6d, 0x49, 0x38, 0x77, 0x45, 0x3d,
    };

    public static List<HashFileInfo>? DecryptFile(string sourceFilename)
    {
        try
        {
            var aesManaged = Aes.Create();
            aesManaged.BlockSize = aesManaged.LegalBlockSizes[0].MaxSize;
            aesManaged.KeySize = aesManaged.LegalKeySizes[0].MaxSize;
            var rfc2898DeriveBytes = new Rfc2898DeriveBytes(_password, _salt, _iterations);
            aesManaged.Key = rfc2898DeriveBytes.GetBytes(aesManaged.KeySize / 8);
            aesManaged.IV = rfc2898DeriveBytes.GetBytes(aesManaged.BlockSize / 8);
            aesManaged.Mode = CipherMode.CBC;
            var transform = aesManaged.CreateDecryptor(aesManaged.Key, aesManaged.IV);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            using (var fileStream = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.CopyTo(cryptoStream);
            }

            var bytes = memoryStream.ToArray();
            return (from row in Encoding.UTF8.GetString(bytes).Split(";;\r\n", StringSplitOptions.RemoveEmptyEntries).Distinct()
                select new HashFileInfo(row.Split(";;", StringSplitOptions.RemoveEmptyEntries))).ToList();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to decrypt file: {Reason}", ex.Message);
        }

        return null;
    }
}

public class HashFileInfo
{
    public string FileName { get; }

    public string FilePath { get; }

    public string Hash { get; }

    protected internal HashFileInfo(IReadOnlyList<string> fileInfos)
    {
        this.FilePath = fileInfos[0].Trim('\uFEFF').Replace("\\", "/");
        this.FileName = Path.GetFileName(this.FilePath ?? string.Empty).Trim('\uFEFF');
        try
        {
            var bytes = Convert.FromBase64String(fileInfos[1]);
            var hex = BitConverter.ToString(bytes).Replace("-", string.Empty);
            this.Hash = hex;
        }
        catch (FormatException ex)
        {
            this.Hash = string.Empty;
            Log.Warning(ex, "Failed to parse hash value [{Hash}] for: {FileName}", fileInfos[1], this.FileName);
        }
    }

    public static string CalculateHash(string pathFile)
    {
        try
        {
            using var sha = SHA256.Create();
            using var fileStream = File.OpenRead(pathFile);
            var text = BitConverter.ToString(sha.ComputeHash(fileStream)).Replace("-", string.Empty);
            return text;
        }
        catch (FileNotFoundException ex)
        {
            Log.Warning(ex, "Failed to calculate hash for: {FileName}", pathFile);
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets a value that indicates whether the assembly manifest at the supplied path contains a strong name signature.
    /// </summary>
    /// <param name="wszFilePath">[in] The path to the portable executable (.exe or .dll) file for the assembly to be verified.</param>
    /// <param name="fForceVerification">[in] true to perform verification, even if it is necessary to override registry settings; otherwise, false.</param>
    /// <param name="pfWasVerified">[out] true if the strong name signature was verified; otherwise, false. pfWasVerified is also set to false if the verification was successful due to registry settings.</param>
    /// <returns>S_OK if the verification was successful; otherwise, an HRESULT value that indicates failure.</returns>
    [DllImport("mscoree.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "StrongNameSignatureVerificationEx")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool StrongNameSignatureVerificationEx(
        [MarshalAs(UnmanagedType.LPWStr)]string wszFilePath,
        [MarshalAs(UnmanagedType.U1)]bool fForceVerification,
        [MarshalAs(UnmanagedType.U1)]ref bool pfWasVerified
    );
}
