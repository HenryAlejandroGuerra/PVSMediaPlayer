#region Usings

using PVS.MediaPlayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

#endregion

#region Suppress some compiler warnings
#pragma warning disable IDE0044 // Add readonly modifier
#endregion


[assembly: CLSCompliant(true)]

namespace PVSPlayerExample
{
    /* Basada en la aplicación de  Peter Vegter
    June 2021, The Netherlands
    
    Microsoft Windows 10 (64-bit)
    Microsoft Visual Studio 2019 Community
    Medion PC (Intel Core i5-7400 CPU @ 3.00 GHz, 8,0GB RAM, NVIDIA GeForce GT 1030)
    AOC i2490PXQU/BT 24-inch monitor
    Dell E173FP 17-inch monitor
    Logitech QuickCam Pro 9000
    Logitech HD Webcam C270

    */

    public sealed partial class MainWindow : Form
    {
        // ******************************** Fields

        #region Fields

        // The application
        internal const string       APPLICATION_NAME            = "PED Music Player";
        private const string        PREFERENCES_NAME            = "Preferencias";
        private const string        PLAYLIST_NAME               = "PlayList";
        private const string        COPY_NAME                   = "ScreenCopy";
        private const string        DEFAULT_PLAYLIST_TITLE      = "Sin Título";
        internal static string      _appDataPath                = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PVSPlayerExample\";

        private int                 _errorCount;                // used to break out of a row of errors with autoPlayNext and onErrorPlayNext;
        internal static bool        UrlClicked;                 // URL (article website link) clicked on the About Form

        // Custom Fonts
        internal PrivateFontCollection FontCollection;
        private Font                _crystalFont16;             // used with positionSlider counters and infolabel
        private Font                _wingDng38;                 // used with video zoom/move/stretch buttons
        private Font                _clockFont25;               // used with on-screen digital clock

        // The Player
        /// <summary>
        /// Represents the main mediaplayer in this sample application.
        /// </summary>
        internal Player             myPlayer;

        internal bool               Has3DView;
        internal bool               HasAspectRatio;
        internal SizeF              AspectRatio                 = new SizeF(16F, 9F);
        internal bool               AspectRatioRemove           = true;
        internal bool               AspectRatioDVD;
        internal bool               HasAspectRatioDVD;

        // Repeat options
        internal Random             _random                     = new Random();
        private int[]               _shuffleList;
        private int                 _shuffleToPlay;
        internal bool               RepeatOne;
        internal bool               RepeatAll;
        internal bool               RepeatShuffle;

        private TimeSpan            _startTimeNext;
        private TimeSpan            _stopTimeNext;

        // The interface
        internal bool               StopAndPlay;                // used with preventing 'flashing' interface elements:
        // when a mediafile is stopped and another is about to be played, the interface is not fully updated
        private double              _oldOpacity;                // used at start up

        private bool                _dontSetAudioDials;         // used with audio dials events (don't change the dials (again) with dial ValueChanged event)
        private bool                _volumeRedDial;
        private bool                _videoMoveMode;             // switch between video stretch and video move with same buttons
        private string              _videoMoveText              = "  Mover Video ↨";
        private string              _videoStretchText           = "  Estirar Video ↨";
        private Color               _pauseColor                 = Color.Lime; // play and pause buttons and play light
        private bool                _highSpeedMode;             // don't show that pause is enabled
        private bool                _ultraSpeedMode;

        private bool                _chapterMode;
        private bool                _hasChapterStartedEvent;

        // Used with Play button contextmenu and submenu
        private int                 _playMenuItemIndex;
        private bool                _playMenuRightButton;
        private Point               _playMenuPopUpLocation      = new Point(0, 0);

        // Position slider options
        private bool                _sliderHidden;
        private bool                _sliderBlocked;
        private bool                _sliderVisible              = true;
        private TimeSpan            _mark1                      = TimeSpan.Zero;
        private TimeSpan            _mark2                      = TimeSpan.Zero;
        private TimeSpan            _mark3                      = TimeSpan.Zero;
        private TimeSpan            _mark4                      = TimeSpan.Zero;
        private MediaChapter[]      _chapters_I;
        private MediaChapter[]      _chapters_II;
        private MediaChapter[]      _chapters_III;

        private InfoLabel           _positionLabel;
        private InfoLabel           _sliderLabel;

        // Open / Save file dialogs
        internal OpenFileDialog     OpenFileDialog1;            // select media files
        internal OpenFileDialog     OpenFileDialog2;            // select playlists
        internal SaveFileDialog     SaveFileDialog1;            // save playlists

        // Used with selecting media files
        internal const string       OPENMEDIA_DIALOG_TITLE      = "Agregar Archivos - " + APPLICATION_NAME;
        internal static string      MediaDirectory;
        internal const string       OPENMEDIA_DIALOG_FILTER     =
            " Media files (*.*)|*.3g2; *.3gp; *.3gp2; *.3gpp; *.aac; *.adts; *.asf; *.avi; *.hevc; *.m2ts; *.m4a; *.m4v; *.mkv; *.mov; *.mp3; *.mp4; *.mpeg; *.mpg; *.mts; *.sami; *.smi; *.vob; *.wav; *.webm; *.wma; *.wmv; *.chap|" +
            " Image files (*.*)|*.bmp; *.gif; *.heic; *.ico; *.jfif; *.jpeg; *.jpg; *.png; *.tif; *.tiff|" +
            " All files|*.*";

        // Used with opening and saving playlists
        internal const string       OPENPLAYLIST_DIALOG_TITLE   = "Open Playlist - " + APPLICATION_NAME;
        internal const string       ADDPLAYLIST_DIALOG_TITLE    = "Add Playlist - " + APPLICATION_NAME;
        internal const string       SAVEPLAYLIST_DIALOG_TITLE   = "Save Playlist - " + APPLICATION_NAME;
        internal const string       PLAYLIST_DIALOG_FILTER      = "Playlists|*.m3u; *.m3u8; *.ppl|All files|*.*";
        internal static string      PlayListDirectory;

        // ScreenCopy
        private string              _copyFile                   = _appDataPath + COPY_NAME + ".png";
        private ImageFormat         _copyFormat                 = ImageFormat.Png;
        private bool                _copyHasDisplayClone;

        // Playlist - a simple playlist with media file names
        internal List<string>       Playlist;
        private bool                _tempPlaylist;              // open file with... playlist - don't save as default playlist
        internal string             PlaylistFile                = _appDataPath + PLAYLIST_NAME + ".inf"; // default playlist
        private int                 _mediaToPlay;               // the next mediafile in the playlist to play
        private const int           START_PLAYITEMS             = 6; // used to skip the first playmenu items (like 'add media files')
        internal string[]           STREAMING_URLS              = { "http://", "https://", "tcp://", "udp://", "rtp://", "rtps://", "rtmp://", "rtsp://", "mms://" };
        private string              _playListExtensions         = ".m3u.m3u8.ppl";

        // Add URL to Playlist
        private AddUrlDialog        _addUrlDialog;
        internal bool               _urlAdded;
        internal int                _urlToPlay;

        // Audio Devices
        private const int           START_AUDIO_DEVICE_ITEMS    = 8; // menu constant items
        private AudioDevice[]       _audioDevices;
        private AudioDevice         _audioDeviceSelected;

        // Webcam Devices
        private WebcamDevice[]      _webcams;

        // Display Shapes
        private DisplayShape        _displayShape               = DisplayShape.Normal;
        private bool                _useVideoShape              = true;
        private bool                _setOverlayShape;

        // Display Overlay Examples
        private bool                _overlayMenuEnabled;        // the visibility of an overlay menu is controlled from the main application
        private bool                _overlayHold;               // overlay hold set by application
        private bool                _userOverlay;               // overlay set by user (used with AutoOverlay)
        // the overlays are created when used for the first time.
        private MessageOverlay      _messageOverlay;
        private ScribbleOverlay     _scribbleOverlay;
        private TilesOverlay        _tileOverlay;
        private BouncingOverlay     _bouncingOverlay;
        private PIPOverlay          _pipOverlay;
        private SubtitlesOverlay    _subtitlesOverlay;
        private ZoomSelectOverlay   _zoomSelectOverlay;
        private VideoWall           _videoWallOverlay;
        private Mp3CoverOverlay     _mp3CoverOverlay;
        private Mp3KaraokeOverlay   _mp3KaraokeOverlay;
        private BigTimeOverlay      _bigTimeOverlay;
        private StatusInfoOverlay   _statusInfoOverlay;

        // Class used with (local disk) finding subtitles and karaoke files
        internal FileSearch         SearchFile;
        internal const int          SEARCH_DEPTH                = 2;

        // Used with Open Website dialog
        private int                 _goToArticle                = 1;

        // On-Screen Digital Clock
        private bool                        _clockVisible;
        private System.Windows.Forms.Timer  _clockTimer;

        // On-Screen Output Level Meter
        private bool                _hasLevelMeterEvents;
        private bool                _levelMeterBusy;
        private float               _levelMeterLeft;
        private float               _levelMeterRight;
        //private double              _baseLevelMeterUnits;
        //private double              _trueLevelMeterUnits;
        private HatchBrush          _levelMeterBrush;
        private HatchStyle          _levelMeterHatchStyle       = HatchStyle.DarkVertical;
        private float               _levelMeterHoldLeft;
        private float               _levelMeterHoldRight;
        internal const float        LEVELMETER_SHORT_DELAY      = 0.1F;

        // Position Changed eventhandler
        private bool                _posTimerBusy;

        // PlayMenu Drag and Drop
        private bool                _ddMouseDown;
        private bool                _ddOurDrag;
        private int                 _ddSourceIndex;
        private Point               _ddMouseLocation;
        private ToolStripMenuItem   _ddDragMenuItem;

        // Modeless Dialogs
        internal VideoColorDialog   _videoColorDialog;
        internal ChannelMixerDialog _channelMixerDialog;
        internal AspectRatioDialog  _aspectRatioDialog;
        internal DeviceVolumeDialog _deviceVolumeDialog;
        internal VideoOverlayDialog _videoOverlayDialog;

        // Video Image Overlay
        internal Bitmap             _videoOverlay;
        internal string             _videoOverlayFileName;
        internal ImagePlacement     _videoOverlayPlacement         = ImagePlacement.Stretch;
        internal Color              _videoOverlayTransparency      = Color.Empty;
        internal int                _videoOverlayMenuSelection     = 0; // transparency menu selection
        internal float              _videoOverlayOpacity           = 1;

        // Disposing
        private bool                _disposed;

        #endregion


        // ******************************** Main - Initializing / Clock / About

        #region Main - Initializing / Clock / About

        // Start up sequence:
        // 1. method Form1(): initialize all 'standard' items (player, fonts, etc.)
        // 2. method Form1_Shown(): check for (open with) arguments and set preferences, auto start etc.
        //
        // Using 2. because player needs form to be first time shown/activated for certain options (e.g. set overlay)


        // Application starting point (for us):
        public MainWindow()
        {
            // Check if Microsoft Media Foundation is installed - see file Program.cs

            InitializeComponent();                              // set designer items

            Icon = Properties.Resources.Media_Normal;           // set main window icon

            try { Directory.CreateDirectory(_appDataPath); }    // create app/preferences folder
            catch { /* ignore */ }

            // Allow dropping media files on the form (handled in source file: DragDrop.cs):
            AllowDrop = true;

            // Install custom fonts
            InstallCustomFonts();

            // Class used with (local disk) subtitle search
            SearchFile = new FileSearch();

            // fix: first time display of custom contextmenus is usually at wrong position (.NET bug))
            screenCopyMenu.AutoSize = false; screenCopyMenu.Height = 0; screenCopyMenu.Show(0, 0); screenCopyMenu.Close(); screenCopyMenu.AutoSize = true;
            copyModeMenu.AutoSize = false; copyModeMenu.Height = 0; copyModeMenu.Show(0, 0); copyModeMenu.Close(); copyModeMenu.AutoSize = true;
            videoTracksMenuItem.DropDown.AutoSize = false; videoTracksMenuItem.DropDown.Height = 0; videoTracksMenuItem.DropDown.Show(0, 0); videoTracksMenuItem.DropDown.Close(); videoTracksMenuItem.DropDown.AutoSize = true;
            audioTracksMenuItem.DropDown.AutoSize = false; audioTracksMenuItem.DropDown.Height = 0; audioTracksMenuItem.DropDown.Show(0, 0); audioTracksMenuItem.DropDown.Close(); audioTracksMenuItem.DropDown.AutoSize = true;

            // Create the main Player:
            CreatePlayer();
            LoadPreferences(); // and load and set preferences (some prefs are set in Form1_Shown())

            // This comes after creating a player because some interface elements use player-settings:
            InitializeInterface(); // set up the user interface
            CreatePlayList();
            // (Almost) Ready to go:
            SetInterfaceOnMediaStop(true);

            _oldOpacity = Opacity;
            Opacity = 0; // 'continues' at Form1_Shown()
        }

        // Create the main Player
        private void CreatePlayer()
        {
            // Create a player with a display
            myPlayer = new Player(displayPanel);

            // Show playback progress in main window's taskbar item
            myPlayer.TaskbarProgress.Add(this);

            // Add Player EventHandlers:

            // Add a player media start eventhandler to update the interface:
            myPlayer.Events.MediaStarted += MyPlayer_MediaStarted;
            // Add a player media end eventhandler to update the interface and playing 'next' media files (if autoPlayNext == true):
            myPlayer.Events.MediaEnded += MyPlayer_MediaEnded;
            // Add a media start- endposition changed event for the playing media (to update start/end textbox values):
            myPlayer.Events.MediaStartStopTimeChanged += MyPlayer_MediaStartStopTimeChanged;
            // Add a player media position eventhandler for showing position time strings (labels on both sides of the position slider):
            myPlayer.Events.MediaPositionChanged += MyPlayer_MediaPositionChanged;
            // Add a player display mode eventhandler for setting the displaymode menu when changed (with move and zoom):
            myPlayer.Events.MediaDisplayModeChanged += MyPlayer_MediaDisplayModeChanged;
            // Add a player pause and resume eventhandler (using same handler) to update the Pause button and text of display contextmenu:
            myPlayer.Events.MediaPausedChanged += MyPlayer_MediaPausedResumed;
            // Display the playback relative speed next to the speedslider:
            myPlayer.Events.MediaSpeedChanged += MyPlayer_MediaSpeedChanged;
            // Display audio volume and balance info beneath audio dials:
            myPlayer.Events.MediaAudioVolumeChanged += MyPlayer_MediaAudioVolumeChanged;
            myPlayer.Events.MediaAudioBalanceChanged += MyPlayer_MediaAudioBalanceChanged;
            // Fullscreen / Fullscreenmode may be set from preferences
            myPlayer.Events.MediaFullScreenChanged += MyPlayer_MediaFullScreenSettingsChanged;
            myPlayer.Events.MediaFullScreenModeChanged += MyPlayer_MediaFullScreenSettingsChanged;

			myPlayer.Events.MediaVideo3DViewChanged += MyPlayer_MediaVideo3DViewChanged;
			myPlayer.Events.MediaVideoAspectRatioChanged += MyPlayer_MediaVideoAspectRatioChanged;

            // Used with search for subtitles
            myPlayer.Subtitles.DirectoryDepth = SEARCH_DEPTH;

            // Enable form drag by display
            SetWindowDrag(true);

            // Re-appearing delay of an overlay after minimized
            myPlayer.Overlay.Delay = 100;


            // custom display shape
            GraphicsPath path    = new GraphicsPath();

            Font font            = new Font("Times New Roman", 128);
            StringFormat format  = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            path.AddString("VIDEO", font.FontFamily, 0, 128, new Point(0, 0), format);
            path.AddString("SHAPE", font.FontFamily, 0, 128, new Point(0, 88), format);

            // Requires powerful cpu/gpu:
            //Pen pen = new Pen(Color.Black, 2);
            //path.Widen(pen); // outline, only the thickness of the pen is relevant
            //pen.Dispose();

            RectangleF r = path.GetBounds();
            r.X += (r.Width / 4); r.Width -= (r.Width / 2);
            r.Y += (r.Height / 4); r.Height -= (r.Height / 2);
            path.FillMode = FillMode.Alternate;
            path.AddEllipse(r);

            myPlayer.Display.CustomShape = path;

            format.Dispose();
            font.Dispose();
            path.Dispose();
        }

		// Set up the part of the user interface that hasn't been done by using the Visual Studio designer
		private void InitializeInterface()
        {
            // Set custom colored menus
            ToolStripManager.Renderer = new CustomMenuRenderer();

            // Set custom fonts
            positionLabel1.Font = _crystalFont16; positionLabel1.UseCompatibleTextRendering = true;
            positionLabel2.Font = _crystalFont16; positionLabel2.UseCompatibleTextRendering = true;

            //zoomInButton.Font = _wingDng38; zoomInButton.UseCompatibleTextRendering = true;
            //zoomOutButton.Font = _wingDng38; zoomOutButton.UseCompatibleTextRendering = true;

            //stretchUpButton.Font = _wingDng38; stretchUpButton.UseCompatibleTextRendering = true;
           // stretchDownButton.Font = _wingDng38; stretchDownButton.UseCompatibleTextRendering = true;
            //stretchLeftButton.Font = _wingDng38; stretchLeftButton.UseCompatibleTextRendering = true;
            //stretchRightButton.Font = _wingDng38; stretchRightButton.UseCompatibleTextRendering = true;

            // Set on-screen clock
            //clockLabel.Font = _clockFont25; clockLabel.UseCompatibleTextRendering = true;
            //clockLabel.ForeColor = Prefs.ClockColor;
            if (Prefs.ClockShow) SetClockDisplay(true);

            // Set on-screen output level meter
            //_levelMeterBrush = new SolidBrush(Prefs.MainLevelMeterColor);
            _levelMeterBrush = new HatchBrush(_levelMeterHatchStyle, Prefs.MainLevelMeterColor);
            myPlayer.Events.MediaPeakLevelChanged += MyPlayer_MediaPeakLevelChanged;
            _hasLevelMeterEvents = !myPlayer.LastError;

            // can't set color in editor anymore?
            speedLight.ForeColor = Color.Red;

            // This can't be done in the designer (?): remove left (check) margin in some menus:
            ((ToolStripDropDownMenu)playListMenuItem.DropDown).ShowImageMargin      = false;
            ((ToolStripDropDownMenu)displayClonesMenuItem.DropDown).ShowImageMargin = false;
            ((ToolStripDropDownMenu)webcamsMenuItem.DropDown).ShowImageMargin       = false;
            ((ToolStripDropDownMenu)chaptersAppleMenuItem.DropDown).ShowImageMargin = false;
            ((ToolStripDropDownMenu)chaptersNeroMenuItem.DropDown).ShowImageMargin  = false;
            //((ToolStripDropDownMenu)chaptersFileMenuItem.DropDown).ShowImageMargin  = false; // moved to SetChapters()

            // Fill the DisplayMode dropdown menu and set the DisplayMode buttontext
            foreach (string item in Enum.GetNames(typeof(DisplayMode)))
            {
                displayModeMenu.Items.Add(item);
                if (item == "Manual") displayModeMenu.Items[displayModeMenu.Items.Count - 1].Enabled = false;
            }
            displayModeMenu.Items.Add("-");
            displayModeMenu.Items.Add("Aspect Ratio…"); // if changed also change name and counter in DisplayModeMenu_ItemClicked method
            displayModeMenu.Items.Add("Stereo 3D View");
            ((ToolStripMenuItem)displayModeMenu.Items[displayModeMenu.Items.Count - 1]).DropDown = video3DMenu;

            SetDisplayModeMenu(myPlayer.Display.Mode, false);


            // When one of the zoom or move buttons has focus (also set after selecting the option from
            // the pop-up display menu - so these options can also be used with fullscreen view)
            // you can also use the mouse scrollwheel to zoom in/out, move left/right or move up/down
           // zoomInButton.MouseWheel += ZoomInButton_MouseWheel;
            //zoomOutButton.MouseWheel += ZoomInButton_MouseWheel;

           // stretchUpButton.MouseWheel += StretchUpButton_MouseWheel;
            //stretchDownButton.MouseWheel += StretchUpButton_MouseWheel;
           // stretchLeftButton.MouseWheel += StretchLeftButton_MouseWheel;
            //stretchRightButton.MouseWheel += StretchLeftButton_MouseWheel;

            // Let the player handle all the sliders (trackbars)
            myPlayer.Sliders.Speed = speedSlider;
            myPlayer.Sliders.Position.TrackBar = positionSlider;
            //myPlayer.Sliders.Position.Mode = PositionSliderMode.Progress; // v 1.4 default setting = progress
            positionSlider.MouseWheel += PositionSlider_MouseWheel;
            //Player1.Sliders.PositionMode = PositionSliderMode.Progress;
            //Player1.Sliders.AudioVolume = volumeSlider;   // replaced by dial control
            //Player1.Sliders.AudioBalance = balanceSlider; // replaced by dial control
            // Now replaced by dails not controlled directly by the player:
            volumeDial.ValueChanged += VolumeDial_ValueChanged;
            balanceDial.ValueChanged += BalanceDial_ValueChanged;
            myPlayer.Sliders.Shuttle = shuttleSlider;

            // this is set AFTER the init of the player sliders, because they have a scroll event handler
            // that will set a value - if infolabels come first, they will display the wrong value
            if (Prefs.ShowInfoLabels) SetInfoLabels(Prefs.ShowInfoLabels);

            // Auto-hide mouse cursor
            if (Prefs.AutoHideCursor) myPlayer.CursorHide.Add(this);

            // Set Audio Device menu
            CreateAudioDeviceMenu();
            myPlayer.Events.MediaAudioDeviceChanged += MyPlayer_MediaAudioDeviceChanged;
            myPlayer.Events.MediaSystemAudioDevicesChanged += MyPlayer_MediaSystemAudioDevicesChanged;

            // Create Open and Save FileDialogs:
            CreateFileDialogs();

            // Set contextmenus shortcut keys:
            SetShortCutKeys();

            // Besides the player's display contextmenu the ESC-key can be used to switch off fullscreen mode
            // Also handles a few media keyboard keys and others
            KeyPreview = true;

            // Disable separators in all contextmenus
            for (int i = 0; i < components.Components.Count; i++)
            {
                if (components.Components[i].GetType() == typeof(ContextMenuStrip))
                {
                    DisableMenuSeparators((ToolStripDropDown)components.Components[i]);
                }
            }
        }

        internal void DisableMenuSeparators(ToolStripDropDown dropDown)
        {
            for (int j = 0; j < dropDown.Items.Count; j++)
            {
                if (dropDown.Items[j].GetType() == typeof(ToolStripSeparator))
                {
                    dropDown.Items[j].Enabled = false;
                }
                else  if (((ToolStripMenuItem)dropDown.Items[j]).DropDownItems.Count > 0)
                {
                    DisableMenuSeparators(((ToolStripMenuItem)dropDown.Items[j]).DropDown);
                }
            }
        }

        // Set the shortcut keys for some contextmenus that couldn't be set properly with the designer
        private void SetShortCutKeys()
        {
            // displayMenu
            stopMenuItem.ShortcutKeys = Keys.Control | Keys.OemPeriod;
            stopMenuItem.ShortcutKeyDisplayString = "Ctrl+.";

            // displayModeMenu
            // this Items[index] is used because the menu is generated from code (enums) and the items have no name
            ((ToolStripMenuItem)displayModeMenu.Items[2]).ShortcutKeys = Keys.F6;
            ((ToolStripMenuItem)displayModeMenu.Items[4]).ShortcutKeys = Keys.F7;
        }

        // Create a PlayList
        private void CreatePlayList()
        {
            Playlist = new List<string>();
        }

        // Create Open and Save FileDialogs
        private void CreateFileDialogs()
        {
            // Create an OpenFileDialog for selecting mediafiles
            OpenFileDialog1 = new OpenFileDialog
            {
                Filter = OPENMEDIA_DIALOG_FILTER,
                FilterIndex = 1 // 1 = Media Files
            };
            MediaDirectory                      = Prefs.MediaFilesFolder;
            OpenFileDialog1.Multiselect         = true;

            // Create an OpenFileDialog for selecting playlists
            OpenFileDialog2 = new OpenFileDialog
            {
                Filter = PLAYLIST_DIALOG_FILTER,
                InitialDirectory = Prefs.PlaylistsFolder,
                Multiselect = false
            };

            // Create a SaveFileDialog for saving playlists
            SaveFileDialog1                     = new SaveFileDialog {Title = SAVEPLAYLIST_DIALOG_TITLE, Filter = PLAYLIST_DIALOG_FILTER};
            SaveFileDialog1.DefaultExt          = "m3u";
            PlayListDirectory                   = Prefs.PlaylistsFolder;
        }

        // Digital Clock
        internal void SetClockDisplay(bool showClock)
        {
            if (_clockVisible != showClock)
            {
                _clockVisible = showClock;
                if (showClock)
                {
                    //nameLabel.Visible = webSiteLabel.Visible = false;

                   // if (Prefs.Clock24Hr) clockLabel.Text = DateTime.Now.ToString("HH:mm:ss");
                   //else clockLabel.Text = DateTime.Now.ToString("hh:mm:ss");

                    //clockLabel.Visible = true;

                    _clockTimer = new System.Windows.Forms.Timer
                    {
                        Interval = 1000
                    };
                    _clockTimer.Tick += ClockTimer_Tick;
                    _clockTimer.Start();
                }
                else
                {
                    _clockTimer.Stop();
                    _clockTimer.Tick -= ClockTimer_Tick;
                    _clockTimer.Dispose(); _clockTimer = null;

                   // clockLabel.Visible = true;
                    //nameLabel.Visible = webSiteLabel.Visible = false;
                }
            }
        }

        internal void SetClockColor(Color color)
        {
            //clockLabel.ForeColor = color;
        }

