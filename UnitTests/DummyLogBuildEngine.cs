using System.Collections;
using Microsoft.Build.Framework;

namespace UnitTests
{
    internal class DummyLogBuildEngine : IBuildEngine5
    {
        public class TelemetryLog
        {
            public TelemetryLog(string eventName, IDictionary<string, string> properties)
            {
                EventName = eventName;
                Properties = properties;
            }
            public string EventName { get; }
            public IDictionary<string, string> Properties { get; }
        }

        public bool IsRunningMultipleNodes { get; set; }
        public bool ContinueOnError { get; set; }
        public int LineNumberOfTaskNode { get; set; }
        public int ColumnNumberOfTaskNode { get; set; }
        public string? ProjectFileOfTaskNode { get; set; }

        public List<BuildEventArgs> LoggedEvents { get; } = new List<BuildEventArgs>();
        public List<TelemetryLog> LoggedTelemetry { get; } = new List<TelemetryLog>();

        #region not implemented
        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs, string toolsVersion)
        {
            throw new NotImplementedException();
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        public BuildEngineResult BuildProjectFilesInParallel(string[] projectFileNames, string[] targetNames, IDictionary[] globalProperties, IList<string>[] removeGlobalProperties, string[] toolsVersion, bool returnTargetOutputs)
        {
            throw new NotImplementedException();
        }

        public bool BuildProjectFilesInParallel(string[] projectFileNames, string[] targetNames, IDictionary[] globalProperties, IDictionary[] targetOutputsPerProject, string[] toolsVersion, bool useResultsCache, bool unloadProjectsOnCompletion)
        {
            throw new NotImplementedException();
        }

        public object GetRegisteredTaskObject(object key, RegisteredTaskObjectLifetime lifetime)
        {
            throw new NotImplementedException();
        }

        public void Reacquire()
        {
            throw new NotImplementedException();
        }

        public void RegisterTaskObject(object key, object obj, RegisteredTaskObjectLifetime lifetime, bool allowEarlyCollection)
        {
            throw new NotImplementedException();
        }

        public object UnregisterTaskObject(object key, RegisteredTaskObjectLifetime lifetime)
        {
            throw new NotImplementedException();
        }

        public void Yield()
        {
            throw new NotImplementedException();
        }

        #endregion

        public void LogCustomEvent(CustomBuildEventArgs e) => LoggedEvents.Add(e);

        public void LogErrorEvent(BuildErrorEventArgs e) => LoggedEvents.Add(e);

        public void LogMessageEvent(BuildMessageEventArgs e) => LoggedEvents.Add(e);

        public void LogWarningEvent(BuildWarningEventArgs e) => LoggedEvents.Add(e);

        public void LogTelemetry(string eventName, IDictionary<string, string> properties)
        {
            LoggedTelemetry.Add(new TelemetryLog(eventName, properties));
        }

    }
}
