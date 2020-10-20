using Jam.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Jam.Shell.Com;
using System.Globalization;
using System.Collections.ObjectModel;
using Jam.Shell.WPF.Controls;
using System.Runtime.InteropServices;

namespace ExplorerBrowserSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TestAppViewModel m_ViewModel;
        public MainWindow()
        {
            InitializeComponent();
            m_ViewModel = new TestAppViewModel(explorerBrowser1);
            m_ViewModel.SpecialFolder = ShellFolder.Drives;
            DataContext = m_ViewModel;
        }

        private void explorerBrowser1_NavigationComplete(object sender, EventArgs e)
        {
            m_ViewModel.Columns = null;
        }

        private void FilterFiles_Click(object sender, RoutedEventArgs e)
        {
            explorerBrowser1.ContentFilter.ShowFiles = !explorerBrowser1.ContentFilter.ShowFiles;
        }

        private void FilterFolders_Click(object sender, RoutedEventArgs e)
        {
            explorerBrowser1.ContentFilter.ShowFolders = !explorerBrowser1.ContentFilter.ShowFolders;
        }

        private void FilterHidden_Click(object sender, RoutedEventArgs e)
        {
            explorerBrowser1.ContentFilter.ShowHidden = !explorerBrowser1.ContentFilter.ShowHidden;
        }

        private void FilterFileSystem_Click(object sender, RoutedEventArgs e)
        {
            explorerBrowser1.ContentFilter.FileSystemOnly = !explorerBrowser1.ContentFilter.FileSystemOnly;
        }

        private void FilterTextFiles_Click(object sender, RoutedEventArgs e)
        {
            //only shows txt - files
            if (String.IsNullOrEmpty(explorerBrowser1.ContentFilter.FilePatternFilter))
                explorerBrowser1.ContentFilter.FilePatternFilter = "*.txt";
            else
                explorerBrowser1.ContentFilter.FilePatternFilter = null;
        }

        private void ChooseFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem lMenuItem = e.Source as MenuItem;
            if (lMenuItem != null)
            {

                if (lMenuItem.Tag != null)
                    m_ViewModel.SpecialFolder = (ShellFolder)lMenuItem.Tag;
                else
                {
                    BrowseForFolder lOpenFolderDialog = new BrowseForFolder();
                    lOpenFolderDialog.SelectedFolderIdList = m_ViewModel.FolderIdList; //start at the current folder.
                    if (lOpenFolderDialog.ShowDialog() == true)
                    {
                        m_ViewModel.FolderIdList = lOpenFolderDialog.SelectedFolderIdList;
                    } 
                }
            }
        }

        private void SelectItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                explorerBrowser1.SelectItem(0);
            }
            catch (COMException)
            {
                // Control panel items cannot be selected
            }
        }

        private void SelectedItems_Click(object sender, RoutedEventArgs e)
        {
            SystemShellListItemCollection<BaseShellItem> lItems = explorerBrowser1.SelectedItems;

            foreach (BaseShellItem lItem in lItems)
                MessageBox.Show(lItem.AbsoluteItemIdList.DisplayPath);
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                explorerBrowser1.DeselectAll();
            }
            catch (COMException)
            {
                // Control panel items cannot be deselected
            }
        }

        private void SingleSelection_Click(object sender, RoutedEventArgs e)
        {
            if ((explorerBrowser1.ViewFolderFlags & FolderFlags.SingleSelection) == FolderFlags.SingleSelection)
                explorerBrowser1.ViewFolderFlags &= ~FolderFlags.SingleSelection;
            else
                explorerBrowser1.ViewFolderFlags |= FolderFlags.SingleSelection;
        }

        private void Items_Click(object sender, RoutedEventArgs e)
        {
            SystemShellListItemCollection<BaseShellItem> lItems = explorerBrowser1.Items;

            foreach (BaseShellItem lItem in lItems)
                MessageBox.Show(lItem.AbsoluteItemIdList.DisplayPath);
        }

        private void explorerBrowser1_ViewChanged(object sender, EventArgs e)
        {
            m_ViewModel.ViewModeChanged();
        }
    }

    public class TestAppViewModel : INotifyPropertyChanged
    {
        private Jam.Shell.WPF.Controls.ExplorerBrowser m_ExplorerBrowser;
        private ICommand m_ChangeViewModeCommand;
        private ICommand m_ChangeVisiblePanesCommand;
        private ICommand m_ChangeColumnsCommand;
        private ICommand m_ChangeColumnHeaderCommand;
        private ICommand m_ChangeCheckModeCommand;

        private ObservableCollection<IJamItemIdList> m_ShellComboItemsSource;

        public ObservableCollection<IJamItemIdList> ShellComboItemsSource
        {
            get
            {
                if (m_ShellComboItemsSource == null)
                {
                    m_ShellComboItemsSource = new ObservableCollection<IJamItemIdList>();
                    m_ShellComboItemsSource.Add(new ItemIdList(ShellFolder.Drives));
                    m_ShellComboItemsSource.Add(new ItemIdList(ShellFolder.Windows));
                    m_ShellComboItemsSource.Add(new ItemIdList(ShellFolder.Personal));
                    m_ShellComboItemsSource.Add(new ItemIdList(@"t:\shellBrowser"));

                }
                return m_ShellComboItemsSource;
            }
        }

        public TestAppViewModel(Jam.Shell.WPF.Controls.ExplorerBrowser pExplorerBrowser)
        {
            m_ExplorerBrowser = pExplorerBrowser;
            m_ExplorerBrowser.Loaded += ExplorerBrowser_Loaded;
            m_ChangeViewModeCommand = new RelayCommand((clickedViewMode) =>
            {
                CheckedItem<FolderViewMode> item = clickedViewMode as CheckedItem<FolderViewMode>;

                if (item.IsChecked)
                {
                    m_ExplorerBrowser.View = item.Item;
                    if (item.Item == FolderViewMode.Thumbnail)
                    {
                        m_ExplorerBrowser.ThumbnailSize = (int)IconSize.Thumbnail;
                    }
                    RaisePropertyChanged("ViewMode");
                }
            });

            m_ChangeVisiblePanesCommand = new RelayCommand((clickedPane) =>
            {
                CheckedItem<ExplorerPane> item = clickedPane as CheckedItem<ExplorerPane>;

                if (item.IsChecked)
                {
                    m_ExplorerBrowser.VisiblePanes |= item.Item;
                }
                else
                {
                    m_ExplorerBrowser.VisiblePanes ^= item.Item;
                }
                RaisePropertyChanged("VisiblePanes");
            });

            m_ChangeColumnsCommand = new RelayCommand((clickedColumn) =>
            {
                CheckedItem<ShellViewColumn> item = clickedColumn as CheckedItem<ShellViewColumn>;

                if (item.IsChecked)
                {
                    m_ExplorerBrowser.Columns.Add(item.Item);
                }
                else
                {
                    m_ExplorerBrowser.Columns.Remove(item.Item);
                }
            });

            m_ChangeColumnHeaderCommand = new RelayCommand((clickedColumnHeader) =>
            {
                CheckedItem<ExplorerShowHeader> item = clickedColumnHeader as CheckedItem<ExplorerShowHeader>;

                if (item.IsChecked)
                {
                    m_ExplorerBrowser.ShowHeader = item.Item;
                    RaisePropertyChanged("ShowColumnHeader");
                }
            });

            m_ChangeCheckModeCommand = new RelayCommand((clickedCheckMode) =>
            {
                CheckedItem<CheckMode> item = clickedCheckMode as CheckedItem<CheckMode>;

                if (item.IsChecked)
                {
                    m_ExplorerBrowser.CheckMode = item.Item;
                    RaisePropertyChanged("CheckMode");
                }
            });
        }

        private void ExplorerBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            // Columns need to be loaded delayed as the IShellView must be ready
            RaisePropertyChanged("Columns");
        }

        public IEnumerable<ExplorerPane> AllPanes
        {
            get
            {
                IEnumerable<ExplorerPane> lSupportedPanes = Enum.GetValues(typeof(ExplorerPane)).Cast<ExplorerPane>();

                lSupportedPanes = lSupportedPanes.Where((supportedPane) =>
                {
                    return (supportedPane != ExplorerPane.AddressBar) && (supportedPane != ExplorerPane.SearchEdit) && (supportedPane != ExplorerPane.History);
                });

                return lSupportedPanes;
            }
        }

        public IEnumerable<FolderViewMode> ViewModes
        {
            get { return Enum.GetValues(typeof(FolderViewMode)).Cast<FolderViewMode>(); }
        }

        public IEnumerable<ExplorerShowHeader> ShowColumnHeaders
        {
            get { return Enum.GetValues(typeof(ExplorerShowHeader)).Cast<ExplorerShowHeader>(); }
        }

        public IEnumerable<CheckMode> CheckModes
        {
            get
            {
                IEnumerable<CheckMode> lSupportedCheckModes = Enum.GetValues(typeof(CheckMode)).Cast<CheckMode>();

                lSupportedCheckModes = lSupportedCheckModes.Where((supportedCheckMode) =>
                {
                    return (supportedCheckMode != Jam.Shell.CheckMode.Checkboxes);
                });

                return lSupportedCheckModes;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }

        internal void ViewModeChanged()
        {
            RaisePropertyChanged("ViewMode");
        }

        public string FilePath
        {
            get { return FolderIdList == null ? String.Empty : FolderIdList.DisplayPath; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    FolderIdList = ItemIdList.Parse(value.Trim('"'));
            }
        }

        public ShellFolder SpecialFolder
        {
            get { return FolderIdList == null ? ShellFolder.Invalid : FolderIdList.SpecialFolder; }
            set
            {
                if (value != ShellFolder.Invalid && value != ShellFolder.FileSystemFolder &&
                    value != ShellFolder.Unknown)
                    FolderIdList = new ItemIdList(value);
            }
        }

        private ItemIdList m_FolderIdList;

        public ItemIdList FolderIdList
        {
            get { return m_FolderIdList; }
            set
            {
                m_FolderIdList = value;
                RaisePropertyChanged("FolderIdList");
                RaisePropertyChanged("SpecialFolder");
                RaisePropertyChanged("FilePath");
            }
        }

        public IEnumerable<CheckedItem<FolderViewMode>> ViewMode
        {
            get { return ViewModes.Select((viewMode, index) => new CheckedItem<FolderViewMode>(viewMode, m_ChangeViewModeCommand, (m_ExplorerBrowser.View == viewMode))); }
        }
        
        public IEnumerable<CheckedItem<ExplorerPane>> VisiblePanes
        {
            get { return AllPanes.Select((pane, index) => new CheckedItem<ExplorerPane>(pane, m_ChangeVisiblePanesCommand, (m_ExplorerBrowser.VisiblePanes & pane) == pane)); }
        }

        public IEnumerable<CheckedItem<ExplorerShowHeader>> ShowColumnHeader
        {
            get { return ShowColumnHeaders.Select((showHeader, index) => new CheckedItem<ExplorerShowHeader>(showHeader, m_ChangeColumnHeaderCommand, (m_ExplorerBrowser.ShowHeader == showHeader))); }
        }

        public IEnumerable<CheckedItem<CheckMode>> CheckMode
        {
            get { return CheckModes.Select((checkMode, index) => new CheckedItem<CheckMode>(checkMode, m_ChangeCheckModeCommand, (m_ExplorerBrowser.CheckMode == checkMode))); }
        }

        internal IEnumerable<CheckedItem<ShellViewColumn>> m_Columns;

        public IEnumerable<CheckedItem<ShellViewColumn>> Columns
        {
            get
            {
                if (m_Columns == null)
                {

                    if (m_ExplorerBrowser.IsLoaded)
                    {
                        m_Columns = m_ExplorerBrowser.AllColumns.Select((column, index) => new CheckedItem<ShellViewColumn>(column, m_ChangeColumnsCommand, (m_ExplorerBrowser.Columns.Contains(column))));
                        RaisePropertyChanged("Columns");
                    }
                }
                return m_Columns;
            }

            set
            {
                m_Columns = value;
                RaisePropertyChanged("Columns");
            }
        }
    }

}