        internal void SetClockTime()
        {
            //if (Prefs.Clock24Hr) clockLabel.Text = DateTime.Now.ToString("HH:mm:ss");
           // else clockLabel.Text = DateTime.Now.ToString("hh:mm:ss");
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
           // if (Prefs.Clock24Hr) clockLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            //else clockLabel.Text = DateTime.Now.ToString("hh:mm:ss");
        }

        // Show the About message
        private void ShowAbout()
        {
            AboutDialog aboutMessage = new AboutDialog(this);
            CenterDialog(this, aboutMessage);
            aboutMessage.ShowDialog(this);
            aboutMessage.Dispose();

            toolTip1.Active = Prefs.ShowTooltips;
            if (UrlClicked) WebSiteLabel_Click(this, EventArgs.Empty);
        }

        #endregion

        // ******************************** Main Form EventHandling - Shown / Closed / KeyDown / ToolTip / Textboxes Enter Key / Audio Dials / Scroll / Drag Window / Dispose

        #region Main Form EventHandling - Shown / Closed / KeyDown / ToolTip / Textboxes Enter Key / Audio Dials / Scroll / Drag Window / Dispose

        // When the main form (already initialized) is shown for the first time
        private void Form1_Shown(object sender, EventArgs e)
        {
            bool autoPlayArg = false;
            bool autoPlayMp3 = false;

            // Set Window Size and Position Preference:
            if (Prefs.SaveWindow || (Prefs.ContinuePlay && Prefs.MediaToPlay >= 0))
            {
                if (Prefs.Maximized) WindowState = FormWindowState.Maximized;
                myPlayer.FullScreenMode = Prefs.FullScreenMode;
                myPlayer.FullScreen = Prefs.Fullscreen;
            }
            toolTip1.Active = Prefs.ShowTooltips;

            if (Prefs.ShowSliderPreview) SetSliderPreview(true);

            #region Get commandline args or load standard PlayList

            // Get commandline args ('open with') or (if no args) load the (default) playlist
            string[] clArgs = Environment.GetCommandLineArgs();
            if (clArgs.Length > 1)
            {
                string[] copyArgs = new string[clArgs.Length - 1];
                for (int i = 1; i < clArgs.Length; i++) { copyArgs[i - 1] = clArgs[i]; }


                if (_playListExtensions.IndexOf(Path.GetExtension(copyArgs[0]), StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try
                    {
                        //Playlist = TryParsePlayListFile(copyArgs[0]);
                        Playlist.AddRange(myPlayer.Playlist.Open(copyArgs[0]));
                        Prefs.PlayListTitle = Path.GetFileNameWithoutExtension(copyArgs[0]);
                        ReBuildPlayListMenu();
                    }
                    catch { LoadPlayList(); }
                }
                else
                {
                    _tempPlaylist = true;
                    AddToPlayList(copyArgs);
                }

                if (Playlist.Count > 0)
                {
                    if (!Prefs.AutoOverlay)
                    {
                        string extension = Path.GetExtension(Playlist[0]);
                        autoPlayMp3 = string.Equals(extension, ".mp3", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".wma", StringComparison.OrdinalIgnoreCase);
                    }
                    if (!Prefs.AutoPlayStart) autoPlayArg = true;
                }
            }
            else LoadPlayList();

            #endregion

            if (autoPlayArg)
            {
                PlayNextMedia();
                if (autoPlayMp3) MP3CoverMenuItem.PerformClick();
            }
            else // if not playing from 'Open With':
            {
                // Continue Play Preference:
                if (Prefs.ContinuePlay && Prefs.MediaToPlay >= 0 && Playlist.Count > Prefs.MediaToPlay)
                {
                    if (Prefs.Paused || !Prefs.VideoPresent) _startTimeNext = TimeSpan.FromMilliseconds(Prefs.Position);
                    else
                    {
                        double startPos = Prefs.Position - (Prefs.RewindSecs * 1000);
                        _startTimeNext = startPos < Prefs.StartPosition ? TimeSpan.FromMilliseconds(Prefs.StartPosition) : TimeSpan.FromMilliseconds(startPos);
                    }
                    _stopTimeNext = TimeSpan.FromMilliseconds(Prefs.EndPosition);

                    myPlayer.Paused = Prefs.Paused;

                    // Overlay
                    if (Prefs.Overlay >= 0)
                    {
                        displayOverlayMenu.Items[Prefs.Overlay].PerformClick();
                        myPlayer.Overlay.Mode = Prefs.OverlayMode;
                    }

                    _mediaToPlay = Prefs.MediaToPlay;
                    PlayNextMedia();

                    // Overlay mode reset
                    if (Prefs.Overlay >= 0 && Prefs.AutoOverlay) _userOverlay = false;
                }
                // Auto Play at Program Start Preference?
                else if (Prefs.AutoPlayStart && Playlist.Count > 0)
                {
                    _mediaToPlay = 0;
                    PlayNextMedia();
                }

                if (myPlayer.Overlay.Window == null && Prefs.SaveOverlay && Prefs.Overlay >= 0) displayOverlayMenu.Items[Prefs.Overlay].PerformClick();
                if (Prefs.SaveRepeat || myPlayer.Playing)
                {
                    switch (Prefs.Repeat)
                    {
                        case 1:
                            repeatOneMenuItem.PerformClick();
                            break;
                        case 2:
                            repeatAllMenuItem.PerformClick();
                            break;
                        case 3:
                            shuffleMenuItem.PerformClick();
                            break;
                    }
                }
            }

			Application.DoEvents();
			Thread.Sleep(50);
			Opacity = _oldOpacity; // reduce flicker on opening
			//DialogFadeIn(this, _oldOpacity);

			// application now is up and running and ready for user input
		}

        // Save the preferences file (that needs, among others, the latest window settings)
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.DoEvents();
            SavePreferences();
        }

