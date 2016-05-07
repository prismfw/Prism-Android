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
using Android.Graphics;
using Android.Runtime;
using Prism.Native;
using Prism.UI.Media;

namespace Prism.Android.UI.Media
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeFonts"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeFonts), IsSingleton = true)]
    public class Fonts : INativeFonts
    {
        /// <summary>
        /// Gets the preferred font size for a button.
        /// </summary>
        public double ButtonFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a button.
        /// </summary>
        public FontStyle ButtonFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a date picker.
        /// </summary>
        public double DatePickerFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a date picker.
        /// </summary>
        public FontStyle DatePickerFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the default font family for UI elements that do not have a font family preference.
        /// </summary>
        public Prism.UI.Media.FontFamily DefaultFontFamily { get; } = new Prism.UI.Media.FontFamily("Roboto");

        /// <summary>
        /// Gets the preferred font size for the detail label of a list box item.
        /// </summary>
        public double DetailLabelFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for the detail label of a list box item.
        /// </summary>
        public FontStyle DetailLabelFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a section footer in a list box that uses a grouped style.
        /// </summary>
        public double GroupedSectionFooterFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a section footer in a list box that uses a grouped style.
        /// </summary>
        public FontStyle GroupedSectionFooterFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a section header in a list box that uses a grouped style.
        /// </summary>
        public double GroupedSectionHeaderFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a section header in a list box that uses a grouped style.
        /// </summary>
        public FontStyle GroupedSectionHeaderFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for the header of a view.
        /// </summary>
        public double HeaderFontSize => 16;

        /// <summary>
        /// Gets the preferred font style for the header of a view.
        /// </summary>
        public FontStyle HeaderFontStyle => (FontStyle)Typeface.DefaultBold.Style;

        /// <summary>
        /// Gets the preferred font size for the title text of a load indicator.
        /// </summary>
        public double LoadIndicatorFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for the title text of a load indicator.
        /// </summary>
        public FontStyle LoadIndicatorFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a search box.
        /// </summary>
        public double SearchBoxFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a search box.
        /// </summary>
        public FontStyle SearchBoxFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a section footer in a list box that uses the default style.
        /// </summary>
        public double SectionFooterFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a section footer in a list box that uses the default style.
        /// </summary>
        public FontStyle SectionFooterFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a section header in a list box that uses the default style.
        /// </summary>
        public double SectionHeaderFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a section header in a list box that uses the default style.
        /// </summary>
        public FontStyle SectionHeaderFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for the display item of a select list.
        /// </summary>
        public double SelectListFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for the display item of a select list.
        /// </summary>
        public FontStyle SelectListFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a standard text label.
        /// </summary>
        public double StandardLabelFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a standard text label.
        /// </summary>
        public FontStyle StandardLabelFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a tab item.
        /// </summary>
        public double TabItemFontSize => 10;

        /// <summary>
        /// Gets the preferred font style for a tab item.
        /// </summary>
        public FontStyle TabItemFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a text area.
        /// </summary>
        public double TextAreaFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a text area.
        /// </summary>
        public FontStyle TextAreaFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a text box.
        /// </summary>
        public double TextBoxFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a text box.
        /// </summary>
        public FontStyle TextBoxFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for a time picker.
        /// </summary>
        public double TimePickerFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for a time picker.
        /// </summary>
        public FontStyle TimePickerFontStyle => (FontStyle)Typeface.Default.Style;

        /// <summary>
        /// Gets the preferred font size for the value label of a list box item.
        /// </summary>
        public double ValueLabelFontSize => 14;

        /// <summary>
        /// Gets the preferred font style for the value label of a list box item.
        /// </summary>
        public FontStyle ValueLabelFontStyle => (FontStyle)Typeface.Default.Style;

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

        /// <summary>
        /// Gets the names of all available fonts.
        /// </summary>
        public string[] GetAvailableFontNames()
        {
            return FontFamilies.Keys.ToArray();
        }

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