// (C) Copyright 2018 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;

using Autodesk.Civil;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_AlignmentGetDirection.MyCommands))]

namespace ALC_AlignmentGetDirection
{

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {

        // Modal Command with pickfirst selection
        [CommandMethod("MyGroup", "ALCGetDirection", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void GetDirectionFromAlignment() // This method can have any name
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            double easting = 0;
            double northing = 0;
            double offset = 0;

            double station = Double.NaN;
            double bearing = Double.NaN;


            bool inRange;

            Point3d ptOnAlignment = new Point3d(0, 0, 0);
            Point3d ptOffAlignment = new Point3d(0, 0, 0);

            // get Alignment (id)

            ObjectId alignId = ObjectId.Null;
            alignId = SelectAlignment();
            if (alignId == ObjectId.Null) return;

           ptOffAlignment = ed.GetPoint("Get point").Value.TransformBy(ed.CurrentUserCoordinateSystem);

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // get the alignment from alignId
                    Alignment align = (Alignment)alignId.GetObject(OpenMode.ForRead);
                    easting = ptOffAlignment.X;
                    northing = ptOffAlignment.Y;
                    align.StationOffset(easting, northing, ref station, ref offset);
                    align.PointLocation(station, 0, 0, ref easting, ref northing, ref bearing);
                    ptOnAlignment = new Point3d(easting, northing, 0);

                    double angle = Math.PI/2 - bearing;
                    if (angle < 0) angle = angle + 2 * Math.PI;

                    ed.WriteMessage("Station: {0} and Bearing: {1} Angle: {2}", station, bearing, angle);

                }
                catch (PointNotOnEntityException)
                // The point is outside of the alignment boundaries
                {
                    inRange = false;
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    ed.WriteMessage(ex.Message);
                    inRange = false;
                }
            }
        }

        public static ObjectId SelectAlignment()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ObjectId result = ObjectId.Null;

            PromptEntityOptions entOpts = new PromptEntityOptions("\nSelect alignment: ");
            entOpts.SetRejectMessage("...not an Alignment, try again!:");
            entOpts.AddAllowedClass(typeof(Alignment), true);

            PromptEntityResult entRes = ed.GetEntity(entOpts);

            if (entRes.Status == PromptStatus.OK) result = entRes.ObjectId;

            return result;
        }


    }

}
