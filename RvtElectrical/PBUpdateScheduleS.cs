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
    static class PBUpdateSchedules
    {
        public static bool PBUpdateSchedule(Document doc, PanelScheduleView panelSchedule)
        {
            //Get GUID Settings from Setting Class
            Guid DeviceIdGuid = TCCElecSettings.DeviceIdGuid;

            //Create DevicePanel from PanelScheduleView
            DevicePanel panel = DevicePanelScheduleData.GetDevicePanelFromPanelSchedule(panelSchedule, doc);
            Element panelElement = panel.PanelElement;

            //Get Panel Schedule Configuration
            PanelConfiguration panelConfig = panel.PanelSchedule.PanelConfig;

            //Get other panels of same system and system type
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
            Transaction trans = new Transaction(doc, "Update Panel Schedule");
            trans.Start();

            //If there are any device connectors circuited to the panel 
            //That are in another venue or not in the circuit range,
            //then remove them
            List<DeviceConnector> adcc = allDeviceConnectors
                .Where(adc => adc.CircuitData.PanelName == panel.PanelName)
                .Where(adc => (adc.ConnectorCircuit < panel.AddressStart || adc.ConnectorCircuit > panel.AddressEnd || adc.Venue != panel.Venue))
                .ToList();

            foreach (DeviceConnector adc in adcc)
            {
                if (adc.CircuitData.HasPanel)
                {
                    adc.CircuitData.RemoveCircuit();
                }
            }
            //Regenerate model before removing other devices
            doc.Regenerate();

            //Section removes circuits in panel that are either
            //changing location or a slot that they are in
            //is being changed
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

                //Determine if either the slot is used and will be recircuited
                //or if a connector assigned to the slot will be recircuited
                
                bool circuitChange = false;

                //If the device connector intended for this circuit is circuited and is not equal to this circuit, 
                //then disconnect it
                //WARNING - THIS MAY HAVE ADVERSE AFFECT IF CIRCUIT NUMBERS CONTINUE FROM PANEL VS STARTING AT 1
                foreach (DeviceConnector dc in dcc)
                {
                    if (dc.CircuitData.HasPanel && dc.CircuitData.DeviceElectricalSystem.CircuitNumber != slotNumber.ToString())
                    {
                        circuitChange = true;
                    }
                }
                if (circuitChange)
                {
                    foreach (DeviceConnector dc in dcc)
                    {
                        dc.CircuitData.RemoveCircuit();
                    }
                }

                //If the device connector has a circuit but no panel, disconnect it
                foreach (DeviceConnector dc in dcc)
                {
                    if (dc.CircuitData.HasCircuit && !dc.CircuitData.HasPanel)
                    {
                        dc.CircuitData.RemoveCircuit();
                    }
                }


                //If there is no device connector intended for this circuit and the panel slot has a circuit, 
                //then disconnect the panel slot provided it is not locked
                if (dcc.Count() == 0 && es != null)
                {
                    if (!panel.PanelSchedule.IsSlotLocked(i))
                    {
                        es.DisconnectPanel();
                    }
                }


                //If the device connector(s) is intended for this slot, but the slot is a spare,
                //then remove the spare(s)
                if ((dcc.Count() > 0))
                {
                    for(int j = 0; j<dcc[0].Poles; j++)
                    {
                        if (panel.PanelSchedule.IsSlotSpare(slotNumber + (j * 2)))
                            panel.PanelSchedule.RemoveSlotSpare(slotNumber + (j * 2));
                    }
                }
                    
                //Gegenerate the model before inserting new information into the panel
                doc.Regenerate();
            }

            //Section circuits to panel
            //adding new circuits, appending,
            //or creating spares
            for (int i = panel.AddressStart; i <= panel.AddressEnd; i++)
            {
                //Identify slot number based on circuit i and its start address
                int slotNumber = i - panel.AddressStart + 1;

                //Use Linq query to find connectors that are equal to the current circuit i
                List<DeviceConnector> dcc = deviceConnectors
                    .Where(dc => dc.ConnectorCircuit == i)
                    .OrderByDescending(dc => dc.CircuitData.HasPanel)
                    .ToList();

                //Test to confirm circuit poles are correct
                if (dcc.Count() > 0)
                {
                    if (!TestCircuitPoles(dcc[0], slotNumber, (panel.AddressEnd - panel.AddressStart + 1)))
                    {
                        trans.Dispose();
                        return false;
                    }
                }

                //One Circuit - Schedule to Panel
                if (dcc.Count() == 1)
                {
                    DeviceConnector deviceConnector = dcc.First();
                    if (!deviceConnector.CircuitData.HasCircuit)
                    {
                        deviceConnector.CircuitData.CreateCircuit(panel.CircuitData.SystemType);
                    }
                    if (!deviceConnector.CircuitData.HasPanel)
                    {
                        deviceConnector.CircuitData.SelectPanel(panelElement);
                    }

                    doc.Regenerate();
                    panel.PanelSchedule.SetSlotDescription(slotNumber, "Box " + deviceConnector.BoxId);
                    if(panel.PanelSchedule.HasAddressColumns)
                        panel.PanelSchedule.SetSlotAddress(slotNumber, i, deviceConnector);
                    
                }

                //Multiple Circuits - Schedule and Append to Panel
                else if(dcc.Count() > 1)
                {
                    //Verify all circuits have same number of poles
                    int numPoles = dcc[0].Poles;
                    foreach(var dc in dcc)
                    {
                        if(numPoles != dc.Poles)
                        {
                            TaskDialog.Show("Breaker Error", "You are attempting to circuit services with different number of poles");
                            trans.Dispose();
                            return false;
                        }

                    }


                    //Connect system to connector circuits if not already connected
                    foreach(var dc in dcc)
                    {
                        if (!dc.CircuitData.HasCircuit)
                        {
                            dc.CircuitData.CreateCircuit(panel.CircuitData.SystemType);
                        }
                    }

                    //Circuit first in list to panel
                    DeviceConnector deviceConnector = dcc[0];
                    List<string> slotDescription = new List<string>();

                    if (!deviceConnector.CircuitData.HasPanel)
                    {
                        deviceConnector.CircuitData.SelectPanel(panelElement);
                    }
                    slotDescription.Add(deviceConnector.BoxId.ToString());

                    //Append remaining circuits
                    ElementSet circuitSet = new ElementSet();
                    for (int j = 1; j < dcc.Count(); j++)
                    {
                        if(!dcc[j].CircuitData.HasPanel)
                            circuitSet.Insert(dcc[j].CircuitData.CircuitElement);
                        slotDescription.Add(dcc[j].BoxId.ToString());
                    }
                    if(circuitSet.Size > 0)
                        deviceConnector.CircuitData.AddElementToCircuit(circuitSet);

                    doc.Regenerate();

                    slotDescription = slotDescription.Distinct().OrderBy(desc => desc).ToList();
                    string slotDescriptionText = slotDescription.First();

                    for(int j=1; j < slotDescription.Count(); j++)
                    {
                        slotDescriptionText = slotDescriptionText + ", " + slotDescription[j];
                    }
                    
                    panel.PanelSchedule.SetSlotDescription(slotNumber, "Box " + slotDescriptionText);
                    if (panel.PanelSchedule.HasAddressColumns)
                        panel.PanelSchedule.SetSlotAddress(slotNumber, i, deviceConnector);

                    //if (panel.System == System.PerfLighting)
                    //    panel.PanelSchedule.SetSlotAddress(slotNumber, i.ToString());
                    //else if (panel.System == System.PerfSVC)
                    //    panel.PanelSchedule.SetSlotAddress(slotNumber, "Y");
                }

                //If no circuit for slot, set spare
                //If slot is locked, do not set address
                else
                {
                    bool testspare = panel.PanelSchedule.IsSlotSpare(slotNumber);
                    bool testsub = panel.PanelSchedule.IsSlotSubPole(slotNumber);

                    if((!panel.PanelSchedule.IsSlotSpare(slotNumber)) && (!panel.PanelSchedule.IsSlotSubPole(slotNumber)))
                    {
                        panel.PanelSchedule.SetSlotSpare(slotNumber); 
                        if (panel.System == System.PerfLighting)
                            panel.PanelSchedule.SetSlotAddress(slotNumber, i.ToString());
                        else if (panel.System == System.PerfSVC)
                            panel.PanelSchedule.SetSlotAddress(slotNumber, "Y");
                    }

                }

            }
            trans.Commit();

            return true;
        }
        private static bool TestCircuitPoles(DeviceConnector dc, int circuitNum, int maxCircuitNum)
        {
            bool success = false;
            int numPoles = dc.Poles;

            //CONFIRM BREAKER WILL NOT EXTEND BEYOND PANEL SPACE
            //CONFIRM BREAKER WILL WORK WITH PANEL LAYOUT
            //WILL NEED TO UPDATE TO ACCOUNT FOR SPECIFIC PANEL CONFIGURATIONS.
            switch (numPoles)
            {
                case 1:
                    success = true;
                    break;
                case 2:
                    if (maxCircuitNum - circuitNum > 1)
                        success = true;
                    else if (circuitNum % 4 == 1 || circuitNum % 4 == 2)
                        success = true;
                    else
                        TaskDialog.Show("Breaker Error", "Attempting to circuit 2-pole breaker to wrong space");
                    break;
                case 3:
                    if (maxCircuitNum - circuitNum > 3)
                        success = true;
                    else if (circuitNum % 6 == 1 || circuitNum % 6 == 2)
                        success = true;
                    else
                        TaskDialog.Show("Breaker Error", "Attempting to circuit 3-pole breaker to wrong space");
                    break;
                default:
                    TaskDialog.Show("Breaker Error", "Runtime Error Circuit Pole Test");
                    break;
            }
            return success;
        }
    }
}
