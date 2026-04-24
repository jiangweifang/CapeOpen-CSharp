using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CapeOpen
{
    /// <summary>
    /// Centralized crash / error logger backed by NLog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Because the CapeOpen assembly is a COM in-process server loaded by external host
    /// applications (Aspen Plus, COFE, etc.), we cannot rely on the host process providing
    /// an NLog configuration file. The configuration is therefore created programmatically
    /// and writes to a fixed location under <c>%LOCALAPPDATA%\CapeOpen\logs</c>.
    /// </para>
    /// <para>
    /// <see cref="Initialize"/> hooks <see cref="AppDomain.UnhandledException"/> and
    /// <see cref="System.Threading.Tasks.TaskScheduler.UnobservedTaskException"/> so that
    /// any unhandled exception from a CAPE-OPEN PMC instance gets persisted to disk.
    /// It is safe to call <see cref="Initialize"/> multiple times; only the first call
    /// installs the configuration and event handlers.
    /// </para>
    /// </remarks>
    [System.Runtime.InteropServices.ComVisible(false)]
    public static class CrashLogger
    {
        private static int s_initialized;
        private static Logger s_logger;

        /// <summary>
        /// Gets the shared NLog <see cref="Logger"/>. Initializes the logging
        /// infrastructure on first access.
        /// </summary>
        public static Logger Logger
        {
            get
            {
                Initialize();
                return s_logger;
            }
        }

        /// <summary>
        /// Gets the directory where crash log files are written.
        /// </summary>
        public static string LogDirectory { get; private set; }

        /// <summary>
        /// Configures NLog and registers global unhandled exception handlers.
        /// This method is idempotent.
        /// </summary>
        public static void Initialize()
        {
            if (Interlocked.CompareExchange(ref s_initialized, 1, 0) != 0)
                return;

            try
            {
                LogDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CapeOpen",
                    "logs");
                Directory.CreateDirectory(LogDirectory);

                var config = new LoggingConfiguration();

                var crashFile = new FileTarget("crashFile")
                {
                    FileName = Path.Combine(LogDirectory, "crash-${shortdate}.log"),
                    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}${onexception:${newline}${exception:format=ToString,Data}}",
                    KeepFileOpen = false,
                    Encoding = System.Text.Encoding.UTF8,
                    ArchiveAboveSize = 10 * 1024 * 1024, // 10 MB
                    MaxArchiveFiles = 10,
                    ConcurrentWrites = true
                };

                var traceFile = new FileTarget("traceFile")
                {
                    FileName = Path.Combine(LogDirectory, "trace-${shortdate}.log"),
                    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}",
                    KeepFileOpen = false,
                    Encoding = System.Text.Encoding.UTF8,
                    ArchiveAboveSize = 10 * 1024 * 1024,
                    MaxArchiveFiles = 5,
                    ConcurrentWrites = true
                };

                config.AddTarget(crashFile);
                config.AddTarget(traceFile);
                config.AddRule(LogLevel.Error, LogLevel.Fatal, crashFile);
                config.AddRule(LogLevel.Info, LogLevel.Warn, traceFile);

                LogManager.Configuration = config;

                s_logger = LogManager.GetLogger("CapeOpen");

                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

                // WinForms UI thread exception. Critical for catching crashes inside
                // PMC editor dialogs (BaseUnitEditor, ArrayValueEditorForm, etc.) when
                // hosted by PMEs such as PRO/II / Aspen Plus / COFE.
                try
                {
                    Application.SetUnhandledExceptionMode(
                        UnhandledExceptionMode.CatchException, false);
                }
                catch { /* host may have already configured this; ignore */ }
                Application.ThreadException += OnWinFormsThreadException;

                s_logger.Info("CapeOpen CrashLogger initialized. Log directory: {0}. Host process: {1} (PID {2})",
                    LogDirectory,
                    System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                    System.Diagnostics.Process.GetCurrentProcess().Id);
            }
            catch
            {
                // Logging must never crash the host. Swallow any setup failure.
                s_initialized = 0;
            }
        }

        /// <summary>
        /// Logs an exception thrown by PMC code. Safe to call from any thread; never throws.
        /// </summary>
        /// <param name="ex">The exception to record.</param>
        /// <param name="context">Optional contextual message describing the operation.</param>
        public static void LogException(Exception ex, string context = null)
        {
            if (ex == null) return;
            try
            {
                Initialize();
                s_logger?.Error(ex, context ?? "Unhandled exception");
            }
            catch
            {
                // Never propagate logging errors to the host.
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null)
                {
                    s_logger?.Fatal(ex, "AppDomain.UnhandledException (IsTerminating={0})", e.IsTerminating);
                }
                else
                {
                    s_logger?.Fatal("AppDomain.UnhandledException with non-Exception object: {0}", e.ExceptionObject);
                }
                LogManager.Flush(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // Never throw from a global exception handler.
            }
        }

        private static void OnUnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                s_logger?.Error(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            }
            catch
            {
                // Never throw from a global exception handler.
            }
        }

        private static void OnWinFormsThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            try
            {
                s_logger?.Error(e.Exception, "WinForms Application.ThreadException (sender={0})", sender?.GetType().FullName ?? "null");
                LogManager.Flush(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // Never throw from a global exception handler.
            }
        }
    }
}
