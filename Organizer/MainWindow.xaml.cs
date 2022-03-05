using System.Windows;
using System.Windows.Data;

namespace Organizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static CollectionViewSource cvs;
        public static CollectionViewSource cvsetiket;

        public MainWindow()
        {
            InitializeComponent();
            cvs = FindResource("Veriler") as CollectionViewSource;
            cvsetiket = FindResource("Etiketler") as CollectionViewSource;
        }
    }
}