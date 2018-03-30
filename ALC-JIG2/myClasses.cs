using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

using Autodesk.Civil;
using Autodesk.Civil.DatabaseServices;
using System;


namespace ALCJIG2
{
    class CircleJig : DrawJig
    {
        private Circle circle;
        private Point3d center;

        public Matrix3d Displacement { get; private set; }

        public CircleJig(Circle circle)
        {
            this.circle = circle;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                geo.PushModelTransform(Displacement);
                geo.Draw(circle);
                geo.PopModelTransform();
            }
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions opts = new JigPromptPointOptions("\nSpecify the circle center: ")
            {
                UserInputControls = UserInputControls.NullResponseAccepted
            };

            PromptPointResult pr = prompts.AcquirePoint(opts);
            if (pr.Value.DistanceTo(center) < Tolerance.Global.EqualPoint)
                return SamplerStatus.NoChange;
            center = pr.Value;
            Vector3d disp = Point3d.Origin.GetVectorTo(center);
            Displacement = Matrix3d.Displacement(disp);
            return SamplerStatus.OK;
        }
    }



    class LineJig : DrawJig
    {
        Point3d StartPoint;
        Point3d EndPoint;
        Editor MgdEditor = Application.DocumentManager.MdiActiveDocument.Editor;

        protected override bool WorldDraw(WorldDraw draw)
        {
            return draw.Geometry.WorldLine(StartPoint, EndPoint);
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            EndPoint = prompts.AcquirePoint("\nEnd point").Value;
            return SamplerStatus.OK;
        }

        public LineJig()
        {
            StartPoint = MgdEditor.GetPoint("Start point").Value.TransformBy(MgdEditor.CurrentUserCoordinateSystem);
            MgdEditor.Drag(this);
            MgdEditor.WriteMessage(string.Format("\nLine with start point {0} and end point {1} has been jigged.", StartPoint, EndPoint));
        }
    }

    class BlockJig : DrawJig
    {
        private BlockReference br;
        // public Point3d basePoint;
        public Point3d insertPoint;
        // public Double baseAngle;
        public Double insertAngle;

        public Matrix3d Displacement { get; private set; }
        public Matrix3d Rotation { get; private set; }

        public int jigStatus = 0;  //0  = Insert :: 1 = Rotation

        public BlockJig(BlockReference br)
        {
            this.br = br;
            // this.basePoint = br.Position;
            // this.baseAngle = br.Rotation;
        }


        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (jigStatus)
            {
                case 0:
                    JigPromptPointOptions opts1 = new JigPromptPointOptions("\nSpecify the insert point: ")
                    {
                        UserInputControls = UserInputControls.NullResponseAccepted
                    };
                    PromptPointResult ppr1 = prompts.AcquirePoint(opts1);

                    if (ppr1.Value.DistanceTo(insertPoint) < Tolerance.Global.EqualPoint)
                        return SamplerStatus.NoChange;

                    insertPoint = ppr1.Value;
                    Vector3d disp = Point3d.Origin.GetVectorTo(insertPoint);
                    Displacement = Matrix3d.Displacement(disp);
                    return SamplerStatus.OK;

                case 1:
                    JigPromptAngleOptions opts2 = new JigPromptAngleOptions("\nSpecify rotation angle:")
                    {
                        BasePoint = insertPoint,
                        UseBasePoint = true,
                        Cursor = CursorType.RubberBand
                    };
                    PromptDoubleResult ppr2 = prompts.AcquireAngle(opts2);

                    if (ppr2.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

                    if (ppr2.Value.Equals(insertAngle))
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        insertAngle = ppr2.Value;
                        Rotation = Matrix3d.Rotation(insertAngle - 0, Vector3d.ZAxis, insertPoint);
                        return SamplerStatus.OK;
                    }
                default: break;
            }
            return SamplerStatus.OK;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {

            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                switch (jigStatus)
                {
                    case 0:
                        geo.PushModelTransform(Displacement);
                        geo.Draw(br);
                        geo.PopModelTransform();
                        break;
                    case 1:
                        // geo.PushModelTransform(Displacement);
                        geo.PushModelTransform(Displacement.PreMultiplyBy(Rotation));
                        geo.Draw(br);
                        geo.PopModelTransform();
                        // geo.PopModelTransform();
                        break; ;
                    default: return false;
                }
            }
            return true;
        }
    }

    class SolidJig : DrawJig
    {
        private Solid3d br;
        // public Point3d basePoint;
        public Point3d insertPoint;
        // public Double baseAngle;
        public Double insertAngle;

        public Matrix3d Displacement { get; private set; }
        public Matrix3d Rotation { get; private set; }

        public int jigStatus = 0;  //0  = Insert :: 1 = Rotation

        public SolidJig(Solid3d br)
        {
            this.br = br;
        }


        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (jigStatus)
            {
                case 0:
                    JigPromptPointOptions opts1 = new JigPromptPointOptions("\nSpecify the insert point: ")
                    {
                        UserInputControls = UserInputControls.NullResponseAccepted
                    };
                    PromptPointResult ppr1 = prompts.AcquirePoint(opts1);

                    if (ppr1.Value.DistanceTo(insertPoint) < Tolerance.Global.EqualPoint)
                        return SamplerStatus.NoChange;

                    insertPoint = ppr1.Value;
                    Vector3d disp = Point3d.Origin.GetVectorTo(insertPoint);
                    Displacement = Matrix3d.Displacement(disp);
                    return SamplerStatus.OK;

                case 1:
                    JigPromptAngleOptions opts2 = new JigPromptAngleOptions("\nSpecify rotation angle:")
                    {
                        BasePoint = insertPoint,
                        UseBasePoint = true,
                        Cursor = CursorType.RubberBand
                    };
                    PromptDoubleResult ppr2 = prompts.AcquireAngle(opts2);

                    if (ppr2.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

                    if (ppr2.Value.Equals(insertAngle))
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        insertAngle = ppr2.Value;
                        Rotation = Matrix3d.Rotation(insertAngle - 0, Vector3d.ZAxis, insertPoint);
                        return SamplerStatus.OK;
                    }
                default: break;
            }
            return SamplerStatus.OK;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {

            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                switch (jigStatus)
                {
                    case 0:
                        geo.PushModelTransform(Displacement);
                        geo.Draw(br);
                        geo.PopModelTransform();
                        break;
                    case 1:
                        // geo.PushModelTransform(Displacement);
                        geo.PushModelTransform(Displacement.PreMultiplyBy(Rotation));
                        geo.Draw(br);
                        geo.PopModelTransform();
                        // geo.PopModelTransform();
                        break; ;
                    default: return false;
                }
            }
            return true;
        }
    }


    class StationJig : DrawJig
    {

        public double station = Double.NaN;

        double offset = Double.NaN;
        double easting = 0;
        double northing = 0;
        bool inRange;

        Alignment align;
        Point3d startPoint = new Point3d(0, 0, 0);
        Point3d endPoint = new Point3d(0, 0, 0);

        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


        protected override bool WorldDraw(WorldDraw draw)
        {
            if (inRange) draw.Geometry.WorldLine(startPoint, endPoint);
            return inRange;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {

            JigPromptPointOptions opts1 = new JigPromptPointOptions("\nSpecify station along baseline: ");

            PromptPointResult ppr1 = prompts.AcquirePoint(opts1);

            if (ppr1.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

            if (ppr1.Value.DistanceTo(endPoint) < Tolerance.Global.EqualPoint)
            {
                return SamplerStatus.NoChange;
            }
            else
            {
                endPoint = ppr1.Value;

                //finding the perpendicular point on the alignment
                try
                {
                    easting = endPoint.X;
                    northing = endPoint.Y;
                    align.StationOffset(easting, northing, ref station, ref offset);
                    align.PointLocation(station, 0, ref easting, ref northing);
                    startPoint = new Point3d(easting, northing, 0);
                    inRange = true;
                }
                catch (PointNotOnEntityException)
                // The point is outside of the alignment boundaries

                {
                    inRange = false;
                }
                catch (Exception ex)
                {
                    ed.WriteMessage(ex.Message);
                    inRange = false;
                }
            }
            return SamplerStatus.OK;
        }


        public StationJig(Alignment align)
        {
            // get the alignment from the id
            this.align = align;
            ed.WriteMessage("jigalign:", this.align.Name);
        }

    }
}


