using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NotesWPF
{
    public partial class DetailsWindow : Window
    {
        public DetailsWindow(string ImagePath)
        {
            InitializeComponent();
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(ImagePath);
            bmp.EndInit();
            img.Source = bmp;
        }
    }
}
