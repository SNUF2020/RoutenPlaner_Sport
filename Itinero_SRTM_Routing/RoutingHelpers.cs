﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RPS
{
    class RoutingHelpers
    {
        public static List<double> GetEleData(List<List<Data>> Input)
        {
            List<List<Data>> _Input = Input;
            List<double> _Output = new List<double>();

            if (Input.Any()) //prevent IndexOutOfRangeException for empty list - code snippet from code.grepper.com
            {
                for (int i = 0; i < _Input.Count; i++)
                {
                    for (int k = 0; k < _Input[i].Count; k++)
                    {
                        double newElement = _Input[i][k].Alt;
                        _Output.Add(newElement);
                    }
                }
            }
            else
            {
                double newElement1 = 0;
                _Output.Add(newElement1);
            }

            return _Output;

        }
        public static List<double> SavitzkyGolayFilter(List<double> data_list)
        {
            List<double> Output = new List<double>(data_list);

            for (int i = 0; i < data_list.Count; i++)
            {
                if (i >= 12 & i < data_list.Count - 12)
                {
                    Output[i] =
                    (data_list[i - 12] * -253 + data_list[i - 11] * -138
                    + data_list[i - 10] * -33 + data_list[i - 9] * 62
                    + data_list[i - 8] * 147 + data_list[i - 7] * 222
                    + data_list[i - 6] * 287 + data_list[i - 5] * 343
                    + data_list[i - 4] * 387 + data_list[i - 3] * 422
                    + data_list[i - 2] * 447 + data_list[i - 1] * 462
                    + data_list[i] * 467
                    + data_list[i + 1] * 462 + data_list[i + 2] * 447
                    + data_list[i + 3] * 422 + data_list[i + 4] * 387
                    + data_list[i + 5] * 343 + data_list[i + 6] * 287
                    + data_list[i + 7] * 222 + data_list[i + 8] * 147
                    + data_list[i + 9] * 62 + data_list[i + 10] * -33
                    + data_list[i + 11] * -138 + data_list[i + 12] * -253) / 5175;
                }
                else
                {
                    if (i >= 8 & i < data_list.Count - 8)
                    {
                        Output[i] = (data_list[i - 8] * -21
                            + data_list[i - 7] * -6
                            + data_list[i - 6] * 7
                            + data_list[i - 5] * 18
                            + data_list[i - 4] * 27
                            + data_list[i - 3] * 34
                            + data_list[i - 2] * 39
                            + data_list[i - 1] * 42
                            + data_list[i] * 43
                            + data_list[i + 1] * 42
                            + data_list[i + 2] * 39
                            + data_list[i + 3] * 34
                            + data_list[i + 4] * 27
                            + data_list[i + 5] * 18
                            + data_list[i + 6] * 7
                            + data_list[i + 7] * -6
                            + data_list[i + 8] * -21) / 323;
                    }
                    else
                    {
                        if (i >= 4 & i < data_list.Count - 4)
                        {
                            Output[i] =
                                (data_list[i - 4] * -21
                                + data_list[i - 3] * 14
                                + data_list[i - 2] * 39
                                + data_list[i - 1] * 54
                                + data_list[i] * 59
                                + data_list[i + 1] * 54
                                + data_list[i + 2] * 39
                                + data_list[i + 3] * 14
                                + data_list[i + 4] * -21) / 231;
                        }
                        else
                        {
                            if (i >= 2 & i < data_list.Count - 2)
                            {
                                Output[i] =
                                    (data_list[i - 2] * -3
                                    + data_list[i - 1] * 12
                                    + data_list[i] * 17
                                    + data_list[i + 1] * 12
                                    + data_list[i + 2] * -3) / 35;
                            }
                        }
                    }
                }
            }

            return Output;
        }
        public static List<List<string[]>> FetchShapeFiles()
        {
            string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string[] shape_files = Directory.GetFiles(strWorkPath + @"\Router_DB", "*.shape.csv");

            List<List<string[]>> _all_RegShape = new List<List<string[]>>();

            foreach (string f in shape_files)
            {
                List<string[]> _RegShape = new List<string[]>();

                try
                {
                    // open file, here: UTF8
                    using (StreamReader sr = new StreamReader(f, Encoding.UTF8))
                    {
                        _RegShape.Clear();

                        // read file until end
                        while (!sr.EndOfStream)
                        {
                            // read single line and split with seperattor sign ";" into columns (stringarray)
                            string[] currentline = sr.ReadLine().Replace(",", ".").Split(new string[] { ";" }, StringSplitOptions.None);
                            _RegShape.Add(currentline);
                        }

                        _all_RegShape.Add(_RegShape);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return _all_RegShape;
        }

        public static List<string[]> FetchSRTMFiles()
        {
            string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string[] SRTM_files = Directory.GetFiles(strWorkPath + @"\SRTM_Data", "*.hgt");

            List<string[]> _all_SRTMs = new List<string[]>();

            foreach (string srtm in SRTM_files)
            {
                string sign_Lat;
                string sign_Lon;

                string _fileName = Path.GetFileName(srtm);

                if (_fileName.Substring(0, 1) == "N")
                {
                    sign_Lat = "";
                }
                else // we are on southern region of the world
                {
                    sign_Lat = "-";
                }

                if (_fileName.Substring(3, 1) == "E")
                {
                    sign_Lon = "";
                }
                else // we are on western region of the world
                {
                    sign_Lon = "-";
                }

                string[] _coordinate1 = { sign_Lon + _fileName.Substring(4, 3), sign_Lat + _fileName.Substring(1, 2) };
                _all_SRTMs.Add(_coordinate1);
            }

            return _all_SRTMs;
        }


        public static double Get_Max_Lat(List<List<Data>> _AllRoutes)
        {
            double max_Lat = _AllRoutes[0][0].Lat;
            for (int i = 0; i < _AllRoutes.Count; i++)
            {
                for (int k = 0; k < _AllRoutes[i].Count; k++)
                {
                    if (_AllRoutes[i][k].Lat > max_Lat)
                        max_Lat = _AllRoutes[i][k].Lat;
                }
            }
            return max_Lat;
        }

        public static double Get_Min_Lat(List<List<Data>> _AllRoutes)
        {
            double min_Lat = _AllRoutes[0][0].Lat;
            for (int i = 0; i < _AllRoutes.Count; i++)
            {
                for (int k = 0; k < _AllRoutes[i].Count; k++)
                {
                    if (_AllRoutes[i][k].Lat < min_Lat)
                        min_Lat = _AllRoutes[i][k].Lat;
                }
            }
            return min_Lat;
        }

        public static double Get_Max_Lon(List<List<Data>> _AllRoutes)
        {
            double max_Lon = _AllRoutes[0][0].Lon;
            for (int i = 0; i < _AllRoutes.Count; i++)
            {
                for (int k = 0; k < _AllRoutes[i].Count; k++)
                {
                    if (_AllRoutes[i][k].Lon > max_Lon)
                        max_Lon = _AllRoutes[i][k].Lon;
                }
            }
            return max_Lon;
        }

        public static double Get_Min_Lon(List<List<Data>> _AllRoutes)
        {
            double min_Lon = _AllRoutes[0][0].Lon;
            for (int i = 0; i < _AllRoutes.Count; i++)
            {
                for (int k = 0; k < _AllRoutes[i].Count; k++)
                {
                    if (_AllRoutes[i][k].Lon < min_Lon)
                        min_Lon = _AllRoutes[i][k].Lon;
                }
            }
            return min_Lon;
        }

        public static int? Check4NegativeValue(int? _value)
        {
            if (_value > 9000)
            {
                _value = 0;
            }
            return _value;
        }
        // Method to get rid of wrong hight values (negativ elevation and "int" format are not a good combination...)


        public static bool IsPointInPolygon4(Point[] polygon, Point testPoint)
        {
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public static bool IsPointInPolygon4(PointF[] polygon, Point testPoint)
        {
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        // Find the polygon's centroid. -> http://csharphelper.com/blog/2014/07/find-the-centroid-of-a-polygon-in-c/
        public static PointF FindCentroid(Point[] _shape_Reg)
        {
            // Add the first point at the end of the array.
            int num_points = _shape_Reg.Length;
            Point[] pts = new Point[num_points + 1];
            _shape_Reg.CopyTo(pts, 0);
            pts[num_points] = _shape_Reg[0];

            // Find the centroid.
            float X = 0;
            float Y = 0;
            float second_factor;
            for (int i = 0; i < num_points; i++)
            {
                second_factor =
                    pts[i].X * pts[i + 1].Y -
                    pts[i + 1].X * pts[i].Y;
                X += (pts[i].X + pts[i + 1].X) * second_factor;
                Y += (pts[i].Y + pts[i + 1].Y) * second_factor;
            }

            // Divide by 6 times the polygon's area.
            float polygon_area = SignedPolygonArea(_shape_Reg);
            X /= (6 * polygon_area);
            Y /= (6 * polygon_area);

            // If the values are negative, the polygon is
            // oriented counterclockwise so reverse the signs.
            if (X < 0)
            {
                X = -X;
                Y = -Y;
            }

            return new PointF(X, Y);
        }

        // Return the polygon's area in "square units."
        // The value will be negative if the polygon is
        // oriented clockwise.

        private static float SignedPolygonArea(Point[] _shape_Reg)
        {
            // Add the first point to the end.
            int num_points = _shape_Reg.Length;
            Point[] pts = new Point[num_points + 1];
            _shape_Reg.CopyTo(pts, 0);
            pts[num_points] = _shape_Reg[0];

            // Get the areas.
            float area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area +=
                    (pts[i + 1].X - pts[i].X) *
                    (pts[i + 1].Y + pts[i].Y) / 2;
            }

            // Return the result.
            return area;
        }

        /// <summary>
        /// Gets the distance between two coordinates in meters.
        /// </summary>
        /// <param name="latitude1">The latitude1.</param>
        /// <param name="longitude1">The longitude1.</param>
        /// <param name="latitude2">The latitude2.</param>
        /// <param name="longitude2">The longitude2.</param>
        /// <returns></returns>
        public static double GetDistanceBetweenTwoPoints(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            if (latitude1 != latitude2 || longitude1 != longitude2)
            {
                double theta = longitude1 - longitude2;
                double distance = Math.Sin(ConvertDecimalDegreesToRadians(latitude1)) *
                                  Math.Sin(ConvertDecimalDegreesToRadians(latitude2)) +
                                  Math.Cos(ConvertDecimalDegreesToRadians(latitude1)) *
                                  Math.Cos(ConvertDecimalDegreesToRadians(latitude2)) *
                                  Math.Cos(ConvertDecimalDegreesToRadians(theta));

                distance = Math.Acos(distance);
                distance = ConvertRadiansToDecimalDegrees(distance);
                distance = distance * 60 * 1.1515;
                // convert to meters
                return (distance * 1.609344 * 1000);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts the decimal degrees to radians.
        /// </summary>
        /// <param name="degree">The degree.</param>
        /// <returns></returns>
        private static double ConvertDecimalDegreesToRadians(double degree)
        {
            return (degree * Math.PI / 180.0);
        }

        /// <summary>
        /// Converts the radians to decimal degrees.
        /// </summary>
        /// <param name="radian">The radian.</param>
        /// <returns></returns>
        private static double ConvertRadiansToDecimalDegrees(double radian)
        {
            return (radian / Math.PI * 180.0);
        }
        public static double GetmaxEle(List<List<Data>> _AllRoutes)
        {
            double max_Ele = _AllRoutes[0][0].Alt;
            for (int i = 0; i < _AllRoutes.Count; i++)
            {
                for (int k = 0; k < _AllRoutes[i].Count; k++)
                {
                    if (_AllRoutes[i][k].Alt > max_Ele)
                        max_Ele = _AllRoutes[i][k].Alt;
                }
            }
            return max_Ele;
        }

        public static double GetminEle(List<List<Data>> _AllRoutes)
        {
            double min_Ele = _AllRoutes[0][0].Alt;
            for (int i = 0; i < _AllRoutes.Count; i++)
            {
                for (int k = 0; k < _AllRoutes[i].Count; k++)
                {
                    if (_AllRoutes[i][k].Alt < min_Ele)
                        min_Ele = _AllRoutes[i][k].Alt;
                }
            }
            return min_Ele;
        }
    }
}
