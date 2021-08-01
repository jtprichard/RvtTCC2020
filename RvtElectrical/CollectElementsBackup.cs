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
    public class CollectElementsBackup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get GUID Settings from Setting Class
            TCCElecSettings settings = new TCCElecSettings();
            Guid BoxIdGuid = settings.BoxIdGuid;
            Guid DeviceIdGuid = settings.DeviceIdGuid;
            Guid ConnectorCircuitGuid = settings.ConnectorCircuitConcatGuid;
            Guid ConnectorCircuitConcatGuid = settings.ConnectorCircuitConcatGuid;

            //Get UIDocument & Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Create Filtered Element Collector & Filter for Electrical Fixtures
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementQuickFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);

            //Apply Filter
            IList<Element> devices = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            //Create List to capture Boxes and Box Numbers for them
            List<Element> boxes = new List<Element>();
            List<int> boxNumbers = new List<int>();

            //Collect all Boxes
            foreach(Element ele in devices)
            {
                //TCC_DEVICE_ID is a Type parameter, so must get element type for each element
                ElementType etype = doc.GetElement(ele.GetTypeId()) as ElementType;

                //Lookup to see if it is aTCC Device
                Parameter param = etype.get_Parameter(DeviceIdGuid);

                //Find the Box ID to add to list
                Parameter param2 = ele.get_Parameter(BoxIdGuid);

                if (param != null && ElecUtils.IsDeviceBox(param.AsInteger()))
                {
                    boxes.Add(ele);
                    boxNumbers.Add(param2.AsInteger());

                //    TaskDialog.Show("Parameter Values", string.Format("param Value Type:  {0}" + Environment.NewLine + "param Value: {1}" + Environment.NewLine + "Box ID: {2}",
                //        param.StorageType.ToString(),
                //        param.AsInteger(),
                //        param2.AsInteger()));
                }
            }

            //Get the unique box numbers removing duplicates
            var uniqueBoxNumbers = boxNumbers.Distinct().ToList();
            uniqueBoxNumbers.Sort();

            foreach (int boxNumber in uniqueBoxNumbers)
            {

                List<Element> boxDevices = new List<Element>();
                List<int> connectorIds = new List<int>();

                //Get all connectors in a box number into a list
                foreach (Element ele in boxes)
                {
                    //Parameter param = ele.LookupParameter("TCC_BOX_ID");
                    Parameter param = ele.get_Parameter(BoxIdGuid);

                    if (param.AsInteger() == boxNumber)
                    {
                        Element deviceType = doc.GetElement(ele.GetTypeId()) as ElementType;
                        Parameter deviceParam = deviceType.get_Parameter(DeviceIdGuid);
                        //Parameter deviceParam = ele.get_Parameter(DeviceIdGuid);

                        if (deviceParam != null)
                        {
                            connectorIds.Add((deviceParam.AsInteger()));
                            boxDevices.Add(ele);
                        }
                    }
                }

                var uniqueConnectorIds = connectorIds.Distinct().ToList();
                uniqueConnectorIds.Sort();

                foreach (int connectorId in uniqueConnectorIds)
                {
                    List<string> circuit = new List<String>();
                    foreach (Element boxDevice in boxDevices)
                    {
                        Parameter deviceParam1 = boxDevice.get_Parameter(DeviceIdGuid);
                        Parameter deviceParam2 = boxDevice.get_Parameter(ConnectorCircuitGuid);
                        if (connectorId == (deviceParam1.AsInteger()/10))
                        {
                            circuit.Add(deviceParam2.AsInteger().ToString());
                        }
                    }
                    string circuits = string.Join<string>(", ", circuit);

                    //foreach (Element boxDevice in boxDevices)
                    //{
                    //    Parameter deviceParam = boxDevice.LookupParameter("TC_CONNECTOR_CIRCUITS_CONCAT");
                    //    Parameter deviceParam3 = boxDevice.LookupParameter("TC_BOX_ID");
                    //    Parameter deviceParam4 = boxDevice.LookupParameter("TC_CONNECTOR_ID");

                    //    if(connectorId == (deviceParam4.AsInteger()/10))
                    //    {
                    //        TaskDialog.Show("Test Parameter Information", string.Format("CONCAT Parameter Value: {0}" + Environment.NewLine +
                    //        "Circuits String: {1}" + Environment.NewLine +
                    //        "Box ID: {2}" + Environment.NewLine +
                    //        "Connector ID: {3}",
                    //        deviceParam.AsString(),
                    //        circuits.ToString(),
                    //        deviceParam3.AsInteger(),
                    //        deviceParam4.AsInteger()));
                    //    }

                    //}

                    foreach (Element boxDevice in boxDevices)
                    {
                        Parameter deviceParam = boxDevice.get_Parameter(ConnectorCircuitGuid);

                        if (connectorId == (deviceParam.AsInteger() / 10))
                        {
                            using (Transaction trans = new Transaction(doc, "Set Circuit Parameter"))
                            {
                                trans.Start();

                                Parameter deviceParam2 = boxDevice.get_Parameter(ConnectorCircuitConcatGuid);
                                deviceParam2.Set(circuits);

                                trans.Commit();
                            }
                        }

                    }
                }

            }

            TaskDialog.Show("Lighting Plates", string.Format("{0} Lighting Plates Counted" + Environment.NewLine + "{1} Box Numbers Counted",
                boxes.Count(),
                uniqueBoxNumbers.Count()));

            return Result.Succeeded;
        }
    }
}
