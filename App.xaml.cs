using System;
using System.Windows;
using net.codingpanda.app.fenestra.wpf;


namespace net.codingpanda.app.fenestra {
  public partial class App {
    private WindowManager WindowManager { get; } = new WindowManager();

    private void ApplicationStartup(object sender, StartupEventArgs e) {
      SetupErrorWriter();
      CreateHiddenMainWindow();
    }

    private void CreateHiddenMainWindow() {
      var fenestraViewModel=new FenestraViewModel(WindowManager);
      WindowManager.CreateHiddenWindow(fenestraViewModel);
      // Window needs to be created before Init may be called
      fenestraViewModel.Init();
    }

    private void SetupErrorWriter() {
      AppDomain.CurrentDomain.UnhandledException+=(s, e) => {
        var exception=e.ExceptionObject as Exception;
        if(exception!=null) {
          Dispatcher.Invoke(() => {
            MessageBox.Show(
              $@"An exception has occurred in Fenestra, it will now close.

The error message is:
{exception}",
              "Fenestra Exception", MessageBoxButton.OK);
          });
          Environment.Exit(-1);
        }
      };
      Dispatcher.UnhandledException+=(s, e) => {
        e.Handled=true;
        var exception=e.Exception;
        if(!IsNonAncestorError(exception)) {
          var result=
            MessageBox.Show(
              $@"An exception has occurred in Fenestra. Would you like to exit?

The error message is:
{exception}", "Fenestra Exception", MessageBoxButton.YesNo);
          if(result==MessageBoxResult.Yes) {
            Environment.Exit(-1);
          }
        }
      };
    }

    private const string NonAncestorError="The specified Visual is not an ancestor of this Visual.";

    public static bool IsNonAncestorError(Exception error) {
      return error is InvalidOperationException && (error.Message==NonAncestorError);
    }
  }
}
