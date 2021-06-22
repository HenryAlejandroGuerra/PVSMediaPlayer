#region Usings

using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

#endregion

namespace PVSPlayerExample
{
    // PlayListBox - asks (save/don't save/cancel) if user wants to save the current playlist
    // before opening another one.

    public partial class PlaylistDialog : Form
    {

        #region Fields

        double      _oldOpacity;

        #endregion


        public PlaylistDialog(string headText)
        {
            InitializeComponent();
            Icon = Properties.Resources.Media_Normal;

            label1.Text = headText;
            _oldOpacity = Opacity;
            Opacity = 0;
        }

        private void PlayListDialog_Shown(object sender, EventArgs e)
        {
            MouseDown += DragForm_MouseDown;
            bottomPanel.MouseDown += DragForm_MouseDown;
            pvsPanel.MouseDown += DragForm_MouseDown;

            //Application.DoEvents();
            //System.Threading.Thread.Sleep(50);
            //Opacity = _oldOpacity;
            MainWindow.DialogFadeIn(this, _oldOpacity);

            SystemSounds.Question.Play();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawIcon(SystemIcons.Question, 23, 25);
            Pen pen = new Pen(Color.FromArgb(109, 103, 76), 1);
            Rectangle rect = DisplayRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            e.Graphics.DrawRectangle(pen, rect);
            pen.Dispose();
        }

        private void PvsPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Properties.Resources.PVSLogoOutline, 4, 0);
        }

        // OK - Save
        private void SaveButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        // No - Don't Save
        private void NoSaveButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        // Cancel
        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }


        // ******************************** Drag Form

        #region Drag Form

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void DragForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Size savedSize = Size;

                Cursor = Cursors.SizeAll;
                ((Control)sender).Capture = false;
                Message msg = Message.Create(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
                base.WndProc(ref msg);

                Size = savedSize;

                Cursor = Cursors.Default;
            }
        }

        #endregion

    }
}
