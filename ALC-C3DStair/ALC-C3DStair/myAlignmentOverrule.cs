
// (C) Copyright 2018 by Andreas Luka
//

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using System;



namespace ALC_C3DStair
{
    public class DrawableOverruleAlignment : DrawableOverrule
    {

        CivilDocument civdoc = CivilApplication.ActiveDocument;

        int steps = 10;
        double width = 2;
        double tread = 0.32;
        double landing = 0;
        double riser = 0.16;
        double elevation = 0;
        bool reverse = false;

        public override bool WorldDraw(Drawable drawable, WorldDraw wd)
        {
            double easting = Double.NaN;
            double northing = Double.NaN;
            double bearing = Double.NaN;
            double angle = Double.NaN;

            // draw the base class
            bool result = base.WorldDraw(drawable, wd);

            Alignment align = (Alignment)drawable;

            if ((align.Length > tread) && MyFunctions.GetStairPropertiesFromAlignment(align, ref tread, ref riser, ref landing, ref width, ref steps, ref reverse, ref elevation))
            {

                int maxSteps = (int)(align.Length / tread);
                MyFunctions.SetStairPropertiesToAlignment(align, tread, riser, landing, width, maxSteps, reverse, elevation);

                if (MyFunctions.IsPlanView())
                {
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

                    // Arrow
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

                }
                else
                {
                    // draw 3D
                    for (int i = 0; i <= maxSteps-1; i++)
                    {
                        align.PointLocation(align.StartingStation + i * tread, 0 ,0, ref easting, ref northing, ref bearing);

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
                            ptLow = new Point3d(easting, northing, elevation + (maxSteps-i-1) * riser);

                            ptHeigh = new Point3d(easting, northing, elevation + (maxSteps- i) * riser);
                            angle = angle-Math.PI;
                        }
                        else
                        {
                            ptLow = new Point3d(easting, northing, elevation + i * riser);
                            ptHeigh = new Point3d(easting, northing,  elevation +(i + 1) * riser);
                        }
                        wd.Geometry.WorldLine(ptLow, ptHeigh);


                        Point3d ptBoxCentroid = new Point3d(ptLow.X+tread/2, ptLow.Y, ptLow.Z + riser / 2);
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




        public override bool IsApplicable(RXObject overruledSubject)
        {
            Alignment align = overruledSubject as Alignment;
            if (align == null) return false;
            else return (MyFunctions.IsStairPropertySetOnAlignment(align));
        }

 

    }
}
