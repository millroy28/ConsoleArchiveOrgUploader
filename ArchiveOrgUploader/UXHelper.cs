

namespace ArchiveOrgUploader
{
    class UXHelper
    {
        ConsoleWriter _console = new ConsoleWriter();
        public UXHelper() { }
        public UXHelper(ConsoleWriter console)
        {
            _console = console;
        }

        public void ShowConfiguration(Config config)
        {
            _console.PrintDefault(["CONFIGURATION LOADED:"
                    ,$"Archive.org Access Key - {config.AccessKey} Secret Key - {config.SecretKey}"
                    ,$"Identifier Prefix - {config.ItemIdentifierPrefix}"
                    ,$"Log File Name - {config.LogFileName}"
                    ,$"Batch Size - {config.DefaultBatchSize.ToString()}"
                    ,$"Requesting Derive Process - {config.RequestDeriveProcess.ToString()}"]);
            _console.PrintBreaker();
        }

        public void PrintTitle()
        {
            _console.PrintHeader("  ARCHIVE.ORG WEB UPLOADER - V 0.2  ");
        }

        public void ResetScreen(string subheader)
        {
            _console.Clear();
            PrintTitle();
            _console.PrintDefault("");
            _console.PrintSubHeader(subheader);
            _console.PrintBreaker();
            return;
        }

        public Config ConfigMenu(Config config)
        {
            ResetScreen("Configuration:");
            ShowConfiguration(config);
            _console.PrintSubHeader("Please choose an option:");
            _console.PrintDefault(["", ""]);
            _console.PrintDefault(["      [ 1 ] Help / Documentation"
                                  ,"      [ 2 ] Run with these settings"
                                  ,"      [ 3 ] Run Test Mode with these Settings (for debugging - does not apply timestamps to log or send requests)"
                                  ,"      [ 4 ] Change Settings"
                                  ,""
                                 ]);
            int choice = _console.GetIntInput("Please enter your choice:", 4, 1);
            // it used to be more complicated - yes I know it could be a switch statment now. Don't reccomend it to me I don't care. 
            if(choice == 1)
            {
                return Help(config);
            } else if (choice == 2)
            {
                config.Upload = true;
            }
            else if (choice == 3)
            {
                config.Upload = false;
            }
            else if (choice == 4)
            {
                return GetUserConfigSettings(config);
            }

            return config;
        }
        public Config GetUserConfigSettings(Config config)
        {
            ResetScreen("Customize Settings");

            _console.PrintDefault($"Current Access Key: {config.AccessKey}");
            string accessKey = _console.GetStringInput("Enter new access key or leave blank to keep same and press enter: ");
            if (accessKey.Trim() != "")
            {
                config.AccessKey = accessKey;
            }

            _console.PrintDefault($"Current Secret Key: {config.SecretKey}");
            string secretKey = _console.GetStringInput("Enter new secret key or leave blank to keep same and press enter: ");
            if (secretKey.Trim() != "")
            {
                config.SecretKey = secretKey;
            }

            _console.PrintDefault($"Current Identifier Prefix: {config.ItemIdentifierPrefix}");
            string identifierPrefix = _console.GetStringInput("Enter new Item Identifier Prefix or leave blank to keep same and press enter: ");
            if (identifierPrefix.Trim() != "")
            {
                config.ItemIdentifierPrefix = identifierPrefix;
            }

            _console.PrintDefault($"Current Log File Name: {config.LogFileName}");
            string logFileName = _console.GetStringInput("Enter new log file name or leave blank to keep same and press enter: ");
            if (logFileName.Trim() != "")
            {
                config.LogFileName = logFileName;
            }

            _console.PrintDefault($"Current Arhcive Directory: {config.ArchiveDirectory}");
            string archiveDirectory = _console.GetStringInput("Enter new archive directory or leave blank to keep same and press enter: ");
            if (logFileName.Trim() != "")
            {
                config.ArchiveDirectory = archiveDirectory;
            }

            _console.PrintDefault($"Current Batch Size: {config.DefaultBatchSize}");
            int batchSize = _console.GetIntInput("Enter new batch size and press enter (max 200): ", 200, 1);
            config.DefaultBatchSize = batchSize;

            _console.PrintDefault($"Current Request Derive Process: {config.RequestDeriveProcess.ToString()}");
            string deriveInput = _console.GetStringInput("Type Y/Yes to request archive.org to derive additional files, N/No to bypass:", ["y", "yes", "n", "no"]);
            if(deriveInput == "y" || deriveInput == "yes") { config.RequestDeriveProcess = true; }
            else { config.RequestDeriveProcess = false; }

            return ConfigMenu(config);
        }

            
        public Config Help(Config config)
        {
            ResetScreen("Customize Settings");
            _console.PrintSubHeader("Docs:");

            _console.PrintDefault("To do: Write Docs");

            _console.PrintWarn("Press Enter to continue or Esc to exit to menu");
            

            return ConfigMenu(config);
        }

        public bool GetYesOrNo(string message)
        {
            message = message + " (Y/N):";
            string input = _console.GetStringInput(message, ["yes", "y", "no", "n"]);
            if (input == "y" || input == "yes") return true;
            else if (input == "n" || input == "no") return false;
            else return false;
        }
    }


}
