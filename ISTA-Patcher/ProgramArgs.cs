// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: Copyright 2022-2023 TautCony
namespace ISTA_Patcher;

using CommandLine;

public static class ProgramArgs
{
    internal enum PatchTypeEnum
    {
        BMW = 0,
        TOYOTA = 1,
    }

    [Verb("patch", HelpText = "Patch application and library.")]
    internal class PatchOptions
    {
        [Option('t', "type", Default = PatchTypeEnum.BMW, HelpText = "Patch type, valid option: BMW, TOYOTA")]
        public PatchTypeEnum PatchType { get; set; }

        [Option('d', "deobfuscate", Default = false, HelpText = "Deobfuscate application and library.")]
        public bool Deobfuscate { get; set; }

        [Option('f', "force", Default = false, HelpText = "Force patch application and library.")]
        public bool Force { get; set; }

        [Value(1, MetaName = "ISTA-P path", Required = true, HelpText = "Path for ISTA-P")]
        public string TargetPath { get; set; }
    }

    [Verb("license", HelpText = "License related operations.")]
    internal class LicenseOptions
    {
        [Option('g', "generate", HelpText = "Generate key pair", Group = "operation")]
        public bool GenerateKeyPair { get; set; }

        [Option('p', "patch", HelpText = "Patch target program", Group = "operation")]
        public string? TargetPath { get; set; }

        [Option('k', "key-pair", HelpText = "Path for key pair file")]
        public string? KeyPairPath { get; set; }

        [Option('l', "license", HelpText = "Path for license request file or base64 encoded content")]
        public string? LicensePath { get; set; }

        [Option('o', "output", HelpText = "Target path for signed license file")]
        public string? OutputPath { get; set; }

        [Option('b', "base64", HelpText = "Base64 encoded")]
        public bool Base64 { get; set; }

        [Option('f', "force", Default = false, HelpText = "Force patch application and library.")]
        public bool Force { get; set; }

        [Option('d', "deobfuscate", Default = false, HelpText = "Deobfuscate application and library.")]
        public bool Deobfuscate { get; set; }
    }

    [Verb("decrypt", HelpText = "Decrypt integrity checklist.")]
    internal class DecryptOptions
    {
        [Value(0, MetaName = "ISTA-P path", Required = true, HelpText = "Path for ISTA-P")]
        public string? TargetPath { get; set; }
    }
}