using PropertyChanged;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Organizer.Model
{
    [XmlRoot(ElementName = "Etiket")]
    public class Etiket : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [XmlAttribute(AttributeName = "Açıklama")]
        public string Açıklama { get; set; }

        [XmlAttribute(AttributeName = "Id")]
        public int Id { get; set; }

        [XmlIgnore]
        public bool Seçili { get; set; }
    }

    [XmlRoot(ElementName = "Etiketler")]
    public class Etiketler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [XmlElement(ElementName = "Etiket")]
        public ObservableCollection<Etiket> Etiket { get; set; }
    }

    [XmlRoot(ElementName = "Veri")]
    public class Veri : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [XmlIgnore]
        [DependsOn("DosyaAdı")]
        public string Dosya { get => Path.GetFileNameWithoutExtension(DosyaAdı); }

        [XmlAttribute(AttributeName = "DosyaAdı")]
        public string DosyaAdı { get; set; }

        [XmlElement(ElementName = "Etiket")]
        public ObservableCollection<Etiket> Etiket { get; set; }
    }

    [XmlRoot(ElementName = "Veriler")]
    public class Veriler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [XmlElement(ElementName = "Etiketler")]
        public Etiketler Etiketler { get; set; }

        [XmlElement(ElementName = "Veri")]
        public ObservableCollection<Veri> Veri { get; set; }
    }
}