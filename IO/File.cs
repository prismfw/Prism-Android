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


using System;
using System.IO;
using System.Threading.Tasks;
using Android.Runtime;
using Prism.Native;

namespace Prism.Android.IO
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeFile"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeFile), IsSingleton = true)]
    public class File : INativeFile
    {
        /// <summary>
        /// Opens the file at the specified path, appends the specified bytes to the end of the file, and then closes
        /// the file.
        /// </summary>
        /// <param name="filePath">The path of the file in which to append the bytes.</param>
        /// <param name="bytes">The bytes to append to the end of the file.</param>
        public Task AppendBytesAsync(string filePath, byte[] bytes)
        {
            return Task.Run(() =>
            {
                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Write))
                {
                    stream.Seek(0, SeekOrigin.End);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
            });
        }

        /// <summary>
        /// Opens the file at the specified path, appends the specified bytes to the end of the file, and then closes
        /// the file.
        /// If the file does not exist, one is created.
        /// </summary>
        /// <param name="filePath">The path of the file in which to append the bytes.</param>
        /// <param name="bytes">The bytes to append to the end of the file.</param>
        public Task AppendAllBytesAsync(string filePath, byte[] bytes)
        {
            return Task.Run(() =>
            {
                using (var stream = System.IO.File.Open(filePath, FileMode.Append))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
            });
        }

        /// <summary>
        /// Opens the file at the specified path, appends the specified text to the end of the file, and then closes the file.
        /// </summary>
        /// <param name="filePath">The path of the file in which to append the text.</param>
        /// <param name="text">The text to append to the end of the file.</param>
        public Task AppendTextAsync(string filePath, string text)
        {
            return Task.Run(() =>
            {
                using (var stream = System.IO.File.AppendText(filePath))
                {
                    stream.Write(text);
                    stream.Flush();
                }
            });
        }

        /// <summary>
        /// Opens the file at the specified path, appends the specified text to the end of the file, and then closes the file.
        /// If the file does not exist, one is created.
        /// </summary>
        /// <param name="filePath">The path of the file in which to append the text.</param>
        /// <param name="text">The text to append to the end of the file.</param>
        public Task AppendAllTextAsync(string filePath, string text)
        {
            return Task.Run(() =>
            {
                try
                {
                    System.IO.File.AppendAllText(filePath, text);
                }
                catch (DirectoryNotFoundException)
                {
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    System.IO.File.AppendAllText(filePath, text);
                }
            });
        }

        /// <summary>
        /// Copies the file at the source path to the destination path, overwriting any existing file.
        /// </summary>
        /// <param name="sourceFilePath">The path of the file to be copied.</param>
        /// <param name="destinationFilePath">The path to where the copied file should be placed.</param>
        public Task CopyAsync(string sourceFilePath, string destinationFilePath)
        {
            return Task.Run(() =>
            {
                System.IO.File.Copy(sourceFilePath, destinationFilePath);
            });
        }

        /// <summary>
        /// Creates a file at the specified path, overwriting any existing file.
        /// </summary>
        /// <param name="filePath">The path at which to create the file.</param>
        /// <param name="bufferSize">The number of bytes buffered for reading and writing to the file.</param>
        public Task<Stream> CreateAsync(string filePath, int bufferSize)
        {
            return Task.Run(() =>
            {
                return System.IO.File.Create(filePath, bufferSize) as Stream;
            });
        }

        /// <summary>
        /// Deletes the file at the specified path.
        /// </summary>
        /// <param name="filePath">The path of the file to delete.</param>
        public Task DeleteAsync(string filePath)
        {
            return Task.Run(() =>
            {
                System.IO.File.Delete(filePath);
            });
        }

        /// <summary>
        /// Moves the file at the source path to the destination path, overwriting any existing file.
        /// </summary>
        /// <param name="sourceFilePath">The path of the file to be moved.</param>
        /// <param name="destinationFilePath">The path to where the file should be moved.</param>
        public Task MoveAsync(string sourceFilePath, string destinationFilePath)
        {
            return Task.Run(() =>
            {
                System.IO.File.Move(sourceFilePath, destinationFilePath);
            });
        }

        /// <summary>
        /// Opens the file at the specified path, optionally creating one if it doesn't exist.
        /// </summary>
        /// <param name="filePath">The path of the file to be opened.</param>
        /// <param name="mode">The manner in which the file should be opened.</param>
        public Task<Stream> OpenAsync(string filePath, Prism.IO.FileMode mode)
        {
            return Task.Run(() =>
            {
                FileMode fileMode;
                switch (mode)
                {
                    case Prism.IO.FileMode.Create:
                        fileMode = FileMode.Create;
                        break;
                    case Prism.IO.FileMode.Open:
                        fileMode = FileMode.Open;
                        break;
                    default:
                        fileMode = FileMode.OpenOrCreate;
                        break;
                }

                return System.IO.File.Open(filePath, fileMode) as Stream;
            });
        }

        /// <summary>
        /// Opens the file at the specified path, reads all of the bytes in the file, and then closes the file.
        /// </summary>
        /// <param name="filePath">The path of the file from which to read the bytes.</param>
        /// <returns>The all bytes.</returns>
        public Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            return Task.Run(() =>
            {
                return System.IO.File.ReadAllBytes(filePath);
            });
        }

        /// <summary>
        /// Opens the file at the specified path, reads all of the text in the file, and then closes the file.
        /// </summary>
        /// <param name="filePath">The path of the file from which to read the text.</param>
        /// <returns>The all text.</returns>
        public Task<string> ReadAllTextAsync(string filePath)
        {
            return Task.Run(() =>
            {
                return System.IO.File.ReadAllText(filePath);
            });
        }

        /// <summary>
        /// Creates a new file at the specified path, writes the specified bytes to the file, and then closes the file.
        /// If a file already exists, it is overwritten.
        /// </summary>
        /// <param name="filePath">The path of the file in which to write the bytes.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        public Task WriteAllBytesAsync(string filePath, byte[] bytes)
        {
            return Task.Run(() =>
            {
                System.IO.File.WriteAllBytes(filePath, bytes);
            });
        }

        /// <summary>
        /// Creates a new file at the specified path, writes the specified text to the file, and then closes the file.
        /// If a file already exists, it is overwritten.
        /// </summary>
        /// <param name="filePath">The path of the file in which to write the text.</param>
        /// <param name="text">The text to write to the file.</param>
        public Task WriteAllTextAsync(string filePath, string text)
        {
            return Task.Run(() =>
            {
                System.IO.File.WriteAllText(filePath, text);
            });
        }
    }
}