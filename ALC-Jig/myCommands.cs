
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_Jig.MyCommands))]


namespace ALC_Jig
{
    public class MyCommands
    {

        [CommandMethod("TestEntityJigger7")]
        public static void TestEntityJigger7_Method()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;

            PromptResult pr = ed.GetString("\nBlock name:");
            if (pr.Status == PromptStatus.OK)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[pr.StringResult], OpenMode.ForRead) as BlockTableRecord;
                    if (btr != null)
                    {
                        BlockReference ent = new BlockReference(new Point3d(0, 0, 0), btr.ObjectId);
                        if (BlockMovingRotating.Jig(ent))
                        {
                            BlockTableRecord modelspace = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                            modelspace.AppendEntity(ent);
                            tr.AddNewlyCreatedDBObject(ent, true);
                            tr.Commit();
                        }
                        else
                        {
                            tr.Abort();
                        }
                    }
                }
            }
        }
    }
}
