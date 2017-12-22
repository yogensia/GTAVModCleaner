using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;

namespace GTAVModCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set initial value of GTAV Installation directory field
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            string GTAVLocation = FindByDisplayName(regKey, "Grand Theft Auto V");
            TextBox_GTAVDirectory.Text = GTAVLocation;
            Console.WriteLine("Detected GTAV Installation folder in: '" + GTAVLocation + "'.");

            // Set initial value of Mods directory field
            string MODLocation = Path.GetDirectoryName(GTAVLocation);
            MODLocation = Path.Combine(MODLocation, "Grand Theft Auto V Mods BAK");
            TextBox_ModsDirectory.Text = MODLocation;
            Console.WriteLine("Auto generating Mods folder in: '" + MODLocation + "'.");
        }

        // Returns installation folder of an application
        static string FindByDisplayName(RegistryKey parentKey, string name)
        {
            string[] nameList = parentKey.GetSubKeyNames();
            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey regKey = parentKey.OpenSubKey(nameList[i]);
                try
                {
                    if (regKey.GetValue("DisplayName").ToString() == name)
                    {
                        return regKey.GetValue("InstallLocation").ToString();
                    }
                }
                catch { }
            }
            return "";
        } // FindByDisplayName

        private void Button_GTAVDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Set GTAV Installation Directory",
                IsFolderPicker = true,
                InitialDirectory = TextBox_GTAVDirectory.Text,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = TextBox_GTAVDirectory.Text,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TextBox_GTAVDirectory.Text = dlg.FileName;
                Console.WriteLine("GTAV Installation directory changed to: '" + dlg.FileName + "'.");
            }
            /*
            OpenFileDialog GTAVDirectory_FileDialog = new OpenFileDialog();
            if (GTAVDirectory_FileDialog.ShowDialog() == true)
                TextBox_GTAVDirectory.Text = System.IO.Path.GetDirectoryName(GTAVDirectory_FileDialog.FileName);
            */
        }

        private void Button_ModsDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Set Mods Backup Directory",
                IsFolderPicker = true,
                InitialDirectory = TextBox_ModsDirectory.Text,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = TextBox_ModsDirectory.Text,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TextBox_ModsDirectory.Text = dlg.FileName;
                Console.WriteLine("Mods Backup directory changed to: '" + dlg.FileName + "'.");
            }
            /*
            OpenFileDialog ModsDirectory_FileDialog = new OpenFileDialog();
            if (ModsDirectory_FileDialog.ShowDialog() == true)
                TextBox_ModsDirectory.Text = System.IO.Path.GetDirectoryName(ModsDirectory_FileDialog.FileName);
            */
        }

        public void Button_CleanMods_Click(object sender, RoutedEventArgs e)
        {
            // Get paths from text fields
            string GTAVLocation = TextBox_GTAVDirectory.Text;
            string MODLocation = TextBox_ModsDirectory.Text;

            // Define known files/folders that we won't move when cleaning the GTAV folder
            string[] knownFiles = {
                "ReadMe",
                "update",
                "x64",
                "bink2w64.dll",
                "commandline.txt",
                "common.rpf",
                "d3dcompiler_46.dll",
                "d3dcsx_46.dll",
                "GFSDK_ShadowLib.win64.dll",
                "GFSDK_TXAA.win64.dll",
                "GFSDK_TXAA_AlphaResolve.win64.dll",
                "GPUPerfAPIDX11-x64.dll",
                "GTA5.exe",
                "GTAVLauncher.exe",
                "PlayGTAV.exe",
                "NvPmApi.Core.win64.dll",
                "version.txt",
                "steam_appid.txt",
                "x64a.rpf",
                "x64b.rpf",
                "x64c.rpf",
                "x64d.rpf",
                "x64e.rpf",
                "x64f.rpf",
                "x64g.rpf",
                "x64h.rpf",
                "x64i.rpf",
                "x64j.rpf",
                "x64k.rpf",
                "x64l.rpf",
                "x64m.rpf",
                "x64n.rpf",
                "x64o.rpf",
                "x64p.rpf",
                "x64q.rpf",
                "x64r.rpf",
                "x64s.rpf",
                "x64t.rpf",
                "x64u.rpf",
                "x64v.rpf",
                "x64w.rpf"
            };

            // Check that MODs Backup folder doesn't already exist
            if (!Directory.Exists(MODLocation))
            {
                Directory.CreateDirectory(MODLocation);
            }

            // Confirm that MODs Backup folder is empty, if not empty warn user
            if (Directory.EnumerateFileSystemEntries(MODLocation).Any())
            {
                Console.WriteLine("Selected Mods Backup directory is NOT Empty.");
                string message_notEmpty, caption_notEmpty;

                message_notEmpty = "Mod Backup directory is not empty! Overriding of old mod backups may ocurr. Are you sure?";
                message_notEmpty = string.Format(message_notEmpty, MODLocation);
                caption_notEmpty = "Whoops!";

                MessageBoxButton buttons_notEmpty = MessageBoxButton.YesNo;

                // Show Confirm Dialog
                if (MessageBox.Show(message_notEmpty, caption_notEmpty, buttons_notEmpty) == MessageBoxResult.No)
                {
                    // User said NO, so do nothing
                    Console.WriteLine("Operation aborted by user.");
                    goto DoNothing;
                }
                Console.WriteLine("User confirmed to continue on non-empty directory.");
            }
            else
            {
                Console.WriteLine("Selected Mods Backup directory is empty, continuing...");
            }

            // Configure Confirm Dialog
            string message_confirm, caption_confirm;

            message_confirm = "Mod files in \"{0}\" will be moved to \"{1}\". Are you sure?";
            message_confirm = string.Format(message_confirm, GTAVLocation, MODLocation);
            caption_confirm = "We're about to clean stuff!";

            MessageBoxButton buttons_confirm = MessageBoxButton.YesNo;

            // Show Confirm Dialog
            if (MessageBox.Show(message_confirm, caption_confirm, buttons_confirm) == MessageBoxResult.Yes)
            {
                // Clone arrays so we can build origin and destination paths
                string[] GTAVLocationFiles = (string[])knownFiles.Clone();

                // Merge paths and filenames
                for (var i = 0; i < knownFiles.Length; i++)
                {
                    GTAVLocationFiles[i] = Path.Combine(GTAVLocation, knownFiles[i]);
                }

                // Check directory for actual files
                string[] DetectedFiles = Directory.GetFiles(GTAVLocation);

                // Compare arrays and move mods (unknown files) to backup folder
                int movedFiles = 0;

                foreach (var item in DetectedFiles)
                {
                    int pos = Array.IndexOf(GTAVLocationFiles, item);

                    if (pos == -1)
                    {
                        // Get destination path
                        string newItem = Path.Combine(MODLocation, Path.GetFileName(item));

                        // Counter to know how many files were moved
                        movedFiles++;

                        Console.WriteLine("Moving unkown file: '" + item + "' to '" + newItem + "'.");
                        try
                        {
                            // Try to move
                            File.Move(item, newItem);
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine("Something went wrong: " + err.ToString());
                        }
                    }
                }

                // Check how many files were moved
                Console.WriteLine(movedFiles + " files were moved.");
            }
            else
            {
                Console.WriteLine("Operation aborted by user.");
            }// if user confirm

            DoNothing:;
            Console.WriteLine("End of operation.");
        } // Button_CleanMods_Click
    } // class MainWindow
} // namespace GTAVModCleaner
