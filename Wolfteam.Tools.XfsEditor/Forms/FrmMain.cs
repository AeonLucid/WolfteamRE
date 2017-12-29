using System.Windows.Forms;
using Wolfteam.Tools.XfsEditor.Xfs;

namespace Wolfteam.Tools.XfsEditor.Forms
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();

            CurrentFile = new XfsFile("C:\\AeriaGames\\WolfTeam-US\\wolf.xfs");
            CurrentFile.Read();
        }

        private XfsFile CurrentFile { get; set; }

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

            CurrentFile = new XfsFile(openFileDialog.FileName);
        }

        private void MenuItemExit_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
