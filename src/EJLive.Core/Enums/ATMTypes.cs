namespace EJLive.Core.Enums
{
    // Enum: AlertSeverity (from 1 sources)
        public partial enum AlertSeverity
        {
            // --- Constants & Fields ---
                Info = 0,
    
                Warning = 1,
    
                Critical = 2,
    
                Emergency = 3
    
    
        }
    // Enum: ATMStatus (from 1 sources)
        public partial enum ATMStatus
        {
            // --- Constants & Fields ---
                Online = 1,
    
                Offline = 2,
    
                InService = 3,
    
                Supervisor = 4,
    
                Fault = 5,
    
                Maintenance = 6,
    
                Unknown = 7
    
    
        }
    // Enum: ATMType (from 4 sources)
        public partial enum ATMType
        {
            // --- Constants & Fields ---
            NCR = 1,
    
            GRG = 2,
    
            WN = 3,
    
            Diebold = 4,
    
            Hyosung = 5
    
    
        }
    // Enum: CommandType (from 1 sources)
        public partial enum CommandType
        {
            // --- Constants & Fields ---
                Heartbeat = 0,
    
                CMD_RESTART = 1,
    
                CMD_SCREENSHOT = 2,
    
                CMD_TIMESYNC = 3,
    
                CMD_CHGPWD = 4,
    
                CMD_SENDIMG = 5,
    
                CMD_GHOSTREMOTE = 6,
    
                CMD_GETLOG = 7,
    
                CMD_UPDATECONFIG = 8,
    
                CMD_PING = 9,
    
                ACK = 99,
    
                NAK = 98,
    
                FileChunk = 100,
    
                FileStart = 101,
    
                FileEnd = 102
    
    
        }
    // Enum: ConnectionType (from 1 sources)
        public partial enum ConnectionType
        {
            // --- Constants & Fields ---
                LAN = 1,
    
                ADSL = 2,
    
                CDMA = 3,
    
                GSM = 4,
    
                VPN = 5
    
    
        }
    // Enum: DeviceComponent (from 1 sources)
        public partial enum DeviceComponent
        {
            // --- Constants & Fields ---
                Printer = 1,
    
                CardReader = 2,
    
                CashDispenser = 3,
    
                ReceiptPrinter = 4,
    
                PINPad = 5,
    
                Camera = 6,
    
                NetworkModule = 7,
    
                UPS = 8
    
    
        }
    // Enum: JournalFileType (from 1 sources)
        public partial enum JournalFileType
        {
            // --- Constants & Fields ---
                EJDATA_LOG = 1,
    
                EJRCPY_LOG = 2,
    
                EJDATA_LOB = 3,
    
                TRACE = 4,
    
                EJ = 5
    
    
        }
    // Enum: LogLevel (from 1 sources)
        public partial enum LogLevel
        {
            // --- Constants & Fields ---
                Debug = 0,
    
                Info = 1,
    
                Warning = 2,
    
                Error = 3,
    
                Fatal = 4
    
    
        }
    // Enum: SyncStatus (from 1 sources)
        public partial enum SyncStatus
        {
            // --- Constants & Fields ---
                Idle = 0,
    
                Storing = 1,
    
                Syncing = 2,
    
                InProgress = 3,
    
                Resyncing = 4,
    
                Completed = 5,
    
                Failed = 6,
    
                Paused = 7,
    
                Archived = 8
    
    
        }
    // Enum: TransactionType (from 1 sources)
        public partial enum TransactionType
        {
            // --- Constants & Fields ---
                CashWithdrawal = 1,
    
                BalanceInquiry = 2,
    
                Transfer = 3,
    
                Deposit = 4,
    
                PinChange = 5,
    
                MiniStatement = 6,
    
                BillPayment = 7,
    
                TopUp = 8,
    
                Unknown = 99
    
    
        }

    public partial enum AlertSeverity
        {
            Info = 0,
    
    
            Warning = 1,
    
    
            Critical = 2,
    
    
            Emergency = 3
    
    
        }
    /// <summary>
    /// Alert severity levels for system monitoring.
    /// </summary>
    public enum AlertSeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2,
        Emergency = 3
    }
    public enum AlertSeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2,
        Emergency = 3
    }
    public partial enum ATMStatus
        {
            Online = 1,
    
    
            Offline = 2,
    
    
            InService = 3,
    
    
            Supervisor = 4,
    
    
            Fault = 5,
    
    
            Maintenance = 6,
    
    
            Unknown = 7
    
    
        }
    /// <summary>
    /// Operational status of an ATM terminal.
    /// </summary>
    public enum ATMStatus
    {
        Online = 1,
        Offline = 2,
        InService = 3,
        Supervisor = 4,
        Fault = 5,
        Maintenance = 6,
        Unknown = 7
    }
    public enum ATMStatus
    {
        Online = 1,
        Offline = 2,
        InService = 3,
        Supervisor = 4,
        Fault = 5,
        Maintenance = 6,
        Unknown = 7
    }
    public partial enum ATMType
        {
            NCR = 1,
    
    
            GRG = 2,
    
    
            WN = 3,
    
    
            Diebold = 4,
    
    
            Hyosung = 5
    
    
        }
    /// <summary>
    /// ATM vendor/manufacturer types supported by the system.
    /// </summary>
    public enum ATMType
    {
        NCR = 1,
        GRG = 2,
        WN = 3,
        Diebold = 4,
        Hyosung = 5
    }
    public enum ATMType
    {
        NCR = 1,
        GRG = 2,
        WN = 3,
        Diebold = 4,
        Hyosung = 5
    }
    public partial enum CommandType
        {
            Heartbeat = 0,
    
    
            CMD_RESTART = 1,
    
    
            CMD_SCREENSHOT = 2,
    
    
            CMD_TIMESYNC = 3,
    
    
            CMD_CHGPWD = 4,
    
    
            CMD_SENDIMG = 5,
    
    
            CMD_GHOSTREMOTE = 6,
    
    
            CMD_GETLOG = 7,
    
    
            CMD_UPDATECONFIG = 8,
    
    
            CMD_PING = 9,
    
    
            ACK = 99,
    
    
            NAK = 98,
    
    
            FileChunk = 100,
    
    
            FileStart = 101,
    
    
            FileEnd = 102
    
    
        }
    /// <summary>
    /// Command types for remote ATM operations.
    /// </summary>
    public enum CommandType
    {
        Heartbeat = 0,
        CMD_RESTART = 1,
        CMD_SCREENSHOT = 2,
        CMD_TIMESYNC = 3,
        CMD_CHGPWD = 4,
        CMD_SENDIMG = 5,
        CMD_GHOSTREMOTE = 6,
        CMD_GETLOG = 7,
        CMD_UPDATECONFIG = 8,
        CMD_PING = 9,
        ACK = 99,
        NAK = 98,
        FileChunk = 100,
        FileStart = 101,
        FileEnd = 102
    }
    public enum CommandType
    {
        Heartbeat = 0,
        CMD_RESTART = 1,
        CMD_SCREENSHOT = 2,
        CMD_TIMESYNC = 3,
        CMD_CHGPWD = 4,
        CMD_SENDIMG = 5,
        CMD_GHOSTREMOTE = 6,
        CMD_GETLOG = 7,
        CMD_UPDATECONFIG = 8,
        CMD_PING = 9,
        ACK = 99,
        NAK = 98,
        FileChunk = 100,
        FileStart = 101,
        FileEnd = 102
    }
    public partial enum ConnectionType
        {
            LAN = 1,
    
    
            ADSL = 2,
    
    
            CDMA = 3,
    
    
            GSM = 4,
    
    
            VPN = 5
    
    
        }
    /// <summary>
    /// Network connection type for ATM communication.
    /// </summary>
    public enum ConnectionType
    {
        LAN = 1,
        ADSL = 2,
        CDMA = 3,
        GSM = 4,
        VPN = 5
    }
    public enum ConnectionType
    {
        LAN = 1,
        ADSL = 2,
        CDMA = 3,
        GSM = 4,
        VPN = 5
    }
    public partial enum DeviceComponent
        {
            Printer = 1,
    
    
            CardReader = 2,
    
    
            CashDispenser = 3,
    
    
            ReceiptPrinter = 4,
    
    
            PINPad = 5,
    
    
            Camera = 6,
    
    
            NetworkModule = 7,
    
    
            UPS = 8
    
    
        }
    /// <summary>
    /// ATM hardware device components for monitoring.
    /// </summary>
    public enum DeviceComponent
    {
        Printer = 1,
        CardReader = 2,
        CashDispenser = 3,
        ReceiptPrinter = 4,
        PINPad = 5,
        Camera = 6,
        NetworkModule = 7,
        UPS = 8
    }
    public enum DeviceComponent
    {
        Printer = 1,
        CardReader = 2,
        CashDispenser = 3,
        ReceiptPrinter = 4,
        PINPad = 5,
        Camera = 6,
        NetworkModule = 7,
        UPS = 8
    }
    public partial enum JournalFileType
        {
            EJDATA_LOG = 1,
    
    
            EJRCPY_LOG = 2,
    
    
            EJDATA_LOB = 3,
    
    
            TRACE = 4,
    
    
            EJ = 5
    
    
        }
    /// <summary>
    /// Types of journal files handled by the system.
    /// </summary>
    public enum JournalFileType
    {
        EJDATA_LOG = 1,
        EJRCPY_LOG = 2,
        EJDATA_LOB = 3,
        TRACE = 4,
        EJ = 5
    }
    public enum JournalFileType
    {
        EJDATA_LOG = 1,
        EJRCPY_LOG = 2,
        EJDATA_LOB = 3,
        TRACE = 4,
        EJ = 5
    }
    public partial enum LogLevel
        {
            Debug = 0,
    
    
            Info = 1,
    
    
            Warning = 2,
    
    
            Error = 3,
    
    
            Fatal = 4
    
    
        }
    /// <summary>
    /// Log levels for structured logging.
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4
    }
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4
    }
    public partial enum SyncStatus
        {
            Idle = 0,
    
    
            Storing = 1,
    
    
            Syncing = 2,
    
    
            InProgress = 3,
    
    
            Resyncing = 4,
    
    
            Completed = 5,
    
    
            Failed = 6,
    
    
            Paused = 7,
    
    
            Archived = 8
    
    
        }
    /// <summary>
    /// Journal synchronization status states.
    /// </summary>
    public enum SyncStatus
    {
        Idle = 0,
        Storing = 1,
        Syncing = 2,
        InProgress = 3,
        Resyncing = 4,
        Completed = 5,
        Failed = 6,
        Paused = 7,
        Archived = 8
    }
    public enum SyncStatus
    {
        Idle = 0,
        Storing = 1,
        Syncing = 2,
        InProgress = 3,
        Resyncing = 4,
        Completed = 5,
        Failed = 6,
        Paused = 7
    }
    public partial enum TransactionType
        {
            CashWithdrawal = 1,
    
    
            BalanceInquiry = 2,
    
    
            Transfer = 3,
    
    
            Deposit = 4,
    
    
            PinChange = 5,
    
    
            MiniStatement = 6,
    
    
            BillPayment = 7,
    
    
            TopUp = 8,
    
    
            Unknown = 99
    
    
        }
    /// <summary>
    /// Types of financial transactions processed at the ATM.
    /// </summary>
    public enum TransactionType
    {
        CashWithdrawal = 1,
        BalanceInquiry = 2,
        Transfer = 3,
        Deposit = 4,
        PinChange = 5,
        MiniStatement = 6,
        BillPayment = 7,
        TopUp = 8,
        Unknown = 99
    }
    public enum TransactionType
    {
        CashWithdrawal = 1,
        BalanceInquiry = 2,
        Transfer = 3,
        Deposit = 4,
        PinChange = 5,
        MiniStatement = 6,
        BillPayment = 7,
        TopUp = 8,
        Unknown = 99
    }
}
