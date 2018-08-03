// (C) Copyright 2018 by Andreas Luka
// Overruling Alignment for creating a Staircase 
// For Testing ParamaterSet for storing attached infornmation is replaced by fixed numbers
// For Testing Filter for Alignment Style, and Attached ParameterSet removed 
//
// Turning Overrule On with OSTAIR1
// Turning Overrule OFF with OSTAIR0
// 

using System;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;


namespace DrawOverrule
{
    public class TransformOverruleAlignment : TransformOverrule
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        static public TransformOverrule transOverrule = new TransformOverruleAlignment();


    }

    public class DrawOverrule : DrawableOverrule
    {
        // Added for Testing
        // Creating and disposing overrule are part of myPlugins Init 
        static public DrawOverrule theOverrule = new DrawOverrule();


        public override bool WorldDraw(Drawable d, WorldDraw wd)
        {
            // just for testing
            // will be replaced by read and write parameter set
            double width = 2;
            double tread = 0.32;
            double riser = 0.16;
            double elevation = 5;
            bool reverse = false;

            
            // Init
            double easting = Double.NaN;
            double northing = Double.NaN;
            double bearing = Double.NaN;
            double angle = Double.NaN;
            bool result; 
            
            
            // Draw the base class
            result = base.WorldDraw(d, wd);


            if (d is Alignment)
            {
                // cast drawable as alignment
                Alignment align = (Alignment)d;

                if (align.Length > tread)
                {
                    int maxSteps = (int)(align.Length / tread);

                    // draw 2D
                    for (int i = 0; i <= maxSteps; i++)
                    {
                        align.PointLocation(align.StartingStation + i * tread, width / 2, ref easting, ref northing);
                        Point3d ptLeft = new Point3d(easting, northing, 0);

                        align.PointLocation(align.StartingStation + i * tread, -width / 2, ref easting, ref northing);
                        Point3d ptRight = new Point3d(easting, northing, 0);

                        wd.Geometry.WorldLine(ptLeft, ptRight);
                    };

                    for (int i = 0; i <= maxSteps - 1; i++)
                    {
                        align.PointLocation(align.StartingStation + i * tread, width / 2, ref easting, ref northing);
                        Point3d ptL1 = new Point3d(easting, northing, 0);
                        align.PointLocation(align.StartingStation + (i + 1) * tread, width / 2, ref easting, ref northing);
                        Point3d ptL2 = new Point3d(easting, northing, 0);
                        wd.Geometry.WorldLine(ptL1, ptL2);

                        align.PointLocation(align.StartingStation + i * tread, -width / 2, ref easting, ref northing);
                        Point3d ptR1 = new Point3d(easting, northing, 0);
                        align.PointLocation(align.StartingStation + (i + 1) * tread, -width / 2, ref easting, ref northing);
                        Point3d ptR2 = new Point3d(easting, northing, 0);
                        wd.Geometry.WorldLine(ptR1, ptR2);
                    };

                    // Draw Arrow
                    if (reverse)
                    {
                        // second step
                        align.PointLocation(align.StartingStation + tread, 0, ref easting, ref northing);
                        Point3d ptL1 = new Point3d(easting, northing, 0);

                        align.PointLocation(align.StartingStation + 1.5 * tread, tread / 2, ref easting, ref northing);
                        Point3d ptL2 = new Point3d(easting, northing, 0);

                        align.PointLocation(align.StartingStation + 1.5 * tread, -tread / 2, ref easting, ref northing);
                        Point3d ptL3 = new Point3d(easting, northing, 0);

                        wd.Geometry.WorldLine(ptL1, ptL2);
                        wd.Geometry.WorldLine(ptL1, ptL3);
                    }
                    else
                    {
                        // before last step
                        align.PointLocation(align.StartingStation + (maxSteps - 1) * tread, 0, ref easting, ref northing);
                        Point3d ptL1 = new Point3d(easting, northing, 0);

                        align.PointLocation(align.StartingStation + (maxSteps - 1) * tread - tread / 2, tread / 2, ref easting, ref northing);
                        Point3d ptL2 = new Point3d(easting, northing, 0);

                        align.PointLocation(align.StartingStation + (maxSteps - 1) * tread - tread / 2, -tread / 2, ref easting, ref northing);
                        Point3d ptL3 = new Point3d(easting, northing, 0);

                        wd.Geometry.WorldLine(ptL1, ptL2);
                        wd.Geometry.WorldLine(ptL1, ptL3);
                    }

                    // draw 3D
                    for (int i = 0; i <= maxSteps - 1; i++)
                    {
                        align.PointLocation(align.StartingStation + i * tread, 0, 0, ref easting, ref northing, ref bearing);

                        // change bearing to angle
                        angle = Math.PI / 2 - bearing;
                        if (angle < 0) angle = angle + 2 * Math.PI;

                        Point3d ptLow;
                        Point3d ptHeigh;
                        Matrix3d displacement;
                        Matrix3d rotation;

                        Vector3d disp;
                        if (reverse)
                        {
                            ptLow = new Point3d(easting, northing, elevation + (maxSteps - i - 1) * riser);
                            ptHeigh = new Point3d(easting, northing, elevation + (maxSteps - i) * riser);
                            angle = angle - Math.PI;
                        }
                        else
                        {
                            ptLow = new Point3d(easting, northing, elevation + i * riser);
                            ptHeigh = new Point3d(easting, northing, elevation + (i + 1) * riser);
                        }

                        // steps
                        Point3d ptBoxCentroid = new Point3d(ptLow.X + tread / 2, ptLow.Y, ptLow.Z + riser / 2);
                        disp = Point3d.Origin.GetVectorTo(ptBoxCentroid);

                        displacement = Matrix3d.Displacement(disp);
                        rotation = Matrix3d.Rotation(angle, Vector3d.ZAxis, ptLow);

                        Solid3d solidStep = new Solid3d();
                        solidStep.CreateBox(tread, width, riser);
                        solidStep.TransformBy(displacement);
                        solidStep.TransformBy(rotation);
                        solidStep.WorldDraw(wd);
                        solidStep.Dispose();
                    }
                }
            }
            return result;
        }
    }

    public class Commands
    {


        // Commands for Testing 
        [CommandMethod("OSTAIR1")]
        public void OverruleStart()
        {
            ObjectOverrule.AddOverrule(RXClass.GetClass(typeof(Drawable)), DrawOverrule.theOverrule, true);
            DrawableOverrule.Overruling = true;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("REGEN3\n", true, false, false);
            doc.Editor.Regen();
        }

        [CommandMethod("OSTAIR0")]
        public void OverruleEnd()
        {
            DrawableOverrule.Overruling = false;
            Overrule.RemoveOverrule(RXClass.GetClass(typeof(Alignment)), DrawOverrule.theOverrule);
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.SendStringToExecute("REGEN3\n", true, false, false);
            doc.Editor.Regen();
        }

    }

}