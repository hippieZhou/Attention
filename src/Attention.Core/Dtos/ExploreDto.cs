﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace Attention.Core.Dtos
{
    public class ExploreDto : BaseDto
    {
        public static IEnumerable<ExploreDto> FakeData => colors.Select(x => new ExploreDto
        {
            Background = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.Backdrop,
                TintColor = x,
                FallbackColor = x,
                TintOpacity = 1.0,
                TintLuminosityOpacity = 1.0,
            },
            Thumbnail = $"ms-appx:///Assets/Explore/Avatar0{random.Next(0, 5)}.png",
            Title = DateTime.Now.ToString()
        });

        public string Title { get; set; }
        public Brush Background { get; set; }
        public string Thumbnail { get; set; }
    }
}
