using NotepadEx.Properties;
using NotepadEx.Util;
using System.Collections.Generic;
using System.Linq;

namespace NotepadEx.Util
{
    public static class RecentFileManager
    {
        private const int MaxRecentsToTrack = 50;

        /// <summary>
        /// Atomically adds a file path to the recent files list, ensuring process safety.
        /// </summary>
        /// <param name="filePath">The file path to add.</param>
        public static void AddFile(string filePath)
        {
            if(string.IsNullOrEmpty(filePath))
            {
                return;
            }

            // Perform the entire read-modify-write operation within a single system-wide lock.
            ProcessSync.RunSynchronized(() =>
            {
                // Step 1: Read the latest list directly from settings.
                var recentFiles = GetRecentFilesFromSettings();

                // Step 2: Modify the list.
                // Remove the item if it already exists to move it to the top.
                recentFiles.Remove(filePath);

                // Add the new item to the top of the list.
                recentFiles.Insert(0, filePath);

                // Trim the list if it's too long.
                if(recentFiles.Count > MaxRecentsToTrack)
                {
                    recentFiles.RemoveAt(recentFiles.Count - 1);
                }

                // Step 3: Write the modified list back to settings and save.
                Settings.Default.RecentFiles = string.Join(",", recentFiles);
                Settings.Default.Save();
            });
        }

        /// <summary>
        /// Atomically retrieves the current list of recent files.
        /// </summary>
        /// <returns>A list of recent file paths.</returns>
        public static List<string> GetRecentFiles()
        {
            List<string> recentFiles = new List<string>();
            ProcessSync.RunSynchronized(() =>
            {
                recentFiles = GetRecentFilesFromSettings();
            });
            return recentFiles;
        }

        /// <summary>
        /// A private helper to read and parse the recent files string from settings.
        /// This should only be called from within a synchronized context.
        /// </summary>
        private static List<string> GetRecentFilesFromSettings()
        {
            string recentFilesString = Settings.Default.RecentFiles;
            if(!string.IsNullOrEmpty(recentFilesString))
            {
                return recentFilesString.Split(',').ToList();
            }
            return new List<string>();
        }
    }
}