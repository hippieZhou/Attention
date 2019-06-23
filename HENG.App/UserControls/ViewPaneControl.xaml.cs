﻿using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace HENG.App.UserControls
{
    public sealed partial class ViewPaneControl : UserControl
    {
        public ViewPaneControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CompositionPropertySet scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(MainScrollViewer);
            Compositor compositor = scrollerPropertySet.Compositor;

            // Get the visual that represents our HeaderTextBlock 
            // And define the progress animation string
            var headerVisual = ElementCompositionPreview.GetElementVisual(ScrollHeader);
            String progress = "Clamp(Abs(scroller.Translation.Y) / 100.0, 0.0, 1.0)";

            // Create the expression and add in our progress string.
            var textExpression = compositor.CreateExpressionAnimation("Lerp(1.5, 1, " + progress + ")");
            textExpression.SetReferenceParameter("scroller", scrollerPropertySet);

            // Shift the header by 50 pixels when scrolling down
            var offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * 50");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            // Logo scale and transform
            var logoHeaderScaleAnimation = compositor.CreateExpressionAnimation("Lerp(Vector2(1,1), Vector2(0.5, 0.5), " + progress + ")");
            logoHeaderScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);

            var logoVisual = ElementCompositionPreview.GetElementVisual(HeaderLogo);
            logoVisual.StartAnimation("Scale.xy", logoHeaderScaleAnimation);

            var logoVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 50, {progress})");
            logoVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Offset.Y", logoVisualOffsetAnimation);

            // Offset the header title
            Visual textVisual = ElementCompositionPreview.GetElementVisual(HeaderText);
            Vector3 finalOffset = new Vector3(-45, 22, 0);
            var headerOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0,0,0), finalOffset, {progress})");
            headerOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            headerOffsetAnimation.SetVector3Parameter("finalOffset", finalOffset);
            textVisual.StartAnimation(nameof(Visual.Offset), headerOffsetAnimation);
        }
    }
}
