﻿using Windows.ApplicationModel.Resources;

namespace Attention.UWP.Extensions
{
    public static class ResourceExtensions
    {
        private static readonly ResourceLoader _resLoader = new ResourceLoader();

        public static string GetLocalized(this string resourceKey)
        {
            return _resLoader.GetString(resourceKey);
        }
    }
}
