using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShellThumbnail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = ((ComboBox)sender).SelectedItem as ComboBoxItem;
            if (selectedItem == null || selectedItem.Tag == null)
            {
                shellThumb.ImageSize = null;
                shellThumb.Stretch = Stretch.Uniform;
                shellThumb.StretchDirection = StretchDirection.DownOnly;
                shellThumb.ClearValue(WidthProperty);
                shellThumb.ClearValue(HeightProperty);
            }
            else
            {
                if (selectedItem.Tag is int)
                {
                    shellThumb.BeginUpdate();
                    shellThumb.Width = shellThumb.Height = (int)selectedItem.Tag;
                    shellThumb.ImageSize = null;
                    shellThumb.EndUpdate();

                }
                else
                {
                    shellThumb.ClearValue(WidthProperty);
                    shellThumb.ClearValue(HeightProperty);

                    shellThumb.ImageSize = (Jam.Shell.SystemImageListSize)selectedItem.Tag;
                }
                shellThumb.Stretch = Stretch.None; //set Stretch to none if a fixed size is set.

            }
        }
    }

    public class BoolToContentTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Jam.Shell.WPF.Controls.ShellThumbnail.ContentType)
            {
                if (((Jam.Shell.WPF.Controls.ShellThumbnail.ContentType)value) == Jam.Shell.WPF.Controls.ShellThumbnail.ContentType.PreferIcon)
                    return true;
            }
            return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Boolean)
            {
                if (((bool)value))
                {
                    return Jam.Shell.WPF.Controls.ShellThumbnail.ContentType.PreferIcon;
                }
                else
                    return Jam.Shell.WPF.Controls.ShellThumbnail.ContentType.Auto;
            }
            return Jam.Shell.WPF.Controls.ShellThumbnail.ContentType.Auto;

        }
    }
}
