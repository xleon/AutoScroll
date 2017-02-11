using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace AutoScroll
{
    // adapted from http://forums.xamarin.com/discussion/comment/23235/#Comment_23235

    public class AutoScrollHelper : IDisposable
    {
        private UIViewController _controller;
        private NSObject _willHideObserver;
        private NSObject _willShowObserver;

        public AutoScrollHelper(UIViewController controller)
        {
            _controller = controller;

            RegisterForKeyboardNotifications();
        }

        private void RegisterForKeyboardNotifications()
        {
            _willHideObserver = NSNotificationCenter.DefaultCenter
                .AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);

            _willShowObserver = NSNotificationCenter.DefaultCenter
                .AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
        }

        private void OnKeyboardNotification(NSNotification notification)
        {
            if (!_controller.IsViewLoaded) return;

            //Check if the keyboard is becoming visible
            var visible = notification.Name == UIKeyboard.WillShowNotification;

            //Start an animation, using values from the keyboard
            UIView.BeginAnimations("FollowKeyboard");
            UIView.SetAnimationBeginsFromCurrentState(true);
            UIView.SetAnimationDuration(UIKeyboard.AnimationDurationFromNotification(notification));
            UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(notification));

            //Pass the notification, calculating keyboard height, etc.
            var landscape = _controller.InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft
                            || _controller.InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;

            var keyboardFrame = visible
                ? UIKeyboard.FrameEndFromNotification(notification)
                : UIKeyboard.FrameBeginFromNotification(notification);

            OnKeyboardChanged(visible, landscape ? keyboardFrame.Width : keyboardFrame.Height);

            //Commit the animation
            UIView.CommitAnimations();
        }

        protected virtual void OnKeyboardChanged(bool visible, nfloat keyboardHeight)
        {
            var activeView = _controller.View.FindFirstResponder();
            var scrollView = activeView?.FindSuperviewOfType(_controller.View, typeof(UIScrollView)) as UIScrollView;

            if (scrollView == null)
                return;

            if (!visible)
            {
                scrollView.ContentInset = UIEdgeInsets.Zero;
                scrollView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
            }
            else
            {
                var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardHeight, 0.0f);
                scrollView.ContentInset = contentInsets;
                scrollView.ScrollIndicatorInsets = contentInsets;

                // Position of the active field relative isnside the scroll view
                var relativeFrame = activeView.Superview.ConvertRectToView(activeView.Frame, scrollView);

                var landscape = _controller.InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft
                                || _controller.InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;

                var spaceAboveKeyboard = (landscape ? scrollView.Frame.Width : scrollView.Frame.Height) - keyboardHeight;

                // Move the active field to the center of the available space
                var offset = relativeFrame.Y - (spaceAboveKeyboard - activeView.Frame.Height) / 2;
                scrollView.ContentOffset = new CGPoint(0, offset);
            }
        }

        public void Dispose()
        {
            if(_willHideObserver != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_willHideObserver);

            if(_willShowObserver != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_willShowObserver);

            _controller = null;
        }
    }
}