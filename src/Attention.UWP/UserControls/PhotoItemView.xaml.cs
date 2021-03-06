﻿using Attention.UWP.ViewModels;
using System;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace Attention.UWP.UserControls
{
    public sealed partial class PhotoItemView : UserControl
    {
        public PhotoItemView()
        {
            this.InitializeComponent();
            destinationFooterElement.Translation += new Vector3(0, 0, 32);
        }

        public PhotoItemViewModel ViewModel
        {
            get { return (PhotoItemViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(PhotoItemViewModel), typeof(PhotoItemView), new PropertyMetadata(null,(d,e)=> 
            {
                if (d is PhotoItemView handler && e.NewValue is PhotoItemViewModel viewmodel)
                {
                    viewmodel.Initialize(handler.destinationElement ?? handler.FindName("destinationElement") as Grid);
                }
            }));

        private void DestinationElement_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }

        private void DestinationElement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            destinationElement_Transform.TranslateX += e.Delta.Translation.X;
            destinationElement_Transform.TranslateY += e.Delta.Translation.Y;
        }

        private void DestinationElement_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            DoubleAnimation CreateTranslateAnimation(EasingFunctionBase easingFunction, double from, double to = 0, double duration = 600)
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    EasingFunction = easingFunction,
                    From = from,
                    To = to,
                    Duration = new Duration(TimeSpan.FromMilliseconds(duration))
                };
                return animation;
            }

            var ef = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            DoubleAnimation translateXAnimation = CreateTranslateAnimation(ef, destinationElement_Transform.TranslateX);
            DoubleAnimation translateYAnimation = CreateTranslateAnimation(ef, destinationElement_Transform.TranslateY);

            Storyboard.SetTarget(translateXAnimation, destinationElement_Transform);
            Storyboard.SetTarget(translateYAnimation, destinationElement_Transform);
            Storyboard.SetTargetProperty(translateXAnimation, "CompositeTransform.TranslateX");
            Storyboard.SetTargetProperty(translateYAnimation, "CompositeTransform.TranslateY");

            Storyboard sb = new Storyboard();
            sb.Children.Add(translateXAnimation);
            sb.Children.Add(translateYAnimation);
            sb.Begin();
        }
    }
}
