using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using WMPLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Net.Http.Headers;
using System.Windows;
using TagLib.Mpeg;

namespace MusicMaster
{
    public partial class Form1 : Form
    {
        //declare a initial few variables
        WindowsMediaPlayer player = new WindowsMediaPlayer();
        bool start = false;
        string[] musicFiles;
        int currentMusicIndex = 0;
        string musicName;
        string musicmake;
        string musicdisplay;
        string musicFolderPathdefault = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        string musicFolderPath;
        bool playing = false;
        int skipdelay;
        bool skipable = true;
        public Form1()
        {
            // InitializeComponent needs to be first
            InitializeComponent();
            // Do some prep stuff
            player.settings.volume = Decimal.ToInt32(Volume.Value);
            NowPlaying.Text = "Now Playing: Nothing";
            MusicFolder.Text = musicFolderPathdefault;
            musicFolderPath = musicFolderPathdefault;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versiontxt = "V" + version;
            /*var versiontxt2 = "V" + version.Major + "." + version.Minor + " (build " + version.Build + ")";*/
            label3.Text = versiontxt;
            imgload();
            GetLatestRelease();
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }
        //make the statupscreen change pic end go away 
        private async Task imgload()
        {

            await Task.Delay(2000); // Wait for 2 seconds
            StartPic.Image = MusicMaster.Properties.Resources.musicmaster;
            await Task.Delay(2000); // Wait for 2 seconds
            StartPic.Visible = false;
        }
        //let it stay-no function
        private void MusicFolder_TextChanged(object sender, EventArgs e)
        {

        }
        //get musicFolderPath
        private void MusicFolderConfirm_Click(object sender, EventArgs e)
        {
            using (var MusicFolderloc = new FolderBrowserDialog())
            {
                DialogResult result = MusicFolderloc.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(MusicFolderloc.SelectedPath))
                {
                    musicFolderPath = MusicFolderloc.SelectedPath;
                    MusicFolder.Text = musicFolderPath;
                }
            }
        }
        //play button
        private void PlayButton_Click(object sender, EventArgs e)
        {
            //check if music has been started in this instance
            if (start == true)
            {
                player.controls.play();
                NowPlaying.Text = "Now Playing: " + musicName;
                playing = true;
                Task.Run(async () =>
                {
                    while (playing == true)
                    {
                        //update music time
                        UpdateMusicTimeDisplay();
                        // wiat 200ms
                        await Task.Delay(200);
                    }
                });
            }
            else
            {
                //check if the musicFolderPath has been confirmed
                if (musicFolderPath != null)
                {
                    musicFiles = Directory.GetFiles(musicFolderPath, "*.mp3", SearchOption.AllDirectories);
                    // check if there are more than 0 soundfiles in the folder
                    if (musicFiles.Length > 0)
                    {
                        IWMPPlaylist playlist = player.playlistCollection.newPlaylist("Music");

                        foreach (string musicFile in musicFiles)
                        {
                            IWMPMedia media = player.newMedia(musicFile);
                            playlist.appendItem(media);
                        }

                        player.currentPlaylist = playlist;
                        player.controls.play();
                        player.PlayStateChange += Player_PlayStateChange;
                        start = true;
                        /*musicName = Path.GetFileNameWithoutExtension(player.controls.currentItem.sourceURL);*/
                        musicName = player.currentMedia.getItemInfo("Title");
                        musicmake = player.currentMedia.getItemInfo("Artist");
                        musicdisplay = musicName + " - " + musicmake;
                        NowPlaying.Text = "Now Playing: " + musicdisplay;
                        //display album cover
                        currentMusicIndex = player.currentPlaylist.count;
                        UpdateAlbumCover();

                        UpdateMusicTotalTimeDisplay();
                        playing = true;
                        //run task to update musictime
                        Task.Run(async () =>
                        {
                            while (playing == true)
                            {
                                //update music time
                                UpdateMusicTimeDisplay();
                                // wiat 200ms
                                await Task.Delay(200);
                            }
                        });
                    }
                    else
                    {
                        MusicFolder.Text = "Geen Muziek gevonden";
                    }
                }
                else
                {
                    MusicFolder.Text = "Geen MuziekFolder gevonden";
                }
            }
            label6.Visible = true;
            label6.Text = playing.ToString();
        }
        //change volume
        private void Volume_ValueChanged(object sender, EventArgs e)
        {
            player.settings.volume = Decimal.ToInt32(Volume.Value);
            MuteIndicator.Text = "Unmuted";
            if (Decimal.ToInt32(Volume.Value) == 0)
            {
                player.settings.volume = Decimal.ToInt32(Volume.Value);
                MuteIndicator.Text = "Muted";
            }
        }
        //pause button
        private void Pause_Click(object sender, EventArgs e)
        {
            musicName = player.currentMedia.getItemInfo("Title");
            musicmake = player.currentMedia.getItemInfo("Artist");
            musicdisplay = musicName + " - " + musicmake;
            NowPlaying.Text = "Now Paused: " + musicdisplay;
            player.controls.pause();
            playing = false;
            label6.Visible = true;
            label6.Text = playing.ToString();
        }
        //stop button
        private void Stop_Click(object sender, EventArgs e)
        {
            NowPlaying.Text = "Now Playing: Nothing";
            player.controls.stop();
            start = false;
            playing = false;
            AlbumCover.Image = null;
        }
        // go 1 musicfile back
        private void Back_Click(object sender, EventArgs e)
        {
            if (playing == false)
            {

            }
            else
            {
                if (player.currentPlaylist != null)
                {
                    // Check if current track is the first track in the playlist
                    if (player.controls.currentItem == player.currentPlaylist.Item[0])
                    {
                        // Loop back to the last track
                        player.controls.currentItem = player.currentPlaylist.Item[player.currentPlaylist.count - 1];
                    }
                    else
                    {
                        // Go to the previous track
                        player.controls.previous();
                    }
                    currentMusicIndex = player.currentPlaylist.count;

                    // Update label with current track information
                    musicName = player.currentMedia.getItemInfo("Title");
                    musicmake = player.currentMedia.getItemInfo("Artist");
                    musicdisplay = musicName + " - " + musicmake;
                    NowPlaying.Text = "Now Playing: " + musicdisplay;
                    //display album cover
                    UpdateAlbumCover();
                    SkipRegulator();
                }
            }
        }
        //skip 1 file
        private void Skip_Click(object sender, EventArgs e)
        {
            if (playing == false)
            {

            }
            else
            {
                if (player.currentPlaylist != null)
                {
                    // Check if current track is the last track in the playlist
                    if (player.controls.currentItem == player.currentPlaylist.Item[player.currentPlaylist.count - 1])
                    {
                        // Loop back to the first track
                        player.controls.currentItem = player.currentPlaylist.Item[0];
                    }
                    else
                    {
                        // Go to the next track
                        player.controls.next();
                    }
                    currentMusicIndex = player.currentPlaylist.count;


                    // Update label with current track information
                    /*                musicName = Path.GetFileNameWithoutExtension(player.controls.currentItem.sourceURL);*/
                    musicName = player.currentMedia.getItemInfo("Title");
                    musicmake = player.currentMedia.getItemInfo("Artist");
                    musicdisplay = musicName + " - " + musicmake;
                    //display album cover
                    UpdateAlbumCover();
                    SkipRegulator();
                }
            }
        }
        //Mute button
        private void Mute_Click(object sender, EventArgs e)
        {
            player.settings.volume = 0;
            MuteIndicator.Text = "Muted";

        }
        //unmute button
        private void Unmute_Click(object sender, EventArgs e)
        {
            player.settings.volume = Decimal.ToInt32(Volume.Value);
            MuteIndicator.Text = "Unmuted";
        }
        //let it stay-no function
        private void NowPlaying_Click(object sender, EventArgs e)
        {

        }
        //let it stay-no function
        private void MuteIndicator_Click(object sender, EventArgs e)
        {

        }
        //Detect if a new musicfile has started
        private void Player_PlayStateChange(int NewState)
        {
            // Check if the new state is "playing"
            if ((WMPPlayState)NewState == WMPPlayState.wmppsPlaying)
            {
                // Update the NowPlaying label with the name of the current music file
                /*musicName = Path.GetFileNameWithoutExtension(player.controls.currentItem.sourceURL);*/
                musicName = player.currentMedia.getItemInfo("Title");
                musicmake = player.currentMedia.getItemInfo("Artist");
                musicdisplay = musicName + " - " + musicmake;
                NowPlaying.Text = "Now Playing: " + musicdisplay;
                //display album cover
                UpdateAlbumCover();

                UpdateMusicTotalTimeDisplay();
            }
        }

