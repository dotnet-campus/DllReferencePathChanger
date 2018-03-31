using System;
using System.Diagnostics;
using System.Text;

namespace DllRefChanger.Utils
{
    static class CmdHelper
    {
        public static bool ExecuteTool(string toolFile, string args, out string result, out string errorMsg)
        {
            bool success = false;
            string msg = string.Empty;
            string errMsg = string.Empty;
            try
            {
                Process p;
                ProcessStartInfo psi;
                psi = new ProcessStartInfo(toolFile);
                psi.Arguments += args;

                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true; //允许重定向标准输出
                psi.CreateNoWindow = true;
                psi.RedirectStandardError = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;

                p = Process.Start(psi);

                msg = p.StandardOutput.ReadToEnd(); //Call ReadToEnd() before WaitForExit()

                p.WaitForExit();

                success = p.ExitCode == 0;
                if (!success)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(p.StandardError.ReadToEnd());
                    errMsg = sb.ToString();
                }

                p.Close();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message + " when invoke " + toolFile;
                return false;
            }
            finally
            {
                result = msg;
                errorMsg = errMsg;
            }
            return success;
        }
    }
}
