﻿using System;
using System.Net;
using PatchKit.Api;
using PatchKit.Unity.Patcher.Cancellation;
using PatchKit.Unity.Patcher.Debug;
using PatchKit.Unity.Patcher.Status;
using PatchKit.Unity.Patcher.UI.Dialogs;

namespace PatchKit.Unity.Patcher.AppUpdater.Commands
{
    internal class ValidateLicenseCommand : IValidateLicenseCommand
    {
        private static readonly DebugLogger DebugLogger = new DebugLogger(typeof(ValidateLicenseCommand));

        private readonly AppUpdaterContext _context;

        public ValidateLicenseCommand(AppUpdaterContext context)
        {
            DebugLogger.LogConstructor();

            _context = context;
        }

        public void Execute(CancellationToken cancellationToken)
        {
            KeySecret = null;

            var appInfo = _context.Data.RemoteData.MetaData.GetAppInfo();

            if (!appInfo.UseKeys)
            {
                return;
            }

            LicenseDialogMessageType messageType = LicenseDialogMessageType.None;

            while (KeySecret == null)
            {
                var result = _context.LicenseDialog.Display(messageType);

                if (result.Type == LicenseDialogResultType.Confirmed)
                {
                    try
                    {
                        KeySecret = _context.Data.RemoteData.MetaData.GetKeySecret(result.Key);
                    }
                    catch (ApiResponseException apiResponseException)
                    {
                        DebugLogger.LogException(apiResponseException);

                        if (apiResponseException.StatusCode == 404)
                        {
                            KeySecret = null;
                            messageType = LicenseDialogMessageType.InvalidLicense;
                        }
                        else if (apiResponseException.StatusCode == 410)
                        {
                            KeySecret = null;
                            messageType = LicenseDialogMessageType.BlockedLicense;
                        }
                        else if (apiResponseException.StatusCode == 403)
                        {
                            KeySecret = null;
                            messageType = LicenseDialogMessageType.ServiceUnavailable;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (WebException webException)
                    {
                        DebugLogger.LogException(webException);

                        KeySecret = null;
                        messageType = LicenseDialogMessageType.ServiceUnavailable;
                    }
                }
                else if (result.Type == LicenseDialogResultType.Aborted)
                {
                    throw new OperationCanceledException();
                }
            }
        }

        public void Prepare(IStatusMonitor statusMonitor)
        {
        }

        public string KeySecret { get; private set; }
    }
}