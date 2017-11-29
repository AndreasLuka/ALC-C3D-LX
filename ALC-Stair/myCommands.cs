using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_Stair.C3D.MyCommands))]

namespace ALC_Stair.C3D
{
    public class MyCommands
    {

        [CommandMethod("ALCGroup", "ALCCreateStair", "ALCStairLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ALCStair()
        {
            myFunctions.createStair3D(3, 0.15, 0.30, 0.63, 2.1);

        }


        [CommandMethod("GetPointFromUser")]
        public static void GetPointFromUser()
        {
            var doc =
            Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            // First let's get the start position 

            var opts =
              new PromptPointOptions("\nSpecify stair location: ");
            var ppr = ed.GetPoint(opts);

            if (ppr.Status == PromptStatus.OK)
            {
                // In order for the visual style to be respected,
                // we'll add the to-be-jigged solid to the database
                

            }
        }

        [CommandMethod("GetIntegerOrKeywordFromUser")]
        public static void GetIntegerOrKeywordFromUser()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
            pIntOpts.Message = "\nEnter number of stairs or ";

            // Restrict input to positive and non-negative values
            pIntOpts.AllowZero = false;
            pIntOpts.AllowNegative = false;
            pIntOpts.DefaultValue = 5;

            // Define the valid keywords and allow Enter
            pIntOpts.Keywords.Add("Tread");
            pIntOpts.Keywords.Add("Riser");
            pIntOpts.Keywords.Add("Landing");
            pIntOpts.Keywords.Add("Width");
            pIntOpts.AllowNone = true;

            // Get the value entered by the user
            PromptIntegerResult pIntRes = doc.Editor.GetInteger(pIntOpts);

            if (pIntRes.Status == PromptStatus.Keyword)
            {
                // Handling keywords
            }
            else
            {
                Application.ShowAlertDialog("Entered value: " + pIntRes.Value.ToString());
                myFunctions.createStair3D(pIntRes.Value, 0.15, 0.30, 0.63, 2);
            }

        }
    }
}


