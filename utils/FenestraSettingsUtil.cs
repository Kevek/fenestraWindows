using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;


namespace net.codingpanda.app.fenestra.utils {
  public static class FenestraSettingsUtil {
    private const string SettingsFileName="settings.json";
    private const string SettingsPath="AppSettings\\Local\\Fenestra";
    private const string StartAtLoginRegistryKey="SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
    private const string FenestraRegistryKeyName="Fenestra";

    private static string GetFenestraSettingsPath() {
      var homePath=Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      return Path.Combine(homePath, SettingsPath);
    }

    private static RegistryKey GetStartAtLoginRegistryKey() {
      return Registry.CurrentUser.OpenSubKey(StartAtLoginRegistryKey, true);
    }

    public static void SaveSettings(FenestraSettingsArgs settingsArgs) {
      var path=GetFenestraSettingsPath();
      if(!Directory.Exists(path)) {
        Directory.CreateDirectory(path);
      }
      var filePath=Path.Combine(GetFenestraSettingsPath(), SettingsFileName);
      using(var sw=new StreamWriter(new FileStream(filePath, FileMode.Create))) {
        var settingsJson=JsonConvert.SerializeObject(settingsArgs);
        sw.Write(settingsJson);
        sw.Flush();
      }
    }

    public static FenestraSettingsArgs LoadSettings() {
      var filePath=Path.Combine(GetFenestraSettingsPath(), SettingsFileName);
      if(!File.Exists(filePath)) {
        return FenestraSettingsArgs.CreateDefault();
      }
      try {
        using(var sr=new StreamReader(new FileStream(filePath, FileMode.Open))) {
          var settingsJson=sr.ReadToEnd();
          return JsonConvert.DeserializeObject<FenestraSettingsArgs>(settingsJson);
        }
      } catch(Exception) {
        return FenestraSettingsArgs.CreateDefault();
      }
    }

    public static void ApplySettings(FenestraSettingsArgs args, Window mainWindow) {
      FenestraHotkeyUtil.SetupHotkey(mainWindow, args.HotKeys);
      var registryKey=GetStartAtLoginRegistryKey();
      if(args.StartAtLogin) {
        var applicationPath=Assembly.GetEntryAssembly().Location;
        if(applicationPath!=null) {
          registryKey.SetValue(FenestraRegistryKeyName, applicationPath);
        }
      } else {
        registryKey.DeleteValue(FenestraRegistryKeyName, false);
      }
    }
  }
}