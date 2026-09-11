using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using EJLive.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// محرك تحليل العمليات الكامل — Regex Profiles لكل نوع صراف (A-02)
    /// يطبق: NCR / GRG / WN Regex Mapping Profiles (A-02)
    ///        تتبع دورة حياة المعاملة كاملة (A-03)
    ///        كشف: سحب ناجح/فاشل / بطاقة محتجزة / أخطاء كاش / Supervisor / Power Up
    /// </summary>
    public class TransactionAnalysisEngine
    {
        // ==========================================
        // Regex Profiles لكل نوع صراف (A-02)
        // ==========================================

        private static class NCR_Patterns
        {
            public static readonly Regex Approved       = new Regex(@"NOTES DISPENSED|CASH DISPENSED|DISPENSE COMPLETE|AMOUNT DISPENSED\s*:\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex Declined       = new Regex(@"UNABLE TO PROCESS|TRANSACTION DECLINED|HOST DECLINED|NOT SUFFICIENT FUNDS|INVALID PIN|CARD NOT ACCEPTED", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex CardCapture    = new Regex(@"CARD CAPTURED|CARD RETAINED|RETAIN CARD|SEIZE CARD|CAPTURE CODE\s*:\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex BalanceInquiry = new Regex(@"BALANCE INQUIRY|ACCOUNT BALANCE|AVAILABLE BALANCE\s*:\s*([\d,\.]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex PowerUp        = new Regex(@"POWER UP RESET|POWER-UP|SYSTEM RESTART|ATM RESTART|XFS STARTUP|STARTUP COMPLETE", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex Supervisor     = new Regex(@"SUPERVISOR MODE|ENTER SUPERVISOR|SUPERVISOR ACCESS|OPERATOR MENU|MANAGEMENT FUNCTION", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex ErrorE3        = new Regex(@"ERROR\s*E3|E3\s*ERROR|CASH UNIT\s*ERROR\s*E3", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex ErrorM18       = new Regex(@"M-18|M18\b|MEDIA UNIT\s*ERROR\s*18", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex ErrorM02       = new Regex(@"M-02|M02\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex ErrorM03       = new Regex(@"M-03|M03\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex ErrorM05       = new Regex(@"M-05|M05\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex ErrorM10       = new Regex(@"M-10|M10\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex ErrorM11       = new Regex(@"M-11|M11\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex TotalCashError = new Regex(@"CASH\s*UNIT\s*ERROR|TOTAL\s*CASH\s*ERROR|DISPENSE\s*ERROR|NOTES\s*ERROR", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex Amount         = new Regex(@"AMOUNT\s*[=:]\s*([\d,]+)|DISPENSED\s*([\d,]+)|TRANSACTION AMOUNT\s*([\d,]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex CardNumber     = new Regex(@"CARD\s*(?:NUMBER|NO|#)\s*[=:]?\s*(\d{4,19})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex DateTime_      = new Regex(@"(\d{2}/\d{2}/\d{4})\s+(\d{2}:\d{2}:\d{2})", RegexOptions.Compiled);
        }

        private static class GRG_Patterns
        {
            public static readonly Regex Approved       = new Regex(@"DISPENSE\s*OK|CASH\s*OUT\s*OK|WITHDRAWAL\s*SUCCESSFUL|COMPLETE\s*DISPENSING", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex Declined       = new Regex(@"DISPENSE\s*FAIL|WITHDRAWAL\s*FAILED|DECLINED\s*BY\s*HOST|TRANSACTION\s*REJECTED", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex CardCapture    = new Regex(@"CARD\s*CAPTURED|RETAIN\s*CARD|EJECT\s*FAIL|CAPTURE\s*CARD", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex PowerUp        = new Regex(@"SYSTEM\s*STARTUP|POWER\s*ON\s*RESET|BOOT\s*COMPLETE|GRG\s*INIT", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex Supervisor     = new Regex(@"SUPERVISOR|MANAGEMENT\s*MODE|OPERATOR\s*ACCESS", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex ErrorE3        = new Regex(@"ERROR\s*CODE\s*E3|E3\s*ALARM|CASH\s*JAM\s*E3", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex Amount         = new Regex(@"AMOUNT\s*[=:]\s*([\d,\.]+)|OUT\s*AMOUNT\s*([\d,\.]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        private static class WN_Patterns
        {
            public static readonly Regex Approved       = new Regex(@"Transaction\s*completed|Dispense\s*successful|Cash\s*presented|End\s*transaction\s*OK", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex Declined       = new Regex(@"Transaction\s*declined|Host\s*refused|Authorization\s*failed|Cash\s*not\s*presented", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex CardCapture    = new Regex(@"Card\s*retained|Card\s*capture|Retain\s*card", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex PowerUp        = new Regex(@"System\s*startup|Power\s*on\s*self\s*test|WOSA\s*init|XFS\s*manager\s*started", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex Supervisor     = new Regex(@"Supervisor\s*menu|Management\s*session|Operator\s*login", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            public static readonly Regex Amount         = new Regex(@"Amount[=:\s]+([\d,\.]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        // ==========================================
        // التحليل الرئيسي
        // ==========================================

        public List<TransactionRecord> AnalyzeText(string text, string atmId, string atmType)
        {
            if (string.IsNullOrEmpty(text)) return new List<TransactionRecord>();
            var lines   = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var records = new List<TransactionRecord>();
            atmType     = atmType?.ToUpperInvariant() ?? "NCR";

            for (int i = 0; i < lines.Length; i++)
            {
                var line  = lines[i];
                var lower = line.ToLowerInvariant();

                // ادمج بضعة سطور متجاورة للسياق
                var context = string.Join(" ", GetContextLines(lines, i, 3));
                var tx = TryParseTransaction(context, atmId, atmType, i);
                if (tx != null)
                    records.Add(tx);
            }
            return records;
        }

        private TransactionRecord TryParseTransaction(string context, string atmId, string atmType, int lineNum)
        {
            atmType = AppConstants.NormalizeATMType(atmType);
            if (atmType == AppConstants.ATM_TYPE_GRG || atmType == AppConstants.ATM_TYPE_HY)
                return TryParseGRG(context, atmId, lineNum);
            if (atmType == AppConstants.ATM_TYPE_WN || atmType == AppConstants.ATM_TYPE_DN)
                return TryParseWN(context, atmId, lineNum);
            return TryParseNCR(context, atmId, lineNum);
        }

        private TransactionRecord TryParseNCR(string ctx, string atmId, int line)
        {
            if (NCR_Patterns.PowerUp.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.PowerReset, Result=TxResult.Info, RawContext=ctx, LineNumber=line };

            if (NCR_Patterns.Supervisor.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.Supervisor, Result=TxResult.Info, RawContext=ctx, LineNumber=line };

            if (NCR_Patterns.CardCapture.IsMatch(ctx))
            {
                var code = TryExtract(NCR_Patterns.CardCapture, ctx, 1);
                return new TransactionRecord { ATMId=atmId, TxType=TxType.CardCapture, Result=TxResult.Warning, ErrorCode="CAPTURE" + (code ?? ""), CardCaptured=true, RawContext=ctx, LineNumber=line };
            }

            if (NCR_Patterns.ErrorE3.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.CashError, Result=TxResult.Error, ErrorCode="E3", RawContext=ctx, LineNumber=line };

            if (NCR_Patterns.TotalCashError.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.CashError, Result=TxResult.Error, ErrorCode="CASH_ERR", RawContext=ctx, LineNumber=line };

            if (NCR_Patterns.ErrorM18.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.MediaError, Result=TxResult.Error, ErrorCode="M-18", RawContext=ctx, LineNumber=line };
            if (NCR_Patterns.ErrorM02.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.MediaError, Result=TxResult.Error, ErrorCode="M-02", RawContext=ctx, LineNumber=line };
            if (NCR_Patterns.ErrorM03.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.MediaError, Result=TxResult.Error, ErrorCode="M-03", RawContext=ctx, LineNumber=line };
            if (NCR_Patterns.ErrorM05.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.MediaError, Result=TxResult.Error, ErrorCode="M-05", RawContext=ctx, LineNumber=line };
            if (NCR_Patterns.ErrorM10.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.MediaError, Result=TxResult.Error, ErrorCode="M-10", RawContext=ctx, LineNumber=line };
            if (NCR_Patterns.ErrorM11.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.MediaError, Result=TxResult.Error, ErrorCode="M-11", RawContext=ctx, LineNumber=line };

            if (NCR_Patterns.Approved.IsMatch(ctx))
            {
                var amt = ParseAmount(NCR_Patterns.Amount, ctx);
                return new TransactionRecord { ATMId=atmId, TxType=TxType.Withdrawal, Result=TxResult.Approved, Amount=amt, RawContext=ctx, LineNumber=line };
            }

            if (NCR_Patterns.Declined.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.Withdrawal, Result=TxResult.Declined, RawContext=ctx, LineNumber=line };

            if (NCR_Patterns.BalanceInquiry.IsMatch(ctx))
                return new TransactionRecord { ATMId=atmId, TxType=TxType.BalanceInquiry, Result=TxResult.Info, RawContext=ctx, LineNumber=line };

            return null;
        }

        private TransactionRecord TryParseGRG(string ctx, string atmId, int line)
        {
            if (GRG_Patterns.PowerUp.IsMatch(ctx))    return new TransactionRecord { ATMId=atmId, TxType=TxType.PowerReset,   Result=TxResult.Info,    RawContext=ctx, LineNumber=line };
            if (GRG_Patterns.Supervisor.IsMatch(ctx)) return new TransactionRecord { ATMId=atmId, TxType=TxType.Supervisor,   Result=TxResult.Info,    RawContext=ctx, LineNumber=line };
            if (GRG_Patterns.CardCapture.IsMatch(ctx)) return new TransactionRecord { ATMId=atmId, TxType=TxType.CardCapture, Result=TxResult.Warning, CardCaptured=true, RawContext=ctx, LineNumber=line };
            if (GRG_Patterns.ErrorE3.IsMatch(ctx))    return new TransactionRecord { ATMId=atmId, TxType=TxType.CashError,    Result=TxResult.Error,   ErrorCode="E3", RawContext=ctx, LineNumber=line };
            if (GRG_Patterns.Approved.IsMatch(ctx))   return new TransactionRecord { ATMId=atmId, TxType=TxType.Withdrawal,   Result=TxResult.Approved, Amount=ParseAmount(GRG_Patterns.Amount, ctx), RawContext=ctx, LineNumber=line };
            if (GRG_Patterns.Declined.IsMatch(ctx))   return new TransactionRecord { ATMId=atmId, TxType=TxType.Withdrawal,   Result=TxResult.Declined, RawContext=ctx, LineNumber=line };
            return null;
        }

        private TransactionRecord TryParseWN(string ctx, string atmId, int line)
        {
            if (WN_Patterns.PowerUp.IsMatch(ctx))     return new TransactionRecord { ATMId=atmId, TxType=TxType.PowerReset,   Result=TxResult.Info,    RawContext=ctx, LineNumber=line };
            if (WN_Patterns.Supervisor.IsMatch(ctx))  return new TransactionRecord { ATMId=atmId, TxType=TxType.Supervisor,   Result=TxResult.Info,    RawContext=ctx, LineNumber=line };
            if (WN_Patterns.CardCapture.IsMatch(ctx)) return new TransactionRecord { ATMId=atmId, TxType=TxType.CardCapture,  Result=TxResult.Warning, CardCaptured=true, RawContext=ctx, LineNumber=line };
            if (WN_Patterns.Approved.IsMatch(ctx))    return new TransactionRecord { ATMId=atmId, TxType=TxType.Withdrawal,   Result=TxResult.Approved, Amount=ParseAmount(WN_Patterns.Amount, ctx), RawContext=ctx, LineNumber=line };
            if (WN_Patterns.Declined.IsMatch(ctx))    return new TransactionRecord { ATMId=atmId, TxType=TxType.Withdrawal,   Result=TxResult.Declined, RawContext=ctx, LineNumber=line };
            return null;
        }

        // ==========================================
        // البحث السريع (أزرار Filter)
        // ==========================================

        public List<string> SearchLines(string text, JournalSearchFilter filter)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();
            var lines   = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var results = new List<string>();
            Regex pattern = BuildFilterPattern(filter);

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && (pattern == null || pattern.IsMatch(line)))
                    results.Add(line);
            }
            return results;
        }

        public List<string> SearchFreeText(string text, string keyword)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(keyword))
                return new List<string>();
            var lines   = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var results = new List<string>();
            var kw      = keyword.ToUpperInvariant();
            foreach (var line in lines)
                if (line.ToUpperInvariant().Contains(kw))
                    results.Add(line);
            return results;
        }

        private Regex BuildFilterPattern(JournalSearchFilter filter)
        {
            return filter switch
            {
                JournalSearchFilter.ApprovedTransactions => NCR_Patterns.Approved,
                JournalSearchFilter.PowerUpReset         => NCR_Patterns.PowerUp,
                JournalSearchFilter.ErrorE3              => NCR_Patterns.ErrorE3,
                JournalSearchFilter.TotalCashError       => NCR_Patterns.TotalCashError,
                JournalSearchFilter.M18                  => NCR_Patterns.ErrorM18,
                JournalSearchFilter.M02_M03_M05          => new Regex(@"M-02|M-03|M-05|M02|M03|M05", RegexOptions.IgnoreCase),
                JournalSearchFilter.M10_M11              => new Regex(@"M-10|M-11|M10|M11", RegexOptions.IgnoreCase),
                JournalSearchFilter.CardCapture          => NCR_Patterns.CardCapture,
                JournalSearchFilter.Declined             => NCR_Patterns.Declined,
                _ => null
            };
        }

        // ==========================================
        // تحليل ملف كامل
        // ==========================================

        public JournalAnalysisResult AnalyzeFile(string filePath, string atmId, string atmType)
        {
            var result = new JournalAnalysisResult { ATMId = atmId, ATMType = atmType, FilePath = filePath };
            try
            {
                string text;
                using var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite | System.IO.FileShare.Delete);
                using var sr = new System.IO.StreamReader(fs, Encoding.UTF8, true);
                text = sr.ReadToEnd();

                result.RawText    = text;
                result.LineCount  = text.Split('\n').Length;
                result.FileSizeKB = (double)fs.Length / 1024;

                var txList  = AnalyzeText(text, atmId, atmType);
                foreach (var tx in txList)
                {
                    result.AllTransactions.Add(tx);
                    switch (tx.TxType)
                    {
                        case TxType.Withdrawal when tx.Result == TxResult.Approved:
                            result.ApprovedCount++;
                            result.TotalCashDispensed += tx.Amount;
                            break;
                        case TxType.Withdrawal when tx.Result == TxResult.Declined:
                            result.DeclinedCount++;
                            break;
                        case TxType.CardCapture:
                            result.CardsCaptured++;
                            break;
                        case TxType.CashError:
                        case TxType.MediaError:
                            result.ErrorCount++;
                            if (!result.ErrorCodes.Contains(tx.ErrorCode))
                                result.ErrorCodes.Add(tx.ErrorCode);
                            break;
                        case TxType.PowerReset:
                            result.PowerResetCount++;
                            break;
                        case TxType.Supervisor:
                            result.SupervisorEntries++;
                            break;
                    }
                }
                result.SuccessRate = result.ApprovedCount + result.DeclinedCount > 0
                    ? (double)result.ApprovedCount / (result.ApprovedCount + result.DeclinedCount) * 100
                    : 0;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            return result;
        }

        // ==========================================
        // دوال مساعدة
        // ==========================================

        private string[] GetContextLines(string[] lines, int idx, int radius)
        {
            var start  = Math.Max(0, idx - radius);
            var end    = Math.Min(lines.Length - 1, idx + radius);
            var result = new string[end - start + 1];
            Array.Copy(lines, start, result, 0, result.Length);
            return result;
        }

        private string TryExtract(Regex rx, string text, int groupIdx)
        {
            var m = rx.Match(text);
            return m.Success && m.Groups.Count > groupIdx ? m.Groups[groupIdx].Value : null;
        }

        private long ParseAmount(Regex rx, string text)
        {
            var m = rx.Match(text);
            for (int g = 1; g < m.Groups.Count; g++)
                if (m.Groups[g].Success && !string.IsNullOrEmpty(m.Groups[g].Value))
                    if (long.TryParse(m.Groups[g].Value.Replace(",", ""), out var v)) return v;
            return 0;
        }
    }

    // ==========================================
    // نماذج النتائج
    // ==========================================

    public enum TxType    { Withdrawal, BalanceInquiry, CardCapture, CashError, MediaError, PowerReset, Supervisor, Other }
    public enum TxResult  { Approved, Declined, Error, Warning, Info }
    public enum JournalSearchFilter
    {
        All, ApprovedTransactions, PowerUpReset, ErrorE3, TotalCashError,
        M18, M02_M03_M05, M10_M11, CardCapture, Declined, FreeText
    }

    public class TransactionRecord
    {
        public string    ATMId       { get; set; }
        public TxType    TxType      { get; set; }
        public TxResult  Result      { get; set; }
        public long      Amount      { get; set; }
        public string    ErrorCode   { get; set; }
        public bool      CardCaptured { get; set; }
        public string    RawContext  { get; set; }
        public int       LineNumber  { get; set; }
        public DateTime  ParsedAt    { get; set; } = DateTime.UtcNow;

        public string ResultIcon => Result switch
        {
            TxResult.Approved => "✅",
            TxResult.Declined => "❌",
            TxResult.Error    => "🚨",
            TxResult.Warning  => "⚠️",
            _                 => "ℹ️"
        };
    }

    public class JournalAnalysisResult
    {
        public string                  ATMId               { get; set; }
        public string                  ATMType             { get; set; }
        public string                  FilePath            { get; set; }
        public string                  RawText             { get; set; }
        public int                     LineCount           { get; set; }
        public double                  FileSizeKB          { get; set; }
        public List<TransactionRecord> AllTransactions     { get; set; } = new List<TransactionRecord>();
        public int                     ApprovedCount       { get; set; }
        public int                     DeclinedCount       { get; set; }
        public int                     CardsCaptured       { get; set; }
        public int                     ErrorCount          { get; set; }
        public int                     PowerResetCount     { get; set; }
        public int                     SupervisorEntries   { get; set; }
        public long                    TotalCashDispensed  { get; set; }
        public double                  SuccessRate         { get; set; }
        public List<string>            ErrorCodes          { get; set; } = new List<string>();
        public string                  Error               { get; set; }
        public DateTime                AnalyzedAt          { get; set; } = DateTime.UtcNow;

        public string Summary =>
            $"✅{ApprovedCount} ❌{DeclinedCount} 💳{CardsCaptured} ⚡{PowerResetCount} 🚨{ErrorCount}";
   
    /// <summary>
    /// محرك تحليل العمليات المالية من الجورنال الإلكتروني
    /// يستخدم أساليب تحليل متعددة (Stream Analysis) لاستخراج العمليات
    /// ومعرفة صحة كل عملية وتفاصيلها
    /// </summary>
    public class TransactionAnalysisEngine
    {
        #region Events
        public event Action<string> OnLog;
        public event Action<ATMTransaction> OnTransactionDetected;
        public event Action<ATMError> OnErrorDetected;
        public event Action<RetainedCard> OnCardRetained;
        public event Action<Exception> OnError;
        #endregion

        #region Fields
        private readonly string _atmType;
        private readonly List<ITransactionPattern> _patterns;
        private readonly List<ATMTransaction> _transactions;
        private readonly List<ATMError> _errors;
        private readonly object _lock = new object();
        private ATMDetailedStatus _currentStatus;
        #endregion

        #region Constructor
        public TransactionAnalysisEngine(string atmType)
        {
            _atmType = atmType;
            _patterns = new List<ITransactionPattern>();
            _transactions = new List<ATMTransaction>();
            _errors = new List<ATMError>();
            _currentStatus = new ATMDetailedStatus();
            InitializePatterns();
        }
        #endregion

        #region Pattern Initialization
        private void InitializePatterns()
        {
            // تسجيل أنماط التحليل حسب نوع الصراف
            switch (_atmType)
            {
                case "NCR":
                    _patterns.Add(new NCRWithdrawalPattern());
                    _patterns.Add(new NCRBalanceInquiryPattern());
                    _patterns.Add(new NCRCardRetainPattern());
                    _patterns.Add(new NCRSupervisorPattern());
                    _patterns.Add(new NCRErrorPattern());
                    _patterns.Add(new NCRCashStatusPattern());
                    break;
                case "GRG":
                    _patterns.Add(new GRGWithdrawalPattern());
                    _patterns.Add(new GRGBalanceInquiryPattern());
                    _patterns.Add(new GRGCardRetainPattern());
                    _patterns.Add(new GRGSupervisorPattern());
                    _patterns.Add(new GRGErrorPattern());
                    _patterns.Add(new GRGCashStatusPattern());
                    break;
                case "WN":
                    _patterns.Add(new WNWithdrawalPattern());
                    _patterns.Add(new WNBalanceInquiryPattern());
                    _patterns.Add(new WNCardRetainPattern());
                    _patterns.Add(new WNSupervisorPattern());
                    _patterns.Add(new WNErrorPattern());
                    _patterns.Add(new WNCashStatusPattern());
                    break;
            }
            OnLog?.Invoke($"[Analysis] Initialized {_patterns.Count} patterns for {_atmType}");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// تحليل نص الجورنال واستخراج العمليات
        /// </summary>
        public List<ATMTransaction> AnalyzeJournalText(string journalText)
        {
            var results = new List<ATMTransaction>();
            if (string.IsNullOrWhiteSpace(journalText)) return results;

            string[] lines = journalText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var buffer = new List<string>();
            bool inTransaction = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // كشف بداية عملية جديدة
                if (IsTransactionStart(line))
                {
                    if (inTransaction && buffer.Count > 0)
                    {
                        var tx = ProcessTransactionBlock(buffer);
                        if (tx != null) results.Add(tx);
                    }
                    buffer.Clear();
                    inTransaction = true;
                }

                if (inTransaction) buffer.Add(line);

                // كشف الأخطاء
                var error = DetectError(line);
                if (error != null)
                {
                    lock (_lock) { _errors.Add(error); }
                    OnErrorDetected?.Invoke(error);
                }

                // كشف البطاقات المحتجزة
                var card = DetectRetainedCard(line);
                if (card != null)
                {
                    lock (_lock) { _currentStatus.RetainedCards.Add(card); }
                    OnCardRetained?.Invoke(card);
                }

                // تحديث حالة النقود
                UpdateCashStatus(line);
            }

            // معالجة آخر كتلة
            if (inTransaction && buffer.Count > 0)
            {
                var tx = ProcessTransactionBlock(buffer);
                if (tx != null) results.Add(tx);
            }

            lock (_lock) { _transactions.AddRange(results); }
            OnLog?.Invoke($"[Analysis] Extracted {results.Count} transactions from journal");
            return results;
        }

        /// <summary>
        /// تحليل ملف جورنال كامل
        /// </summary>
        public List<ATMTransaction> AnalyzeJournalFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    OnLog?.Invoke($"[Analysis] File not found: {filePath}");
                    return new List<ATMTransaction>();
                }
                string content = File.ReadAllText(filePath);
                return AnalyzeJournalText(content);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return new List<ATMTransaction>();
            }
        }

        /// <summary>
        /// الحصول على حالة الصراف التفصيلية
        /// </summary>
        public ATMDetailedStatus GetDetailedStatus()
        {
            lock (_lock)
            {
                _currentStatus.LastUpdate = DateTime.Now;
                // تحديد حالة التشغيل بناءً على آخر العمليات
                if (_transactions.Count > 0)
                {
                    var lastTx = _transactions.Last();
                    if (lastTx.Type == TransactionType.SupervisorMode)
                        _currentStatus.OperationalState = ATMOperationalState.SupervisorMode;
                    else if ((DateTime.Now - lastTx.Timestamp).TotalMinutes < 5)
                        _currentStatus.OperationalState = ATMOperationalState.InService;
                    else
                        _currentStatus.OperationalState = ATMOperationalState.OutOfService;

                    // آخر عملية صرف
                    var lastCash = _transactions.LastOrDefault(t => t.Type == TransactionType.CashWithdrawal && t.IsSuccessful);
                    if (lastCash != null) _currentStatus.LastCashWithdrawal = lastCash.Timestamp;
                }
                return _currentStatus;
            }
        }

        /// <summary>
        /// إنشاء تقرير تحليل شامل
        /// </summary>
        public TransactionAnalysisReport GenerateReport(string atmId, DateTime from, DateTime to)
        {
            lock (_lock)
            {
                var filtered = _transactions.Where(t => t.Timestamp >= from && t.Timestamp <= to).ToList();
                var report = new TransactionAnalysisReport
                {
                    ReportID = Guid.NewGuid().ToString("N").Substring(0, 8),
                    ATM_ID = atmId,
                    FromDate = from,
                    ToDate = to,
                    GeneratedAt = DateTime.Now,
                    TotalTransactions = filtered.Count,
                    SuccessfulTransactions = filtered.Count(t => t.IsSuccessful),
                    FailedTransactions = filtered.Count(t => t.Status == TransactionStatus.Failed),
                    TotalAmountDispensed = filtered.Where(t => t.Type == TransactionType.CashWithdrawal && t.IsSuccessful).Sum(t => t.Amount),
                    RetainedCardsCount = filtered.Count(t => t.Type == TransactionType.CardRetained),
                    Transactions = filtered,
                    Errors = _errors.Where(e => e.Timestamp >= from && e.Timestamp <= to).ToList()
                };

                // تصنيف حسب النوع
                foreach (TransactionType type in Enum.GetValues(typeof(TransactionType)))
                {
                    int count = filtered.Count(t => t.Type == type);
                    if (count > 0) report.TransactionsByType[type] = count;
                }

                // تصنيف حسب الساعة
                for (int h = 0; h < 24; h++)
                {
                    int hour = h;
                    report.TransactionsByHour[h] = filtered.Count(t => t.Timestamp.Hour == hour);
                }

                return report;
            }
        }

        /// <summary>
        /// فلترة العمليات حسب الفترة الزمنية
        /// </summary>
        public List<ATMTransaction> FilterByDateRange(DateTime from, DateTime to)
        {
            lock (_lock) { return _transactions.Where(t => t.Timestamp >= from && t.Timestamp <= to).ToList(); }
        }

        /// <summary>
        /// فلترة حسب نوع العملية
        /// </summary>
        public List<ATMTransaction> FilterByType(TransactionType type)
        {
            lock (_lock) { return _transactions.Where(t => t.Type == type).ToList(); }
        }

        /// <summary>
        /// الحصول على البطاقات المحتجزة
        /// </summary>
        public List<RetainedCard> GetRetainedCards()
        {
            lock (_lock) { return _currentStatus.RetainedCards.ToList(); }
        }
        #endregion

        #region Private Analysis Methods
        private bool IsTransactionStart(string line)
        {
            // أنماط بداية العملية حسب نوع الصراف
            switch (_atmType)
            {
                case "NCR":
                    return line.Contains("CARD INSERTED") || line.Contains("TXN START") ||
                           line.Contains("SUPERVISOR") || Regex.IsMatch(line, @"^\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}:\d{2}\s+CARD");
                case "GRG":
                    return line.Contains("[TXN]") || line.Contains("CARD_IN") ||
                           line.Contains("[SUPERVISOR]") || Regex.IsMatch(line, @"^\[\d{4}-\d{2}-\d{2}.*\]\s*TXN");
                case "WN":
                    return line.Contains("Transaction Start") || line.Contains("Card Inserted") ||
                           line.Contains("Supervisor Entry");
                default:
                    return false;
            }
        }

        private ATMTransaction ProcessTransactionBlock(List<string> block)
        {
            if (block == null || block.Count == 0) return null;

            var tx = new ATMTransaction
            {
                TransactionID = Guid.NewGuid().ToString("N").Substring(0, 12),
                RawJournalText = string.Join(Environment.NewLine, block)
            };

            // تطبيق كل الأنماط على الكتلة
            foreach (var pattern in _patterns)
            {
                if (pattern.TryMatch(block, tx))
                {
                    OnTransactionDetected?.Invoke(tx);
                    return tx;
                }
            }

            // محاولة تحليل أساسي إذا لم ينطبق أي نمط
            tx.Type = TransactionType.Unknown;
            tx.Timestamp = ExtractTimestamp(block[0]) ?? DateTime.Now;
            tx.Status = TransactionStatus.Completed;
            return tx;
        }

        private ATMError DetectError(string line)
        {
            // أنماط الأخطاء
            Match m = null;
            switch (_atmType)
            {
                case "NCR":
                    m = Regex.Match(line, @"ERROR\s*[:\-]\s*(\w+)\s*[:\-]\s*(.+)", RegexOptions.IgnoreCase);
                    if (!m.Success) m = Regex.Match(line, @"FAULT\s+(\w+)\s+(.+)", RegexOptions.IgnoreCase);
                    break;
                case "GRG":
                    m = Regex.Match(line, @"\[ERROR\]\s*(\w+)\s*:\s*(.+)", RegexOptions.IgnoreCase);
                    break;
                case "WN":
                    m = Regex.Match(line, @"Error Code:\s*(\w+)\s*-\s*(.+)", RegexOptions.IgnoreCase);
                    break;
            }

            if (m != null && m.Success)
            {
                return new ATMError
                {
                    Timestamp = ExtractTimestamp(line) ?? DateTime.Now,
                    ErrorCode = m.Groups[1].Value,
                    Description = m.Groups[2].Value.Trim(),
                    Severity = DetermineErrorSeverity(m.Groups[1].Value)
                };
            }
            return null;
        }

        private RetainedCard DetectRetainedCard(string line)
        {
            if (line.IndexOf("CARD RETAIN", StringComparison.OrdinalIgnoreCase) >= 0 ||
                line.IndexOf("CARD_CAPTURED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                line.IndexOf("Card Retained", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var card = new RetainedCard
                {
                    Timestamp = ExtractTimestamp(line) ?? DateTime.Now,
                    RetainCode = ExtractRetainCode(line),
                    RetainReason = ExtractRetainReason(line),
                    CardNumberMasked = ExtractCardNumber(line)
                };
                return card;
            }
            return null;
        }

        private void UpdateCashStatus(string line)
        {
            // تحديث حالة النقود من الجورنال
            Match m;
            switch (_atmType)
            {
                case "NCR":
                    m = Regex.Match(line, @"DISPENSED\s*:\s*(\d+)", RegexOptions.IgnoreCase);
                    if (m.Success) _currentStatus.CashInfo.TotalDispensed += decimal.Parse(m.Groups[1].Value);
                    m = Regex.Match(line, @"REMAINING\s*:\s*(\d+)", RegexOptions.IgnoreCase);
                    if (m.Success) _currentStatus.CashInfo.TotalRemaining = decimal.Parse(m.Groups[1].Value);
                    m = Regex.Match(line, @"CASSETTE\s*(\d+)\s*:\s*(\d+)\s*NOTES", RegexOptions.IgnoreCase);
                    if (m.Success) _currentStatus.CashInfo.CassetteNotes[int.Parse(m.Groups[1].Value)] = int.Parse(m.Groups[2].Value);
                    break;
                case "GRG":
                    m = Regex.Match(line, @"Dispense Amount:\s*(\d+)", RegexOptions.IgnoreCase);
                    if (m.Success) _currentStatus.CashInfo.TotalDispensed += decimal.Parse(m.Groups[1].Value);
                    m = Regex.Match(line, @"Cash Level:\s*(\w+)", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        _currentStatus.CashInfo.IsLow = m.Groups[1].Value.Equals("LOW", StringComparison.OrdinalIgnoreCase);
                        _currentStatus.CashInfo.IsEmpty = m.Groups[1].Value.Equals("EMPTY", StringComparison.OrdinalIgnoreCase);
                    }
                    break;
                case "WN":
                    m = Regex.Match(line, @"Amount Dispensed:\s*(\d+)", RegexOptions.IgnoreCase);
                    if (m.Success) _currentStatus.CashInfo.TotalDispensed += decimal.Parse(m.Groups[1].Value);
                    break;
            }
        }

        private DateTime? ExtractTimestamp(string line)
        {
            // محاولة استخراج التاريخ والوقت من السطر
            Match m = Regex.Match(line, @"(\d{2})/(\d{2})/(\d{4})\s+(\d{2}):(\d{2}):(\d{2})");
            if (m.Success)
            {
                try { return new DateTime(int.Parse(m.Groups[3].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[1].Value), int.Parse(m.Groups[4].Value), int.Parse(m.Groups[5].Value), int.Parse(m.Groups[6].Value)); }
                catch { }
            }
            m = Regex.Match(line, @"(\d{4})-(\d{2})-(\d{2})\s+(\d{2}):(\d{2}):(\d{2})");
            if (m.Success)
            {
                try { return new DateTime(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value), int.Parse(m.Groups[4].Value), int.Parse(m.Groups[5].Value), int.Parse(m.Groups[6].Value)); }
                catch { }
            }
            return null;
        }

        private string DetermineErrorSeverity(string errorCode)
        {
            if (string.IsNullOrEmpty(errorCode)) return "Warning";
            string upper = errorCode.ToUpper();
            if (upper.StartsWith("F") || upper.Contains("FATAL") || upper.Contains("CRIT")) return "Critical";
            if (upper.StartsWith("W") || upper.Contains("WARN")) return "Warning";
            return "Information";
        }

        private string ExtractRetainCode(string line)
        {
            Match m = Regex.Match(line, @"CODE[:\s]*(\w+)", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : "UNKNOWN";
        }

        private string ExtractRetainReason(string line)
        {
            Match m = Regex.Match(line, @"REASON[:\s]*(.+?)(?:\s*$|\|)", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value.Trim() : "Unknown reason";
        }

        private string ExtractCardNumber(string line)
        {
            Match m = Regex.Match(line, @"(\d{4})\s*\*{4,8}\s*(\d{4})");
            if (m.Success) return m.Groups[1].Value + "****" + m.Groups[2].Value;
            m = Regex.Match(line, @"\*{4,12}(\d{4})");
            if (m.Success) return "****" + m.Groups[1].Value;
            return "****";
        }
        #endregion
    }

    #region Transaction Pattern Interface
    /// <summary>
    /// واجهة نمط العملية - لكل نوع صراف أنماط مختلفة
    /// </summary>
    public interface ITransactionPattern
    {
        bool TryMatch(List<string> block, ATMTransaction tx);
    }
    #endregion

    #region NCR Patterns
    public class NCRWithdrawalPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (text.IndexOf("WITHDRAWAL", StringComparison.OrdinalIgnoreCase) < 0 &&
                text.IndexOf("CASH DISPENSED", StringComparison.OrdinalIgnoreCase) < 0) return false;

            tx.Type = TransactionType.CashWithdrawal;
            tx.Timestamp = ExtractTime(block[0]);
            Match m = Regex.Match(text, @"AMOUNT[:\s]*(\d+\.?\d*)", RegexOptions.IgnoreCase);
            if (m.Success) tx.Amount = decimal.Parse(m.Groups[1].Value);
            m = Regex.Match(text, @"CARD[:\s]*(\d{4}\*+\d{4}|\*+\d{4})", RegexOptions.IgnoreCase);
            if (m.Success) tx.CardNumber = m.Groups[1].Value;
            tx.Status = text.IndexOf("APPROVED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        text.IndexOf("DISPENSED OK", StringComparison.OrdinalIgnoreCase) >= 0
                        ? TransactionStatus.Completed : TransactionStatus.Failed;
            m = Regex.Match(text, @"RESP[:\s]*(\w+)", RegexOptions.IgnoreCase);
            if (m.Success) tx.ResponseCode = m.Groups[1].Value;
            return true;
        }
        private DateTime ExtractTime(string line)
        {
            Match m = Regex.Match(line, @"(\d{2})/(\d{2})/(\d{4})\s+(\d{2}):(\d{2}):(\d{2})");
            if (m.Success) try { return new DateTime(int.Parse(m.Groups[3].Value), int.Parse(m.Groups[2].Value), int.Parse(m.Groups[1].Value), int.Parse(m.Groups[4].Value), int.Parse(m.Groups[5].Value), int.Parse(m.Groups[6].Value)); } catch { }
            return DateTime.Now;
        }
    }

    public class NCRBalanceInquiryPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (text.IndexOf("BALANCE", StringComparison.OrdinalIgnoreCase) < 0) return false;
            tx.Type = TransactionType.BalanceInquiry;
            tx.Status = TransactionStatus.Completed;
            tx.Amount = 0;
            return true;
        }
    }

    public class NCRCardRetainPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (text.IndexOf("CARD RETAIN", StringComparison.OrdinalIgnoreCase) < 0 &&
                text.IndexOf("CARD CAPTURED", StringComparison.OrdinalIgnoreCase) < 0) return false;
            tx.Type = TransactionType.CardRetained;
            tx.Status = TransactionStatus.Completed;
            Match m = Regex.Match(text, @"REASON[:\s]*(.+?)(?:\s+|$)", RegexOptions.IgnoreCase);
            if (m.Success) tx.ErrorDescription = m.Groups[1].Value;
            return true;
        }
    }

    public class NCRSupervisorPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (text.IndexOf("SUPERVISOR", StringComparison.OrdinalIgnoreCase) < 0) return false;
            tx.Type = TransactionType.SupervisorMode;
            tx.Status = TransactionStatus.Completed;
            return true;
        }
    }

    public class NCRErrorPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (text.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) < 0 &&
                text.IndexOf("FAULT", StringComparison.OrdinalIgnoreCase) < 0) return false;
            tx.Type = TransactionType.Unknown;
            tx.Status = TransactionStatus.Failed;
            Match m = Regex.Match(text, @"ERROR[:\s]*(\w+)", RegexOptions.IgnoreCase);
            if (m.Success) tx.ErrorCode = m.Groups[1].Value;
            return true;
        }
    }

    public class NCRCashStatusPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx) { return false; } // معالجة خاصة في UpdateCashStatus
    }
    #endregion

    #region GRG Patterns
    public class GRGWithdrawalPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (text.IndexOf("[TXN]", StringComparison.OrdinalIgnoreCase) < 0 ||
                text.IndexOf("WITHDRAW", StringComparison.OrdinalIgnoreCase) < 0) return false;
            tx.Type = TransactionType.CashWithdrawal;
            tx.Status = text.Contains("SUCCESS") ? TransactionStatus.Completed : TransactionStatus.Failed;
            Match m = Regex.Match(text, @"Amount:\s*(\d+)", RegexOptions.IgnoreCase);
            if (m.Success) tx.Amount = decimal.Parse(m.Groups[1].Value);
            return true;
        }
    }
    public class GRGBalanceInquiryPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (!text.Contains("BALANCE") && !text.Contains("INQUIRY")) return false;
            tx.Type = TransactionType.BalanceInquiry; tx.Status = TransactionStatus.Completed; return true;
        }
    }
    public class GRGCardRetainPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (!text.Contains("CARD_CAPTURED") && !text.Contains("RETAIN")) return false;
            tx.Type = TransactionType.CardRetained; tx.Status = TransactionStatus.Completed; return true;
        }
    }
    public class GRGSupervisorPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (!text.Contains("[SUPERVISOR]")) return false;
            tx.Type = TransactionType.SupervisorMode; tx.Status = TransactionStatus.Completed; return true;
        }
    }
    public class GRGErrorPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (!text.Contains("[ERROR]")) return false;
            tx.Type = TransactionType.Unknown; tx.Status = TransactionStatus.Failed;
            Match m = Regex.Match(text, @"\[ERROR\]\s*(\w+)", RegexOptions.IgnoreCase);
            if (m.Success) tx.ErrorCode = m.Groups[1].Value; return true;
        }
    }
    public class GRGCashStatusPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx) { return false; }
    }
    #endregion

    #region WN Patterns
    public class WNWithdrawalPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (!text.Contains("Withdrawal") && !text.Contains("Cash Dispense")) return false;
            tx.Type = TransactionType.CashWithdrawal;
            tx.Status = text.Contains("Successful") ? TransactionStatus.Completed : TransactionStatus.Failed;
            Match m = Regex.Match(text, @"Amount:\s*(\d+)", RegexOptions.IgnoreCase);
            if (m.Success) tx.Amount = decimal.Parse(m.Groups[1].Value); return true;
        }
    }
    public class WNBalanceInquiryPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (!text.Contains("Balance")) return false;
            tx.Type = TransactionType.BalanceInquiry; tx.Status = TransactionStatus.Completed; return true;
        }
    }
    public class WNCardRetainPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (!text.Contains("Card Retained")) return false;
            tx.Type = TransactionType.CardRetained; tx.Status = TransactionStatus.Completed; return true;
        }
    }
    public class WNSupervisorPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (!text.Contains("Supervisor")) return false;
            tx.Type = TransactionType.SupervisorMode; tx.Status = TransactionStatus.Completed; return true;
        }
    }
    public class WNErrorPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx)
        {
            string text = string.Join(" ", block);
            if (!text.Contains("Error")) return false;
            tx.Type = TransactionType.Unknown; tx.Status = TransactionStatus.Failed; return true;
        }
    }
    public class WNCashStatusPattern : ITransactionPattern
    {
        public bool TryMatch(List<string> block, ATMTransaction tx) { return false; }
    }
    #endregion
}
