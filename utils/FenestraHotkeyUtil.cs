using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;


namespace net.codingpanda.app.fenestra.utils {
  public static class FenestraHotkeyUtil {
    [DllImport("User32.dll")]
    private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

    [DllImport("User32.dll")]
    private static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);

    private static HwndSource HwndSource;
    private const int WmHotkey=0x0312;
    private const int HotkeyId=9000;
    private const int EscapeHotkeyId=9001;
    public static event Action OnHotkeyPressed;
    public static event Action OnEscapePressed;

    public static void SetupHotkey(Window parentWindow, FenestraSettingsHotKeys keys) {
      // Get hWnd (or create if window hasn't been shown)
      var hWnd=new WindowInteropHelper(parentWindow).EnsureHandle();
      HwndSource=HwndSource.FromHwnd(hWnd);
      if(HwndSource==null) {
        Environment.Exit(-1);
      }
      HwndSource.AddHook(HwndHook);
      RegisterHotKey(parentWindow, keys.ModifierKeys, keys.HotKey);
    }

    public static void SetupEscHotkey(Window parentWindow) {
      var hWnd=new WindowInteropHelper(parentWindow).EnsureHandle();
      RegisterHotKey(hWnd, EscapeHotkeyId, 0, (uint)KeyInterop.VirtualKeyFromKey(Key.Escape));
    }

    public static void RemoveHotkey(Window parentWindow) {
      HwndSource.RemoveHook(HwndHook);
      HwndSource=null;
      UnregisterHotKey(parentWindow);
    }

    public static void RemoveEscHotkey(Window parentWindow) {
      var hWnd=new WindowInteropHelper(parentWindow).EnsureHandle();
      UnregisterHotKey(hWnd, EscapeHotkeyId);
    }

    private static void RegisterHotKey(Window parentWindow, IEnumerable<Key> modifierKeys, Key hotKey) {
      var hWnd=new WindowInteropHelper(parentWindow).EnsureHandle();
      uint modifierKeyInt=0x0;
      foreach(var modifierKey in modifierKeys) {
        switch(modifierKey.GetCanonicalKey()) {
          case Key.LeftCtrl:
            modifierKeyInt|=0x0002;
            break;
          case Key.LeftAlt:
            modifierKeyInt|=0x0001;
            break;
          case Key.LWin:
            modifierKeyInt|=0x0008;
            break;
          case Key.LeftShift:
            modifierKeyInt|=0x0004;
            break;
        }
      }

      RegisterHotKey(hWnd, HotkeyId, modifierKeyInt, (uint)KeyInterop.VirtualKeyFromKey(hotKey));
    }

    private static void UnregisterHotKey(Window parentWindow) {
      var hWnd=new WindowInteropHelper(parentWindow).EnsureHandle();
      UnregisterHotKey(hWnd, HotkeyId);
    }

    private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      switch(msg) {
        case WmHotkey:
          switch(wParam.ToInt32()) {
            case HotkeyId:
              OnHotkeyPressed?.Invoke();
              handled=true;
              break;
            case EscapeHotkeyId:
              OnEscapePressed?.Invoke();
              handled=true;
              break;
          }
          break;
      }
      return IntPtr.Zero;
    }

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
