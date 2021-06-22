#region Usings

using PVS.MediaPlayer;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

#endregion

namespace PVSPlayerExample
{
	public partial class VideoOverlayDialog : Form
	{

		// ******************************** Fields

		#region Fields

		private MainWindow			_baseForm;
		private Webcam_Window		_webcamForm;
		private Player				_basePlayer;

		private double				_oldOpacity;

		private OpenFileDialog		_openFileDialog;

		private string				_fileName;
		private Bitmap				_bitmap;
		private ImagePlacement		_placement				= ImagePlacement.Stretch;
		private int					_transparencySelection	= 0;
		private Color				_transparency			= Color.Empty;
		private float				_opacity				= 1;

		private bool				_isWebcam;
		private bool				_mouseMode;
		private bool				_isPNGType;

		private bool				_disposed;

		#endregion



		// ******************************** Main

		#region Main

		public VideoOverlayDialog(MainWindow baseForm, Webcam_Window webcamForm, Player basePlayer)
		{
			InitializeComponent();
			Icon = Properties.Resources.Media_Normal;

			_baseForm	= baseForm;
			_webcamForm = webcamForm;
			_isWebcam	= _webcamForm != null;
			if (_isWebcam) Text = webcamForm.Text + " Image Overlay";
			_basePlayer = basePlayer;

			// build placement menu
			string[] items = Enum.GetNames(typeof(ImagePlacement));
			for (int i = 0; i < items.Length; i++)
			{
				placementMenu.Items.Add(items[i]);
				if (items[i] == "Custom") placementMenu.Items[placementMenu.Items.Count - 1].Enabled = false;
				if (i == 3 || i == 6 || i == 9 || i == 12 || i == 15)
				{
					placementMenu.Items.Add("-");
					placementMenu.Items[placementMenu.Items.Count - 1].Enabled = false;
				}
			}
			((ToolStripMenuItem)placementMenu.Items[0]).Checked = true;

			_baseForm.DisableMenuSeparators(transparencyMenu);
			_baseForm.DisableMenuSeparators(displayMenu);

			_openFileDialog = new OpenFileDialog();
			_openFileDialog.Title = "Select a PVS.MediaPlayer Video Overlay";
			_openFileDialog.Filter = " Image files (*.*)|*.bmp; *.gif; *.heic; *.ico; *.jfif; *.jpeg; *.jpg; *.png; *.tif; *.tiff| All files|*.*";
			_openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

			Bitmap bitmap = null;
			if (_isWebcam)
			{
				try
				{
					bitmap = (Bitmap)_webcamForm._videoOverlay.Clone();
					if (bitmap != null)
					{
						_fileName = _webcamForm._videoOverlayFileName;
						SetBitmap(bitmap, _fileName);

						SetPlacementMenu(_webcamForm._videoOverlayPlacement);

						_transparency = _webcamForm._videoOverlayTransparency;
						SetTransparencyMenu(_webcamForm._videoOverlayMenuSelection);

						SetOpacityMenu(_webcamForm._videoOverlayOpacity);
					}
				}
				catch
				{
					if (bitmap != null) bitmap.Dispose();
				}
			}
			else if (_baseForm._videoOverlay != null)
			{
				try
				{
					bitmap = (Bitmap)_baseForm._videoOverlay.Clone();
					if (bitmap != null)
					{
						_fileName = _baseForm._videoOverlayFileName;
						SetBitmap(bitmap, _fileName);

						SetPlacementMenu(_baseForm._videoOverlayPlacement);

						_transparency = _baseForm._videoOverlayTransparency;
						SetTransparencyMenu(_baseForm._videoOverlayMenuSelection);

						SetOpacityMenu(_baseForm._videoOverlayOpacity);
					}
				}
				catch
				{
					if (bitmap != null) bitmap.Dispose();
				}
			}

			// Allow dropping media files on the form (handled in source file: DragDrop.cs):
			AllowDrop = true;

			_oldOpacity = Opacity;
			Opacity = 0;
		}

