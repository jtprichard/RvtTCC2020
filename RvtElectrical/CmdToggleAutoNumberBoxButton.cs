using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdToggleAutoNumberBoxButton : IExternalCommand
    //Trigger to Toggle Device Box AutoNumber Function On and Off
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ExternalApplication.Instance.ToggleAutoNumberBox(commandData.Application);
            UIApplication uiapp = ExternalApplication._uiApplicationAutoUpdate;

            int boxNumber;
            if (ExternalApplication.Instance.AutoNumberBoxOn)
            {
                //GET THE CURRENT BOX NUMBER
                string rt = "TCC Technical";
                string rp = "Device Boxes";
                string riBoxNumStr = "Box_Number_Text";
                TextBox tb = null;

                if (RibbonUtils.GetRibbonItem(rt, rp, riBoxNumStr, uiapp, out RibbonItem riBoxNum))
                {
                    tb = riBoxNum as TextBox;
                    if (tb.Value == null)
                    {
                        TaskDialog.Show("BOX NUMBER ERRROR", "Please provide a Box Number in the text box");
                        ExternalApplication.Instance.ToggleAutoNumberBox(commandData.Application);
                        return Result.Failed;
                    }

                    else
                        boxNumber = int.Parse(tb.Value.ToString());
                }
                else
                {
                    TaskDialog.Show("Box Number Error", "Provide a valid box number in the associated text box");
                    ExternalApplication.Instance.ToggleAutoNumberBox(commandData.Application);
                    return Result.Failed;
                }


                //DETERMINE IF THE BOX NUMBER IS ALREADY BEING USED
                //If so, offer to advance to next available.
                if (DeviceBox.GetDuplicateDeviceBoxes(doc, boxNumber).Count() > 0)
                {
                    string strMessage = string.Format("Box Number {0} is already in use.  Use next available number?", boxNumber);
                    TaskDialogResult result = TaskDialog.Show("DUPLICATE DEVICE BOX NUMBER", strMessage,
                        TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

                    if (result == TaskDialogResult.Yes)
                    {
                        boxNumber = DeviceBox.NextDeviceBoxNumber(boxNumber, doc);
                        tb.Value = boxNumber;
                    }
                    else
                    {
                        ExternalApplication.Instance.ToggleAutoNumberBox(commandData.Application);
                        return Result.Failed;
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
