using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;


namespace net.codingpanda.app.fenestra.utils {
  public static class FenestraWindowUtil {
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    public static IntPtr GetForegroundWindowHandle() {
      return GetForegroundWindow();
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    public static string GetForegroundWindowHeader(IntPtr handle) {
      const int nChars=256;
      var sb=new StringBuilder(nChars);
      return GetWindowText(handle, sb, nChars)>0 
        ? sb.ToString() 
        : null;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll", EntryPoint="GetClassLong")]
    private static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint="GetClassLongPtr")]
    private static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

    private const uint WmGetIcon=0x007f;
    private static readonly IntPtr IconSmall2=new IntPtr(2);
    private const int GclHIcon=-14;

    private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex) {
      return IntPtr.Size==4 
        ? new IntPtr(GetClassLong32(hWnd, nIndex))
        : GetClassLong64(hWnd, nIndex);
    }

    public static Image GetForegroundWindowIcon(IntPtr handle) {
      try {
        var handle1=SendMessage(handle, WmGetIcon, IconSmall2, IntPtr.Zero);
        if(handle1==IntPtr.Zero) {
          handle1=GetClassLongPtr(handle, GclHIcon);
        }
        if(handle1==IntPtr.Zero) {
          handle1=LoadIcon(IntPtr.Zero, (IntPtr)0x7F00);
        }
        return handle1!=IntPtr.Zero
          ? new Bitmap(Icon.FromHandle(handle1).ToBitmap(), 16, 16)
          : null;
      } catch {
        return null;
      }
    }


    [Flags]
    private enum WindowFlags {
      NoZOrder = 0X4,
    }


    [DllImport("user32.dll", EntryPoint="SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

    private enum ShowWindowCommands {
      SwRestore=9,
    }

    [DllImport("user32.dll")]
    [return : MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public static void ResizeGlobalWindow(IntPtr handle, int left, int top, int width, int height) {
      ShowWindow(handle, (int)ShowWindowCommands.SwRestore);
      SetWindowPos(handle, 0, left, top, width, height, (int)(WindowFlags.NoZOrder));
    }


    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
      WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    private const uint WinEventOutOfContext=0;
    private const uint EventSystemForeground=3;


    public delegate void WinEventDelegate(
      IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
      uint dwmsEventTime);


    public class ForegroundWindowEventInfo {
      public WinEventDelegate WinEventDelegate;
      public IntPtr Handle;

      public ForegroundWindowEventInfo(WinEventDelegate winEventDelegate, IntPtr handle) {
        WinEventDelegate=winEventDelegate;
        Handle=handle;
      }
    }


    public static ForegroundWindowEventInfo AddHookToForegroundWindowEvent(Action onForegroundWindowChanged) {
      var hookDelegate=new WinEventDelegate(
        (hWinEventHook, eventType, hWnd, idObject, idChild, dwEventThread, dwmsEventTime) => onForegroundWindowChanged());
      var hookHandle=SetWinEventHook(EventSystemForeground, EventSystemForeground,
        IntPtr.Zero, hookDelegate, 0, 0, WinEventOutOfContext);
      return new ForegroundWindowEventInfo(hookDelegate, hookHandle);
    }

    public static void RemoveHookToForegroundWindowEvent(IntPtr handle) {
      UnhookWinEvent(handle);
    }


    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    public static uint GetWindowThreadProcessId(IntPtr handle) {
      GetWindowThreadProcessId(handle, out var handleProcessId);
      return handleProcessId;
    }

    public static bool IsHandleOwnedByFenestraProcess(uint fenestraProcessId, IntPtr handle) {
      return fenestraProcessId==GetWindowThreadProcessId(handle);
    }


    public enum DwmWindowAttribute {
      ExtendedFrameBounds=9,
    }


    [DllImport(@"dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out Rect pvAttribute, int cbAttribute);

    [DllImport(@"user32.dll")]
    [return : MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);


    public static bool GetHiddenBorder(IntPtr handle) {
      if(DwmGetWindowAttribute(handle, (int)DwmWindowAttribute.ExtendedFrameBounds,
           out var rect1, Marshal.SizeOf(typeof(Rect)))>=0) {
        if(GetWindowRect(handle, out var rect2)) {
          return !rect1.Equals(rect2);
        }
      }
      return true;
    }

    // Hide Windows from Alt-Tab from http://stackoverflow.com/a/551847/921904
    public static void HideWindowFromAltTab(IntPtr handle) {
      var extantStyle=(int)GetWindowLong(handle, (int)GetWindowLongFields.GwlExstyle);
      var newStyle=extantStyle|(int)ExtendedWindowStyles.WsExToolwindow;
      SetWindowLong(handle, (int)GetWindowLongFields.GwlExstyle, (IntPtr)newStyle);
    }

    [DllImport("user32.dll", EntryPoint="SetWindowLongPtr", SetLastError=true)]
    private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);


    [DllImport("user32.dll", EntryPoint="SetWindowLong", SetLastError=true)]
    private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

    [DllImport("kernel32.dll", EntryPoint="SetLastError")]
    public static extern void SetLastError(int dwErrorCode);


    [Flags]
    public enum ExtendedWindowStyles {
      WsExToolwindow = 0x00000080,
    }


    public enum GetWindowLongFields {
      GwlExstyle = -20,
    }


    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);


    public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
      int error;
      IntPtr result;
      // Clear any errors
      SetLastError(0);

      if(IntPtr.Size==4) {
        // use SetWindowLong
        var tempResult=IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
        error=Marshal.GetLastWin32Error();
        result=new IntPtr(tempResult);
      } else {
        // use SetWindowLongPtr
        result=IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
        error=Marshal.GetLastWin32Error();
      }

      if((result==IntPtr.Zero) && (error!=0)) {
        throw new System.ComponentModel.Win32Exception(error);
      }
      return result;
    }


    private static int IntPtrToInt32(IntPtr intPtr) {
      return unchecked((int)intPtr.ToInt64());
    }
  }
}
