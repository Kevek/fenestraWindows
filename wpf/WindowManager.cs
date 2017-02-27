using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using net.codingpanda.app.fenestra.utils;


namespace net.codingpanda.app.fenestra.wpf {
  public class WindowManagerWindowArgs {
    public WindowStartupLocation WindowStartupLocation { get; set; }
    public SizeToContent SizeToContent { get; set; }
    public bool AllowTransparency { get; set; }
    public WindowStyle WindowStyle { get; set; }
    public double Opacity { get; set; }
    public bool Topmost { get; set; }

    public WindowManagerWindowArgs() {
      WindowStartupLocation=WindowStartupLocation.Manual;
      SizeToContent=SizeToContent.WidthAndHeight;
      AllowTransparency=true;
      WindowStyle=WindowStyle.None;
      Opacity=1;
      Topmost=true;
    }

    public WindowManagerWindowArgs(WindowStartupLocation windowStartupLocation, SizeToContent sizeToContent,
      bool allowTransparency, WindowStyle windowStyle, double opacity, bool topmost) {
      WindowStartupLocation=windowStartupLocation;
      SizeToContent=sizeToContent;
      AllowTransparency=allowTransparency;
      WindowStyle=windowStyle;
      Opacity=opacity;
      Topmost=topmost;
    }

    public static WindowManagerWindowArgs CreateDefault() {
      return new WindowManagerWindowArgs(
        WindowStartupLocation.CenterScreen,
        SizeToContent.WidthAndHeight,
        false,
        WindowStyle.SingleBorderWindow, 
        1.0,
        false);
    }
  }

  public class WindowManager {
    public Window MainWindow { get; set; }
    private Dictionary<object, Window> KeyToWindow { get; } = new Dictionary<object, Window>();

    public IEnumerable<object> GetWindowKeys() {
      return KeyToWindow.Keys;
    }

    private Window CreateWindowBase(object key, WindowManagerWindowArgs args=null) {
      var args1=args ?? WindowManagerWindowArgs.CreateDefault();
      var newWindow=new Window {
        Content=key,
        WindowStartupLocation=args1.WindowStartupLocation,
        SizeToContent=args1.SizeToContent,
        AllowsTransparency=args1.AllowTransparency,
        WindowStyle=args1.WindowStyle,
        Opacity=args1.Opacity,
        Topmost=args1.Topmost,
        Background=Brushes.Transparent,
      };
      KeyToWindow.Add(key, newWindow);
      return newWindow;
    }

    public Window CreateWindow(object key, WindowManagerWindowArgs args=null) {
      var newWindow=CreateWindowBase(key, args);
      newWindow.Show();
      return newWindow;
    }

    public Window CreateCenteredWindow(object key, Screen screen) {
      var args=new WindowManagerWindowArgs(
        WindowStartupLocation.Manual,
        SizeToContent.WidthAndHeight,
        true,
        WindowStyle.None,
        1,
        true);
      var newWindow=CreateWindowBase(key, args);
      newWindow.Show();
      newWindow.Left=screen.Bounds.Left+screen.Bounds.Width/2-newWindow.Width/2;
      newWindow.Top=screen.Bounds.Top+screen.Bounds.Height/2-newWindow.Height/2;
      return newWindow;
    }

    public Window CreateHiddenWindow(object key) {
      var newWindow=CreateWindowBase(key);
      newWindow.Hide();
      return newWindow;
    }

    public void CloseWindow(object key) {
      Window window;
      if(KeyToWindow.TryGetValue(key, out window)) {
        KeyToWindow.Remove(key);
        window.Close();
      }
    }

    public void CloseAllSelectionWindows() {
      GlobalHotkeyUtil.RemoveEscHotkey(MainWindow);
      foreach(var key in KeyToWindow.Keys
        .OfType<FenestraResizeSelectionViewModel>()
        .ToArray()) {
        CloseWindow(key);
      }
    }

    public Window GetWindow(object key) {
      return KeyToWindow[key];
    }
  }
}
