using System;
using System.Windows.Forms;
using Autodesk.Revit.UI;

namespace RvtElectrical
{
    public static class UIUtils
    {
        public static void EBSGenException(Exception ex)
        {
            string message = ex.Message;
            string stacktrace = ex.StackTrace;

            if (TaskDialog.Show("Application Error", "Application Error.  Please report to developer" + Environment.NewLine +
                "Internal Error Message: " + message + Environment.NewLine + Environment.NewLine +
                "Copy Error Information to Clipboard? ", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No) == TaskDialogResult.Yes)
                //Copy Family Name Code to Clipboard
                Clipboard.SetText(stacktrace);
        }
        public static void EBSGenException(Exception ex, String devMessage)
        {
            string message = ex.Message;
            string stacktrace = ex.StackTrace;

            if (TaskDialog.Show("Application Error", "Application Error.  Please report to developer" + Environment.NewLine +
                "Developer Message: " + devMessage + Environment.NewLine + Environment.NewLine +
                "Internal Error Message: " + message + Environment.NewLine + Environment.NewLine +
                "Copy Error Information to Clipboard? ", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No) == TaskDialogResult.Yes)
                //Copy Family Name Code to Clipboard
                Clipboard.SetText(stacktrace);
        }
    }
}
