using System.Collections.Generic;
using System.Windows.Input;
using net.codingpanda.app.fenestra.utils;


namespace net.codingpanda.app.fenestra {
  public class FenestraSettingsHotKeys {
    public List<Key> ModifierKeys { get; set; }
    public Key HotKey { get; set; }

    public FenestraSettingsHotKeys(List<Key> modifierKeys, Key hotKey) {
      ModifierKeys=modifierKeys;
      HotKey=hotKey;
    }

    public static FenestraSettingsHotKeys CreateDefault() {
      return new FenestraSettingsHotKeys(
        new List<Key> {
          Key.LeftShift,
          Key.LeftCtrl
        },
        Key.D);
    }
  }
}
