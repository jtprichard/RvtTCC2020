using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Electrical;
using System.Dynamic;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdPBRemoveSpare : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get GUID Settings from Setting Class
            Guid DeviceIdGuid = TCCElecSettings.DeviceIdGuid;

            //Get UIDocument & Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Find Out if Panel Schedule View Active and Get Panel Schedule
            if(!(doc.ActiveView.ViewType == ViewType.PanelSchedule))
            {
                TaskDialog.Show("View Error", "Please Select a Panel Schedule View");
                return Result.Failed;
            }

            //Get panelscheduleview
            PanelScheduleView panelSchedule = doc.ActiveView as PanelScheduleView;

            //Create DevicePanel from PanelScheduleView
            DevicePanel panel = DevicePanelScheduleData.GetDevicePanelFromPanelSchedule(panelSchedule, doc);
            Element panelElement = panel.PanelElement;

            //BEGIN TRANSACTION
            Transaction trans = new Transaction(doc, "Fill In Spares");
            trans.Start();

            for (int i = panel.AddressStart; i <= panel.AddressEnd; i++)
            {
                //Identify slot number based on circuit i and its start address
                int slotNumber = i - panel.AddressStart + 1;

                if(panel.PanelSchedule.IsSlotSpare(slotNumber))
                {
                    panel.PanelSchedule.RemoveSlotSpare(slotNumber);
                }

            }
            trans.Commit();

            return Result.Succeeded;
        }
    }
}
