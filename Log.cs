/// <summary>
        /// Contain log info.
        /// </summary>
        private static class Log
        {
            /// <summary>
            /// Default location to save the log file.
            /// </summary>
            private static readonly string logDirectory = Directory.GetCurrentDirectory() + @"\Logs\";

            /// <summary>
            /// Current log file's path.
            /// </summary>
            private static readonly string logPath = logDirectory + "Log " + 
                DateTime.Now.ToString("MM-dd hh_mm_ss") + ".txt";

            /// <summary>
            /// Append a new log to the log file.
            /// </summary>
            /// <param name="log">The line of log to write.</param>
            public static void WriteLog(string log)
            {
                if (Directory.Exists(logDirectory))
                {
                    File.AppendAllText(logPath, DateTime.Now.ToString() + " " + log + Environment.NewLine);
                }
                else
                {
                    Directory.CreateDirectory(logDirectory);
                    File.AppendAllText(logPath, DateTime.Now.ToString() + " " + log + Environment.NewLine);
                }
            }

            /// <summary>
            /// Ask the user if they want to delete the existing log file.
            /// </summary>
            public static void AutoDeleteLogs()
            {
                // Can only delete it if it is present.
                if (Directory.Exists(logDirectory))
                {
                    // Check if the file's size is over 10MB.
                    if (DirSize(new DirectoryInfo(logDirectory)) > 10 * 1024 * 1024)
                    {
                        DialogResult result
                            = MessageBox.Show("Do you want to erase the existing log directory?",
                            "Log Directory Size Over 10MB", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            Directory.Delete(logDirectory, true);
                            Directory.CreateDirectory(logDirectory);
                            WriteLog("User - Deleted existing log directory since it is over 10MB.");
                            WriteLog("System - Created a new log directory.");
                        }
                    }
                }
            }

            /// <summary>
            /// Calculates the size of a given directory.
            /// </summary>
            /// <param name="d">Directory Info of the target directory.</param>
            /// <returns>An integer represents the size of the directory.</returns>
            private static int DirSize(DirectoryInfo d)
            {
                int size = 0;
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += (int)fi.Length;
                }
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += DirSize(di);
                }
                return size;
            }
        }
