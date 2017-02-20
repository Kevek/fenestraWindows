using System;
using System.Collections.Generic;
using System.Windows.Input;
using net.codingpanda.app.fenestra.utils;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;


namespace net.codingpanda.app.fenestra {
  public partial class FenestraSettingsView {
    public FenestraSettingsView() {
      InitializeComponent();
    }

    private void OnHotKeyDown(object sender, KeyEventArgs e) {
      var vm=DataContext as FenestraSettingsViewModel;
      if(vm==null) {
        return;
      }
      vm.HotKeys.Clear();
      var newKeys=new List<Key>();
      foreach(Key key in Enum.GetValues(typeof(Key))) {
        if(key==Key.None) {
          continue;
        }
        if(Keyboard.IsKeyDown(key)) {
          newKeys.Add(key);
        }
      }
      foreach(var key in newKeys.GetAtMostOneNonModifier()) {
        vm.HotKeys.Add(key);
      }
    }
  }
}
