#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using System.Runtime.InteropServices;

#endregion

namespace PVSPlayerExample
{
    // Drag and Drop
    //
    // These Drag and Drop methods allow files (with certain extensions (including playlists .m3u and .m3u8)) to be dropped on the main Form
    // (not just the player's display) and added to the playlist.
    // Some of the Display Overlays pass on their Drag an Drop handling to these methods.

    public partial class MainWindow
    {
        // Lists of supported audio and video extensions
        //private string _mediaExtensions = ".3g2.3gp.3gp2.3gpp.aac.adts.asf.avi.m4a.m4v.mkv.mov.mp3.mp4.sami.smi.wav.wma.wmv";
        private string _mediaExtensions = ".m3u.m3u8.3g2.3gp.3gp2.3gpp.aac.adts.asf.avi.m2ts.m4a.m4v.mkv.mov.mp3.mp4.mpeg.mpg.mts.sami.smi.vob.wav.webm.wma.wmv.bmp.gif.ico.jfif.jpeg.jpg.png.tif.tiff.chap";

        protected override void OnDragEnter(DragEventArgs e)
        {
            DoOnDragEnter(e);
        }
        
        // also used by some display overlays
        internal void DoOnDragEnter(DragEventArgs e)
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
                        extension = Path.GetExtension(ResolveShortcut(dragFiles[i]));
                    }
                    if (!string.IsNullOrWhiteSpace(extension) && _mediaExtensions.IndexOf(extension, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        e.Effect = DragDropEffects.Link; // allow drop if there's at least one valid media file
                        break;
                    }
                }
            }
            myPlayer.DragAndDrop.DragEnter(e);
        }

        protected override void OnDragLeave(EventArgs e)
        {
            DoOnDragLeave();
        }

		// also used by some display overlays
		internal void DoOnDragLeave()
		{
			myPlayer.DragAndDrop.DragLeave();
		}

		protected override void OnDragOver(DragEventArgs e)
		{
            DoOnDragOver(e);
        }

        // also used by some display overlays
        internal void DoOnDragOver(DragEventArgs e)
        {
            myPlayer.DragAndDrop.DragOver(e);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            DoOnDragDrop(e);
        }

        // also used by some display overlays
        internal void DoOnDragDrop(DragEventArgs e)
        {
            myPlayer.DragAndDrop.DragDrop(e);

            int newToPlay = Playlist.Count;
            bool filesAdded = false;

            string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            List<string> newFiles = new List<string>(dropFiles.Length);

            for (int i = 0; i < dropFiles.Length; i++)
            {
                string extension = Path.GetExtension(dropFiles[i]);
                if (string.Equals(extension, ".m3u", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".m3u8", StringComparison.OrdinalIgnoreCase))
                {
                    // add playlist
                    AddToPlaylist(dropFiles[i]);
                    filesAdded = true;
                }
                else
                {
                    if (string.Equals(extension, ".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        dropFiles[i] = ResolveShortcut(dropFiles[i]);
                        extension = Path.GetExtension(dropFiles[i]);
                    }
                    if (!string.IsNullOrWhiteSpace(extension) && _mediaExtensions.IndexOf(extension, StringComparison.OrdinalIgnoreCase) >= 2)
                    {
                        // add media file
                        newFiles.Add(dropFiles[i]);
                        filesAdded = true;
                    }
                }
            }

            if (filesAdded)
            {
                if (newFiles.Count > 0) AddToPlayList(newFiles.ToArray());

                if (!myPlayer.Playing && Prefs.AutoPlayAdded)
                {
                    _mediaToPlay = newToPlay;
                    PlayNextMedia();
                }
            }
        }

        // taken from: https://blez.wordpress.com/2013/02/18/get-file-shortcuts-target-with-c/
        internal string ResolveShortcut(string fileName)
        {
            //if (!String.Equals(Path.GetExtension(fileName), ".lnk", StringComparison.OrdinalIgnoreCase)) return string.Empty;
            try
            {
                FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read);
                using (BinaryReader fileReader = new BinaryReader(fileStream))
                {
                    fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                    uint flags = fileReader.ReadUInt32();        // Read flags
                    if ((flags & 1) == 1)
                    {
                        // Bit 1 set means we have to skip the shell item ID list
                        fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                        uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                        fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                    }

                    long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                    // structure begins
                    uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                    fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                    uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                    // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                    fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                    // base pathname (target)
                    long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                    // the base pathname. I don't need the 2 terminating nulls.
                    char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                    string link = new string(linkTarget);

                    int begin = link.IndexOf("\0\0", StringComparison.Ordinal);
                    if (begin > -1)
                    {
                        int end = link.IndexOf("\\\\", begin + 2, StringComparison.Ordinal) + 2;
                        end = link.IndexOf('\0', end) + 1;

                        string firstPart = link.Substring(0, begin);
                        string secondPart = link.Substring(end);

                        return firstPart + secondPart;
                    }
                    return link;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

    }
}
