#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PVS.MediaPlayer;

#endregion

namespace PVSPlayerExample
{
    // AddUrl dialog - allows urls (http://... etc. links) to be added to the playlist.

    public partial class AddUrlDialog : Form
    {
        #region Fields

        private MainWindow  _baseForm;
        private Player      _basePlayer;
        private double      _oldOpacity;

        #endregion


        public AddUrlDialog(MainWindow baseForm, Player basePlayer)
        {
            InitializeComponent();
            Icon = Properties.Resources.Media_Normal;

            _baseForm = baseForm;
            _basePlayer = basePlayer;
            _baseForm._urlAdded = false;

            //DoubleBuffered = true;
            ResizeRedraw = true;
            _basePlayer.DragAndDrop.Add(this);

            _oldOpacity = Opacity;
            Opacity = 0;

        }

        private void AddURLDialog_Shown(object sender, EventArgs e)
        {
            MouseDown += DragForm_MouseDown;
            pvsPanel.MouseDown += DragForm_MouseDown;

            //Application.DoEvents();
            //System.Threading.Thread.Sleep(50);
            //Opacity = _oldOpacity;
            MainWindow.DialogFadeIn(this, _oldOpacity);
        }

        private void AddUrlDialog_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = DisplayRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            Pen pen = new Pen(Color.FromArgb(109, 103, 76), 1);
            e.Graphics.DrawRectangle(pen, rect);
            pen.Dispose();
        }

        private void PvsPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Properties.Resources.PVSLogoOutline, 4, 0);
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            GetUrLs();
            this.Visible = false;
            _baseForm = null;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            _baseForm = null;
            Close();
        }

        // Get the URLs and add them to the playlist
        private void GetUrLs()
        {
            List<string> urls = new List<string>();

            foreach (string item in textBoxURLs.Lines)
            {
                string testString = item.Trim();
                for (int i = 0; i < _baseForm.STREAMING_URLS.Length; i++)
                {
                    if (testString.StartsWith(_baseForm.STREAMING_URLS[i], StringComparison.OrdinalIgnoreCase))
                    {
                        urls.Add(testString);
                        break;
                    }
                }
            }
            if (urls.Count > 0)
            {
                _baseForm._urlToPlay = _baseForm.Playlist.Count;
                _baseForm.AddToPlayList(urls.ToArray());
                _baseForm._urlAdded = true;
            }
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
