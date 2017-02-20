using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using net.codingpanda.app.fenestra.utils;
using net.codingpanda.app.fenestra.wpf;


namespace net.codingpanda.app.fenestra {
  public class FenestraSelectionCell : NotifyPropertyChangedBase {
    private bool selected;

    public bool Selected {
      get { return selected; }
      set { SetValue(ref selected, value); }
    }
  }


  public class FenestraSelectionRow : NotifyPropertyChangedBase {
    public ObservableCollection<FenestraSelectionCell> Cells { get; } =
      new ObservableCollection<FenestraSelectionCell>();
  }


  public class FenestraResizeSelectionViewModel : NotifyPropertyChangedBase {
    public Screen Screen { get; }
    public WindowManager WindowManager { get; }
    public IntPtr ForegroundHandle { get; private set; }

    private string foregroundWindowHeader;

    public string ForegroundWindowHeader {
      get { return foregroundWindowHeader; }
      set { SetValue(ref foregroundWindowHeader, value); }
    }

    private BitmapSource foregroundWindowIcon;

    public BitmapSource ForegroundWindowIcon {
      get { return foregroundWindowIcon; }
      set { SetValue(ref foregroundWindowIcon, value); }
    }

    public ObservableCollection<FenestraSelectionRow> Rows { get; } =
      new ObservableCollection<FenestraSelectionRow>();

    public ICommand CloseCommand { get; }

    public FenestraResizeSelectionViewModel(Screen screen, WindowManager windowManager) {
      Screen=screen;
      WindowManager=windowManager;

      CloseCommand=new CommandImpl(() => WindowManager.CloseAllSelectionWindows());

      InitGrid();
    }

    public void LoadForegroundWindowInfo(IntPtr newForegroundHandle) {
      ForegroundHandle=newForegroundHandle;
      ForegroundWindowHeader=ForegroundWindowUtil.GetForegroundWindowHeader(ForegroundHandle);
      var iconBitmap=new Bitmap(ForegroundWindowUtil.GetForegroundWindowIcon(ForegroundHandle));
      ForegroundWindowIcon=System.Windows.Interop.Imaging
        .CreateBitmapSourceFromHBitmap(iconBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
          BitmapSizeOptions.FromEmptyOptions());
    }

    private void InitGrid() {
      var args=Task.Run(() => FenestraSettingsUtil.LoadSettings()).Result;
      for(var i=0; i<args.Columns; i++) {
        var newRow=new FenestraSelectionRow();
        for(var j=0; j<args.Rows; j++) {
          newRow.Cells.Add(new FenestraSelectionCell());
        }
        Rows.Add(newRow);
      }
    }

    public void RepositionForegroundWindow() {
      var rowCount=Rows.Count;
      var columnCount=Rows.First()?.Cells.Count ?? 0;

      Tuple<double, double> newStartPos=null;
      Tuple<double, double> newEndPos=null;
      for(var i=0; i<rowCount; i++) {
        for(var j=0; j<columnCount; j++) {
          var cell=Rows[i].Cells[j];
          if(cell.Selected) {
            if(newStartPos==null) {
              newStartPos=Tuple.Create(((double)Screen.WorkingArea.Width)*j/columnCount,
                (double)Screen.WorkingArea.Height*i/rowCount);
            }
            newEndPos=Tuple.Create((double)Screen.WorkingArea.Width*(j+1)/columnCount,
              (double)Screen.WorkingArea.Height*(i+1)/rowCount);
          }
        }
      }
      if(newStartPos!=null) {
        var width=newEndPos.Item1-newStartPos.Item1;
        var height=newEndPos.Item2-newStartPos.Item2;
        ForegroundWindowUtil.ResizeGlobalWindow(
          ForegroundHandle,
          (int)newStartPos.Item1, (int)newStartPos.Item2,
          (int)width, (int)height);
      }

      WindowManager.CloseAllSelectionWindows();
    }
  }
}
