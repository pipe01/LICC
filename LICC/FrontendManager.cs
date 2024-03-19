using LICC.API;

namespace LICC
{
    /// <summary>
    /// Class to be used to set and manage the primary frontend.
    /// </summary>
    public static class FrontendManager
    {
        internal static IWriteableHistory _History;
        internal static IWriteableHistory History
        {
            get
            {
                if (_History == null)
                    _History = new History();

                return _History;
            }
        }

        internal static Frontend _Frontend;
        public static Frontend Frontend
        {
            get => _Frontend;
            set
            {
                _Frontend?.Stop();
                _Frontend = value;

                // Set the history to the primary history. If the history was already set it gets replaced.
                _Frontend.History = History;
                // Assume that when a frontend supports Stop(), it also supports another call to Init()
                _Frontend.Init();
            }
        }

        public static bool HasFrontend => Frontend != null;
    }
}
