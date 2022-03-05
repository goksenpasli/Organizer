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

        public MainWindow()
        {
            InitializeComponent();
            cvs = FindResource("Veriler") as CollectionViewSource;
        }
    }
}