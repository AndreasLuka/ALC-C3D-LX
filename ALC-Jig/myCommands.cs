
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using System;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_Jig.MyCommands))]


namespace ALC_Jig
{
    public class MyCommands
    {

        [CommandMethod("AlignmentTest")]
        public void alignmenttest()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            ObjectId alignId = SelectAlignment(doc.Editor);

            if (alignId == ObjectId.Null)
                return;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Alignment align = (Alignment)alignId.GetObject(OpenMode.ForRead);

                double station = Double.NaN;
                double offset = Double.NaN;
                double easting = 0;
                double northing = 0;
 
                PromptPointOptions ptOpts = new PromptPointOptions("\nPick start point for stair:");
                ptOpts.AllowNone = true;
                ptOpts.AllowArbitraryInput = false;

                PromptPointResult ptRes = doc.Editor.GetPoint(ptOpts);

                    try
                    {
                        align.StationOffset(ptRes.Value.X, ptRes.Value.Y, ref station, ref offset);
                        align.PointLocation(station, 0, ref easting, ref northing);

                        Point3d p = new Point3d(easting, northing,0);
                        using (Circle acCirc = new Circle())
                        {
                            acCirc.Center = p;
                            acCirc.Radius = 100;

                            // Open the Block table for read
                            BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                            // Open the Block table record Model space for write
                            BlockTableRecord blkTblRec = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                            // Add the new object to the block table record and the transaction
                            blkTblRec.AppendEntity(acCirc);
                            tr.AddNewlyCreatedDBObject(acCirc, true);

                        }

                    }
                    catch (Autodesk.Civil.CivilException ex)
                    {
                        string msg = ex.Message;
                    }
                tr.Commit();

                
            }
        }

        private ObjectId SelectAlignment(Editor ed)
        {
            ObjectId result = ObjectId.Null;
            PromptEntityOptions entOpts = new PromptEntityOptions("\nSelect alignment: ");
            entOpts.SetRejectMessage("...not an Alignment, try again!:");
            entOpts.AddAllowedClass(typeof(Alignment), true);
            PromptEntityResult entRes = ed.GetEntity(entOpts);
            if (entRes.Status == PromptStatus.OK)
                result = entRes.ObjectId;
            return result;
        }


    }

}
