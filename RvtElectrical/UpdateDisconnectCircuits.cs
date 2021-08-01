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
    public class UpdateDisconnectCircuits : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            ////Get GUID Settings from Setting Class
            Guid DeviceIdGuid = TCCElecSettings.DeviceIdGuid;

            //Get UIDocument & Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Find Out if Panel Schedule View Active and Get Panel Schedule
            if (!(doc.ActiveView.ViewType == ViewType.PanelSchedule))
            {
                TaskDialog.Show("View Error", "Please Select a Panel Schedule View");
                return Result.Failed;
            }

            //Get panelscheduleview
            PanelScheduleView panelSchedule = doc.ActiveView as PanelScheduleView;

            //Create DevicePanel from PanelScheduleView
            DevicePanel panel = DevicePanelScheduleData.GetDevicePanelFromPanelSchedule(panelSchedule, doc);
            Element panelElement = panel.PanelElement;

            //Get other panels of same system to test for errors
            var devicePanels = DevicePanel.GetDevicePanels(doc, panel.System);

            //Filter for panels in same venue
            devicePanels = devicePanels.Where(dp => dp.Venue == panel.Venue)
                .ToList();

            //Test Panels for Errors
            DevicePanel.TestPanelsForErrors(devicePanels);

            //Get Lighting Device Boxes
            IList<DeviceBox> deviceBoxes = DeviceBox.GetDeviceBoxes(doc, panel.System);

            //Filter by Venue
            deviceBoxes = DeviceBox.FilterBoxByVenue(deviceBoxes, panel.Venue);

            //Filter for Power Connectors
            var powerConnectors = DeviceConnector.GetConnectorsByBox(deviceBoxes);

            //Get Used Power Connector Circuits
            List<Element> powerCircuits = new List<Element>();
            List<ElectricalCircuitData> powerCircuitData = new List<ElectricalCircuitData>();

            foreach (var powerConnector in powerConnectors)
            {
                if (powerConnector.CircuitData.HasPanel)
                {
                    powerCircuits.Add(powerConnector.Connector);
                    powerCircuitData.Add(powerConnector.CircuitData);
                }
            }

            //Disconnect Power Circuits
            if (powerCircuits.Count() > 0)
            {
                foreach (ElectricalCircuitData powerCircuitDatum in powerCircuitData)
                {
                    //powerCircuitDatum.DisconnectPanel();
                    powerCircuitDatum.RemoveCircuit();
                }
            }
            else
                TaskDialog.Show("Circuiting Error", "No Powered Circuits");


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
