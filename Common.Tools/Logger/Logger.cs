using System;
using System.IO;
using log4net;
using log4net.Config;
using log4net.Repository;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace Common.Tools.Logger
{
    public static class Logger
    {
        static ILoggerRepository _repository = LogManager.CreateRepository("NETCoreRepository");
        
        private static ILog _logError = LogManager.GetLogger(_repository.Name,"logerror");
        private static ILog _logInfo = LogManager.GetLogger(_repository.Name, "loginfo");
        private static ILog _logDebug = LogManager.GetLogger(_repository.Name, "logdebug");
        private static ILog _logWarn = LogManager.GetLogger(_repository.Name, "logwarn");
        private static ILog _logFatal = LogManager.GetLogger(_repository.Name, "logfatal");
        private static ILog _logErrDb = LogManager.GetLogger(_repository.Name, "logerrdb");

        public static void Config(string configFile=null)
        {
            if (string.IsNullOrEmpty(configFile)) configFile = $"{AppDomain.CurrentDomain.BaseDirectory}log4net.config";
            if (!File.Exists(configFile))
            {
                configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);
            }
            XmlConfigurator.Configure(_repository, new FileInfo(configFile));

            _logError = LogManager.GetLogger(_repository.Name, "logerror");
            _logInfo = LogManager.GetLogger(_repository.Name, "loginfo");
            _logDebug = LogManager.GetLogger(_repository.Name, "logdebug");
            _logWarn = LogManager.GetLogger(_repository.Name, "logwarn");
            _logFatal = LogManager.GetLogger(_repository.Name, "logfatal");
            _logErrDb = LogManager.GetLogger(_repository.Name, "logerrdb");
        }
        public static void Error(string message)
        {
            if (_logError.IsErrorEnabled)
            {
                _logError.Error(message);
            }
        }

        public static void Error(string message, Exception ex)
        {
            if (_logError.IsErrorEnabled)
            {
                string msg = message.Trim();
                _logError.Error(message, ex);
            }
        }

        public static void Debug(string message)
        {

            if (_logDebug.IsDebugEnabled)
            {
                _logDebug.Debug(message);
            }

        }

        public static void Debug(string message, Exception ex)
        {

            if (_logDebug.IsDebugEnabled)
            {
                _logDebug.Debug(message, ex);
            }

        }

        /// <summary>
        /// 严重错误
        /// </summary>
        /// <param name="message"></param>
        public static void Fatal(string message)
        {

            if (_logFatal.IsFatalEnabled)
            {
                _logFatal.Fatal(message);
            }
        }

        public static void Fatal(string message, Exception ex)
        {

            if (_logFatal.IsFatalEnabled)
            {
                _logFatal.Fatal(message, ex);
            }
        }

        /// <summary>
        /// 消息日志
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            if (_logInfo.IsInfoEnabled)
            {
                _logInfo.Info(message);
            }
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(string message)
        {
            if (_logWarn.IsWarnEnabled)
            {
                _logWarn.Warn(message);
            }
        }

        public static void Warn(string message, Exception ex)
        {
            if (_logWarn.IsWarnEnabled)
            {
                _logWarn.Warn(message, ex);
            }
        }

        public static void LogErrDb<T>(T log)
        {
            if (log == null)
                return;
            if (_logErrDb.IsErrorEnabled)
            {
                _logErrDb.Error(log);
            }
        }
    }
}