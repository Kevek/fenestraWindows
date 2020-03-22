using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Forms;


namespace net.codingpanda.app.fenestra.utils {
  public static class FenestraNotifyIconUtil {
    // Unfortunately I have been unable to find an elegant way to convince WPF to show a NotifyIcon
    //   So the below is sequestered in this util class
    public static void CreateNotifyIcon(Action openSettings) {
      var notifyIcon=new NotifyIcon {
        Icon=Properties.Resources.icon,
        Visible=true
      };
      var settingsMenuItem=new ToolStripMenuItem {
        Text="Settings"
      };
      settingsMenuItem.Click+=(s, e) => openSettings();

      var exitMenuItem=new ToolStripMenuItem() {
        Text="Exit"
      };
      exitMenuItem.Click+=(s, e) => {
        notifyIcon.Visible=false;
        Environment.Exit(0);
      };

      var contextMenu=new ContextMenuStrip();
      contextMenu.Items.Add(settingsMenuItem);
      contextMenu.Items.Add("-");
      contextMenu.Items.Add(exitMenuItem);
      notifyIcon.ContextMenuStrip=contextMenu;
    }
  }
}
