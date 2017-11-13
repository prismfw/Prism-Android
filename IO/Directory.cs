/*
Copyright (C) 2017  Prism Framework Team

This file is part of the Prism Framework.

The Prism Framework is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

The Prism Framework is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/


using System.IO;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Prism.IO;
using Prism.Native;

using Environment = Android.OS.Environment;

namespace Prism.Android.IO
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeDirectory"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeDirectory), IsSingleton = true)]
    public class Directory : INativeDirectory
    {
        /// <summary>
        /// Gets the directory path to the folder that contains the application's bundled assets.
        /// </summary>
        public string AssetDirectoryPath
        {
            get { return "assets/"; }
        }

        /// <summary>
        /// Gets the directory path to a folder for storing persisted application data that is specific to the user.
        /// </summary>
        public string DataDirectoryPath
        {
            get { return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/"; }
        }

        /// <summary>
        /// Gets the character that is used to separate directories.
        /// </summary>
        public char SeparatorChar
        {
            get { return Path.DirectorySeparatorChar; }
        }

        /// <summary>
        /// Copies the directory from the source path to the destination path, including all subdirectories and files
        /// within it.
        /// </summary>
        /// <param name="sourceDirectoryPath">The path of the directory to be copied.</param>
        /// <param name="destinationDirectoryPath">The path to where the copied directory should be placed.</param>
        /// <param name="overwrite">Whether to overwrite any subdirectories or files at the destination path that have identical names to
        /// subdirectories or files at the source path.</param>
        public Task CopyAsync(string sourceDirectoryPath, string destinationDirectoryPath, bool overwrite)
        {
            return Task.Run(async () =>
            {
                if (!System.IO.Directory.Exists(destinationDirectoryPath))
                {
                    System.IO.Directory.CreateDirectory(destinationDirectoryPath);
                }

                var dirInfo = new System.IO.DirectoryInfo(sourceDirectoryPath);

                // Get the files in the directory and copy them to the new location.
                System.IO.FileInfo[] files = dirInfo.GetFiles();
                foreach (System.IO.FileInfo file in files)
                {
                    string tempPath = Path.Combine(destinationDirectoryPath, file.Name);
                    file.CopyTo(tempPath, overwrite);
                }
                    
                foreach (var subInfo in dirInfo.GetDirectories())
                {
                    string temppath = Path.Combine(destinationDirectoryPath, subInfo.Name);
                    await CopyAsync(subInfo.FullName, temppath, overwrite);
                }
            });
        }

        /// <summary>
        /// Creates a directory at the specified path.
        /// </summary>
        /// <param name="directoryPath">The path at which to create the directory.</param>
        public Task CreateAsync(string directoryPath)
        {
            return Task.Run(() =>
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            });
        }

        /// <summary>
        /// Deletes the directory at the specified path.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to delete.</param>
        /// <param name="recursive">Whether to delete all subdirectories and files within the directory.</param>
        public Task DeleteAsync(string directoryPath, bool recursive)
        {
            return Task.Run(() =>
            {
                System.IO.Directory.Delete(directoryPath, recursive);
            });
        }

        /// <summary>
        /// Gets the names of the subdirectories in the directory at the specified path,
        /// optionally getting the names of directories in any subdirectories as well.
        /// </summary>
        /// <param name="directoryPath">The path of the directory whose subdirectories are to be retrieved.</param>
        /// <param name="searchOption">A value indicating whether to search subdirectories or just the top directory.</param>
        /// <returns>The directories.</returns>
        public Task<string[]> GetDirectoriesAsync(string directoryPath, Prism.IO.SearchOption searchOption)
        {
            return Task.Run(() =>
            {
                return System.IO.Directory.GetDirectories(directoryPath, "*", searchOption == Prism.IO.SearchOption.AllDirectories ?
                    System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
            });
        }

        /// <summary>
        /// Gets the names of the files in the directory at the specified path,
        /// optionally getting the names of files in any subdirectories as well.
        /// </summary>
        /// <param name="directoryPath">The path of the directory whose files are to be retrieved.</param>
        /// <param name="searchOption">A value indicating whether to search subdirectories or just the top directory.</param>
        /// <returns>The files.</returns>
        public Task<string[]> GetFilesAsync(string directoryPath, Prism.IO.SearchOption searchOption)
        {
            return Task.Run(() =>
            {
                return System.IO.Directory.GetFiles(directoryPath, "*", searchOption == Prism.IO.SearchOption.AllDirectories ?
                    System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
            });
        }

        /// <summary>
        /// Gets the number of free bytes that are available on the drive that contains the directory at the specified path.
        /// </summary>
        /// <param name="directoryPath">The path of a directory on the drive.  If <c>null</c>, the current drive is used.</param>
        /// <returns>The free bytes.</returns>
        public Task<long> GetFreeBytesAsync(string directoryPath)
        {
            return Task.Run(() =>
            {
                var stats = new StatFs(directoryPath);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr2)
                {
                    return stats.AvailableBytes;
                }
                else
                {
#pragma warning disable 0618 // deprecated calls are for pre-API 18 devices
                    return stats.AvailableBlocks * stats.BlockSize;
#pragma warning restore 0618
                }
            });
        }

        /// <summary>
        /// Gets information about the specified system directory.
        /// </summary>
        /// <param name="directory">The system directory whose information is to be retrieved.</param>
        /// <returns>Information about the system directory.</returns>
        public async Task<INativeDirectoryInfo> GetSystemDirectoryInfoAsync(SystemDirectory directory)
        {
            return await Task.Run(() =>
            {
                switch (directory)
                {
                    case SystemDirectory.Assets:
                        return new DirectoryInfo(Path.GetDirectoryName(Application.MainActivity.PackageResourcePath));
                    case SystemDirectory.Local:
                        return new DirectoryInfo(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
                    case SystemDirectory.Shared:
                        return Application.MainActivity.ObbDir == null ? null : new DirectoryInfo(Application.MainActivity.ObbDir.AbsolutePath);
                    case SystemDirectory.Temp:
                        return new DirectoryInfo(Path.GetTempPath());
                    case SystemDirectory.External:
                        return new DirectoryInfo(Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat ?
                            Application.MainActivity.GetExternalFilesDirs(null) :
                            new[] { Application.MainActivity.GetExternalFilesDir(null) });
                    case SystemDirectory.Music:
                        return new DirectoryInfo(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryMusic).AbsolutePath);
                    case SystemDirectory.Photos:
                        return new DirectoryInfo(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures).AbsolutePath);
                    case SystemDirectory.Videos:
                        return new DirectoryInfo(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryMovies).AbsolutePath);
                    default:
                        // Extras that are not explicitly identified by the core framework library.
                        var folder = (System.Environment.SpecialFolder)(directory - System.Enum.GetValues(typeof(SystemDirectory)).Length);
                        return System.Enum.IsDefined(typeof(System.Environment.SpecialFolder), folder) ?
                            new DirectoryInfo(System.Environment.GetFolderPath(folder)) : null;
                }
            });
        }

        /// <summary>
        /// Gets the total number of bytes on the drive that contains the directory at the specified path.
        /// </summary>
        /// <param name="directoryPath">The path of a directory on the drive.  If <c>null</c>, the current drive is used.</param>
        /// <returns>The total bytes.</returns>
        public Task<long> GetTotalBytesAsync(string directoryPath)
        {
            return Task.Run(() =>
            {
                var stats = new StatFs(directoryPath);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr2)
                {
                    return stats.TotalBytes;
                }
                else
                {
#pragma warning disable 0618 // deprecated calls are for pre-API 18 devices
                    return stats.BlockCount * stats.BlockSize;
#pragma warning restore 0618
                }
            });
        }

        /// <summary>
        /// Moves the directory at the source path to the destination path.
        /// </summary>
        /// <param name="sourceDirectoryPath">The path of the directory to be moved.</param>
        /// <param name="destinationDirectoryPath">The path to where the directory should be moved.</param>
        /// <param name="overwrite">Whether to overwrite any subdirectories or files at the destination path that have identical names to
        /// subdirectories or files at the source path.</param>
        public Task MoveAsync(string sourceDirectoryPath, string destinationDirectoryPath, bool overwrite)
        {
            return Task.Run(async () =>
            {
                if (!System.IO.Directory.Exists(destinationDirectoryPath))
                {
                    System.IO.Directory.CreateDirectory(destinationDirectoryPath);
                }

                var dirInfo = new System.IO.DirectoryInfo(sourceDirectoryPath);

                // Get the files in the directory and copy them to the new location.
                System.IO.FileInfo[] files = dirInfo.GetFiles();
                foreach (System.IO.FileInfo file in files)
                {
                    string tempPath = Path.Combine(destinationDirectoryPath, file.Name);
                    if (overwrite || !System.IO.File.Exists(tempPath))
                    {
                        file.CopyTo(tempPath, overwrite);
                        file.Delete();
                    }

                }
                    
                foreach (var subInfo in dirInfo.GetDirectories())
                {
                    string temppath = Path.Combine(destinationDirectoryPath, subInfo.Name);
                    await MoveAsync(subInfo.FullName, temppath, overwrite);
                    subInfo.Delete();
                }
            });
        }
    }
}