﻿using Attention.App.Extensions;
using Attention.App.UserControls;
using Microsoft.Xaml.Interactivity;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Attention.App.Behaviors
{
    public class ImageScrollBehavior : DependencyObject, IBehavior
    {
        private const int _opacityMaxValue = 250;
        private const int _alpha = 255;
        private const int _maxFontSize = 42;
        private const int _minFontSize = 24;
        private const int scrollViewerThresholdValue = 190;
        private ScrollViewer scrollViewer;
        private ListViewBase listGridView;

        public DependencyObject AssociatedObject { get; private set; }

        public Control TargetControl
        {
            get { return (Control)GetValue(TargetControlProperty); }
            set { SetValue(TargetControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetControlProperty =
            DependencyProperty.Register("TargetControl", typeof(Control), typeof(ImageScrollBehavior), new PropertyMetadata(null));

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;
            if (!GetScrollViewer())
            {
                ((ListViewBase)associatedObject).Loaded += ListGridView_Loaded;
            }
        }

        private void ListGridView_Loaded(object sender, RoutedEventArgs e)
        {
            GetScrollViewer();
            listGridView = sender as ListViewBase;
        }

        private bool GetScrollViewer()
        {
            scrollViewer = UIHelper.GetDescendantsOfType<ScrollViewer>(AssociatedObject).FirstOrDefault();
            if (scrollViewer != null)
            {
                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
                return true;
            }
            return false;
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            double verticalOffset = ((ScrollViewer)sender).VerticalOffset;
            var header = (PageHeader)TargetControl;
            header.BackgroundColorOpacity = verticalOffset / _opacityMaxValue;
            header.AcrylicOpacity = 0.3 * (1 - (verticalOffset / _opacityMaxValue));
            if (verticalOffset < 10)
            {
                header.BackgroundColorOpacity = 0;
                header.FontSize = 42;
                header.Foreground = new SolidColorBrush(Colors.White);
                header.AcrylicOpacity = 0.3;
            }
            else if (verticalOffset > scrollViewerThresholdValue)
            {
                header.FontSize = _minFontSize;
            }
            else
            {
                header.FontSize = -(((verticalOffset / scrollViewerThresholdValue) * (_maxFontSize - _minFontSize)) - _maxFontSize);
            }
        }

        public void Detach()
        {
            ((ListViewBase)AssociatedObject).Loaded -= ListGridView_Loaded;
            AssociatedObject = null;
        }
    }
}
