﻿using Attention.UWP.Background;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Windows.ApplicationModel.Background;
using Windows.UI.Popups;

namespace Attention.UWP
{
    public class BackgroundProxy
    {
        public static void Initialize(bool liveTitle)
        {
            if(liveTitle)
            {
                Register();
            }
            else
            {
                UnRegister();
            }
        }

        private static async void Register()
        {
            if (BackgroundTaskHelper.IsBackgroundTaskRegistered(nameof(LiveTitleBackgroundExecution)))
            {
                return;
            }

            var access = await BackgroundExecutionManager.RequestAccessAsync();

            if (access == BackgroundAccessStatus.DeniedBySystemPolicy || access == BackgroundAccessStatus.DeniedByUser)
            {
                await new MessageDialog("The system is turned off in the background, please go to 'System Settings' to set up").ShowAsync();
                return;
            }

            BackgroundTaskRegistration task = BackgroundTaskHelper.Register(
                nameof(LiveTitleBackgroundExecution),
                typeof(LiveTitleBackgroundExecution).FullName,
                new TimeTrigger(60, false),
                false, true,
                new SystemCondition(SystemConditionType.InternetAvailable),
                new SystemCondition(SystemConditionType.UserPresent));
        }

        private static void UnRegister() => BackgroundTaskHelper.Unregister(nameof(LiveTitleBackgroundExecution));
    }
}
