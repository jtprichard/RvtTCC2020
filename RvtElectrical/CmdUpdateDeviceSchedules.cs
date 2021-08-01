using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdUpdateDeviceSchedules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get GUID Settings from Setting Class
            Guid BoxIdGuid = TCCElecSettings.BoxIdGuid;

            //Get UIDocument & Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Get all device boxes in model
            IList<DeviceBox> deviceBoxes = DeviceBox.GetDeviceBoxes(doc);
            
            //Transaction to update circuits for each discrete box number
            using (Transaction trans = new Transaction(doc, "Update User Circuits"))
            {
                trans.Start();
                foreach(DeviceBox deviceBox in deviceBoxes)
                {
                    //Update the Concatenated circuit parameter with a string of circuits
                    deviceBox.UpdateDeviceBoxConcat();
                }

                trans.Commit();
            }

            return Result.Succeeded;

        }
    }
}