		private void BitmapOverlayDialog_Shown(object sender, System.EventArgs e)
		{
			MouseDown				+= DragForm_MouseDown;
			bitmapLabel.MouseDown	+= DragForm_MouseDown;
			bottomPanel.MouseDown	+= DragForm_MouseDown;
			pvsPanel.MouseDown		+= DragForm_MouseDown;

			if (_isWebcam) _basePlayer.Events.MediaEndedNotice += BasePlayer_MediaEndedNotice;

			MainWindow.DialogFadeIn(this, _oldOpacity);
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;
				if (disposing)
				{
					MouseDown				-= DragForm_MouseDown;
					bitmapLabel.MouseDown	-= DragForm_MouseDown;
					bottomPanel.MouseDown	-= DragForm_MouseDown;
					pvsPanel.MouseDown		-= DragForm_MouseDown;
					if (_isWebcam) _basePlayer.Events.MediaEndedNotice -= BasePlayer_MediaEndedNotice;

					if (_bitmap != null)			_bitmap.Dispose();
					if (_openFileDialog != null)	_openFileDialog.Dispose();

					_baseForm	= null;
					_webcamForm = null;
					_basePlayer	= null;

					if (components != null) components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion


		// ******************************** Event Handling

		#region Event Handling

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				if (_mouseMode) // transparency mouse selecting
				{
					MouseMode_End();
					return true;
				}
			}
			else if (keyData == (Keys.Control | Keys.V))
			{
				if (Clipboard.ContainsImage())
				{
					Bitmap bitmap = null;
					try
					{
						bitmap = (Bitmap)Clipboard.GetImage();
						string fileName = Path.Combine(MainWindow._appDataPath, string.Format("Image Paste {0:yyyy-MM-dd} at {0:HH-mm-ss}.png", DateTime.Now));
						bitmap.Save(fileName, ImageFormat.Png);
						SetBitmap(bitmap, fileName);
					}
					catch
					{
						if (bitmap != null) bitmap.Dispose();
					}
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Rectangle rect	= DisplayRectangle;
			rect.Width		-= 1;
			rect.Height		-= 1;
			Pen pen			= new Pen(Color.FromArgb(109, 103, 76), 1);
			e.Graphics.DrawRectangle(pen, rect);
			pen.Dispose();
		}

		private void PvsPanel_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawImage(Properties.Resources.PVSLogoOutline, 4, 0);
		}

		private void BasePlayer_MediaEndedNotice(object sender, EndedEventArgs e)
		{
			if (_bitmap != null)
			{
				_bitmap.Dispose();
				_bitmap = null;
			}
			Apply();
			Close();
		}

		#endregion


		// ******************************** Placement Menu

		#region Placement Menu

		private void PlacementMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			SetPlacementMenu((ImagePlacement)Enum.Parse(typeof(ImagePlacement), e.ClickedItem.Text));
		}

		private void SetPlacementMenu(ImagePlacement placement)
		{
			_placement = placement;
			placementLight.LightOn = _placement != ImagePlacement.Stretch;

			string placementName = _placement.ToString();
			toolTip1.SetToolTip(placementButton, placementName);

			int count = placementMenu.Items.Count;
			for (int i = 0; i < count; i++)
			{
				if (placementMenu.Items[i] != null && placementMenu.Items[i].GetType() == typeof(ToolStripMenuItem))
				{
					((ToolStripMenuItem)placementMenu.Items[i]).Checked = placementMenu.Items[i].Text == placementName;
				}
			}
		}

		#endregion


		// ******************************** Transparency Menu

		#region Transparency Menu

		// values -1 or 0 to 3
		private void SetTransparencyMenu(int selection)
		{
			bool enabled = _bitmap != null;

			fromDialogMenuItem.Enabled = enabled;
			fromMouseMenuItem.Enabled = enabled;
			fromScanMenuItem.Enabled = _isPNGType ? enabled : false;

			if (selection != -1)
			{
				_transparencySelection = selection;

				fromDialogMenuItem.Checked = false;
				fromMouseMenuItem.Checked = false;
				fromScanMenuItem.Checked = false;
				offMenuItem.Checked = false;

				if (selection == 0)
				{
					offMenuItem.Checked = true;
					_transparency = Color.Empty;
					transparencyColorPanel.BackColor = _transparency;

					toolTip1.SetToolTip(transparencyColorPanel, "Off");
					toolTip1.SetToolTip(transparencyButton, "Off");
				}
				else
				{
					toolTip1.SetToolTip(transparencyColorPanel, _transparency.ToString());
					toolTip1.SetToolTip(transparencyButton, _transparency.ToString());
					transparencyColorPanel.BackColor = Color.FromArgb(255, _transparency);

					if (selection == 1) fromDialogMenuItem.Checked = true;
					else if (selection == 2) fromMouseMenuItem.Checked = true;
					else fromScanMenuItem.Checked = true;
				}
			}
		}

		// Transparency Menu - Color From Color Dialog
		private void FromDialogMenuItem_Click(object sender, EventArgs e)
		{
			GetTransparencyDialog();
		}

