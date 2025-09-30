namespace UnitTests.MSBuildTestHelpers
{
    internal class ItemSpecModifiersMetadata
    {
        public string? FullPath { get; set; }
        public string? RootDir { get; set; }
        public string? FileName { get; set; }
        public string? Extension { get; set; }
        public string? Directory { get; set; }
        public string? RelativeDir { get; set; }
        // not derivable
        // public string? RecursiveDir { get; set; }
        // Is the ItemSpec
        //public string? Identity { get; set; }

        public string? DefiningProjectFullPath { get; set; }
        public string? DefiningProjectDirectory { get; set; }
        public string? DefiningProjectName { get; set; }
        public string? DefiningProjectExtension { get; set; }

        // ISO 8601 Universal time with sortable format
        internal const string FileTimeFormat = "yyyy'-'MM'-'dd HH':'mm':'ss'.'fffffff";
        
        public string? ModifiedTime { get; set; }
        public string? CreatedTime { get; set; }
        public string? AccessedTime { get; set; }
        public DateTime? ModifiedTimeDateTime { get; set; }
        public DateTime? CreatedTimeDateTime { get; set; }
        public DateTime? AccessedTimeDateTime { get; set; }


        internal static string GetTimeString(DateTime dateTime) => dateTime.ToString(FileTimeFormat, null);
        internal string? TryGetModifiedTime() => TryGetTime(ModifiedTimeDateTime, ModifiedTime);
        internal string? TryGetCreatedTime() => TryGetTime(CreatedTimeDateTime, CreatedTime);
        internal string? TryGetAccessedTime() => TryGetTime(AccessedTimeDateTime, AccessedTime);


        private static string? TryGetTime(DateTime? dateTime, string? dateTimeString)
        {
            if(dateTime != null)
            {
                return GetTimeString(dateTime.Value);
            }
            return dateTimeString;
        }
    }
}
