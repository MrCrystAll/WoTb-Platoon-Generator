using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GénérateurWot
{
    public class AsyncCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        private bool _isExecuting;

        private bool _IsExecuting
        {
            get => _isExecuting;
            set
            {
                _isExecuting = value;
                OnCanExecuteChanged();
            }
        }
        
        public AsyncCommand(Func<Task> execute): this(execute, () => true){}

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return !(_isExecuting && _canExecute());
        }

        public event EventHandler CanExecuteChanged;

        public async void Execute(object parameter)
        {
            _IsExecuting = true;

            try
            {
                await _execute();
            }
            finally
            {
                _IsExecuting = false;
            }
        }
        
        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}