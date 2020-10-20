using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace JamExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region members

        private Jam.Shell.ShellControlConnector m_ShellControlConnector = new Jam.Shell.ShellControlConnector();

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            shellAddressBar.ShellControlConnector = m_ShellControlConnector;
            shellTree.ShellControlConnector = m_ShellControlConnector;
            shellList.ShellControlConnector = m_ShellControlConnector;
            shellFilePreview.ShellControlConnector = m_ShellControlConnector;
            shellThumbnail.ShellControlConnector = m_ShellControlConnector;
            shellTree.SpecialFolder = Jam.Shell.ShellFolder.Personal; //use ShellControlConnector to sync other components.

        }

        private void MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            shellTree.FullRefresh();
        }
        private void MenuItem_Refresh_ShellList_Click(object sender, RoutedEventArgs e)
        {
            shellList.FullRefresh();
            
        }
        private void MenuItem_CreateDir_ShellList_Click(object sender, RoutedEventArgs e)
        {
            shellList.Control.CreateDir("", true);
        }

        
        private void MultiRoots_Click(object sender, RoutedEventArgs e)
        {
            shellTree.MultipleRoots = Jam.Shell.MultipleRoots.MultipleRoots;

        }

        private void RootAtDesktop_Click(object sender, RoutedEventArgs e)
        {
            shellTree.RootedAt = Jam.Shell.ShellFolder.Desktop;
        }

        private void RootAtThisPC_Click(object sender, RoutedEventArgs e)
        {
            shellTree.RootedAt = Jam.Shell.ShellFolder.Drives;
        }

        private void RootAtC_Click(object sender, RoutedEventArgs e)
        {

            shellTree.RootedAtFileSystemFolder = "c:\\";
        }

        private void CustomRoots_Click(object sender, RoutedEventArgs e)
        {

            shellTree.MultipleRoots = Jam.Shell.MultipleRoots.MultipleRoots;
            shellTree.Control.ClearRoots();
            shellTree.Control.AddRoot(Jam.Shell.ShellFolder.MyMusic);
            shellTree.Control.AddRoot(Jam.Shell.ShellFolder.MyPictures);
            shellTree.Control.AddRoot("c:\\");
        }

        private void Checkboxes_Checked(object sender, RoutedEventArgs e)
        {
            shellList.CheckBoxes = true;
        }

        private void Checkboxes_Unchecked(object sender, RoutedEventArgs e)
        {
            shellList.CheckBoxes = false;
        }

        private void ShowSelected_Click(object sender, RoutedEventArgs e)
        {
            if (m_ShellControlConnector.SelectionList.Count == 0)
            {
                MessageBox.Show("Nothing is checked.");
            }
            else
            {
                MessageBox.Show(m_ShellControlConnector.SelectionList.ToString());
            }
        }

        private void CustomTreeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(shellTree.SelectedNode.Text + " was clicked.");
            
        }

        private GridLength m_PreviousPreviewWidth, m_PreviousThumbnailHeight;
        private void TogglePreview_Click(object sender, RoutedEventArgs e)
        {
            if (shellFilePreview.Visibility == Visibility.Visible)
            {
                //hide
                m_PreviousPreviewWidth = panelPreview.Width;
                shellFilePreview.Visibility = Visibility.Hidden;
                panelPreview.Width = new GridLength(0);
                panelPreviewSplitter.Width = new GridLength(0);
                ((MenuItem)sender).IsChecked = false;
            }
            else
            {
                //show
                shellFilePreview.Visibility = Visibility.Visible;
                panelPreview.Width = m_PreviousPreviewWidth;
                panelPreviewSplitter.Width = new GridLength(5);
                ((MenuItem)sender).IsChecked = true;

            }
        }

        private void ToggleThumbnail_Click(object sender, RoutedEventArgs e)
        {
            if (shellThumbnail.Visibility == Visibility.Visible)
            {
                //hide
                m_PreviousThumbnailHeight = panelThumbnail.Height;
                shellThumbnail.Visibility = Visibility.Hidden;
                panelThumbnail.Height = new GridLength(0);
                ((MenuItem)sender).IsChecked = false;
            }
            else
            {
                //show
                shellThumbnail.Visibility = Visibility.Visible;
                panelThumbnail.Height = m_PreviousThumbnailHeight;
                ((MenuItem)sender).IsChecked = true;

            }
        }

    }
}
