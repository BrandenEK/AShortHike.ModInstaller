﻿using Blasphemous.Modding.Installer.Properties;

namespace Blasphemous.Modding.Installer;

internal class SettingsHandler
{
    public InstallerSettings Properties { get; private set; } = new();

    public void Save()
    {
        FormWindowState windowState = Core.UIHandler.WindowState;
        Rectangle windowBounds = Core.UIHandler.RestoreBounds;

        Settings.Default.Location = windowState == FormWindowState.Normal
            ? Core.UIHandler.Location
            : windowBounds.Location;

        Settings.Default.Size = windowState == FormWindowState.Normal
            ? Core.UIHandler.Size
            : windowBounds.Size;

        Settings.Default.Maximized = windowState == FormWindowState.Maximized;

        Settings.Default.Blas1RootFolder = Properties.Blas1RootFolder;
        Settings.Default.Blas2RootFolder = Properties.Blas2RootFolder;
        Settings.Default.HikeRootFolder = Properties.HikeRootFolder;

        Settings.Default.LastSection = (byte)Properties.CurrentSection;
        Settings.Default.Blas1ModSort = (byte)Properties.Blas1ModSort;
        Settings.Default.Blas1SkinSort = (byte)Properties.Blas1SkinSort;
        Settings.Default.Blas2ModSort = (byte)Properties.Blas2ModSort;
        Settings.Default.HikeModSort = (byte)Properties.HikeModSort;

        Settings.Default.Blas1ModTime = Properties.Blas1ModTime;
        Settings.Default.Blas1SkinTime = Properties.Blas1SkinTime;
        Settings.Default.Blas2ModTime = Properties.Blas2ModTime;
        Settings.Default.HikeModTime = Properties.HikeModTime;

        Settings.Default.Save();
    }

    public void Load()
    {
        Core.UIHandler.WindowState = Settings.Default.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
        Core.UIHandler.Location = Settings.Default.Location;
        Core.UIHandler.Size = Settings.Default.Size;

        Properties = new InstallerSettings()
        {
            Blas1RootFolder = Settings.Default.Blas1RootFolder,
            Blas2RootFolder = Settings.Default.Blas2RootFolder,
            HikeRootFolder = Settings.Default.HikeRootFolder,
            CurrentSection = (SectionType)Settings.Default.LastSection,
            Blas1ModSort = (SortType)Settings.Default.Blas1ModSort,
            Blas1SkinSort = (SortType)Settings.Default.Blas1SkinSort,
            Blas2ModSort = (SortType)Settings.Default.Blas2ModSort,
            HikeModSort = (SortType)Settings.Default.HikeModSort,
            Blas1ModTime = Settings.Default.Blas1ModTime,
            Blas1SkinTime = Settings.Default.Blas1SkinTime,
            Blas2ModTime = Settings.Default.Blas2ModTime,
            HikeModTime = Settings.Default.HikeModTime,
        };
    }
}
