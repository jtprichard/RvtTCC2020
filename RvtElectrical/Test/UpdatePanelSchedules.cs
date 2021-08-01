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
    public class UpdatePanelSchedules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get GUID Settings from Setting Class
            Guid DeviceIdGuid = TCCElecSettings.DeviceIdGuid;

            //Get UIDocument & Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Extract lighting panels
            IList<DevicePanel> lightingPanels = DevicePanel.GetDevicePanels(doc, System.PerfLighting);

            //Test Panels for Errors
            DevicePanel.TestPanelsForErrors(lightingPanels);

            //Get Lighting Device Boxes
            IList<DeviceBox> lightingBoxes;
            var lightingQuery = from lightingBox in lightingBoxes = DeviceBox.GetDeviceBoxes(doc)
                         where lightingBox.DeviceId.IsLightingBox() == true
                         select lightingBox;
            lightingBoxes = lightingQuery.Cast<DeviceBox>().ToList();

            //Filter by Venue
            lightingBoxes = DeviceBox.FilterBoxByVenue(lightingBoxes, "Theatre");

            //Filter for Power Connectors
            var powerConnectors = DeviceConnector.GetPowerConnectorsByBox(lightingBoxes);

            //Get UnPowered Power Connector Circuits
            List<ElectricalCircuitData> powerCircuitData = new List<ElectricalCircuitData>();

            foreach (var powerConnector in powerConnectors)
            {
                if (!powerConnector.CircuitData.HasCircuit)
                {
                    powerCircuitData.Add(powerConnector.CircuitData);
                }
            }

            //Create Power Circuits
            if (powerCircuitData.Count() > 0)
            {
                foreach (ElectricalCircuitData powerCircuitItem in powerCircuitData)
                {
                    powerCircuitItem.CreatePowerCircuit();
                }
            }
            else
                TaskDialog.Show("Circuiting Error", "No available circuits to power");

            //Choose Lighting Panel
            DevicePanel selectedPanel = null;
            foreach(DevicePanel lightingPanel in lightingPanels)
            {
                if(lightingPanel.AddressStart == 1)
                {
                    selectedPanel = lightingPanel;
                }     
            }

            Element selectedPanelElement = selectedPanel.PanelElement;
            //PanelScheduleData psData = new PanelScheduleData(doc, selectedPanel);

            //Get Uncircuited Power Circuits
            powerCircuitData.Clear();
            //powerCircuits.Clear();
            foreach (var powerConnector in powerConnectors)
            {
                if (!powerConnector.CircuitData.HasPanel)
                {
                    powerCircuitData.Add(powerConnector.CircuitData);
                }
            }

            ////Circuit to Panel
            //foreach (var powerCircuitItem in powerCircuitData)
            //{
            //    powerCircuitItem.SelectPanel(selectedPanelElement);
            //}

            for (int i=1; i<selectedPanel.AddressEnd; i++)
            {
                int numCircuited = 0;
                foreach(var powerConnector in powerConnectors)
                {
                    if(powerConnector.ConnectorCircuit == i)
                    {
                        if(numCircuited == 0)
                        {
                            powerConnector.CircuitData.SelectPanel(selectedPanelElement);
                            numCircuited++;
                        }
                        else if(numCircuited > 0)
                        {
                            powerConnector.CircuitData.AddElementToCircuit(selectedPanelElement);
                            numCircuited++;
                        }
                        else
                        {
                            //NEED TO FIX THIS
                            //selectedPanel.PanelSchedule.PanelSchedule.AddSpare(3, 3);
                            
                        }
                    }
                }
            }



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


            //TESTING TO FIGURE THIS FUCKING THING OUT!!!!!

            //try
            //{
            //    Transaction transaction = new Transaction(doc, "Circuiting Test");
            //    transaction.Start();

            //    //Electrical System
            //    ElectricalSystem es;
            //    FamilyInstance fip = selectedPanelElement as FamilyInstance;

            //    //Power Circuit Element
            //    Element testConnectorElement = powerConnectors[0].Connector;
            //    DeviceConnector testDeviceConnector = powerConnectors[0];

            //    FamilyInstance fi = testConnectorElement as FamilyInstance;
            //    ConnectorSet cs = fi.MEPModel.ConnectorManager.Connectors;
            //    foreach (Connector c in cs)
            //    {
            //        es = ElectricalSystem.Create(c, ElectricalSystemType.PowerCircuit);
            //        es.SelectPanel(fip);
            //    }

            //    transaction.Commit();
            //}

            //catch
            //{
            //    TaskDialog.Show("Error", "Test Failed");
            //}



            //Filter by Start and End Address


            //List<FamilyInstance> list = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Where(a => a.get_Parameter(DeviceIdGuid).AsInteger() > 100000).Cast<FamilyInstance>().ToList();


            ////Create List to capture Boxes and distinct Box numbers
            //IList<int> boxNumbers = new List<int>();
            //foreach(Element device in devices)
            //{
            //    boxNumbers.Add(device.get_Parameter(BoxIdGuid).AsInteger());
            //}
            //boxNumbers.Distinct();

            ////Transaction to update circuits for each discrete box number
            //using (Transaction trans = new Transaction(doc, "Update User Circuits"))
            //{
            //    trans.Start();
            //    foreach(int targetBoxNumber in boxNumbers)
            //    {
            //        //Retrieve device plate and connectors for a specific box number
            //        IList<Element> box = ElecUtils.CollectDeviceByBoxNumber(devices, targetBoxNumber, BoxIdGuid);

            //        //Create a new DeviceBox 
            //        DeviceBox deviceBox = new DeviceBox(doc, box);

            //        //Update the Concatenated circuit parameter with a string of circuits
            //        deviceBox.UpdateDeviceBoxConcat();
            //    }

            //    trans.Commit();
            //}

            return Result.Succeeded;
        }
    }
}
