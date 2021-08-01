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
    public class CmdCreateFaceplateSchedules : IExternalCommand
    {
        //TEMPORARY HARD CODED NAMES
        string _templateName = "Faceplate_Schedule_Template";
        string _scheduleNamePrefix = "Faceplate ";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;
            Document doc = uidoc.Document;



            try
            {
                //Locate all Distinct DeviceBoxes
                var svcBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfSVC);
                IList<string> svcPlateCodes = new List<string>();
                foreach(var svcBox in svcBoxes)
                {
                    svcPlateCodes.Add(svcBox.PlateCode);
                }
                svcPlateCodes = svcPlateCodes.Distinct().ToList();

                //Performance Lighting Boxes
                var ltgBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfLighting);
                IList<string> ltgPlateCodes = new List<string>();
                foreach (var ltgBox in ltgBoxes)
                {
                    ltgPlateCodes.Add(ltgBox.PlateCode);
                }

                //Also add arch lighting boxes
                var archLtgBoxes = DeviceBox.GetDeviceBoxes(doc, System.ArchLighting);
                foreach (var archLtgBox in archLtgBoxes)
                {
                    ltgPlateCodes.Add(archLtgBox.PlateCode);
                }

                ltgPlateCodes = ltgPlateCodes.Distinct().ToList();

                var machBoxes = DeviceBox.GetDeviceBoxes(doc, System.PerfMachinery);
                IList<string> machPlateCodes = new List<string>();
                foreach (var mchBox in machBoxes)
                {
                    machPlateCodes.Add(mchBox.PlateCode);
                }
                machPlateCodes = machPlateCodes.Distinct().ToList();



                //Locate a schedule to duplicate

                bool located = false;
                FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule));
                ViewSchedule template = col.First() as ViewSchedule;

                foreach(ViewSchedule vs in col)
                {
                    if (vs.Name == _templateName)
                    {
                        template = vs;
                        located = true;
                    }
                }
                if (!located)
                    return Result.Failed;
                            
                    foreach(string svcPlateCode in svcPlateCodes)
                    {
                        if(!ScheduleExists(svcPlateCode, doc))
                            CreateSchedule(0, 1, 0, svcPlateCode, template, doc);
                    }

                    foreach (string ltgPlateCode in ltgPlateCodes)
                    {
                        if (!ScheduleExists(ltgPlateCode, doc))
                            CreateSchedule(1, 0, 0, ltgPlateCode, template, doc);
                    }

                    foreach (string machPlateCode in machPlateCodes)
                    {
                        if (!ScheduleExists(machPlateCode, doc))
                            CreateSchedule(0, 0, 1, machPlateCode, template, doc);
                    }

                    TaskDialog.Show("Success", "Schedule Creation Complete");

            }
            catch
            {
                TaskDialog.Show("ERROR", "Testing Error has occurred");
            }

            return Result.Succeeded;
        }
        private void CreateSchedule(int ltgbool, int svcbool, int machbool, string boxCode, ViewSchedule template, Document doc)
        {
            var defaultScheduleTypeParameterName = "TCC Schedule Type";
            var defaultSchedulePrefix = "Faceplate - ";

            using (Transaction trans = new Transaction(doc, "CreateSchedule"))
            {
                try
                {
                    trans.Start();

                    //Duplicates and renames schedule
                    ElementId newScheduleId = template.Duplicate(ViewDuplicateOption.Duplicate);
                    ViewSchedule newSchedule = doc.GetElement(newScheduleId) as ViewSchedule;
            
                    newSchedule.Name = _scheduleNamePrefix + boxCode;

                    //Get Schedule Type Parameter and assign name
                    Parameter scheduleTypeParam = newSchedule.LookupParameter(defaultScheduleTypeParameterName);
                    if (ltgbool == 1)
                        scheduleTypeParam.Set(defaultSchedulePrefix + "PL");
                    else if (svcbool == 1)
                        scheduleTypeParam.Set(defaultSchedulePrefix + "SVC");
                    else if (machbool == 1)
                        scheduleTypeParam.Set(defaultSchedulePrefix + "PM");


                    var svcParamId = SharedParameterElement.Lookup(doc, new Guid("2d71673f-91f3-4627-98d6-7de0a8926157")).Id;
                    var ltgParamId = SharedParameterElement.Lookup(doc, new Guid("cfa4ada1-0dc3-4911-bf37-6aaa9d9c86e6")).Id;
                    var machParamId = SharedParameterElement.Lookup(doc, new Guid("975c6667-6c6b-4193-a60a-7cadb145ec2e")).Id;
                    var boxCodeParamId = SharedParameterElement.Lookup(doc, TCCElecSettings.PlateCodeGuid).Id;

                    ScheduleFieldId svcFieldId = null;
                    ScheduleFieldId ltgFieldId = null;
                    ScheduleFieldId machFieldId = null;
                    ScheduleFieldId boxCodeFieldId = null;

                    //ScheduleFilter filter = new ScheduleFilter();
                    var scheduleFieldIds = newSchedule.Definition.GetFieldOrder();
                    var scheduleFields = new List<ScheduleField>();

                    foreach (ScheduleFieldId id in scheduleFieldIds)
                    {
                        scheduleFields.Add(newSchedule.Definition.GetField(id));
                    }

                    foreach (ScheduleField sf in scheduleFields)
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
                    foreach (ScheduleFilter filter in scheduleFilters)
                    {
                        //if (filter.IsStringValue && filter.GetStringValue() == "PLATE")

                        if (filter.FieldId == svcFieldId)
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
                    //foundsvcFilter.SetValue(svcbool);
                    //foundltgFilter.SetValue(ltgbool);
                    //foundmachFilter.SetValue(machbool);
                    foundBoxCodeFilter.SetValue(boxCode);

                    //newSchedule.Definition.SetFilter(svcindex, foundsvcFilter);
                    //newSchedule.Definition.SetFilter(ltgindex, foundltgFilter);
                    //newSchedule.Definition.SetFilter(machindex, foundmachFilter);
                    newSchedule.Definition.SetFilter(boxCodeIndex, foundBoxCodeFilter);

                    trans.Commit();
                }

                catch
                {
                    trans.Dispose();
                }

            }

        }

        private bool ScheduleExists(string name, Document doc)
        {
            bool located = false;
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule));

            foreach (ViewSchedule vs in col)
            {
                if (vs.Name == name)
                {
                    located = true;
                }
            }

            return located;

        }
    }
}
