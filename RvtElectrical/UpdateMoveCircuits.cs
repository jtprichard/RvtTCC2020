using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Electrical;


namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class UpdateMoveCircuits : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            ////Get GUID Settings from Setting Class
            Guid DeviceIdGuid = TCCElecSettings.DeviceIdGuid;

            //Get UIDocument & Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Extract lighting panels
            IList<DevicePanel> lightingPanels = DevicePanel.GetDevicePanels(doc, System.PerfLighting);

            //Test Panels for Errors
            DevicePanel.TestPanelsForErrors(lightingPanels);

            //Choose Lighting Panel
            DevicePanel selectedPanel = null;
            foreach (DevicePanel lightingPanel in lightingPanels)
            {
                if (lightingPanel.AddressStart == 1)
                {
                    selectedPanel = lightingPanel;
                }

            }

            Element selectedPanelElement = selectedPanel.PanelElement;

            selectedPanel.PanelSchedule.SetSlotSpare(24);
            selectedPanel.PanelSchedule.SetSlotSpare(11);

            selectedPanel.PanelSchedule.SetSlotDescription(24, "Test Description 24");
            selectedPanel.PanelSchedule.SetSlotAddress(24, "DMX 24");

            selectedPanel.PanelSchedule.SetSlotDescription(5, "Test Description 5");
            selectedPanel.PanelSchedule.SetSlotAddress(5, "DMX 5");

            selectedPanel.PanelSchedule.MoveSlot(1, 13);

            ////Get Lighting Device Boxes
            //IList<DeviceBox> lightingBoxes;
            //var lightingQuery = from lightingBox in lightingBoxes = DeviceBox.GetDeviceBoxes(doc)
            //             where lightingBox.DeviceId.IsLightingBox() == true
            //             select lightingBox;
            //lightingBoxes = lightingQuery.Cast<DeviceBox>().ToList();

            ////Filter by Venue
            //lightingBoxes = DeviceBox.FilterBoxByVenue(lightingBoxes, "Theatre");

            ////Filter for Power Connectors
            //var powerConnectors = DeviceConnector.GetPowerConnectorsByBox(lightingBoxes);

            ////Get UnPowered Power Connector Circuits
            //List<Element> powerCircuits = new List<Element>();
            //List<ElectricalCircuitData> powerCircuitData = new List<ElectricalCircuitData>();

            //foreach (var powerConnector in powerConnectors)
            //{
            //    if (!powerConnector.CircuitData.HasCircuit)
            //    {
            //        powerCircuits.Add(powerConnector.Connector);
            //        powerCircuitData.Add(powerConnector.CircuitData);
            //    }
            //}

            ////Create Power Circuits
            //if (powerCircuits.Count() > 0)
            //{
            //    foreach (ElectricalCircuitData powerCircuitDatum in powerCircuitData)
            //    {
            //        powerCircuitDatum.CreatePowerCircuit();
            //    }
            //}
            //else
            //    TaskDialog.Show("Circuiting Error", "No available circuits to power");

            ////Get Uncircuited Power Circuits
            //powerCircuitData.Clear();
            //powerCircuits.Clear();
            //foreach (var powerConnector in powerConnectors)
            //{
            //    if (!powerConnector.CircuitData.HasPanel)
            //    {
            //        powerCircuits.Add(powerConnector.Connector);
            //        powerCircuitData.Add(powerConnector.CircuitData);
            //    }
            //}

            ////Circuit first item to Panel
            //powerCircuitData[0].SelectPanel(selectedPanelElement);

            ////get remaining circuits
            //ElementSet circuitElements = new ElementSet();
            //for (int i = 1; i < powerCircuitData.Count(); i++)
            //{
            //    circuitElements.Insert(powerCircuitData[i].CircuitElement);
            //}

            ////Append circuits to first circuit in list
            //powerCircuitData[0].AddElementToCircuit(circuitElements);



            return Result.Succeeded;
        }
    }
}
