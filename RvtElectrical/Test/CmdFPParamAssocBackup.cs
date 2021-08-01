using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;


namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdFPParamAssoc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Create Dictionaries
            Dictionary<Int32, string> SystemCodeDict = new Dictionary<int, string>();
            SystemCodeDict.Add(1, "L");
            SystemCodeDict.Add(2, "A");
            SystemCodeDict.Add(3, "S");
            SystemCodeDict.Add(6, "C");
            SystemCodeDict.Add(7, "M");
            SystemCodeDict.Add(8, "F");
            SystemCodeDict.Add(9, "G");

            Dictionary<string, string> BoxClassificationDict = new Dictionary<string, string>();
            BoxClassificationDict.Add("Arch Button Station", "AB");
            BoxClassificationDict.Add("Arch Sensor", "AS");
            BoxClassificationDict.Add("Busway", "BW");
            BoxClassificationDict.Add("Control Box", "CB");
            BoxClassificationDict.Add("Connector Strip", "CS");
            BoxClassificationDict.Add("Power Box", "PB");
            BoxClassificationDict.Add("Portable", "PT");
            BoxClassificationDict.Add("Touch Screen", "TS");
            BoxClassificationDict.Add("Universal Box", "UB");

            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (!doc.IsFamilyDocument)
            {
                TaskDialog.Show("Document Error", "Command only availabe in Family Editor");
                return Result.Failed;
            }

            //Store Local Variables
            string famDeviceBox = "Device Box";
            string concatDeviceCode = "";

            //Get Elements in Family Builder

            //Create Filtered Element Collector & Filter for Electrical Fixtures
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementQuickFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);

            //Apply Filter
            IList<Element> devices = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            //Locate Device Box Family
            List<Element> deviceBoxes = new List<Element>();
            foreach(Element device in devices)
            {
                ElementId typeId = device.GetTypeId();
                ElementType type = doc.GetElement(typeId) as ElementType;
                if (type.FamilyName == famDeviceBox)
                    deviceBoxes.Add(device);
            }

            //Verify there is only one device box in the model
            if(deviceBoxes.Count() > 1)
            {
                TaskDialog.Show("Device Box Error", "Only one device box can be in the family.  Exiting");
                return Result.Failed;
            }
            else if(deviceBoxes.Count == 0)
            {
                TaskDialog.Show("Device Box Error", "No device box found.  Exiting");
                return Result.Failed;
            }
            Element deviceBox = deviceBoxes.First();


            //Locate the Connector Families
            IList<Element> deviceConnectors = devices
                .Where(d => d.get_Parameter(TCCElecSettings.BoxIdGuid) != null)
                .Where(d => d.get_Parameter(TCCElecSettings.DeviceIdGuid) != null)
                .ToList();

            //Filter out only connectors that can be associated using the BoxID parameter
            //(Eliminates embedded family connectors)
            List<Element> filteredDeviceConnectors = new List<Element>();
            foreach(Element deviceConnector in deviceConnectors)
            {
                Parameter boxIdParam = deviceConnector.get_Parameter(TCCElecSettings.BoxIdGuid);
                if (doc.FamilyManager.CanElementParameterBeAssociated(boxIdParam))
                    filteredDeviceConnectors.Add(deviceConnector);
            }

            deviceConnectors = filteredDeviceConnectors;

            using (Transaction trans = new Transaction(doc, "Associate Parameters"))
            {
                trans.Start();

                //DEVICE BOX PARAMETER MAPPING

                //Get all Parameters in Devicebox
                List<Parameter> deviceBoxParams = (from Parameter p in deviceBox.Parameters select p).ToList();

                //Get all Family Parameters in Family
                List<FamilyParameter> famParams = (from FamilyParameter fp in doc.FamilyManager.Parameters select fp).ToList();

                //Associate Device Box Parameters
                foreach(Parameter deviceBoxParam in deviceBoxParams)
                {
                    foreach(FamilyParameter famParam in famParams)
                    {
                        if (deviceBoxParam.Definition.Name == famParam.Definition.Name)
                            doc.FamilyManager.AssociateElementParameterToFamilyParameter(deviceBoxParam, famParam);
                    }
                }

                //DEVICE CONNECTOR PARAMETER MAPPING

                int i = 1;
                foreach(Element deviceConnector in deviceConnectors)
                {
                    //Get all Parameters in Devicebox
                    List<Parameter> deviceConnectorParams = (from Parameter p in deviceConnector.Parameters select p).ToList();

                    //Associate Device Connector Parameters
                    foreach (Parameter deviceConnectorParam in deviceConnectorParams)
                    {
                        foreach (FamilyParameter famParam in famParams)
                        {
                            if (deviceConnectorParam.Definition.Name == famParam.Definition.Name)
                            {
                                if(doc.FamilyManager.CanElementParameterBeAssociated(deviceConnectorParam))
                                    doc.FamilyManager.AssociateElementParameterToFamilyParameter(deviceConnectorParam, famParam);
                            }

                        }
                    }

                    Parameter connectPosParam = deviceConnector.get_Parameter(TCCElecSettings.ConnectorPositionGuid);
                    connectPosParam.Set(i);
                    i++;
                }



                //FAMILY CODE CREATION

                //Get System Code
                List<int> iDeviceCodesList = new List<int>();
                FamilyParameter famDeviceIdParam = doc.FamilyManager.get_Parameter(TCCElecSettings.DeviceIdGuid);
                
                //Iterate through types to get parameter value
                //Apparently no way to access forumla value directly
                foreach(FamilyType t in doc.FamilyManager.Types)
                {
                    int iTemp = Convert.ToInt32(t.AsInteger(famDeviceIdParam));
                    iDeviceCodesList.Add(iTemp);
                }
                
                //Error check that Device Code template is correct
                if(iDeviceCodesList.Distinct().Count() != 1)
                {
                    TaskDialog.Show("Device Code Error", "Error in family Device Code parameter.  Check Template File");
                    trans.Dispose();
                    return Result.Failed;
                }

                int iDeviceCode = iDeviceCodesList[0];
                string SystemCode = SystemCodeDict[iDeviceCode/10000000].ToString();


                //Get Backbox Code
                ElementId boxTypeId = deviceBox.GetTypeId();
                ElementType boxType = doc.GetElement(boxTypeId) as ElementType;
                string boxCode = boxType.get_Parameter(TCCElecSettings.BackboxCodeGuid).AsString();


                //Get connector device codes
                List<String> deviceCodes = new List<string>();
                List<DeviceId> deviceIds = new List<DeviceId>();
                List<int> connectorGroups = new List<int>();
                
                foreach(Element deviceConnector in deviceConnectors)
                {
                    //Get Element Type
                    ElementId eTypeId = deviceConnector.GetTypeId();
                    ElementType eType = doc.GetElement(eTypeId) as ElementType;

                    deviceCodes.Add(eType.get_Parameter(TCCElecSettings.ConnectorGroupCodeGuid).AsString());
                    var tempDeviceId = new DeviceId(eType.get_Parameter(TCCElecSettings.DeviceIdGuid).AsInteger());
                    deviceIds.Add(tempDeviceId);
                    connectorGroups.Add(tempDeviceId.ConnectorGroup);

                }

                //Get distinct device codes
                List<string> distinctDeviceCodes = deviceCodes.Distinct().ToList();
                List<string> sortedDistinctDeviceCodes = new List<String>();

                //Sort Distinct Codes
                distinctDeviceCodes.Sort();

                //Find if there are any power connectors
                //And re-sort to place them at the beginning
                foreach(string distinctDeviceCode in distinctDeviceCodes)
                {
                    if (distinctDeviceCode.Contains("X"))
                    {
                        sortedDistinctDeviceCodes.Add(distinctDeviceCode);
                    }
                }
                foreach (string distinctDeviceCode in distinctDeviceCodes)
                {
                    if (!distinctDeviceCode.Contains("X"))
                    {
                        sortedDistinctDeviceCodes.Add(distinctDeviceCode);
                    }
                }

                //Box Type Classification Code
                //Determine if this is a Power, Control, or Universal Box
                FamilyParameter BoxClassificationCodeParam = doc.FamilyManager.get_Parameter(TCCElecSettings.BoxClassificationCode);

                //Get Distinct Connector Groups
                List<int> distinctConnectorGroups = connectorGroups.Distinct().ToList();

                //Get Box Classification Code
                string boxClassificationCode = "";
                if(distinctDeviceCodes.Count != 1)
                    boxClassificationCode = "UB";
                else if(distinctConnectorGroups[0] % 10 == 9)
                    boxClassificationCode = "PB";
                else
                    boxClassificationCode = "CB";

                //Assign Box Classification Code
                doc.FamilyManager.SetFormula(BoxClassificationCodeParam, "\""+boxClassificationCode+"\"");

                //Create string of connectors & Box Code
                //System Code
                concatDeviceCode = SystemCode + "-" + boxClassificationCode + "-";

                foreach (string distinctDeviceCode in sortedDistinctDeviceCodes)
                {
                    int count = deviceCodes.Where(c => c.Equals(distinctDeviceCode))
                        .Select(c => c)
                        .Count();
                    concatDeviceCode = concatDeviceCode + distinctDeviceCode + count.ToString();
                }
                concatDeviceCode = concatDeviceCode + "-" + boxCode;
                concatDeviceCode = "\"" + concatDeviceCode + "\"";

                //Assign to family parameter
                FamilyParameter famNameCodeParam = doc.FamilyManager.get_Parameter(TCCElecSettings.PlateFamilyCodeGuid);
                doc.FamilyManager.SetFormula(famNameCodeParam, concatDeviceCode);



                //CREATE BOX CODE



                //TRANSFER BOX SIZE TO FAMILY
                FamilyParameter famBoxSizeParam = doc.FamilyManager.get_Parameter(TCCElecSettings.BackboxSizeGuid);
                string boxSize = boxType.get_Parameter(TCCElecSettings.BackboxSizeGuid).AsString();
                boxSize = "\"" + boxSize + "\"";
                doc.FamilyManager.SetFormula(famBoxSizeParam, boxSize);

                trans.Commit();
            }

            TaskDialog.Show("Command Succeeded", "Parameters Association Complete" + Environment.NewLine +
                "Family Device Code: " + concatDeviceCode);

            return Result.Succeeded;
        }

    }
}
