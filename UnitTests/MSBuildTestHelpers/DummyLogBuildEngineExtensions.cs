using Microsoft.Build.Framework;

namespace UnitTests.MSBuildTestHelpers
{
    internal static class DummyLogBuildEngineExtensions
    {
        public static IEnumerable<BuildErrorEventArgs> ErrorEvents(this DummyLogBuildEngine dummyLogBuildEngine) => dummyLogBuildEngine.LoggedEvents.OfType<BuildErrorEventArgs>();
        public static IEnumerable<BuildWarningEventArgs> WarningEvents(this DummyLogBuildEngine dummyLogBuildEngine) => dummyLogBuildEngine.LoggedEvents.OfType<BuildWarningEventArgs>();

        public static IEnumerable<string> ErrorMessages(this DummyLogBuildEngine dummyLogBuildEngine) => dummyLogBuildEngine.ErrorEvents().Select(e => e.Message);

        public static IEnumerable<string> WarningMessages(this DummyLogBuildEngine dummyLogBuildEngine) => dummyLogBuildEngine.WarningEvents().Select(e => e.Message);

        public static BuildWarningEventArgs SingleWarningEvent(this DummyLogBuildEngine dummyLogBuildEngine) => dummyLogBuildEngine.SingleEvent<BuildWarningEventArgs>();

        public static string SingleWarningMessage(this DummyLogBuildEngine dummyLogBuildEngine) => dummyLogBuildEngine.SingleWarningEvent().Message;

        public static BuildErrorEventArgs SingleErrorEvent(this DummyLogBuildEngine dummyLogBuildEngine) => dummyLogBuildEngine.SingleEvent<BuildErrorEventArgs>();

        public static string SingleErrorMessage(this DummyLogBuildEngine dummyLogBuildEngine) => dummyLogBuildEngine.SingleErrorEvent().Message;

        private static T SingleEvent<T>(this DummyLogBuildEngine dummyLogBuildEngine) where T : BuildEventArgs => dummyLogBuildEngine.LoggedEvents.OfType<T>().Single();

        public static bool HasEvents<T>(this DummyLogBuildEngine dummyLogBuildEngine) where T : BuildEventArgs => dummyLogBuildEngine.LoggedEvents.OfType<T>().Any();

    }
}
