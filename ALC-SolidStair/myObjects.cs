using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace LX_SolidStair
{
    public class BaseStairObject
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public double Riser {get; set; }
        public double Tread { get; set; }
        public double Width { get; set;  }
        public double Landing { get; set; }
        public double Slope { get; set; }
        public int Steps { get; set; }
    }


    public class SolidStairObject : BaseStairObject
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Elevation { get; set; }
        public double Rotation { get; set; }
    }


    public class LayerObject
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public Boolean IsSelected { get; set; }
    }
}
