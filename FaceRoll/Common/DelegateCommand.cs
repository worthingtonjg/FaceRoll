using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace FaceRoll.Common
{
    public class DelegateCommand : ICommand
    {
        private static readonly Action<object> EmptyExecute = (o) => { };
        private static readonly Func<bool> EmptyCanExecute = () => true;

        private Action<object> execute;
        private Func<bool> canExecute;

        public DelegateCommand(Action<object> execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? EmptyExecute;
            this.canExecute = canExecute ?? EmptyCanExecute;
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

        public bool CanExecute()
        {
            return this.canExecute();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute();
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        void ICommand.Execute(object parameter)
        {
            this.Execute(parameter);
        }
    }

    static class CommandExtensions
    {
        public static void RaiseCanExecuteChanged(this ICommand self)
        {
            var delegateCommand = self as DelegateCommand;
            if (delegateCommand == null)
            {
                return;
            }

            delegateCommand.RaiseCanExecuteChanged();
        }

        public static void RaiseCanExecuteChanged(this IEnumerable<ICommand> self)
        {
            foreach (var command in self)
            {
                command.RaiseCanExecuteChanged();
            }
        }

        public static void RaiseCanExecuteChanged(params ICommand[] commands)
        {
            commands.RaiseCanExecuteChanged();
        }
    }
}