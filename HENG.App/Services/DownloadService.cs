﻿using HENG.App.Models;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace HENG.App.Services
{
    public enum DownloadStartResult
    {
        Started,
        Error,
        AllreadyDownloaded,
    }

    public partial class DownloadService
    {
        public const string NAME = "DownloadCompleteTrigger";

        private readonly ToastNotifier _notifier = ToastNotificationManager.CreateToastNotifier();

        public static async Task AttachToDownloads()
        {
            IReadOnlyList<DownloadOperation> downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            foreach (DownloadOperation download in downloads)
            {
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>();
                await download.AttachAsync().AsTask(progressCallback);
            }
        }

        public async Task<DownloadStartResult> Download(DownloadItem download)
        {
            var sourceUri = new Uri(download.FullHDImageURL);

            var hash = GetFileNameFromUri(sourceUri);
            var file = await CheckLocalFileExistsFromUriHash(sourceUri);

            var downloadingAlready = await IsDownloading(sourceUri);

            if (null == file && !downloadingAlready)
            {
                await Task.Run(() => 
                {
                    var task = StartDownload(sourceUri, BackgroundTransferPriority.High, hash);
                    task.ContinueWith((state) =>
                    {
                        if (state.Exception != null)
                        {
                            Trace.WriteLine($"An error occured with this download {state.Exception}");
                        }
                        else
                        {
                            Debug.WriteLine("Download Completed");
                        }
                    });
                });
                return DownloadStartResult.Started;
            }
            else if (file != null)
            {
                Trace.WriteLine("Already downloaded.");
                return DownloadStartResult.AllreadyDownloaded;
            }
            else
            {
                return DownloadStartResult.Error;
            }
        }
    }

    public partial class DownloadService
    {
        private async Task<IStorageItem> CheckLocalFileExistsFromUriHash(Uri sourceUri)
        {
            string hash = GetFileNameFromUri(sourceUri);

            StorageFolder folder = await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                return await StorageFolder.GetFolderFromPathAsync(AppSettings.Current.DownloadPath);
            }, Windows.UI.Core.CoreDispatcherPriority.High);
      
            IStorageItem file = await folder.TryGetItemAsync(hash);
            if (file != null)
            {
                var props = await file.GetBasicPropertiesAsync();
                if (props.Size == 0)
                {
                    await file.DeleteAsync();
                    return null;
                }
            }
            return file;
        }

        private async Task<bool> IsDownloading(Uri sourceUri)
        {
            var downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            return downloads.Where(dl => dl.RequestedUri == sourceUri).FirstOrDefault() != null;
        }

        private async Task StartDownload(Uri target, BackgroundTransferPriority priority, string localFilename)
        {
            await BackgroundExecutionManager.RequestAccessAsync();

            StorageFile destinationFile = await GetLocalFileFromName(localFilename);

            var group = BackgroundTransferGroup.CreateGroup(Guid.NewGuid().ToString());
            group.TransferBehavior = BackgroundTransferBehavior.Serialized;

            BackgroundTransferCompletionGroup completionGroup = new BackgroundTransferCompletionGroup();

            // this will cause the app to be activated when the download completes and
            // CheckCompletionResult will be called for the final download state
            RegisterBackgroundTask(completionGroup.Trigger);

            BackgroundDownloader downloader = new BackgroundDownloader(completionGroup)
            {
                TransferGroup = group
            };
            group.TransferBehavior = BackgroundTransferBehavior.Serialized;
            CreateNotifications(downloader);
            DownloadOperation download = downloader.CreateDownload(target, destinationFile);
            download.Priority = priority;

            completionGroup.Enable();

            Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
            var downloadTask = download.StartAsync().AsTask(progressCallback);

            CreateToast(GetFileNameFromUri(target), localFilename);

            try
            {
                await downloadTask;

                // Will occur after download completes
                ResponseInformation response = download.GetResponseInformation();
            }
            catch (Exception)
            {
                Debug.WriteLine("Download exception");
            }
        }


        private static async Task<StorageFile> GetLocalFileFromName(string filename)
        {
            StorageFile file = null;
            try
            {
                StorageFolder folder = await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                 {
                     return await StorageFolder.GetFolderFromPathAsync(AppSettings.Current.DownloadPath);
                 }, Windows.UI.Core.CoreDispatcherPriority.High);

                file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            }
            catch (FileNotFoundException)
            {
            }
            return file;
        }

        private void RegisterBackgroundTask(IBackgroundTrigger trigger)
        {
            var builder = new BackgroundTaskBuilder
            {
                Name = NAME
            };
            builder.SetTrigger(trigger);

            BackgroundTaskRegistration task = builder.Register();
        }

        private void CreateNotifications(BackgroundDownloader downloader)
        {
            var successToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            successToastXml.GetElementsByTagName("text").Item(0).InnerText =
                "Downloads completed successfully.";
            ToastNotification successToast = new ToastNotification(successToastXml);
            downloader.SuccessToastNotification = successToast;

            var failureToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            failureToastXml.GetElementsByTagName("text").Item(0).InnerText =
                "At least one download completed with failure.";
            ToastNotification failureToast = new ToastNotification(failureToastXml);
            downloader.FailureToastNotification = failureToast;
        }

        private static void CreateToast(string title, string tag)
        {
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "File downloading...",
                            },

                            new AdaptiveProgressBar()
                            {
                                Title = title,
                                Value = new BindableProgressBarValue("progressValue"),
                                ValueStringOverride = new BindableString("p"),
                                Status = "Downloading...",
                            },
                        },
                    },
                },
            };

            var data = new Dictionary<string, string>
            {
                { "progressValue", "0" },
                { "p", $"{DateTime.Now}" }, // TODO: better than cool
            };

            // And create the toast notification
            ToastNotification notification = new ToastNotification(toastContent.GetXml())
            {
                Tag = tag,
                Data = new NotificationData(data),
            };

            // And then send the toast
            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }

        private void DownloadProgress(DownloadOperation obj)
        {
            Debug.WriteLine(obj.Progress.ToString());
            var progress = (double)obj.Progress.BytesReceived / obj.Progress.TotalBytesToReceive;
            UpdateToast(obj.ResultFile.Name, progress);
        }

        private void UpdateToast(string toastTag, double progressValue)
        {
            var data = new Dictionary<string, string>
            {
                { "progressValue", progressValue.ToString() },
                { "p", $"{DateTime.Now}" }, // TODO: better than cool
            };

            try
            {
                _notifier.Update(new NotificationData(data), toastTag);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }

    public partial class DownloadService
    {
        private static string SafeHashUri(Uri sourceUri)
        {
            string Hash(string input)
            {
                IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
                HashAlgorithmProvider hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
                var hashByte = hashAlgorithm.HashData(buffer).ToArray();
                var sb = new StringBuilder(hashByte.Length * 2);
                foreach (byte b in hashByte)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }

            var hash = Hash(Path.GetFileNameWithoutExtension(sourceUri.PathAndQuery).ToLower());
            return hash;
        }

        public static string GetFileNameFromUri(Uri sourceUri) => SafeHashUri(sourceUri) + Path.GetExtension(sourceUri.PathAndQuery);
    }
}