using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALCJIG2.MyCommands))]

namespace ALCJIG2
{
    public class MyCommands
    {

        [CommandMethod("CJ")]
        public void DragCircle()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            try
            {

                using (Transaction tr = db.TransactionManager.StartTransaction())
                using (Circle circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 3.0))
                {
                    CircleJig jig = new CircleJig(circle);
                    PromptResult pr = ed.Drag(jig);
                    if (pr.Status == PromptStatus.OK)
                    {
                        circle.TransformBy(jig.Displacement);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                        btr.AppendEntity(circle);
                        tr.AddNewlyCreatedDBObject(circle, true);
                    }
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(ex.ToString());
            }
        }



        [CommandMethod("LJ")]
        public void DragLine()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            LineJig jig = new LineJig();

        }



        [CommandMethod("BIRJ")]
        public void InsertRotateBlock()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {

                    // Get the block
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord block = (BlockTableRecord)tr.GetObject(bt["TEST"], OpenMode.ForRead);

                    BlockReference br = new BlockReference(Point3d.Origin, block.ObjectId);

                    // Create the Jig and ask the user to place the block
                    BlockJig jig = new BlockJig(br);
                    PromptResult pr;

                    do
                    {
                        pr = ed.Drag(jig);
                    }

                    while (pr.Status != PromptStatus.Cancel && pr.Status != PromptStatus.Error && jig.jigStatus++ <= 1);

                    if (pr.Status == PromptStatus.OK)
                    {
                        br.TransformBy(jig.Displacement);
                        br.TransformBy(jig.Rotation);
                        BlockTableRecord curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                        curSpace.AppendEntity(br);
                        tr.AddNewlyCreatedDBObject(br, true);
                    }
                    tr.Commit();
                }


            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(ex.ToString());
            }
        }


        [CommandMethod("SIRJ")]
        public void InsertRotateSolid()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {

                    // Create the Solid
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                    Solid3d br = new Solid3d();
                    br.CreateBox(1, 1, 1);

                    // Create the Jig and ask the user to place the solid
                    SolidJig jig = new SolidJig(br);
                    PromptResult pr;


                    do
                    {
                        pr = ed.Drag(jig);
                    }

                    while (pr.Status != PromptStatus.Cancel && pr.Status != PromptStatus.Error && jig.jigStatus++ <= 1);

                    if (pr.Status == PromptStatus.OK)
                    {
                        br.TransformBy(jig.Displacement);
                        br.TransformBy(jig.Rotation);
                        BlockTableRecord curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                        curSpace.AppendEntity(br);
                        tr.AddNewlyCreatedDBObject(br, true);
                    }
                    else { } // dispose solid
                    tr.Commit();
                }


            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(ex.ToString());
            }
        }


        // Modal Command with pickfirst selection
        [CommandMethod("MyGroup", "ALCStation", CommandFlags.Modal | CommandFlags.UsePickSet)]

        public void AlignmentStationTest()
        {
            // Document doc = Application.DocumentManager.MdiActiveDocument;

            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


            ObjectId alignId = MyFunctions.SelectAlignment();

            if (alignId == ObjectId.Null) return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // get the alignment from alignId
                Alignment align = (Alignment)alignId.GetObject(OpenMode.ForRead);

                // Create the Jig and ask the user to place the block
                StationJig jig = new StationJig(align);
                PromptResult pr;
                do
                {
                    pr = ed.Drag(jig);
                }
                while (pr.Status != PromptStatus.Cancel && pr.Status != PromptStatus.Error);
            }
        }

    }
}
