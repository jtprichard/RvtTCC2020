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
    static class PBRemoveCircuits
    {
        public static bool PBRemoveCircuit(Document doc, PanelScheduleView panelSchedule)
        {
            //Get GUID Settings from Setting Class
            Guid DeviceIdGuid = TCCElecSettings.DeviceIdGuid;

            //Create DevicePanel from PanelScheduleView
            DevicePanel panel = DevicePanelScheduleData.GetDevicePanelFromPanelSchedule(panelSchedule, doc);
            Element panelElement = panel.PanelElement;

            //Get other panels of same system and system type to test for errors
            var devicePanels = DevicePanel.GetDevicePanels(doc, panel.System, panel.SystemType);

            //Filter for panels in same venue
            devicePanels = devicePanels.Where(dp => dp.Venue == panel.Venue)
                .ToList();

            //If Panel is low voltage, filter for similar data connector type
            if(!panel.IsPowerPanel)
            {
                devicePanels = devicePanels.Where(dp => dp.DeviceId.ConnectorGroup == panel.DeviceId.ConnectorGroup)
                    .ToList();
            }

            //Test Panels for Errors
            if (DevicePanel.TestPanelsForErrors(devicePanels) == false)
                return false;

            //Get Device Boxes
            IList<DeviceBox> deviceBoxes = DeviceBox.GetDeviceBoxes(doc, panel.System);

            //Filter Device Boxes by Venue
            deviceBoxes = DeviceBox.FilterBoxByVenue(deviceBoxes, panel.Venue);

            //Get All Device Boxes in model to ensure there are none that should be circuited here
            IList<DeviceBox> allDeviceBoxes = DeviceBox.GetDeviceBoxes(doc);

            //Filter for Device Connectors
            var deviceConnectors = DeviceConnector.GetConnectorsByBox(deviceBoxes, panel.DeviceId.ConnectorGroup);
            var allDeviceConnectors = DeviceConnector.GetConnectorsByBox(allDeviceBoxes);

            //BEGIN TRANSACTION
            Transaction trans = new Transaction(doc, "Remove Circuits");
            trans.Start();

            //Section removes circuits in panel
            for (int i = panel.AddressStart; i <= panel.AddressEnd; i++)
            {
                //Identify slot number based on circuit i and its start address
                int slotNumber = i - panel.AddressStart + 1;

                //Use Linq query to find connectors that are equal to the current circuit i
                List<DeviceConnector> dcc = deviceConnectors
                    .Where(dc => dc.ConnectorCircuit == i)
                    .ToList();

                //Find the current electrical system for the slot to be updated
                ElectricalSystem es = panel.PanelSchedule.GetCircuitBySlot(slotNumber);

                //Disconnect the panel slot provided it is not locked

                foreach (DeviceConnector dc in dcc)
                {
                    if (dc.CircuitData.HasCircuit || dc.CircuitData.HasPanel)
                    {
                        dc.CircuitData.RemoveCircuit();
                    }
                }

                //if (es != null)
                //{
                //    if (!panel.PanelSchedule.IsSlotLocked(i))
                //    {
                //        es.DisconnectPanel();
                //    }
                //}
            }

            trans.Commit();

            return true;
        }
    }
}
