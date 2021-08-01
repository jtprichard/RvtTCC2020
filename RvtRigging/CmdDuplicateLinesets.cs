using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RvtRigging
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdDuplicateLinesets : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;
            Document doc = uidoc.Document;
            
            //STATIC VARIABLES FOR TESTING
            string rp = "Performance_Rigging";
            string riQtyStr = "Lineset_Qty";
            string riSpaceStr = "Lineset_Spacing";

            if (doc.IsFamilyDocument)
            {
                TaskDialog.Show("Document Error", "Command only available in Model");
                return Result.Failed;
            }

            int lsNumber = 0;
            double lsSpacing = 0;

            if (RibbonUtils.GetRibbonItem(rp, riQtyStr,uiapp,out RibbonItem riQty))
            {
                TextBox tb = riQty as TextBox;
                lsNumber = int.Parse(tb.Value.ToString());
            }
            else
            {
                TaskDialog.Show("Lineset Error", "Provide a quantity of linesets in the associated text box");
                return Result.Failed;
            }

            if (RibbonUtils.GetRibbonItem(rp, riSpaceStr, uiapp, out RibbonItem riSpace))
            {
                Units units = doc.GetUnits();
                UnitType uType = UnitType.UT_Length;
                TextBox tb = riSpace as TextBox;

                if (UnitFormatUtils.TryParse(units, uType, tb.Value.ToString(), out double dResult))
                    lsSpacing = dResult;
                else
                {
                    TaskDialog.Show("Error", "Code Error - Contact Programmer!");
                    return Result.Failed;
                }

            }
            else
            {
                TaskDialog.Show("Lineset Error", "Provide a quantity of linesets in the associated text box");
                return Result.Failed;
            }



            Lineset lsObject = null;

            //Define a reference Object to accept the pick result
            Reference pickedObj = null;

            //Pick an object
            Selection sel = uiapp.ActiveUIDocument.Selection;
            SelectionFilterLineset selFilter = new SelectionFilterLineset();

            pickedObj = sel.PickObject(ObjectType.Element, selFilter, "Select a Lineset");
            Element elem = doc.GetElement(pickedObj);

            if (pickedObj != null)
            {
                // Retreive Element
                ElementId eleId = pickedObj.ElementId;
                Element ele = doc.GetElement(eleId) as Element;
                ElementId typeId = ele.GetTypeId();
                ElementType type = doc.GetElement(typeId) as ElementType;

                if (type.get_Parameter(TCCRiggingSettings.IsLinesetGuid) != null)
                    lsObject = new Lineset(doc, ele);
                else
                {
                    TaskDialog.Show("Selection Error", "Please select a lineset to duplicate");
                    return Result.Failed;
                }
            }
            else
            {
                TaskDialog.Show("Selection Error", "Please select a lineset to duplicate");
                return Result.Failed;
            }

            double lsStartDist = lsObject.Distance;
            int lsStartNum = lsObject.Number;


            //Create Filtered Element Collector & Filter for Electrical Fixtures
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementQuickFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_SpecialityEquipment);

            //Apply Filter
            IList<Element> eles = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

            //Locate all existing the Lineset Families  
            IList<Lineset> linesets = new List<Lineset>();
            foreach (Element ele in eles)
            {
                ElementId typeId = ele.GetTypeId();
                ElementType type = doc.GetElement(typeId) as ElementType;
                if (type.get_Parameter(TCCRiggingSettings.IsLinesetGuid) != null)
                    if(type.get_Parameter(TCCRiggingSettings.IsLinesetGuid).AsInteger() == 1)
                        linesets.Add(new Lineset(doc, ele));
            }


            using (Transaction trans = new Transaction(doc, "Duplicate Lineset"))
            {
                trans.Start();

                IList<ElementId> lsElementIds = new List<ElementId>();

                XYZ xyzTranslate = new XYZ(0, 0, 0);
                for (int i = 1; i <= lsNumber; i++)
                {
                    ICollection<ElementId> tempElems = (ElementTransformUtils.CopyElement(doc, pickedObj.ElementId, xyzTranslate));
                    Element e = doc.GetElement(tempElems.First());
                    Parameter paramDist = e.get_Parameter(TCCRiggingSettings.LinesetDistanceGuid);
                    paramDist.Set(lsStartDist + (lsSpacing * i));

                    Parameter paramNum = e.get_Parameter(TCCRiggingSettings.LinesetNumberGuid);
                    paramNum.Set(Convert.ToString(lsStartNum + i));

                }

                trans.Commit();
            }

                ////Locate Device Box Family
                //List<Element> deviceBoxes = new List<Element>();
                //foreach(Element device in devices)
                //{
                //    ElementId typeId = device.GetTypeId();
                //    ElementType type = doc.GetElement(typeId) as ElementType;
                //    if (type.FamilyName == famDeviceBox)
                //        deviceBoxes.Add(device);
                //}

                ////Verify there is only one device box in the model
                //if(deviceBoxes.Count() > 1)
                //{
                //    TaskDialog.Show("Device Box Error", "Only one device box can be in the family.  Exiting");
                //    return Result.Failed;
                //}
                //else if(deviceBoxes.Count == 0)
                //{
                //    TaskDialog.Show("Device Box Error", "No device box found.  Exiting");
                //    return Result.Failed;
                //}
                //Element deviceBox = deviceBoxes.First();


                ////Locate the Connector Families
                //IList<Element> deviceConnectors = devices
                //    .Where(d => d.get_Parameter(TCCElecSettings.BoxIdGuid) != null)
                //    .Where(d => d.get_Parameter(TCCElecSettings.DeviceIdGuid) != null)
                //    .ToList();

                ////Filter out only connectors that can be associated using the BoxID parameter
                ////(Eliminates embedded family connectors)
                //List<Element> filteredDeviceConnectors = new List<Element>();
                //foreach(Element deviceConnector in deviceConnectors)
                //{
                //    Parameter boxIdParam = deviceConnector.get_Parameter(TCCElecSettings.BoxIdGuid);
                //    if (doc.FamilyManager.CanElementParameterBeAssociated(boxIdParam))
                //        filteredDeviceConnectors.Add(deviceConnector);
                //}

                //deviceConnectors = filteredDeviceConnectors;

                //using (Transaction trans = new Transaction(doc, "Associate Parameters"))
                //{
                //    trans.Start();

                //    //DEVICE BOX PARAMETER MAPPING

                //    //Get all Parameters in Devicebox
                //    List<Parameter> deviceBoxParams = (from Parameter p in deviceBox.Parameters select p).ToList();

                //    //Get all Family Parameters in Family
                //    List<FamilyParameter> famParams = (from FamilyParameter fp in doc.FamilyManager.Parameters select fp).ToList();

                //    //Associate Device Box Parameters
                //    foreach(Parameter deviceBoxParam in deviceBoxParams)
                //    {
                //        foreach(FamilyParameter famParam in famParams)
                //        {
                //            if (deviceBoxParam.Definition.Name == famParam.Definition.Name)
                //                doc.FamilyManager.AssociateElementParameterToFamilyParameter(deviceBoxParam, famParam);
                //        }
                //    }

                //    //DEVICE CONNECTOR PARAMETER MAPPING

                //    int i = 1;
                //    foreach(Element deviceConnector in deviceConnectors)
                //    {
                //        //Get all Parameters in Devicebox
                //        List<Parameter> deviceConnectorParams = (from Parameter p in deviceConnector.Parameters select p).ToList();

                //        //Associate Device Connector Parameters
                //        foreach (Parameter deviceConnectorParam in deviceConnectorParams)
                //        {
                //            foreach (FamilyParameter famParam in famParams)
                //            {
                //                if (deviceConnectorParam.Definition.Name == famParam.Definition.Name)
                //                {
                //                    if(doc.FamilyManager.CanElementParameterBeAssociated(deviceConnectorParam))
                //                        doc.FamilyManager.AssociateElementParameterToFamilyParameter(deviceConnectorParam, famParam);
                //                }

                //            }
                //        }

                //        Parameter connectPosParam = deviceConnector.get_Parameter(TCCElecSettings.ConnectorPositionGuid);
                //        connectPosParam.Set(i);
                //        i++;
                //    }

                //    //FAMILY CODE CREATION

                //    //Get Backbox Code
                //    ElementId boxTypeId = deviceBox.GetTypeId();
                //    ElementType boxType = doc.GetElement(boxTypeId) as ElementType;
                //    string boxCode = boxType.get_Parameter(TCCElecSettings.BackboxCodeGuid).AsString();

                //    //Get connector device codes
                //    List<String> deviceCodes = new List<string>();

                //    foreach(Element deviceConnector in deviceConnectors)
                //    {
                //        //Get Element Type
                //        ElementId eTypeId = deviceConnector.GetTypeId();
                //        ElementType eType = doc.GetElement(eTypeId) as ElementType;

                //        deviceCodes.Add(eType.get_Parameter(TCCElecSettings.ConnectorGroupCodeGuid).AsString());
                //    }

                //    //Get distinct device codes
                //    List<string> distinctDeviceCodes = deviceCodes.Distinct().ToList();
                //    List<string> sortedDistinctDeviceCodes = new List<String>();

                //    //Sort Distinct Codes
                //    distinctDeviceCodes.Sort();

                //    //Find if there are any power connectors
                //    //And re-sort to place them at the beginning
                //    foreach(string distinctDeviceCode in distinctDeviceCodes)
                //    {
                //        if (distinctDeviceCode.Contains("X"))
                //        {
                //            sortedDistinctDeviceCodes.Add(distinctDeviceCode);
                //        }
                //    }
                //    foreach (string distinctDeviceCode in distinctDeviceCodes)
                //    {
                //        if (!distinctDeviceCode.Contains("X"))
                //        {
                //            sortedDistinctDeviceCodes.Add(distinctDeviceCode);
                //        }
                //    }

                //    //Create string of connectors & Box Code

                //    foreach (string distinctDeviceCode in sortedDistinctDeviceCodes)
                //    {
                //        int count = deviceCodes.Where(c => c.Equals(distinctDeviceCode))
                //            .Select(c => c)
                //            .Count();
                //        concatDeviceCode = concatDeviceCode + distinctDeviceCode + count.ToString();
                //    }
                //    concatDeviceCode = concatDeviceCode + "-" + boxCode;
                //    concatDeviceCode = "\"" + concatDeviceCode + "\"";

                //    //Assign to family parameter
                //    FamilyParameter famNameCodeParam = doc.FamilyManager.get_Parameter(TCCElecSettings.PlateFamilyCodeGuid);
                //    doc.FamilyManager.SetFormula(famNameCodeParam, concatDeviceCode);


                //    //TRANSFER BOX SIZE TO FAMILY
                //    FamilyParameter famBoxSizeParam = doc.FamilyManager.get_Parameter(TCCElecSettings.BackboxSizeGuid);
                //    string boxSize = boxType.get_Parameter(TCCElecSettings.BackboxSizeGuid).AsString();
                //    boxSize = "\"" + boxSize + "\"";
                //    doc.FamilyManager.SetFormula(famBoxSizeParam, boxSize);


                //    //CREATE BOX CODE


                //    trans.Commit();
                //}

                //TaskDialog.Show("Command Succeeded", "Parameters Association Complete" + Environment.NewLine +
                //    "Family Device Code: " + concatDeviceCode);

                return Result.Succeeded;
        }
    }
}