		// Transparency Menu - Color From Mouse Selection
		private void FromMouseMenuItem_Click(object sender, EventArgs e)
		{
			MouseMode_Start();
		}

		// Transparency Menu - Color From Bitmap Scan
		private void FromScanMenuItem_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			GetTransparencyScan();
			Cursor = Cursors.Default;
		}

		// Transparency Menu - Off
		private void OffMenuItem_Click(object sender, EventArgs e)
		{
			SetTransparencyMenu(0);
		}

		// Select Transparency Color from Color Dialog
		private void GetTransparencyDialog()
		{
			if (_bitmap != null)
			{
				Enabled = false;

				ColorDialog dlg = new ColorDialog();
				dlg.Color = _transparency;
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					_transparency = dlg.Color;
					SetTransparencyMenu(1);
				}
				dlg.Dispose();

				Enabled = true;
			}
		}

		// Select Transparency Color with the Mouse Cursor
		private void MouseMode_Start()
		{
			if (_bitmap != null)
			{
				toolTip1.SetToolTip(bitmapPanel, string.Empty);

				Cursor.Clip = bitmapPanel.RectangleToScreen(bitmapPanel.ClientRectangle);
				bitmapPanel.Cursor = Cursors.Cross;

				bitmapPanel.MouseDown -= DragForm_MouseDown;
				bitmapPanel.MouseDown += MouseMode_Click;

				// Form background colors interfere with opacity < 1
				Opacity = 1;

				_mouseMode = true;
			}
		}

		private void MouseMode_Click(object sender, MouseEventArgs e)
		{
			MouseMode_End();

			if (e.Button == MouseButtons.Left)
			{
				try
				{
					Bitmap bitmap = new Bitmap(1, 1);
					Graphics g = Graphics.FromImage(bitmap);
					g.CopyFromScreen(bitmapPanel.PointToScreen(e.Location), new Point(0, 0), new Size(1, 1));
					_transparency = bitmap.GetPixel(0, 0);
					// TODO 255, 17, 18, 19 = backcolor of panel with bitmap, no idea what 0, 211, 211, 211 is, found it by blendin image without transparency
					if (_transparency == Color.FromArgb(255, 17, 18, 19)) _transparency = Color.LightGray; // Color.FromArgb(0, 211, 211, 211);
					g.Dispose();
					bitmap.Dispose();

					SetTransparencyMenu(2);
				}
				catch { /* ignored */ }
			}

			Opacity = _oldOpacity;
		}

		private void MouseMode_End()
		{
			_mouseMode = false;

			bitmapPanel.MouseDown -= MouseMode_Click;
			bitmapPanel.MouseDown += DragForm_MouseDown;

			Cursor.Clip = Rectangle.Empty;
			bitmapPanel.Cursor = Cursors.Default;

			toolTip1.SetToolTip(bitmapPanel, _fileName);
		}

		// Select Transparency Color with Bitmap Scan
		private void GetTransparencyScan()
		{
			if (_bitmap != null && _isPNGType)
			{
				Color color;
				bool found = false;

				for (int i = 0; i < _bitmap.Height && !found; i++)
				{
					for (int j = 0; j < _bitmap.Width; j++)
					{
						color = _bitmap.GetPixel(j, i);
						if (color.A == 0)
						{
							//_transparency = color;
							// TODO find real background color transparency
							_transparency = Color.LightGray; // Color.FromArgb(0, 211, 211, 211);
							found = true;
							break;
						}
					}
				}

				if (!found)
				{
					Enabled = false;

					SetTransparencyMenu(0);

					ErrorDialog errorDialog = new ErrorDialog("Video Image Overlay", "IMAGE SCAN:\r\n\r\n" + _fileName + "\r\n\r\nThe image does not contain a transparency color key.", false, true);
					errorDialog.checkBox1.Hide();
					errorDialog.checkBox2.Hide();
					MainWindow.CenterDialog(this, errorDialog);
					errorDialog.ShowDialog(this);
					errorDialog.Dispose();

					Enabled = true;
				}
				else
				{
					SetTransparencyMenu(3);
				}
			}
		}

		#endregion


		// ******************************** Opacity Menu

		#region Opacity Menu

		private void OpacityMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			SetOpacityMenu(((opacityMenu.Items.IndexOf(e.ClickedItem)) + 1) / 10f);
		}

		// values 0.0 to 1.0
		private void SetOpacityMenu(float opacity)
		{
			if (opacity < 0.1f) opacity = 0.1f;
			else if (opacity > 1) opacity = 1;

			_opacity = opacity;
			opacityLight.LightOn = _opacity != 1;

			int count = opacityMenu.Items.Count;
			for (int i = 0; i < count; i++)
			{
				((ToolStripMenuItem)opacityMenu.Items[i]).Checked = false;
			}

			int index = (int)(opacity * 10) - 1;
			((ToolStripMenuItem)opacityMenu.Items[index]).Checked = true;
			toolTip1.SetToolTip(opacityButton, opacityMenu.Items[index].Text);
		}

		#endregion


		// ******************************** Display Context Menu

		#region Display Context Menu

		private void DisplayMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			if (_bitmap != null)
			{
				int index = displayMenu.Items.IndexOf(e.ClickedItem);
				bool doRotateFlip = true;
				switch (index)
				{
					case 0: // Flip X
						_bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
						break;
					case 1: // Flip Y
						_bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
						break;

					case 2: // Flip XY
						_bitmap.RotateFlip(RotateFlipType.RotateNoneFlipXY);
						break;

					case 4: // Rotate 90
						_bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
						break;

					case 5: // Rotate 270
						_bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
						break;

					case 6: // Rotate 290
						_bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
						break;

					case 8: // Sepia
						if (_bitmap != null) _bitmap = ReColorBitmap(_bitmap, 2);
						else doRotateFlip = false;
						break;

					case 9: // Inverse
						if (_bitmap != null) _bitmap = ReColorBitmap(_bitmap, 3);
						else doRotateFlip = false;
						break;

					case 10: // Grayscale
						if (_bitmap != null) _bitmap = ReColorBitmap(_bitmap, 1);
						else doRotateFlip = false;
						break;

					case 11: // Black & White
						if (_bitmap != null) _bitmap = ReColorBitmap(_bitmap, 4);
						else doRotateFlip = false;
						break;

					case 13: // Restore Original Bitmap
						if (_bitmap != null && !string.IsNullOrWhiteSpace(_fileName))
						{
							try
							{
								Bitmap bitmap = new Bitmap(_fileName);
								SetBitmap(bitmap, _fileName);
							}
							catch { /* ignored */ }
						}
						doRotateFlip = false;
						break;

					default: doRotateFlip = false;
						break;
				}

				if (doRotateFlip)
				{
					bitmapPanel.BackgroundImage = null;
					bitmapPanel.BackgroundImage = _bitmap;
				}
			}
		}


		#endregion


		// ******************************** Settings

		#region Settings

		private void SetBitmap(Bitmap bitmap, string fileName)
		{
			if (bitmap != null)
			{
				if (_bitmap != null) _bitmap.Dispose();
				_bitmap = bitmap;
				_fileName = fileName;

				bitmapPanel.MouseDown -= DragForm_MouseDown; // is this event removed by background image?
				bitmapLabel.Hide();

				bitmapPanel.BackgroundImage = _bitmap;
				clearButton.Enabled = true;

				bitmapPanel.MouseDown += DragForm_MouseDown;
				toolTip1.SetToolTip(bitmapPanel, _fileName);

				string extension = Path.GetExtension(_fileName);
				_isPNGType = (string.Compare(extension, ".png", true) == 0 ||
					string.Compare(extension, ".gif", true) == 0 ||
					string.Compare(extension, ".ico", true) == 0 ||
					string.Compare(extension, ".heic", true) == 0);

				SetTransparencyMenu(0);
				bitmapPanel.ContextMenuStrip = displayMenu;
			}
		}

		private void RemoveBitmap()
		{
			bitmapPanel.BackgroundImage = null;
			clearButton.Enabled = false;

			toolTip1.SetToolTip(bitmapPanel, string.Empty);
			bitmapLabel.Show();

			_fileName =  string.Empty;
			if (_bitmap != null)
			{
				_bitmap.Dispose();
				_bitmap = null;
			}

			_isPNGType = false;
			SetTransparencyMenu(0);
			bitmapPanel.ContextMenuStrip = null;
		}

		private void ShowBitmapErrorDialog(string fileName)
		{
			ErrorDialog errorDialog = new ErrorDialog("Video Image Overlay", "OPEN IMAGE:\r\n\r\n" + fileName + "\r\n\r\nThe selected file is not a valid image file, or its format is not currently supported.", false, true);
			errorDialog.checkBox1.Hide();
			errorDialog.checkBox2.Hide();
			MainWindow.CenterDialog(this, errorDialog);
			errorDialog.ShowDialog(this);
			errorDialog.Dispose();
		}

		#endregion


		// ******************************** Buttons Handling

		#region Buttons Handling

		private void OpenButton_Click(object sender, EventArgs e)
		{
			_openFileDialog.FileName = string.Empty;
			if (_openFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				Bitmap temp = null;
				Bitmap bitmap = null;

				_openFileDialog.InitialDirectory = string.Empty;
				Enabled = false;
				try
				{
					temp = new Bitmap(_openFileDialog.FileName);
					bitmap = new Bitmap(temp);
					SetBitmap(bitmap, _openFileDialog.FileName);
				}
				catch
				{
					if (bitmap != null) bitmap.Dispose();
					ShowBitmapErrorDialog(_openFileDialog.FileName);
				}
				if (temp != null) temp.Dispose();
				Enabled = true;
			}
			okButton.Focus();
		}

		private void RemoveButton_Click(object sender, EventArgs e)
		{
			RemoveBitmap();
			okButton.Focus();
		}

		private void ApplyButton_Click(object sender, EventArgs e)
		{
			Apply();
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void OkButton_Click(object sender, EventArgs e)
		{
			Apply();
			Close();
		}

		private void Apply()
		{
			if (_isWebcam)
			{
				if (_bitmap == null)
				{
					if (_webcamForm._videoOverlay != null)
					{
						_webcamForm._videoOverlay.Dispose();
						_webcamForm._videoOverlay = null;
					}
					_webcamForm._videoOverlayFileName = string.Empty;
					_webcamForm._videoOverlayPlacement = ImagePlacement.Stretch;
					_webcamForm._videoOverlayTransparency = Color.Empty;
					_webcamForm._videoOverlayMenuSelection = 0;
					_webcamForm._videoOverlayOpacity = 1;

					_basePlayer.Video.RemoveOverlay(); // the player checks if there is a bitmap overlay
				}
				else
				{
					if (_webcamForm._videoOverlay != null) _webcamForm._videoOverlay.Dispose();
					_webcamForm._videoOverlay = (Bitmap)_bitmap.Clone();
					_webcamForm._videoOverlayFileName = _fileName;
					_webcamForm._videoOverlayPlacement = _placement;
					_webcamForm._videoOverlayTransparency = _transparency;
					_webcamForm._videoOverlayMenuSelection = _transparencySelection;
					_webcamForm._videoOverlayOpacity = _opacity;

					//_basePlayer.Video.SetOverlay(_bitmap, _placement, RectangleF.Empty, _transparency, _opacity, true);
					_basePlayer.Video.SetOverlay(_bitmap, _placement, _transparency, _opacity);
				}
			}
			else
			{
				if (_bitmap == null)
				{
					if (_baseForm._videoOverlay != null)
					{
						_baseForm._videoOverlay.Dispose();
						_baseForm._videoOverlay				= null;
					}
					_baseForm._videoOverlayFileName			= string.Empty;
					_baseForm._videoOverlayPlacement		= ImagePlacement.Stretch;
					_baseForm._videoOverlayTransparency		= Color.Empty;
					_baseForm._videoOverlayMenuSelection	= 0;
					_baseForm._videoOverlayOpacity			= 1;

					_basePlayer.Video.RemoveOverlay(); // the player checks if there is a bitmap overlay
				}
				else
				{
					if (_baseForm._videoOverlay != null) _baseForm._videoOverlay.Dispose();
					_baseForm._videoOverlay					= (Bitmap)_bitmap.Clone();
					_baseForm._videoOverlayFileName			= _fileName;
					_baseForm._videoOverlayPlacement		= _placement;
					_baseForm._videoOverlayTransparency		= _transparency;
					_baseForm._videoOverlayMenuSelection	= _transparencySelection;
					_baseForm._videoOverlayOpacity			= _opacity;

					_basePlayer.Video.SetOverlay(_bitmap, _placement, RectangleF.Empty, _transparency, _opacity, true);
				}
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


		// ******************************** Drag and Drop

		#region Drag and Drop

		private string _bitmapExtensions = ".bmp.gif.heic.ico.jfif.jpeg.jpg.png.tif.tiff";

		protected override void OnDragEnter(DragEventArgs e)
		{
			e.Effect = DragDropEffects.None;
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] dragFiles = (string[])e.Data.GetData(DataFormats.FileDrop, false);
				for (int i = 0; i < dragFiles.Length; i++)
				{
					string extension = Path.GetExtension(dragFiles[i]);
					if (string.Equals(extension, ".lnk", StringComparison.OrdinalIgnoreCase))
					{
						extension = Path.GetExtension(_baseForm.ResolveShortcut(dragFiles[i]));
					}
					if (!string.IsNullOrWhiteSpace(extension) && _bitmapExtensions.IndexOf(extension, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						e.Effect = DragDropEffects.Link; // allow drop if there's at least one valid media file
						break;
					}
				}
			}
			_basePlayer.DragAndDrop.DragEnter(e);
		}

		protected override void OnDragLeave(EventArgs e)
		{
			_basePlayer.DragAndDrop.DragLeave();
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			_basePlayer.DragAndDrop.DragOver(e);
		}

		protected override void OnDragDrop(DragEventArgs e)
		{
			_basePlayer.DragAndDrop.DragDrop(e);

			bool fileFound = false;
			int index = 0;

			string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop, false);
			for (int i = 0; i < dropFiles.Length; i++)
			{
				string extension = Path.GetExtension(dropFiles[i]);
				if (string.Equals(extension, ".lnk", StringComparison.OrdinalIgnoreCase))
				{
					dropFiles[i] = _baseForm.ResolveShortcut(dropFiles[i]);
					extension = Path.GetExtension(dropFiles[i]);
				}
				if (!string.IsNullOrWhiteSpace(extension) && _bitmapExtensions.IndexOf(extension, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					index = i;
					fileFound = true;
					break; // only one bitmap
				}
			}

			if (fileFound)
			{
				Bitmap bitmap = null;
				try
				{
					bitmap = new Bitmap(dropFiles[index]);
					SetBitmap(bitmap, dropFiles[index]);
				}
				catch
				{
					if (bitmap != null) bitmap.Dispose();
					ShowBitmapErrorDialog(dropFiles[index]);
				}
			}
		}

		#endregion


		// ******************************** Recolor Bitmap

		#region Recolor Bitmap

		// Method by and thanks to: Brandon Cannaday, Stephen Toub and others
		private Bitmap ReColorBitmap(Bitmap original, int color)
		{
			ColorMatrix colorMatrix;

			// create a new bitmap the same size as original
			Bitmap newBitmap = new Bitmap(original.Width, original.Height);

			// get a graphics object from the new image
			Graphics g = Graphics.FromImage(newBitmap);

			switch (color)
			{
				case 1: // Grayscale - Matrix of type float
					colorMatrix = new ColorMatrix(new[]
					{
						new[] {.3f, .3f, .3f, 0, 0},
						new[] {.59f, .59f, .59f, 0, 0},
						new[] {.11f, .11f, .11f, 0, 0},
						new[] {0f, 0f, 0f, 1f, 0f},
						new[] {0f, 0f, 0f, 0f, 1f}
					});
					break;

				case 2: // Sepia - Matrix of type float
					colorMatrix = new ColorMatrix(new[]
					{
						new[] {.393f, .349f, .272f, 0, 0},
						new[] {.769f, .686f, .534f, 0, 0},
						new[] {.189f, .168f, .131f, 0, 0},
						new[] {0f, 0f, 0f, 1f, 0f},
						new[] {0f, 0f, 0f, 0f, 1f}
					});
					break;

				case 3: // Inverse - Matrix of type float
					colorMatrix = new ColorMatrix(new[]
					{
						new[] {-1f, 0f, 0f, 0f, 0f},
						new[] {0f, -1f, 0f, 0f, 0f},
						new[] {0f, 0f, -1f, 0f, 0f},
						new[] {0f, 0f, 0f, 1f, 0f},
						new[] {1f, 1f, 1f, 0f, 1f}
					});
					break;

				default: // Black & White - Matrix of type float
					colorMatrix = new ColorMatrix(new[]
					{
						new[] {1.5f, 1.5f, 1.5f, 0f, 0f},
						new[] {1.5f, 1.5f, 1.5f, 0f, 0f},
						new[] {1.5f, 1.5f, 1.5f, 0f, 0f},
						new[] {0f, 0f, 0f, 1f, 0f},
						new[] {-1f, -1f, -1f, 0f, 1f}
					});
					break;
			}

			// create some image attributes
			ImageAttributes attributes = new ImageAttributes();

			// set the color matrix attribute
			attributes.SetColorMatrix(colorMatrix);

			// draw the original image on the new image using the color matrix
			g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
			   0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

			// dispose the Graphics object
			g.Dispose();

			// dispose the original bitmap
			original.Dispose();

			return newBitmap;
		}

		#endregion

	}
}
