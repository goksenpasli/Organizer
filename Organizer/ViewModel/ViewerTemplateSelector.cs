using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Organizer.ViewModel
{
    public class ViewerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Docx { get; set; }

        public DataTemplate Empty { get; set; }

        public DataTemplate Image { get; set; }

        public DataTemplate Pdf { get; set; }

        public DataTemplate Video { get; set; }

        public DataTemplate Xlsx { get; set; }

        public DataTemplate Xps { get; set; }

        public DataTemplate Zip { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is string dosya)
            {
                string ext = new FileInfo(dosya).Extension.ToLower();
                if (ext == ".pdf")
                {
                    return Pdf;
                }
                if (ext == ".xps")
                {
                    return Xps;
                }
                if (ext == ".zip")
                {
                    return Zip;
                }
                if (docext.Contains(ext))
                {
                    return Docx;
                }
                if (xlsext.Contains(ext))
                {
                    return Xlsx;
                }
                if (imageext.Contains(ext))
                {
                    return Image;
                }
                if (videoext.Contains(ext))
                {
                    return Video;
                }
                else
                {
                    return Empty;
                }
            }
            return Empty;
        }

        private readonly string[] docext = new string[] { ".docx", ".txt", ".xml", ".xaml", ".xsl", ".xslt" };

        private readonly string[] imageext = new string[] { ".jpg", ".jpeg", ".jfif", ".tif", ".tiff", ".png", ".bmp" };

        private readonly string[] videoext = new string[] { ".mp4", ".wmv", ".avi", ".mpg", ".mov", ".3gp2", ".3gpp", ".3gp", ".mpeg" };

        private readonly string[] xlsext = new string[] { ".csv", ".xls", ".xlsx", ".xlsb" };
    }
}