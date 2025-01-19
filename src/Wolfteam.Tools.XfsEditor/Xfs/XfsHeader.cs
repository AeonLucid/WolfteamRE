namespace Wolfteam.Tools.XfsEditor.Xfs
{
    public class XfsHeader
    {
        public byte[] Junk { get; set; }

        public byte Version { get; set; }

        public uint Dummy { get; set; }

        public uint Entries { get; set; }

        public uint Zero { get; set; }

        public uint XfsOffset { get; set; }
    }
}
