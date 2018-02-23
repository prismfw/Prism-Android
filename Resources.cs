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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Prism.Native;
using Prism.UI.Media;

namespace Prism.Android
{
    /// <summary>
    /// Represents an Android implementation of an <see cref="INativeResources"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeResources), IsSingleton = true)]
    public class Resources : INativeResources
    {
        private static readonly Drawable buttonBackground = new global::Android.Widget.Button(Application.MainActivity).Background;
        private static readonly Drawable selectListDisplayGlyph = new global::Android.Widget.Spinner(Application.MainActivity, global::Android.Widget.SpinnerMode.Dropdown).Background;
        private static readonly Drawable selectListListBackground = new global::Android.Widget.Spinner(Application.MainActivity, global::Android.Widget.SpinnerMode.Dropdown).PopupBackground;
        private static readonly Drawable sliderThumb = new global::Android.Widget.SeekBar(Application.MainActivity).Thumb;
        private static readonly Drawable textBoxBackground = new global::Android.Widget.EditText(Application.MainActivity).Background;

        /// <summary>
        /// Gets the color associated with the specified key.
        /// </summary>
        /// <param name="owner">The object that owns the resource, or <c>null</c> if the resource is not owned by a specified object.</param>
        /// <param name="key">The key associated with the color resource to get.</param>
        public static Color GetColor(object owner, object key)
        {
            object retval;
            TryGetResource(owner, key, out retval, false);
            return retval is Color ? (Color)retval : new Color(0);
        }

        /// <summary>
        /// Gets the drawable associated with the specified key.
        /// </summary>
        /// <param name="owner">The object that owns the resource, or <c>null</c> if the resource is not owned by a specified object.</param>
        /// <param name="key">The key associated with the drawable resource to get.</param>
        public static Drawable GetDrawable(object owner, object key)
        {
            object retval;
            TryGetResource(owner, key, out retval, false);
            return retval is Color ? new ColorDrawable((Color)retval) : retval as Drawable;
        }

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
            return TryGetResource(owner, key, out value, true);
        }

