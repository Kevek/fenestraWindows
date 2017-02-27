using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;


namespace net.codingpanda.app.fenestra.utils {
  public static class GlobalHotkeyUtil {
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
  }
}
