using Extensions;
using Microsoft.Win32;
using Organizer.Model;
using System;
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

            DatabaseSave = new RelayCommand<object>(parameter =>
            {
                try
                {
                    Veriler.Serialize();
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
            }, parameter => File.Exists(Properties.Settings.Default.XmlDataPath));

            RefreshDatabase = new RelayCommand<object>(parameter => Veriler.Veri = ExtensionMethods.VerileriYükle(), parameter => File.Exists(Properties.Settings.Default.XmlDataPath));

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

            EtiketSil = new RelayCommand<object>(parameter =>
            {
                if (MessageBox.Show("Seçili Etiketi Silmek İstiyor musun?", App.Current.MainWindow.Title, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    Etiket silinecek = SeçiliVeri.Etiket.FirstOrDefault(z => z.Id == (parameter as Etiket)?.Id);
                    SeçiliVeri.Etiket.Remove(silinecek);
                    DatabaseSave.Execute(null);
                }
            }, parameter => SeçiliVeri is not null);

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

            ExploreFile = new RelayCommand<object>(parameter => Extensions.ExtensionMethods.OpenFolderAndSelectItem(Path.GetDirectoryName(Properties.Settings.Default.XmlDataPath), Path.GetFileName(parameter as string)), parameter => true);

            KayıtEkle = new RelayCommand<object>(parameter =>
            {
                try
                {
                    if (Veriler.Veri.Any(z => z.DosyaAdı == Path.GetFileName(Veri.DosyaAdı)))
                    {
                        MessageBox.Show("Bu İsimde Dosya Zaten Var.", App.Current?.MainWindow?.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    Veri veri = new()
                    {
                        DosyaAdı = Path.GetFileName(Veri.DosyaAdı),
                        Etiket = new()
                    };

                    foreach (int etiketid in Veriler.Etiketler.Etiket.Where(z => z.Seçili).Select(z => z.Id))
                    {
                        veri.Etiket.Add(new Etiket() { Id = etiketid });
                    }

                    File.Copy(Veri.DosyaAdı, Path.Combine(Path.GetDirectoryName(Properties.Settings.Default.XmlDataPath), Path.GetFileName(Veri.DosyaAdı)), true);
                    Veriler.Veri.Add(veri);
                    DatabaseSave.Execute(null);
                    Veri.DosyaAdı = null;
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
            }, parameter => !string.IsNullOrWhiteSpace(Properties.Settings.Default.XmlDataPath) && !string.IsNullOrWhiteSpace(Veri.DosyaAdı) && Veriler.Etiketler.Etiket.Any(z => z.Seçili));

            KayıtEtiketEkle = new RelayCommand<object>(parameter =>
            {
                SeçiliVeri.Etiket.Add(new Etiket() { Id = (int)parameter });
                DatabaseSave.Execute(null);
            }, parameter => parameter is not null && SeçiliVeri?.Etiket?.Any(z => z.Id == (int)parameter) == false);

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

        public bool DosyaİsimArama { get; set; }

        public string Etiket { get; set; }

        public string EtiketAçıklamaMetni { get; set; }

        public string EtiketAramaMetni { get; set; }

        public ICommand EtiketEkle { get; }

        public ICommand EtiketSil { get; }

        public ICommand ExploreFile { get; }

        public ICommand KayıtEkle { get; }

        public ICommand KayıtEtiketEkle { get; }

        public bool Panorama { get; set; }

        public ICommand RefreshDatabase { get; }

        public string SampleXmlData => @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Veriler xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<Etiketler/>
</Veriler>";

        public ICommand SaveSampleXml { get; }

        public Veri SeçiliVeri { get; set; }

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
                if (string.IsNullOrEmpty(EtiketAramaMetni))
                {
                    MainWindow.cvs.Filter += (s, e) => e.Accepted = true;
                    return;
                }
                IEnumerable<int> id = Veriler?.Etiketler?.Etiket?.Where(z => z.Açıklama?.Contains(EtiketAramaMetni) == true).Select(z => z.Id);
                MainWindow.cvs.Filter += (s, e) =>
                {
                    e.Accepted = DosyaİsimArama
                        ? (e.Item as Veri)?.DosyaAdı?.Contains(EtiketAramaMetni) == true || (e.Item as Veri)?.Etiket?.Any(z => id?.Contains(z.Id) == true) == true
                        : (e.Item as Veri)?.Etiket?.Any(z => id?.Contains(z.Id) == true) == true;
                };
            }

            if (e.PropertyName is "EtiketAçıklamaMetni")
            {
                if (string.IsNullOrEmpty(EtiketAçıklamaMetni))
                {
                    MainWindow.cvsetiket.Filter += (s, e) => e.Accepted = true;
                    return;
                }
                MainWindow.cvsetiket.Filter += (s, e) => e.Accepted = (e.Item as Etiket)?.Açıklama?.Contains(EtiketAçıklamaMetni) == true;
            }
            if (e.PropertyName is "SeçiliVeri" && SeçiliVeri is not null && !ViewerTemplateSelector.imageext.Contains(Path.GetExtension(SeçiliVeri.DosyaAdı).ToLower()))
            {
                Panorama = false;
            }
        }
    }
}