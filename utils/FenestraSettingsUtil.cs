using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace net.codingpanda.app.fenestra.utils {
  public static class FenestraSettingsUtil {
    private const string SettingsFileName="settings.json";
    private const string SettingsPath="AppSettings\\Local\\Fenestra";

    private static string GetFenestraSettingsPath() {
      var homePath=Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      return Path.Combine(homePath, SettingsPath);
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
  }
}