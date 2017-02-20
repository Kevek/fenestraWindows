using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace net.codingpanda.app.fenestra.wpf {
  public class NotifyPropertyChangedBase : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) {
      var handler=PropertyChanged;
      handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName=null) {
      if(EqualityComparer<T>.Default.Equals(field, value) || propertyName==null) {
        return false;
      }
      field=value;
      OnPropertyChanged(propertyName);
      return true;
    }
  }
}
