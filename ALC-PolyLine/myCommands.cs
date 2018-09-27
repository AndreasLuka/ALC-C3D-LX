// (C) Copyright 2018 by Andreas Luka


using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(LX_PolyLine.MyCommands))]

namespace LX_PolyLine
{

    public class MyCommands
    {

        // Modal Command with pickfirst selection
        [CommandMethod("MyGroup", "LX-Polyline", CommandFlags.Modal)]
        public void AlignToPoly() 
        {
            // Getting the autodesk database (needed for transaction)
            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            // Getting the editor object (needed for error messaging)
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            // Getting the civil document object (need for check of alignments presence)
            CivilDocument cd = CivilApplication.ActiveDocument;

            // Getting the list of all alignments in an collection of the ids
            ObjectIdCollection alignIds = cd.GetAlignmentIds();

            // Getting out if there are no alignments in drawing
            if (alignIds.Count == 0)
            {
                ed.WriteMessage("\nNo alignments in drawing.");
                return;
            }

            // Calling function to select a single alignment
            ObjectId alignId = SelectAlignment();

            // Getting out if the selection failes
            if (alignId == ObjectId.Null) return;

            // Getting the alignment from his id and creating the polyline
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Alignment align = (Alignment)alignId.GetObject(OpenMode.ForRead);
                    ObjectId plId = align.GetPolyline();
                }
                catch { }
                tr.Commit();
            }
        }


        // This function select an Alignment and returns his ObjectId or Null in case of Cancelation or Error
        public static ObjectId SelectAlignment()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            // Setting ObjectId to Null 
            ObjectId result = ObjectId.Null;

            // Creating the promt option for a single entity prompt
            PromptEntityOptions prOptions = new PromptEntityOptions("\nSelect alignment: ");
            prOptions.SetRejectMessage("...not an Alignment, try again!:");
            prOptions.AddAllowedClass(typeof(Alignment), true);
            
            // Getting the selected element from the editor
            PromptEntityResult prResult = ed.GetEntity(prOptions);

            // Returning the selected alignment's id
            if (prResult.Status == PromptStatus.OK) result = prResult.ObjectId;
            return result;
        }
    }
}
