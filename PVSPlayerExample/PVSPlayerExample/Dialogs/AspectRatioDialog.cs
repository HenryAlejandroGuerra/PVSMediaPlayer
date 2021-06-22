#region Usings

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using PVS.MediaPlayer;

#endregion

namespace PVSPlayerExample
{
    /*

    Aspect Ratio Dialog - Preview to change the main player's video aspect ratio.

    The preview is shown by using a display clone (with DisplayMode.Stretch) in a freely adjustable but not visible panel
    within another visible panel. The dimensions of the invisible panel are set according to the specified aspect ratio.

    */

    public partial class AspectRatioDialog : Form
    {
        // ******************************* Fields

        #region Fields

        private const string ERROR_NOT_A_NUMBER     = "Not a valid number.";
        private const string ERROR_INVALID_NUMBER   = "This value must be greater than or equal to 1.";
        private const string ERROR_CLEAR            = "";


        private MainWindow  _baseForm;
        private Player      _basePlayer;

        private bool        _enabled;
        private bool        _valid;

        NumberFormatInfo    _numberFormat;
        private float       _width;
        private float       _height;
        private bool        _reversed;

        private double      _oldOpacity;

        private bool        _disposed;

        #endregion


        // ******************************* Main

        #region Main

        public AspectRatioDialog(MainWindow baseForm, Player basePlayer)
        {
            InitializeComponent();

            Icon        = Properties.Resources.Media_Normal;
            SizeChanged += AspectRatioDialog_SizeChanged;

            _baseForm   = baseForm;
            _basePlayer = basePlayer;

            _numberFormat = new NumberFormatInfo();
            _numberFormat.NumberDecimalSeparator = ".";

            _basePlayer.DragAndDrop.Add(this);

            _oldOpacity = Opacity;
            Opacity     = 0;
        }

        private void AspectRatioDialog_Shown(object sender, System.EventArgs e)
        {
            SetMediaInfo();

            // get aspect ratio
            if (_baseForm.HasAspectRatio)
            {
                SizeF _aspectRatio      = _baseForm.AspectRatio;
                _width                  = _aspectRatio.Width;
                _height                 = _aspectRatio.Height;
            }
            else
            {
                _width                  = 16;
                _height                 = 9;
            }

            widthBox.Text               = _width.ToString(_numberFormat);
            heightBox.Text              = _height.ToString(_numberFormat);

            useRatioCheckBox.Checked    = _baseForm.HasAspectRatio || _baseForm.HasAspectRatioDVD;
            restoreCheckBox.Checked     = _baseForm.AspectRatioRemove;
            dvdCheckBox.Checked         = _baseForm.AspectRatioDVD;

            _enabled                    = useRatioCheckBox.Checked;
            _valid                      = true;

            // set preview display - displayclone
            _basePlayer.DisplayClones.Add(videoPanel);
            CloneProperties props       = _basePlayer.DisplayClones.GetProperties(videoPanel);
            props.DragEnabled           = true;
            props.Layout                = CloneLayout.Stretch;
            _basePlayer.DisplayClones.SetProperties(videoPanel, props);

            // set preview
            if (_basePlayer.Playing)
            {
                UpdatePreview();
            }

            _basePlayer.Events.MediaStarted += BasePlayer_MediaStarted;
            _basePlayer.Events.MediaEndedNotice += BasePlayer_MediaEndedNotice;
            _basePlayer.Events.MediaOverlayActiveChanged += BasePlayer_MediaOverlayActiveChanged;

            MouseDown += DragForm_MouseDown;
            bottomPanel.MouseDown += DragForm_MouseDown;
            pvsPanel.MouseDown += DragForm_MouseDown;

            //Application.DoEvents();
            //System.Threading.Thread.Sleep(50);
            //Opacity = _oldOpacity;
            MainWindow.DialogFadeIn(this, _oldOpacity);
        }

