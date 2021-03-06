using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdTagDeviceBoxPower : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            string circuitValue;


            do
            {
                //Define a reference Object to accept the pick result
                Reference pickedObj = null;
                Element elem;

                try
                {
                    //Pick an object
                    Selection sel = uiapp.ActiveUIDocument.Selection;
                    SelectionFilterDeviceBox selFilter = new SelectionFilterDeviceBox();

                    pickedObj = sel.PickObject(ObjectType.Element, selFilter, "Select a Device Box");
                    elem = doc.GetElement(pickedObj);
                }

                //catch Escape from routine
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }

                //Get the defaul text value
                string rt = "TCC Technical";
                string rp = "Device Tags";
                string riBoxNumStr = "Circuit_Default_Text";
                TextBox tb = null;

                if (RibbonUtils.GetRibbonItem(rt, rp, riBoxNumStr, uiapp, out RibbonItem riBoxNum))
                {
                    tb = riBoxNum as TextBox;
                    circuitValue = tb.Value.ToString();

                }
                else
                {
                    TaskDialog.Show("Create Tag Error", "Tag Information Not Found - Notify Developer");
                    throw new Exception();
                }

                try
                {
                    DeviceBox db = new DeviceBox(doc, elem);

                    var powerConnectors = DeviceConnector.GetConnectorsByBox(db, Voltage.High120V1Ph);
                    Element powerConnectorElement = powerConnectors.First().Connector;

                    var powerConnectorElements = powerConnectors
                        .GroupBy(n => n.Connector.Name)
                        .Select(g => g.First())
                        .ToList();

                    using (Transaction trans = new Transaction(doc, "Tag DeviceBox Power"))
                    {
                        //A TAG IS ONLY VISIBLE IF IT'S TAGGED ELEMENT IS VISIBLE IN A VIEW
                        //FOR CONNECTORS, IF THEY ARE OUTSIDE OF A VIEW RANGE, THE TAG WILL NOT SHOW UP REGARDLESS OF THE FACT THAT
                        //WE ARE USING THE DEVICE BOX AS THE XYZ GEOMETRY
                        //CHOICES:  ENSURE ALL CONNECTORS ARE ON THE FACE OF THE DEVICE BOX, OR ADJUST THE VIEW DEPTH IN THE MODEL

                        trans.Start();
                        foreach (var connectorElement in powerConnectorElements)
                        {

                            try
                            {

                                var deviceTag = new DeviceTag();

                                //Test for General vs. Specialty plate
                                if (db.System == DeviceSystem.General)
                                {
                                    deviceTag = DeviceTag.GeneralPowerTag();

                                    //Change Connector_Concat to "GP" if nothing set
                                    Parameter circuitParam = connectorElement.Connector.get_Parameter(TCCElecSettings.ConnectorCircuitConcatGuid);
                                    if (!circuitParam.HasValue)
                                        circuitParam.Set(circuitValue);
                                }

                                else
                                {
                                    if (connectorElement.IsIG)
                                        deviceTag = DeviceTag.DeviceIGPowerTag();
                                    else
                                    {
                                        deviceTag = DeviceTag.DevicePowerTag();
                                        //Change Connector_Concat to "GP" if nothing set
                                        Parameter circuitParam = connectorElement.Connector.get_Parameter(TCCElecSettings.ConnectorCircuitConcatGuid);
                                        if (!circuitParam.HasValue)
                                            circuitParam.Set(circuitValue);
                                    }

                                }

                                LocationPoint elemLocation = elem.Location as LocationPoint;
                                deviceTag.Location = elemLocation.Point;
                                deviceTag.TagReference = new Reference(connectorElement.Connector);

                                deviceTag.TagDeviceBox(doc, view);

                            }

                            catch (Exception e)
                            {
                                TaskDialog.Show("ERROR", e.Message);
                                trans.Dispose();
                            }
                        }

                        trans.Commit();
                    }

                }
                catch
                {
                    TaskDialog.Show("ERROR", "Testing Error has occurred");
                }
            } while (true);
            


            return Result.Succeeded;
        }
    }
}
