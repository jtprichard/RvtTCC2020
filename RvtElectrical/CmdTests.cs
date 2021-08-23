using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdTests : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            //Define a reference Object to accept the pick result
            Reference pickedObj = null;
            
            //Pick an object
            Selection sel = uiapp.ActiveUIDocument.Selection;

            pickedObj = sel.PickObject(ObjectType.Element, "Select an element");
            Element elem = doc.GetElement(pickedObj);


            


            try
            {


                DeviceBox db = new DeviceBox(doc, elem);

                //GET THE POWER CONNECTORS
                //INSTEAD OF JUST GETTING THE FIRST, CREATE A TAG FOR EACH ONE SLIGHTLY OFFSET
                //USE THIS TECHNIQUE FOR TAGGING POWER VS. NON POWER ITEMS
                //THIS CAN BE THE TAGGING TECHNIQUE FOR DEVICE BOXES IN GENERAL
                //CHANGE SEARCH PARAMETERS TO LOOK FOR CONNECTORS BY VOLTAGE
                //WILL NEED TO ADD GETCONNECTORSBYBOX OVERLOAD TO SEARCH FOR VOLTAGE

                var powerConnectors = DeviceConnector.GetConnectorsByBox(db,Voltage.High120V1Ph);
                Element powerConnectorElement = powerConnectors.First().Connector;


                //string dbBoxId = db.DeviceId.Value.ToString();
                //string dbNumber = db.BoxId.ToString();
                //string dbVenue = db.Venue.ToString();
                //string dbNumConnectors = db.Connectors.Count().ToString();

                //TaskDialog.Show("TEST MESSAGE", "DeviceBox ID: " + dbBoxId + Environment.NewLine +
                //    "Box Number: " + dbNumber + Environment.NewLine +
                //    "Venue: " + dbVenue + Environment.NewLine +
                //    "Connector Qty: " + dbNumConnectors);

                //IList<DeviceBox> deviceBoxes = DeviceBox.GetDeviceBoxes(doc);

                //IList<DeviceBox> lightingDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfLighting);

                //IList<DeviceBox> svcDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfSVC);

                //IList<DeviceBox> machDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfMachinery);

                //IList<DeviceBox> cablePasses = DeviceBox.GetDeviceBoxes(doc, System.CablePasses);

                //Locate a list of deviceboxes
                //IList<DeviceBox> lightingDeviceBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfLighting);

                //Locate a schedule to duplicate



                using (Transaction trans = new Transaction(doc, "Command Test"))
                {
                    try
                    {
                        //A TAG IS ONLY VISIBLE IF IT'S TAGGED ELEMENT IS VISIBLE IN A VIEW
                        //FOR CONNECTORS, IF THEY ARE OUTSIDE OF A VIEW RANGE, THE TAG WILL NOT SHOW UP REGARDLESS OF THE FACT THAT
                        //WE ARE USING THE DEVICE BOX AS THE XYZ GEOMETRY
                        //CHOICES:  ENSURE ALL CONNECTORS ARE ON THE FACE OF THE DEVICE BOX, OR ADJUST THE VIEW DEPTH IN THE MODEL
                        
                        trans.Start();

                        //Desired tag info
                        var familyTagName = "Power Tag Multicategory";
                        var familyTagNameSymb = "Power_IG";
                        var bicTagBeing = BuiltInCategory.OST_MultiCategoryTags;
                        bool hasLeader = true;
                        
                        //Define tag mode and orientation
                        TagMode tagMode = TagMode.TM_ADDBY_MULTICATEGORY;
                        TagOrientation tagorn = TagOrientation.Horizontal;

                        LocationPoint elemLocation = elem.Location as LocationPoint;

                        XYZ boxLocation = elemLocation.Point;
                        Reference elemRef = new Reference(powerConnectorElement);

                        IndependentTag newTag = IndependentTag.Create(doc, view.Id, elemRef, hasLeader, tagMode, tagorn, boxLocation);

                        Element desiredTagType = FamilyLocateUtils.FindFamilyType(doc, typeof(FamilySymbol), familyTagName, familyTagNameSymb, bicTagBeing);

                        if (desiredTagType != null)
                        {
                            newTag.ChangeTypeId(desiredTagType.Id);
                        }




                        trans.Commit();
                    }

                    catch (Exception e)
                    {
                        TaskDialog.Show("ERROR", e.Message);
                        trans.Dispose();
                    }

                }



            }
            catch
            {
                TaskDialog.Show("ERROR", "Testing Error has occurred");
            }

            return Result.Succeeded;
        }
    }
}
