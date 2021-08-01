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
    public class CmdPBUpdatePLSchedule : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get GUID Settings from Setting Class
            Guid DeviceIdGuid = TCCElecSettings.DeviceIdGuid;

            //Get UIDocument & Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Retreive all lighting panels in project
            IList<DevicePanel> devicePanels = DevicePanel.GetDevicePanels(doc, System.PerfLighting);

            if(devicePanels.Count > 0)
            {
                //Iterate through each panel and populate
                foreach (DevicePanel devicePanel in devicePanels)
                {
                    if (devicePanel.PanelSchedule != null)
                    {
                        PanelScheduleView panelSchedule = devicePanel.PanelSchedule.PanelScheduleView;

                        if (!PBUpdateSchedules.PBUpdateSchedule(doc, panelSchedule))
                            return Result.Failed;
                    }
                    else
                    {
                        TaskDialog.Show("Panel Schedule Error", string.Format("Panel {0} does not have a " +
                            "panel schedule and could not be circuited", devicePanel.PanelName));
                    }

                }
            }
            else
            {
                TaskDialog.Show("Panel Schedule", "There are no compatible lighting panels to circuit");
            }

            return Result.Succeeded;
        }
    }
}
