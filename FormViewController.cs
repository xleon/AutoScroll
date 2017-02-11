using System.Linq;
using Cirrious.FluentLayouts.Touch;
using UIKit;

namespace AutoScroll
{
    public class FormViewController : UIViewController
    {
        private UITapGestureRecognizer _gesture;
        private AutoScrollHelper _autoScrollHelper;
        private UIView _contentView;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			Title = "Auto Scroll Test";
            View.BackgroundColor = UIColor.White;
            EdgesForExtendedLayout = UIRectEdge.None;

            // Create containers
            _contentView = new UIView();
            var scrollView = new UIScrollView {_contentView};
            Add(scrollView);

            // Create form elements
			const int count = 7;
			for (var i = 1; i <= count; i++)
            {
                _contentView.Add(new UITextField
                {
                    Placeholder = $"Test {i}",
                    BorderStyle = UITextBorderStyle.RoundedRect,
                    Tag = i,
                    ReturnKeyType = i < count ? UIReturnKeyType.Send : UIReturnKeyType.Done
                });
            }

			var loginButton = new UIButton(UIButtonType.System);
            loginButton.SetTitle("Save", UIControlState.Normal);
            loginButton.TouchUpInside += (sender, args) => Save();
            
            _contentView.Add(loginButton);

            // Auto layout
            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            View.AddConstraints(scrollView.FullWidthOf(View));
            View.AddConstraints(scrollView.FullHeightOf(View));
            View.AddConstraints(
                _contentView.WithSameWidth(View),
                _contentView.WithSameHeight(View).SetPriority(UILayoutPriority.DefaultLow)
            );

            scrollView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            scrollView.AddConstraints(_contentView.FullWidthOf(scrollView));
            scrollView.AddConstraints(_contentView.FullHeightOf(scrollView));

			var formConstraints = _contentView
			    .VerticalStackPanelConstraints(new Margins(20), _contentView.Subviews);

            // very important to make scrolling work
            var bottomViewConstraint = _contentView.Subviews.Last().AtBottomOf(_contentView).Minus(20);

            _contentView.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            _contentView.AddConstraints(formConstraints);
			_contentView.AddConstraints(bottomViewConstraint);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Hide keyboard when user taps on the View background
            _gesture = View.DismissKeyboardOnTap();
            _autoScrollHelper = new AutoScrollHelper(this);

            for (var i = 0; i < _contentView.Subviews.Count(); i++)
            {
                var textField = _contentView.Subviews[i] as UITextField;
                if (textField != null)
                {
                    textField.Tag = i;
                    textField.ShouldReturn += ShouldReturn;
                }
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            _gesture.Dispose();
            _gesture = null;

            _autoScrollHelper.Dispose();
            _autoScrollHelper = null;

            for (var i = 0; i < _contentView.Subviews.Count(); i++)
            {
                var textField = _contentView.Subviews[i] as UITextField;
                if (textField != null)
                    textField.ShouldReturn -= ShouldReturn;
            }
        }

        private bool ShouldReturn(UITextField textField)
        {
            if (textField.ReturnKeyType == UIReturnKeyType.Done)
            {
                // we are done, hide the keyboard
                View.EndEditing(true);

                // nothing else to edit, why not just saving the form?
                Save();

                return false;
            }

            var nextTag = textField.Tag + 1;
            UIResponder nextControl = _contentView.ViewWithTag(nextTag);

            if (nextControl != null)
            {
                // set focus on the next control
                nextControl.BecomeFirstResponder();
            }
            else
            {
                // Not found, hide keyboard.
                View.EndEditing(true);
            }

            return false;
        }

        private void Save()
        {
            var alert = UIAlertController.Create("Success", "All good here", UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            PresentViewController(alert, true, null);
        }
    }
}