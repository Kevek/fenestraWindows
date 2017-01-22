using System.Windows;
using net.codingpanda.app.fenestra.wpf;


namespace net.codingpanda.app.fenestra {
  public partial class App {
    private WindowManager WindowManager { get; } = new WindowManager();

    private void ApplicationStartup(object sender, StartupEventArgs e) {
      CreateHiddenMainWindow();
    }

    private void CreateHiddenMainWindow() {
      var fenestraViewModel=new FenestraViewModel(WindowManager);
      WindowManager.CreateHiddenWindow(fenestraViewModel);
      // Window needs to be created before Init may be called
      fenestraViewModel.Init();
    }
  }
}
