using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace RvtRigging
{
    public class DimStringConvert
    {
        private static string StringFilter(string input)
        {
            //Remove all characters except numbers and ' "
            var numericChars = "0123456789'\" /".ToCharArray();
            string tempvalue = new String(input.Where(c => numericChars.Any(n => n == c)).ToArray());

            //Look for multiple ' marks
            List<int> positions = new List<int>();
            int pos = 0;
            while ((pos < tempvalue.Length) && (pos = tempvalue.IndexOf("'",pos)) != -1)
            {
                positions.Add(pos);
                pos += 1;
            }

            if (positions.Count > 1)
                return "Feet Error";

            // Look for multiple " marks
            positions.Clear();
            pos = 0;
            while ((pos < tempvalue.Length) && (pos = tempvalue.IndexOf("\"", pos)) != -1)
            {
                positions.Add(pos);
                pos += 1;
            }

            if (positions.Count > 1)
                return "Inches Error";

            return tempvalue.ToString();

        }

        private static int GetFeet(string input)
        {
            int tempvalue;
            int feetindex;
            int inchindex;

            feetindex = input.IndexOf("'");
            inchindex = input.IndexOf("\"");

            if (feetindex > 0)
                tempvalue = int.Parse(input.Substring(0, feetindex));
            else if (inchindex < 0)
                tempvalue = int.Parse(input);
            else
                tempvalue = 0;
            return tempvalue;

        }

        private static int GetInches(string input)
        {
            int tempvalue;
            int feetindex;
            int inchindex;

            feetindex = input.IndexOf("'");
            inchindex = input.IndexOf("\"");
            int length = input.Length;

            //Gets inches from f'i"
            if (feetindex > 0 && inchindex > 0)
                tempvalue = int.Parse(input.Substring(feetindex + 1, inchindex - feetindex - 1));
            //Gets inches from f'i
            //Ensures no testing for f' which creates an exception
            else if (feetindex > 0 && feetindex != length-1 && inchindex < 0)
                tempvalue = int.Parse(input.Substring(feetindex + 1));
            //Gets inches from i"
            else if (feetindex < 0 && inchindex > 0)
                tempvalue = int.Parse(input.Substring(0, inchindex));
            else
                tempvalue = 0;

            return tempvalue;

        }

        public static int DimStrToDecInch(string input)
        //Retrieve integer result in inches from string input in f'-i" 
        //or other variations acceptable to Revit
        {
            string output = StringFilter(input);
            int feet = GetFeet(output);
            int inch = GetInches(output);

            int total = (feet * 12) + inch;

            return total;
        }
    }
}
