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

namespace ShellChangeNotifierExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ChangeNotifierViewModel();
        }

        private void ShellChangeNotifier_Change(object sender, Jam.Shell.ChangeNotificationEventArgs e)
        {
            textBoxNotifications.AppendText(e.Event.ToString() + ": ");
            textBoxNotifications.AppendText(e.Path1);
            if (!String.IsNullOrEmpty(e.Path2))
                textBoxNotifications.AppendText(" -> " + e.Path2);
            textBoxNotifications.AppendText(Environment.NewLine);
            textBoxNotifications.ScrollToEnd();
        }

        private void textBoxNotifications_TextChanged(object sender, TextChangedEventArgs e)
        {
            logScroller.ScrollToEnd();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            textBoxNotifications.Clear();


        }
    }

    /// <summary>
    /// Example that demonstrates the usage of a <see cref="ShellChangeNotifier"/>. 
    /// <para>
    /// An instance is created and configured in <see cref="ChangeNotifierViewModel()"/>, attaching the eventhandler <see cref="OnShellChangeNotifierChange(object, ChangeNotificationEventArgs)"/>. 
    /// It prints out the received notifications in a textbox.
    /// </para>
    /// </summary>
    public class ChangeNotifierViewModel : INotifyPropertyChanged
    {
        #region members

        private Jam.Shell.WPF.Controls.ShellChangeNotifier m_ShellChangeNotifer;
        
        /// <summary>The <see cref="ShellChangeNotifier"/> instance central to this example.</summary>
        /// <value>A ShellChangeNotifier.</value>
        public Jam.Shell.WPF.Controls.ShellChangeNotifier ChangeNotifier
        {
            get { return m_ShellChangeNotifer; }
        }

        private string m_CaptionEvents;

        /// <summary>Collects the checked notification evens as comma seperated list.</summary>
        /// <value>A comma seperated list of events.</value>
        public string CaptionEvents
        {
            get { return m_CaptionEvents; }
            private set
            {
                m_CaptionEvents = value;
                RaisePropertyChanged("CaptionEvents");
            }
        }

        /// <summary>Collects the checked notification evens as comma seperated list.</summary>
        private void RefreshCaptionEvents()
        {
            string result = String.Empty;
            foreach (CheckedItem<NotificationEvents> lEvent in EventSelection)
            {
                if (lEvent.IsChecked)
                {
                    if (result != String.Empty) result += ", ";
                    result += Enum.GetName(typeof(NotificationEvents), lEvent.Item);
                }
            }
            CaptionEvents = result;
        }

        private string m_NotificationText;
        
        /// <summary>Holds the text that collects notifications.</summary>
        /// <value>The notification text.</value>
        public string NotificationText
        {
            get { return m_NotificationText; }
            set
            {
                m_NotificationText = value;
                RaisePropertyChanged("NotificationText");
            }
        }



        private ItemIdList m_WatchedDirectory = new ItemIdList(ShellFolder.Drives);
        
        /// <summary>Holds the folder to watch as ItemIdList .</summary>
        /// <value>The ItemIdList of the watched directory.</value>
        public ItemIdList WatchedDirectory
        {
            get { return m_WatchedDirectory; }
            set
            {
                m_WatchedDirectory = value;
                m_ShellChangeNotifer.ItemIdList = m_WatchedDirectory;
                
            }
        }
        /// <summary>A helping list that contains all events that can be watched for.</summary>
        /// <value>List of all events.</value>
        private IEnumerable<NotificationEvents> AllEvents
        {
            get
            {
                var result = Enum.GetValues(typeof(NotificationEvents)).Cast<NotificationEvents>().ToList();
                result.Remove(NotificationEvents.Unknown); //this is an internal value.
                return result;
            }
        }

        private IEnumerable<CheckedItem<NotificationEvents>> m_EventSelection;
        
        /// <summary>Contains a list of checkable NotificationEvents.</summary>
        /// <value>A list of CheckedItems containing a NotificationEvent and a checkstate.</value>
        public IEnumerable<CheckedItem<NotificationEvents>> EventSelection
        {
            get
            {
                if (m_EventSelection == null)
                {
                    m_EventSelection =

                        AllEvents.Select((pEvent, pIndex) =>
                            new CheckedItem<NotificationEvents>(pEvent,
                                (pItem, pIsChecked) => 
                                {
                                    //is executed, when an item in the list is checked or unchecked.
                                    if (pIsChecked)
                                        m_ShellChangeNotifer.EventFilter |= pItem;
                                    else
                                        m_ShellChangeNotifer.EventFilter ^= pItem;
                                    RefreshCaptionEvents(); //update the text of the combobox.
                                },
                                (m_ShellChangeNotifer.EventFilter & pEvent) == pEvent));
                }
                return m_EventSelection;
            }
        }
        #endregion

        #region constructor

        /// <summary>Default constructor. Especially creates and configures a <see cref="ShellChangeNotifier"/> instance.</summary>
        public ChangeNotifierViewModel()
        {
            m_ShellChangeNotifer = new Jam.Shell.WPF.Controls.ShellChangeNotifier();
            m_ShellChangeNotifer.EventFilter = NotificationEvents.FileCreate | NotificationEvents.FileChange | NotificationEvents.FileDelete | NotificationEvents.FileRename |
                NotificationEvents.FolderCreate | NotificationEvents.FolderRename | NotificationEvents.FolderDelete | NotificationEvents.FolderUpdate;
            RefreshCaptionEvents();
            m_ShellChangeNotifer.Change += OnShellChangeNotifierChange;

        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }
        #endregion

        #region ShellChangeNotifier
       
        /// <summary>Called when a notification matching the criteria (<see cref="EventSelection"/>, <see cref="WatchedDirectory"/>, <see cref="ShellChangeNotifier.Recursive"/>) is received.</summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event information to send to registered event handlers.</param>
        private void OnShellChangeNotifierChange(object sender, ChangeNotificationEventArgs e)
        {
            string lNewText = m_NotificationText
                + DateTime.Now.ToString() + " "
                + e.Event.ToString() + ": "
                + e.Path1;

            if (!String.IsNullOrEmpty(e.Path2))
                lNewText+= (" -> " + e.Path2);

            lNewText += Environment.NewLine;
            NotificationText = lNewText;

        }
        #endregion

    }
}
