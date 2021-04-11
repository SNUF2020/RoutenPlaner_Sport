using BruTile;
using BruTile.Cache;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace RPS
{
    class Renderer
    {
        private readonly Bitmap _canvas;
        private readonly Bitmap _canvas2; // Routing layer

        public Renderer(Bitmap canvas, Bitmap canvas2)
        {
            _canvas = canvas;
            _canvas2 = canvas2;
        }



        public void Render(Viewport viewport, ITileSource tileSource, ITileCache<Tile<Image>> tileCache, List<List<Data>> AllRoutes,
            double _minX_Value, double _maxX_Value, List<string[]> _RegShape, bool _boundary, List<List<string[]>> _all_RegShapes,
            Point _mousePosition, bool _newDB, List<string[]> _all_SRTMShapes)
        {
            var level = Utilities.GetNearestLevel(tileSource.Schema.Resolutions, viewport.UnitsPerPixel);
            var tileInfos = tileSource.Schema.GetTileInfos(viewport.Extent, level);

            // ploting map
            using (var g = Graphics.FromImage(_canvas))
            {
                //g.Clear(Color.White);

                foreach (var tileInfo in tileInfos)
                {
                    var extent = viewport.WorldToScreen(tileInfo.Extent.MinX, tileInfo.Extent.MinY,
                                                            tileInfo.Extent.MaxX, tileInfo.Extent.MaxY);
                    var tile = tileCache.Find(tileInfo.Index);
                    if (tile != null)
                    {
                        DrawTile(tileSource.Schema, g, (Bitmap)tile.Image, extent, tileInfo.Index.Level);
                    }
                }
            }

            //plotting route and shape of routerdb or overview of database (SRTM and intinero)
            using (var h = Graphics.FromImage(_canvas2))
            {
                h.Clear(Color.Transparent);

                // plot ALL boundaries of ALL routing db
                if (_newDB)
                {
                    string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                    List<Point[]> reg_Shapes_all = new List<Point[]>();

                    string[] shape_files = Directory.GetFiles(strWorkPath + @"\Router_DB", "*.shape.csv");

                    for (int s = 0; s < _all_RegShapes.Count; s++)
                    {
                        Point[] shape_Reg = new Point[_all_RegShapes[s].Count];

                        for (int i = 0; i < _all_RegShapes[s].Count; i++)
                        {
                            PointF Punkt_shape = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_all_RegShapes[s][i][0], CultureInfo.InvariantCulture)),
                                viewport.LatitudeToY(Convert.ToDouble(_all_RegShapes[s][i][1], CultureInfo.InvariantCulture)));

                            shape_Reg[i].X = (int)Punkt_shape.X;
                            shape_Reg[i].Y = (int)Punkt_shape.Y;
                        }

                        if (RoutingHelpers.IsPointInPolygon4(shape_Reg, _mousePosition))
                        {
                            //h.FillPolygon(new SolidBrush(Color.Red), shape_Reg);
                            h.DrawPolygon(new Pen(Color.Red, 5), shape_Reg);
                            //int found = shape_files[s].IndexOf("_All");
                            h.DrawString(Path.GetFileName(shape_files[s]).Substring(0, 7).ToString(), new Font("Arial", 10),
                                new SolidBrush(Color.Red),
                                RoutingHelpers.FindCentroid(shape_Reg).X,
                                RoutingHelpers.FindCentroid(shape_Reg).Y);
                        }
                        else
                        {
                            h.DrawPolygon(new Pen(Color.Red, 2), shape_Reg);
                        }

                    }
                    // plot ALL boundaries of ALL STRM files

                    for (int t = 0; t < _all_SRTMShapes.Count; t++)
                    {
                        PointF[] shape_SRTM = new PointF[4];

                        // Every SRTM shape is defined by ONE coordinate (bottom, left point)
                        shape_SRTM[0] = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_all_SRTMShapes[t][0], CultureInfo.InvariantCulture)),
                                viewport.LatitudeToY(Convert.ToDouble(_all_SRTMShapes[t][1], CultureInfo.InvariantCulture)));
                        shape_SRTM[1] = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_all_SRTMShapes[t][0], CultureInfo.InvariantCulture) + 1),
                                viewport.LatitudeToY(Convert.ToDouble(_all_SRTMShapes[t][1], CultureInfo.InvariantCulture)));
                        shape_SRTM[2] = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_all_SRTMShapes[t][0], CultureInfo.InvariantCulture) + 1),
                                viewport.LatitudeToY(Convert.ToDouble(_all_SRTMShapes[t][1], CultureInfo.InvariantCulture) + 1));
                        shape_SRTM[3] = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_all_SRTMShapes[t][0], CultureInfo.InvariantCulture)),
                                viewport.LatitudeToY(Convert.ToDouble(_all_SRTMShapes[t][1], CultureInfo.InvariantCulture) + 1));


                        if (RoutingHelpers.IsPointInPolygon4(shape_SRTM, _mousePosition))
                        {
                            //h.FillPolygon(new SolidBrush(Color.Blue), shape_SRTM);
                            h.DrawString(_all_SRTMShapes[t][1].ToString() + "°/" + _all_SRTMShapes[t][0].ToString() + "°",
                                new Font("Arial", 10),
                                new SolidBrush(Color.Blue),
                                shape_SRTM[0].X + 1, (shape_SRTM[0].Y - (shape_SRTM[1].Y - shape_SRTM[2].Y) / 2));
                        }

                        h.DrawPolygon(new Pen(Color.Blue, 2), shape_SRTM);
                    }
                }

                // plot boundary of routing db
                if (_boundary)
                {
                    Point[] shape_Reg = new Point[_RegShape.Count];

                    for (int i = 0; i < _RegShape.Count; i++)
                    {
                        PointF Punkt_shape = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_RegShape[i][0], CultureInfo.InvariantCulture)),
                            viewport.LatitudeToY(Convert.ToDouble(_RegShape[i][1], CultureInfo.InvariantCulture)));

                        shape_Reg[i].X = (int)Punkt_shape.X;
                        shape_Reg[i].Y = (int)Punkt_shape.Y;
                    }

                    using (var path = new GraphicsPath())
                    {
                        path.AddPolygon(shape_Reg);
                        // Uncomment this to invert:
                        Rectangle _frame = new Rectangle(0, 0, _canvas2.Width, _canvas2.Height);
                        path.AddRectangle(_frame);

                        using (var brush = new SolidBrush(Color.FromArgb(188, 211, 211, 211))) // lightgrey = 3 x 211
                        {
                            h.FillPath(brush, path);
                        }
                    }
                }

                // Plot route (from file or online-created) 
                if (AllRoutes != null)
                {
                    Pen Linie = new Pen(Color.Blue);
                    Point Anfang = new Point();
                    Point Ende = new Point();

                    foreach (List<Data> TeilRoute in AllRoutes)
                    {
                        Linie.Width = 2;
                        int arrow = 0;
                        //int circle = 0;

                        PointF Punkt = viewport.WorldToScreen(viewport.LongitudeToX(TeilRoute[0].Lon), viewport.LatitudeToY(TeilRoute[0].Lat));

                        // Zeichne Track in pictureBox1.Imaqge ein
                        Anfang.X = (int)Punkt.X;
                        Anfang.Y = (int)Punkt.Y;
                        for (int i = 1; i < TeilRoute.Count; i++)
                        {
                            arrow++;

                            Punkt = viewport.WorldToScreen(viewport.LongitudeToX(TeilRoute[i].Lon), viewport.LatitudeToY(TeilRoute[i].Lat));

                            Ende.X = (int)Punkt.X;
                            Ende.Y = (int)Punkt.Y;

                            if (_maxX_Value > TeilRoute[i].Distance / 1000 && _minX_Value < TeilRoute[i].Distance / 1000)
                            {
                                Linie.Color = Color.Red;
                                Linie.Width = 3;
                            }
                            else
                            {
                                Linie.Color = Color.Blue;
                                Linie.Width = 2;
                            }

                            if (arrow > 20)
                            {
                                arrow = 0;
                                Linie.Width = 5;
                                Linie.EndCap = LineCap.ArrowAnchor;
                                h.DrawLine(Linie, Anfang, Ende);
                                Linie.EndCap = LineCap.Round;
                                Linie.Width = 2;
                            }
                            else
                            {
                                h.DrawLine(Linie, Anfang, Ende);
                            }

                            Anfang = Ende;
                        }
                    }
                }
            }
        }

        private static void DrawTile(ITileSchema schema, Graphics graphics, Bitmap bitmap, RectangleF extent, int level)
        {
            // For drawing on WinForms there are two things to take into account
            // to prevent seams between tiles.
            // 1) The WrapMode should be set to TileFlipXY. This is related
            //    to how pixels are rounded by GDI+
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
            // 2) The rectangle should be rounded to actual pixels.
            var roundedExtent = RoundToPixel(extent);
            graphics.DrawImage(bitmap, roundedExtent, 0, 0, schema.GetTileWidth(level), schema.GetTileHeight(level), GraphicsUnit.Pixel, imageAttributes);
        }

        private static Rectangle RoundToPixel(RectangleF dest)
        {
            // To get seamless aligning you need to round the 
            // corner coordinates to pixel.
            return new Rectangle(
                (int)Math.Round(dest.Left),
                (int)Math.Round(dest.Top),
                (int)(Math.Round(dest.Right) - Math.Round(dest.Left)),
                (int)(Math.Round(dest.Bottom) - Math.Round(dest.Top)));
        }
    }
}
