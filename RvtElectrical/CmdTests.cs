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

            //Define a reference Object to accept the pick result
            Reference pickedObj = null;
            
            ////Pick an object
            //Selection sel = uiapp.ActiveUIDocument.Selection;

            //pickedObj = sel.PickObject(ObjectType.Element, "Select an element");
            //Element elem = doc.GetElement(pickedObj);

            

            try
            {
                //DeviceBox db = new DeviceBox(doc, elem);

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

                bool located = false;
                FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule));
                ViewSchedule template = col.First() as ViewSchedule;

                foreach(ViewSchedule vs in col)
                {
                    if (vs.Name == "Faceplate_Schedule_Template")
                    {
                        template = vs;
                        //template.Duplicate(ViewDuplicateOption.Duplicate);
                        located = true;
                    }
                }
                if (!located)
                    return Result.Failed;

                using (Transaction trans = new Transaction(doc, "Command Test"))
                {
                    try
                    {
                        trans.Start();

                        //Duplicates and renames schedule
                        ElementId newScheduleId = template.Duplicate(ViewDuplicateOption.Duplicate);
                        ViewSchedule newSchedule = doc.GetElement(newScheduleId) as ViewSchedule;
                        newSchedule.Name = "TestName";


                        var svcParamId = SharedParameterElement.Lookup(doc, new Guid("2d71673f-91f3-4627-98d6-7de0a8926157")).Id;
                        var ltgParamId = SharedParameterElement.Lookup(doc, new Guid("cfa4ada1-0dc3-4911-bf37-6aaa9d9c86e6")).Id;
                        var machParamId = SharedParameterElement.Lookup(doc, new Guid("975c6667-6c6b-4193-a60a-7cadb145ec2e")).Id;
                        var boxCodeParamId = SharedParameterElement.Lookup(doc, TCCElecSettings.PlateCodeGuid).Id;

                        //var sfs = newSchedule.Definition.GetSchedulableFields();
                        //var svcField = new SchedulableField();
                        //var ltgField = new SchedulableField();
                        //var machField = new SchedulableField();


                        //foreach(SchedulableField f in sfs)
                        //{
                        //    if (f.ParameterId == svcSPE)
                        //        svcField = f;
                        //    if (f.ParameterId == ltgSPE)
                        //        ltgField = f;
                        //    if (f.ParameterId == machSPE)
                        //        machField = f;

                        //}


                        ScheduleFieldId svcFieldId = null;
                        ScheduleFieldId ltgFieldId = null;
                        ScheduleFieldId machFieldId = null;
                        ScheduleFieldId boxCodeFieldId = null;

                        //ScheduleFilter filter = new ScheduleFilter();
                        var scheduleFieldIds = newSchedule.Definition.GetFieldOrder();
                        var scheduleFields = new List<ScheduleField>();

                        foreach(ScheduleFieldId id in scheduleFieldIds)
                        {
                            scheduleFields.Add(newSchedule.Definition.GetField(id));
                        }

                        foreach(ScheduleField sf in scheduleFields)
                        {
                            if (sf.ParameterId == svcParamId)
                                svcFieldId = sf.FieldId;
                            //svcFieldIndex = sf.FieldIndex;
                            if (sf.ParameterId == ltgParamId)
                                ltgFieldId = sf.FieldId;
                            //ltgFieldIndex = sf.FieldIndex;
                            if (sf.ParameterId == machParamId)
                                machFieldId = sf.FieldId;
                            //machFieldIndex = sf.FieldIndex;
                            if (sf.ParameterId == boxCodeParamId)
                                boxCodeFieldId = sf.FieldId;
                                //boxCodeIndex = sf.FieldIndex;
                        }


                        //Gets schedule filters by current label data
                        //probably not the best way to do this
                        var scheduleFilters = newSchedule.Definition.GetFilters();
                        var foundsvcFilter = new ScheduleFilter();
                        var foundltgFilter = new ScheduleFilter();
                        var foundmachFilter = new ScheduleFilter();
                        var foundBoxCodeFilter = new ScheduleFilter();
                        int svcindex = 0;
                        int ltgindex = 0;
                        int machindex = 0;
                        int boxCodeIndex = 0;
                        int i = 0;
                        foreach(ScheduleFilter filter in scheduleFilters)
                        {
                            //if (filter.IsStringValue && filter.GetStringValue() == "PLATE")
                  
                            if(filter.FieldId == svcFieldId)
                            {
                                foundsvcFilter = filter;
                                svcindex = i;
                            }

                            if (filter.FieldId == ltgFieldId)
                            {
                                foundltgFilter = filter;
                                ltgindex = i;
                            }

                            if (filter.FieldId == machFieldId)
                            {
                                foundmachFilter = filter;
                                machindex = i;
                            }

                            if (filter.FieldId == boxCodeFieldId)
                            {
                                foundBoxCodeFilter = filter;
                                boxCodeIndex = i;
                            }

                            i++;
                        }

                        //Applies filter data and filter
                        foundsvcFilter.SetValue(1);
                        foundltgFilter.SetValue(0);
                        foundmachFilter.SetValue(0);
                        foundBoxCodeFilter.SetValue("HELLO!");

                        newSchedule.Definition.SetFilter(svcindex, foundsvcFilter);
                        newSchedule.Definition.SetFilter(ltgindex, foundltgFilter);
                        newSchedule.Definition.SetFilter(machindex, foundmachFilter);
                        newSchedule.Definition.SetFilter(boxCodeIndex, foundBoxCodeFilter);



                        trans.Commit();
                    }

                    catch
                    {
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
