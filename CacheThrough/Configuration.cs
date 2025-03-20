namespace ServerSide.CacheThrough;

using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository;
using log4net.Config;
using System;

public class Configuration
{

    public static string GetFileVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
        return fileVersionInfo.FileVersion;
    }

    public static void ConfigureLogging(string logFilePath, string logLevel = "Debug")
    {
        var layout = new PatternLayout
        {
            // ConversionPattern = "%date [%thread] %-5level %logger (%file:%line) - %message%newline"
            ConversionPattern = "%date [%thread] %-5level (%file:%line) - %message%newline"
        };
        layout.ActivateOptions();

        var appender = new RollingFileAppender
        {
            File = logFilePath,
            AppendToFile = true,
            RollingStyle = RollingFileAppender.RollingMode.Size,
            MaxSizeRollBackups = 100,
            MaximumFileSize = "10MB",
            StaticLogFileName = true,
            Layout = layout,
            ImmediateFlush = true,
            //LocationInfo = true
        };
        appender.ActivateOptions();

        ILoggerRepository repository = LogManager.GetRepository();
        BasicConfigurator.Configure(repository, appender);

        // Set the log level from parameter
        if (logLevel != null)
        {
            switch (logLevel.ToLower())
            {
                case "debug":
                    ((log4net.Repository.Hierarchy.Hierarchy)repository).Root.Level = log4net.Core.Level.Debug;
                    break;
                case "info":
                    ((log4net.Repository.Hierarchy.Hierarchy)repository).Root.Level = log4net.Core.Level.Info;
                    break;
                case "warn":
                    ((log4net.Repository.Hierarchy.Hierarchy)repository).Root.Level = log4net.Core.Level.Warn;
                    break;
                case "error":
                    ((log4net.Repository.Hierarchy.Hierarchy)repository).Root.Level = log4net.Core.Level.Error;
                    break;
                case "fatal":
                    ((log4net.Repository.Hierarchy.Hierarchy)repository).Root.Level = log4net.Core.Level.Fatal;
                    break;
                default:
                    ((log4net.Repository.Hierarchy.Hierarchy)repository).Root.Level = log4net.Core.Level.Debug;
                    break;
            }
        }
        else
        {
            ((log4net.Repository.Hierarchy.Hierarchy)repository).Root.Level = log4net.Core.Level.Debug;
        }
      ((log4net.Repository.Hierarchy.Hierarchy)repository).RaiseConfigurationChanged(EventArgs.Empty);
    }


}