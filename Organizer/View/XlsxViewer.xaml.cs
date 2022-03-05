using ExcelDataReader;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Organizer.View
{
    /// <summary>
    /// Interaction logic for XlsxViewer.xaml
    /// </summary>
    public partial class XlsxViewer : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty XlsxDataFilePathProperty = DependencyProperty.Register("XlsxDataFilePath", typeof(string), typeof(XlsxViewer), new PropertyMetadata(null, XlsxDataFilePathChanged));

        public XlsxViewer()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DataTableCollection Tablolar
        {
            get => tablolar;

            set
            {
                if (tablolar != value)
                {
                    tablolar = value;
                    OnPropertyChanged(nameof(Tablolar));
                }
            }
        }

        public DataView XlsDataVieW { get; set; }

        public string XlsxDataFilePath { get => (string)GetValue(XlsxDataFilePathProperty); set => SetValue(XlsxDataFilePathProperty, value); }

        public static DataSet XlsxStreamToDt(FileStream stream)
        {
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
            return reader.AsDataSet(new ExcelDataSetConfiguration { UseColumnDataType = true, ConfigureDataTable = _ => new ExcelDataTableConfiguration { EmptyColumnNamePrefix = "Kolon" } });
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DataTableCollection tablolar;

        private static DataSet CsvStreamToDt(FileStream stream)
        {
            IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream, new ExcelReaderConfiguration { FallbackEncoding = Encoding.GetEncoding(1254) });
            return reader.AsDataSet(new ExcelDataSetConfiguration { UseColumnDataType = true, ConfigureDataTable = _ => new ExcelDataTableConfiguration { EmptyColumnNamePrefix = "Kolon" } });
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static void XlsxDataFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is XlsxViewer viewer && e.NewValue != null)
            {
                try
                {
                    string uriString = ((string)e.NewValue);
                    using FileStream fs = File.Open(uriString, FileMode.Open, FileAccess.Read);
                    viewer.Tablolar = (Path.GetExtension(uriString)) switch
                    {
                        ".csv" => CsvStreamToDt(fs).Tables,
                        _ => XlsxStreamToDt(fs).Tables,
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, App.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}