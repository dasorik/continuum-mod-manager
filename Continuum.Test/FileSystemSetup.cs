namespace Continuum.Test
{
    public static class FileSystemSetup
    {
        private static string _tempPath;

        public static string TempPath
        {
            get
            {
                if (string.IsNullOrEmpty(_tempPath))
                {
                    _tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ContinuumModManager.Test");
                }

                return _tempPath;
            }
        }
    }
}
