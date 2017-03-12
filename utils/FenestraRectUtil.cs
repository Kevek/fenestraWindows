using System.Windows;


namespace net.codingpanda.app.fenestra.utils {
  public static class FenestraRectUtil {
    public static bool Intersects(this Rect one, Rect two) {
      return !one.IsEmpty &&
             !two.IsEmpty && 
             one.Right>=two.Left &&
             one.Left<=two.Right &&
             one.Bottom>=two.Top &&
             one.Top<=two.Bottom;
    }
  }
}
