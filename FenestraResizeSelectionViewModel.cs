using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using net.codingpanda.app.fenestra.utils;
using net.codingpanda.app.fenestra.wpf;
using Point = System.Drawing.Point;


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
      ForegroundWindowHeader=FenestraWindowUtil.GetForegroundWindowHeader(ForegroundHandle) ??
                             "Fenestra unable to load window name";
      var windowIconImage=FenestraWindowUtil.GetForegroundWindowIcon(ForegroundHandle);
      if(windowIconImage!=null) {
        var iconBitmap=new Bitmap(windowIconImage);
        ForegroundWindowIcon=System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
          iconBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
      } else {
        ForegroundWindowIcon=Imaging.CreateBitmapSourceFromHBitmap(
          Properties.Resources.unknown_icon.GetHbitmap(),
          IntPtr.Zero,
          Int32Rect.Empty,
          BitmapSizeOptions.FromEmptyOptions());
      }
    }

    private void InitGrid() {
      var args=Task.Run(() => FenestraSettingsUtil.LoadSettings()).Result;
      for(var i=0; i<args.Rows; i++) {
        var newRow=new FenestraSelectionRow();
        for(var j=0; j<args.Columns; j++) {
          newRow.Cells.Add(new FenestraSelectionCell());
        }
        Rows.Add(newRow);
      }
    }

    public void RepositionForegroundWindow() {
      var rowCount=Rows.Count;
      var columnCount=Rows.First()?.Cells.Count ?? 0;

      Point? newStartPos=null;
      Point? newEndPos=null;
      var screenRect=Screen.WorkingArea;
      for(var i=0; i<rowCount; i++) {
        for(var j=0; j<columnCount; j++) {
          var cell=Rows[i].Cells[j];
          if(cell.Selected) {
            if(newStartPos==null) {
              newStartPos=new Point(
                screenRect.Width*j/columnCount,
                screenRect.Height*i/rowCount);
            }
            newEndPos=new Point(
              screenRect.Width*(j+1)/columnCount,
              screenRect.Height*(i+1)/rowCount);
          }
        }
      }
      var hasHiddenBorder=FenestraWindowUtil.GetHiddenBorder(ForegroundHandle);
      var hiddenBorder=hasHiddenBorder
        ? 8
        : 0;
      if(newStartPos.HasValue) {
        var width=newEndPos.Value.X-newStartPos.Value.X+(2*hiddenBorder);
        var height=newEndPos.Value.Y-newStartPos.Value.Y+hiddenBorder;
        var screenX=Screen.Bounds.X+newStartPos.Value.X-hiddenBorder;
        var screenY=Screen.Bounds.Y+newStartPos.Value.Y;
        FenestraWindowUtil.ResizeGlobalWindow(
          ForegroundHandle,
          screenX, screenY,
          width, height);
      }

      WindowManager.CloseAllSelectionWindows();
    }
  }
}
