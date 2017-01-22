using System.Collections.Generic;
using System.Windows.Input;


namespace net.codingpanda.app.fenestra {
  public class FenestraSettingsArgs {
    public int Columns { get; set; }
    public int Rows { get; set; }
    public FenestraSettingsHotKeys HotKeys { get; set; }
    public bool StartAtLogin { get; set; }

    public static FenestraSettingsArgs CreateDefault() {
      return new FenestraSettingsArgs {
        Columns=6,
        Rows=4,
        HotKeys=FenestraSettingsHotKeys.CreateDefault(),
        StartAtLogin=false,
      };
    }
  }
}
