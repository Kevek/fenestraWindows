using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;


namespace net.codingpanda.app.fenestra.utils {
  public static class FenestraNotifyIconUtil {
    // Unfortunately I have been unable to find an elegant way to convince WPF to show a NotifyIcon
    //   So the below is sequestered in this util class
    public static void CreateNotifyIcon(Action openSettings) {
      var entryLocation=Assembly.GetEntryAssembly().Location;
      var entryDirectory=Path.GetDirectoryName(entryLocation) ?? "";
      var notifyIcon=new NotifyIcon {
        Icon=new Icon(Path.Combine(entryDirectory, "resources/icon.ico")),
        Visible=true
      };
      var settingsMenuItem=new MenuItem {
        Text="Settings"
      };
      settingsMenuItem.Click+=(s, e) => openSettings();

      var exitMenuItem=new MenuItem("Exit");
      exitMenuItem.Click+=(s, e) => {
        notifyIcon.Visible=false;
        Environment.Exit(0);
      };

      var contextMenu=new ContextMenu();
      contextMenu.MenuItems.Add(settingsMenuItem);
      contextMenu.MenuItems.Add("-");
      contextMenu.MenuItems.Add(exitMenuItem);
      notifyIcon.ContextMenu=contextMenu;
    }
  }
}
