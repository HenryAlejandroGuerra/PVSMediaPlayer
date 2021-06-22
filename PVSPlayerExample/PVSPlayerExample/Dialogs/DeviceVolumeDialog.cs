#region Usings

using PVS.MediaPlayer;
using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace PVSPlayerExample
{
	public partial class DeviceVolumeDialog : Form
	{
        #region Fields

        private const int   TIMER_INTERVAL      = 500;
        private const int   FAST_INTERVAL       = 50;

		private Player      _basePlayer;

        private Timer       _timer;
        private bool        _fastMode;
        private int         _fastCount;

        //private InfoLabel   _volumeLabel;

        private bool        _hasAudiodevices;

        private bool        _mute;
        private float       _volume             = -1;
        private bool        _dialBusy;
        private bool        _sliderBusy;
        private bool        _ignoreValueChange;

        private double      _oldOpacity;
        private bool        _disposed;

        #endregion


        #region Main

        public DeviceVolumeDialog(Player basePlayer)
		{
			InitializeComponent();

			Icon            = Properties.Resources.Media_Normal;

			_basePlayer     = basePlayer;

            //_volumeLabel = new InfoLabel
            //{
            //    FontSize = 16f,
            //    RoundedCorners = true,
            //    TextMargin = new Padding(8, 0, 8, 2),
            //    ForeColor = Color.FromArgb(179, 173, 146)
            //};
            //_volumeLabel.BackBrush = new LinearGradientBrush(new Rectangle(new Point(0, 0), _volumeLabel.Size),
            //    Color.FromArgb(64, 64, 64), Color.Black, LinearGradientMode.Vertical);

            _timer          = new Timer();
            _timer.Interval = TIMER_INTERVAL;
			_timer.Tick     += Timer_Tick;

            _oldOpacity     = Opacity;
            Opacity         = 0;
        }

		private void MasterVolumeDialog_Shown(object sender, EventArgs e)
        {
            _basePlayer.DragAndDrop.Add(this);
            _basePlayer.Events.MediaAudioDeviceChanged += BasePlayer_MediaAudioDeviceChanged;

            SetAudioDevice();
            _timer.Start();

            KeyPreview              = true;
            KeyDown                 += MasterVolumeDialog_KeyDown;

            MouseDown               += DragForm_MouseDown;
            bottomPanel.MouseDown   += DragForm_MouseDown;
            pvsPanel.MouseDown      += DragForm_MouseDown;

            //Application.DoEvents();
            //System.Threading.Thread.Sleep(75);
            //Opacity = _oldOpacity;
            MainWindow.DialogFadeIn(this, _oldOpacity);
        }

        private void MasterVolumeDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer.Dispose();

            _basePlayer.DragAndDrop.Remove(this);
            _basePlayer.Events.MediaAudioDeviceChanged -= BasePlayer_MediaAudioDeviceChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    if (_timer != null) _timer.Dispose();
                    _basePlayer = null;

                    if (components != null) components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion


        #region Event Handling

        protected override void OnPaint(PaintEventArgs e)
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_ignoreValueChange)
            {
                SetMute(_basePlayer.Audio.DeviceMute, false);
                SetVolume(_basePlayer.Audio.DeviceVolume, false);
            }
        }

        private void BasePlayer_MediaAudioDeviceChanged(object sender, EventArgs e)
        { SetAudioDevice(); }

		private void VolumeControl_MouseDown(object sender, MouseEventArgs e)
		{ _timer.Stop(); }

        private void VolumeControl_MouseUp(object sender, MouseEventArgs e)
        { _timer.Start(); }

        private void VolumeDial_ValueChanged(object sender, Dial.ValueChangedEventArgs e)
        {
            if (!_ignoreValueChange)
            {
                _dialBusy = true;
                SetVolume((float)volumeDial.Value / 1000, true);
                _dialBusy = false;
            }
        }

        //private void VolumeSlider_Scroll(object sender, EventArgs e)
        //{
        //    // Get the position slider's x-coordinate of the current position (= thumb location)
        //    Point location = SliderValue.ToPoint(volumeSlider, volumeSlider.Value);
        //    location.Y = 9;

        //    // Show the infolabel
        //    _volumeLabel.Show(volumeSlider.Value.ToString(), volumeSlider, location);
        //}

        private void VolumeSlider_ValueChanged(object sender, EventArgs e)
        {
            if (!_ignoreValueChange)
            {
                _sliderBusy = true;
                SetVolume((float)volumeSlider.Value / 100, true);
                _sliderBusy = false;
            }
        }

        private void DeviceNameLabel_Click(object sender, EventArgs e)
        {
            SetMute(!_mute, true);
        }

        private void MasterVolumeDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void MuteButton_Click(object sender, EventArgs e)
        {
            OKButton.Focus();
            SetMute(!_mute, true);
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region Settings

        private void SetVolume(float volume, bool setDeviceVolume)
        {
            if (volume != _volume)
            {
				_ignoreValueChange = true;

                _volume = volume;

                if (setDeviceVolume)
                {
                    _basePlayer.Audio.DeviceVolume = _volume;
                }
                else
                {
                    if (!_fastMode)
                    {
                        _fastMode = true;
                        _fastCount = 0;
                        _timer.Interval = FAST_INTERVAL;
                    }
                    else _fastCount = 0;
                }

                if (!_dialBusy) volumeDial.Value = (int)((_volume * 1000) + 0.05);
                if (!_sliderBusy) volumeSlider.Value = (int)((_volume * 100) + 0.05);

                volumeLabel.Text = ((int)((_volume * 100) + 0.05)).ToString();

                _ignoreValueChange = false;
            }
            if (_fastMode)
            {
                if (_fastCount++ > 15)
                {
                    _fastMode = false;
                    _fastCount = 0;
                    _timer.Interval = TIMER_INTERVAL;
                }
            }
        }

        private void SetMute(bool mute, bool setDeviceMute)
        {
            if (mute != _mute)
            {
                _mute = mute;

                if (setDeviceMute) _basePlayer.Audio.DeviceMute = _mute;

                if (_mute)
                {
                    deviceNameLabel.ForeColor = Color.Red;
                    volumeLabel.ForeColor = Color.Red;
                    volumeDial.SwitchImage(true);
                    muteButton.Text = "Unmute";
                }
                else
                {
                    deviceNameLabel.ForeColor = UIColors.MenuTextEnabledColor;
                    volumeLabel.ForeColor = UIColors.MenuTextEnabledColor;
                    volumeDial.SwitchImage(false);
                    muteButton.Text = "Mute";
                }
            }
        }

        private void SetAudioDevice()
        {
            if (_basePlayer.Audio.DeviceCount == 0)
            {
                _ignoreValueChange = true;

                _hasAudiodevices = false;

                deviceNameLabel.Text = "No Audio Devices";
                volumeLabel.Text = "--";
                volumeDial.Value = 0;
                volumeSlider.Value = 0;

                volumeDial.Enabled = false;
                volumeSlider.Enabled = false;

                _volume = -1;

                _ignoreValueChange = false;
            }
            else
            {
                if (!_hasAudiodevices)
                {
                    volumeDial.Enabled = true;
                    volumeSlider.Enabled = true;

                    _hasAudiodevices = true;
                }

                AudioDevice device = _basePlayer.Audio.Device;
                if (device == null) device = _basePlayer.Audio.GetDefaultDevice();

                deviceNameLabel.Text = device.ToString();
                SetMute(_basePlayer.Audio.DeviceMute, false);
                SetVolume(_basePlayer.Audio.DeviceVolume, false);
            }
        }

		#endregion

		#region Context Menu

		private void MuteMenuItem_Click(object sender, EventArgs e)
		{
            //SetVolume(0, true);
            _timer.Interval = FAST_INTERVAL; // timer tick reads values and sets dial and slider, timer is set to lower interval afterwards
            _basePlayer.Audio.DeviceVolumeTo(0);
        }

		private void LowVolumeMenuItem_Click(object sender, EventArgs e)
		{
            //SetVolume(0.25f, true);
            _timer.Interval = FAST_INTERVAL; // timer tick reads values and sets dial and slider, timer is set to lower interval afterwards
            _basePlayer.Audio.DeviceVolumeTo(0.25f);
        }

		private void MediumVolumeMenuItem_Click(object sender, EventArgs e)
		{
            //SetVolume(0.50f, true);
            _timer.Interval = FAST_INTERVAL; // timer tick reads values and sets dial and slider, timer is set to lower interval afterwards
            _basePlayer.Audio.DeviceVolumeTo(0.50f);
        }

		private void HighVolumeMenuItem_Click(object sender, EventArgs e)
		{
            //SetVolume(0.75f, true);
            _timer.Interval = FAST_INTERVAL; // timer tick reads values and sets dial and slider, timer is set to lower interval afterwards
            _basePlayer.Audio.DeviceVolumeTo(0.75f);
        }

		private void MaximumVolumeMenuItem_Click(object sender, EventArgs e)
		{
            //SetVolume(1, true);
            _timer.Interval = FAST_INTERVAL; // timer tick reads values and sets dial and slider, timer is set to lower interval afterwards
            _basePlayer.Audio.DeviceVolumeTo(1);
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
