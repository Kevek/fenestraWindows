using System;
using System.Windows.Input;


namespace net.codingpanda.app.fenestra.wpf {
  public class CommandImpl : ICommand {
    private readonly Action action;

    public CommandImpl(Action action) {
      this.action=action;
    }

    public bool CanExecute(object parameter) => true;

    public void Execute(object parameter) {
      action?.Invoke();
    }

    public event EventHandler CanExecuteChanged;
  }
}
