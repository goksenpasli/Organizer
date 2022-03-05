using Extensions;
using Microsoft.Win32;
using Organizer.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Organizer.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Veriler = new Veriler
            {
                Etiketler = new Etiketler
                {
                    Etiket = ExtensionMethods.EtiketleriYükle()
                },
                Veri = ExtensionMethods.VerileriYükle()
            };

            Veri = new Veri();

            DatabaseSave = new RelayCommand<object>(parameter => Veriler.Serialize(), parameter => File.Exists(Properties.Settings.Default.XmlDataPath));

            YoluKaydet = new RelayCommand<object>(parameter =>
            {
                string folderpath = GetFolderPath();
                string path = folderpath + @"\Data.xml";
                if (File.Exists(path) && folderpath != null)
                {
                    Properties.Settings.Default.XmlDataPath = path;
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Uygulama Kapatılacak Yeni Ayarlarla Tekrar Çalıştırın.", App.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Application.Current.MainWindow.Close();
                }
                else
                {
                    MessageBox.Show("Klasör Seçimi Yanlış.", App.Current?.MainWindow?.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            });

            EtiketEkle = new RelayCommand<object>(parameter =>
            {
                Veriler.Etiketler.Etiket.Add(new Etiket
                {
                    Id = ExtensionMethods.RandomNumber(),
                    Açıklama = Etiket,
                    Seçili = false
                });
                DatabaseSave.Execute(null);
                Etiket = null;
            }, parameter => !string.IsNullOrWhiteSpace(Properties.Settings.Default.XmlDataPath) && !string.IsNullOrWhiteSpace(Etiket));

            DosyaEkle = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new() { Multiselect = false, Filter = "Tüm Dosyalar (*.*)|*.*" };
                if (openFileDialog.ShowDialog() == true)
                {
                    Veri.DosyaAdı = openFileDialog.FileName;
                }
            }, parameter => true);

            SaveSampleXml = new RelayCommand<object>(parameter =>
            {
                SaveFileDialog saveFileDialog = new() { Filter = "Xml Dosyası(*.xml)|*.xml" };
                saveFileDialog.FileName = "Data.xml";
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, SampleXmlData);
                }
            }, parameter => true);

            DosyaAç = new RelayCommand<object>(parameter =>
            {
                string file = Path.Combine(Path.GetDirectoryName(Properties.Settings.Default.XmlDataPath), parameter as string);
                if (File.Exists(file))
                {
                    _ = Process.Start(file);
                }
            });

            ExploreFile = new RelayCommand<object>(parameter => ExtensionMethods.OpenFolderAndSelectItem(Path.GetDirectoryName(Properties.Settings.Default.XmlDataPath), Path.GetFileName(parameter as string)), parameter => true);

            KayıtEkle = new RelayCommand<object>(parameter =>
            {
                try
                {
                    Veri veri = new()
                    {
                        DosyaAdı = Path.GetFileName(Veri.DosyaAdı),
                        Etiket = new()
                    };

                    foreach (int etiketid in Veriler.Etiketler.Etiket.Where(z => z.Seçili).Select(z => z.Id))
                    {
                        veri.Etiket.Add(new Etiket() { Id = etiketid });
                    }

                    Veriler.Veri.Add(veri);
                    File.Copy(Veri.DosyaAdı, Path.Combine(Path.GetDirectoryName(Properties.Settings.Default.XmlDataPath), Path.GetFileName(Veri.DosyaAdı)), true);
                    DatabaseSave.Execute(null);
                    Veri.DosyaAdı = null;
                }
                catch (System.Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
            }, parameter => !string.IsNullOrWhiteSpace(Properties.Settings.Default.XmlDataPath) && !string.IsNullOrWhiteSpace(Veri.DosyaAdı) && Veriler.Etiketler.Etiket.Any(z => z.Seçili));

            if (!File.Exists(Properties.Settings.Default.XmlDataPath))
            {
                if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                {
                    return;
                }
                MessageBox.Show("Xml Dosyası Bulunamadı. Klasör Açtan Xml Dosyasını Gösterin.", App.Current?.MainWindow?.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Properties.Settings.Default.XmlDataPath = null;
                Properties.Settings.Default.Save();
            }

            PropertyChanged += MainViewModel_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand DatabaseSave { get; }

        public ICommand DosyaAç { get; }

        public ICommand DosyaEkle { get; }

        public string Etiket { get; set; }

        public string EtiketAramaMetni { get; set; }

        public ICommand EtiketEkle { get; }

        public ICommand ExploreFile { get; }

        public ICommand KayıtEkle { get; }

        public string SampleXmlData => @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Veriler xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<Etiketler/>
</Veriler>";

        public ICommand SaveSampleXml { get; }

        public Veri Veri { get; set; }

        public Veriler Veriler { get; set; }

        public ICommand YoluKaydet { get; }

        private string GetFolderPath()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new();
            dialog.Description = "Data.xml Dosyasını Gösterin.";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            return result == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is "EtiketAramaMetni")
            {
                IEnumerable<int> id = Veriler?.Etiketler?.Etiket?.Where(z => z.Açıklama?.Contains(EtiketAramaMetni) == true).Select(z => z.Id);
                MainWindow.cvs.Filter += (s, e) => e.Accepted = (e.Item as Veri)?.Etiket?.Any(z => id?.Contains(z.Id) == true) == true;
            }
        }
    }
}