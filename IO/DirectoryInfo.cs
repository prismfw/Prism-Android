/*
Copyright (C) 2018  Prism Framework Team

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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.IO;
using Prism.Native;

namespace Prism.Android.IO
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeDirectoryInfo"/>.
    /// </summary>
    [Register(typeof(INativeDirectoryInfo))]
    public class DirectoryInfo : INativeDirectoryInfo
    {
        /// <summary>
        /// Gets or sets the attributes of the directory.
        /// </summary>
        public FileAttributes Attributes
        {
            get { return (FileAttributes)(info?.Attributes ?? 0); }
            set
            {
                if (info != null)
                {
                    info.Attributes = (System.IO.FileAttributes)value;
                }
            }
        }

        /// <summary>
        /// Gets the date and time that the directory was created.
        /// </summary>
        public DateTime CreationTime
        {
            get { return info?.CreationTime ?? invalidDateTime.ToLocalTime(); }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the directory was created.
        /// </summary>
        public DateTime CreationTimeUtc
        {
            get { return info?.CreationTimeUtc ?? invalidDateTime; }
        }

        /// <summary>
        /// Gets a value indicating whether the directory exists.
        /// </summary>
        public bool Exists
        {
            get { return info?.Exists ?? false; }
        }

        /// <summary>
        /// Gets the date and time that the directory was last accessed.
        /// </summary>
        public DateTime LastAccessTime
        {
            get { return info?.LastAccessTime ?? invalidDateTime.ToLocalTime(); }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the directory was last accessed.
        /// </summary>
        public DateTime LastAccessTimeUtc
        {
            get { return info?.LastAccessTimeUtc ?? invalidDateTime; }
        }

        /// <summary>
        /// Gets the date and time that the directory was last modified.
        /// </summary>
        public DateTime LastWriteTime
        {
            get { return info?.LastWriteTime ?? invalidDateTime.ToLocalTime(); }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the directory was last modified.
        /// </summary>
        public DateTime LastWriteTimeUtc
        {
            get { return info?.LastWriteTimeUtc ?? invalidDateTime; }
        }

        /// <summary>
        /// Gets the name of the directory.
        /// </summary>
        public string Name
        {
            get { return info?.Name ?? string.Empty; }
        }

        /// <summary>
        /// Gets the full path to the directory.
        /// </summary>
        public string Path
        {
            get { return info?.FullName ?? string.Empty; }
        }

        private static readonly DateTime invalidDateTime = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly System.IO.DirectoryInfo info;
        private readonly IEnumerable<System.IO.DirectoryInfo> subdirectories;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryInfo"/> class.
        /// </summary>
        /// <param name="directoryPath">The path to the directory.</param>
        public DirectoryInfo(string directoryPath)
            : this(new System.IO.DirectoryInfo(directoryPath))
        {
        }

        internal DirectoryInfo(System.IO.DirectoryInfo info)
        {
            this.info = info;
        }

        internal DirectoryInfo(Java.IO.File[] infos)
        {
            subdirectories = infos.GroupBy(f => System.IO.Path.GetPathRoot(f.AbsolutePath))
                .Select(fg => new System.IO.DirectoryInfo(fg.First().AbsolutePath));
        }

        /// <summary>
        /// Gets information about the subdirectories within the current directory,
        /// optionally getting information about directories in any subdirectories as well.
        /// </summary>
        /// <param name="searchOption">A value indicating whether to search subdirectories or just the top directory.</param>
        /// <returns>An <see cref="Array"/> containing the directory information.</returns>
        public Task<INativeDirectoryInfo[]> GetDirectoriesAsync(SearchOption searchOption)
        {
            return Task.Run<INativeDirectoryInfo[]>(() =>
            {
                if (info == null)
                {
                    // Ignoring search pattern for this case.  Should only happen for External "virtual" directory.
                    var directories = subdirectories;
                    if (searchOption == SearchOption.AllDirectories)
                    {
                        directories = directories.Concat(directories.SelectMany(d =>
                            d.EnumerateFileSystemInfos("*", System.IO.SearchOption.AllDirectories))
                            .OfType<System.IO.DirectoryInfo>());
                    }

                    return directories.Select(d => new DirectoryInfo(d)).ToArray();
                }

                return info.GetFileSystemInfos("*", searchOption == SearchOption.AllDirectories ?
                    System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly)
                    .OfType<System.IO.DirectoryInfo>().Select(d => new DirectoryInfo(d)).ToArray();
            });
        }

        /// <summary>
        /// Gets information about the files in the current directory,
        /// optionally getting information about files in any subdirectories as well.
        /// </summary>
        /// <param name="searchOption">A value indicating whether to search subdirectories or just the top directory.</param>
        /// <returns>An <see cref="Array"/> containing the file information.</returns>
        public Task<INativeFileInfo[]> GetFilesAsync(SearchOption searchOption)
        {
            return Task.Run(() =>
            {
                if (info == null)
                {
                    if (searchOption == SearchOption.TopDirectoryOnly)
                    {
                        return new INativeFileInfo[0];
                    }

                    return subdirectories.SelectMany(d => d.EnumerateFileSystemInfos("*", System.IO.SearchOption.AllDirectories))
                        .OfType<System.IO.FileInfo>().Select(f => new FileInfo(f)).ToArray();
                }

                return info.GetFileSystemInfos("*", searchOption == SearchOption.AllDirectories ?
                    System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly)
                    .OfType<System.IO.FileInfo>().Select(f => new FileInfo(f)).ToArray();
            });
        }

        /// <summary>
        /// Gets information about the parent directory in which the current directory exists.
        /// </summary>
        /// <returns>The directory information.</returns>
        public Task<INativeDirectoryInfo> GetParentAsync()
        {
            return Task.Run<INativeDirectoryInfo>(() =>
            {
                return info?.Parent == null ? null : new DirectoryInfo(info.Parent);
            });
        }
    }
}
