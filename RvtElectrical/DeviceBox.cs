using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtElectrical
{
    //ON REFACTORING, WE NEED TO THINK ABOUT STORING THE MAIN DEVICE BOX SEPARATE FROM ALL ELEMENTS
    //PROVIDE MEANS OF ACCESSING DEVICEBOX FAMILY OTHER LABEL PARAMETER SO THAT THE CONCAT CAN BE UPDATED
    public class DeviceBox
    {
        public IList<Element> Box { get; private set; }                     //Stores element reference in DeviceBox
        public IList<DeviceConnector> Connectors { get; private set; }      //List of Connector Elements
        public int BoxId { get; private set; }                              //Box ID Integer
        public DeviceId DeviceId { get; private set; }                      //Device ID Value
        public System System { get; private set; }                          //Device Box System
        public string Venue { get; private set; }                           //Venue Value
        public string PlateCode { get; private set; }                       //Plate Code
        public string Mount { get; private set; }                           //Mounting Condition

        //public DeviceBox(Document doc, IList<Element> elements)
        ////OLD CONSTRUCTOR - NO LONGER USED
        //{
        //    // Get GUID Values for Shared Parameters
        //    Guid boxIdGuid = TCCElecSettings.BoxIdGuid;
        //    Guid venueIdGuid = TCCElecSettings.VenueGuid;

        //    //Assign Elements to Box Property
        //    Box = elements;

        //    this.Connectors = new List<DeviceConnector>();

        //    try
        //    {
        //        //Find Faceplate Element
        //        foreach (Element ele in elements)
        //        {
        //            DeviceId deviceId = new DeviceId(DeviceId.GetDeviceId(ele, doc));

        //            if (deviceId.IsFaceplate())
        //            {
        //                DeviceId = deviceId;
        //                System = deviceId.System;
        //                BoxId = ele.get_Parameter(boxIdGuid).AsInteger();
        //                Venue = ele.get_Parameter(venueIdGuid).AsString();
        //            }

        //            //If not the plate, add connectors to the Connector list
        //            else
        //            {
        //                Connectors.Add(new DeviceConnector(ele, Venue, doc));

        //            }
        //        }
        //        //If no faceplate is found, return exception
        //        if (DeviceId == null)
        //        {
        //            Exception ex = new Exception("No Plate ID Passed");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public DeviceBox(Document doc, Element ele)
        //Constructor
        {
            // Get GUID Values for Shared Parameters
            Guid boxIdGuid = TCCElecSettings.BoxIdGuid;
            Guid venueIdGuid = TCCElecSettings.VenueGuid;

            this.Connectors = new List<DeviceConnector>();

            try
            {
                IList<Element> boxElements = new List<Element>();
                
                //Test that this is a devicebox
                if (!IsDeviceBox(ele, doc))
                    throw new Exception();

                //Assign Faceplate Element
                DeviceId deviceId = new DeviceId(DeviceId.GetDeviceId(ele, doc));
                if (deviceId.IsFaceplate())
                {
                    DeviceId = deviceId;
                    System = deviceId.System;
                    
                    //Instance Parameters
                    BoxId = ele.get_Parameter(boxIdGuid).AsInteger();
                    Venue = ele.get_Parameter(venueIdGuid).AsString();

                    //Type Parameters
                    ElementType etype = doc.GetElement(ele.GetTypeId()) as ElementType;
                    PlateCode = etype.get_Parameter(TCCElecSettings.PlateCodeGuid).AsString();
                    Mount = etype.get_Parameter(TCCElecSettings.MountConditionGuid).AsString();

                    //Add main device element to box parameters
                    boxElements.Add(ele);
                }

                //Get SubComponents
                FamilyInstance eleFam = ele as FamilyInstance;
                IList<ElementId> subComponentIds = (IList<ElementId>)eleFam.GetSubComponentIds();
                if (subComponentIds.Count() > 0)
                {
                    //Get elements from elementIds
                    IList<Element> subComponents = new List<Element>();
                    foreach(ElementId eid in subComponentIds)
                    {
                        subComponents.Add(doc.GetElement(eid));
                    }

                    foreach(Element subcomponent in subComponents)
                    {
                        if (null != subcomponent.get_Parameter(boxIdGuid))
                        {
                            //Add elements as connectors
                            Connectors.Add(new DeviceConnector(subcomponent, Venue, doc));

                            //Add elements to Box Parameter list of elements
                            boxElements.Add(subcomponent);
                        }

                        //Get Subconnectors if they exist
                        FamilyInstance subEleFam = subcomponent as FamilyInstance;
                        IList<ElementId> subConnectorIds = (IList<ElementId>)subEleFam.GetSubComponentIds();
                        if (subConnectorIds.Count() > 0)
                        {
                            IList<Element> subConnectors = new List<Element>();
                            foreach (ElementId seid in subConnectorIds)
                            {
                                subConnectors.Add(doc.GetElement(seid));
                            }
                            foreach (Element subConnector in subConnectors)
                            {
                                if (null != subConnector.get_Parameter(boxIdGuid))
                                {
                                    //Add elements as connectors
                                    Connectors.Add(new DeviceConnector(subConnector, Venue, doc));

                                    //Add elements to Box Parameter list of elements
                                    boxElements.Add(subConnector);
                                }
                            }
                        }
                    }
                    //Assign elements to Box Parameter for element access
                    Box = boxElements;
                }

                //If no faceplate is found, return exception
                if (DeviceId == null)
                {
                    Exception ex = new Exception("No Plate ID Passed");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void UpdateDeviceBoxConcat()
        // Filters through a Device Box's connectors to consolidate connector circuits
        {
            List<int> uniqueDeviceIds = new List<int>();
            //Find the unique connector types to combine.
            //This is based on the specific Device ID value
            //Note that this will only combine specific connectors, not necessarily signal types

            //Identify if any devices are multi-connectors
            //If they are, change their Device ID to match the first connector they contain
            foreach (DeviceConnector connector in Connectors)
            {
                if (connector.DeviceId.IsMultConnector())
                {
                    connector.ChangeConnectorId(DeviceConnector.GetMultConnectorSubId(Connectors, connector.ConnectorPosition));
                }
            }

            //Locate the unique connector IDs in the box
            foreach (DeviceConnector connector in Connectors)
            {
                uniqueDeviceIds.Add(connector.DeviceId.Value);
            }

            uniqueDeviceIds = uniqueDeviceIds.Distinct().ToList();

            //Iterate through the unique Ids and create list of circuits of the same connector ID 
            foreach(int uniqueDeviceId in uniqueDeviceIds)
            {

                //Get all connectors with the unique deviceid
                var tempConnectors = new List<DeviceConnector>();
                foreach (DeviceConnector connector in Connectors)
                {
                    if (connector.DeviceId.Value == uniqueDeviceId)
                    {
                        tempConnectors.Add(connector);
                    }
                }


                if (tempConnectors.Count > 0)
                {
                    //Find if connectors have unique prefixes
                    var uniquePrefixes = new List<string>();
                    foreach (DeviceConnector connector in tempConnectors)
                    {
                        uniquePrefixes.Add(connector.ConnectorLabelPrefix); 

                    }
                    uniquePrefixes = uniquePrefixes.Distinct().ToList();

                    //Create concat label for each connector with a unique prefix
                    foreach(string prefix in uniquePrefixes)
                    {
                        var connectorCircuits = new List<int>();
                        string concatConnector = "";
                        foreach (DeviceConnector connector in tempConnectors)
                        {
                            if(connector.ConnectorLabelPrefix == prefix)
                            {
                                if(connector.ConnectorCircuit != 0)
                                    connectorCircuits.Add(connector.ConnectorCircuit);
                            }

                        }
                        if(connectorCircuits.Count()>0)
                            concatConnector = ElecUtils.CircuitListToString(connectorCircuits, prefix);

                        foreach (DeviceConnector connector in tempConnectors)
                        {
                            if (connector.ConnectorLabelPrefix == prefix)
                            {
                                if(connector.ConnectorLabelOther == null || connector.ConnectorLabelOther == "")
                                {
                                    connector.UpdateConnectorConcat(concatConnector);
                                }
                                else
                                    connector.UpdateConnectorConcat(connector.ConnectorLabelOther);
                            }

                        }
                    }
                }
            }
            return;
        }
        
        public static IList<DeviceBox> GetDeviceBoxes(Document doc)
        {
            var deviceBoxes = FilterAllDeviceBoxes(doc);
            return deviceBoxes;
        }

        public static IList<DeviceBox> GetDeviceBoxes(Document doc, System deviceScope)
        //Get Device Boxes based on devices having a specific system scope
        {
            //Get all deviceboxes in model
            var deviceBoxes = FilterAllDeviceBoxes(doc);

            //If a device box contains a connector of the selected system
            //then change the scope of that devicebox to match the system
            deviceBoxes = ChangeDeviceBoxToConnectorScope(deviceBoxes, deviceScope);

            //Use Linq query to filter by System
            List<DeviceBox> filteredDeviceBoxes = deviceBoxes
                .Where(db => db.System == deviceScope)
                .ToList();

            return filteredDeviceBoxes;
        }
        
        private static IList<DeviceBox> ChangeDeviceBoxToConnectorScope(IList<DeviceBox> deviceBoxes, System deviceScope)
        {
            var updatedDeviceBoxes = new List<DeviceBox>();

            foreach(var deviceBox in deviceBoxes)
            {
                IList<DeviceConnector> connectors = deviceBox.Connectors
                    .Where(dc => dc.System == deviceScope)
                    .ToList();

                if(deviceBox.System != deviceScope && connectors.Count > 0)
                {
                    deviceBox.System = deviceScope;
                }
                updatedDeviceBoxes.Add(deviceBox);
            }
            return updatedDeviceBoxes;
        }

        private static IList<DeviceBox> FilterAllDeviceBoxes(Document doc)
        //Get all device boxes in model
        {
            //Get BoxIdGuid
            Guid boxIdGuid = TCCElecSettings.BoxIdGuid;
            Guid deviceIdGuid = TCCElecSettings.DeviceIdGuid;
            
            //Create Filtered Element Collector & Filter for Electrical Fixtures
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementQuickFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);

            //Apply Filter
            IList<Element> devices = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();
            IList<Element> filteredDevices = devices
                .Where(d => d.get_Parameter(boxIdGuid) != null)
                .Where(d => d.get_Parameter(deviceIdGuid) != null)
                //.Where(d => (d as FamilyInstance).SuperComponent == null)
                .Where(d => IsDeviceBox(d, doc))
                .ToList();

            List<DeviceBox> deviceBoxes = new List<DeviceBox>();
            foreach (Element device in filteredDevices)
            {
                deviceBoxes.Add(new DeviceBox(doc, device));
            }

            return deviceBoxes;
        }

        //OLD VERSION - SEE ABOVE
        //private static IList<DeviceBox> FilterAllDeviceBoxes(Document doc)
        ////Get all device boxes in model
        //{
        //    //Get BoxIdGuid
        //    Guid boxIdGuid = TCCElecSettings.BoxIdGuid;
        //    Guid deviceIdGuid = TCCElecSettings.DeviceIdGuid;

        //    //Create Filtered Element Collector & Filter for Electrical Fixtures
        //    FilteredElementCollector collector = new FilteredElementCollector(doc);
        //    ElementQuickFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);

        //    //Apply Filter
        //    IList<Element> devices = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();
        //    IList<Element> filteredDevices = devices
        //        .Where(d => d.get_Parameter(boxIdGuid) != null)
        //        .Where(d => d.get_Parameter(deviceIdGuid) != null)
        //        .ToList();

        //    //Create List to capture Boxes and distinct Box numbers
        //    IList<int> boxNumbers = new List<int>();
        //    foreach (Element device in filteredDevices)
        //    {
        //        boxNumbers.Add(device.get_Parameter(boxIdGuid).AsInteger());
        //    }
        //    boxNumbers = boxNumbers.Distinct().ToList();

        //    List<DeviceBox> deviceBoxes = new List<DeviceBox>();
        //    foreach (int targetBoxNumber in boxNumbers)
        //    {
        //        //Retrieve device plate and connectors for a specific box number
        //        IList<Element> box = ElecUtils.CollectDeviceByBoxNumber(filteredDevices, targetBoxNumber);

        //        //Create a new DeviceBox
        //        deviceBoxes.Add(new DeviceBox(doc, box));
        //    }
        //    return deviceBoxes;
        //}
        public static IList<DeviceBox> FilterBoxByVenue(IList<DeviceBox> deviceBoxes, string venue)
        {
            IList<DeviceBox> venueBoxes = new List<DeviceBox>();
            foreach (DeviceBox deviceBox in deviceBoxes)
            {
                if(deviceBox.Venue == venue)
                    venueBoxes.Add(deviceBox);
            }

            return venueBoxes;
        }

        public static IList<DeviceBox> GetDuplicateDeviceBoxes(Document doc, int boxNumber)
        //Provides a list of deviceboxes based on the box number.
        {
                  
            IList<DeviceBox> deviceBoxes = GetDeviceBoxes(doc);

            IList<DeviceBox> filteredDB = deviceBoxes
                .Where(d => d.BoxId == boxNumber)
                .ToList();

            return filteredDB;
        }

        public static IList<DeviceBox> GetDuplicateDeviceBoxes(Document doc)
        //Searches the project and returns DeviceBoxes where there are duplicates in the project.
        {
            IList<DeviceBox> deviceBoxes = GetDeviceBoxes(doc);
            IList<DeviceBox> duplicateDeviceBoxes = new List<DeviceBox>();

            foreach(DeviceBox db in deviceBoxes)
            {
                IList<DeviceBox> filteredDB = deviceBoxes
                    .Where(d => d.BoxId == db.BoxId)
                    .ToList();
                if (filteredDB.Count > 1)
                    foreach(DeviceBox fdb in filteredDB)
                    {
                        duplicateDeviceBoxes.Add(fdb);
                    }
            }
            return duplicateDeviceBoxes;

        }

        public static bool IsDeviceBox(Element ele, Document doc)
        //Receives an element and determins whether it is a DeviceBox capable family
        {
            bool isDB = false;

            //Determine if the element is a family instance
            FamilyInstance fi;
            if (!(ele is FamilyInstance))
                return isDB;

            //Get DeviceID and test if it is a devicebox
            try
            {
                DeviceId deviceId = new DeviceId(DeviceId.GetDeviceId(ele, doc));
                if (deviceId.IsDeviceBox())
                    isDB = true;
            }
            catch
            {
            }

            return isDB;
        }

        public static int NextDeviceBoxNumber(int boxNumber, Document doc)
        //Returns the next available devicebox in a document
        //Returns the original box number if it is unused
        {
            int nextBox = boxNumber;
            bool found = true;

            //Get all device boxes
            IList<DeviceBox> deviceBoxes = GetDeviceBoxes(doc);

            do
            {
                IList<DeviceBox> filteredDeviceBoxes = deviceBoxes
                    .Where(d => d.BoxId == nextBox)
                    .ToList();

                //If no devicebox with this number exists, return the box number
                if (filteredDeviceBoxes.Count() == 0)
                    found = true;
                else
                {
                    found = false;
                    nextBox++;
                }
            } while (found == false);

            return nextBox;
        }

    }

}
