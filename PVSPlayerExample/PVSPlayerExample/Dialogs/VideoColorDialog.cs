#region Usings

using PVS.MediaPlayer;
using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace PVSPlayerExample
{
    public partial class VideoColorDialog : Form
    {

        #region Fields

        private Player      _basePlayer;

        private double      _oldBrightness;
        private double      _brightness;
        private double      _oldContrast;
        private double      _contrast;
        private double      _oldHue;
        private double      _hue;
        private double      _oldSaturation;
        private double      _saturation;

        private double      _oldOpacity;

        private bool        _disposed;

        #endregion


        #region Main

        public VideoColorDialog(Player basePlayer)
        {
            InitializeComponent();

            Icon        = Properties.Resources.Media_Normal;

            _basePlayer = basePlayer;

            _basePlayer.Sliders.Brightness = brightnessSlider;
            brightnessSlider.TickFrequency = 20;
            GetBrightness();
            _oldBrightness = _brightness;

            _basePlayer.Sliders.Contrast = contrastSlider;
            contrastSlider.TickFrequency = 20;
            GetContrast();
            _oldContrast = _contrast;

            _basePlayer.Sliders.Hue = hueSlider;
            hueSlider.TickFrequency = 20;
            GetHue();
            _oldHue = _hue;

            _basePlayer.Sliders.Saturation = saturationSlider;
            saturationSlider.TickFrequency = 20;
            GetSaturation();
            _oldSaturation = _saturation;

            _basePlayer.DragAndDrop.Add(this);

            _oldOpacity = Opacity;
            Opacity     = 0;
        }

        private void VideoColorDialog_Shown(object sender, EventArgs e)
        {
            _basePlayer.Events.MediaStarted += Events_MediaStarted;
            _basePlayer.Events.MediaVideoColorChanged += Events_MediaVideoColorChanged;

            brightnessPanel.MouseDown += DragForm_MouseDown;
            brightnessLabel.MouseDown += DragForm_MouseDown;
            brightnessValue.MouseDown += DragForm_MouseDown;

            contrastPanel.MouseDown += DragForm_MouseDown;
            contrastLabel.MouseDown += DragForm_MouseDown;
            contrastValue.MouseDown += DragForm_MouseDown;

            huePanel.MouseDown += DragForm_MouseDown;
            hueLabel.MouseDown += DragForm_MouseDown;
            hueValue.MouseDown += DragForm_MouseDown;

            saturationPanel.MouseDown += DragForm_MouseDown;
            saturationLabel.MouseDown += DragForm_MouseDown;
            saturationValue.MouseDown += DragForm_MouseDown;

            //Application.DoEvents();
            //System.Threading.Thread.Sleep(75);
            //Opacity = _oldOpacity;
            MainWindow.DialogFadeIn(this, _oldOpacity);
        }

        private void Events_MediaStarted(object sender, EventArgs e)
        {
            // check if values set while not playing are now still valid
            if (brightnessLight.LightOn) GetBrightness();
            if (contrastLight.LightOn) GetContrast();
            if (hueLight.LightOn) GetHue();
            if (saturationLight.LightOn) GetSaturation();
        }

        private void Events_MediaVideoColorChanged(object sender, VideoColorEventArgs e)
        {
            switch (e.ColorAttribute)
            {
                case VideoColorAttribute.Brightness:
                    GetBrightness();
                    break;

                case VideoColorAttribute.Contrast:
                    GetContrast();
                    break;

                case VideoColorAttribute.Hue:
                    GetHue();
                    break;

                case VideoColorAttribute.Saturation:
                    GetSaturation();
                    break;
            }
        }

        private void VideoColorDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _basePlayer.Events.MediaStarted -= Events_MediaStarted;
            _basePlayer.Events.MediaVideoColorChanged -= Events_MediaVideoColorChanged;

            _basePlayer.Sliders.Brightness = null;
            _basePlayer.Sliders.Contrast = null;
            _basePlayer.Sliders.Hue = null;
            _basePlayer.Sliders.Saturation = null;
        }

		protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    _basePlayer = null;

                    if (components != null) components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Get Color Values

        // get and set slider values (labels) and indication colors - called by local init and sliders events
        // sliders are controlled by the player

        private void GetBrightness()
        {
            _brightness                 = _basePlayer.Video.Brightness;

            brightnessValue.Text        = _brightness.ToString("0.00");

            brightnessLight.LightOn     = _brightness != 0;
            brightnessValue.ForeColor   = _brightness == 0 ? UIColors.MenuTextEnabledColor : Color.Lime;
        }

        private void GetContrast()
        {
            _contrast               = _basePlayer.Video.Contrast;

            contrastValue.Text      = _contrast.ToString("0.00");

            contrastLight.LightOn   = _contrast != 0;
            contrastValue.ForeColor = _contrast == 0 ? UIColors.MenuTextEnabledColor : Color.Lime;
        }

        private void GetHue()
        {
            _hue                = _basePlayer.Video.Hue;

            hueValue.Text       = _hue.ToString("0.00");

            hueLight.LightOn    = _hue != 0;
            hueValue.ForeColor  = _hue == 0 ? UIColors.MenuTextEnabledColor : Color.Lime;
        }

        private void GetSaturation()
        {
            _saturation                 = _basePlayer.Video.Saturation;

            saturationValue.Text        = _saturation.ToString("0.00");

            saturationLight.LightOn     = _saturation != 0;
            saturationValue.ForeColor   = _saturation == 0 ? UIColors.MenuTextEnabledColor : Color.Lime;
        }

        #endregion

        #region Context Menu

        private void SetColorSettings(bool reset)
        {
            if (reset)
            {
                _basePlayer.Video.Brightness    = _oldBrightness;
                _basePlayer.Video.Contrast      = _oldContrast;
                _basePlayer.Video.Hue           = _oldHue;
                _basePlayer.Video.Saturation    = _oldSaturation;
            }
            else // clear:
            {
                _basePlayer.Video.Brightness    = 0;
                _basePlayer.Video.Contrast      = 0;
                _basePlayer.Video.Hue           = 0;
                _basePlayer.Video.Saturation    = 0;
            }

            GetBrightness();
            GetContrast();
            GetHue();
            GetSaturation();
        }

        int colorMenuItem = 0;
        private void ColorDialogMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Point location = PointToClient(Cursor.Position);

            if (brightnessPanel.Bounds.Contains(location))
            {
                resetItemMenuItem.Text = "Reset Brightness";
                resetItemMenuItem.Enabled = _brightness != _oldBrightness;

                clearItemMenuItem.Text = "Clear Brightness";
                clearItemMenuItem.Enabled = _brightness != 0; // && _oldBrightness != 0;

                resetAllMenuItem.Enabled = (_contrast != _oldContrast) || (_hue != _oldHue) || (_saturation != _oldSaturation);
                clearAllMenuItem.Enabled = (_contrast != 0) || (_hue != 0) || (_saturation != 0);

                colorMenuItem = 1;
            }
            else if (contrastPanel.Bounds.Contains(location))
            {
                resetItemMenuItem.Text = "Reset Contrast";
                resetItemMenuItem.Enabled = _contrast != _oldContrast;

                clearItemMenuItem.Text = "Clear Contrast";
                clearItemMenuItem.Enabled = _contrast != 0; // && _oldContrast != 0;

                resetAllMenuItem.Enabled = (_brightness != _oldBrightness) || (_hue != _oldHue) || (_saturation != _oldSaturation);
                clearAllMenuItem.Enabled = (_brightness != 0) || (_hue != 0) || (_saturation != 0);

                colorMenuItem = 2;
            }
            else if (huePanel.Bounds.Contains(location))
            {
                resetItemMenuItem.Text = "Reset Hue";
                resetItemMenuItem.Enabled = _hue != _oldHue;

                clearItemMenuItem.Text = "Clear Hue";
                clearItemMenuItem.Enabled = _hue != 0; // && _oldHue != 0;

                resetAllMenuItem.Enabled = (_brightness != _oldBrightness) || (_contrast != _oldContrast) || (_saturation != _oldSaturation);
                clearAllMenuItem.Enabled = (_brightness != 0) || (_contrast != 0) || (_saturation != 0);

                colorMenuItem = 3;
            }
            else if (saturationPanel.Bounds.Contains(location))
            {
                resetItemMenuItem.Text = "Reset Saturation";
                resetItemMenuItem.Enabled = _saturation != _oldSaturation;

                clearItemMenuItem.Text = "Clear Saturation";
                clearItemMenuItem.Enabled = _saturation != 0; // && _oldSaturation != 0;

                resetAllMenuItem.Enabled = (_brightness != _oldBrightness) || (_contrast != _oldContrast) || (_hue != _oldHue);
                clearAllMenuItem.Enabled = (_brightness != 0) || (_contrast != 0) || (_hue != 0);

                colorMenuItem = 4;
            }
        }

        private void ResetItemMenuItem_Click(object sender, EventArgs e)
        {
            switch (colorMenuItem)
            {
                case 1: // Brightness
                    _basePlayer.Video.Brightness = _oldBrightness;
                    GetBrightness();
                    break;

                case 2: // Contrast
                    _basePlayer.Video.Contrast = _oldContrast;
                    GetContrast();
                    break;

                case 3: // Hue
                    _basePlayer.Video.Hue = _oldHue;
                    GetHue();
                    break;

                case 4: // Saturation
                    _basePlayer.Video.Saturation = _oldSaturation;
                    GetSaturation();
                    break;
            }
        }

        private void ClearItemMenuItem_Click(object sender, EventArgs e)
        {
            switch (colorMenuItem)
            {
                case 1: // Brightness
                    _basePlayer.Video.Brightness = 0;
                    GetBrightness();
                    break;

                case 2: // Contrast
                    _basePlayer.Video.Contrast = 0;
                    GetContrast();
                    break;

                case 3: // Hue
                    _basePlayer.Video.Hue = 0;
                    GetHue();
                    break;

                case 4: // Saturation
                    _basePlayer.Video.Saturation = 0;
                    GetSaturation();
                    break;
            }
        }

        private void ResetAllMenuItem_Click(object sender, EventArgs e)
        {
            SetColorSettings(true);
        }

        private void ClearAllMenuItem_Click(object sender, EventArgs e)
        {
            SetColorSettings(false);
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        //private void CloseDialogMenuItem_Click(object sender, EventArgs e)
        //{
        //    DialogResult = DialogResult.OK;
        //    Close();
        //}

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
