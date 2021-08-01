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
    public class CmdPBRemoveCircuits : IExternalCommand
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

            if (PBRemoveCircuits.PBRemoveCircuit(doc, panelSchedule))
                return Result.Succeeded;
            else
                return Result.Failed;
        }
    }
}