        private void StartPic_Click(object sender, EventArgs e)
        {

        }
        //let it stay-no function
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        // Update the music time display
        private void UpdateMusicTimeDisplay()
        {
            // Get the current position of the playing music
            double currentPosition = player.controls.currentPosition;

            // Format the position as a time string
            string musicTimeText = TimeSpan.FromSeconds(currentPosition).ToString(@"mm\:ss");

            // Update the musicTime label with the current position
            musictime.Text = musicTimeText;
        }
        // Update the toal time
        private void UpdateMusicTotalTimeDisplay()
        {
            var MusicTotalTIme = player.currentMedia.duration;
            string MusicTotalTImes = TimeSpan.FromSeconds(MusicTotalTIme).ToString(@"mm\:ss");
            musictimetot.Text = MusicTotalTImes;
        }
        //update album cover
        private void UpdateAlbumCover()
        {
            string currentMusicFile = player.currentMedia.sourceURL;

            if (System.IO.File.Exists(currentMusicFile))
            {
                var file = TagLib.File.Create(currentMusicFile);

                if (file.Tag.Pictures.Length >= 1)
                {
                    var picture = file.Tag.Pictures[0];
                    var memoryStream = new MemoryStream(picture.Data.Data);
                    AlbumCover.Image = Image.FromStream(memoryStream);
                    AlbumCover.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else
                {
                    // Clear the PictureBox if no album cover is available
                    AlbumCover.Image = Properties.Resources.DefaultAlbumCover;
                    AlbumCover.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
        }
        //10sec skip
        private void button1_Click(object sender, EventArgs e)
        {
            var newpositien = player.controls.currentPosition + 10;
            player.controls.currentPosition = newpositien;
        }
        //10sec back
        private void button2_Click(object sender, EventArgs e)
        {
            var newpositien = player.controls.currentPosition - 10;
            player.controls.currentPosition = newpositien;
        }
        //let it stay-no function
        private void musictime_Click(object sender, EventArgs e)
        {

        }

        private void Albumcover_Click(object sender, EventArgs e)
        {

        }
        //github version checker
        private async Task GetLatestRelease()
        {
            string apiUrl = "https://api.github.com/repos/Pascal-Benink/MusicMaster/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "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");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 11.0; Windows NT 10.0; WOW64; Trident/7.0)");

                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string currentgithubversion = "v1.1.5.0";
                        string json = await response.Content.ReadAsStringAsync();
                        dynamic release = JsonConvert.DeserializeObject(json);
                        string tagName = release.tag_name;
                        /*label6.Text = "Latest Release Tag: " + tagName;*/
                        if (tagName == currentgithubversion)
                        {

                        }
                        else
                        {
                            NewVersion.Visible = true;
                            NewVersion.Text = $"Version {tagName} of MusicMaster Is Out Click Here To Download" +
                                $" You Rurrent version is {currentgithubversion}";
                            button3.Visible = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    label6.Visible = true;
                    label6.Text = "An error occurred: " + ex.Message;
                }
            }
        }

        private void NewVersion_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/Pascal-Benink/MusicMaster/releases/latest";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NewVersion.Visible = false;
            button3.Visible = false;
        }
        //Regulate skippablitiy to let everything load after each skip
        private async Task SkipRegulator()
        {
            for (int i = 3; i > 0; i--)
            {
                Skip.Enabled = false;
                Back.Enabled = false;
                await Task.Delay(200);
                skipable = false;
            }
            Skip.Enabled = true;
            Back.Enabled = true;
            skipable = true;
        }
        //Keybinds
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.MediaPlayPause)
            {
                //chenk if currently is playing
                if (playing == false)
                {
                    PlayButton_Click(sender, e);
                }
                else if (playing == true)
                {
                    Pause_Click(sender, e);
                }
            }
            if (e.KeyCode == Keys.MediaNextTrack)
            {
                //chenk if currently is playing
                if (skipable == true)
                {
                    Skip_Click(sender, e);
                    skipable = false;
                }
                
            }
            if (e.KeyCode == Keys.MediaPreviousTrack)
            {
                //chenk if currently is playing
                if (skipable == true)
                {
                    Back_Click(sender, e);
                    skipable = false;
                }   
            }
        }
    }
}
