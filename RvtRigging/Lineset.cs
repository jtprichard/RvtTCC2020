
using Autodesk.Revit.DB;
using System;

namespace RvtRigging
{
    public class Lineset
    {
        private readonly Document _doc;
        public Element lsElement { get; private set; }          //Lineset Element
        public string Name { get; private set; }                //Lineset Description
        public double Distance { get; private set; }            //Distace from Insertion
        public int Number { get; private set; }                 //Lineset Numerical Designation
        public string AltDesignation { get; private set; }      //Lineset Alternate Designation
        public string Venue { get; private set; }               //Venue Name

        public Lineset (Document doc, Element ele)
        {
            _doc = doc;
            lsElement = ele;

            Name = ele.get_Parameter(TCCRiggingSettings.LinesetNameGuid).AsString();
            Distance = ele.get_Parameter(TCCRiggingSettings.LinesetDistanceGuid).AsDouble();
            
            //Get Nunerical or Alternate Designation
            string lsDesignation = ele.get_Parameter(TCCRiggingSettings.LinesetNumberGuid).AsString();
            int tempNum;
            if(Int32.TryParse(lsDesignation,out tempNum))
            {
                Number = tempNum;
                AltDesignation = "";
            }
            else
            {
                Number = 0;
                AltDesignation = lsDesignation;
            }


        }
        public static bool Duplicate (Document doc, Lineset ls)
        {
            Location location = ls.lsElement.Location;
            LocationPoint locationPt = location as LocationPoint;
            //Point pt = locationPt as Point;

            //XYZ xyzpt = pt as XYZ;


            //XYZ location = ls.lsElement.get_Geometry()



            return true;
        }

    }
}
