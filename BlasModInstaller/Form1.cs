﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace BlasModInstaller
{
    public partial class MainForm : Form
    {
        private string SavedModsPath => Environment.CurrentDirectory + "\\downloads\\mods.json";
        private string DownloadsPath => Environment.CurrentDirectory + "\\downloads\\";
        private string ConfigPath => Environment.CurrentDirectory + "\\installer.cfg";

        public static MainForm Instance { get; private set; }

        private Config config;
        public static string BlasRootFolder => Instance.config.BlasRootFolder;

        private Octokit.GitHubClient github;
        private List<Mod> mods;

        public MainForm()
        {
            Directory.CreateDirectory(DownloadsPath);
            if (Instance == null)
                Instance = this;
            LoadConfig();

            InitializeComponent();
            CreateGithubClient();
            
            LoadModsFromJson();
            LoadModsFromWeb();
        }

        private void LoadModsFromJson()
        {
            if (File.Exists(SavedModsPath))
            {
                string json = File.ReadAllText(SavedModsPath);
                mods = JsonConvert.DeserializeObject<List<Mod>>(json);

                for (int i = 0; i < mods.Count; i++)
                    mods[i].UI.CreateUI(modHolder, Width, i);
            }
            else
            {
                mods = new List<Mod>();
            }
        }

        private async Task LoadModsFromWeb()
        {
            using (HttpClient client = new HttpClient())
            {
                string json = await client.GetStringAsync("https://raw.githubusercontent.com/BrandenEK/Blasphemous-Mod-Installer/main/mods.json");
                Mod[] webMods = JsonConvert.DeserializeObject<Mod[]>(json);

                foreach (Mod webMod in webMods)
                {                    
                    Octokit.Release latestRelease = await github.Repository.Release.GetLatest(webMod.GithubAuthor, webMod.GithubRepo);
                    Version webVersion = new Version(latestRelease.TagName);

                    if (ModExists(webMod.Name, out Mod localMod))
                    {
                        localMod.CopyData(webMod);

                        bool displayUpdate = false;
                        if (localMod.Installed)
                        {
                            Version localVersion = new Version(localMod.Version);
                            displayUpdate = webVersion.CompareTo(localVersion) > 0;
                        }
                        else
                        {
                            localMod.Version = webVersion.ToString();
                        }
                        localMod.UI.UpdateUI(displayUpdate);
                    }
                    else
                    {
                        webMod.Version = webVersion.ToString();
                        mods.Add(webMod);
                        webMod.UI.CreateUI(modHolder, Width, mods.Count - 1);
                    }
                }
            }

            SaveMods();
        }

        // After loading more mods from web or updating version, need to save new json
        public void SaveMods()
        {
            File.WriteAllText(SavedModsPath, JsonConvert.SerializeObject(mods));
        }

        private bool ModExists(string name, out Mod foundMod)
        {
            foundMod = null;
            foreach (Mod mod in mods)
            {
                if (name == mod.Name)
                {
                    foundMod = mod;
                    return true;
                }
            }
            return false;
        }

        private void CreateGithubClient()
        {
            github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("BlasModInstaller"));

            string tokenPath = Environment.CurrentDirectory + "\\token.txt";
            if (File.Exists(tokenPath))
            {
                string token = File.ReadAllText(tokenPath);
                github.Credentials = new Octokit.Credentials(token);
            }
        }

        private void ClickedDebug(object sender, EventArgs e)
        {
            fakePanel.Visible = !fakePanel.Visible;
        }

        public async Task Download(Mod mod)
        {
            if (BlasRootFolder == null) return;

            mod.UI.DisplayDownloadBar();

            Octokit.Release latestRelease = await github.Repository.Release.GetLatest(mod.GithubAuthor, mod.GithubRepo);
            string newVersion = latestRelease.TagName;
            string downloadUrl = latestRelease.Assets[0].BrowserDownloadUrl;
            string downloadPath = $"{DownloadsPath}{mod.Name.Replace(' ', '_')}_{newVersion}.zip";

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => 
                {
                    BeginInvoke(new MethodInvoker(() => mod.UI.UpdateDownloadBar(e.ProgressPercentage)));
                };
                client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                {
                    BeginInvoke(new MethodInvoker(() => mod.InstallMod(newVersion, downloadPath)));
                };
                client.DownloadFileAsync(new Uri(downloadUrl), downloadPath);
            }
        }

        private void LoadConfig()
        {
            if (File.Exists(ConfigPath))
            {
                // Load config
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }
            else
            {
                // Create new config
                config = new Config();
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        private void ChooseBlasLocation(object sender, EventArgs e)
        {
            if (blasLocDialog.ShowDialog() == DialogResult.OK)
            {
                string text = blasLocDialog.FileName;
                int exeIdx = text.IndexOf("Blasphemous.exe");
                if (exeIdx >= 0 && File.Exists(text))
                {
                    config.BlasRootFolder = text.Substring(0, exeIdx - 1);
                    SaveConfig();
                    // Remove fake panel & load mods
                }
            }
        }
    }
}
