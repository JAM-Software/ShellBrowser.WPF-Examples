using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Jam.Shell
{
    public class CheckedItem<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool m_IsChecked;
        private T m_Item;

        private ICommand m_Command; //Command that can be attached to a menuitem.
        private Action<T, bool> m_CheckStateChanged; //function that is executed when the checkstate of an item changes.

        public CheckedItem(T item, ICommand pCommand, bool isChecked = false)
        {
            this.m_Item = item;
            this.m_IsChecked = isChecked;
            this.m_Command = pCommand;
        }

        public CheckedItem(T pItem, Action<T, bool> pCheckStateChanged, bool pIsChecked = false)
        {
            m_Item = pItem;
            m_CheckStateChanged = pCheckStateChanged;
            m_IsChecked = pIsChecked;

        }


        public T Item
        {
            get
            {
                return m_Item;
            }
            set
            {
                m_Item = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Item"));
            }
        }

        public bool IsChecked
        {
            get
            {
                return m_IsChecked;
            }
            set
            {
                m_IsChecked = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                if (m_CheckStateChanged != null)
                {
                    m_CheckStateChanged(Item, m_IsChecked);
                }
            }
        }

        public ICommand Command
        {
            get
            {
                return m_Command;
            }
            set
            {
                m_Command = value;
            }
        }

        public override string ToString()
        {
            return m_Item.ToString();
        }
    }
}