        // Handles keyboard keys
        // shortcut keys don't work before the menu has been opened once?
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control) // Handle CTRL combinations
            {
                #region Handle CTRL KeyCodes

                e.Handled = true;
                switch (e.KeyCode)
                {
                    case Keys.Enter: // Play - start playing first item in playlist
                        if (!myPlayer.Playing && Playlist.Count > 0)
                        {
                            _mediaToPlay = 0;
                            if (RepeatShuffle) SetShuffleList();
                            PlayNextMedia();
                        }
                        break;

                    case Keys.N: // New Playlist
                        newPlayListMenuItem.PerformClick();
                        break;
                    case Keys.O: // Open Playlist
                        openPlayListMenuItem.PerformClick();
                        break;
                    case Keys.A: // Add Playlist
                        addPlayListMenuItem.PerformClick();
                        break;
                    case Keys.S: // Save Playlist As
                        savePlayListMenuItem.PerformClick();
                        break;

                    case Keys.M: // Add Media Files
                        addMediaFilesMenuItem.PerformClick();
                        break;
                    case Keys.U: // Add Media Urls
                        addMediaURLMenuItem.PerformClick();
                        break;

                    case Keys.B: // Play Previous
                        previousMenuItem.PerformClick();
                        break;
                    case Keys.F: // Play Next
                        nextMenuItem.PerformClick();
                        break;
                    case Keys.Q: // Quit Application
                        quitMenuItem.PerformClick();
                        break;
                    case Keys.Space: // Pause/Resume Playing
                        pauseMenuItem.PerformClick();
                        break;
                    case Keys.OemPeriod: // Stop Playing
                        stopMenuItem.PerformClick();
                        break;

                    // Repeat
                    case Keys.T:
                        if ((ModifierKeys & Keys.Shift) == Keys.Shift) repeatChapterMenuItem.PerformClick();
                        else repeatOneMenuItem.PerformClick();
                        break;
                    case Keys.R:
                        if ((ModifierKeys & Keys.Shift) == Keys.Shift) repeatOffMenuItem.PerformClick();
                        else repeatAllMenuItem.PerformClick();
                        break;
                    case Keys.H:
                        shuffleMenuItem.PerformClick();
                        break;

                    // Speed
                    case Keys.Add:
                    case Keys.Oemplus: // Speed Increase
                        myPlayer.Speed.Rate += 100;
                        break;
                    case Keys.Subtract:
                    case Keys.OemMinus: // Speed Decrease
                        myPlayer.Speed.Rate -= 100;
                        break;

                    // Audio
                    case Keys.Up: // Audio Volume Up
                        myPlayer.Audio.Volume += 100;
                        break;
                    case Keys.Down: // Audio Volume Down
                        myPlayer.Audio.Volume -= 100;
                        break;
                    case Keys.Left: // Audio Balance Left
                        myPlayer.Audio.Balance -= 100;
                        break;
                    case Keys.Right: // Audio Balance Right
                        myPlayer.Audio.Balance += 100;
                        break;
                    case Keys.NumPad0:
                    case Keys.D0: // Audio Mute On/Off
                        myPlayer.Audio.Volume = myPlayer.Audio.Volume == 0 ? 1000 : 0;
                        break;

                    case Keys.D: // Add Display Clone
                        addCloneMenuItem.PerformClick();
                        break;

                    case Keys.W: // Play all webcams
                        _webcams = myPlayer.Webcam.GetDevices();
                        if (_webcams != null)
                        {
                            for (int index = 0; index < _webcams.Length; index++)
                            {
                                WebcamMenu_PlayWebcam(index);
                            }
                        }
                        break;

                    //case Keys.C:
                    //    DoScreenCopy();
                    //    break;

                    default:
                        e.Handled = false;
                        break;
                }

                #endregion
            }
            else if (e.Alt) // Handle ALT combinations (Display Overlays)
            {
                #region Handle ALT KeyCodes

                e.Handled = true;
                switch (e.KeyCode)
                {
                    // Display Overlays

                    // Toggle Overlay Mode
                    case Keys.D: // Overlay Display
                        displayMenuItem.PerformClick();
                        break;

                    case Keys.V: // Overlay Video
                        videoMenuItem.PerformClick();
                        break;

                    case Keys.H: // Overlay Hold
                        overlayHoldMenuItem.PerformClick();
                        break;

                    // Activate example overlay
                    case Keys.F1:
                            messageMenuItem.PerformClick();
                            break;
                    case Keys.F2:
                            scribbleMenuItem.PerformClick();
                            break;
                    case Keys.F3:
                            tilesMenuItem.PerformClick();
                            break;
                    case Keys.F4:
                            bouncingMenuItem.PerformClick();
                            break;
                    case Keys.F5:
                            PiPMenuItem.PerformClick();
                            break;
                    case Keys.F6:
                            subtitlesMenuItem.PerformClick();
                            break;
                    case Keys.F7:
                            zoomSelectMenuItem.PerformClick();
                            break;
                    case Keys.F8:
                            videoWallMenuItem.PerformClick();
                            break;
                    case Keys.F9:
                            MP3CoverMenuItem.PerformClick();
                            break;
                    case Keys.F10:
                            MP3KaraokeMenuItem.PerformClick();
                            break;
                    case Keys.F11:
                            bigTimeMenuItem.PerformClick();
                            break;
                    case Keys.F12:
                            statusInfoMenuItem.PerformClick();
                            break;

                    case Keys.O:
                    case Keys.D0: // Overlay Off
                            overlayOffMenuItem.PerformClick();
                            break;

                    case Keys.S:
                    case Keys.M: // Show Overlay Menu On/Off
                            overlayMenuMenuItem.PerformClick();
                            break;

                    default:
                        e.Handled = false;
                        break;
                }

                #endregion
            }
            else
            {
                #region Handle KeyCodes

                e.Handled = true;
                switch (e.KeyCode)
                {
                    // handle a few media keyboard keys:
                    // (audio (volume/mute) is handled by Windows)
                    case Keys.MediaNextTrack: // Play Next
                        nextMenuItem.PerformClick();
                        break;
                    case Keys.MediaPlayPause: // Pause/Resume Playing
                        pauseMenuItem.PerformClick();
                        break;
                    case Keys.MediaPreviousTrack: // Play Previous
                        previousMenuItem.PerformClick();
                        break;
                    case Keys.MediaStop: // Stop Playing
                        stopMenuItem.PerformClick();
                        break;

                    // Function keys
                    case Keys.F1: // Show About
                        //NameLabel_Click(nameLabel, EventArgs.Empty);
                        break;
                    case Keys.F2: // Ask Open WebSite
                       // WebSiteLabel_Click(webSiteLabel, EventArgs.Empty);
                        break;

                    case Keys.F3: // Screencopy Copy
                        if (e.Shift) clearCopyMenuItem.PerformClick();
                        else copyMenuItem.PerformClick();
                        break;
                    case Keys.F4: // Screencopy Open
                        openCopyMenuItem.PerformClick();
                        break;
                    case Keys.F5: // Screencopy Open With
                        openWithCopyMenuItem.PerformClick();
                        break;

                    case Keys.F6: // DisplayMode ZoomandCenter
                        SetDisplayModeMenu(DisplayMode.Stretch, true);
                        break;
                    case Keys.F7: // DisplayMode Stretch
                        SetDisplayModeMenu(DisplayMode.ZoomCenter, true);
                        break;

                    case Keys.F8: // FullScreen Form
                        fullScreenFormMenuItem.PerformClick();
                        break;
                    case Keys.F9: // // FullScreen Parent
                        fullScreenParentMenuItem.PerformClick();
                        break;
                    case Keys.F10: // // FullScreen Display
                        fullScreenDisplayMenuItem.PerformClick();
                        break;
                    case Keys.F11: // // FullScreen On/Off
                        fullScreenOffMenuItem.PerformClick();
                        break;

                    // ESC key
                    case Keys.Escape:
                        if (myPlayer.FullScreen)
                        {
                            myPlayer.FullScreen = false;
                            SetFullScreenModeMenu();
                        }
                        //else stopMenuItem.PerformClick();
                        break;

                    default:
                        e.Handled = false;
                        break;
                }

                #endregion
            }
        }

        // Draw tooltip custom colors
        private void ToolTip1_Draw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            e.DrawBorder();
            e.DrawText();
        }

        // Position textboxes Enter key
        private void PositionTextBoxes_KeyPress(object sender, KeyPressEventArgs e)
        {
           // if (e.KeyChar == 13)
            {
                //e.Handled = true;
               // if (sender == startTimeNextTextBox || sender == startTimeTextBox) ProcessTabKey(true);
                //else ProcessTabKey(false);
            }
        }

        // Speed textbox Enter key
        private void SpeedTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                ProcessTabKey(true);
                //ProcessTabKey(false); // cursor back to textbox
            }
        }

        // Audio volume dial 'turned'
        private void VolumeDial_ValueChanged(object sender, Dial.ValueChangedEventArgs e)
        {
            _dontSetAudioDials = true;
            myPlayer.Audio.Volume = e.Value * 0.001f;
            _dontSetAudioDials = false;
        }

        // Audio balance dial 'turned'
        private void BalanceDial_ValueChanged(object sender, Dial.ValueChangedEventArgs e)
        {
            _dontSetAudioDials = true;
            myPlayer.Audio.Balance = (e.Value * 0.002f) - 1.0f;
            _dontSetAudioDials = false;
        }

        private void PositionSlider_Scroll(object sender, EventArgs e)
        {
            if (ModifierKeys != Keys.Shift)
            {
                // Get the position slider's x-coordinate of the current position (= thumb location)
                Point location = SliderValue.ToPoint(positionSlider, positionSlider.Value);
                //location.Y = -1; // adjust vertical position - should do in infolabel init
                location.Y = -3;

                // Show the infolabel
                _positionLabel.Show(myPlayer.Position.FromStart.ToString(@"hh\:mm\:ss"), positionSlider, location);
            }
        }

        // disable position slider mousewheel
        private void PositionSlider_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void SpeedSlider_Scroll(object sender, EventArgs e)
        {
            if (ModifierKeys != Keys.Shift)
            {
                // Get the position slider's x-coordinate of the current position (= thumb location)
                Point location = SliderValue.ToPoint(speedSlider, speedSlider.Value);
                location.Y = 9; // move closer to thumb, with horizontal sliders ValueToPoint y = 0

                // Show the infolabel
                if (myPlayer.Playing) _sliderLabel.Show(string.Format("({0:0.00#})   {1:0.00#}   ({2:0.00#})", myPlayer.Speed.Minimum, myPlayer.Speed.Rate, myPlayer.Speed.Maximum), speedSlider, location);
                else _sliderLabel.Show(string.Format("(0.00)   {0:0.00#}   (8.00)", myPlayer.Speed.Rate), speedSlider, location);
            }
        }

        private void ShuttleSlider_Scroll(object sender, EventArgs e)
        {
            if (ModifierKeys != Keys.Shift)
            {
                // Get the position slider's x-coordinate of the current position (= thumb location)
                Point location = SliderValue.ToPoint(shuttleSlider, shuttleSlider.Value);
                location.Y = 9; // move closer to thumb, with horizontal sliders ValueToPoint y = 0

                // Show the infolabel
                _sliderLabel.Show(" Shuttle " + (shuttleSlider.Value).ToString("+#;-#;0") + " ", shuttleSlider, location);
            }
        }

        private bool _isMinimized;
        // This has to do with clone windows (or any child window) and display overlays that 'CanFocus':
        // if the owner/parent of the clone windows is the main window, a focused display overlay (on the main window) might show in front of the clone windows
        // if the clone windows have no owner/parent, they stay visible (but without video picture) when the main window is minimized
        // so we're using clone windows without owner/parent but make them invisible (minimized) when the main window is minimized.
        // Yes, having editable items on a display overlay might not always be a good idea. :)
        private void MainWindow_Resize(object sender, EventArgs e)
        {
            if (myPlayer.Has.DisplayClones)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    if (!_isMinimized)
                    {
                        CloneWindows_SetVisibility(false);
                        _isMinimized = true;
                    }
                }
                else if (_isMinimized)
                {
                    CloneWindows_SetVisibility(true);
                    _isMinimized = false;
                }
            }
        }

        #region Drag Main Window

        // As some display overlays also use the player display mouse events, the drag window
        // events have to be disabled when these overlays are activated (see also 'Display Overlay Button')
        internal void SetWindowDrag(bool enable)
        {
            myPlayer.Display.DragEnabled = enable;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    // dispose slider preview and infolabels (if any) 
                    RemoveSliderPreview();
                    if (_positionLabel != null) { _positionLabel.Dispose(); _positionLabel = null; }
                    if (_sliderLabel != null) { _sliderLabel.Dispose(); _sliderLabel = null; }

                    // got problems with webcam windows and redraw of main window
                    myPlayer.Events.MediaPositionChanged -= MyPlayer_MediaPositionChanged;

                    // stop (if) playing and free overlay (if any)
                    myPlayer.Stop();
                    myPlayer.Overlay.Window = null;
                    if (_hasCloneWindows) CloneWindows_CloseAll();

                    // Clean up example display overlays (if used/created: 'lazy initialization')
                    if (_messageOverlay != null) { _messageOverlay.Dispose(); _messageOverlay = null; }
                    if (_scribbleOverlay != null) { _scribbleOverlay.Dispose(); _scribbleOverlay = null; }
                    if (_tileOverlay != null) { _tileOverlay.Dispose(); _tileOverlay = null; }
                    if (_bouncingOverlay != null) { _bouncingOverlay.Dispose(); _bouncingOverlay = null; }
                    if (_pipOverlay != null) { _pipOverlay.Dispose(); _pipOverlay = null; }
                    if (_subtitlesOverlay != null) { _subtitlesOverlay.Dispose(); _subtitlesOverlay = null; }
                    if (_zoomSelectOverlay != null) { _zoomSelectOverlay.Dispose(); _zoomSelectOverlay = null; }
                    if (_videoWallOverlay != null) { _videoWallOverlay.Dispose(); _videoWallOverlay = null; }
                    if (_mp3CoverOverlay != null) { _mp3CoverOverlay.Dispose(); _mp3CoverOverlay = null; }
                    if (_mp3KaraokeOverlay != null) { _mp3KaraokeOverlay.Dispose(); _mp3KaraokeOverlay = null; }
                    if (_bigTimeOverlay != null) { _bigTimeOverlay.Dispose(); _bigTimeOverlay = null; }
                    if (_statusInfoOverlay != null) { _statusInfoOverlay.Dispose(); _statusInfoOverlay = null; }

                    // stop automatic hiding of mouse cursor
                    myPlayer.CursorHide.RemoveAll();

                    // unsubscribe from flashing button timer
                    if (myPlayer.Paused) ButtonFlash.Remove(pauseButton);

                    // remove system time clock timer
                    if (_clockVisible)
                    {
                        _clockTimer.Dispose(); _clockTimer = null;
                    }

                    // remove on-screen output meter
                    if (_hasLevelMeterEvents)
                    {
                        myPlayer.Events.MediaPeakLevelChanged -= MyPlayer_MediaPeakLevelChanged;
                        _hasLevelMeterEvents = false;
                    }

                    // reset preventing computer going to sleep
                    // not really needed as player.dispose resets sleep mode
                    myPlayer.SleepDisabled = false;

                    // Clean up player
                    myPlayer.Dispose(); myPlayer = null;

                    leftLevelMeterPanel.Dispose(); // don't redraw
                    rightLevelMeterPanel.Dispose();
                    _levelMeterBrush.Dispose(); _levelMeterBrush = null;

                    // Clean up custom fonts
                    _crystalFont16.Dispose(); _crystalFont16 = null;
                    _wingDng38.Dispose(); _wingDng38 = null;
                    _clockFont25.Dispose(); _clockFont25 = null;
                    FontCollection.Dispose(); FontCollection = null;

                    // Clean up file dialogs
                    OpenFileDialog1.Dispose(); OpenFileDialog1 = null;
                    OpenFileDialog2.Dispose(); OpenFileDialog2 = null;
                    SaveFileDialog1.Dispose(); SaveFileDialog1 = null;

                    // Clear playlist
                    Playlist.Clear();

                    if (_addUrlDialog != null)          { _addUrlDialog.Dispose(); _addUrlDialog = null; }
                    if (_videoColorDialog != null)      { _videoColorDialog.Dispose(); _videoColorDialog = null; }
                    if (_channelMixerDialog != null)    { _channelMixerDialog.Dispose(); _channelMixerDialog = null; }
                    if (_aspectRatioDialog != null)     { _aspectRatioDialog.Dispose(); _aspectRatioDialog = null; }
                    if (_deviceVolumeDialog != null)    { _deviceVolumeDialog.Dispose(); _deviceVolumeDialog = null; }

                    if (_videoOverlayDialog != null)   { _videoOverlayDialog.Dispose(); _videoOverlayDialog = null; }
                    if (_videoOverlay != null)         { _videoOverlay.Dispose(); _videoOverlay = null; }

                    // clear temp files
                    try
                    {
                        string[] tempFiles = Directory.GetFiles(_appDataPath, "*.png");
                        foreach (string file in tempFiles) { File.Delete(file); }
                    }
                    catch { };

                    if (components != null) components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        
        // ******************************** Player EventHandling

        #region Player EventHandling

        // A mediafile has started playing
        void MyPlayer_MediaStarted(object sender, EventArgs e)
        {
            SetInterfaceOnMediaStart();
        }

        // A mediafile has finished or stopped playing
        private void MyPlayer_MediaEnded(object sender, EndedEventArgs e)
        {
            switch (e.StopReason)
            {
                case StopReason.Finished:
                    if (Prefs.AutoPlayNext)
                    {
                        _chapterMode = false;
                        SetInterfaceOnMediaStop(false);

                        if (RepeatAll || RepeatShuffle || _mediaToPlay < Playlist.Count) PlayNextMedia();
                        else SetInterfaceOnMediaStop(true);
                    }
                    else
                    {
                        SetInterfaceOnMediaStop(true);
                    }
                    break;

                case StopReason.AutoStop:
                    SetInterfaceOnMediaStop(false);
                    break;

                case StopReason.UserStop:
                    SetInterfaceOnMediaStop(true);
                    StopAndPlay = false;
                    break;

                case StopReason.Error:
                    SetInterfaceOnMediaStop(false);
                    ShowMediaEndedError(e);
                    if (Prefs.AutoPlayNext && Prefs.OnErrorPlayNext && (RepeatAll || RepeatShuffle || _mediaToPlay < Playlist.Count))
                    {
                        PlayNextMedia();
                    }
                    else SetInterfaceOnMediaStop(true);
                    break;
            }
        }

        private void ShowMediaEndedError(EndedEventArgs e)
        {
            ErrorDialog errorDialog;
            errorDialog = new ErrorDialog(APPLICATION_NAME, "PLAY MEDIA:\r\n\r\n" + e.MediaName + "\r\n\r\n" + myPlayer.GetErrorString(e.ErrorCode) + ".\r\n\r\nError code: 0x" + myPlayer.LastErrorCode.ToString("X"), false, true);
            CenterDialog(this, errorDialog);

            errorDialog.PlayNext = Prefs.OnErrorPlayNext;
            errorDialog.OnErrorRemove = Prefs.OnErrorRemove;

            if (Playlist.Count < 2) errorDialog.PlayNextVisible = false;

            //Player1.Display.Hold = false; // not necessary?
            myPlayer.Display.HoldClear();
            errorDialog.ShowDialog(this);

            Prefs.OnErrorPlayNext = errorDialog.PlayNext;
            Prefs.OnErrorRemove = errorDialog.OnErrorRemove;

            errorDialog.Dispose();
        }

        // The player's start/endposition (for the next media to play) has changed.
        // No longer a player event - now called directly.
        private void MyPlayer_MediaStartStopTimeNextChanged()
        {
            //startTimeNextTextBox.SuspendLayout();
            //stopTimeNextTextBox.SuspendLayout();

            //startTimeNextTextBox.Text = _startTimeNext.ToString().Substring(0, 8);
            //stopTimeNextTextBox.Text = _stopTimeNext.ToString().Substring(0, 8);

            if (_startTimeNext.TotalMilliseconds == 0)
            {
                if (_stopTimeNext.TotalMilliseconds == 0)
                {
                   // startTimeNextTextBox.ForeColor = UIColors.MenuTextEnabledColor;
                    //stopTimeNextTextBox.ForeColor = UIColors.MenuTextEnabledColor;
                }
                else
                {
                    //startTimeNextTextBox.ForeColor = Color.Green;
                    //stopTimeNextTextBox.ForeColor = Color.Firebrick;
                }
            }
            else
            {
               // startTimeNextTextBox.ForeColor = Color.Firebrick;
                //stopTimeNextTextBox.ForeColor = _stopTimeNext.TotalMilliseconds == 0 ? Color.Green : Color.Firebrick;
            }

           // stopTimeNextTextBox.ResumeLayout();
            //startTimeNextTextBox.ResumeLayout();
        }

		// The playing media's start/endposition has changed.
		private void MyPlayer_MediaStartStopTimeChanged(object sender, EventArgs e)
		{
			//startTimeTextBox.SuspendLayout();
			//stopTimeTextBox.SuspendLayout();

			//startTimeTextBox.Text = myPlayer.Media.StartTime.ToString().Substring(0, 8);
			//stopTimeTextBox.Text = myPlayer.Media.StopTime.ToString().Substring(0, 8);

			if (myPlayer.Media.StartTime.TotalMilliseconds == 0)
			{
				if (myPlayer.Media.StopTime.TotalMilliseconds == 0 || myPlayer.Media.StopTime == myPlayer.Media.GetDuration(MediaPart.BeginToEnd))
				{
					//startTimeTextBox.ForeColor = UIColors.MenuTextEnabledColor;
					//stopTimeTextBox.ForeColor = UIColors.MenuTextEnabledColor;
				}
				else
				{
					//startTimeTextBox.ForeColor = Color.Green;
					//stopTimeTextBox.ForeColor = Color.Firebrick;
				}
			}
			else
			{
				//startTimeTextBox.ForeColor = Color.Firebrick;
				//if (myPlayer.Media.StopTime.TotalMilliseconds == 0 || myPlayer.Media.StopTime == myPlayer.Media.GetDuration(MediaPart.BeginToEnd)) stopTimeTextBox.ForeColor = Color.Green;
				//else stopTimeTextBox.ForeColor = Color.Firebrick;

				//startTimeTextBox.BackColor = Color.FromArgb(64, 0, 0);
				//if (Player1.Media.StopTime.TotalMilliseconds == 0 || Player1.Media.StopTime == Player1.Media.GetLength(MediaLength.BeginToEnd)) stopTimeTextBox.BackColor = Color.FromArgb(0, 52, 0);
				//else stopTimeTextBox.ForeColor = Color.FromArgb(80, 0, 0);
			}

			//stopTimeTextBox.ResumeLayout();
			//startTimeTextBox.ResumeLayout();
		}

		// The playback position of a mediafile has changed - update position info labels
		// The position slider (TrackBar) is handled by the player
		private void MyPlayer_MediaPositionChanged(object sender, PositionEventArgs e)
        {
            if (_posTimerBusy) return;
            _posTimerBusy = true;

            if (myPlayer.Sliders.Position.Mode == PositionSliderMode.Track)
            {
                //positionLabel1.Text = TimeSpan.FromTicks(e.FromBegin).ToString().Substring(0, 8);
                //positionLabel2.Text = TimeSpan.FromTicks(e.ToEnd).ToString().Substring(0, 8);
                positionLabel1.Text = TimeSpan.FromTicks(e.FromBegin).ToString(@"hh\:mm\:ss");
                positionLabel2.Text = TimeSpan.FromTicks(e.ToEnd).ToString(@"hh\:mm\:ss");
            }
            else
            {
                //positionLabel1.Text = TimeSpan.FromTicks(e.FromStart).ToString().Substring(0, 8);
                //positionLabel2.Text = TimeSpan.FromTicks(e.ToStop).ToString().Substring(0, 8);
                positionLabel1.Text = TimeSpan.FromTicks(e.FromStart).ToString(@"hh\:mm\:ss");
                positionLabel2.Text = TimeSpan.FromTicks(e.ToStop).ToString(@"hh\:mm\:ss");
            }

            _posTimerBusy = false;
        }

        // The player's displaymode has changed (to 'Manual' with VideoMove or VideoZoom) - set the displaymode menu
        private void MyPlayer_MediaDisplayModeChanged(object sender, EventArgs e)
        {
            // false = no need to set the player's displaymode (again)
            SetDisplayModeMenu(myPlayer.Display.Mode, false);
        }

        // This handler is used for both the player's pause and resume events
        private void MyPlayer_MediaPausedResumed(object sender, EventArgs e)
        {
            if (myPlayer.Paused)
            {
                if (!_highSpeedMode)
                {
                    ButtonFlash.Add(pauseButton, Color.Black, _pauseColor);
                    if (myPlayer.Playing)
                    {
                        ButtonFlash.Add(playButtonLight, Color.Black, _pauseColor);
                        playButton.ForeColor = _pauseColor;
                        Icon = Properties.Resources.Media_Paused;
                    }
                    pauseMenuItem.Text = "Resume";
                    myPlayer.SleepDisabled = false;
                }
            }
            else
            {
                ButtonFlash.Remove(pauseButton);
                pauseButton.ForeColor = UIColors.MenuTextEnabledColor;
                if (myPlayer.Playing)
                {
                    ButtonFlash.Remove(playButtonLight);
                    playButtonLight.ForeColor = Color.Lime;
                    playButton.ForeColor = Color.Lime;
                    Icon = Properties.Resources.Media_Playing;

                    myPlayer.SleepDisabled = true;
                }
                pauseMenuItem.Text = "Pause";
            }
        }

        // The player's playback speed has changed - show the playback speed next to the speedslider
        private void MyPlayer_MediaSpeedChanged(object sender, EventArgs e)
        {
            speedTextBox.Text = myPlayer.Speed.Rate.ToString("0.00#");
            speedLight.LightOn = myPlayer.Speed.Rate != 1.0f;
        }

        // The player's audio volume has changed - show the value next to the audiovolumeslider - and now also: set the volume dial
        private void MyPlayer_MediaAudioVolumeChanged(object sender, EventArgs e)
        {
            if (myPlayer.Audio.Volume == 0)
            {
                if (!_volumeRedDial)
                {
                    _volumeRedDial = true;
                    volumeDial.SwitchImage(true);
                    balanceDial.SwitchImage(true);
                }
                volumeDialLabel.Text = "Mute";
                //volumeLight.LightOn = true;
            }
            else
            {
                if (_volumeRedDial && !myPlayer.Audio.Mute)
                {
                    _volumeRedDial = false;
                    volumeDial.SwitchImage(false);
                    balanceDial.SwitchImage(false);
                }
                volumeDialLabel.Text = myPlayer.Audio.Volume == 1.0 ? "Max" : (myPlayer.Audio.Volume).ToString("0.00");
                //volumeLight.LightOn = false;
            }

            if (!_dontSetAudioDials) volumeDial.SetValue((int)(myPlayer.Audio.Volume * 1000)); // does not raise changed event
        }

        // The player's audio balance has changed - show the value next to the audiobalanceslider - and now also: set the balance dial
        private void MyPlayer_MediaAudioBalanceChanged(object sender, EventArgs e)
        {
            float balance = myPlayer.Audio.Balance;
            if (balance == 0) balanceDialLabel.Text = "Center";
            else if (balance < 0)
            {
                if (balance == -1) balanceDialLabel.Text = "Full Left";
                else balanceDialLabel.Text = (-balance).ToString("Left -0.0");
            }
            else
            {
                if (balance == 1) balanceDialLabel.Text = "Full Right";
                else balanceDialLabel.Text = (balance).ToString("Right 0.0");
            }

            if (!_dontSetAudioDials) balanceDial.SetValue((int)((balance + 1) * 500)); // does not raise changed event
        }

        // Fullscreen / FullscreenMode settings has changed (from preferences settings)
        private void MyPlayer_MediaFullScreenSettingsChanged(object sender, EventArgs e)
        {
            SetFullScreenModeMenu();

            // hide position slider with fullscreen display and display shape
            BlockPositionSlider();
        }

        private void BlockPositionSlider()
        {
            // hide position slider with fullscreen display and non-standard display shape
            bool handled = false;

            if (myPlayer.Has.DisplayShape)
            {
                if (myPlayer.FullScreen && myPlayer.FullScreenMode == FullScreenMode.Display)
                {
                    if (!_sliderBlocked)
                    {
                        SliderPanelHide();
                        _sliderBlocked = true;
                    }
                    handled = true;
                }
            }

            if (!handled && _sliderBlocked)
            {
                if (!_sliderHidden) SliderPanelShow();
                _sliderBlocked = false;
            }
        }

        // On-screen output level meter
        private void MyPlayer_MediaPeakLevelChanged(object sender, PeakLevelEventArgs e)
        {
            if (_levelMeterBusy) return;
            _levelMeterBusy = true;

            if (e.MasterPeakValue == -1)
            {
                // straight to 0 (media paused, stopped or ended)
                _levelMeterLeft = _levelMeterRight = 0;
                _levelMeterHoldLeft = _levelMeterHoldRight = 0;
            }
            else
            {
                if (e.ChannelsValues[0] < _levelMeterHoldLeft) _levelMeterLeft = _levelMeterHoldLeft;
                else _levelMeterLeft = e.ChannelsValues[0];
                _levelMeterHoldLeft = _levelMeterLeft - LEVELMETER_SHORT_DELAY;

                if (e.ChannelsValues[1] < _levelMeterHoldRight) _levelMeterRight = _levelMeterHoldRight;
                else _levelMeterRight = e.ChannelsValues[1];
                _levelMeterHoldRight = _levelMeterRight - LEVELMETER_SHORT_DELAY;
            }

            leftLevelMeterPanel.Invalidate();
            rightLevelMeterPanel.Invalidate();

            _levelMeterBusy = false;
        }

        // Audio devices
        private void MyPlayer_MediaSystemAudioDevicesChanged(object sender, SystemAudioDevicesEventArgs e)
        {
            if (!e.IsInputDevice)
            {
                CreateAudioDeviceMenu();
            }
        }

        private void MyPlayer_MediaAudioDeviceChanged(object sender, EventArgs e)
        {
            _audioDeviceSelected = myPlayer.Audio.Device;
            CreateAudioDeviceMenu();
        }

        private void MyPlayer_MediaVideoAspectRatioChanged(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)displayModeMenu.Items[displayModeMenu.Items.Count - 2]).Checked = myPlayer.Video.AspectRatio != Size.Empty;
        }

        private void MyPlayer_MediaVideo3DViewChanged(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)displayModeMenu.Items[displayModeMenu.Items.Count - 1]).Checked = myPlayer.Video.View3D != Video3DView.NormalImage;
        }

        private void MyPlayer_MediaChapterStarted(object sender, ChapterStartedEventArgs e)
        {
            if (e.Index == -1)
            {
                Text = myPlayer.Media.GetName(MediaName.FileNameWithoutExtension);
                ClearChapters(_chapterMode);
                _chapterMode = false;
                SetChapters();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(e.Title) && e.Title[0] == '#') Text = myPlayer.Media.GetName(MediaName.FileNameWithoutExtension);
                else Text = myPlayer.Media.GetName(MediaName.FileNameWithoutExtension) + " - " + e.Title;
				if (_chapters_III != null)
				{
					try
					{
						for (int i = 0; i < _chapters_III.Length; i++)
						{
							((ToolStripMenuItem)(chaptersFileMenuItem.DropDown.Items[i])).Checked = false;
						}
					    ((ToolStripMenuItem)(chaptersFileMenuItem.DropDown.Items[e.Index])).Checked = true;
					}
					catch { /* ignored */ }
				}
            }
            if (_hasCloneWindows) CloneWindows_SetTitle(Text);
        }

        #endregion


        // ******************************** Set Interface On Media Start and Media End/Stop

        #region Set Interface On Media Start and Media End/Stop / Set Chapters

        // When a mediafile starts or ends/stops playing a few things in the interface may have to be changed
        // Called from MediaStart, MediaEnd and MediaStop eventhandlers

        internal void SetWindowTitle()
        {
            if (myPlayer.Playing)
            {
                // chapter title is set with ChapterStarted event - to prevent flicker:
                if (!_chapterMode) Text = myPlayer.Media.GetName(MediaName.FileNameWithoutExtension);
            }
            else
            {
                if (_tempPlaylist) Text = "Playlist sin titulo";
                else Text = "Playlist " + Prefs.PlayListTitle;
            }

            if (_hasCloneWindows) CloneWindows_SetTitle(Text);
        }

        private void SetInterfaceOnMediaStart()
        {
            // Set the (position) sliderContextMenu
            if (myPlayer.Media.Length.TotalMilliseconds > 0)
            {
                markStartPositionMenuItem.Enabled = true;
                markEndPositionMenuItem.Enabled = true;

                markPositionMenuItem.Enabled = true;
                mark1_MenuItem.Enabled = true;
                mark2_MenuItem.Enabled = true;
                mark3_MenuItem.Enabled = true;
                mark4_MenuItem.Enabled = true;

                goToStartMenuItem.Enabled = true;

                _startTimeNext = TimeSpan.Zero;
                _stopTimeNext = TimeSpan.Zero;
                MyPlayer_MediaStartStopTimeNextChanged();

                //startTimeTextBox.Enabled = true;
                //stopTimeTextBox.Enabled = true;
            }

            // Set window title
            SetWindowTitle();
            if (myPlayer.Paused) Icon = Properties.Resources.Media_Paused;
            else Icon = Properties.Resources.Media_Playing;

            // Set zoom and move buttons enabled/disabled
            //SetZoomPanelStatus(myPlayer.Video.Present);

            // Turn on the Play button light
            playButtonLight.LightOn = true;
            if (myPlayer.Paused)
            {
                ButtonFlash.Add(playButtonLight, Color.Black, _pauseColor);
                playButton.ForeColor = _pauseColor;
            }
            else
            {
                playButton.ForeColor = Color.Lime;
            }

            _errorCount = 0;

            // Set checkmark playlist
            ((ToolStripMenuItem)playMenu.Items[_mediaToPlay + START_PLAYITEMS - 1]).Checked = true;

            // set overlay hold by application
            if (_overlayHold)
            {
                if (!myPlayer.Overlay.Hold && myPlayer.Overlay.Window != null) overlayHoldMenuItem.Checked = myPlayer.Overlay.Hold = true;
            }

            // set video track menu items and label
            int trackCount = myPlayer.Media.VideoTrackCount;
            if (trackCount > 0)
            {
                videoTracksLabel.Text = trackCount.ToString();
                videoTracksLabel.Visible = true;

                videoTracksMenuItem.Enabled = true;
                videoTracksMenuItem.DropDown.Enabled = true;
                videoTracksMenuItem.DropDown.Items.Clear();

                VideoTrack[] tracks = myPlayer.Media.GetVideoTracks();
                for (int i = 0; i < trackCount; i++)
                {
                    if (tracks[i].Language != string.Empty)
                    {
                        try
                        {
                            CultureInfo cultureInfo = new CultureInfo(tracks[i].Language);
                            if (cultureInfo != CultureInfo.CurrentCulture)
                            {
                                videoTracksMenuItem.DropDown.Items.Add(tracks[i].Name + " [" + cultureInfo.NativeName + "] (" + cultureInfo.DisplayName + ")");
                            }
                            else
                            {
                                videoTracksMenuItem.DropDown.Items.Add(tracks[i].Name + " [" + cultureInfo.DisplayName + "]");
                            }
                        }
                        catch
                        {
                            videoTracksMenuItem.DropDown.Items.Add(tracks[i].Name + " [" + tracks[i].Language + "]");
                        }
                    }
                    else videoTracksMenuItem.DropDown.Items.Add(tracks[i].Name);
                }
                ((ToolStripMenuItem)(videoTracksMenuItem.DropDown.Items[0])).Checked = true;
                videoTracksMenuItem.DropDown.ItemClicked += VideoTracks_ItemClicked;

                //videoColorMenuItem.Enabled = true;
            }
            else
            {
                videoTracksLabel.Visible = false;
            }

            // set audio track menu items and label
            trackCount = myPlayer.Media.AudioTrackCount;
            if (trackCount > 0)
            {
                audioTracksLabel.Text = trackCount.ToString();
                audioTracksLabel.Visible = true;

                audioTracksMenuItem.Enabled = true;
                audioTracksMenuItem.DropDown.Enabled = true;
                audioTracksMenuItem.DropDown.Items.Clear();

                AudioTrack[] tracks = myPlayer.Media.GetAudioTracks();
                for (int i = 0; i < trackCount; i++)
                {
                    if (tracks[i].Language != string.Empty)
                    {
                        try
                        {
                            CultureInfo cultureInfo = new CultureInfo(tracks[i].Language);
                            if (cultureInfo != CultureInfo.CurrentCulture)
                            {
                                audioTracksMenuItem.DropDown.Items.Add(tracks[i].Name + " [" + cultureInfo.NativeName + "] (" + cultureInfo.DisplayName + ")");
                            }
                            else
                            {
                                audioTracksMenuItem.DropDown.Items.Add(tracks[i].Name + " [" + cultureInfo.DisplayName + "]");
                            }
                        }
                        catch
                        {
                            audioTracksMenuItem.DropDown.Items.Add(tracks[i].Name + " [" + tracks[i].Language + "]");
                        }
                    }
                    else audioTracksMenuItem.DropDown.Items.Add(tracks[i].Name);
                }
                ((ToolStripMenuItem)(audioTracksMenuItem.DropDown.Items[0])).Checked = true;
                audioTracksMenuItem.DropDown.ItemClicked += AudioTracks_ItemClicked;
            }
            else
            {
                audioTracksLabel.Visible = false;
            }

            channelCountLabel.Text = myPlayer.Audio.ChannelCount.ToString();
            channelCountLabel.Visible = myPlayer.Audio.ChannelCount > 0;

            // prevent computer going to sleep while playing a mediafile
            myPlayer.SleepDisabled = true;
            if (Prefs.ShowSliderPreview)
            {
                if (!sp_Created) CreateSliderPreview(myPlayer, this);
                StartSliderPreview(); // if enabled
            }

            myPlayer.Display.Hold = true;

            SetChapters();
        }

        private void SetChapters()
        {
            if (!_chapterMode)
            {
                // Set chapters (embedded) information
                string extension = myPlayer.Media.GetName(MediaName.Extension);
                if (string.Compare(extension, ".mp4", true) == 0 || extension.StartsWith(".m4", StringComparison.OrdinalIgnoreCase) || string.Compare(extension, ".mkv", true) == 0 || string.Compare(extension, ".webm", true) == 0)
                {
                    myPlayer.Chapters.FromMedia(out _chapters_I, out _chapters_II);
                    if (_chapters_I != null)
                    {
                        int index = 0;

                        chaptersAppleMenuItem.DropDown.Items.Clear(); // contains item "No Chapters"
                        if (_chapters_I[0].Language != null)
                        {
                            string language = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
                            for (int i = 0; i < _chapters_I[0].Language.Length; i++)
                            {
                                if (string.Compare(_chapters_I[0].Language[i], language, true) == 0)
                                {
                                    index = i;
                                    break;
                                }
                            }
                        }
                        int last = _chapters_I.Length - 1;
                        for (int i = 0; i < _chapters_I.Length; i++)
                        {
                            // just to be sure - maybe it's not necessary to check if every chapter has the same amount of localized titles
                            int trueIndex = _chapters_I[i].Title.Length > index ? index : 0;

                            //if (_chapters_I[i].EndTime != TimeSpan.Zero) chaptersAppleMenuItem.DropDown.Items.Add(_chapters_I[i].StartTime.ToString().Substring(0, 8) + " - " + _chapters_I[i].EndTime.ToString().Substring(0, 8) + "  " + _chapters_I[i].Title[trueIndex]);
                            //else chaptersAppleMenuItem.DropDown.Items.Add(_chapters_I[i].StartTime.ToString().Substring(0, 8) + "  " + _chapters_I[i].Title[trueIndex]);

                            if ((i == last) && _chapters_I[i].EndTime == TimeSpan.Zero) chaptersAppleMenuItem.DropDown.Items.Add(_chapters_I[i].StartTime.ToString().Substring(0, 8) + " - " + myPlayer.Media.Length.ToString().Substring(0, 8) + "  " + _chapters_I[i].Title[trueIndex]);
                            else chaptersAppleMenuItem.DropDown.Items.Add(_chapters_I[i].StartTime.ToString().Substring(0, 8) + " - " + _chapters_I[i].EndTime.ToString().Substring(0, 8) + "  " + _chapters_I[i].Title[trueIndex]);


                        }
                        chaptersAppleMenuItem.Enabled = true;
                        chaptersAppleMenuItem.Checked = true;
                        chaptersAppleMenuItem.DropDown.Enabled = true;
                        chaptersAppleMenuItem.DropDown.ItemClicked += AppleChaptersDropDown_ItemClicked;
                    }
                    if (_chapters_II != null)
                    {
                        chaptersNeroMenuItem.DropDown.Items.Clear(); // contains item "No Chapters"
                        int last = _chapters_II.Length - 1;
                        for (int i = 0; i < _chapters_II.Length; i++)
                        {
                            //if (_chapters_II[i].EndTime != TimeSpan.Zero) chaptersNeroMenuItem.DropDown.Items.Add(_chapters_II[i].StartTime.ToString().Substring(0, 8) + " - " + _chapters_II[i].EndTime.ToString().Substring(0, 8) + "  " + _chapters_II[i].Title[0]);
                            //else chaptersNeroMenuItem.DropDown.Items.Add(_chapters_II[i].StartTime.ToString().Substring(0, 8) + "  " + _chapters_II[i].Title[0]);

                            if ((i == last) && _chapters_II[i].EndTime == TimeSpan.Zero) chaptersNeroMenuItem.DropDown.Items.Add(_chapters_II[i].StartTime.ToString().Substring(0, 8) + " - " + myPlayer.Media.Length.ToString().Substring(0, 8) + "  " + _chapters_II[i].Title[0]);
                            else chaptersNeroMenuItem.DropDown.Items.Add(_chapters_II[i].StartTime.ToString().Substring(0, 8) + " - " + _chapters_II[i].EndTime.ToString().Substring(0, 8) + "  " + _chapters_II[i].Title[0]);
                        }
                        chaptersNeroMenuItem.Enabled = true;
                        chaptersNeroMenuItem.Checked = true;
                        chaptersNeroMenuItem.DropDown.Enabled = true;
                        chaptersNeroMenuItem.DropDown.ItemClicked += NeroChaptersDropDown_ItemClicked;
                    }
                }
            }

            // Set chapters (from file) information
            if (!_chapterMode)
            {
                ((ToolStripDropDownMenu)chaptersFileMenuItem.DropDown).ShowImageMargin = false;
                _chapters_III = myPlayer.Chapters.FromFile();
            }
            else ((ToolStripDropDownMenu)chaptersFileMenuItem.DropDown).ShowImageMargin = true;

            if (_chapters_III != null)
			{
				chaptersFileMenuItem.DropDown.Items.Clear(); // contains item "No Chapters"
				int last = _chapters_III.Length - 1;
				for (int i = 0; i < _chapters_III.Length; i++)
				{
					//if (_chapters_III[i].EndTime != TimeSpan.Zero) chaptersFileMenuItem.DropDown.Items.Add(_chapters_III[i].StartTime.ToString().Substring(0, 8) + " - " + _chapters_III[i].EndTime.ToString().Substring(0, 8) + "  " + _chapters_III[i].Title[0]);
					//else chaptersFileMenuItem.DropDown.Items.Add(_chapters_III[i].StartTime.ToString().Substring(0, 8) + "  " + _chapters_III[i].Title[0]);

					if ((i == last) && _chapters_III[i].EndTime == TimeSpan.Zero) chaptersFileMenuItem.DropDown.Items.Add(_chapters_III[i].StartTime.ToString().Substring(0, 8) + " - " + myPlayer.Media.Length.ToString().Substring(0, 8) + "  " + _chapters_III[i].Title[0]);
					else chaptersFileMenuItem.DropDown.Items.Add(_chapters_III[i].StartTime.ToString().Substring(0, 8) + " - " + _chapters_III[i].EndTime.ToString().Substring(0, 8) + "  " + _chapters_III[i].Title[0]);
				}
				chaptersFileMenuItem.Enabled = true;
				chaptersFileMenuItem.Checked = true;
				chaptersFileMenuItem.DropDown.Enabled = true;
				chaptersFileMenuItem.DropDown.ItemClicked += FileChaptersDropDown_ItemClicked;
			}
		}

        private void ClearChapters(bool with_III)
        {
            if (chaptersAppleMenuItem.Enabled)
            {
                chaptersAppleMenuItem.DropDown.ItemClicked -= AppleChaptersDropDown_ItemClicked;
                chaptersAppleMenuItem.Enabled = false;
                chaptersAppleMenuItem.Checked = false;
                chaptersAppleMenuItem.DropDown.Items.Clear();
                chaptersAppleMenuItem.DropDown.Items.Add("No Chapters");
                chaptersAppleMenuItem.DropDown.Enabled = false;
                _chapters_I = null;
            }
            if (chaptersNeroMenuItem.Enabled)
            {
                chaptersNeroMenuItem.DropDown.ItemClicked -= NeroChaptersDropDown_ItemClicked;
                chaptersNeroMenuItem.Enabled = false;
                chaptersNeroMenuItem.Checked = false;
                chaptersNeroMenuItem.DropDown.Items.Clear();
                chaptersNeroMenuItem.DropDown.Items.Add("No Chapters");
                chaptersNeroMenuItem.DropDown.Enabled = false;
                _chapters_II = null;
            }
            if (with_III && chaptersFileMenuItem.Enabled)
            {
                chaptersFileMenuItem.DropDown.ItemClicked -= FileChaptersDropDown_ItemClicked;
                chaptersFileMenuItem.Enabled = false;
                chaptersFileMenuItem.Checked = false;
                chaptersFileMenuItem.DropDown.Items.Clear();
                chaptersFileMenuItem.DropDown.Items.Add("No Chapters");
                chaptersFileMenuItem.DropDown.Enabled = false;
                _chapters_III = null;
            }
        }

		private void SetInterfaceOnMediaStop(bool setAll)
        {
            // Reset position slider marks
            _mark1 = TimeSpan.Zero;
            _mark2 = TimeSpan.Zero;
            _mark3 = TimeSpan.Zero;
            _mark4 = TimeSpan.Zero;

            // and these ((position) sliderContextMenu)
            markPositionMenuItem.Checked = false;
            mark1_MenuItem.Checked = false;
            mark2_MenuItem.Checked = false;
            mark3_MenuItem.Checked = false;
            mark4_MenuItem.Checked = false;

            goToMarkMenuItem.Checked = false;
            goToMarkMenuItem.Enabled = false;
            goToMark1_MenuItem.Text = "Ir a Marca #1"; goToMark1_MenuItem.Checked = false; goToMark1_MenuItem.Enabled = false;
            goToMark2_MenuItem.Text = "Ir a Marca #2"; goToMark2_MenuItem.Checked = false; goToMark2_MenuItem.Enabled = false;
            goToMark3_MenuItem.Text = "Ir a Marca #3"; goToMark3_MenuItem.Checked = false; goToMark3_MenuItem.Enabled = false;
            goToMark4_MenuItem.Text = "Ir a Marca #4"; goToMark4_MenuItem.Checked = false; goToMark4_MenuItem.Enabled = false;

            // Reset chapters menus
            ClearChapters(!_chapterMode);

            // Reset checkmark playlist
            UnCheckMenuItems(playMenu, START_PLAYITEMS, 0);

            // Reset video tracks menu
            videoTracksMenuItem.Enabled = false;
            videoTracksMenuItem.DropDown.Enabled = false;
            videoTracksMenuItem.DropDown.ItemClicked -= VideoTracks_ItemClicked;
            videoTracksMenuItem.DropDown.Close();
            videoTracksMenuItem.DropDown.Items.Clear();
            videoTracksMenuItem.DropDown.Items.Add("No Video Tracks"); // empty dropdown menu also removes dropdown menu arrow
            //videoColorMenuItem.Enabled = false;

            // Reset audio tracks menu
            audioTracksMenuItem.Enabled = false;
            audioTracksMenuItem.DropDown.Enabled = false;
            audioTracksMenuItem.DropDown.ItemClicked -= AudioTracks_ItemClicked;
            audioTracksMenuItem.DropDown.Close();
            audioTracksMenuItem.DropDown.Items.Clear();
            audioTracksMenuItem.DropDown.Items.Add("No Audio Tracks");

            StopSliderPreview();

            if (AspectRatioRemove)
            {
                if (Has3DView)
                {
                    video3DStereoMenuItem.PerformClick();
                }
                else
                {
                    if (HasAspectRatio || HasAspectRatioDVD)
                    {
                        HasAspectRatio = false;
                        HasAspectRatioDVD = false;
                        myPlayer.Video.AspectRatio = SizeF.Empty;
                    }
                }
            }

            if (HasAspectRatioDVD)
            {
                HasAspectRatioDVD = false;
                if (HasAspectRatio) myPlayer.Video.AspectRatio = AspectRatio;
                else myPlayer.Video.AspectRatio = SizeF.Empty;
            }

            if (setAll) // used to prevent 'flashing' interface elements - see PlayMedia
            {
                //ClearChapters(true);
                //_chapterMode = false;

                //Player1.Display.Hold = false;
                myPlayer.Display.HoldClear();
                if (_highSpeedMode)
                {
                    _highSpeedMode = false;
                    _ultraSpeedMode = false;
                    if (myPlayer.Speed.Rate == 1) speedLight.LightOn = false;
                    speedLight.ForeColor = Color.Red;
                    myPlayer.Position.StepEOFMargin = 200;
                    myPlayer.Speed.Rate = 1;
                }

                if (myPlayer.Overlay.Window != null)
                {
                    if (!_userOverlay) overlayOffMenuItem.PerformClick();
                    else ((IOverlay)myPlayer.Overlay.Window).MediaStopped();
                }

                // Turn off the Play button light and video and audio track lights
                playButton.ForeColor = UIColors.MenuTextEnabledColor;
                if (myPlayer.Paused)
                {
                    ButtonFlash.Remove(playButtonLight);
                    playButtonLight.ForeColor = Color.Lime;
                }
                playButtonLight.LightOn = false;

                videoTracksLabel.Visible = false;

                audioTracksLabel.Visible = false;
                channelCountLabel.Visible = false;

                _errorCount = 0; // reset error count

                // Set the (position) sliderContextMenu
                markStartPositionMenuItem.Enabled = false;
                markEndPositionMenuItem.Enabled = false;

                markPositionMenuItem.Enabled = false;
                mark1_MenuItem.Enabled = false;
                mark2_MenuItem.Enabled = false;
                mark3_MenuItem.Enabled = false;
                mark4_MenuItem.Enabled = false;

                goToStartMenuItem.Enabled = false;

                //startTimeTextBox.Enabled = false;
                //stopTimeTextBox.Enabled = false;

                //SetZoomPanelStatus(false);

                // Set window title
                SetWindowTitle();
                Icon = Properties.Resources.Media_Normal;

                // reset overlay hold by application
                if (_overlayHold)
                {
                    overlayHoldMenuItem.Checked = myPlayer.Overlay.Hold = false;
                }

                // reset preventing computer going to sleep (if no other players are using SleepDisabled)
                // PVS lib takes care of everything, no need to test if already off
                myPlayer.SleepDisabled = false;
            }
        }

       /* private void SetZoomPanelStatus(bool status)
        {
            zoomInButton.Enabled = status;
            zoomOutButton.Enabled = status;

            stretchUpButton.Enabled = status;
            stretchLeftButton.Enabled = status;
            stretchRightButton.Enabled = status;
            stretchDownButton.Enabled = status;
        }*/

        #endregion


        // ******************************** Player PlayMedia (includes error messagebox) / PlayNextMedia / PlayPreviousMedia

        #region Player PlayMedia / PlayNextMedia / PlayPreviousMedia

        // Play a mediafile
        private void PlayMedia(string fileName)
        {
            StopAndPlay = myPlayer.Playing;

            // check if it's a chapters file
            //if (!string.IsNullOrWhiteSpace(fileName) && Path.GetExtension(fileName) == ".chap")
            _chapterMode = false;
            if (Path.GetExtension(fileName) == ".chap")
			{
				_chapters_III = myPlayer.Chapters.FromFile(fileName);
                if (_chapters_III != null)
                {
                    string mediaName = Path.ChangeExtension(fileName, " (chapter media)");
                    fileName = myPlayer.Chapters.GetMediaFile(fileName);
                    if (fileName == null) fileName = mediaName;
					if (!_hasChapterStartedEvent)
                    {
                        myPlayer.Events.MediaChapterStarted += MyPlayer_MediaChapterStarted;
                        _hasChapterStartedEvent = true;
                    }
                    _chapterMode = true;
                }
				myPlayer.Play(fileName, _chapters_III); // results in an error if chapters is null
			}
			else if (_startTimeNext != TimeSpan.Zero || _stopTimeNext != TimeSpan.Zero)
            {
                myPlayer.Play(fileName, _startTimeNext, _stopTimeNext);
            }
            else
            {
                myPlayer.Play(fileName);
            }

            if (myPlayer.LastError)
            {
                _chapterMode = false;
                Prefs.OnErrorPlayNext = Prefs.AutoPlayNext;

                if (Prefs.ShowErrorMessages)
                {
                    // TODO replace by ShowMediaEndedError() (above)

                    ErrorDialog errorDialog;
                    errorDialog = new ErrorDialog(APPLICATION_NAME, "PLAY MEDIA:\r\n\r\n" + fileName + "\r\n\r\n" + myPlayer.LastErrorString + ".\r\n\r\nError code: 0x" + myPlayer.LastErrorCode.ToString("X"), false, true);
                    CenterDialog(this, errorDialog);

                    errorDialog.PlayNext = Prefs.OnErrorPlayNext;
                    errorDialog.OnErrorRemove = Prefs.OnErrorRemove;

                    if (Playlist.Count < 2) errorDialog.PlayNextVisible = false;

                    if (myPlayer.Overlay.Window != null)
                    {
                        if (!_userOverlay) overlayOffMenuItem.PerformClick();
                        else
                        {
                            if (_overlayHold)
                            {
                                _overlayHold = false;
                                myPlayer.Overlay.Hold = false;
                            }
                            ((IOverlay)myPlayer.Overlay.Window).MediaStopped();
                        }
                    }
                    myPlayer.Display.HoldClear();
                    errorDialog.ShowDialog(this);

                    Prefs.OnErrorPlayNext = errorDialog.PlayNext;
                    Prefs.OnErrorRemove = errorDialog.OnErrorRemove;

                    errorDialog.Dispose();
                }

                if (_startTimeNext != TimeSpan.Zero || _stopTimeNext != TimeSpan.Zero)
                {
                    _startTimeNext = TimeSpan.Zero;
                    _stopTimeNext = TimeSpan.Zero;
                    MyPlayer_MediaStartStopTimeNextChanged();
                }

                // Remove error mediafile from playlist
                if (Prefs.OnErrorRemove)
                {
                    Playlist.RemoveAt(--_mediaToPlay);
                    ReBuildPlayListMenu();
                    SavePlayList();
                    if (RepeatShuffle)
                    {
                        CreateShuffleList();
                        SetShuffleList();
                    }
                }

                // Continue playing next mediafile
                if (Prefs.OnErrorPlayNext && Playlist.Count > 1 && ++_errorCount < 2)
                {
                    if (RepeatAll || RepeatShuffle || _mediaToPlay < Playlist.Count) PlayNextMedia();
                    else SetInterfaceOnMediaStop(true);
                }
                else SetInterfaceOnMediaStop(true);
            }
            else
            {
                _errorCount = 0;

                if (myPlayer.Has.Video)
                {
                    // Handle video aspect ratio
                    if (!Has3DView)
                    {
                        if (AspectRatioDVD && ((myPlayer.Video.SourceSize.Width == 704 || myPlayer.Video.SourceSize.Width == 720) && myPlayer.Video.SourceSize.Height == 576))
                        {
                            myPlayer.Video.Widescreen = true;
                            HasAspectRatioDVD = true;
                        }
                        else if (HasAspectRatio)
                        {
                            myPlayer.Video.AspectRatio = AspectRatio;
                        }
                    }
                }

                // automatic detection of overlay (only if user has not selected an overlay)
                if (Prefs.AutoOverlay && !_userOverlay)
                {
                    bool overlaySet = false;
                    if (myPlayer.Video.Present)
                    {
                        // Subtitles Overlay
                        if (myPlayer.Subtitles.Exists)
                        {
                            // test overlay == null because of 'lazy initialization' of overlays
                            if (_subtitlesOverlay == null || myPlayer.Overlay.Window != _subtitlesOverlay) subtitlesMenuItem.PerformClick();
                            overlaySet = true;
                        }
                    }
                    else
                    {
                        overlaySet = true;

                        // MP3Cover or MP3Karaoke Overlay
                        //if (string.Equals(Player1.GetMediaName(MediaName.Extension), ".mp3", StringComparison.OrdinalIgnoreCase)
                        if (myPlayer.Media.GetName(MediaName.FileName).EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                            && (!string.IsNullOrEmpty(SearchFile.Find(myPlayer.Media.GetName(MediaName.FileNameWithoutExtension) + ".cdg", myPlayer.Media.GetName(MediaName.DirectoryName), SEARCH_DEPTH))))
                        {
                            if (_mp3KaraokeOverlay == null || myPlayer.Overlay.Window != _mp3KaraokeOverlay)
                            {
                                MP3KaraokeMenuItem.PerformClick();
                            }
                        }
                        else if (_mp3CoverOverlay == null || myPlayer.Overlay.Window != _mp3CoverOverlay)
                        {
                            try
                            {
                                MP3CoverMenuItem.PerformClick();
                            }
                            catch { overlaySet = false; }
                        }
                    }
                    if (!overlaySet) overlayOffMenuItem.PerformClick();
                    else _userOverlay = false;
                }

                if (_highSpeedMode)
                {
                    if (myPlayer.Has.Video)
                    {
                        myPlayer.Speed.Rate = 1;
                        if (_ultraSpeedMode) UltraSpeedPlayback();
                        else HighSpeedPlayback();
                    }
                    else
                    {
                        if (myPlayer.Speed.Rate == 1) speedLight.LightOn = false;
                        speedLight.ForeColor = Color.Red;
                        myPlayer.Position.StepEOFMargin = 200;
                        myPlayer.Speed.Rate = 8;
                    }
                }
            }
        }

		// Play next mediafile and update the 'next' counter
		private void PlayNextMedia()
        {
            if (Playlist.Count > 0)
            {
                if (RepeatShuffle)
                {
                    if (_shuffleToPlay >= _shuffleList.Length)
                    {
                        // create new shufflelist and prevent new first = old last
                        int i = _shuffleList[_shuffleList.Length - 1];
                        CreateShuffleList();
                        if (_shuffleList[0] == i)
                        {
                            _shuffleList[0] = _shuffleList[_shuffleList.Length - 1];
                            _shuffleList[_shuffleList.Length - 1] = i;
                        }
                    }
                    _mediaToPlay = _shuffleList[_shuffleToPlay++];
                }
                else
                {
                    if (_mediaToPlay >= Playlist.Count) _mediaToPlay = 0;
                }
                PlayMedia(Playlist[_mediaToPlay++]);
            }
        }

        // Play previous mediafile and update the 'next' counter
        private void PlayPreviousMedia()
        {
            if (myPlayer.Playing && Playlist.Count > 0)
            {
                if (RepeatShuffle)
                {
                    _shuffleToPlay -= 2;
                    if (_shuffleToPlay < 0) _shuffleToPlay = _shuffleList.Length - 1;
                    _mediaToPlay = _shuffleList[_shuffleToPlay++];
                }
                else
                {
                    _mediaToPlay -= 2;
                    if (_mediaToPlay < 0) _mediaToPlay = Playlist.Count - 1;
                }
                PlayMedia(Playlist[_mediaToPlay++]);
            }
        }

        #endregion


        // ******************************** User Interface Handling

        // About the menus:
        // All (dropdown/pop-up) menus are handled by a usercontrol (dropdownbutton) or the standard windows
        // contextmenu handler (right mousebutton), so all there is to do is to handle the menu items selected,
        // except for the playmenu, which has an additional pop-up menu and drag & drop.


        // ******************************** Name and WebSite Label Click (Show About message / Open WebSite)

        #region Name and WebSite Label Click (Show About message / Open WebSite)

        // Clicking on the nameLabel shows the About message
        private void NameLabel_Click(object sender, EventArgs e)
        {
            ShowAbout();
        }

        // Clicking on the webSiteLabel asks for opening a website
        private void WebSiteLabel_Click(object sender, EventArgs e)
        {
            string theWebPage = @"https://www.udbvirtual.edu.sv";

            WebSiteDialog webSiteDialog = new WebSiteDialog(this) { Selection = _goToArticle };
            CenterDialog(this, webSiteDialog);
            if (webSiteDialog.ShowDialog(this) == DialogResult.OK)
            {
                _goToArticle = webSiteDialog.Selection;
                webSiteDialog.Dispose();

                if (_goToArticle == 1) theWebPage += @"/Articles/109714/PVS-MediaPlayer-Audio-and-Video-Player-Library";
                else if (_goToArticle == 2) theWebPage += @"/Articles/1116698/PVS-AVPlayer-MCI-Sound-Recorder";
                try
                {
                    Process.Start(theWebPage);
                }
                catch
                {
                    MessageBox.Show(
                    caption: APPLICATION_NAME,
                    icon: MessageBoxIcon.Exclamation,
                    text: "No se puedo abrir la página web. Favor revisar la direcci´pn",
                    buttons: MessageBoxButtons.OK,
                    owner: this);
                }
            }
            else
            {
                webSiteDialog.Dispose();
            }
        }

        #endregion

        
        // ******************************** Play Button / PlayMenu Drag and Drop / Pause Button / Stop Button

        #region Play Button / Play Menu

        private void PlayMenu_Opening(object sender, CancelEventArgs e)
        {
            myPlayer.CursorHide.Enabled = false;

            // Rebuild the webcam menu (if not playing a webcam)
            WebcamDevice[] webcams = myPlayer.Webcam.GetDevices();
            if (webcams != null || _webcams != null)
            {
                if (webcams != null && _webcams != null && webcams.Length == _webcams.Length)
                {
                    // menu already exists
                }
                else
                {
                    webcamsMenuItem.DropDown.Items.Clear();
                    if (_webcams != null)
                    {
                        webcamsMenuItem.Enabled = false;
                        _webcams = null;
                        webcamsMenuItem.DropDown.ItemClicked -= WebcamMenu_ItemClicked;
                    }

                    if (webcams != null)
                    {
                        _webcams = webcams;
                        webcamsMenuItem.Enabled = true;
                        int i = 0;
                        for (; i < _webcams.Length; i++)
                        {
                            webcamsMenuItem.DropDown.Items.Add(_webcams[i].Name);
                            webcamsMenuItem.DropDown.Items[i].Enabled = true;
                        }

                        if (_webcams.Length > 1)
                        {
                            webcamsMenuItem.DropDown.Items.Add("-"); // separator
                            webcamsMenuItem.DropDown.Items[i++].Enabled = false;
                            webcamsMenuItem.DropDown.Items.Add("Reproducri todas las Webcams");
                            ((ToolStripMenuItem)(webcamsMenuItem.DropDown.Items[i])).ShortcutKeys = Keys.Control | Keys.W;
                            webcamsMenuItem.DropDown.Items[i].Enabled = true;
                        }

                        webcamsMenuItem.DropDown.ItemClicked += WebcamMenu_ItemClicked;
                    }
                    else
                    {
                        webcamsMenuItem.DropDown.Items.Add("No Webcams");
                        webcamsMenuItem.DropDown.Items[0].Enabled = false;
                    }
                }
            }
        }

        private void PlayMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            myPlayer.CursorHide.Enabled = true;
        }

        // When the Playbutton is clicked a Play contextmenu will be shown
        // The Play contextmenu has a pop-up submenu that is activated (shown) when the right mousebutton is clicked
        // The Play contextmenu is also used as a dropdown menu with the player's display contextmenu

        // Check which mouse button is clicked on the Play contextmenu
        private void PlayMenu_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _playMenuRightButton = true;
                _playMenuPopUpLocation.X = e.Location.X - 1;
                _playMenuPopUpLocation.Y = e.Location.Y - 1;
            }
        }

        // An item has been selected on the Play contextmenu
        private void PlayMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            _playMenuItemIndex = playMenu.Items.IndexOf(e.ClickedItem);
            if (_playMenuItemIndex < 3 || _playMenuItemIndex == START_PLAYITEMS - 1) return; // ignore playlist and separator lines

            // Submenu Playlist has it's own menuhandler
            if (_playMenuItemIndex == 3) // This is menu item 'Add Media Files'
            {
                playMenu.Close();
                if (displayMenu.Visible) displayMenu.Close();
                SelectMediaFiles();
            }
            else if (_playMenuItemIndex == 4) // This is menu item 'Add URLs'
            {
                playMenu.Close();
                if (displayMenu.Visible) displayMenu.Close();
                ShowAddUrlDialog();
            }
            else
            {
                // If right mousebutton pressed
                if (_playMenuRightButton)
                {
                    _playMenuRightButton = false;
                    playMenu.AutoClose = false;
                    if (displayMenu.Visible) displayMenu.AutoClose = false;
                    playMenu.Items[_playMenuItemIndex].BackColor = Color.Maroon; // TODO - use global color def.

                    bool notAnUrl = true;
                    for (int i = 0; i < STREAMING_URLS.Length; i++)
                    {
                        if (Playlist[_playMenuItemIndex - START_PLAYITEMS].StartsWith(STREAMING_URLS[i], StringComparison.OrdinalIgnoreCase))
                        {
                            openLocationMenuItem.Enabled = propertiesMenuItem.Enabled = false;
                            notAnUrl = false;
                            break;
                        }
                    }
                    if (notAnUrl) openLocationMenuItem.Enabled = propertiesMenuItem.Enabled = true;

                    // Selected item gets passed on with (global) playMenuItemIndex:
                    playSubMenu.Show(playMenu.PointToScreen(_playMenuPopUpLocation));
                }
                else
                {
                    playMenu.Close();
                    if (displayMenu.Visible) displayMenu.Close();
                    // Play the selected mediafile
                    _mediaToPlay = _playMenuItemIndex - START_PLAYITEMS;
                    if (RepeatShuffle) SetShuffleList();
                    PlayNextMedia();
                }
            }
        }

        // The Play contextmenu submenu handler
        private void PlaySubMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            bool localFile = true;

            playSubMenu.Close();
            switch (playSubMenu.Items.IndexOf(e.ClickedItem))
            {
                case 0: // Play the mediafile
                    playMenu.Close();
                    if (displayMenu.Visible) displayMenu.Close();
                    _mediaToPlay = _playMenuItemIndex - START_PLAYITEMS;
                    if (RepeatShuffle) SetShuffleList();
                    PlayNextMedia();
                    break;

                case 2: // Open file location
                    playMenu.Close();
                    if (displayMenu.Visible) displayMenu.Close();

                    for (int i = 0; i < STREAMING_URLS.Length; i++)
                    {
                        if (Playlist[_playMenuItemIndex - START_PLAYITEMS].StartsWith(STREAMING_URLS[i], StringComparison.OrdinalIgnoreCase))
                        {
                            localFile = false;
                            break;
                        }
                    }
                    if (localFile)
                    {
                        try
                        {
                            Process.Start("explorer.exe", "/select,\"" + Playlist[_playMenuItemIndex - START_PLAYITEMS] + "\"");
                        }
                        catch { }
                    }
                    break;

                case 3: // Properties
                    playMenu.Close();
                    bool menuOn = displayMenu.Visible;
                    if (menuOn) displayMenu.Close();

                    for (int i = 0; i < STREAMING_URLS.Length; i++)
                    {
                        if (Playlist[_playMenuItemIndex - START_PLAYITEMS].StartsWith(STREAMING_URLS[i], StringComparison.OrdinalIgnoreCase))
                        {
                            localFile = false;
                            break;
                        }
                    }
                    if (localFile)
                    {
                        if (File.Exists(Playlist[_playMenuItemIndex - START_PLAYITEMS]))
                        {
                            Cursor.Position = menuOn ? displayMenu.PointToScreen(Point.Empty) : playPanel.PointToScreen(Point.Empty);
                            try
                            {
                                SafeNativeMethods.SHELLEXECUTEINFO info = new SafeNativeMethods.SHELLEXECUTEINFO();
                                info.cbSize = Marshal.SizeOf(info);
                                info.lpVerb = "Propiedades";
                                info.lpParameters = "Detalles";
                                info.lpFile = Playlist[_playMenuItemIndex - START_PLAYITEMS];
                                info.nShow = SafeNativeMethods.SW_SHOW;
                                info.fMask = SafeNativeMethods.SEE_MASK_INVOKEIDLIST;
                                SafeNativeMethods.ShellExecuteEx(ref info);
                            }
                            catch { }
                        }
                        else // file not found
                        {
                            // This should not happen but shows how to
                            // get localized error text (Win32Exception(errorCode).Message)

                            StringBuilder infoText = new StringBuilder(450);
                            infoText.AppendLine("Propiedades\r\n")
                            .AppendLine(Playlist[_playMenuItemIndex - START_PLAYITEMS] + "\r\n")
                            .Append(new Win32Exception(2).Message) // 2 = Win32 file not found
                            .Append('.');

                            ErrorDialog errorDialog = new ErrorDialog(APPLICATION_NAME, infoText.ToString(), false, true);

                            errorDialog.checkBox1.Hide();
                            errorDialog.checkBox2.Hide();

                            CenterDialog(this, errorDialog);
                            errorDialog.ShowDialog();

                            errorDialog.Dispose();
                        }
                    }
                    break;

                case 5: // Remove from List
                    if (((ToolStripMenuItem)playMenu.Items[_playMenuItemIndex]).Checked)
                    {
                        myPlayer.Stop();
                        Application.DoEvents();
                    }
                    Playlist.RemoveAt(_playMenuItemIndex - START_PLAYITEMS);
                    if (RepeatShuffle) CreateShuffleList();
                    if (myPlayer.Playing)
                    {
                        if (_mediaToPlay > (_playMenuItemIndex - START_PLAYITEMS)) --_mediaToPlay;
                        if (RepeatShuffle) SetShuffleList();
                    }
                    ReBuildPlayListMenu();
                    SavePlayList();
                    Prefs.PlayListChanged = true;
                    break;

                case 7: // Sort List
                    Playlist.Sort(CompareFileNames);
                    if (myPlayer.Playing) _mediaToPlay = GetPlayListIndex() + 1; // Adjust menu checkmark and playing item
                    ReBuildPlayListMenu();
                    SavePlayList();
                    Prefs.PlayListChanged = true;
                    break;

                case 8: // Copy List As Text
                    playMenu.Close();
                    int itemCount = playMenu.Items.Count;
                    if (itemCount > START_PLAYITEMS)
                    {
                        StringBuilder copyData = new StringBuilder(itemCount * 40);
                        copyData.AppendLine("Playlist " + Prefs.PlayListTitle + Environment.NewLine);
                        for (int i = START_PLAYITEMS; i < itemCount; i++)
                        {
                            copyData.AppendLine(playMenu.Items[i].Text.Replace("&&", "&"));
                        }
                        Clipboard.SetText(copyData.ToString());
                        copyData = null;
                    }
                    break;
            }
        }

        private int GetPlayListIndex()
        {
            if (myPlayer.Playing)
            {
                string target = myPlayer.Media.GetName(MediaName.FullPath);
                for (int i = Playlist.Count - 1; i >= 0; --i)
                {
                    if (Playlist[i] == target) return i;
                }
            }
            return -1;
        }

        // Sort PlayList - Compare filenames (not the full path)
        private int CompareFileNames(string x, string y)
        {
            //return String.Compare(Path.GetFileName(x), Path.GetFileName(y), StringComparison.OrdinalIgnoreCase);
            return SafeNativeMethods.StrCmpLogicalW(Path.GetFileName(x), Path.GetFileName(y));
        }

        // Called when the pop-up submenu of the Play contextmenu is closed
        private void PlaySubMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (displayMenu.Visible)
            {
                displayMenu.AutoClose = true;
                displayMenu.Show(); // this seems to have to be done to restore autoclose
            }
            playMenu.AutoClose = true;
            playMenu.Show(); // this seems to have to be done to restore autoclose

            playMenu.Items[_playMenuItemIndex].BackColor = playMenu.Items[0].BackColor;
        }

        // Close the submenu when the mouse leaves the menu
        private void PlaySubMenu_MouseLeave(object sender, EventArgs e)
        {
            //if (!menuCloseOnLeave) return; // maybe better not with this one
            if (playSubMenu.Visible) playSubMenu.Close();
        }

        #endregion

        #region PlayList Menu

        private void NewPlayListMenuItem_Click(object sender, EventArgs e)
        {
            NewPlayList();
        }

        private void OpenPlayListMenuItem_Click(object sender, EventArgs e)
        {
            OpenPlayList();
        }

        private void AddPlayListMenuItem_Click(object sender, EventArgs e)
        {
            AddPlayList();
        }

        private void SavePlayListMenuItem_Click(object sender, EventArgs e)
        {
            SavePlayListByUser();
        }

        #endregion

        #region Webcam Menu

        // See also PlayMenu_Opening (Play Button / Play Menu)
        private void WebcamMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int item = webcamsMenuItem.DropDown.Items.IndexOf(e.ClickedItem);

            playMenu.Close();
            if (displayMenu.Visible) displayMenu.Close();

            if (_webcams != null)
            {
                if (item < _webcams.Length)
                {
                    WebcamMenu_PlayWebcam(item);
                }
                else
                {
                    for (int index = 0; index < _webcams.Length; index++)
                    {
                        WebcamMenu_PlayWebcam(index);
                    }
                }
            }
        }

        private void WebcamMenu_PlayWebcam(int index)
        {
            // check if already playing
            WebcamDevice webcam = _webcams[index];

            FormCollection forms = Application.OpenForms;
            int count = forms.Count;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                if (string.Compare(forms[i].Text, webcam.Name) == 0)
                {
                    count = i;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                if (forms[count].WindowState == FormWindowState.Minimized) forms[count].WindowState = FormWindowState.Normal;
                else forms[count].BringToFront();
            }
            else
            {
                Webcam_Window webcamWindow = new Webcam_Window(this, webcam);
                webcamWindow.Show();
            }
        }

        #endregion

        #region PlayMenu Drag and Drop

        // The ContextMenuStrip (playMenu) does not raise a mousedown event (?)

        private void PlayMenu_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _ddMouseLocation = e.Location;
                _ddDragMenuItem = (ToolStripMenuItem)sender;
                _ddDragMenuItem.MouseMove += PlayMenu_MouseMove;
                _ddDragMenuItem.MouseUp += PlayMenu_MouseUp;
                _ddSourceIndex = playMenu.Items.IndexOf((ToolStripMenuItem)sender);
                _ddMouseDown = true;
            }
        }

        private void PlayMenu_MouseMove(object sender, MouseEventArgs e)
        {
            if (_ddMouseDown && (Math.Abs(_ddMouseLocation.X - e.Location.X) > 2 || Math.Abs(_ddMouseLocation.Y - e.Location.Y) > 2))
            {
                _ddMouseDown = false; // we don't get a mouse up event after dodragdrop

                _ddDragMenuItem.MouseMove -= PlayMenu_MouseMove;
                _ddDragMenuItem.MouseUp -= PlayMenu_MouseUp;

                _ddOurDrag = true;
                playMenu.DoDragDrop(Playlist[_ddSourceIndex - START_PLAYITEMS], DragDropEffects.Move);
                _ddOurDrag = false;
            }
        }

        private void PlayMenu_MouseUp(object sender, MouseEventArgs e)
        {
            _ddDragMenuItem.MouseMove -= PlayMenu_MouseMove;
            _ddDragMenuItem.MouseUp -= PlayMenu_MouseUp;
            _ddMouseDown = false;
        }

        // DragEnter not needed - responding only to 'our drag'
        private void PlayMenu_DragOver(object sender, DragEventArgs e)
        {
            if (_ddOurDrag)
            {
                Point location = playMenu.PointToClient(new Point(e.X, e.Y));
                e.Effect = playMenu.Items.IndexOf(playMenu.GetItemAt(location)) >= START_PLAYITEMS ? DragDropEffects.Move : DragDropEffects.None;
            }
            else e.Effect = DragDropEffects.None;
        }

        private void PlayMenu_DragDrop(object sender, DragEventArgs e)
        {
            if (_ddOurDrag)
            {
                Point location = playMenu.PointToClient(new Point(e.X, e.Y));
                int ddDestIndex = playMenu.Items.IndexOf(playMenu.GetItemAt(location));
                if (ddDestIndex != _ddSourceIndex && ddDestIndex >= START_PLAYITEMS)
                {
                    ToolStripMenuItem menuItem = (ToolStripMenuItem)playMenu.Items[_ddSourceIndex];
                    playMenu.Items.RemoveAt(_ddSourceIndex);
                    playMenu.Items.Insert(ddDestIndex, menuItem);

                    string listItem = Playlist[_ddSourceIndex - START_PLAYITEMS];
                    Playlist.RemoveAt(_ddSourceIndex - START_PLAYITEMS);
                    Playlist.Insert(ddDestIndex - START_PLAYITEMS, listItem);

                    // Adjust next to play if dragging playing medianame
                    if (menuItem.Checked) _mediaToPlay = ddDestIndex - START_PLAYITEMS + 1;

                    SavePlayList();
                    Prefs.PlayListChanged = true;
                }
            }
        }

        #endregion

        #region Pause Button / Previous Button / Next Button / Stop Button

        // Pause and Resume
        private void PauseButton_Click(object sender, EventArgs e)
        {
            myPlayer.Paused = !myPlayer.Paused;
            // The interface changes are handled with the player's MediaPause/Resume eventhandler
        }

        // Previous (Backward)
        private void PreviousButton_Click(object sender, EventArgs e)
        {
			if (myPlayer.Has.Chapters && ((ModifierKeys & Keys.Shift) != Keys.Shift) && (myPlayer.Chapters.Index > 0 || myPlayer.Repeat))
			{
				myPlayer.Chapters.Index--;
                if (myPlayer.LastError) PlayPreviousMedia();
            }
			else PlayPreviousMedia();
        }

        // Next (Forward)
        private void NextButton_Click(object sender, EventArgs e)
        {
            if (!myPlayer.Playing && Playlist.Count > 0)
            {
                _mediaToPlay = 0;
                if (RepeatShuffle) SetShuffleList();
                PlayNextMedia();
            }
            else
            {
                if (myPlayer.Has.Chapters && ((ModifierKeys & Keys.Shift) != Keys.Shift))
                {
                    myPlayer.Chapters.Index++;
                    if (myPlayer.LastError) PlayNextMedia();
                }
                else PlayNextMedia();
            }
        }

        // Stop
        private void StopButton_Click(object sender, EventArgs e)
        {
            myPlayer.Stop();
            // The interface changes are handled with the player's MediaStop eventhandler
        }

        #endregion


        // ******************************** Display Label Click / DisplayMode Button Menu / Aspect Ratio dialog / Video 3D View Menu

        #region Display Label Click (Show Control Panel)

        // Display Label Click - Opens System Display Control Panel
        private void DisplayModeLabel_Click(object sender, EventArgs e)
        {
            myPlayer.SystemPanels.ShowDisplaySettings(this);
        }

        #endregion

        #region DisplayMode Button Menu

        private void DisplayModeMenu_Opening(object sender, CancelEventArgs e)
        {
            myPlayer.CursorHide.Enabled = false;
        }

        private void DisplayModeMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            myPlayer.CursorHide.Enabled = true;
        }

        private void DisplayModeMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.Text.StartsWith("Aspect"))
                {
                    ShowAspectRatioDialog();
                }
                else
                {
                    // Translate the text of the clicked item into a displaymode enum value
                    SetDisplayModeMenu((DisplayMode)Enum.Parse(typeof(DisplayMode), e.ClickedItem.Text), true);
                }
            }
            catch { }
        }

        private void SetDisplayModeMenu(DisplayMode displayMode, bool setMode)
        {
            string displayModeName = displayMode.ToString();
            if (displayModeButton.Text != displayModeName)
            {
                if (setMode) myPlayer.Display.Mode = displayMode;

                if (displayMode == DisplayMode.ZoomCenter)
                {
                    displayModeLight.LightOn = false;
                }
                else
                {
                    if (displayMode == DisplayMode.Manual) displayModeLight.ForeColor = Color.Red;
                    else displayModeLight.ForeColor = Color.Lime; //  Color.Gold;
                    displayModeLight.LightOn = true;
                }

                displayModeButton.Text = displayModeName;
                int count = displayModeMenu.Items.Count - 3;
                for (int i = 0; i < count; i++)
                {
                    //if (displayModeMenu.Items[i] != null && displayModeMenu.Items[i].GetType() == typeof(ToolStripMenuItem))
                    {
                        ((ToolStripMenuItem)displayModeMenu.Items[i]).Checked = displayModeMenu.Items[i].Text == displayModeName;
                    }
                }
            }
        }

        #endregion

        #region Aspect Ratio Dialog

        private void ShowAspectRatioDialog()
        {
            if (_aspectRatioDialog == null)
            {
                _aspectRatioDialog = new AspectRatioDialog(this, myPlayer);
				_aspectRatioDialog.FormClosed += AspectRatioDialog_FormClosed;
                CenterDialog(this, _aspectRatioDialog);
                _aspectRatioDialog.Show(this);
            }
            else
            {
                _aspectRatioDialog.Activate();
            }
        }

		private void AspectRatioDialog_FormClosed(object sender, FormClosedEventArgs e)
		{
            _aspectRatioDialog = null;

        }

        #endregion

        #region Video 3D View Menu

        private void Video3DStereoMenuItem_Click(object sender, EventArgs e)
        {
            SetVideo3DView((ToolStripMenuItem)sender, Video3DView.NormalImage);
            Has3DView           = false;
            HasAspectRatio      = false;
            HasAspectRatioDVD   = false;
        }

        private void Video3DLeftMenuItem_Click(object sender, EventArgs e)
        {
            SetVideo3DView((ToolStripMenuItem)sender, Video3DView.LeftImage);
            Has3DView = true;
        }

        private void Video3DTopMenuItem_Click(object sender, EventArgs e)
        {
            SetVideo3DView((ToolStripMenuItem)sender, Video3DView.TopImage);
            Has3DView = true;
        }

        private void SetVideo3DView(ToolStripMenuItem menuItem, Video3DView view3D)
        {
            video3DStereoMenuItem.Checked   = false;
            video3DLeftMenuItem.Checked     = false;
            video3DTopMenuItem.Checked      = false;
            menuItem.Checked                = true;

            myPlayer.Video.View3D           = view3D;
        }

        #endregion


        // ******************************** Display Shape Menu / FullScreenMode Button Menu

        #region Display Shape Menu

        private void ArrowDownShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(arrowDownShapeMenuItem, DisplayShape.ArrowDown);
        }

        private void ArrowLeftShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(arrowLeftShapeMenuItem, DisplayShape.ArrowLeft);
        }

        private void ArrowRightShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(arrowRightShapeMenuItem, DisplayShape.ArrowRight);
        }

        private void ArrowUpShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(arrowUpShapeMenuItem, DisplayShape.ArrowUp);
        }

        private void BarsShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(barsShapeMenuItem, DisplayShape.Bars);
        }

        private void BeamsShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(beamsShapeMenuItem, DisplayShape.Beams);
        }

        private void CircleShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(circleShapeMenuItem, DisplayShape.Circle);
        }

        private void CrossShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(crossShapeMenuItem, DisplayShape.Cross);
        }

        private void CustomShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(customShapeMenuItem, DisplayShape.Custom);
        }

        private void DiamondShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(diamondShapeMenuItem, DisplayShape.Diamond);
        }

        private void FrameShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(frameShapeMenuItem, DisplayShape.Frame);
        }

        private void HeartShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(heartShapeMenuItem, DisplayShape.Heart);
        }

        private void HexagonShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(hexagonShapeMenuItem, DisplayShape.Hexagon);
        }

        private void OvalShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(ovalShapeMenuItem, DisplayShape.Oval);
        }

        private void RectangleShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(rectangleShapeMenuItem, DisplayShape.Rectangle);
        }

        private void RingShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(ringShapeMenuItem, DisplayShape.Ring);
        }

        private void RoundedShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(roundedShapeMenuItem, DisplayShape.Rounded);
        }

        private void SquareShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(squareShapeMenuItem, DisplayShape.Square);
        }

        private void StarShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(starShapeMenuItem, DisplayShape.Star);
        }

        private void TilesShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(tilesShapeMenuItem, DisplayShape.Tiles);
        }

        private void TriangleDownMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(triangleDownMenuItem, DisplayShape.TriangleDown);
        }

        private void TriangleLeftMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(triangleLeftMenuItem, DisplayShape.TriangleLeft);
        }

        private void TriangleRightMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(triangleRightMenuItem, DisplayShape.TriangleRight);
        }

        private void TriangleUpMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(triangleUpMenuItem, DisplayShape.TriangleUp);
        }

        private void ShapeVideoMenuItem_Click(object sender, EventArgs e)
        {
            if (!shapeVideoMenuItem.Checked)
            {
                shapeDisplayMenuItem.Checked = false;
                shapeVideoMenuItem.Checked = true;
                useVideoBoundsMenuItem.Checked = true;
                _useVideoShape = true;
                myPlayer.Display.SetShape(_displayShape, _useVideoShape, _setOverlayShape);
            }
        }

        private void ShapeDisplayMenuItem_Click(object sender, EventArgs e)
        {
            if (!shapeDisplayMenuItem.Checked)
            {
                shapeVideoMenuItem.Checked = false;
                shapeDisplayMenuItem.Checked = true;
                useVideoBoundsMenuItem.Checked = false;
                _useVideoShape = false;
                myPlayer.Display.SetShape(_displayShape, _useVideoShape, _setOverlayShape);
            }
        }

        private void ShapeOverlayOnMenuItem_Click(object sender, EventArgs e)
        {
            if (!shapeOverlayOnMenuItem.Checked)
            {
                shapeOverlayOffMenuItem.Checked = false;
                shapeOverlayOnMenuItem.Checked = true;
                setOverlayShapeMenuItem.Checked = true;
                _setOverlayShape = true;
                myPlayer.Display.SetShape(_displayShape, _useVideoShape, _setOverlayShape);
            }
        }

        private void ShapeOverlayOffMenuItem_Click(object sender, EventArgs e)
        {
            if (!shapeOverlayOffMenuItem.Checked)
            {
                shapeOverlayOnMenuItem.Checked = false;
                shapeOverlayOffMenuItem.Checked = true;
                setOverlayShapeMenuItem.Checked = false;
                _setOverlayShape = false;
                myPlayer.Display.SetShape(_displayShape, _useVideoShape, _setOverlayShape);
            }
        }

        private void NormalShapeMenuItem_Click(object sender, EventArgs e)
        {
            SetDisplayShape(normalShapeMenuItem, DisplayShape.Normal);
        }

        private void SetDisplayShape(ToolStripMenuItem menuItem, DisplayShape shape)
        {
            if (myPlayer.Display.Shape == shape)
            {
                _displayShape = DisplayShape.Normal;
                myPlayer.Display.SetShape(_displayShape, _useVideoShape, _setOverlayShape);

                CheckMenuItems(displayShapeMenuItem.DropDown, 0, 24, normalShapeMenuItem);
                normalShapeMenuItem.Checked = true;
            }
            else
            {
                _displayShape = shape;
                myPlayer.Display.SetShape(_displayShape, _useVideoShape, _setOverlayShape);

                CheckMenuItems(displayShapeMenuItem.DropDown, 0, 24, menuItem);
                normalShapeMenuItem.Checked = menuItem == normalShapeMenuItem;
            }

            BlockPositionSlider();
        }

        #endregion

        #region FullScreenMode Button Menu

        private void FullScreenModeMenu_Opening(object sender, CancelEventArgs e)
        {
            myPlayer.CursorHide.Enabled = false;

            bool enabled = Screen.AllScreens.Length > 1; // check for null
            fullScreenFormAllMenuItem.Enabled    = enabled;
            fullScreenParentAllMenuItem.Enabled  = enabled;
            fullScreenDisplayAllMenuItem.Enabled = enabled;
        }

        private void FullScreenModeMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            myPlayer.CursorHide.Enabled = true;
        }

        private void FullScreenModeMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string fullScreenModeName = e.ClickedItem.Text;
            if (fullScreenModeName == "") return; // ignore separator lines

            switch (fullScreenModeMenu.Items.IndexOf(e.ClickedItem))
            {
                case 0:
                    myPlayer.FullScreenMode = FullScreenMode.Form;
                    myPlayer.FullScreen = true;
                    break;
                case 1: 
                    myPlayer.FullScreenMode = FullScreenMode.Form_AllScreens;
                    myPlayer.FullScreen = true;
                    break;

                case 2:
                    myPlayer.FullScreenMode = FullScreenMode.Parent;
                    myPlayer.FullScreen = true;
                    break;
                case 3:
                    myPlayer.FullScreenMode = FullScreenMode.Parent_AllScreens;
                    myPlayer.FullScreen = true;
                    break;

                case 4:
                    myPlayer.FullScreenMode = FullScreenMode.Display;
                    myPlayer.FullScreen = true;
                    break;
                case 5:
                    myPlayer.FullScreenMode = FullScreenMode.Display_AllScreens;
                    myPlayer.FullScreen = true;
                    break;

                case 7: // FullscreenMode On/Off
                    myPlayer.FullScreen = !myPlayer.FullScreen;
                    break;
            }
            SetFullScreenModeMenu();
        }

        private void SetFullScreenModeMenu()
        {
            UnCheckMenuItems(fullScreenModeMenu, 0, 0);

            if (myPlayer.FullScreen)
            {
                switch (myPlayer.FullScreenMode)
                {
                    case FullScreenMode.Form:
                        ((ToolStripMenuItem)fullScreenModeMenu.Items[0]).Checked = true;
                        fullScreenModeButton.Text = fullScreenModeMenu.Items[0].Text;
                        break;
                    case FullScreenMode.Form_AllScreens:
                        ((ToolStripMenuItem)fullScreenModeMenu.Items[1]).Checked = true;
                        fullScreenModeButton.Text = fullScreenModeMenu.Items[1].Text;
                        break;

                    case FullScreenMode.Parent:
                        ((ToolStripMenuItem)fullScreenModeMenu.Items[2]).Checked = true;
                        fullScreenModeButton.Text = fullScreenModeMenu.Items[2].Text;
                        break;
                    case FullScreenMode.Parent_AllScreens:
                        ((ToolStripMenuItem)fullScreenModeMenu.Items[3]).Checked = true;
                        fullScreenModeButton.Text = fullScreenModeMenu.Items[3].Text;
                        break;

                    case FullScreenMode.Display:
                        ((ToolStripMenuItem)fullScreenModeMenu.Items[4]).Checked = true;
                        fullScreenModeButton.Text = fullScreenModeMenu.Items[4].Text;
                        break;
                    case FullScreenMode.Display_AllScreens:
                        ((ToolStripMenuItem)fullScreenModeMenu.Items[5]).Checked = true;
                        fullScreenModeButton.Text = fullScreenModeMenu.Items[5].Text;
                        break;
                }
                fullScreenLight.LightOn = true;
            }
            else
            {
                ((ToolStripMenuItem)fullScreenModeMenu.Items[7]).Checked = true;
                fullScreenModeButton.Text = fullScreenModeMenu.Items[7].Text;
                fullScreenLight.LightOn = false;

                if (!myPlayer.Playing) Icon = Properties.Resources.Media_Normal;
                else
                {
                    if (myPlayer.Paused) Icon = Properties.Resources.Media_Paused;
                    else Icon = Properties.Resources.Media_Playing;
                }
            }
        }

        #endregion

        
        // ******************************** Display Overlay Button / Display Overlay Menu Button

        #region Display Overlay Button

        private void DisplayOverlayMenu_Opening(object sender, CancelEventArgs e)
        {
            myPlayer.CursorHide.Enabled = false;
        }

        private void DisplayOverlayMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            myPlayer.CursorHide.Enabled = true;
        }

        // Overlay Mode Video
        private void VideoMenuItem_Click(object sender, EventArgs e)
        {
            SetOverlayVideoMode();
        }

        // also called from some display overlays
        internal void SetOverlayVideoMode()
        {
            myPlayer.Overlay.Mode = OverlayMode.Video;
            videoMenuItem.Checked = true;
            displayMenuItem.Checked = false;
            overlayModeMenuItem.Checked = false;
        }

        // Overlay Mode Display
        internal void DisplayMenuItem_Click(object sender, EventArgs e)
        {
            SetOverlayDisplayMode();
        }

        // also called from some display overlays
        internal void SetOverlayDisplayMode()
        {
            myPlayer.Overlay.Mode = OverlayMode.Display;
            videoMenuItem.Checked = false;
            displayMenuItem.Checked = true;
            overlayModeMenuItem.Checked = true;
        }

        // Overlay Hold
        private void OverlayHoldMenuItem_Click(object sender, EventArgs e)
        {
            myPlayer.Overlay.Hold = !myPlayer.Overlay.Hold;
            overlayHoldMenuItem.Checked = myPlayer.Overlay.Hold;

            _overlayHold = false; // reset overlay hold by application
        }

        // Message Overlay
        private void MessageMenuItem_Click(object sender, EventArgs e)
        {
            if (_messageOverlay == null) _messageOverlay = new MessageOverlay(this, myPlayer);
            SetWindowDrag(true);
            SetOverlay(sender, _messageOverlay, true, true);
        }

        // Scribble Overlay
        private void ScribbleMenuItem_Click(object sender, EventArgs e)
        {
            if (_scribbleOverlay == null) _scribbleOverlay = new ScribbleOverlay(this, myPlayer);
            SetWindowDrag(false);
            SetOverlay(sender, _scribbleOverlay, true, true);
        }

        // Tiles Overlay
        private void TilesMenuItem_Click(object sender, EventArgs e)
        {
            if (_tileOverlay == null) _tileOverlay = new TilesOverlay(this, myPlayer);
            //SetWindowDrag(false); is handled by the overlay (2 states: tiles & puzzle)
            SetOverlay(sender, _tileOverlay, true, true);
        }

        // Bouncing Overlay
        private void BouncingMenuItem_Click(object sender, EventArgs e)
        {
            if (_bouncingOverlay == null) _bouncingOverlay = new BouncingOverlay(this, myPlayer);
            //SetWindowDrag(false); is handled by the overlay (2 states: bounce & pong)
            SetOverlay(sender, _bouncingOverlay, true, true);
        }

        // PiP Overlay
        private void PiPMenuItem_Click(object sender, EventArgs e)
        {
            if (_pipOverlay == null) _pipOverlay = new PIPOverlay(this, myPlayer);
            SetWindowDrag(true);
            SetOverlay(sender, _pipOverlay, true, true);
        }

        // SubTitles Overlay
        private void SubtitlesMenuItem_Click(object sender, EventArgs e)
        {
            if (_subtitlesOverlay == null) _subtitlesOverlay = new SubtitlesOverlay(this, myPlayer);
            SetWindowDrag(true);
            SetOverlay(sender, _subtitlesOverlay, true, true);
        }

        // Zoom Select Overlay
        private void ZoomSelectMenuItem_Click(object sender, EventArgs e)
        {
            if (_zoomSelectOverlay == null) _zoomSelectOverlay = new ZoomSelectOverlay(this, myPlayer);
            SetWindowDrag(false);
            SetOverlay(sender, _zoomSelectOverlay, false, true);
        }

        // Video Wall Overlay
        private void VideoWallMenuItem_Click(object sender, EventArgs e)
        {
            if (_videoWallOverlay == null) _videoWallOverlay = new VideoWall(this, myPlayer);
            SetWindowDrag(true);
            SetOverlay(sender, _videoWallOverlay, true, true);
        }

        // MP3 Cover Overlay
        private void MP3CoverMenuItem_Click(object sender, EventArgs e)
        {
            if (_mp3CoverOverlay == null) _mp3CoverOverlay = new Mp3CoverOverlay(this, myPlayer);
            SetWindowDrag(true);
            SetOverlay(sender, _mp3CoverOverlay, false, true);
        }

        // MP3 Karaoke Overlay
        private void MP3KaraokeMenuItem_Click(object sender, EventArgs e)
        {
            if (_mp3KaraokeOverlay == null) _mp3KaraokeOverlay = new Mp3KaraokeOverlay(this, myPlayer);
            SetWindowDrag(true);
            SetOverlay(sender, _mp3KaraokeOverlay, false, true);
        }

        // Big Time Overlay
        private void BigTimeMenuItem_Click(object sender, EventArgs e)
        {
            if (_bigTimeOverlay == null) _bigTimeOverlay = new BigTimeOverlay(this, myPlayer, FontCollection);
            SetWindowDrag(true);
            SetOverlay(sender, _bigTimeOverlay, false, true);
        }

        // Status Info Overlay
        private void StatusInfoMenuItem_Click(object sender, EventArgs e)
        {
            if (_statusInfoOverlay == null) _statusInfoOverlay = new StatusInfoOverlay(this, myPlayer);
            SetWindowDrag(true);
            SetOverlay(sender, _statusInfoOverlay, false, true);
        }

        // Overlay Off
        private void OverlayOffMenuItem_Click(object sender, EventArgs e)
        {
            SetOverlay(sender, null, false, false);
            if (_overlayMenuEnabled) OverlayMenuButton_Click(overlayMenuButton, EventArgs.Empty);
            SetWindowDrag(true);
        }

        // General set overlay method called by overlaymenu eventhandlers
        private void SetOverlay(object sender, Form theOverlay, bool canFocus, bool hold)
        {
            if (myPlayer.Overlay.Window == theOverlay)
            {
                if (theOverlay == null) return;
                if (_overlayMenuEnabled) OverlayMenuButton_Click(overlayMenuButton, EventArgs.Empty);
                sender = overlayOffMenuItem;
                theOverlay = null;
                canFocus = false;
                hold = false;
            }

            // every overlay (in this application) has an IOverlay interface: HasMenu and a MenuEnabled property
            if (theOverlay != null)
            {
                if (((IOverlay)theOverlay).HasMenu)
                {
                    ((IOverlay)theOverlay).MenuEnabled = _overlayMenuEnabled;
                    myPlayer.Overlay.CanFocus = _overlayMenuEnabled;
                }
                else myPlayer.Overlay.CanFocus = canFocus;
            }

            // set / reset overlay hold by application
            if (hold)
            {
                if (!myPlayer.Overlay.Hold)
                {
                    if (myPlayer.Playing) overlayHoldMenuItem.Checked = myPlayer.Overlay.Hold = true;
                    _overlayHold = true;
                }
            }
            else if (_overlayHold)
            {
                overlayHoldMenuItem.Checked = myPlayer.Overlay.Hold = _overlayHold = false;
            }

            myPlayer.Overlay.Window = theOverlay;

            UnCheckMenuItems(displayOverlayMenu, 3, 0);
            if (theOverlay == null)
            {
                overlayOffMenuItem.Checked = true;
                displayOverlayButton.Text = overlayOffMenuItem.Text;
                overlayLight.LightOn = false;
                _userOverlay = false;
            }
            else
            {
                ((ToolStripMenuItem)sender).Checked = true;
                displayOverlayButton.Text = ((ToolStripMenuItem)sender).Text.Substring(8); // Strip 'Example' from the name
                overlayLight.LightOn = true;
                _userOverlay = true;
            }
        }

        #endregion

        #region Display Overlay Menu Button

        private void OverlayMenuButton_Click(object sender, EventArgs e)
        {
            _overlayMenuEnabled = !_overlayMenuEnabled;
            // every overlay (in this application) has an IOverlay interface: HasMenu and a MenuEnabled property
            if (myPlayer.Overlay.Window != null)
            {
                if (((IOverlay)myPlayer.Overlay.Window).HasMenu)
                {
                    ((IOverlay)myPlayer.Overlay.Window).MenuEnabled = _overlayMenuEnabled;
                    myPlayer.Overlay.CanFocus = _overlayMenuEnabled;
                }
            }
            overlayMenuLight.LightOn = _overlayMenuEnabled;
            overlayMenuMenuItem.Text = _overlayMenuEnabled ? "Hide Overlay Menu" : "Show Overlay Menu";
            //overlayMenuButton.Text = _overlayMenuEnabled ? "  Overlay Menu On" : "  Overlay Menu Off";
        }

        private void OverlayMenuMenuItem_Click(object sender, EventArgs e)
        {
            OverlayMenuButton_Click(overlayMenuButton, EventArgs.Empty);
            //overlayMenuButton.PerformClick();
        }

        #endregion

        
        // ******************************** Repeat / Start- and EndPosition

        #region Repeat

        private void RepeatMenu_Opening(object sender, CancelEventArgs e)
        {
            myPlayer.CursorHide.Enabled = false;
        }

        private void RepeatMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            myPlayer.CursorHide.Enabled = true;
        }

        // Repeat MenuHandler Repeat One
        private void RepeatOneMenuItem_Click(object sender, EventArgs e)
        {
            UnCheckMenuItems(repeatMenu, 0, 0);
            repeatButton.Text = repeatOneMenuItem.Text;
            repeatOneMenuItem.Checked = true;
            repeatLight.LightOn = true;

            RepeatAll = RepeatShuffle   = false;
            myPlayer.RepeatChapter      = false;
            myPlayer.Repeat = RepeatOne = true;

            _shuffleList = null;
        }

        private void RepeatChapterMenuItem_Click(object sender, EventArgs e)
        {
            //if (repeatChapterMenuItem.Enabled)
            {
                UnCheckMenuItems(repeatMenu, 0, 0);
                repeatButton.Text = repeatChapterMenuItem.Text;
                repeatChapterMenuItem.Checked = true;
                repeatLight.LightOn = true;

                RepeatAll = RepeatShuffle = false;
                myPlayer.Repeat = RepeatOne = false;
                myPlayer.RepeatChapter = true;

                _shuffleList = null;
            }
        }

        // Repeat MenuHandler Repeat All
        private void RepeatAllMenuItem_Click(object sender, EventArgs e)
        {
            UnCheckMenuItems(repeatMenu, 0, 0);
            repeatButton.Text = repeatAllMenuItem.Text;
            repeatAllMenuItem.Checked = true;
            repeatLight.LightOn = true;

            if (myPlayer.Repeat) myPlayer.Repeat = false;
            RepeatOne = RepeatShuffle            = false;
            myPlayer.RepeatChapter               = false;
            RepeatAll                            = true;

            _shuffleList = null;
        }

        // Repeat MenuHandler Repeat Shuffle
        private void ShuffleMenuItem_Click(object sender, EventArgs e)
        {
            UnCheckMenuItems(repeatMenu, 0, 0);
            repeatButton.Text = shuffleMenuItem.Text;
            shuffleMenuItem.Checked = true;
            repeatLight.LightOn = true;

            if (myPlayer.Repeat) myPlayer.Repeat = false;
            RepeatOne = RepeatAll                = false;
            myPlayer.RepeatChapter               = false;
            RepeatShuffle                        = true;

            CreateShuffleList();
        }

        // Create shuffle list
        private void CreateShuffleList()
        {
            if (Playlist.Count < 2)
            {
                RepeatAllMenuItem_Click(repeatAllMenuItem, EventArgs.Empty);
                return;
            }

            int n = Playlist.Count;

            _shuffleList = new int[n];
            for (int i = 0; i < n; i++) _shuffleList[i] = i;

            while (n > 0)
            {
                int k = _random.Next(n--);
                int temp = _shuffleList[k];
                _shuffleList[k] = _shuffleList[n];
                _shuffleList[n] = temp;
            }
            _shuffleToPlay = 0;
        }

        private void SetShuffleList()
        {
            if (_shuffleList != null)
            {
                int found = 0;
                for (int i = 0; i < _shuffleList.Length; i++)
                {
                    if (_shuffleList[i] == _mediaToPlay)
                    {
                        found = i;
                        break;
                    }
                }

                if (found > 0)
                {
                    _shuffleList[found] = _shuffleList[0];
                    _shuffleList[0] = _mediaToPlay;
                }
                _shuffleToPlay = 0;                
            }
        }

        // Repeat MenuHandler Repeat Off
        private void RepeatOffMenuItem_Click(object sender, EventArgs e)
        {
            UnCheckMenuItems(repeatMenu, 0, 0);
            repeatButton.Text = repeatOffMenuItem.Text;
            repeatOffMenuItem.Checked = true;
            repeatLight.LightOn = false;

            if (myPlayer.Repeat) myPlayer.Repeat = false;
            RepeatOne = RepeatAll = RepeatShuffle = false;
            myPlayer.RepeatChapter = false;

            _shuffleList = null;
        }

        #endregion

        #region Start- and EndPosition

        private static string CompleteTextBoxTime(string timeText)
        {
            if (timeText.Contains(" ")) timeText = timeText.Replace(' ', '0');
            if (timeText.Length < 8) timeText = timeText.PadRight(8, '0');
            return timeText;
        }

        // Input start time next OK (for next media to play)
        private void StartTimeNextTextBox_Validated(object sender, EventArgs e)
        {
            //startTimeNextTextBox.Text = CompleteTextBoxTime(startTimeNextTextBox.Text);
           // if (TimeSpan.TryParse(startTimeNextTextBox.Text, out TimeSpan startTimeNextSpan))
            {
                //_startTimeNext = startTimeNextSpan;
                MyPlayer_MediaStartStopTimeNextChanged();
            }
        }

        // Input end time next OK (for next media to play)
        private void StopTimeNextTextBox_Validated(object sender, EventArgs e)
        {
            //stopTimeNextTextBox.Text = CompleteTextBoxTime(stopTimeNextTextBox.Text);
           // if (TimeSpan.TryParse(stopTimeNextTextBox.Text, out TimeSpan stopTimeNextSpan))
            {
               // _stopTimeNext = stopTimeNextSpan;
               // MyPlayer_MediaStartStopTimeNextChanged();
            }
        }

        // Input start time OK (for playing media)
        private void StartTimeTextBox_Validated(object sender, EventArgs e)
        {
            //startTimeTextBox.Text = CompleteTextBoxTime(startTimeTextBox.Text);
            //if (TimeSpan.TryParse(startTimeTextBox.Text, out TimeSpan startTimeSpan))
            {
               // myPlayer.Media.StartTime = startTimeSpan;
                if (myPlayer.LastError)
                {
                   // startTimeTextBox.Text = myPlayer.Media.StartTime.ToString().Substring(0, 8);
                }
            }
        }

        // Input end time OK (for playing media)
        private void StopTimeTextBox_Validated(object sender, EventArgs e)
        {
            //stopTimeTextBox.Text = CompleteTextBoxTime(stopTimeTextBox.Text);
            //if (TimeSpan.TryParse(stopTimeTextBox.Text, out TimeSpan stopTimeSpan))
            {
                //myPlayer.Media.StopTime = stopTimeSpan;
                //if (myPlayer.LastError)
                {
                  //  stopTimeTextBox.Text = myPlayer.Media.StopTime.ToString().Substring(0, 8);
                }
            }
        }

        #endregion

        
        // ******************************** Speed TextBox

        #region Speed TextBox

        private void SpeedTextBox_Validated(object sender, EventArgs e)
        {
            if (float.TryParse(speedTextBox.Text, out float speed))
            {
                if (myPlayer.Speed.Boost)
                {
                    if (speed == 1.0) myPlayer.Speed.Boost = false;
                }
                else
                {
                    if (speed >= 9.9) myPlayer.Speed.Boost = true;
                }

                if (speed < myPlayer.Speed.Minimum) speed = myPlayer.Speed.Minimum;
                else if (speed > myPlayer.Speed.Maximum) speed = myPlayer.Speed.Maximum;

                if (myPlayer.Speed.Rate == speed) speedTextBox.Text = myPlayer.Speed.Rate.ToString("0.00#");
                else
                {
                    myPlayer.Speed.Rate = speed;
                    if (myPlayer.LastError)
                    {
                        speedTextBox.Text = myPlayer.Speed.Rate.ToString("0.00#");
                    }
                }
            }
        }

        #endregion

        
        // ******************************** Output Level Meter

        #region Output Level Meter

        // Output Level Meter Click - Set color
        private void LevelMeterPanels_Click(object sender, EventArgs e)
        {
            SetLevelMeterColor();
        }

        private void SetLevelMeterColor()
        {
            ColorDialog dlg = new ColorDialog
            {
                Color = Prefs.MainLevelMeterColor
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _levelMeterBrush.Dispose();
                _levelMeterBrush = new HatchBrush(_levelMeterHatchStyle, dlg.Color);
                Prefs.MainLevelMeterColor = dlg.Color;
            }
            dlg.Dispose();
        }

        // Output Level Meter Left
        private void LeftLevelMeterPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(_levelMeterBrush, 0, 0, _levelMeterLeft * leftLevelMeterPanel.ClientRectangle.Width, leftLevelMeterPanel.ClientRectangle.Height);
        }

        // Output Level Meter Right
        private void RightLevelMeterPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(_levelMeterBrush, 0, 0, _levelMeterRight * rightLevelMeterPanel.ClientRectangle.Width, rightLevelMeterPanel.ClientRectangle.Height);
        }

        #endregion

        
        // ******************************** Audio Mute Label Click / Audio Devices Menu

        #region Audio Label Click

        // Audio Label Click - Mute
        private void VolumeLabelPanel_Click(object sender, EventArgs e)
        {
            myPlayer.Audio.Mute = !myPlayer.Audio.Mute;
            if (!myPlayer.Audio.Mute)
            {
                audioTracksLabel.ForeColor = Color.Lime;
                channelCountLabel.ForeColor = Color.Lime;

                ButtonFlash.Remove(audioLight);
                audioLight.ForeColor = Color.Lime;
                if (myPlayer.Audio.Volume > 0)
                {
                    _volumeRedDial = false;
                    volumeDial.SwitchImage(false);
                    balanceDial.SwitchImage(false);
                }
            }
            else
            {
                audioTracksLabel.ForeColor = Color.Red;
                channelCountLabel.ForeColor = Color.Red;

                audioLight.ForeColor = Color.Red;
                ButtonFlash.Add(audioLight, Color.Black, Color.Red);
                if (!_volumeRedDial)
                {
                    _volumeRedDial = true;
                    volumeDial.SwitchImage(true);
                    balanceDial.SwitchImage(true);
                }
            }
        }

        private void SystemSoundMenuItem_Click(object sender, EventArgs e)
        {
            myPlayer.SystemPanels.ShowAudioDevices(this);
        }

        private void SystemMixerMenuItem_Click(object sender, EventArgs e)
        {
            myPlayer.SystemPanels.ShowAudioMixer(this);
        }

        #endregion

        #region Audio Device Button and Menu

        private void AudioDeviceMenu_Opening(object sender, CancelEventArgs e)
        {
            myPlayer.CursorHide.Enabled = false;
        }

        private void AudioDeviceMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            myPlayer.CursorHide.Enabled = true;
        }

        private void AudioDeviceMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int index = audioDeviceMenu.Items.IndexOf(e.ClickedItem);

            switch (index)
            {
                case 0: // audio devices
                    myPlayer.SystemPanels.ShowAudioDevices(this);
                    break;

                case 1: // audio volume mixer
                    myPlayer.SystemPanels.ShowAudioMixer(this);
                    break;

                case 2: // separator
                    break;

                case 3: // audio device volume
                    if (_deviceVolumeDialog == null)
                    {
                        _deviceVolumeDialog = new DeviceVolumeDialog(myPlayer);
                        _deviceVolumeDialog.FormClosed += DeviceVolumeDialog_FormClosed;
                        CenterDialog(this, _deviceVolumeDialog);
                        _deviceVolumeDialog.Show(this);
                        deviceVolumeMenuItem.Checked = true;
                    }
                    else
                    {
                        _deviceVolumeDialog.Activate();
                    }
                    break;

                case 4: // audio channel mixer
                    if (_channelMixerDialog == null)
                    {
                        _channelMixerDialog = new ChannelMixerDialog(myPlayer);
                        _channelMixerDialog.FormClosed += ChannelMixerDialog_FormClosed;
                        CenterDialog(this, _channelMixerDialog);
                        _channelMixerDialog.Show(this);
                        channelVolumesMenuItem.Checked = true;
                    }
                    else
                    {
                        _channelMixerDialog.Activate();
                    }
                    break;

                case 5: // Mono Audio
                    myPlayer.Audio.Mono         = !myPlayer.Audio.Mono;
                    monoAudioMenuItem.Checked   = myPlayer.Audio.Mono;
                    channelCountLabel.Text      = myPlayer.Audio.ChannelCount.ToString();
                    break;

                case 6: // separator
                    break;

                default:
                    if (index >= START_AUDIO_DEVICE_ITEMS - 1)
                    {
                        if (!((ToolStripMenuItem)audioDeviceMenu.Items[index]).Checked)
                        {
                            if (index == START_AUDIO_DEVICE_ITEMS - 1) myPlayer.Audio.Device = null;
                            else myPlayer.Audio.Device = _audioDevices[index - START_AUDIO_DEVICE_ITEMS];
                            // menu and other items updated by audio device event handlers
                        }
                    }
                    break;
            }
        }

        private void DeviceVolumeDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            deviceVolumeMenuItem.Checked = false;
            _deviceVolumeDialog = null;
        }

        private void ChannelMixerDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            channelVolumesMenuItem.Checked = false;
            _channelMixerDialog = null;
        }

        private void CreateAudioDeviceMenu()
        {
            audioDeviceMenu.SuspendLayout();

            while (audioDeviceMenu.Items.Count > START_AUDIO_DEVICE_ITEMS) audioDeviceMenu.Items.RemoveAt(START_AUDIO_DEVICE_ITEMS);

            _audioDevices = myPlayer.Audio.GetDevices();
            if (_audioDevices != null)
            {
                if (_audioDevices.Length > 1 || myPlayer.Audio.Device != null)
                {
                    for (int i = 0; i < _audioDevices.Length; i++) audioDeviceMenu.Items.Add(_audioDevices[i].Name);
                    //for (int i = 0; i < _audioDevices.Length; i++) audioDeviceMenu.Items.Add(_audioDevices[i].ToString());
                }
            }

            audioDeviceMenu.ResumeLayout();
            SetAudioDeviceMenu();
        }

        private void SetAudioDeviceMenu()
        {
            int count = _audioDevices == null ? 0 : _audioDevices.Length;

            if (count == 0)
            {
                _audioDeviceSelected = null;
                audioDeviceButton.Text = "[ No Devices ]";

                // no devices = no sound -> reset peakmeter
                //_levelMeterLeft = _levelMeterRight = 0;
                //_levelMeterHoldLeft = _levelMeterHoldRight = 0;
                //leftLevelMeterPanel.Invalidate();
                //rightLevelMeterPanel.Invalidate();
            }
            else
            {
                bool set = false;
                if (count > 1 || myPlayer.Audio.Device != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        // default device may have changed
                        if (_audioDeviceSelected != null && _audioDevices[i].Id == _audioDeviceSelected.Id)
                        {
                            ((ToolStripMenuItem)audioDeviceMenu.Items[i + START_AUDIO_DEVICE_ITEMS]).Checked = true;
                            _audioDeviceSelected = _audioDevices[i];
                            audioDeviceButton.Text = _audioDeviceSelected.Name;
                            set = true;
                        }
                        else ((ToolStripMenuItem)audioDeviceMenu.Items[i + START_AUDIO_DEVICE_ITEMS]).Checked = false;
                    }
                }
                if (!set)
                {
                    _audioDeviceSelected = null;
                    ((ToolStripMenuItem)audioDeviceMenu.Items[START_AUDIO_DEVICE_ITEMS - 1]).Checked = true;
                    audioDeviceButton.Text = "[ " + myPlayer.Audio.GetDefaultDevice().Name + " ]";
                }
                else ((ToolStripMenuItem)audioDeviceMenu.Items[START_AUDIO_DEVICE_ITEMS - 1]).Checked = false;
            }
        }

        #endregion

        
        // ******************************** Video and Audio Tracks Label and Menu Item Click

        #region Video and Audio Label and Tracks Menu Item Click

        private void VideoTracksLabel_MouseClick(object sender, MouseEventArgs e)
        {
            videoTracksMenuItem.DropDown.Show(videoTracksLabel, -9, 11);
        }

        private void VideoTracks_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int index = videoTracksMenuItem.DropDown.Items.IndexOf(e.ClickedItem);
            myPlayer.Video.Track = index;
            if (!myPlayer.LastError)
            {
                for (int i = 0; i < videoTracksMenuItem.DropDown.Items.Count; i++)
                {
                    ((ToolStripMenuItem)(videoTracksMenuItem.DropDown.Items[i])).Checked = false;
                }
                ((ToolStripMenuItem)(videoTracksMenuItem.DropDown.Items[index])).Checked = true;
            }
        }

        private void AudioTracksLabel_MouseClick(object sender, MouseEventArgs e)
        {
            audioTracksMenuItem.DropDown.Show(audioTracksLabel, -9, 11);
        }

        private void AudioTracks_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int index = audioTracksMenuItem.DropDown.Items.IndexOf(e.ClickedItem);
            myPlayer.Audio.Track = index;
            if (!myPlayer.LastError)
            {
                for (int i = 0; i < audioTracksMenuItem.DropDown.Items.Count; i++)
                {
                    ((ToolStripMenuItem)(audioTracksMenuItem.DropDown.Items[i])).Checked = false;
                }
                ((ToolStripMenuItem)(audioTracksMenuItem.DropDown.Items[index])).Checked = true;

                channelCountLabel.Text = myPlayer.Audio.ChannelCount.ToString();
            }
        }

        #endregion

        
        // ******************************** Video Zoom, Move and Stretch Buttons

        #region Video Zoom, Move and Stretch Buttons

        // Video Zoom MouseWheel
        void ZoomInButton_MouseWheel(object sender, MouseEventArgs e)
        {
            myPlayer.Video.Zoom(e.Delta > 0 ? 1.1 : 0.9);
        }

        // Video Stretch/Move MouseWheel
        void StretchUpButton_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (_videoMoveMode) myPlayer.Video.Move(0, 4);
                else DoStretch(0, -4);
            }
            else
            {
                if (_videoMoveMode) myPlayer.Video.Move(0, -4);
                else DoStretch(0, 4);
            }
        }

        void StretchLeftButton_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (_videoMoveMode) myPlayer.Video.Move(4, 0);
                else DoStretch(-4, 0);
            }
            else
            {
                if (_videoMoveMode) myPlayer.Video.Move(-4, 0);
                else DoStretch(4, 0);
            }
        }

        // Video Zoom Buttons
        private void ZoomInButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) myPlayer.Video.Zoom(1.1);
        }

        private void ZoomOutButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) myPlayer.Video.Zoom(0.9);
        }

        // Video Stretch/Move Buttons
        private void StretchUpButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_videoMoveMode) DoMove(0, -1);
                else DoStretch(0, 2);
            }
        }

        private void StretchDownButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_videoMoveMode) DoMove(0, 1);
                else DoStretch(0, -2);
            }
        }

        private void StretchLeftButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_videoMoveMode) DoMove(-1, 0);
                else DoStretch(-2, 0);
            }
        }

        private void StretchRightButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_videoMoveMode) DoMove(1, 0);
                DoStretch(2, 0);
            }
        }

        private void DoMove(int x, int y)
        {
            do
            {
                myPlayer.Video.Move(x, y);
                Application.DoEvents();
                Thread.Sleep(2);
            }
            while ((MouseButtons & MouseButtons.Left) != 0);
        }

        private void DoStretch(int x, int y)
        {
            do
            {
                myPlayer.Video.Stretch(x, y);
                Application.DoEvents();
                Thread.Sleep(2);
            }
            while ((MouseButtons & MouseButtons.Left) != 0);
        }

        // Switch between Video Move and Video Stretch
        private void StretchLabel_MouseClick(object sender, MouseEventArgs e)
        {
           // _videoMoveMode = !_videoMoveMode;
            //if (_videoMoveMode) stretchLabel.Text = _videoMoveText;
            //else stretchLabel.Text = _videoStretchText;
        }

        #endregion

        
        // ******************************** Video Color Dialog

        #region Video Color

        private void VideoColorMenuItem_Click(object sender, EventArgs e)
        {
            if (_videoColorDialog == null)
            {
                _videoColorDialog = new VideoColorDialog(myPlayer);
                _videoColorDialog.FormClosed += VideoColorDialog_FormClosed;

                CenterDialog(this, _videoColorDialog);
                _videoColorDialog.Top = RectangleToScreen(displayPanel.Bounds).Bottom - _videoColorDialog.Height - 64;

                _videoColorDialog.Show(this);
            }
            else
            {
                _videoColorDialog.Close();
            }
        }

		private void VideoColorDialog_FormClosed(object sender, FormClosedEventArgs e)
		{
            _videoColorDialog = null;
        }

        #endregion


        // ******************************** Video Overlay Dialog

        #region Video Overlay Dialog

        private void VideoOverlayMenuItem_Click(object sender, EventArgs e)
        {
            if (_videoOverlayDialog == null)
            {
                _videoOverlayDialog = new VideoOverlayDialog(this, null, myPlayer);
                _videoOverlayDialog.FormClosed += VideoOverlayDialog_FormClosed;
                CenterDialog(this, _videoOverlayDialog);
                _videoOverlayDialog.Show(this);
            }
            else
            {
                _videoOverlayDialog.Activate();
            }
        }

        private void VideoOverlayDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            _videoOverlayDialog = null;
        }

        #endregion


        // ******************************** Copy Picturebox

        #region Copy Picturebox

        private void SetCopyMenu()
        {
            //if (pictureBox1.Image == null)
            {
                openCopyMenuItem.Enabled = false;
                openWithCopyMenuItem.Enabled = false;
                clearCopyMenuItem.Enabled = false;
            }
            //else
            {
                openCopyMenuItem.Enabled = true;
                openWithCopyMenuItem.Enabled = true;
                clearCopyMenuItem.Enabled = true;
            }
        }

        // left click on copymode label - open contextmenu (besides 'normal' right click)
        private void CopyModeLabel_MouseClick(object sender, MouseEventArgs e)
        {
            //copyModeMenu.Show(copyModeLabel.PointToScreen(e.Location));
        }

        // Picturebox clicked - Copy
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
           // if (e.Button == MouseButtons.Left) DoScreenCopy();
        }

        // playing with display clones - 1/2:
        private void PictureBox1_MouseEnter(object sender, EventArgs e)
        {
           // if (pictureBox1.Image == null && (myPlayer.Has.Video || myPlayer.Has.OverlayShown) && (myPlayer.Copy.Mode == CopyMode.Video || myPlayer.Copy.Mode == CopyMode.Display))
            {
               // myPlayer.DisplayClones.Add(pictureBox1);
                //if (!myPlayer.LastError) _copyHasDisplayClone = true;
            }
        }

        // playing with display clones - 2/2:
        private void PictureBox1_MouseLeave(object sender, EventArgs e)
        {
            if (_copyHasDisplayClone)
            {
                _copyHasDisplayClone = false;
                //myPlayer.DisplayClones.Remove(pictureBox1);
            }
        }

        /*private void DoScreenCopy()
        {
            //if (myPlayer.Video.Present || myPlayer.Overlay.Visible || myPlayer.Copy.Mode == CopyMode.Form || myPlayer.Copy.Mode == CopyMode.Screen)
            {
                //if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
                else if (_copyHasDisplayClone)
                {
                  //  myPlayer.DisplayClones.Remove(pictureBox1);
                    _copyHasDisplayClone = false;
                }

                //pictureBox1.Image = myPlayer.Copy.ToImage();
               // if (pictureBox1.Image != null) Clipboard.SetImage(pictureBox1.Image);

                SetCopyMenu();
            }
        }*/

        // The copy contextmenu items click events:

        // Copy
        private void CopyMenuItem_Click(object sender, EventArgs e)
        {
            //DoScreenCopy();
        }

        // Copy Mode
        private void CopyModeMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Translate the text of the clicked item into a displaymode enum value
            try
            {
                SetScreenCopyModeMenu((CopyMode)Enum.Parse(typeof(CopyMode), e.ClickedItem.Text), true);
            }
            catch { }
        }

        // Set Copy Mode
        private void SetScreenCopyModeMenu(CopyMode copyMode, bool setMode)
        {
            string screenCopyModeName = copyMode.ToString();

            if (setMode) myPlayer.Copy.Mode = copyMode;
            //copyModeLabel.Text = screenCopyModeName;
            foreach (ToolStripMenuItem menuItem in copyModeMenu.Items)
            {
                menuItem.Checked = menuItem.Text == screenCopyModeName;
            }
        }

        // Open
        private void OpenCopyMenuItem_Click(object sender, EventArgs e)
        {
           // if //(pictureBox1.Image != null)
            {
                //try
                {
                   // pictureBox1.Image.Save(_copyFile, _copyFormat);
                    Process.Start(_copyFile);
                }
                //catch { }
            }
        }

        // Open With
        private void OpenWithMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //pictureBox1.Image.Save(_copyFile, _copyFormat);
                Process.Start("rundll32.exe", "shell32.dll,OpenAs_RunDLL " + _copyFile);
            }
            catch { }
        }

        // Clear
        private void ClearCopyMenuItem_Click(object sender, EventArgs e)
        {
           // if (pictureBox1.Image != null)
            {
               // pictureBox1.Image.Dispose();
               //pictureBox1.Image = null;
                try { if (File.Exists(_copyFile)) File.Delete(_copyFile); }
                catch { }

                Clipboard.Clear();

                SetCopyMenu();
            }
        }

		#endregion


		// ******************************** Sliders and Dials ContextMenus / High Speed Playback / PositionSlider

		#region Sliders and Dials ContextMenus

		// Context menu used with the Speed, AudioVolume an AudioBalance Sliders (trackbars)

		// Closed handled by ContextMenu_Closing (MouseHide)
		private void SliderMenu_Opening(object sender, CancelEventArgs e)
        {
            myPlayer.CursorHide.Enabled = false;

            if (sliderMenu.SourceControl == speedSlider || sliderMenu.SourceControl == speedLabelText)
            {
                sliderMenuItem1.Text = "Velocidad Mínima";
                sliderMenuItem2.Text = "Velocidad Media";
                sliderMenuItem3.Text = "Velocidad Normal";
                sliderMenuItem4.Text = "Doble Velocidad";
                sliderMenuItem5.Text = "Máxima Velocidad";

                // High Speed
                sliderMenu.Items.Add("-");
                sliderMenu.Items.Add("Alta Velocidad");
                sliderMenu.Items.Add("Ultra Velocidad");

                // only local video files - but always show high speed menu items
                if (myPlayer.Has.Video && myPlayer.Media.SourceCategory == MediaSourceCategory.LocalFile)
                {
                    sliderMenu.Items[6].Click += SliderMenuItem6_Click;
                    sliderMenu.Items[7].Click += SliderMenuItem7_Click;
                }
                else
                {
                    sliderMenu.Items[6].Enabled = false;
                    sliderMenu.Items[7].Enabled = false;
                }
            }
            else if (sliderMenu.SourceControl == volumeDial || sliderMenu.SourceControl == volumeDialLabel)
            {
                sliderMenuItem1.Text = "Fade Out";
                sliderMenuItem2.Text = "Volumen Bajo";
                sliderMenuItem3.Text = "Volumen Medio";
                sliderMenuItem4.Text = "Volumen Alto";
                sliderMenuItem5.Text = "Máximo Volumen";
            }
            else if (sliderMenu.SourceControl == balanceDial || sliderMenu.SourceControl == balanceDialLabel)
            {
                sliderMenuItem1.Text = "Balance Izquierdo";
                sliderMenuItem2.Text = "Balance Izquierdo Central";
                sliderMenuItem3.Text = "Balcande Central";
                sliderMenuItem4.Text = "Balance Derecho Central";
                sliderMenuItem5.Text = "Balance Derecho";
            }
        }

        private void SliderMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (sliderMenu.Items.Count == 8)
            {
                sliderMenu.Items.RemoveAt(5);
                sliderMenu.Items.RemoveAt(5);
                sliderMenu.Items.RemoveAt(5);
            }

            myPlayer.CursorHide.Enabled = true;
        }

        // Minimum
        private void SliderMenuItem1_Click(object sender, EventArgs e)
        {
            if (sliderMenu.SourceControl == speedSlider || sliderMenu.SourceControl == speedLabelText)
            {
                if (myPlayer.Playing) myPlayer.Speed.Rate = myPlayer.Speed.Minimum;
                else myPlayer.Speed.Rate = 0.1f;
            }
            else if (sliderMenu.SourceControl == volumeDial || sliderMenu.SourceControl == volumeDialLabel)     myPlayer.Audio.VolumeTo(0);     // myPlayer.Audio.Volume  = 0;
            else if (sliderMenu.SourceControl == balanceDial || sliderMenu.SourceControl == balanceDialLabel)   myPlayer.Audio.BalanceTo(-1);   //myPlayer.Audio.Balance = -1;
        }

        // Low
        private void SliderMenuItem2_Click(object sender, EventArgs e)
        {
            if (sliderMenu.SourceControl == speedSlider || sliderMenu.SourceControl      == speedLabelText)     myPlayer.Speed.Rate = 0.5f;
            else if (sliderMenu.SourceControl == volumeDial || sliderMenu.SourceControl  == volumeDialLabel)    myPlayer.Audio.VolumeTo(0.25f);     // myPlayer.Audio.Volume  = 0.25f;
            else if (sliderMenu.SourceControl == balanceDial || sliderMenu.SourceControl == balanceDialLabel)   myPlayer.Audio.BalanceTo(-0.50f);   //myPlayer.Audio.Balance = -0.50f;
        }

        // Center
        private void SliderMenuItem3_Click(object sender, EventArgs e)
        {
            if (sliderMenu.SourceControl == speedSlider || sliderMenu.SourceControl      == speedLabelText)     myPlayer.Speed.Rate = 1;
            else if (sliderMenu.SourceControl == volumeDial || sliderMenu.SourceControl  == volumeDialLabel)    myPlayer.Audio.VolumeTo(0.5f);  // myPlayer.Audio.Volume  = 0.5f;
            else if (sliderMenu.SourceControl == balanceDial || sliderMenu.SourceControl == balanceDialLabel)   myPlayer.Audio.BalanceTo(0);    //myPlayer.Audio.Balance = 0;
        }

        // High
        private void SliderMenuItem4_Click(object sender, EventArgs e)
        {
            if (sliderMenu.SourceControl == speedSlider || sliderMenu.SourceControl      == speedLabelText)     myPlayer.Speed.Rate = 2.0f;
            else if (sliderMenu.SourceControl == volumeDial || sliderMenu.SourceControl  == volumeDialLabel)    myPlayer.Audio.VolumeTo(0.75f);     // myPlayer.Audio.Volume  = 0.75f;
            else if (sliderMenu.SourceControl == balanceDial || sliderMenu.SourceControl == balanceDialLabel)   myPlayer.Audio.BalanceTo(0.75f);    //myPlayer.Audio.Balance = 0.50f;
        }

        // Maximum
        private void SliderMenuItem5_Click(object sender, EventArgs e)
        {
            if (sliderMenu.SourceControl == speedSlider || sliderMenu.SourceControl == speedLabelText)
            {
                if (myPlayer.Playing) myPlayer.Speed.Rate = myPlayer.Speed.Maximum;
                else myPlayer.Speed.Rate = 4;
            }
            else if (sliderMenu.SourceControl == volumeDial || sliderMenu.SourceControl == volumeDialLabel)     myPlayer.Audio.VolumeTo(1);     // myPlayer.Audio.Volume  = 1;
            else if (sliderMenu.SourceControl == balanceDial || sliderMenu.SourceControl == balanceDialLabel)   myPlayer.Audio.BalanceTo(1);    // myPlayer.Audio.Balance = 1;
        }

        // High Speed
        private void SliderMenuItem6_Click(object sender, EventArgs e)
        {
            HighSpeedPlayback();
        }

        // Ultra Speed
        private void SliderMenuItem7_Click(object sender, EventArgs e)
        {
            UltraSpeedPlayback();
        }

        #endregion

        #region High Speed Playback

        private void HighSpeedPlayback()
        {
            // only local video files
            if (myPlayer.Has.Video && myPlayer.Media.SourceCategory == MediaSourceCategory.LocalFile)
            {
                // the "try catch" is for when the application is closed while this is running
                _highSpeedMode = true; // don't show pause is enabled (with Pause())

                myPlayer.Resume(); // pause lights off (if any)
                myPlayer.Pause();
                myPlayer.Speed.Rate = 1;
                myPlayer.TaskbarProgress.State = TaskbarProgressState.Normal;

                speedLight.ForeColor = Color.Lime;
                speedLight.LightOn = true;

                int result = 0;
                try
                {
                    while (result == 0 && MouseButtons != MouseButtons.Left)
                    {
                        result = myPlayer.Position.Step(1);
                        // slow down - timing depends on the computer used - not good
                        for (int i = 0; i < 5500; i++) Application.DoEvents();
                    }
                    myPlayer.Resume();
                }
                catch { }

                try
                {
                    if (result == 0)
                    {
                        speedLight.LightOn = false;
                        speedLight.ForeColor = Color.Red;
                        _highSpeedMode = false;
                        _ultraSpeedMode = false;
                    }
                }
                catch { }
            }
        }

        private void UltraSpeedPlayback()
        {
            // only local video files
            if (myPlayer.Has.Video && myPlayer.Media.SourceCategory == MediaSourceCategory.LocalFile)
            {

                _highSpeedMode = true; // don't show pause is enabled (with Pause())
                _ultraSpeedMode = true;

                if (myPlayer.Media.Length.TotalSeconds < 15)
                {
                    HighSpeedPlayback();
                }
                else
                {
                    myPlayer.Resume(); // pause lights off (if any)
                    myPlayer.Pause();
                    myPlayer.Speed.Rate = 1;
                    myPlayer.TaskbarProgress.State = TaskbarProgressState.Normal;

                    speedLight.ForeColor = Color.Lime;
                    speedLight.LightOn = true;
                    //Player1.Position.StepEOFMargin = 500;

                    // the "try catch" is for when the application is closed while this is running
                    int result = 0;
                    try
                    {
                        while (result == 0 && MouseButtons != MouseButtons.Left)
                        {
                            result = myPlayer.Position.Step(1);
                            for (int i = 0; i < 2000; i++) Application.DoEvents();
                        }
                        myPlayer.Resume();
                    }
                    catch { }

                    try
                    {
                        if (result == 0)
                        {
                            speedLight.LightOn = false;
                            speedLight.ForeColor = Color.Red;
                            _highSpeedMode = false;
                            _ultraSpeedMode = false;
                            //Player1.Position.StepEOFMargin = 200;
                        }
                    }
                    catch { }
                }
            }
        }

        #endregion

        #region PositionSlider Contextmenu

        private void PositionSliderMenu_Opening(object sender, CancelEventArgs e)
        {
            myPlayer.CursorHide.Enabled = false;
        }

        private void PositionSliderMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            myPlayer.CursorHide.Enabled = true;
        }

        #region PositionSlider Visible / Progress / LiveUpdate / Silent Seek

        // Always Visible
        private void SliderAlwayVisibleMenuItem_Click(object sender, EventArgs e)
        {
            SetSliderVisibility(!_sliderHidden);
        }

        private void SetSliderVisibility(bool hide)
        {
            if (_sliderHidden != hide)
            {
                _sliderHidden = hide;
                if (_sliderHidden)
                {
                    sliderAlwaysVisibleMenuItem.Checked = false;
                    SliderPanelHide();

                    positionSlider.MouseLeave += PositionSliderPanel_MouseLeave;
                    positionSliderPanel.MouseLeave += PositionSliderPanel_MouseLeave;
                    positionSliderPanel.MouseEnter += PositionSliderPanel_MouseEnter;
                }
                else
                {
                    positionSlider.MouseLeave -= PositionSliderPanel_MouseLeave;
                    positionSliderPanel.MouseLeave -= PositionSliderPanel_MouseLeave;
                    positionSliderPanel.MouseEnter -= PositionSliderPanel_MouseEnter;

                    sliderAlwaysVisibleMenuItem.Checked = true;
                    SliderPanelShow();
                }
            }
        }

        // Slider Track / Progress
        private void SliderShowsProgressMenuItem_Click(object sender, EventArgs e)
        {
            SetSliderProgress(myPlayer.Sliders.Position.Mode == PositionSliderMode.Track);
        }

        private void SetSliderProgress(bool progress)
        {
            if (progress != sliderShowsProgressMenuItem.Checked)
            {
                myPlayer.Sliders.Position.Mode = progress ? PositionSliderMode.Progress : PositionSliderMode.Track;
                sliderShowsProgressMenuItem.Checked = progress;

                //Player1.TaskbarProgress.Mode = progress ? TaskbarProgressMode.Progress : TaskbarProgressMode.Track;
            }
        }

        // Seek Live Update
        private void SliderSeekLiveUpdateMenuItem_Click(object sender, EventArgs e)
        {
            SetSliderLiveUpdate(!sliderSeekLiveUpdateMenuItem.Checked);
        }

        private void SetSliderLiveUpdate(bool update)
        {
            if (update != sliderSeekLiveUpdateMenuItem.Checked)
            {
                myPlayer.Sliders.Position.LiveUpdate = sliderSeekLiveUpdateMenuItem.Checked = update;
            }
        }

        // Seek Always Silent
        private void SliderSeekSilentMenuItem_Click(object sender, EventArgs e)
        {
            SetSliderSeekSilent(!sliderSeekSilentMenuItem.Checked);
        }

        private void SetSliderSeekSilent(bool silent)
        {
            if (silent != sliderSeekSilentMenuItem.Checked)
            {
                myPlayer.Sliders.Position.SilentSeek = silent ? SilentSeek.Always : SilentSeek.OnMoving;
                sliderSeekSilentMenuItem.Checked = silent;
            }
        }

        #endregion

        #region PositionSlider InfoLabels / Preview

        // Sliders InfoLabels
        private void SlidersShowInfoLabelsMenuItem_Click(object sender, EventArgs e)
        {
            SetInfoLabels(!slidersShowInfoLabelsMenuItem.Checked);
        }

        private void SetInfoLabels(bool show)
        {
            if (show != slidersShowInfoLabelsMenuItem.Checked)
            {
                Prefs.ShowInfoLabels = slidersShowInfoLabelsMenuItem.Checked = show;
                if (show)
                {
                    if (_positionLabel == null)
                    {
                        // position slider label
                        _positionLabel = new InfoLabel
                        {
                            Font                        = _crystalFont16,
                            UseCompatibleTextRendering  = true,
                            RoundedCorners              = true,
                            TextMargin                  = new Padding(2, 2, 2, 0), // fine tune spacing
                            ForeColor                   = Color.FromArgb(179, 173,146)
                        };
                        //_positionLabel.BackColor   = Color.FromArgb(18, 18, 18);
                        //_positionLabel.BorderColor = _sliderLabel.ForeColor;
                        _positionLabel.BackBrush = new LinearGradientBrush(
                            new Rectangle(0, 0, _positionLabel.Size.Width, _positionLabel.Size.Height),
                            Color.FromArgb(50, 50, 50), Color.Black, LinearGradientMode.Vertical);

                        // other sliders
                        _sliderLabel = new InfoLabel
                        {
                            FontSize        = 9.25f,
                            RoundedCorners  = true,
                            TextMargin      = new Padding(1, 0, 1, 2), // fine tune spacing
                            ForeColor       = Color.FromArgb(179, 173, 146)
                        };
                        //_sliderLabel.BackColor                    = Color.FromArgb(18, 18, 18);
                        //_sliderLabel.BorderColor                  = _sliderLabel.ForeColor;
                        _sliderLabel.BackBrush = new LinearGradientBrush(
                            new Rectangle(new Point(0, 0), _sliderLabel.Size),
                            Color.FromArgb(64, 64, 64), Color.Black, LinearGradientMode.Vertical);

                        // the infolabels are used with:
                        positionSlider.Scroll   += PositionSlider_Scroll;
                        speedSlider.Scroll      += SpeedSlider_Scroll;
                        shuttleSlider.Scroll    += ShuttleSlider_Scroll;
                    }
                }
                else
                {
                    positionSlider.Scroll -= PositionSlider_Scroll;
                    speedSlider.Scroll -= SpeedSlider_Scroll;
                    shuttleSlider.Scroll -= ShuttleSlider_Scroll;

                    _positionLabel.Dispose();
                    _positionLabel = null;

                    _sliderLabel.Dispose();
                    _sliderLabel = null;
                }
            }
        }

        // Slider Preview
        private void SliderMousePreviewMenuItem_Click(object sender, EventArgs e)
        {
            SetSliderPreview(!sliderMousePreviewMenuItem.Checked);
        }

        private void SetSliderPreview(bool show)
        {
            if (show != sliderMousePreviewMenuItem.Checked)
            {
                Prefs.ShowSliderPreview = sliderMousePreviewMenuItem.Checked = show;
                if (show)
                {
                    if (!sp_Created) CreateSliderPreview(myPlayer, this);
                    if (myPlayer.Playing) StartSliderPreview();
                }
                else
                {
                    if (sp_Created) RemoveSliderPreview();
                }
            }
        }

        #endregion

        #region PositionSlider Mouse Wheel

        private void Sec12MenuItem_Click(object sender, EventArgs e)
        {
            SetSliderMouseWheel(500, true);
        }

        private void Sec1MenuItem_Click(object sender, EventArgs e)
        {
            SetSliderMouseWheel(1000, true);
        }

        private void Sec5MenuItem_Click(object sender, EventArgs e)
        {
            SetSliderMouseWheel(5000, true);
        }

        private void Sec10MenuItem_Click(object sender, EventArgs e)
        {
            SetSliderMouseWheel(10000, true);
        }

        private void Sec15MenuItem_Click(object sender, EventArgs e)
        {
            SetSliderMouseWheel(15000, true);
        }

        private void Sec30MenuItem_Click(object sender, EventArgs e)
        {
            SetSliderMouseWheel(30000, true);
        }

        private void Sec60MenuItem_Click(object sender, EventArgs e)
        {
            SetSliderMouseWheel(60000, true);
        }

        private void SecOffMenuItem_Click(object sender, EventArgs e)
        {
            SetSliderMouseWheel(0, false);
        }

        // this way because also called from set preferences
        private void SetSliderMouseWheel(int millisec, bool sliderFocus)
        {
            // these (context) menu item types are a mess...
            foreach (ToolStripItem item in mouseWheelMenuItem.DropDownItems)
            {
                if (!(item is ToolStripSeparator)) ((ToolStripMenuItem)item).Checked = false;
            }

            switch (millisec)
            {
                case 500:
                    sec12MenuItem.Checked = true;
                    break;

                case 5000:
                    sec5MenuItem.Checked = true;
                    break;

                case 10000:
                    sec10MenuItem.Checked = true;
                    break;

                case 15000:
                    sec15MenuItem.Checked = true;
                    break;

                case 30000:
                    sec30MenuItem.Checked = true;
                    break;

                case 60000:
                    sec60MenuItem.Checked = true;
                    break;

                case 0:
                    secOffMenuItem.Checked = true;
                    break;

                default: // case 1000
                    sec1MenuItem.Checked = true;
                    millisec = 1000;
                    break;
            }

            myPlayer.Sliders.Position.MouseWheel = millisec;
            mouseWheelMenuItem.Checked = millisec != 0;

            if (sliderFocus && myPlayer.Playing) positionSlider.Focus();
        }

        #endregion

        #region PositionSlider Mark / Goto

        // Mark Repeat StartPosition
        private void MarkStartPositionMenuItem_Click(object sender, EventArgs e)
        {
            // the player will ignore a startposition greater than the endposition, so:
            if (myPlayer.Media.StopTime > TimeSpan.Zero && myPlayer.Position.FromBegin > myPlayer.Media.StopTime) myPlayer.Media.StopTime = TimeSpan.Zero;

            myPlayer.Media.StartTime = myPlayer.Position.FromBegin;
            repeatOneMenuItem.PerformClick();
        }

        // Mark Repeat StopPosition
        private void MarkStopPositionMenuItem_Click(object sender, EventArgs e)
        {
            // the player will ignore a stopposition earlier than the startposition, so
            // (Position.FromBegin = current position)
            if (myPlayer.Media.StartTime > myPlayer.Position.FromBegin) myPlayer.Media.StartTime = TimeSpan.Zero;

            // set repeat one mode, if not already
            if (!repeatOneMenuItem.Checked) repeatOneMenuItem.PerformClick();

            // the player starts repeating automatically with
            myPlayer.Media.StopTime = myPlayer.Position.FromBegin;
        }

        // Go to Start Position
        private void GoToStartMenuItem_Click(object sender, EventArgs e)
        {
            myPlayer.Position.Rewind();
        }

        // Mark Position #1
        private void Mark1_MenuItem_Click(object sender, EventArgs e)
        {
            _mark1 = myPlayer.Position.FromStart;

            markPositionMenuItem.Checked = true;
            mark1_MenuItem.Checked = true;

            goToMarkMenuItem.Enabled = true;
            goToMarkMenuItem.Checked = true;

            goToMark1_MenuItem.Enabled = true;
            goToMark1_MenuItem.Text = "Ir a " + _mark1.ToString().Substring(0, 8);
            goToMark1_MenuItem.Checked = true;
        }

        // Mark Position #2
        private void Mark2_MenuItem_Click(object sender, EventArgs e)
        {
            _mark2 = myPlayer.Position.FromStart;

            markPositionMenuItem.Checked = true;
            mark2_MenuItem.Checked = true;

            goToMarkMenuItem.Enabled = true;
            goToMarkMenuItem.Checked = true;

            goToMark2_MenuItem.Enabled = true;
            goToMark2_MenuItem.Text = "Ir a " + _mark2.ToString().Substring(0, 8);
            goToMark2_MenuItem.Checked = true;
        }

        // Mark Position #3
        private void Mark3_MenuItem_Click(object sender, EventArgs e)
        {
            _mark3 = myPlayer.Position.FromStart;

            markPositionMenuItem.Checked = true;
            mark3_MenuItem.Checked = true;

            goToMarkMenuItem.Enabled = true;
            goToMarkMenuItem.Checked = true;

            goToMark3_MenuItem.Enabled = true;
            goToMark3_MenuItem.Text = "Ir a " + _mark3.ToString().Substring(0, 8);
            goToMark3_MenuItem.Checked = true;
        }

        // Mark Position #4
        private void Mark4_MenuItem_Click(object sender, EventArgs e)
        {
            _mark4 = myPlayer.Position.FromStart;

            markPositionMenuItem.Checked = true;
            mark4_MenuItem.Checked = true;

            goToMarkMenuItem.Enabled = true;
            goToMarkMenuItem.Checked = true;

            goToMark4_MenuItem.Enabled = true;
            goToMark4_MenuItem.Text = "Ir a " + _mark4.ToString().Substring(0, 8);
            goToMark4_MenuItem.Checked = true;
        }

        // Go to Mark Position #1
        private void GoToMark1_MenuItem_Click(object sender, EventArgs e)
        {
            if (myPlayer.Playing && _mark1 != TimeSpan.Zero)
                myPlayer.Position.FromStart = _mark1;
        }

        // Go to Mark Position #2
        private void GoToMark2_MenuItem_Click(object sender, EventArgs e)
        {
            if (myPlayer.Playing && _mark2 != TimeSpan.Zero)
                myPlayer.Position.FromStart = _mark2;
        }

        // Go to Mark Position #3
        private void GoToMark3_MenuItem_Click(object sender, EventArgs e)
        {
            if (myPlayer.Playing && _mark3 != TimeSpan.Zero)
                myPlayer.Position.FromStart = _mark3;
        }

        // Go to Mark Position #4
        private void GoToMark4_MenuItem_Click(object sender, EventArgs e)
        {
            if (myPlayer.Playing && _mark4 != TimeSpan.Zero)
                myPlayer.Position.FromStart = _mark4;
        }

        #endregion

        #region Positionslider Chapters

        private void AppleChaptersDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int index = chaptersAppleMenuItem.DropDown.Items.IndexOf(e.ClickedItem);
            if (myPlayer.Playing && _chapters_I != null && index < _chapters_I.Length)
            {
                positionSliderMenu.Close();
                myPlayer.Position.FromBegin = _chapters_I[index].StartTime;
            }
        }

        private void NeroChaptersDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int index = chaptersNeroMenuItem.DropDown.Items.IndexOf(e.ClickedItem);
            if (myPlayer.Playing && _chapters_II != null && index < _chapters_II.Length)
            {
                positionSliderMenu.Close();
                myPlayer.Position.FromBegin = _chapters_II[index].StartTime;
            }
        }

        private void FileChaptersDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int index = chaptersFileMenuItem.DropDown.Items.IndexOf(e.ClickedItem);
            if (myPlayer.Playing && _chapters_III != null && index < _chapters_III.Length)
            {
                positionSliderMenu.Close();
                if (_chapterMode) myPlayer.Chapters.Index = index;
                else myPlayer.Position.FromBegin = _chapters_III[index].StartTime;
            }
        }

        #endregion

        #endregion

        #region Position Slider Visiblity

        // The position slider itself is handled by the player
        // Here the option is added to show and hide it when the mouse moves on/off it
        // The slider and the 2 position labels are inside a panel that triggers the mouse enter/leave events

        // Show position slider and labels
        private void PositionSliderPanel_MouseEnter(object sender, EventArgs e)
        {
            if (!_sliderBlocked) SliderPanelShow();
        }

        // Hide position slider and labels
        private void PositionSliderPanel_MouseLeave(object sender, EventArgs e)
        {
            if (!_sliderBlocked && _sliderHidden && !positionSliderPanel.DisplayRectangle.Contains(positionSliderPanel.PointToClient(Cursor.Position)))
            {
                SliderPanelHide();
            }
        }

        // Show (the contents of) the position slider panel
        private void SliderPanelShow()
        {
            if (!_sliderVisible)
            {
                positionLabel1.Show();
                positionLabel2.Show();
                positionSlider.Show();
                _sliderVisible = true;
            }
        }

        // Hide (the contents of) the position slider panel
        private void SliderPanelHide()
        {
            if (_sliderVisible)
            {
                positionLabel1.Hide();
                positionLabel2.Hide();
                positionSlider.Hide();
                _sliderVisible = false;
            }
        }

        #endregion

        
        // ******************************** DisplayMenu Opening / Closing

        #region DisplayMenu Opening / Closing

        private void DisplayMenu_Opening(object sender, CancelEventArgs e)
        {
            myPlayer.CursorHide.Enabled = false;
        }

        private void DisplayMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            myPlayer.CursorHide.Enabled = true;
        }

        #endregion

        
        // ******************************** DisplayMenu Display Clones

        #region DisplayMenu Display Clones

        private void AddCloneMenuItem_Click(object sender, EventArgs e)
        {
            CloneWindows_Add();
        }

        private void ShowOverlayMenuItem_Click(object sender, EventArgs e)
        {
            myPlayer.DisplayClones.ShowOverlay = !myPlayer.DisplayClones.ShowOverlay;
            if (myPlayer.DisplayClones.ShowOverlay) showOverlayMenuItem.Text = "Ocultar";
            else showOverlayMenuItem.Text = "Mostrar";
        }

        private void Fps01MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 1);
        }

        private void Fps02MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 2);
        }

        private void Fps05MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 5);
        }

        private void Fps10MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 10);
        }

        private void Fps15MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 15);
        }

        private void Fps20MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 20);
        }

        private void Fps25MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 25);
        }

        private void Fps30MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 30);
        }

        private void Fps40MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 40);
        }

        private void Fps50MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 50);
        }

        private void Fps60MenuItem_Click(object sender, EventArgs e)
        {
            SetClonesFrameRate(sender, 60);
        }

        private void SetClonesFrameRate(object sender, int frameRate)
        {
            foreach (ToolStripMenuItem item in refreshRateMenuItem.DropDownItems)
            {
                item.Checked = false;
            }
            ((ToolStripMenuItem)(sender)).Checked = true;
            myPlayer.DisplayClones.FrameRate = frameRate;
        }

        private void RemoveAllClonesMenuItem_Click(object sender, EventArgs e)
        {
            CloneWindows_CloseAll();
        }

        #endregion

        
        // ******************************** DisplayMenu Preferences

        #region DisplayMenu Preferences

        private void PreferencesMenuItem_Click(object sender, EventArgs e)
        {
            PreferencesDialog prefsDialog = new PreferencesDialog(this, myPlayer);
            CenterDialog(this, prefsDialog);
            prefsDialog.ShowDialog();
            prefsDialog.Dispose();

            toolTip1.Active = Prefs.ShowTooltips;
        }

        #endregion

        
        // ******************************** DisplayMenu Quit

        #region DisplayMenu Quit

        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            Hide();
            Application.Exit();
        }

        #endregion

        
        // ******************************** PlayList Handling

        #region PlayList Handling

        // A simple playlist is used to store media filenames and URLs
        // It is saved as a textfile when changed and opened when the application starts

        // ******************************** PlayList New, Open, Add and Save As

        #region PlayList New, Open, Add and Save As

        // Ask and save current playlist with New, Open and Add PlayList
        private bool AskSaveCurrentPlayList(string text)
        {
            bool result = true;
            if (Prefs.PlayListChanged && Playlist.Count > 0)
            {
                PlaylistDialog playListDialog = new PlaylistDialog(text) { Text = APPLICATION_NAME };
                CenterDialog(this, playListDialog);
                DialogResult r = playListDialog.ShowDialog();
                playListDialog.Dispose();

                if (r == DialogResult.Yes) result = SavePlayListByUser();
                else if (r == DialogResult.Cancel) result = false;
            }
            return result;
        }

        // Clear playlist
        private void NewPlayList()
        {
            if (AskSaveCurrentPlayList("NEW PLAYLIST"))
            {
                myPlayer.Stop();
                _tempPlaylist = false;

                _mediaToPlay = 0;
                Playlist.Clear();

                ReBuildPlayListMenu();

                Prefs.PlayListChanged = false;
                Prefs.PlayListTitle = DEFAULT_PLAYLIST_TITLE;
                SetWindowTitle();

                SelectMediaFiles();
            }
        }

        // Open a playlist
        private void OpenPlayList()
        {
            if (AskSaveCurrentPlayList("OPEN PLAYLIST"))
            {
                OpenFileDialog2.Title = OPENPLAYLIST_DIALOG_TITLE;
                if (ModifierKeys == Keys.Shift || ModifierKeys == Keys.Control) OpenFileDialog2.InitialDirectory = Prefs.PlaylistsFolder;
                else OpenFileDialog2.InitialDirectory = PlayListDirectory;
                OpenFileDialog2.FileName = string.Empty;

                if (OpenFileDialog2.ShowDialog(this) == DialogResult.OK)
                {
                    Playlist.Clear();
                    try
                    {
                        PlayListDirectory = Path.GetDirectoryName(OpenFileDialog2.FileName);

                        //Playlist = TryParsePlayListFile(OpenFileDialog2.FileName);
                        Playlist.AddRange(myPlayer.Playlist.Open(OpenFileDialog2.FileName));
                        ReBuildPlayListMenu();

                        Prefs.PlayListChanged = false;
                        Prefs.PlayListTitle = Path.GetFileNameWithoutExtension(OpenFileDialog2.FileName);
                        //SetWindowTitle();

                        _mediaToPlay = 0;
                        SetWindowTitle();
                        if (Prefs.AutoPlayAdded) PlayNextMedia();
                        else myPlayer.Stop();
                    }
                    catch { }
                }
            }
        }

        // Add (merge) a playlist
        private void AddPlayList()
        {
            if (AskSaveCurrentPlayList("ADD PLAYLIST"))
            {
                OpenFileDialog2.Title = ADDPLAYLIST_DIALOG_TITLE;
                if (Control.ModifierKeys == Keys.Shift || Control.ModifierKeys == Keys.Control) OpenFileDialog2.InitialDirectory = Prefs.PlaylistsFolder;
                else OpenFileDialog2.InitialDirectory = PlayListDirectory;
                OpenFileDialog2.FileName = string.Empty;

                if (OpenFileDialog2.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        _tempPlaylist = false;
                        PlayListDirectory = Path.GetDirectoryName(OpenFileDialog2.FileName);

                        Playlist.AddRange(myPlayer.Playlist.Open(OpenFileDialog2.FileName));
                        ReBuildPlayListMenu();
                        Prefs.PlayListChanged = true;
                    }
                    catch { }
                }
            }
        }

        internal void AddToPlaylist(string fileName)
        {
            try
            {
                _tempPlaylist = false;
                PlayListDirectory = Path.GetDirectoryName(fileName);

                Playlist.AddRange(myPlayer.Playlist.Open(fileName));
                ReBuildPlayListMenu();
                Prefs.PlayListChanged = true;
            }
            catch { }
        }

        // Save playlist (by user)
        private bool SavePlayListByUser()
        {
            bool result = true;
            if (Playlist.Count > 0)
            {
                if (Control.ModifierKeys != Keys.Shift && Control.ModifierKeys != Keys.Control) SaveFileDialog1.InitialDirectory = Prefs.PlaylistsFolder;

                if (Prefs.PlayListTitle == DEFAULT_PLAYLIST_TITLE)
                {
                    Metadata tags = myPlayer.Media.GetMetadata(Playlist[0], ImageSource.None);
                    if (!string.IsNullOrEmpty(tags.Artist))
                    {
                        SaveFileDialog1.FileName = tags.Artist;
                        if (!string.IsNullOrEmpty(tags.Album)) SaveFileDialog1.FileName += " - " + tags.Album;
                    }
                    else if (!string.IsNullOrEmpty(tags.Album)) SaveFileDialog1.FileName = tags.Album;
                    else SaveFileDialog1.FileName = DEFAULT_PLAYLIST_TITLE;
                    tags.Dispose();
                }
                else
                {
                    SaveFileDialog1.FileName = Prefs.PlayListTitle;
                }

                if (SaveFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        if (string.Equals(Path.GetExtension(SaveFileDialog1.FileName), ".m3u8", StringComparison.OrdinalIgnoreCase))
                        {
                            File.WriteAllLines(SaveFileDialog1.FileName, Playlist.ToArray(), Encoding.UTF8);
                        }
                        else
                        {
                            File.WriteAllLines(SaveFileDialog1.FileName, Playlist.ToArray());
                        }
                        Prefs.PlayListChanged = false;
                        Prefs.PlayListTitle = Path.GetFileNameWithoutExtension(SaveFileDialog1.FileName);
                        if (!playButtonLight.LightOn) SetWindowTitle();
                        _tempPlaylist = false;
                    }
                    catch { }
                }
                else result = false;
            }
            return result;
        }

        // Read PlayList File (m3u, m3u8, ppl)
        //private List<string> TryParsePlayListFile(string fileName)
        //{
        //    bool validExtension = false;
        //    Encoding encoding = Encoding.Default;

        //    List<string> playList = new List<string>();

        //    string extension = Path.GetExtension(fileName);
        //    if (string.Equals(extension, ".ppl", StringComparison.OrdinalIgnoreCase)
        //        || (string.Equals(extension, ".m3u", StringComparison.OrdinalIgnoreCase))) validExtension = true;
        //    else if (string.Equals(extension, ".m3u8", StringComparison.OrdinalIgnoreCase))
        //    {
        //        validExtension = true;
        //        encoding = Encoding.UTF8;
        //    }

        //    if (validExtension)
        //    {
        //        StreamReader file = null;
        //        string playListPath = Path.GetDirectoryName(fileName);
        //        string line;

        //        try
        //        {
        //            if (encoding == Encoding.UTF8) file = new StreamReader(fileName, encoding);
        //            else file = new StreamReader(fileName); // something wrong with Encoding.Default?
        //            while ((line = file.ReadLine()) != null)
        //            {
        //                line = line.TrimStart();
        //                // skip if line is empty, #extm3u, #extinf info or # comment
        //                if (line != string.Empty && line[0] != '#')
        //                {
        //                    // get absolute path... (use other solution?)
        //                    // if (line[1] != ':' && !line.Contains(@"://")) line = Path.Combine(playListPath, line);
        //                    if (line[1] != ':' && !line.Contains(@"://")) line = Path.GetFullPath(Path.Combine(playListPath, line));
        //                    playList.Add(line);
        //                }
        //            }
        //        }
        //        catch { /* ignore */ }

        //        if (file != null) file.Close();
        //    }

        //    return playList;
        //}

        #endregion

        // ******************************** PlayList Add Media Files, Add URLs, Handle default PlayList, ReBuildPlayListMenu

        #region PlayList Add Media Files, Add URLs, Handle default PlayList, ReBuildPlayListMenu

        // Use an OpenFileDialog to select media files and add them to the PlayList
        internal void SelectMediaFiles()
        {
            OpenFileDialog1.Title = OPENMEDIA_DIALOG_TITLE;
            if (Control.ModifierKeys == Keys.Shift || Control.ModifierKeys == Keys.Control) OpenFileDialog1.InitialDirectory = Prefs.MediaFilesFolder;
            else OpenFileDialog1.InitialDirectory = MediaDirectory;
            OpenFileDialog1.FileName = string.Empty;

            if (OpenFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                _tempPlaylist = false;
                MediaDirectory = Path.GetDirectoryName(OpenFileDialog1.FileName);

                int newToPlay = Playlist.Count;
                AddToPlayList(OpenFileDialog1.FileNames);

                if (!myPlayer.Playing && Prefs.AutoPlayAdded)
                {
                    _mediaToPlay = newToPlay;
                    PlayNextMedia();
                }

                if (Prefs.PlayListTitle == DEFAULT_PLAYLIST_TITLE && Playlist != null && Playlist.Count > 0)
                {
                    Metadata tags = myPlayer.Media.GetMetadata(Playlist[0], ImageSource.None);
                    if (!string.IsNullOrEmpty(tags.Artist))
                    {
                        Prefs.PlayListTitle = tags.Artist;
                        if (!string.IsNullOrEmpty(tags.Album)) Prefs.PlayListTitle += " - " + tags.Album;
                    }
                    else if (!string.IsNullOrEmpty(tags.Album)) Prefs.PlayListTitle = tags.Album;
                    tags.Dispose();

                    if (!playButtonLight.LightOn) SetWindowTitle();
                }
            }
        }

        // Show the add URL dialog
        private void ShowAddUrlDialog()
        {
            _addUrlDialog = new AddUrlDialog(this, myPlayer) {Text = APPLICATION_NAME};
            CenterDialog(this, _addUrlDialog);

            _addUrlDialog.FormClosed += AddUrlDialog_FormClosed;

            _urlToPlay = Playlist.Count;
            _addUrlDialog.Show();

            //int newToPlay = Playlist.Count;
            //addUrlDialog.ShowDialog(this);
            //if (addUrlDialog.UrlAdded)
            //{
            //    _tempPlaylist = false;
            //    if (!Player1.Playing && Prefs.AutoPlayAdded)
            //    {
            //        _mediaToPlay = newToPlay;
            //        PlayNextMedia();
            //    }
            //}
            //addUrlDialog.Dispose();
        }

        private void AddUrlDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_urlAdded)
            {
                this.Activate();
                Application.DoEvents();

                _tempPlaylist = false;
                if (!myPlayer.Playing && Prefs.AutoPlayAdded)
                {
                    _mediaToPlay = _urlToPlay;
                    PlayNextMedia();
                }
            }
            _addUrlDialog.Dispose();
            _addUrlDialog = null;
        }

        // Add filenames to the PlayList
        internal void AddToPlayList(string[] fileNames)
        {
            string title;
            //int contextMenuCount = playMenu.Items.Count;
            // Add names to playlist and Play contextmenu and save the PlayList
            Playlist.AddRange(fileNames);
            for (int i = 0; i < fileNames.Length; i++)
            {
                //playMenu.Items.Add(Path.GetFileName(fileNames[i]));

                title = Path.GetFileName(fileNames[i]);
                if (string.IsNullOrWhiteSpace(title)) title = fileNames[i];
                playMenu.Items.Add(title);
            }
            ReBuildPlayListMenu();
            Prefs.PlayListChanged = true;
        }

        // Add the mediafilenames of the PlayList to the Play contextmenu
        internal void ReBuildPlayListMenu()
        {
            bool isAnUrl = false;
            playMenu.SuspendLayout();

            // remove items from menu (if any; the first 4 items ('Playlists', 'Add media files', 'Add URL' and a separator) stay in place)
            while (playMenu.Items.Count > START_PLAYITEMS) playMenu.Items.RemoveAt(START_PLAYITEMS);

            // Add playlist names to Play contextmenu
            for (int i = 0; i < Playlist.Count; i++)
            {
                if (Prefs.PlayListShowExtensions) playMenu.Items.Add(Path.GetFileName(Playlist[i].Replace("&", "&&")));
                else playMenu.Items.Add(Path.GetFileNameWithoutExtension(Playlist[i].Replace("&", "&&")));

                if (Path.GetExtension(Playlist[i]) == ".chap") playMenu.Items[i + START_PLAYITEMS].ForeColor = Color.DarkSeaGreen;
                else
                {
                    isAnUrl = false;
                    for (int j = 0; j < STREAMING_URLS.Length; j++)
                    {
                        if (Playlist[i].StartsWith(STREAMING_URLS[j], StringComparison.OrdinalIgnoreCase))
                        {
                            isAnUrl = true;
                            break;
                        }
                    }
                    if (isAnUrl) playMenu.Items[i + START_PLAYITEMS].ForeColor = Color.LightSteelBlue;
                }
                playMenu.Items[i + START_PLAYITEMS].MouseDown += PlayMenu_MouseDown; // TODO ?
            }

            // Restore checkmark
            if (myPlayer.Playing)
            {
                if (_mediaToPlay == 0) ((ToolStripMenuItem)playMenu.Items[START_PLAYITEMS]).Checked = true;
                else if ((_mediaToPlay + START_PLAYITEMS - 1) < playMenu.Items.Count) ((ToolStripMenuItem)playMenu.Items[_mediaToPlay + START_PLAYITEMS - 1]).Checked = true;
            }

            // Adjust playlist menu
            if (Playlist.Count > 0)
            {
                newPlayListMenuItem.Enabled = true;
                addPlayListMenuItem.Enabled = true;
                savePlayListMenuItem.Enabled = true;
            }
            else
            {
                newPlayListMenuItem.Enabled = false;
                addPlayListMenuItem.Enabled = false;
                savePlayListMenuItem.Enabled = false;
            }

            playMenu.ResumeLayout();

            // Save the (default) playlist
            SavePlayList();

            // Also rebuild the PiPOverlay playlist
            if (_pipOverlay != null) _pipOverlay.ReBuildPIPPlayListMenu();

            // and shufflelist
            if (RepeatShuffle) CreateShuffleList();
        }

        // Save the (default) PlayList to disk (used when the PlayList has changed)
        private void SavePlayList()
        {
            if (!_tempPlaylist)
            {
                try
                {
                    if (Playlist.Count > 0)
                    {
                        File.WriteAllLines(PlaylistFile, Playlist.ToArray());
                    }
                    else File.Delete(PlaylistFile);
                }
                catch { /* ignore */ }
            }
        }

        // Load the previous saved PlayList (used when the application starts)
        private void LoadPlayList()
        {
            if (File.Exists(PlaylistFile))
            {
                try
                {
                    Playlist = new List<string>(File.ReadAllLines(PlaylistFile));
                    ReBuildPlayListMenu();
                }
                catch { /* ignore */ }
            }
        }

        #endregion

        #endregion


        // ******************************** Utility Functions - Uncheck MenuItems / Center Dialog

        #region Utility Functions - Uncheck MenuItems / Center Dialog

        internal static void CheckMenuItems(ToolStrip theMenu, ToolStripMenuItem checkItem)
        {
            int count = theMenu.Items.Count;
            for (int i = 0; i < count; i++)
            {
                if (theMenu.Items[i] != null && theMenu.Items[i].GetType() == typeof(ToolStripMenuItem))
                {
                    if (checkItem == (ToolStripMenuItem)theMenu.Items[i]) ((ToolStripMenuItem)theMenu.Items[i]).Checked = true;
                    else ((ToolStripMenuItem)theMenu.Items[i]).Checked = false;
                }
            }
        }

        internal static void CheckMenuItems(ToolStrip theMenu, int first, int last, ToolStripMenuItem checkItem)
        {
            if (last == 0) last = theMenu.Items.Count;
            for (int i = first; i < last; i++)
            {
                if (theMenu.Items[i] != null && theMenu.Items[i].GetType() == typeof(ToolStripMenuItem))
                {
                    if (checkItem == (ToolStripMenuItem)theMenu.Items[i]) ((ToolStripMenuItem)theMenu.Items[i]).Checked = true;
                    else ((ToolStripMenuItem)theMenu.Items[i]).Checked = false;
                }
            }
        }

        internal static void UnCheckMenuItems(ToolStrip theMenu, int first, int last)
        {
            if (last == 0) last = theMenu.Items.Count;
            for (int i = first; i < last; i++)
            {
                if (theMenu.Items[i] != null && theMenu.Items[i].GetType() == typeof(ToolStripMenuItem))
                {
                    ((ToolStripMenuItem)theMenu.Items[i]).Checked = false;
                }
            }
        }

        internal static void CheckMenuItems(ContextMenuStrip theMenu, ToolStripMenuItem checkItem)
        {
            int count = theMenu.Items.Count;
            for (int i = 0; i < count; i++)
            {
                if (theMenu.Items[i] != null && theMenu.Items[i].GetType() == typeof(ToolStripMenuItem))
                {
                    if (checkItem == (ToolStripMenuItem)theMenu.Items[i]) ((ToolStripMenuItem)theMenu.Items[i]).Checked = true;
                    else ((ToolStripMenuItem)theMenu.Items[i]).Checked = false;
                }
            }
        }

        internal static void CheckMenuItems(ContextMenuStrip theMenu, int checkItem)
        {
            int count = theMenu.Items.Count;
            for (int i = 0; i < count; i++)
            {
                if (theMenu.Items[i] != null && theMenu.Items[i].GetType() == typeof(ToolStripMenuItem))
                {
                    if (i == checkItem) ((ToolStripMenuItem)theMenu.Items[i]).Checked = true;
                    else ((ToolStripMenuItem)theMenu.Items[i]).Checked = false;
                }
            }
        }

        internal static void CheckMenuItem(ContextMenuStrip theMenu, int first, int last, ToolStripMenuItem checkItem)
        {
            if (last == 0) last = theMenu.Items.Count;
            for (int i = first; i < last; i++)
            {
                if (theMenu.Items[i] != null && theMenu.Items[i].GetType() == typeof(ToolStripMenuItem))
                {
                    if (checkItem == (ToolStripMenuItem)theMenu.Items[i]) ((ToolStripMenuItem)theMenu.Items[i]).Checked = true;
                    else ((ToolStripMenuItem)theMenu.Items[i]).Checked = false;
                }
            }
        }

        internal static void UnCheckMenuItems(ContextMenuStrip theMenu, int first, int last)
        {
            if (last == 0) last = theMenu.Items.Count;
            for (int i = first; i < last; i++)
            {
                if (theMenu.Items[i] != null && theMenu.Items[i].GetType() == typeof(ToolStripMenuItem))
                {
                    ((ToolStripMenuItem)theMenu.Items[i]).Checked = false;
                }
            }
        }

        internal static void CenterDialog(Form baseForm, Form centerForm)
        {
            Rectangle r = Screen.GetWorkingArea(baseForm);

            centerForm.Left = baseForm.Left + ((baseForm.Width - centerForm.Width) / 2);
            if (centerForm.Left < r.Left) centerForm.Left = r.Left + 4;
            else if (centerForm.Left + centerForm.Width > r.Width) centerForm.Left = r.Width - centerForm.Width - 6;

            centerForm.Top = baseForm.Top + (baseForm.Height - centerForm.Height) / 2;
            if (centerForm.Top < r.Top) centerForm.Top = r.Top + 4;
            else if (centerForm.Top + centerForm.Height > r.Height) centerForm.Top = r.Height - centerForm.Height - 6;
        }

        #endregion

        #region Dialog Fade-In

        private const int       DIALOGFADE_INTERVAL = 10;
        private const double    DIALOGFADE_STEP     = 0.1;

        internal static System.Windows.Forms.Timer _dialogFadeTimer;
        internal static Form    _dialogFadeDialog;
        internal static double  _dialogFadeOpacity;
        internal static bool    _dialogFading = true; // set to true to disable fade option

        internal static void DialogFadeIn(Form dialog, double opacity)
        {
            if (_dialogFading)
            {
                Application.DoEvents();
                Thread.Sleep(50);
                dialog.Opacity = opacity;
            }
            else
            {
                _dialogFading               = true;
                _dialogFadeDialog           = dialog;
                _dialogFadeOpacity          = opacity;

				_dialogFadeTimer = new System.Windows.Forms.Timer
				{
					Interval = DIALOGFADE_INTERVAL
				};
				_dialogFadeTimer.Tick       += DialogFadeTimer_Tick;

                _dialogFadeTimer.Start();
            }
        }

		private static void DialogFadeTimer_Tick(object sender, EventArgs e)
		{
            _dialogFadeDialog.Opacity += DIALOGFADE_STEP;
            if (_dialogFadeDialog.Opacity >= _dialogFadeOpacity)
            {
                _dialogFadeDialog.Opacity = _dialogFadeOpacity;

                _dialogFadeTimer.Dispose();
                _dialogFadeTimer    = null;
                _dialogFadeDialog   = null;
                _dialogFading       = false;
            }
        }

        #endregion
    }
}
