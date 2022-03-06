using Organizer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Organizer.ViewModel
{
    internal static class ExtensionMethods
    {
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

        internal static T DeSerialize<T>(this string xmldatapath) where T : class, new()
        {
            try
            {
                XmlSerializer serializer = new(typeof(T));
                using StreamReader stream = new(xmldatapath);
                return serializer.Deserialize(stream) as T;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
                return null;
            }
        }

        internal static T DeSerialize<T>(this XElement xElement) where T : class, new()
        {
            XmlSerializer serializer = new(typeof(T));
            return serializer.Deserialize(xElement.CreateReader()) as T;
        }

        internal static ObservableCollection<T> DeSerialize<T>(this IEnumerable<XElement> xElement) where T : class, new()
        {
            ObservableCollection<T> list = new();
            foreach (XElement element in xElement)
            {
                list.Add(element.DeSerialize<T>());
            }
            return list;
        }

        internal static ObservableCollection<Etiket> EtiketleriYükle()
        {
            return DesignerProperties.GetIsInDesignMode(new DependencyObject())
           ? null
           : File.Exists(Properties.Settings.Default.XmlDataPath)
           ? Properties.Settings.Default.XmlDataPath.DeSerialize<Veriler>().Etiketler.Etiket
           : new ObservableCollection<Etiket>();
        }

        internal static string RandomColor()
        {
            return $"#{new Random(Guid.NewGuid().GetHashCode()).Next(0x1000000):X6}";
        }

        internal static int RandomNumber()
        {
            return new Random(Guid.NewGuid().GetHashCode()).Next(1, int.MaxValue);
        }

        internal static void Serialize<T>(this T dataToSerialize) where T : class
        {
            XmlSerializer serializer = new(typeof(T));
            using TextWriter stream = new StreamWriter(Properties.Settings.Default.XmlDataPath);
            serializer.Serialize(stream, dataToSerialize);
        }

        internal static ObservableCollection<Veri> VerileriYükle()
        {
            return DesignerProperties.GetIsInDesignMode(new DependencyObject())
           ? null
           : File.Exists(Properties.Settings.Default.XmlDataPath)
           ? Properties.Settings.Default.XmlDataPath.DeSerialize<Veriler>().Veri
           : new ObservableCollection<Veri>();
        }
    }
}