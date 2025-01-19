using System.Windows.Forms;
using Wolfteam.Tools.XfsEditor.Xfs;

namespace Wolfteam.Tools.XfsEditor.Forms
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
            
            fileListView.CanExpandGetter = model => !((XfsFileRead) model).IsFile;
            fileListView.ChildrenGetter = model => ((XfsFileRead) model).Children;

            olvColumn4.TextAlign = HorizontalAlignment.Right;
            olvColumn4.AspectToStringConverter = rowObject =>
            {
                var size = (uint) rowObject;
                if (size == 0)
                {
                    return string.Empty;
                }

                var limits = new[] {1024 * 1024 * 1024, 1024 * 1024, 1024};
                var units = new[] {"GB", "MB", "KB"};

                for (var i = 0; i < limits.Length; i++)
                {
                    if (size >= limits[i])
                    {
                        return string.Format("{0:#,##0.##} " + units[i], (double) size / limits[i]);
                    }
                }
                return $"{size} bytes";
            };
        }

        private XfsFileReader CurrentFileReader { get; set; }

        private void ApplyDefaultSort()
        {
            fileListView.PrimarySortOrder = SortOrder.Descending;
            fileListView.PrimarySortColumn = olvColumn3;

            fileListView.SecondarySortOrder = SortOrder.Ascending;
            fileListView.SecondarySortColumn = olvColumn1;

            fileListView.Roots = CurrentFileReader.Files;
            fileListView.Sort();
        }

        private void FrmMain_Load(object sender, System.EventArgs e)
        {
//            CurrentFileReader = new XfsFileReader("C:\\AeriaGames\\WolfTeam-US\\wolf.xfs");
//            CurrentFileReader.Run();
//
//            ApplyDefaultSort();
        }

        private void MenuItemOpen_Click(object sender, System.EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = @"Select an XFS file.",
                Filter = @"XFS File|*.xfs;",
                InitialDirectory = "C:\\AeriaGames\\WolfTeam-US"
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            CurrentFileReader?.Dispose();
            CurrentFileReader = new XfsFileReader(openFileDialog.FileName);
            CurrentFileReader.Run();

            fileListView.Roots = CurrentFileReader.Files;

            if (CurrentFileReader.Files.Count == 1)
            {
                fileListView.Expand(CurrentFileReader.Files[0]);
            }
            
            ApplyDefaultSort();
        }

        private void MenuItemExit_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void menuItemResetSort_Click(object sender, System.EventArgs e)
        {
            ApplyDefaultSort();
        }
    }
}
