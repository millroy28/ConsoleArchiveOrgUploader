

namespace ArchiveOrgUploader
{
    public class ConsoleWriter
    {
        public void PrintBreaker()
        {
            Console.WriteLine("--------------------------------");
            return;
        }
        public void PrintDefault(string message)
        {
            PrintLine(message);
            return;
        }
        public void PrintDefault(string[] message)
        {
            foreach (string s in message)
            {
                PrintLine(s);
            }
            return;
        }
        public void PrintWarn(string message)
        {
            SetWarningColor();
            PrintLine(message);
            ResetColor();
            return;
        }
        public void PrintWarn(string[] message)
        {
            SetWarningColor();
            foreach (string s in message)
            {
                PrintLine(s);
            }
            ResetColor();
            return;
        }
        public void PrintSuccess(string message)
        {
            SetSuccessColor();
            PrintLine(message);
            ResetColor();
            return;
        }
        public void PrintFail(string message)
        {
            SetFailColor();
            PrintLine(message);
            ResetColor();
            return;
        }
        public void PrintError(string message)
        {
            SetErrorColor();
            PrintLine(message);
            ResetColor();
            return;
        }
        public void PrintHeader(string message)
        {
            SetHeaderColor();
            PrintLine(message);
            ResetColor();
            return;
        }
        public void PrintSubHeader(string[] message)
        {
            SetSubHeaderColor();
            foreach (string s in message)
            {
                PrintLine(s);
            }
            ResetColor();
            return;
        }
        public void PrintSubHeader(string message)
        {
            SetSubHeaderColor();
            PrintLine("   " + message.ToUpper() + "");
            ResetColor();
            return;
        }
        public void PrintError(string[] message)
        {
            SetErrorColor();
            foreach (string s in message)
            {
                PrintLine(s);
            }
            ResetColor();
            return;
        }

        public string GetStringInput(string message, string[] acceptedValues)
        {
            SetInputPromptColor();
            PrintLine(message);
            SetInputColor();
            string input = Console.ReadLine();
            foreach(var acceptedValue in acceptedValues)
            {
                if(input.ToLower().Trim() == acceptedValue.ToLower())
                {
                    return input;
                }
            }
            // if input was not acceptable, re-prompt:
            PrintWarn("Input invalid!");
            ResetColor();
            return GetStringInput(message, acceptedValues);
        }
        public string GetStringInput(string message)
        {
            SetInputPromptColor();
            PrintLine(message);
            SetInputColor();
            string input = Console.ReadLine();
            ResetColor();
            return input;
        }
        public int GetIntInput(string message, int upperBound, int lowerBound)
        {
            SetInputPromptColor();
            PrintLine(message);
            SetInputColor();
            string input = Console.ReadLine();
            int parsedInput;
            if(int.TryParse(input, out parsedInput))
            {
                if(parsedInput >  upperBound || parsedInput < lowerBound)
                {
                    PrintWarn("Input out of bounds!");
                    ResetColor();
                    return GetIntInput(message, upperBound, lowerBound);
                }
            } else
            {
                PrintWarn("Input must be an integer value!");
                ResetColor();
                return GetIntInput(message, upperBound, lowerBound);
            }
            ResetColor();
            return parsedInput;
        }

        public void Clear()
        {
            Console.Clear();
            return;
        }

        // PRIVATE METHODS
        private void PrintLine(string message)
        {
            Console.WriteLine(message);
            return;
        }

        private void PrintLines(string[] messages)
        {
            foreach (string message in messages)
            {
                Console.WriteLine(message);
            }
            return;
        }
        private void SetHeaderColor()
        {
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.Black;
            return;
        }
        private void SetSubHeaderColor()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            return;
        }
        private void SetWarningColor()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            return;
        }
        private void SetErrorColor()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            return;
        }
        private void SetDefaultColor()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            return;
        }
        private void SetInputPromptColor()
        {
            Console.ForegroundColor= ConsoleColor.Green;
            return;
        }
        private void SetInputColor()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            return;
        }
        private void ResetColor()
        {
            SetDefaultColor();
            return;
        }
        private void SetSuccessColor()
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Green;
            return;
        }
        private void SetFailColor()
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Red;
            return;
        }
    }
}
