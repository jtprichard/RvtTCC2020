using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdTagDeviceBox : IExternalCommand
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

                try
                {
                    DeviceBox db = new DeviceBox(doc, elem);

                    using (Transaction trans = new Transaction(doc, "Tag DeviceBox"))
                    {
                        //A TAG IS ONLY VISIBLE IF IT'S TAGGED ELEMENT IS VISIBLE IN A VIEW
                        //FOR CONNECTORS, IF THEY ARE OUTSIDE OF A VIEW RANGE, THE TAG WILL NOT SHOW UP REGARDLESS OF THE FACT THAT
                        //WE ARE USING THE DEVICE BOX AS THE XYZ GEOMETRY
                        //CHOICES:  ENSURE ALL CONNECTORS ARE ON THE FACE OF THE DEVICE BOX, OR ADJUST THE VIEW DEPTH IN THE MODEL

                        trans.Start();

                        try
                            {

                                var deviceTag = new DeviceTag();

                                //Test for SVC vs Other Plate
                                if (db.System == DeviceSystem.PerfSVC)
                                    if (db.HasPower)
                                    {
                                        if(db.HasIG)
                                            deviceTag = DeviceTag.SpecialtyDeviceTagPowerIG();
                                        else
                                            deviceTag = DeviceTag.SpecialtyDeviceTagPower();
                                }

                                else
                                    deviceTag = DeviceTag.SpecialtyDeviceTagNormal();

                                LocationPoint elemLocation = elem.Location as LocationPoint;
                                deviceTag.Location = elemLocation.Point;
                                deviceTag.TagReference = new Reference(elem);

                                deviceTag.TagDeviceBox(doc, view);

                            }

                            catch (Exception e)
                            {
                                TaskDialog.Show("ERROR", e.Message);
                                trans.Dispose();
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
