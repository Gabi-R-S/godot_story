using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    internal static class MyDrawable
    {

    public static void FadeIn(this CanvasItem node, float duration, Action? callback = null)
    {
        node.Show(); // ensure visible
        node.Modulate = new Color(1, 1, 1, 0); // start transparent

        var tween = node.CreateTween();
        tween.TweenProperty(node, "modulate:a", 1.0f, duration)
             .SetTrans(Tween.TransitionType.Linear)
             .SetEase(Tween.EaseType.InOut);

        // optional user callback after finished
        if (callback != null)
            tween.Finished += callback;
    }

    public static void FadeOut(this CanvasItem node, float duration, Action? callback = null)
    {
        var tween = node.CreateTween();
        tween.TweenProperty(node, "modulate:a", 0.0f, duration)
             .SetTrans(Tween.TransitionType.Linear)
             .SetEase(Tween.EaseType.InOut);

        // Always restore alpha and hide, then call user callback (if any).
        tween.Finished += () =>
        {
            node.Modulate = new Color(1, 1, 1, 1); // restore alpha for next show
            node.Hide();
            callback?.Invoke();
        };
    }

    public static void MoveTo(this CanvasItem node, Vector2 position, float duration, Action? callback = null)
    {
        var tween = node.CreateTween();
        if (callback != null)
        {
            tween.Finished += callback;
        }
        tween.TweenProperty(node, "position", position, duration).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.InOut);
    }
}
