﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Text;
//using System.Threading.Tasks;

//namespace VeilBlockToDB.ModelsJson
//{
//    [DataContract]
//    public class LineGraphDataPointDouble
//    {
//        public LineGraphDataPointDouble(long x, double y, string label)
//        {
//            this.X = x;
//            this.Y = y;
//            this.label = label;
//        }

//        public LineGraphDataPointDouble()
//        {
//        }

//        //Explicitly setting the name to be used while serializing to JSON.
//        [DataMember(Name = "x")]
//        public long X;

//        //Explicitly setting the name to be used while serializing to JSON.
//        [DataMember(Name = "y")]
//        public double Y;

//        //Explicitly setting the name to be used while serializing to JSON.
//        [DataMember(Name = "label")]
//        public string label = "";
//    }
//}
