using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace ALC_Stair.C3D
{
    class myFunctions
    {

        public void SetXrecord(ObjectId id, string key, ResultBuffer rb)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent != null)
                {
                    ent.UpgradeOpen();
                    ent.CreateExtensionDictionary();
                    DBDictionary xd = (DBDictionary)tr.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite);
                    Xrecord xr = new Xrecord();
                    xr.Data = rb;
                    xd.SetAt(key, xr);

                    tr.AddNewlyCreatedDBObject(xr, true);
                }
                tr.Commit();
            }
        }

        public ResultBuffer GetXrecord(ObjectId id, string key)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            // ResultBuffer res = new ResultBuffer();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Xrecord xr = new Xrecord();
                Entity ent = tr.GetObject(id, OpenMode.ForRead, false) as Entity;
                if (ent != null)
                {
                    try
                    {
                        DBDictionary xDict = (DBDictionary)tr.GetObject(ent.ExtensionDictionary, OpenMode.ForRead, false);
                        xr = (Xrecord)tr.GetObject(xDict.GetAt(key), OpenMode.ForRead, false);
                        return xr.Data;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else return null;
            }
        }


        public static void createStair3D(int numStairs, double riser, double tread, double landing, double width)
        {
            // Get the current document and database, and start a transaction
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // Open the Block table record for read
                BlockTable blkTable;
                blkTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord blkTblRec;
                blkTblRec = tr.GetObject(blkTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a 3D polyline 
                using (Polyline3d pline3D = new Polyline3d())
                {
                    // Add the new object to the block table record and the transaction
                    // Before adding vertexes, the polyline must be in the drawing

                    blkTblRec.AppendEntity(pline3D);
                    tr.AddNewlyCreatedDBObject(pline3D, true);

                    Point3dCollection pnts = new Point3dCollection();

                    // Create points for 3d polyline
                    pnts.Add(new Point3d(0, 0, 0));

                    for (int i = 1; i <= numStairs; i++)
                    {
                        pnts.Add(new Point3d(0, (i - 1) * tread, i * riser));
                        pnts.Add(new Point3d(0, i * tread, i * riser));
                    };

                    pnts.Add(new Point3d(0, numStairs * 0.30, (numStairs - 1) * riser));
                    pnts.Add(new Point3d(0, tread, 0));

                    // Creating vertexes from points
                    foreach (Point3d pnt in pnts)
                    {
                        using (PolylineVertex3d vert = new PolylineVertex3d(pnt))
                        {
                            pline3D.AppendVertex(vert);
                            tr.AddNewlyCreatedDBObject(vert, true);
                        }
                    }
                    pline3D.Closed = true;

                    DBObjectCollection lines = new DBObjectCollection();
                    pline3D.Explode(lines);

                    // Create a region from the set of lines.
                    DBObjectCollection reg = new DBObjectCollection();
                    reg = Region.CreateFromCurves(lines);
                    Region pRegion = (Region)reg[0];

                    // Create a Polyline for sweep path
                    Polyline pline = new Polyline();
                    pline.AddVertexAt(0, new Point2d(-width/2, 0), 0, 0, 0);
                    pline.AddVertexAt(1, new Point2d(width/2, 0), 0, 0, 0);

                    // createStair3D sweep options 
                    SweepOptionsBuilder sob = new SweepOptionsBuilder();

                    sob.Align = SweepOptionsAlignOption.AlignSweepEntityToPath;
                    sob.BasePoint = pline.StartPoint;
                    sob.Bank = true;

                    using (Solid3d stair3d = new Solid3d())
                    {
                        stair3d.RecordHistory = true;
                        stair3d.CreateSweptSolid(pRegion, pline, sob.ToSweepOptions());
                        blkTblRec.AppendEntity(stair3d);
                        tr.AddNewlyCreatedDBObject(stair3d, true);
                        
                        // Extension dictionary
                        DBObject dbObj = stair3d;
                        ObjectId extId = dbObj.ExtensionDictionary;
                        
                        dbObj.UpgradeOpen();
                        dbObj.CreateExtensionDictionary();
                        extId = dbObj.ExtensionDictionary;
 
                        DBDictionary dbExt = (DBDictionary)tr.GetObject(extId, OpenMode.ForRead);

                        dbExt.UpgradeOpen();
                        Xrecord xr = new Xrecord();
                        ResultBuffer rb = new ResultBuffer();
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataInteger32, numStairs));
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataReal, tread));
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataReal, riser));
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataReal, landing));
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataReal, width));

                        xr.Data = rb;
                        dbExt.SetAt("StairCase", xr);
                        tr.AddNewlyCreatedDBObject(xr, true);

                    };
 
                    // Deleting the 3d polyline
                    pline3D.Erase(true);
                }

                // Save the new object to the database
                tr.Commit();
            }
        }
    }
}