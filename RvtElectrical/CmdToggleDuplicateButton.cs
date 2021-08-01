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
    public class CmdToggleDuplicateButton : IExternalCommand
    //Trigger to Toggle Device Box Number Duplicate Monitoring Function On and Off
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ExternalApplication.Instance.ToggleDuplicateBox(commandData.Application);

            if (ExternalApplication.Instance.DuplicateBoxOn)
            {
                try
                {
                    IList<DeviceBox> duplicateDeviceBoxes = DeviceBox.GetDuplicateDeviceBoxes(doc);
                    if(duplicateDeviceBoxes.Count > 0)
                    {
                        IList<int> dup = new List<int>();
                        foreach(DeviceBox db in duplicateDeviceBoxes)
                        {
                            dup.Add(db.BoxId);
                        }
                        dup = dup.Distinct().ToList();
                        StringBuilder sb = new StringBuilder();
                        foreach(int d in dup)
                        {
                            if(d == dup.Last())
                                sb.Append(d.ToString());
                            else
                            sb.Append(d.ToString() + ", ");
                        }

                        TaskDialog.Show("DUPLICATE DEVICE BOXES", "Duplicate Device Boxes Have Been Detected in Model" + Environment.NewLine +
                            Environment.NewLine +
                            "Box Numbers: " + Environment.NewLine +
                            sb);
                    }

                }
                catch(Exception ex)
                {
                    UIUtils.EBSGenException(ex);
                }
            }

            return Result.Succeeded;
        }
    }
}
