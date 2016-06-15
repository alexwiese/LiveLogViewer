using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LiveLogViewer.Helpers
{
    public class RelayCommand : RelayCommand<object>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand{T}" /> class.
        /// </summary>
        /// <param name="execute">The action to invoke when executed.</param>
        /// <param name="canExecute">The predicate that determines if the command can execute.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the execute parameter is null</exception>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : base(Execute(execute), o => canExecute == null || canExecute())
        {
        }

        public static Action<object> Execute(Action action)
        {
            return o => action();
        }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public async void Execute(object parameter)
        {
            await _execute()
                .ConfigureAwait(false);
            CommandManager.InvalidateRequerySuggested();
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, bool> _canExecute;

        private readonly Action<T> _execute;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand{T}" /> class.
        /// </summary>
        /// <param name="execute"> The action to invoke when executed. </param>
        /// <param name="canExecute"> The predicate that determines if the command can execute. </param>
        /// <exception cref="System.ArgumentNullException">Thrown if the execute parameter is null</exception>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            if (parameter == null || parameter is T)
            {
                return _canExecute == null || _canExecute((T)(parameter ?? default(T)));
            }

            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter == null || parameter is T)
            {
                _execute((T)(parameter ?? default(T)));
            }
        }
    }
}