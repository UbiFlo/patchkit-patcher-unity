﻿using System;
using JetBrains.Annotations;
using PatchKit.Api;
using PatchKit.Api.Models.Main;
using PatchKit.Network;
using PatchKit.Unity.Patcher.Debug;

namespace PatchKit.Unity.Patcher.AppData.Remote
{
    public class RemoteMetaData : IRemoteMetaData
    {
        private static readonly DebugLogger DebugLogger = new DebugLogger(typeof(RemoteMetaData));

        private readonly string _appSecret;
        private readonly MainApiConnection _mainApiConnection;
        private readonly KeysApiConnection _keysApiConnection;

        public RemoteMetaData([NotNull] string appSecret, [NotNull] IRequestTimeoutCalculator requestTimeoutCalculator)
        {
            if (string.IsNullOrEmpty(appSecret))
                throw new ArgumentException("Value cannot be null or empty.", "appSecret");
            if (requestTimeoutCalculator == null) throw new ArgumentNullException("requestTimeoutCalculator");

            DebugLogger.LogConstructor();
            DebugLogger.LogVariable(appSecret, "appSecret");

            _appSecret = appSecret;

            var mainSettings = Settings.GetMainApiConnectionSettings();

            _mainApiConnection = new MainApiConnection(mainSettings)
            {
                HttpClient = new UnityHttpClient(),
                RequestTimeoutCalculator = requestTimeoutCalculator,
                RequestRetryStrategy = new SimpleInfiniteRequestRetryStrategy(),
                Logger = PatcherLogManager.DefaultLogger
            };

            var keysSettings = Settings.GetKeysApiConnectionSettings();

            _keysApiConnection = new KeysApiConnection(keysSettings)
            {
                HttpClient = new UnityHttpClient(),
                RequestTimeoutCalculator = requestTimeoutCalculator,
                Logger = PatcherLogManager.DefaultLogger
            };

        }

        public int GetLatestVersionId()
        {
            DebugLogger.Log("Getting latest version id.");
            return _mainApiConnection.GetAppLatestAppVersionId(_appSecret).Id;
        }

        public Api.Models.Main.App GetAppInfo()
        {
            DebugLogger.Log("Getting app info.");
            return _mainApiConnection.GetApplicationInfo(_appSecret);
        }

        public AppContentSummary GetContentSummary(int versionId)
        {
            Checks.ArgumentValidVersionId(versionId, "versionId");
            DebugLogger.Log(string.Format("Getting content summary of version with id {0}.", versionId));

            return _mainApiConnection.GetAppVersionContentSummary(_appSecret, versionId);
        }

        public AppDiffSummary GetDiffSummary(int versionId)
        {
            Checks.ArgumentValidVersionId(versionId, "versionId");
            DebugLogger.Log(string.Format("Getting diff summary of version with id {0}.", versionId));

            return _mainApiConnection.GetAppVersionDiffSummary(_appSecret, versionId);
        }

        public string GetKeySecret(string key, string cachedKeySecret)
        {
            Checks.ArgumentNotNullOrEmpty(key, "key");
            DebugLogger.Log(string.Format("Getting key secret from key {0}.", key));

            var keySecret = _keysApiConnection.GetKeyInfo(key, _appSecret, cachedKeySecret).KeySecret;

            return keySecret;
        }
    }
}