        private void PvsPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Properties.Resources.PVSLogoOutline, 4, 0);
        }

        private void AspectRatioDialog_SizeChanged(object sender, System.EventArgs e)
        {
            UpdatePreview();
        }

        private void AspectRatioDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _basePlayer.Events.MediaStarted -= BasePlayer_MediaStarted;
            _basePlayer.Events.MediaEndedNotice -= BasePlayer_MediaEndedNotice;
            _basePlayer.Events.MediaOverlayActiveChanged -= BasePlayer_MediaOverlayActiveChanged;

            _basePlayer.DisplayClones.Remove(videoPanel);
        }

		protected override void Dispose(bool disposing)
        {
            // see also AspectRatioDialog_FormClosed
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    _baseForm = null;
                    _basePlayer = null;

                    if (components != null) components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion


        // ******************************** Player Events

        #region Player Events

        private void BasePlayer_MediaStarted(object sender, System.EventArgs e)
        {
            SetMediaInfo();
            UpdatePreview();

            // in rare cases, display clones can cause minor display problems if they are resized during certain media startup
            if (_basePlayer.Media.GetName(MediaName.Extension).Equals(".avi", StringComparison.OrdinalIgnoreCase) && _basePlayer.Has.Video && _basePlayer.Media.SourceCategory == MediaSourceCategory.LocalFile)
            {
                System.Threading.Thread.Sleep(50);
                Application.DoEvents();
                _basePlayer.Position.FromBegin += TimeSpan.FromMilliseconds(50);
            }
        }

        private void BasePlayer_MediaEndedNotice(object sender, EndedEventArgs e)
        {
            SetMediaInfo();
        }

        private void BasePlayer_MediaOverlayActiveChanged(object sender, EventArgs e)
        {
            // the player automatically activates the mp3 cover overlay AFTER media has started playing
            // so the media started event does not set this particular overlay correctly
            Application.DoEvents();
            UpdatePreview();
        }

        #endregion


        // ******************************* Interface Items

        #region Interface Items

        private void SetMediaInfo()
        {
            if (_basePlayer.Playing)
            {
                nameLabel.Text = _basePlayer.Media.GetName(MediaName.FileName);
                if (_basePlayer.Has.Video)
                {
                    sizeLabel.Text = _basePlayer.Media.VideoSourceSize.ToString().Trim(new char[] { '{', '}' });
                }
                else sizeLabel.Text = "No Video";
            }
            else
            {
                nameLabel.Text = "No Media";
                sizeLabel.Text = "-";
            }
        }

        private void UseRatioCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            _enabled = useRatioCheckBox.Checked;
            updateButton.Enabled = _enabled;

            if (_enabled)
            {
                WidthBox_Check();
                HeightBox_Check();
            }
            else
            {
                videoPanel.Wipe = false;

                errorProvider1.SetError(widthBox, ERROR_CLEAR);
                errorProvider1.SetError(heightBox, ERROR_CLEAR);

                Application.DoEvents();
                videoPanel.Wipe = true;

                // don't set dvd mode - don't overwrite disabled selection
            }

            UpdatePreview();
        }

        private void DvdCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (dvdCheckBox.Checked && !useRatioCheckBox.Checked && _basePlayer.Has.Video && ((_basePlayer.Video.SourceSize.Width == 704 || _basePlayer.Video.SourceSize.Width == 720) && _basePlayer.Video.SourceSize.Height == 576))
            {
                _width = 16;
                _height = 9;
                useRatioCheckBox.Checked = true;
            }
        }

        private void PresetRatesMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            float temp;
            bool insert = true;
            int index   = presetRatesMenu.Items.IndexOf(e.ClickedItem);

            switch (index)
            {
                case 0: // 1:1
                    _width  = 1F;
                    _height = 1F;
                    break;

                case 1: // 5:4
                    _width  = 5F;
                    _height = 4F;
                    break;

                case 2: // 4:3
                    _width  = 4F;
                    _height = 3F;
                    break;

                case 3: // 16:10
                    _width  = 16F;
                    _height = 10F;
                    break;

                case 4: // 16:9
                    _width  = 16F;
                    _height = 9F;
                    break;

                case 5: // 1.85:1
                    _width  = 1.85F;
                    _height = 1F;
                    break;

                case 6: // 2:1
                    _width = 2F;
                    _height = 1F;
                    break;

                case 7: // 2.35:1
                    _width  = 2.35F;
                    _height = 1F;
                    break;

                case 8: // 2.39:1
                    _width  = 2.39F;
                    _height = 1F;
                    break;

                // case 9: Separator

                case 10: // Reversed
                    _reversed = !_reversed;
                    reversedMenuItem.Checked = _reversed;

                    if (!_reversed)
                    {
                        temp    = _width;
                        _width  = _height;
                        _height = temp;
                    }
                    break;

                default:
                    insert = false;
                    break;
            }

            if (insert)
            {
                if (_reversed)
                {
                    temp    = _width;
                    _width  = _height;
                    _height = temp;
                }

                widthBox.Text   = _width.ToString(_numberFormat);
                heightBox.Text  = _height.ToString(_numberFormat);

                if (_enabled)
                {
                    WidthBox_Check();
                    HeightBox_Check();
                    UpdatePreview();
                }
                else useRatioCheckBox.Checked = true;
            }
        }

        private void UpdateButton_Click(object sender, System.EventArgs e)
        {
            UpdatePreview();
            okButton.Focus();
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, System.EventArgs e)
        {
            // oops, this a little bit complicated :)

            bool setRatio = false;

            if (_enabled)
            {
                if (_valid)
                {
                    _baseForm.HasAspectRatio    = true;
                    _baseForm.HasAspectRatioDVD = false;
                    _baseForm.AspectRatio       = new SizeF(_width, _height);
                    setRatio                    = true;
                }
                else return; // break
            }
            else
            {
                if (_baseForm.HasAspectRatio || _baseForm.HasAspectRatioDVD) setRatio = true;

                _baseForm.HasAspectRatio        = false;
                _baseForm.AspectRatio           = Size.Empty;

                if (dvdCheckBox.Checked)
                {
                    if (!_baseForm.HasAspectRatioDVD && _basePlayer.Has.Video && ((_basePlayer.Video.SourceSize.Width == 704 || _basePlayer.Video.SourceSize.Width == 720) && _basePlayer.Video.SourceSize.Height == 576))
                    {
                        _basePlayer.Video.Widescreen = true;
                        _baseForm.HasAspectRatioDVD  = true;
                    }
                    setRatio = false;
                }
                else
                {
                    _baseForm.HasAspectRatioDVD = false;
                    _baseForm.AspectRatioDVD    = false;
                }
            }

            _baseForm.AspectRatioRemove = restoreCheckBox.Checked;
            _baseForm.AspectRatioDVD    = dvdCheckBox.Checked;

            if (setRatio) _basePlayer.Video.AspectRatio = _baseForm.AspectRatio;

            Close();
        }

        #endregion


        // ******************************* Input Validation

        #region Input Validation

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.')) e.Handled = true;
            else if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1)) e.Handled = true;
        }

        private void TextBox_Enter(object sender, System.EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void WidthBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WidthBox_Check();
        }

        private void HeightBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HeightBox_Check();
            if (_enabled && _valid) UpdatePreview();
        }

        private void WidthBox_Check()
        {
            GetTextBoxValue(widthBox, widthErrorPanel, out _width);
            _valid = (_width != -1 && _height != -1);
        }

        private void HeightBox_Check()
        {
            GetTextBoxValue(heightBox, heightErrorPanel, out _height);
            _valid = (_width != -1 && _height != -1);
        }

        private void GetTextBoxValue(TextBox inputBox, Panel errorPanel, out float number)
        {
            if (!float.TryParse(inputBox.Text, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US"), out number))
            {
                number = -1;
                errorProvider1.SetError(errorPanel, ERROR_NOT_A_NUMBER);
            }
            else if (number < 1)
            {
                number = -1;
                errorProvider1.SetError(errorPanel, ERROR_INVALID_NUMBER);
            }
            else
            {
                videoPanel.Wipe = false;
                errorProvider1.SetError(errorPanel, ERROR_CLEAR);
                Application.DoEvents();
                videoPanel.Wipe = true;
            }
        }

        #endregion


        // ******************************* Update Preview

        #region Update Preview

        private void UpdatePreview()
        {
            if (_basePlayer.Has.Video || _basePlayer.Has.OverlayShown)
            {
                Rectangle bounds;

                if (_basePlayer.Has.Video)
                {
                    bounds = new Rectangle(Point.Empty, _basePlayer.Video.SourceSize);

                    if (_enabled && _valid)
                    {
                        if (bounds.Width > bounds.Height) bounds.Height = (int)((_height / _width) * bounds.Width);
                        else bounds.Width = (int)((_width / _height) * bounds.Height);

                        updateButton.Enabled = true;
                    }
                }
                else
                {
                    bounds = new Rectangle(Point.Empty, _basePlayer.Overlay.Bounds.Size);
                    updateButton.Enabled = false;
                }

                double difX = (double)displayPanel.Width / bounds.Width;
                double difY = (double)displayPanel.Height / bounds.Height;

                if (difX < difY)
                {
                    bounds.Width = (int)(difX * bounds.Width);
                    bounds.Height = (int)(difX * bounds.Height);

                    bounds.X = 0;
                    bounds.Y = (displayPanel.Height - bounds.Height) / 2;
                }
                else
                {
                    bounds.Width = (int)(difY * bounds.Width);
                    bounds.Height = (int)(difY * bounds.Height);

                    bounds.Y = 0;
                    bounds.X = (displayPanel.Width - bounds.Width) / 2;
                }

                if (videoPanel.Bounds != bounds) videoPanel.Bounds = bounds;
            }
            else
            {
                Rectangle r = new Rectangle(Point.Empty, displayPanel.Size);
                if (videoPanel.Bounds != r) videoPanel.Bounds = r;
                updateButton.Enabled = false;
            }
        }

        #endregion

        
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
