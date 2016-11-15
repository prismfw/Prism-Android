/*
Copyright (C) 2016  Prism Framework Team

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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Android;
using Android.Content;
using Android.OS;
using Android.Views;
using Prism.Native;
using Prism.UI.Media;

namespace Prism.Android
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeResources"/>.
    /// </summary>
    [Register(typeof(INativeResources), IsSingleton = true)]
    public class Resources : INativeResources
    {
        private readonly Dictionary<object, object> resourceValues = new Dictionary<object, object>()
        {
            { SystemResources.BaseFontFamilyKey, new FontFamily("Roboto") },
            { SystemResources.BaseFontSizeKey, 14.0 },
            { SystemResources.ButtonPaddingKey, new Thickness(24, 14) },
            { SystemResources.HorizontalScrollBarHeightKey, 4.0 },
            { SystemResources.ListBoxItemDetailHeightKey, 52.0 },
            { SystemResources.ListBoxItemIndicatorSizeKey, new Size() },
            { SystemResources.ListBoxItemInfoButtonSizeKey, new Size() },
            { SystemResources.ListBoxItemInfoIndicatorSizeKey, new Size() },
            { SystemResources.ListBoxItemStandardHeightKey, 44.0 },
            { SystemResources.PopupSizeKey, new Size(540, 620) },
            { SystemResources.SearchBoxBorderWidthKey, 1.0 },
            { SystemResources.SelectListDisplayItemPaddingKey, new Thickness(3, 4, 20, 4) },
            { SystemResources.SelectListListItemPaddingKey, new Thickness(4, 10, 16, 10) },
            { SystemResources.TabItemFontSizeKey, 10.0 },
            { SystemResources.VerticalScrollBarWidthKey, 4.0 },
            { SystemResources.ViewHeaderFontSizeKey, 16.0 },
            { SystemResources.ViewHeaderFontStyleKey, FontStyle.Bold },
        };
        
        /// <summary>
        /// Gets the names of all available fonts.
        /// </summary>
        public string[] GetAvailableFontNames()
        {
            return FontFamilies.Keys.ToArray();
        }

        /// <summary>
        /// Gets the system resource associated with the specified key.
        /// </summary>
        /// <param name="owner">The object that owns the resource, or <c>null</c> if the resource is not owned by a specified object.</param>
        /// <param name="key">The key associated with the resource to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the system resources contain a resource with the specified key; otherwise, <c>false</c>.</returns>
        public bool TryGetResource(object owner, object key, out object value)
        {
            if (resourceValues.TryGetValue(key, out value))
            {
                return true;
            }

            int resourceId = 0;
            var context = owner as Context ?? (owner as View)?.Context ?? Application.MainActivity;
            
            if (key is int)
            {
                resourceId = (int)key;
            }
            else
            {
                string resourceName = (key as ResourceKey)?.Id ?? key.ToString();
                if (!int.TryParse(resourceName, out resourceId))
                {
                    string[] names = resourceName.Split('|');
                    foreach (var name in names)
                    {
                        resourceId = context.Resources.GetIdentifier(name, null, context.PackageName);
                        if (resourceId != 0)
                        {
                            resourceName = name;
                            break;
                        }
                    }
                }
            }

            if (resourceId == 0)
            {
                return false;
            }
            
            try
            {
                switch (context.Resources.GetResourceTypeName(resourceId))
                {
                    case "anim":
                        value = context.Resources.GetAnimation(resourceId);
                        return true;
                    case "attr":
                        var array = context.ObtainStyledAttributes(Resource.Style.Theme, new int[] { resourceId });
                        var typeValue = array?.PeekValue(0);
                        if (typeValue == null)
                        {
                            return false;
                        }
                        
                        try
                        {
                            value = array.GetColor(0, 0);
                        }
                        catch
                        {
                            try
                            {
                                value = array.GetDrawable(0);
                            }
                            catch
                            {
                                return false;
                            }
                        }
                        return true;
                    case "bool":
                        value = context.Resources.GetBoolean(resourceId);
                        return true;
                    case "color":
#pragma warning disable 0618 // deprecated method is called for pre-marshmallow devices
                        value = Build.VERSION.SdkInt >= BuildVersionCodes.M ?
                            context.Resources.GetColor(resourceId, context.Theme) : context.Resources.GetColor(resourceId);
#pragma warning restore 0618
                        return true;
                    case "dimen":
                        value = context.Resources.GetDimension(resourceId);
                        return true;
                    case "drawable":
#pragma warning disable 0618 // deprecated method is called for pre-lollipop devices
                        value = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ?
                            context.Resources.GetDrawable(resourceId, context.Theme) : context.Resources.GetDrawable(resourceId);
#pragma warning restore 0618
                        return true;
                    case "integer":
                        value = context.Resources.GetInteger(resourceId);
                        return true;
                    case "layout":
                        value = context.Resources.GetLayout(resourceId);
                        return true;
                    case "string":
                        value = context.Resources.GetString(resourceId);
                        return true;
                    case "xml":
                        value = context.Resources.GetXml(resourceId);
                        return true;
                }
            }
            catch { }

            return false;
        }
        
        /// <summary>
        /// Gets a collection of the available font families and the paths to their font files.
        /// </summary>
        internal static Dictionary<string, string> FontFamilies
        {
            get
            {
                if (fontFamilies != null)
                {
                    return fontFamilies;
                }

                fontFamilies = new Dictionary<string, string>();

                var fontFiles = Directory.GetFiles("/system/fonts", "*", SearchOption.TopDirectoryOnly).Where(f => f.EndsWith("-Regular.ttf") || !f.Contains('-'));
                foreach (var filePath in fontFiles)
                {
                    try
                    {
                        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        var reader = new BinaryReader(fileStream);
                        var bytes = ToBigEndian(reader.ReadBytes(Marshal.SizeOf<OffsetTable>()));

                        var handle = Marshal.AllocHGlobal(bytes.Length);
                        Marshal.Copy(bytes, 0, handle, bytes.Length);

                        var offsetTable = (OffsetTable)Marshal.PtrToStructure(handle, typeof(OffsetTable));
                        Marshal.FreeHGlobal(handle);

                        //Major must be 1 and Minor must be 0
                        if (offsetTable.MajorVersion != 1 || offsetTable.MinorVersion != 0)
                        {
                            continue;
                        }

                        bool isFound = false;
                        var directory = new TableDirectory();
                        for (int i = 0; i < offsetTable.NumberOfTables; i++)
                        {
                            bytes = reader.ReadBytes(Marshal.SizeOf<TableDirectory>());                        
                            handle = Marshal.AllocHGlobal(bytes.Length);
                            Marshal.Copy(bytes, 0, handle, bytes.Length);

                            directory = (TableDirectory)Marshal.PtrToStructure(handle, typeof(TableDirectory));
                            Marshal.FreeHGlobal(handle);

                            string name = new string(new[] { directory.Tag1, directory.Tag2, directory.Tag3, directory.Tag4 });
                            if (name.Length > 0)
                            {
                                if (name == "name")
                                {
                                    isFound = true;

                                    var length = BitConverter.GetBytes(directory.Length);
                                    var offset = BitConverter.GetBytes(directory.Offset);
                                    Array.Reverse(length);
                                    Array.Reverse(offset);
                                    directory.Length = BitConverter.ToUInt32(length, 0);
                                    directory.Offset = BitConverter.ToUInt32(offset, 0);
                                    break;
                                }
                            }
                        }

                        if (isFound)
                        {
                            fileStream.Position = directory.Offset;

                            bytes = ToBigEndian(reader.ReadBytes(Marshal.SizeOf<TableHeader>()));
                            handle = Marshal.AllocHGlobal(bytes.Length);
                            Marshal.Copy(bytes, 0, handle, bytes.Length);

                            var tableHeader = (TableHeader)Marshal.PtrToStructure(handle, typeof(TableHeader));
                            Marshal.FreeHGlobal(handle);

                            isFound = false;
                            for (int i = 0; i < tableHeader.Count; i++)
                            {
                                var recordBytes = ToBigEndian(reader.ReadBytes(Marshal.SizeOf<Record>()));
                                var recordHandle = Marshal.AllocHGlobal(recordBytes.Length);
                                Marshal.Copy(recordBytes, 0, recordHandle, recordBytes.Length);

                                var record = (Record)Marshal.PtrToStructure(recordHandle, typeof(Record));
                                Marshal.FreeHGlobal(recordHandle);
                                if (record.NameID == 1)
                                {
                                    long streamPos = fileStream.Position;
                                    fileStream.Position = directory.Offset + record.StringOffset + tableHeader.StorageOffset;

                                    var result = reader.ReadChars(record.StringLength);
                                    if (result.Length != 0)
                                    {
                                        fontFamilies[new string(result.Where(c => c != '\0').ToArray())] = filePath;
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }

                return fontFamilies;
            }
        }
        private static Dictionary<string, string> fontFamilies;

        private static byte[] ToBigEndian(byte[] array)
        {
            byte[] bigEndian = new byte[array.Length];
            for (int y = 0; y < array.Length - 1; y += 2)
            {
                bigEndian[y] = array[y + 1];
                bigEndian[y + 1] = array[y];
            }
            return bigEndian;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct OffsetTable
        {
            public ushort MajorVersion;
            public ushort MinorVersion;
            public ushort NumberOfTables;
            public ushort SearchRange;
            public ushort EntrySelector;
            public ushort RangeShift;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TableDirectory
        {
            public char Tag1;
            public char Tag2;
            public char Tag3;
            public char Tag4;
            public uint CheckSum;
            public uint Offset;
            public uint Length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TableHeader
        {
            public ushort Selector;
            public ushort Count;
            public ushort StorageOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Record
        {
            public ushort PlatformID;
            public ushort EncodingID;
            public ushort LanguageID;
            public ushort NameID;
            public ushort StringLength;
            public ushort StringOffset;
        }
    }
}
