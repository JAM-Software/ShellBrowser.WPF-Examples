using System;
using System.Windows.Input;

namespace Jam.Shell
{
    public class RelayCommand : ICommand
    {
        private Action<object> m_Execute;
        private Func<object, bool> m_CanExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            m_Execute = execute;
            m_CanExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return m_CanExecute == null || m_CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            m_Execute(parameter);
        }
    }
}