        private static bool TryGetResource(object owner, object key, out object value, bool convert)
        {
            int resourceId = 0;
            var context = owner as Context ?? (owner as View)?.Context ?? (owner as Fragment)?.Activity ?? Application.MainActivity;

            if (key is int)
            {
                resourceId = (int)key;
            }
            else
            {
                var resourceKey = key as ResourceKey;
                if (resourceKey != null)
                {
                    switch ((SystemResourceKeyId)resourceKey.Id)
                    {
                        case SystemResourceKeyId.ActionMenuMaxDisplayItems:
                            value = 2;
                            return true;
                        case SystemResourceKeyId.ButtonBorderWidth:
                        case SystemResourceKeyId.DateTimePickerBorderWidth:
                        case SystemResourceKeyId.SelectListBorderWidth:
                        case SystemResourceKeyId.TextBoxBorderWidth:
                            value = 0.0;
                            return true;
                        case SystemResourceKeyId.SearchBoxBorderWidth:
                            value = 1.0;
                            return true;
                        case SystemResourceKeyId.ButtonPadding:
                            value = new Thickness(24, 14);
                            return true;
                        case SystemResourceKeyId.ListBoxItemDetailHeight:
                            value = 52.0;
                            return true;
                        case SystemResourceKeyId.ListBoxItemStandardHeight:
                            value = 44.0;
                            return true;
                        case SystemResourceKeyId.ListBoxItemIndicatorSize:
                        case SystemResourceKeyId.ListBoxItemInfoButtonSize:
                        case SystemResourceKeyId.ListBoxItemInfoIndicatorSize:
                            value = new Size();
                            return true;
                        case SystemResourceKeyId.PopupSize:
                            value = new Size(540, 620);
                            return true;
                        case SystemResourceKeyId.SelectListDisplayItemPadding:
                            value = new Thickness(3, 4, Math.Ceiling(selectListDisplayGlyph.IntrinsicWidth.GetScaledDouble()), 4);
                            return true;
                        case SystemResourceKeyId.SelectListListItemPadding:
                            value = new Thickness(4, 10, 16, 10);
                            return true;
                        case SystemResourceKeyId.ShouldAutomaticallyIndentSeparators:
                            value = true;
                            return true;
                        case SystemResourceKeyId.HorizontalScrollBarHeight:
                        case SystemResourceKeyId.VerticalScrollBarWidth:
                            value = 4.0;
                            return true;
                        case SystemResourceKeyId.BaseFontFamily:
                            value = new FontFamily("Roboto");
                            return true;
                        case SystemResourceKeyId.ButtonFontSize:
                        case SystemResourceKeyId.DateTimePickerFontSize:
                        case SystemResourceKeyId.DetailLabelFontSize:
                        case SystemResourceKeyId.GroupedSectionHeaderFontSize:
                        case SystemResourceKeyId.LabelFontSize:
                        case SystemResourceKeyId.LoadIndicatorFontSize:
                        case SystemResourceKeyId.SearchBoxFontSize:
                        case SystemResourceKeyId.SectionHeaderFontSize:
                        case SystemResourceKeyId.SelectListFontSize:
                        case SystemResourceKeyId.TextBoxFontSize:
                        case SystemResourceKeyId.ValueLabelFontSize:
                            value = 14.0;
                            return true;
                        case SystemResourceKeyId.TabItemFontSize:
                            value = 12.0;
                            return true;
                        case SystemResourceKeyId.ViewHeaderFontSize:
                            value = 16.0;
                            return true;
                        case SystemResourceKeyId.ButtonFontStyle:
                        case SystemResourceKeyId.DateTimePickerFontStyle:
                        case SystemResourceKeyId.DetailLabelFontStyle:
                        case SystemResourceKeyId.GroupedSectionHeaderFontStyle:
                        case SystemResourceKeyId.LabelFontStyle:
                        case SystemResourceKeyId.LoadIndicatorFontStyle:
                        case SystemResourceKeyId.SearchBoxFontStyle:
                        case SystemResourceKeyId.SectionHeaderFontStyle:
                        case SystemResourceKeyId.SelectListFontStyle:
                        case SystemResourceKeyId.TabItemFontStyle:
                        case SystemResourceKeyId.TextBoxFontStyle:
                        case SystemResourceKeyId.ValueLabelFontStyle:
                            value = FontStyle.Normal;
                            return true;
                        case SystemResourceKeyId.ViewHeaderFontStyle:
                            value = FontStyle.Bold;
                            return true;
                        case SystemResourceKeyId.AccentBrush:
                        case SystemResourceKeyId.ActivityIndicatorForegroundBrush:
                        case SystemResourceKeyId.ProgressBarForegroundBrush:
                        case SystemResourceKeyId.TabViewForegroundBrush:
                        case SystemResourceKeyId.ToggleSwitchForegroundBrush:
                        case SystemResourceKeyId.ToggleSwitchThumbOnBrush:
                            resourceId = Resource.Attribute.ColorAccent;
                            break;
                        case SystemResourceKeyId.ActionMenuBackgroundBrush:
                        case SystemResourceKeyId.ButtonBorderBrush:
                        case SystemResourceKeyId.DateTimePickerBorderBrush:
                        case SystemResourceKeyId.GroupedListBoxItemBackgroundBrush:
                        case SystemResourceKeyId.GroupedSectionHeaderBackgroundBrush:
                        case SystemResourceKeyId.ListBoxBackgroundBrush:
                        case SystemResourceKeyId.ListBoxItemBackgroundBrush:
                        case SystemResourceKeyId.ListBoxItemSelectedBackgroundBrush:
                        case SystemResourceKeyId.ProgressBarBackgroundBrush:
                        case SystemResourceKeyId.SearchBoxBackgroundBrush:
                        case SystemResourceKeyId.SearchBoxBorderBrush:
                        case SystemResourceKeyId.SectionHeaderBackgroundBrush:
                        case SystemResourceKeyId.SelectListBackgroundBrush:
                        case SystemResourceKeyId.SelectListBorderBrush:
                        case SystemResourceKeyId.SelectListListSeparatorBrush:
                        case SystemResourceKeyId.TabViewBackgroundBrush:
                        case SystemResourceKeyId.TextBoxBorderBrush:
                        case SystemResourceKeyId.ToggleSwitchBorderBrush:
                        case SystemResourceKeyId.ViewBackgroundBrush:
                        case SystemResourceKeyId.ViewHeaderBackgroundBrush:
                            value = null;
                            return true;
                        case SystemResourceKeyId.ActionMenuForegroundBrush:
                        case SystemResourceKeyId.ButtonForegroundBrush:
                        case SystemResourceKeyId.DateTimePickerForegroundBrush:
                        case SystemResourceKeyId.MenuFlyoutForegroundBrush:
                        case SystemResourceKeyId.GroupedSectionHeaderForegroundBrush:
                        case SystemResourceKeyId.LabelForegroundBrush:
                        case SystemResourceKeyId.ListBoxSeparatorBrush:
                        case SystemResourceKeyId.LoadIndicatorForegroundBrush:
                        case SystemResourceKeyId.SearchBoxForegroundBrush:
                        case SystemResourceKeyId.SectionHeaderForegroundBrush:
                        case SystemResourceKeyId.SelectListForegroundBrush:
                        case SystemResourceKeyId.TabItemForegroundBrush:
                        case SystemResourceKeyId.TextBoxForegroundBrush:
                        case SystemResourceKeyId.ToggleSwitchBackgroundBrush:
                        case SystemResourceKeyId.ToggleSwitchThumbOffBrush:
                        case SystemResourceKeyId.ViewHeaderForegroundBrush:
                            resourceId = Resource.Attribute.TextColorPrimary;
                            break;
                        case SystemResourceKeyId.ButtonBackgroundBrush:
                        case SystemResourceKeyId.DateTimePickerBackgroundBrush:
                            value = convert ? (object)new DataBrush(buttonBackground.GetConstantState().NewDrawable()) : buttonBackground.GetConstantState().NewDrawable();
                            return true;
                        case SystemResourceKeyId.DetailLabelForegroundBrush:
                        case SystemResourceKeyId.ValueLabelForegroundBrush:
                            resourceId = Resource.Attribute.TextColorSecondary;
                            break;
                        case SystemResourceKeyId.FlyoutBackgroundBrush:
                        case SystemResourceKeyId.SelectListListBackgroundBrush:
                            value = convert ? (object)new DataBrush(selectListListBackground.GetConstantState().NewDrawable()) : selectListListBackground.GetConstantState().NewDrawable();
                            return true;
                        case SystemResourceKeyId.LoadIndicatorBackgroundBrush:
                            resourceId = Resource.Attribute.PanelColorBackground;
                            break;
                        case SystemResourceKeyId.SliderBackgroundBrush:
                            value = new SolidColorBrush(new Prism.UI.Color(33, 36, 40));
                            return true;
                        case SystemResourceKeyId.SliderForegroundBrush:
                            value = new SolidColorBrush(new Prism.UI.Color(38, 169, 216));
                            return true;
                        case SystemResourceKeyId.SliderThumbBrush:
                            value = convert ? (object)new DataBrush(sliderThumb.GetConstantState().NewDrawable()) : sliderThumb.GetConstantState().NewDrawable();
                            return true;
                        case SystemResourceKeyId.TextBoxBackgroundBrush:
                            value = convert ? (object)new DataBrush(textBoxBackground.GetConstantState().NewDrawable()) : textBoxBackground.GetConstantState().NewDrawable();
                            return true;
                    }
                }

                if (resourceId == 0)
                {
                    string resourceName = key.ToString();
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
            }

            if (resourceId == 0)
            {
                value = null;
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

                        try
                        {
                            value = array.GetDrawable(0);
                        }
                        catch
                        {
                            try
                            {
                                value = array.GetColor(0, 0);
                            }
                            catch
                            {
                                value = null;
                                return false;
                            }
                        }

                        if (convert)
                        {
                            if (value is Color)
                            {
                                value = new SolidColorBrush(((Color)value).GetColor());
                            }
                            else
                            {
                                var drawable = value as ColorDrawable;
                                value = drawable != null ? (Brush)new SolidColorBrush(drawable.Color.GetColor()) : new DataBrush(value);
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

                        if (convert && value is Color)
                        {
                            value = new SolidColorBrush(((Color)value).GetColor());
                        }
                        return true;
                    case "dimen":
                        value = context.Resources.GetDimension(resourceId);
                        return true;
                    case "drawable":
#pragma warning disable 0618 // deprecated method is called for pre-lollipop devices
                        value = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ?
                            context.Resources.GetDrawable(resourceId, context.Theme) : context.Resources.GetDrawable(resourceId);
#pragma warning restore 0618

                        if (convert)
                        {
                            var drawable = value as ColorDrawable;
                            value = drawable != null ? (Brush)new SolidColorBrush(drawable.Color.GetColor()) : new DataBrush(value);
                        }
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

            value = null;
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
