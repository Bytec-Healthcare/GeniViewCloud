namespace GeniView.Data.Hardware
{
    public class LogInfo : Abstract.Data
    {
        public LogInfo DeepCopy()
        {
            // MemberwiseClone creates a shallow copy.
            // Use it for value types, then manually create ref types.
            LogInfo n = (LogInfo)this.MemberwiseClone();

            return n;
        }

        public long StartIndex { get; set; }

        public long EndIndex { get; set; }

        public long LogCount
        {
            get
            {
                // End index higher means no logs.
                if (StartIndex > EndIndex)
                    return 0;
                else
                    return (EndIndex - StartIndex) + 1; // Inlcuding log at StartIndex.
            }
        }
    }
}
