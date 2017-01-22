using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;


namespace net.codingpanda.app.fenestra.utils {
  public static class KeysUtil {
    public static Key GetCanonicalKey(this Key key) {
      switch(key) {
        case Key.LeftCtrl:
        case Key.RightCtrl:
          return Key.LeftCtrl;
        case Key.LeftAlt:
        case Key.RightAlt:
          return Key.LeftAlt;
        case Key.LWin:
        case Key.RWin:
          return Key.LWin;
        case Key.LeftShift:
        case Key.RightShift:
          return Key.LeftShift;
        default:
          return key;
      }
    }

    public static bool IsModifier(this Key key) {
      switch(GetCanonicalKey(key)) {
        case Key.LeftCtrl:

        case Key.LeftAlt:

        case Key.LWin:

        case Key.LeftShift:
          return true;
        default:
          return false;
      }
    }

    public static string GetDisplayText(this Key key) {
      switch(GetCanonicalKey(key)) {
        case Key.LeftCtrl:
          return "Ctrl";
        case Key.LeftAlt:
          return "Alt";
        case Key.LWin:
          return "Win";
        case Key.LeftShift:
          return "Shift";
        default:
          return key.ToString();
      }
    }

    public static IEnumerable<Key> GetAtMostOneNonModifier(this IEnumerable<Key> keys) {
      var seenNonModifier=false;
      foreach(var key in keys.OrderBy(x => x)) {
        if(key.IsModifier()) {
          yield return key;
        } else if(!seenNonModifier) {
          seenNonModifier=true;
          yield return key;
        }
      }
    }

    public static FenestraSettingsHotKeys SplitModifierKeys(this IEnumerable<Key> keys) {
      var keyLookup=keys.ToLookup(x => x.IsModifier());
      var nonModifiers=keyLookup[false].ToArray();
      return new FenestraSettingsHotKeys(
        keyLookup[true].ToList(),
        nonModifiers.Length!=1
          ? Key.None
          : nonModifiers[0]);
    }
  }
}
