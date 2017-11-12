// Guids.cs
// MUST match guids.h
using System;

namespace chun.autopush
{
    static class GuidList
    {
        public const string guidautopushPkgString = "393377ab-6281-4012-a6c7-aaea1569388b";
        public const string guidautopushCmdSetString = "c1efb940-afc5-492e-93ec-2a67973b216e";

        public static readonly Guid guidautopushCmdSet = new Guid(guidautopushCmdSetString);
    };
}