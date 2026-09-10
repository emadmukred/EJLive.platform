using System;
using System.Collections.Generic;
using EJLive.Core;
using EJLive.Core.Models;

namespace EJLive.Core.Services
{
    public sealed class NcrReferenceCapabilityFactory
    {
        public IReadOnlyDictionary<string, string> CreateDefaults() => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Journal"] = AppConstants.NCR_JournalPath,
            ["EJDATA"] = AppConstants.NCR_EJData,
            ["EJRCPY"] = AppConstants.NCR_EJRcpy
        };

        public NcrConfigCapabilityProfile BuildReferenceProfile()
        {
            var profile = new NcrConfigCapabilityProfile
            {
                SourceName = "NCR extracted references",
                Manufacturer = "NCR",
                TerminalModel = "56XX/662x lineage",
                HasCashInConfiguration = true,
                HasSupervisorReceiptConfig = true,
                HasVoiceGuidance = true,
                HasPopupKeyboard = true,
                HasEnhancedAudio = true,
                HasCameraService = true,
                HasChequeAcceptService = true,
                HasPersistentDataService = true,
                ReportSeparateCashUnits = true,
                ExpandCimRetractNoteList = true,
                AllowRetractToTransport = true,
                DisableAutomaticResets = true,
                TamperSenseSuspend = true,
                SuspendTimeoutSeconds = 900,
                ResetActionOnInitialise = 3,
                BankName = "Configured",
                BranchId = "Configured",
                NetworkId = "Configured",
                TerminalId = "Configured"
            };

            profile.IncludedConfigFiles.Add("AdvanceNDCCore.accfg");
            profile.IncludedConfigFiles.Add("AdvanceNDCCustom.accfg");
            profile.IncludedConfigFiles.Add("CashInCore.accfg");
            profile.IncludedConfigFiles.Add("CashInCustom.accfg");
            profile.IncludedConfigFiles.Add("SupervisorReceiptConfig.accfg");

            profile.ReceiptForms.Add("ExceptionReportDetails");
            profile.ReceiptForms.Add("DepositError");
            profile.ReceiptForms.Add("DepositErrorNoRecords");
            profile.ReceiptForms.Add("CashRetract");
            profile.ReceiptForms.Add("CashRetractNoRecords");
            profile.ReceiptForms.Add("HostTimeOut");
            profile.ReceiptForms.Add("HostTimeOutNoRecords");

            profile.TranslationDomains.Add("TcpipSupportTask");

            profile.RegistryFlags.Add("ReportSeparateCashUnits");
            profile.RegistryFlags.Add("ExpandCIMRetractNoteList");
            profile.RegistryFlags.Add("AllowRetractToTransport");
            profile.RegistryFlags.Add("DisableAutomaticResets");
            profile.RegistryFlags.Add("TamperSenseSuspend");

            return profile;
        }
    }
}
