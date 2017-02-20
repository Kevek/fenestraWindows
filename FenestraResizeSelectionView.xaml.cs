using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using net.codingpanda.app.fenestra.utils;


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
            var bottomRight=transform.Transform(new Point(cell.ActualWidth, cell.ActualHeight));

            return new FenestraSelectionCellView(cell.DataContext as FenestraSelectionCell, topLeft, bottomRight);
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
      var rect=new Rect(
        new Point(Math.Min(startPos.X, currentPos.X), Math.Min(startPos.Y, currentPos.Y)),
        new Point(Math.Max(startPos.X, currentPos.X), Math.Max(startPos.Y, currentPos.Y)));

      foreach(var cell in cells) {
        // For each cell in the view model, test if it is intersected or within the selection box
        cell.SelectionIntersects(rect);
      }
    }


    private class FenestraSelectionCellView {
      public FenestraSelectionCell ViewModel { get; }
      private Point TopLeft { get; }
      private Point BottomRight { get; }
      private Rect Rect { get; }

      public FenestraSelectionCellView(FenestraSelectionCell viewModel,
        Point topLeft, Point bottomRight) {
        ViewModel=viewModel;
        TopLeft=topLeft;
        BottomRight=bottomRight;
        Rect=new Rect(TopLeft, BottomRight);
      }

      public void SelectionIntersects(Rect selectionRect) {
        ViewModel.Selected=Rect.Intersects(selectionRect);
      }
    }
  }
}
