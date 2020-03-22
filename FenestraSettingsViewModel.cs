using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using net.codingpanda.app.fenestra.utils;
using net.codingpanda.app.fenestra.wpf;


namespace net.codingpanda.app.fenestra {
  public class FenestraSettingsViewModel : NotifyPropertyChangedBase {
    private int rows;

    public int Rows {
      get => rows;
      set => SetValue(ref rows, value);
    }

    private int columns;

    public int Columns {
      get => columns;
      set => SetValue(ref columns, value);
    }

    private bool startAtLogin;

    public bool StartAtLogin {
      get => startAtLogin;
      set => SetValue(ref startAtLogin, value);
    }

    public ObservableCollection<Key> HotKeys { get; } = new ObservableCollection<Key>();

    private string hotKeysDisplay;

    public string HotKeyDisplay {
      get => hotKeysDisplay;
      set => SetValue(ref hotKeysDisplay, value);
    }

    public ICommand SaveCommand { get; }

    public event Action SettingsSaved;

    public FenestraSettingsViewModel() {
      PropertyChanged+=(s, e) => {
        switch(e.PropertyName) {
          case nameof(Rows):
            if(Rows<2) {
              Rows=2;
            } else if(Rows>24) {
              Rows=24;
            }
            break;
          case nameof(Columns):
            if(Columns<2) {
              Columns=2;
            } else if(Columns>24) {
              Columns=24;
            }
            break;
        }
      };

      HotKeys.CollectionChanged+=(s, e) => {
        var keys=new List<Key>();
        foreach(Key key in Enum.GetValues(typeof(Key))) {
          if(HotKeys.Contains(key) && key!=Key.Escape) {
            keys.Add(key);
          }
        }
        var fenestraKeys=keys.SplitModifierKeys();
        var modifierKeyDisplayText=fenestraKeys.ModifierKeys.Count>0
          ? fenestraKeys.ModifierKeys
            .Select(x => x.GetDisplayText())
            .Aggregate((s1, s2) => $"{s1}-{s2}")
          : "";
        var hotKeyDisplayText=fenestraKeys.HotKey.GetDisplayText();
        HotKeyDisplay=fenestraKeys.HotKey==Key.None
          ? ""
          : $"{modifierKeyDisplayText} {hotKeyDisplayText}";
      };

      SaveCommand=new CommandImpl(SaveSettings);

      LoadSettings();
    }

    private void SaveSettings() {
      var args=new FenestraSettingsArgs {
        Rows=Rows,
        Columns=Columns,
        HotKeys=HotKeys.SplitModifierKeys(),
        StartAtLogin=StartAtLogin,
      };
      FenestraSettingsUtil.SaveSettings(args);
      SettingsSaved?.Invoke();
    }

    private void LoadSettings() {
      var extantSettings=FenestraSettingsUtil.LoadSettings();
      Rows=extantSettings.Rows;
      Columns=extantSettings.Columns;
      foreach(var modifierKey in extantSettings.HotKeys.ModifierKeys) {
        HotKeys.Add(modifierKey);
      }
      HotKeys.Add(extantSettings.HotKeys.HotKey);
      StartAtLogin=extantSettings.StartAtLogin;
    }
  }
}
