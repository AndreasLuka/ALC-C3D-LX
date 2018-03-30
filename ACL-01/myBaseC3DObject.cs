using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace ALCJIG2.Civil3D
{
    public class BaseC3DObject
    {
        private string name = "";
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private Boolean isSelected = false;
        public Boolean IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }
        private ObjectId id;
        public ObjectId Id
        {
            get { return id; }
            set { id = value; }
        }
    }
}
