// (C) Copyright 2018 by Andreas Luka (Lu An Jie)
//

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

using static LX_Anchor.MyFunctions;
using acEntity = Autodesk.AutoCAD.DatabaseServices.Entity; //alias for AutoCAD entities to avoid ambigues reference between civil3d and autocad db



// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(LX_Anchor.MyCommands))]

namespace LX_Anchor
{

    public class MyCommands
    {
        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        CivilDocument civdoc = CivilApplication.ActiveDocument;
        string keyAnchor = "anchor";


        [CommandMethod("MyGroup", "LXAddAnchor", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void AddAnchorToAlignment()
        {
            ed.WriteMessage("\n Command started");

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // select object to anchor
                    HashSet<Type> validTypes = new HashSet<Type>() { typeof(Circle), typeof(CogoPoint) };

                    ObjectId idAnchored = MyFunctions.SelectEntity(validTypes);
                    if (idAnchored == ObjectId.Null) return;
                    acEntity enAnchored = (acEntity)idAnchored.GetObject(OpenMode.ForWrite);

                    Point3d ptAnchored = Point3d.Origin;

                    // defining anchor points

                    if (enAnchored is Circle c)
                    {
                        ptAnchored = c.Center;
                        ed.WriteMessage(ptAnchored.ToString());
                    }
                    if (enAnchored is CogoPoint cp)
                    {
                        ptAnchored = new Point3d(cp.Northing, cp.Easting, 0);
                        ed.WriteMessage(ptAnchored.ToString());
                    }

                    //select alignment to anchor on
                    ObjectId idAlignment = MyFunctions.SelectAlignment();
                    if (idAlignment == ObjectId.Null) return;
                    Alignment alHost = (Alignment)idAlignment.GetObject(OpenMode.ForRead);


                    // create the jig to find point on alignment
                    StationJig jig = new StationJig(alHost, enAnchored, ptAnchored);
                    PromptResult pr;
                    do
                    {
                        pr = ed.Drag(jig);
                        if (pr.Status == PromptStatus.Cancel) return;
                    }
                    while (pr.Status != PromptStatus.OK);

                    if (pr.Status == PromptStatus.OK)
                    {
                        //move point to new location

                        enAnchored.TransformBy(jig.matDisplacement);

                        ResultBuffer rbHost = ReadXRecord(idAlignment, keyAnchor);
                        ResultBuffer rbAnchored = ReadXRecord(idAnchored, keyAnchor);

                        if (rbAnchored == null) // Object not anchored
                        {
                            ed.WriteMessage("\n new anchor ");

                            rbAnchored = new ResultBuffer(new TypedValue((int)DxfCode.SoftPointerId, idAlignment), new TypedValue((int)DxfCode.Real, jig.station));

                            if (rbHost == null) rbHost = new ResultBuffer(new TypedValue((int)DxfCode.SoftPointerId, idAnchored));
                            else rbHost.Add(new TypedValue((int)DxfCode.SoftPointerId, idAnchored));

                            WriteXRecord(idAlignment, keyAnchor, rbHost);
                            WriteXRecord(idAnchored, keyAnchor, rbAnchored);
                        }
                        else // Object has already an anchor
                        {
                            TypedValue[] arrTVAnchor = rbAnchored.AsArray();
                            ObjectId idHostExisting = (ObjectId)arrTVAnchor[0].Value;

                            ed.WriteMessage("\n Anchor exist on:" + idAlignment.ToString());


                            if (idHostExisting == idAlignment)
                            {
                                rbAnchored = new ResultBuffer(new TypedValue((int)DxfCode.SoftPointerId, idAlignment), new TypedValue((int)DxfCode.Real, jig.station));
                                WriteXRecord(idAnchored, keyAnchor, rbAnchored);
                            }
                            else
                            {
                                ed.WriteMessage("\n Moving Point from " + idHostExisting.ToString() + "to: " + idAlignment.ToString());

                                rbAnchored = new ResultBuffer(new TypedValue((int)DxfCode.SoftPointerId, idAlignment), new TypedValue((int)DxfCode.Real, jig.station));
                                WriteXRecord(idAnchored, keyAnchor, rbAnchored);

                                if (rbHost == null) rbHost = new ResultBuffer(new TypedValue((int)DxfCode.SoftPointerId, idAnchored));
                                else rbHost.Add(new TypedValue((int)DxfCode.SoftPointerId, idAnchored));
                                WriteXRecord(idAlignment, keyAnchor, rbHost);
                                ed.WriteMessage("\n Anchor added to: " + idAlignment.ToString() + " ," + rbHost.ToString());

                                ed.WriteMessage("\n Updating old Alignment: "+ idHostExisting.ToString());

                                ResultBuffer rbHostExisting = ReadXRecord(idHostExisting, keyAnchor);
                                ResultBuffer rbUpdated = new ResultBuffer ();
                                Boolean isEmpty = true;

                                foreach (TypedValue tv in rbHostExisting.AsArray())
                                {
                                    if (idAnchored != (ObjectId)tv.Value)
                                    {
                                        rbUpdated.Add (tv);
                                        ed.WriteMessage("\n Anchor to keep:" + idAnchored.ToString() + " ,"+ tv.ToString());
                                        isEmpty = false;
                                    }
                                    else ed.WriteMessage("\n Anchor removed " + idAnchored.ToString() + " ," + tv.ToString());
                                };
                                if (isEmpty)
                                {
                                    DeleteXRecord(idHostExisting, keyAnchor);
                                    ed.WriteMessage("\n Empty record removed");
                                }
                                else
                                {
                                    WriteXRecord(idHostExisting, keyAnchor, rbUpdated);
                                }
                            }
                        }
                        tr.Commit();
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    ed.WriteMessage("\n Error after selection: {0}", ex.Message);
                }
            }
        }

        [CommandMethod("MyGroup", "LXMovePt", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void MovePtToAlignment()
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {

                    //select alignment to anchor on
                    ObjectId idAlignment = MyFunctions.SelectAlignment();
                    if (idAlignment == ObjectId.Null) return;
                    Alignment alHost = (Alignment)idAlignment.GetObject(OpenMode.ForRead);

                    // select object to anchor
                    HashSet<Type> validTypes = new HashSet<Type>() { typeof(CogoPoint) };
                    ObjectId idAnchored = MyFunctions.SelectEntity(validTypes);

                    if (idAnchored == ObjectId.Null) return;

                    CogoPoint cp = (CogoPoint)idAnchored.GetObject(OpenMode.ForWrite) as CogoPoint;

                    // create the jig to find point on alignment
                    CogoPointJig jig = new CogoPointJig(cp, new Point3d (cp.Easting, cp.Northing, 0));
                    PromptResult pr = ed.Drag(jig);
                    if (pr.Status == PromptStatus.Cancel) return;


                    if (pr.Status == PromptStatus.OK)
                    {
                        //moving point
                        cp.Easting = jig.easting;
                        cp.Northing = jig.northing;
                        tr.Commit();
                    }

                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    ed.WriteMessage("\n Error after selection: {0}", ex.Message);
                }
            }
        }
    }
}

