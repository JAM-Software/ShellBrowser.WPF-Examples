using Jam.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Jam.Shell.WPF.Controls;

namespace Preview
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
            m_ViewModel = new TestAppViewModel();
            m_ViewModel.Folder = new ItemIdList(ShellFolder.Documents);
            DataContext = m_ViewModel;

        }
        private void listView1_SelectionChanged(
           object sender, SelectionChangedEventArgs e)
        {
            //Synchronize the viewmodel
            m_ViewModel.SelectedFile = listView1.SelectedValue as ItemIdList;

            m_ViewModel.SelectedItems.Clear();
            if (listView1.SelectedItems != null && listView1.SelectedItems.Count > 0)
            {
                m_ViewModel.SelectedItems.AddRange(listView1.SelectedItems.Cast<ItemIdList>());
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ItemIdList lSelected = listView1.SelectedValue as ItemIdList;
            m_ViewModel.Open(lSelected);
        }

        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (m_ViewModel.Folder != null)
                {
                    m_ViewModel.Folder = m_ViewModel.Folder.Parent;
                }
            }
            else if (e.Key == Key.Enter)
            {
                ItemIdList lSelected = listView1.SelectedValue as ItemIdList;
                m_ViewModel.Open(lSelected);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            int lByteSize = 0;
            foreach (ItemIdList lElement in listView1.SelectedItems)
            {
                object lValue = lElement.GetPropertyVariantValue(SHCOLUMNID.ShellColumnSize);
                if ( lValue != null)
                {
                    lByteSize += Convert.ToInt32(lValue);
                }
            }

            string lMbSize = (lByteSize / (double)Math.Pow(1024, 2)).ToString("0.00");
            MessageBox.Show(String.Format("Number of selected files: {0} \n Size: {1} MB", listView1.SelectedItems.Count, lMbSize));
        }

        private void FilePreview_LoadPreview(object sender, LoadPreviewEventArgs e)
        {
            Guid TXT_PREVIEW = new Guid("1531d583-8375-4d3f-b5fb-d23bbd169f22");
            Guid WEB_PREVIEW = new Guid("{d99d682b-76e3-471e-8f43-afc1ec720414}");
            string file = e.Path;
            if (file == null)
            {
                return;
            }
            if (".xyz".Equals(System.IO.Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
            {
                e.PreviewHandlerGuid = TXT_PREVIEW;
            }
            else if (".pas".Equals(System.IO.Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
            {
                e.CustomPreviewHandler = new CustomPreviewHandler();
            }
            else if (WEB_PREVIEW.Equals(e.PreviewHandlerGuid))
            {
                // Like Windows Explorer we do not use this PreviewHandler (used for e.g. .htm, .xml, .jer, .eml files). 
                // While it works for some files, for others it'll show a SaveAs message. 
                e.PreviewHandlerGuid = null;
            }
            else if (e.PreviewHandlerGuid == null)
            {
                //add actions to take if no registered PreviewHandler is available.
            }
        }
    }

    class CustomPreviewHandler : IShellPreviewHandler
    {
        private TextBox m_TextBox;

        private ShellFilePreview parent;
        public bool Load(ICommonPreviewHandlerHost p_Parent, string p_Path, ItemIdList p_AbsolutePidl)
        {
            parent = (p_Parent as ShellFilePreview);
            ScrollViewer scroll = new ScrollViewer();
            m_TextBox = new TextBox();
            m_TextBox.Text = System.IO.File.ReadAllText(p_Path);
            scroll.Content = m_TextBox;
            parent.SetCustomPreviewHandler(scroll);
            return true;
        }

        public void Resize() {}

        public void Show() {}

        public void Unload()
        {
            parent.RemoveCustomPreviewHandler();
        }
    }

    public class TestAppViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }

        #endregion INotifiyPropertyChanged

        #region commands
        /// <summary>Command to demonstrate the usage of commands in custom menu items.</summary>
        private class OpenCmdPromptCommand : ICommand
        {
#pragma warning disable 67
            public event EventHandler CanExecuteChanged;
#pragma warning restore 67

            public bool CanExecute(object parameter) => true; 

            public void Execute(object parameter)
            {
                ItemIdList lElement = parameter as ItemIdList;
                if (ItemIdList.IsNullOrInvalid(lElement))
                    return;


                string lPath = lElement.IsFolder ? lElement.Path : lElement.ParentPath;

                System.Diagnostics.ProcessStartInfo lProcess = new System.Diagnostics.ProcessStartInfo("cmd.exe");
                lProcess.Arguments = String.Format(" /k cd /d {0}", lPath);
                System.Diagnostics.Process.Start(lProcess);
            }
        }

        private ICommand m_OpenCmdPrompt;

        public ICommand OpenCmdPrompt
        {
            get
            {
                return m_OpenCmdPrompt ?? (m_OpenCmdPrompt = new OpenCmdPromptCommand());
            }
        }

        #endregion

        #region members
        private ItemIdList m_Folder;
        private ItemIdList m_SelectedFile;
        private JamItemIdListCollection m_Items;
        private JamItemIdListCollection m_SelectedItems = new JamItemIdListCollection();
        #endregion  members

        #region properties

        public ItemIdList Folder
        {
            get { return m_Folder; }
            set
            {
                if (m_Folder != value)
                {
                    m_Folder = value;
                    try
                    {
                        m_Items = JamItemIdListCollection.GetChildren(m_Folder);
                        m_Items.Sort();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        m_Items = new JamItemIdListCollection(false);
                    }
                    RaisePropertyChanged("Folder");
                    RaisePropertyChanged("Items");
                    SelectedFile = null;
                }
            }
        }
        public ItemIdList SelectedFile
        {
            get { return m_SelectedFile; }
            set
            {
                m_SelectedFile = value;
                RaisePropertyChanged("SelectedFile");
            }
        }

        public JamItemIdListCollection Items
        {
            get { return m_Items; }
        }

        public JamItemIdListCollection SelectedItems
        {
            get
            {
                return m_SelectedItems;
            }
        }

        #endregion properties

        #region methods

        public void Open(ItemIdList pItemIdList)
        {
            if (pItemIdList == null)
                return;

            if (pItemIdList.IsFolder)
            {
                Folder = pItemIdList;
            }
            else
            {
                new Jam.Shell.WPF.Controls.ShellContextMenuProvider(pItemIdList).ExecuteCommand(ShellCommand.Open);
            }


        }
        #endregion methods

    }

}
