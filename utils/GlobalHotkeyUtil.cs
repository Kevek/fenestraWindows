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
    private const int HotkeyId=9000;
    public static event Action OnHotkeyPressed;

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

    public static void RemoveHotkey(Window parentWindow) {
      HwndSource.RemoveHook(HwndHook);
      HwndSource=null;
      UnregisterHotKey(parentWindow);
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

      if(!RegisterHotKey(hWnd, HotkeyId, modifierKeyInt, (uint)KeyInterop.VirtualKeyFromKey(hotKey))) {
        // TODO: handle error
      }
    }

    private static void UnregisterHotKey(Window parentWindow) {
      var helper=new WindowInteropHelper(parentWindow);
      UnregisterHotKey(helper.Handle, HotkeyId);
    }

    private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      const int wmHotkey=0x0312;
      switch(msg) {
        case wmHotkey:
          switch(wParam.ToInt32()) {
            case HotkeyId:
              OnHotkeyPressed?.Invoke();
              handled=true;
              break;
          }
          break;
      }
      return IntPtr.Zero;
    }
  }
}
