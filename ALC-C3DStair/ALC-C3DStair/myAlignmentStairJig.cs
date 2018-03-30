// (C) Copyright 2018 by Andreas Luka

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;

using Autodesk.Civil;
using Autodesk.Civil.DatabaseServices;



namespace ALC_C3DStair
{
    class StationJig : DrawJig
    {
        int steps = 5;
        double width = 2;
        double tread = 0.32;
        double riser = 0.16;

        public int jigStatus = 0;  //0  = StartPoint :: 1 = Endpoint

        public double stationStart = Double.NaN;
        public double stationEnd = Double.NaN;

        double offset = Double.NaN;
        double easting = 0;
        double northing = 0;

        bool inRange;

        Alignment align;
        Point3d ptOnAlignment = new Point3d(0, 0, 0);
        Point3d ptOffAlignment = new Point3d(0, 0, 0);

        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


        protected override bool WorldDraw(WorldDraw wd)
        {
            if (inRange)

                switch (jigStatus)
                {
                    case 0:
                        wd.Geometry.WorldLine(ptOnAlignment, ptOffAlignment);
                        return inRange;
                    case 1:
                        wd.Geometry.WorldLine(ptOnAlignment, ptOffAlignment);

                        double length = Math.Abs(stationEnd - stationStart);
                        int rev = 1;
                        if (stationEnd < stationStart) rev = -1;

                        if (length > tread)
                        {

                            int maxSteps = (int)(length / tread);

                            for (int i = 0; i <= maxSteps; i++)
                            {
                                align.PointLocation(stationStart+ rev * i * tread, width / 2, ref easting, ref northing);
                                Point3d ptLeft = new Point3d(easting, northing, 0);

                                align.PointLocation(stationStart + rev * i * tread, -width / 2, ref easting, ref northing);
                                Point3d ptRight = new Point3d(easting, northing, 0);

                                wd.Geometry.WorldLine(ptLeft, ptRight);
                            }
                        }
                        return inRange;
                    default:
                        return inRange;
                }
            return inRange;
        }


        protected override SamplerStatus Sampler(JigPrompts prompts)
        {

            switch (jigStatus)
            {
                case 0:
                    JigPromptPointOptions opts1 = new JigPromptPointOptions("\nSpecify first station along alignment: ");
                    PromptPointResult ppr1 = prompts.AcquirePoint(opts1);

                    if (ppr1.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

                    if (ppr1.Value.DistanceTo(ptOffAlignment) < Tolerance.Global.EqualPoint)
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        ptOffAlignment = ppr1.Value;

                        //finding the perpendicular point on the alignment
                        try
                        {
                            easting = ptOffAlignment.X;
                            northing = ptOffAlignment.Y;
                            align.StationOffset(easting, northing, ref stationStart, ref offset);
                            align.PointLocation(stationStart, 0, ref easting, ref northing);
                            ptOnAlignment = new Point3d(easting, northing, 0);
                            inRange = true;
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
                    return SamplerStatus.OK;


                case 1:
                    JigPromptPointOptions opts2 = new JigPromptPointOptions("\nSpecify second station along alignment:");
                    PromptPointResult ppr2 = prompts.AcquirePoint(opts2);

                    if (ppr2.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

                    if (ppr2.Value.DistanceTo(ptOffAlignment) < Tolerance.Global.EqualPoint)
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        ptOffAlignment = ppr2.Value;

                        //finding the perpendicular point on the alignment
                        try
                        {
                            easting = ptOffAlignment.X;
                            northing = ptOffAlignment.Y;
                            align.StationOffset(easting, northing, ref stationEnd, ref offset);
                            align.PointLocation(stationEnd, 0, ref easting, ref northing);
                            ptOnAlignment = new Point3d(easting, northing, 0);
                            inRange = true;
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
                    return SamplerStatus.OK;
                default:
                    return SamplerStatus.OK;
            }
        }


        public StationJig(Alignment align)
        {
            // get the alignment from the id
            this.align = align;
        }

    }
}
