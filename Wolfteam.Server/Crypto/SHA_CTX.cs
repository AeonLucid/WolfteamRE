using System.Runtime.InteropServices;

namespace Wolfteam.Server.Crypto;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal ref struct SHA_CTX
{
    public uint h0;
    public uint h1;
    public uint h2;
    public uint h3;
    public uint h4;
    public uint Nl;
    public uint Nh;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public Span<uint> data;

    public int num;
}