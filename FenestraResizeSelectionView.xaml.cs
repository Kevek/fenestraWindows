using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace net.codingpanda.app.fenestra {
  public partial class FenestraResizeSelectionView {
    private FenestraSelectionCellView[] cells;

    private Point startPos;
    private bool isMouseDown;

    private const string SelectionCellName="selectionCell";

    public FenestraResizeSelectionView() {
      InitializeComponent();

      Loaded+=(s, e) => {
        cells=GetCells(selectionGrid)
          .Select(cell => {
            var transform=cell.TransformToAncestor(selectionGrid);
            var topLeft=transform.Transform(new Point(0, 0));
            var topRight=transform.Transform(new Point(cell.ActualWidth, 0));
            var bottomLeft=transform.Transform(new Point(0, cell.ActualHeight));
            var bottomRight=transform.Transform(new Point(cell.ActualWidth, cell.ActualHeight));

            return new FenestraSelectionCellView(
              cell.DataContext as FenestraSelectionCell,
              topLeft,
              topRight,
              bottomLeft,
              bottomRight);
          })
          .Where(x => x.ViewModel!=null)
          .ToArray();
      };
    }

    private static IEnumerable<Border> GetCells(DependencyObject parent) {
      for(var i=0; i<VisualTreeHelper.GetChildrenCount(parent); i++) {
        var child=VisualTreeHelper.GetChild(parent, i);
        var cell=child as Border;
        if(cell?.Name==SelectionCellName) {
          yield return cell;
        } else {
          foreach(var childCell in GetCells(child)) {
            yield return childCell;
          }
        }
      }
    }

    private void GridMouseDown(object sender, MouseButtonEventArgs e) {
      isMouseDown=true;
      // Record start position of the mouse on the selectionGrid
      startPos=e.GetPosition(selectionGrid);
      // Start watching mouse movement
      selectionGrid.CaptureMouse();
      // Show the selection box
      Canvas.SetLeft(selectionBox, startPos.X);
      Canvas.SetTop(selectionBox, startPos.Y);
      selectionBox.Width=0;
      selectionBox.Height=0;
      selectionBox.Visibility=Visibility.Visible;
    }

    private void GridMouseUp(object sender, MouseButtonEventArgs e) {
      isMouseDown=false;
      // Stop watching mouse movement
      selectionGrid.ReleaseMouseCapture();
      // Resize window based on cell selection
      (DataContext as FenestraResizeSelectionViewModel)?.RepositionForegroundWindow();
      selectionBox.Visibility=Visibility.Collapsed;
    }

    private void GridMouseMove(object sender, MouseEventArgs e) {
      if(!isMouseDown) {
        return;
      }

      var currentPos=e.GetPosition(selectionGrid);
      // Handle selection box x-axis
      if(startPos.X<=currentPos.X) {
        Canvas.SetLeft(selectionBox, startPos.X);
        selectionBox.Width=currentPos.X-startPos.X;
      } else {
        Canvas.SetLeft(selectionBox, currentPos.X);
        selectionBox.Width=startPos.X-currentPos.X;
      }

      // Handle selection box y-axis
      if(startPos.Y<=currentPos.Y) {
        Canvas.SetTop(selectionBox, startPos.Y);
        selectionBox.Height=currentPos.Y-startPos.Y;
      } else {
        Canvas.SetTop(selectionBox, currentPos.Y);
        selectionBox.Height=startPos.Y-currentPos.Y;
      }

      // Get top-left and bottom-right points of the selection box within the selectionGrid
      var upperLeft=new Point(Math.Min(startPos.X, currentPos.X), Math.Min(startPos.Y, currentPos.Y));
      var bottomRight=new Point(Math.Max(startPos.X, currentPos.X), Math.Max(startPos.Y, currentPos.Y));
      foreach(var cell in cells) {
        // For each cell in the view model, test if it is intersected or within the selection box
        cell.ViewModel.Selected=false;
        cell.TestContainsOrContained(upperLeft, bottomRight);
      }
    }


    private class FenestraSelectionCellView {
      public FenestraSelectionCell ViewModel { get; }
      private Point TopLeft { get; }
      private Point TopRight { get; }
      private Point BottomLeft { get; }
      private Point BottomRight { get; }

      private double Top => TopLeft.Y;
      private double Left => TopLeft.X;
      private double Bottom => BottomRight.Y;
      private double Right => BottomRight.X;

      public FenestraSelectionCellView(FenestraSelectionCell viewModel,
        Point topLeft, Point topRight, Point bottomLeft, Point bottomRight) {
        ViewModel=viewModel;
        TopLeft=topLeft;
        TopRight=topRight;
        BottomLeft=bottomLeft;
        BottomRight=bottomRight;
      }

      public void TestContainsOrContained(Point selectionTopLeft, Point selectionBottomRight) {
        // Is one of this cell's verticies contained within our selection?
        if(ContainsPoint(selectionTopLeft, selectionBottomRight, TopLeft)) {
          ViewModel.Selected=true;
          return;
        }
        if(ContainsPoint(selectionTopLeft, selectionBottomRight, TopRight)) {
          ViewModel.Selected=true;
          return;
        }
        if(ContainsPoint(selectionTopLeft, selectionBottomRight, BottomLeft)) {
          ViewModel.Selected=true;
          return;
        }
        if(ContainsPoint(selectionTopLeft, selectionBottomRight, BottomRight)) {
          ViewModel.Selected=true;
          return;
        }

        // Selection is contained within cell
        if(selectionTopLeft.X>=Left &&
           selectionBottomRight.X<=Right &&
           selectionTopLeft.Y>=Top &&
           selectionBottomRight.Y<=Bottom) {
          ViewModel.Selected=true;
          return;
        }

        // Selection is contained horizontally within cell (or cell contains selection terminal)
        if(((selectionTopLeft.X<=Right && selectionBottomRight.X>=Left) ||
            ContainsPoint(TopLeft, TopRight, selectionTopLeft) ||
            ContainsPoint(TopLeft, TopRight, selectionBottomRight)) &&
           selectionTopLeft.Y>=Top &&
           selectionBottomRight.Y<=Bottom) {
          ViewModel.Selected=true;
          return;
        }

        // Selection is contained vertically within cell (or cell contains selection terminal)
        if(((selectionTopLeft.Y<=Bottom && selectionBottomRight.Y>=Top) ||
            ContainsPoint(TopLeft, TopRight, selectionTopLeft) ||
            ContainsPoint(TopLeft, TopRight, selectionBottomRight)) &&
           selectionTopLeft.X>=Left &&
           selectionBottomRight.X<=Right) {
          ViewModel.Selected=true;
          return;
        }
      }

      private static bool ContainsPoint(Point containgTopLeft, Point contaningTopRight, Point testPoint) {
        if(testPoint.X>=containgTopLeft.X &&
           testPoint.Y>=containgTopLeft.Y &&
           testPoint.X<=contaningTopRight.X &&
           testPoint.Y<=contaningTopRight.Y) {
          return true;
        }
        return false;
      }
    }
  }
}
