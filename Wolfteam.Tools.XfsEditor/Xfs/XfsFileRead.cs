using System.Collections.Generic;
using System.Linq;

namespace Wolfteam.Tools.XfsEditor.Xfs
{
    public class XfsFileRead
    {
        public XfsFileRead(string name, uint offset, uint size, uint sizeCompressed, bool isFile)
        {
            Name = name;
            Offset = offset;
            Size = size;
            SizeCompressed = sizeCompressed;
            IsFile = isFile;
            Children = new List<XfsFileRead>();
        }

        public string Name { get; }

        public uint Offset { get; }

        public uint Size { get; }

        public uint SizeCompressed { get; }

        public bool IsFile { get; }

        // TreeListView

        public string FileType => IsFile ? "File" : "Dir";

        public XfsFileRead Parent { get; set; }

        public int ChildrenCount => !IsFile ? Children.Sum(x => x.ChildrenCount) : 1;

        public List<XfsFileRead> Children { get; set; }
    }
}
