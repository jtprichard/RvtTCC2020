using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Events;
using System.Windows.Forms;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdFPParamAssoc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            bool isValidPlate = true;

            if (!doc.IsFamilyDocument)
            {
                TaskDialog.Show("Document Error", "Command only availabe in Family Editor");
                return Result.Failed;
            }

            try
            {
                //Get FamilyManager
                FamilyManager fm = doc.FamilyManager;

                //Store Local Variables
                string famDeviceBox = "Device Box";
                string concatDeviceCode = "";
                string concatPlateCode = "";


                //GET SYSTEM CODE and TEMPLATE CLASSIFICATION INFORMATION
                //Get System Code
            
                DeviceId templateDeviceId = FamilyUtils.GetTemplateSystemCode(fm);
                if (templateDeviceId.System == 0)
                {
                    TaskDialog.Show("Template Family Error", "The family document is not compatible with this routine.  " +
                        "Ensure you are using the correct template");
                    return Result.Failed;
                }
                string SystemCode =  templateDeviceId.SystemCode.ToString();

                //Get Box Classifcation Code
                string boxClassCode = FamilyUtils.GetTemplateBoxClassificationCode(fm);
                if (boxClassCode == null)
                    isValidPlate = false;

                //Determine if template should apply distinct numbers to connectors
                bool isConnectSetPosition = FamilyUtils.IsSetConnectorPosition(boxClassCode);

                //GET ELEMENTS IN FAMILY BUILDER

                //Create Filtered Element Collector & Filter for Electrical Fixtures
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ElementQuickFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);

                //Apply Filter
                IList<Element> devices = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();

                //Locate Device Box(es) Family
                //List<Element> deviceBoxes = new List<Element>();
                IList<Element> deviceBoxes = devices
                    .Where(d => d.get_Parameter(TCCElecSettings.BackboxCodeGuid) != null)
                    .ToList();

                //Locate the visible Connector Families
                //Visibility filters out CS connectors

                IList<Element> deviceConnectorElements = devices
                    .Where(d => d.get_Parameter(TCCElecSettings.BoxIdGuid) != null)
                    .Where(d => d.get_Parameter(TCCElecSettings.DeviceIdGuid) != null)
                    .Where(d => d.get_Parameter(BuiltInParameter.IS_VISIBLE_PARAM).AsInteger() != 0)
                    .ToList();

                //Filter out only connectors that can be associated using the BoxID parameter
                //(Eliminates embedded family connectors)
                List<Element> filteredDeviceConnectorElements = new List<Element>();
                foreach (Element deviceConnectorElement in deviceConnectorElements)
                {
                    Parameter boxIdParam = deviceConnectorElement.get_Parameter(TCCElecSettings.BoxIdGuid);
                    if (doc.FamilyManager.CanElementParameterBeAssociated(boxIdParam))
                        filteredDeviceConnectorElements.Add(deviceConnectorElement);
                }

                //Create a list of Conector Devices from Elements
                deviceConnectorElements = filteredDeviceConnectorElements;
                List<DeviceConnector> deviceConnectors = new List<DeviceConnector>();

                foreach(Element filteredDeviceConnectorElement in filteredDeviceConnectorElements)
                {
                    deviceConnectors.Add(new DeviceConnector(filteredDeviceConnectorElement, "", doc));
                }

                //Get all Family Parameters in Family
                List<FamilyParameter> famParams = (from FamilyParameter fp in doc.FamilyManager.Parameters select fp).ToList();


                using (Transaction trans = new Transaction(doc, "Map Parameters"))
                {
                    trans.Start();

                    //DEVICE BOX PARAMETER MAPPING

                    foreach (Element db in deviceBoxes)
                    {
                        //Get all Parameters in Devicebox
                        List<Parameter> deviceBoxParams = (from Parameter p in db.Parameters select p).ToList();

                        //Associate Device Box Parameters
                        FamilyUtils.AssociateParameters(deviceBoxParams, famParams, doc);
                    }

                    //DEVICE CONNECTOR PARAMETER MAPPING

                    int i = 1;
                    int j = 1;
                    foreach (Element deviceConnectorElement in deviceConnectorElements)
                    {
                        //Get all Parameters in Devicebox
                        List<Parameter> deviceConnectorParams = (from Parameter p in deviceConnectorElement.Parameters select p).ToList();

                        //Associate Connector Parameters
                        FamilyUtils.AssociateParameters(deviceConnectorParams, famParams, doc);

                        //Set the connector position
                        if (isConnectSetPosition)
                        {
                            //Associate Connector Positions
                            Parameter connectPosParam = deviceConnectorElement.get_Parameter(TCCElecSettings.ConnectorPositionGuid);
                            if (!connectPosParam.IsReadOnly)
                            {
                                if (doc.FamilyManager.GetAssociatedFamilyParameter(connectPosParam) != null)
                                    doc.FamilyManager.AssociateElementParameterToFamilyParameter(connectPosParam, null);
                                connectPosParam.Set(i);
                                i++;
                            }

                            //Associate Sub Connector Positions
                            Parameter subconnectPosParam = deviceConnectorElement.get_Parameter(TCCElecSettings.ConnectorSubPositionGuid);
                            if (!subconnectPosParam.IsReadOnly)
                            {
                                if (doc.FamilyManager.GetAssociatedFamilyParameter(subconnectPosParam) != null)
                                    doc.FamilyManager.AssociateElementParameterToFamilyParameter(subconnectPosParam, null);
                                subconnectPosParam.Set(j);
                                j++;
                            }
                        }



                    }
                    trans.Commit();
                }



                if (isValidPlate)
                {
                    //FAMILY CODE CREATION
                    using (Transaction trans = new Transaction(doc, "Map Parameters"))
                    {
                        trans.Start();


                        try
                        {

                            //Get connector group series codes

                            // Get distinct group connectors and sort
                            List<DeviceConnector> distinctDeviceConnectors = deviceConnectors
                                .GroupBy(dc => dc.ConnectorGroupCode)
                                .Select(d => d.First())
                                .OrderBy(o => o.ConnectorGroupCode)
                                .ToList();

                            // Set Has Power and Has Low Voltage equal to false
                            bool hasPower = false;
                            //bool hasLowVoltage = false;

                            //Sort connectors with power connectors first
                            List<DeviceConnector> sortedDeviceConnectors = new List<DeviceConnector>();
                            List<DeviceConnector> noPowerDeviceConnectors = new List<DeviceConnector>();

                            foreach (DeviceConnector ddc in distinctDeviceConnectors)
                            {
                                if (ddc.DeviceId.Voltage != Voltage.LowVoltage)
                                    sortedDeviceConnectors.Add(ddc);
                                else
                                    noPowerDeviceConnectors.Add(ddc);
                            }
                            //Check if any have power or low voltage to determine box classification
                            if (sortedDeviceConnectors.Count() > 0)
                                hasPower = true;
                            //if (noPowerDeviceConnectors.Count() > 0)
                            //    hasLowVoltage = true;

                            sortedDeviceConnectors.AddRange(noPowerDeviceConnectors);

                            //Create Group Series Code String
                            string groupSeriesCodes = "";

                            foreach (DeviceConnector sortedDeviceConnector in sortedDeviceConnectors)
                            {
                                int count = deviceConnectors.Where(c => c.ConnectorGroupCode.Equals(sortedDeviceConnector.ConnectorGroupCode))
                                    .Select(c => c)
                                    .Count();
                                groupSeriesCodes = groupSeriesCodes + sortedDeviceConnector.ConnectorGroupCode + count.ToString();
                            }


                            //Box Type Classification Code
                            //If this is a type of DEvice Box, then
                            //Determine if this is a Power, Control, or Universal Box
                            string plateClassCode = "";
                            if (FamilyUtils.IsDeviceBox(boxClassCode))
                            {
                                FamilyParameter BoxClassificationCodeParam = doc.FamilyManager.get_Parameter(TCCElecSettings.BoxClassificationCode);

                                //Get Box and Plate Classification Codes

                                if (sortedDeviceConnectors.Count() > 1)
                                {
                                    boxClassCode = "UB";
                                    plateClassCode = "U";
                                }
                                else if (hasPower)
                                {
                                    boxClassCode = "PB";
                                    plateClassCode = sortedDeviceConnectors[0].ConnectorGroupCode;
                                    plateClassCode = plateClassCode[0].ToString();
                                }
                                else
                                {
                                    boxClassCode = "CB";
                                    plateClassCode = sortedDeviceConnectors[0].ConnectorGroupCode;
                                    plateClassCode = plateClassCode[0].ToString();
                                }
                                //Assign Box Classification Code
                                doc.FamilyManager.SetFormula(BoxClassificationCodeParam, "\"" + boxClassCode + "\"");
                            }
                            else
                            {
                                plateClassCode = FamilyUtils.getPlateClassCode(boxClassCode);
                            }


                            //Get Backbox Code
                            //First check if Connector strip, and if so, apply C and length
                            string boxCode = "";
                            if (boxClassCode == "CS")
                            {
                                double? csLength = FamilyUtils.GetTemplateCSLength(fm);
                                boxCode = Math.Floor(Convert.ToDouble(csLength)).ToString() + "C";

                                //TRANSFER BOX SIZE TO FAMILY
                                FamilyParameter famBoxSizeParam = doc.FamilyManager.get_Parameter(TCCElecSettings.BackboxSizeGuid);
                                string boxSize = "\"" + csLength.ToString() + "' Long" + "\"";
                                doc.FamilyManager.SetFormula(famBoxSizeParam, boxSize);
                            }
                            else
                            {
                                if (deviceBoxes.Count == 0)
                                {
                                    TaskDialog.Show("Device Box Error", "There are no compatible device boxes in the family.  Family name cannot be created.  Exiting.");
                                    trans.Commit();
                                    return Result.Succeeded;
                                }
                                if (deviceBoxes.Count > 1)
                                {
                                    TaskDialog.Show("Device Box Warning", "WARNING - Multiple device boxes are in the family.  " +
                                        "Association may not work correctly.  Verify family.");
                                }
                                ElementId boxTypeId = deviceBoxes[0].GetTypeId();
                                ElementType boxType = doc.GetElement(boxTypeId) as ElementType;
                                boxCode = boxType.get_Parameter(TCCElecSettings.BackboxCodeGuid).AsString();

                                //TRANSFER BOX SIZE TO FAMILY
                                FamilyParameter famBoxSizeParam = doc.FamilyManager.get_Parameter(TCCElecSettings.BackboxSizeGuid);
                                string boxSize = boxType.get_Parameter(TCCElecSettings.BackboxSizeGuid).AsString();
                                boxSize = "\"" + boxSize + "\"";
                                doc.FamilyManager.SetFormula(famBoxSizeParam, boxSize);
                            }

                            //Create the concatenated device code
                            concatDeviceCode = SystemCode + "-" + boxClassCode + "-" +
                                groupSeriesCodes + "-" + boxCode;
                            string concatParamDeviceCode = "\"" + concatDeviceCode + "\"";

                            //Assign to family parameter
                            FamilyParameter famNameCodeParam = doc.FamilyManager.get_Parameter(TCCElecSettings.PlateFamilyCodeGuid);
                            doc.FamilyManager.SetFormula(famNameCodeParam, concatParamDeviceCode);


                            //CREATE BOX CODE
                            string plateThirdDigit = "";
                            if (plateClassCode == "U")
                            {
                                for (int j = 0; j < 2; j++)
                                {
                                    if (boxCode[j].ToString() != "G")
                                        plateThirdDigit += boxCode[j];
                                }
                            }
                            else if (plateClassCode == "C")
                            {
                                int count = deviceConnectors.Where(c => c.DeviceId.Voltage != Voltage.LowVoltage)
                                    .Select(c => c)
                                    .Count();
                                plateThirdDigit = count.ToString();
                            }
                            else
                            {
                                plateThirdDigit = deviceConnectors.Count().ToString();
                            }

                            string plateFourthDigit = "";
                            if (plateClassCode == "U" || plateClassCode == "S")
                                plateFourthDigit = "#";

                            concatPlateCode = concatPlateCode + SystemCode +
                                plateClassCode +
                                plateThirdDigit +
                                plateFourthDigit;

                            string concatParamPlateCode = "\"" + concatPlateCode + "\"";

                            //Assign to family parameter
                            FamilyParameter famPlateCodeParam = doc.FamilyManager.get_Parameter(TCCElecSettings.PlateCodeGuid);
                            doc.FamilyManager.SetFormula(famPlateCodeParam, concatParamPlateCode);

                            //Copy Family Name Code to Clipboard
                            Clipboard.SetText(concatDeviceCode);
                            trans.Commit();

                            if (TaskDialog.Show("Command Succeeded", "Parameters Association Complete" + Environment.NewLine +
                                "Family Device Code: " + concatDeviceCode + Environment.NewLine +
                                "Plate Code: " + concatPlateCode + Environment.NewLine +
                                "Family Device Code has been copied to clipboard." + Environment.NewLine +
                                "Press <ctl>V to paste in the file name" + Environment.NewLine + Environment.NewLine +
                                "Save File? ", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No) == TaskDialogResult.Yes)
                                uidoc.SaveAs();

                        }
                        catch
                        {
                            TaskDialog.Show("Plate Code Creation Error", "Error Creating Plate Code.  \n Please manually enter");
                            trans.Commit();
                        }
                    }
                    
                }
                return Result.Succeeded;
            }

            catch (Exception ex)
            {
                UIUtils.EBSGenException(ex);
                return Result.Failed;
            }
        }
    }
}
