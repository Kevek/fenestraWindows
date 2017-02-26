using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms.VisualStyles;


namespace net.codingpanda.app.fenestra.utils {
  public static class ForegroundWindowUtil {
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
      if(GetWindowText(handle, sb, nChars)>0) {
        return sb.ToString();
      }
      return null;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll", EntryPoint="GetClassLong")]
    private static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint="GetClassLongPtr")]
    private static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

    private const uint WmGeticon=0x007f;
    private static readonly IntPtr IconSmall2=new IntPtr(2);
    private const int GclHicon=-14;

    private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex) {
      if(IntPtr.Size==4) {
        return new IntPtr(GetClassLong32(hWnd, nIndex));
      }
      return GetClassLong64(hWnd, nIndex);
    }

    public static Image GetForegroundWindowIcon(IntPtr handle) {
      try {
        var handle1=SendMessage(handle, WmGeticon, IconSmall2, IntPtr.Zero);
        if(handle1==IntPtr.Zero) {
          handle1=GetClassLongPtr(handle, GclHicon);
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
      ShowWindow = 0x0040
    }


    [DllImport("user32.dll", EntryPoint="SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

    private enum ShowWindowCommands {
      SW_RESTORE=9,
    }

    [DllImport("user32.dll")]
    [return : MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public static void ResizeGlobalWindow(IntPtr handle, int left, int top, int width, int height) {
      ShowWindow(handle, (int)ShowWindowCommands.SW_RESTORE);
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
      uint handleProcessId;
      GetWindowThreadProcessId(handle, out handleProcessId);
      return handleProcessId;
    }

    public static bool IsHandleOwnedByFenestraProcess(uint fenestraProcessId, IntPtr handle) {
      return fenestraProcessId==GetWindowThreadProcessId(handle);
    }
  }
}
