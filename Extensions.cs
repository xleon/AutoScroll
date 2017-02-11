using System;
using System.Linq;
using UIKit;

namespace AutoScroll
{
    public static class Extensions
    {
        public static UITapGestureRecognizer DismissKeyboardOnTap(this UIView view)
        {
            // Add gesture recognizer to hide keyboard
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => view.EndEditing(true));
            tap.ShouldReceiveTouch = (recognizer, touch) => !(touch.View is UIControl);

            view.AddGestureRecognizer(tap);

            return tap;
        }

        public static UIView FindFirstResponder(this UIView view)
        {
            if (view.IsFirstResponder)
                return view;

            return view.Subviews
                .Select(subView => subView.FindFirstResponder())
                .FirstOrDefault(firstResponder => firstResponder != null);
        }

        public static UIView FindSuperviewOfType(this UIView view, UIView stopAt, Type type)
        {
            if (view.Superview == null)
                return null;

            if (type.IsInstanceOfType(view.Superview))
                return view.Superview;

            return !Equals(view.Superview, stopAt) ? view.Superview.FindSuperviewOfType(stopAt, type) : null;
        }

        public static UIView FindTopSuperviewOfType(this UIView view, UIView stopAt, Type type)
        {
            var superview = view.FindSuperviewOfType(stopAt, type);
            var topSuperView = superview;
            while (superview != null && !Equals(superview, stopAt))
            {
                superview = superview.FindSuperviewOfType(stopAt, type);
                if (superview != null)
                    topSuperView = superview;
            }

            return topSuperView;
        }
    }
}