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
    public class GetParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                //Pick Object
                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                //Display Element ID
                if (pickedObj != null)
                {
                    // Retreive Element
                    ElementId eleId = pickedObj.ElementId;
                    Element ele = doc.GetElement(eleId);

                    //Get Parameter                  
                    Parameter param = ele.LookupParameter("TC_BOX_ID");

                    TaskDialog.Show("Parameter Values", string.Format("Parameter Storage Type {0} and value {1}",
                        param.StorageType.ToString(),
                        param.AsInteger()));

                    //InternalDefinition paramDef = param.Definition as InternalDefinition;

                    //TaskDialog.Show("Parameters", string.Format("{0} parameter of type {1} with bultinparameter {2}",
                    //    paramDef.Name,
                    //    paramDef.UnitType,
                    //    paramDef.BuiltInParameter));

                    //Set Parameter Value
                    using (Transaction trans = new Transaction(doc, "Set Parameter"))
                    {
                        trans.Start();

                        param.Set(301);

                        trans.Commit();
                    }

                }
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}
