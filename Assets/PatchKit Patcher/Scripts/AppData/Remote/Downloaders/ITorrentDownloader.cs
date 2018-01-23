﻿using PatchKit.Unity.Patcher.Cancellation;

namespace PatchKit.Unity.Patcher.AppData.Remote.Downloaders
{
    public interface ITorrentDownloader
    {
        event DownloadProgressChangedHandler DownloadProgressChanged;

        void Download(CancellationToken cancellationToken);
    }
}