using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using Itinero;
using Itinero.Osm.Vehicles;
using SRTM.Sources.USGS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;

//
// S.Nuf.2021
// Code under MIT Licence
//
// RPS = Routen Planer Sport Version2.2 (Version2.0 -> First Release, now V2.1 is with localizied folder system (all in bin-folder), V2.2 for GitHub)
//
// Source of MapControl: see GitHub Project BruTile > Samples > BruTile.Demo (WPF) -> Transformed to WinForm -> RPSV1.0   
//
// Structure of code: First part = Routing etc. / second part = elevation chart / Third part = MapControl
//
// Correlations: Longitude (Längengrad) = X-direction / Latitude (Breitengrad) = Y-Direction
//
// Doku:
// Map data: Thunderforest (API key necessary) or OSM and Bing (w/o API key)
// Doku Routenplaner: https://docs.itinero.tech/docs/osmsharp/index.html
// Raw data for routerDB (osm.pbf files): http://download.geofabrik.de/
// Doku GPX-Writer/Reader: https://github.com/macias/Gpx/blob/master/Gpx/Implementation/GpxWriter.cs
// Embedding of elevation data: https://github.com/itinero/srtm 
// elevation data from http://viewfinderpanoramas.org/dem3.html (90m-resolution, Alpes 30m) and (better): 
// from https://e4ftl01.cr.usgs.gov/MEASURES/SRTMGL1.003/2000.02.11/ (NASA, world-wide with 30m-resolution)
// Problem at both servers: Downloading of data at running program does not work -> Since app. 2014 from server-side disabled 
// -> Data will be downloaded to local disc (at NASA server password required)
// All elevation data are stored in SRTM_Data at bin-folder

namespace RPS
{
    public partial class Form1 : Form
    {
        //
        // General Constructor Form1
        //

        public Form1()
        {
            InitializeComponent();
            // DoubleBuffered = True (see Form1)

            InitializeMapControl();
            InitializeRouting();
        }

        // --------------------------------------------------------------------------------------------------------------
        //
        // Routing code
        //

        //
        // Routing Fields
        //

        // working directory (should be ".../bin")
        private string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private string API_key = "place here your API key for thunderforest";

        // Shape of routerDB, all routerDB's and all SRTM areas
        List<string[]> RegShape = new List<string[]>();
        List<List<string[]>> all_RegShape = new List<List<string[]>>();
        List<string[]> all_srtmShape = new List<string[]>();

        List<List<Data>> AllRoutes = new List<List<Data>>();
        List<Data> MouseKlicks = new List<Data>();
        int Gesamtzeit = 0;
        int Gesamtstrecke = 0;

        DateTime localDate = DateTime.Now;

        RouterDb routerDb = null;
        Itinero.Profiles.Profile profile = null;

        private Bitmap _route;

        private Point _mousePosition;

        private bool hike = true;
        private bool cross = false;
        private bool race = false;
        private bool _newDB = false;

        //
        // Marking elevation data in chart_ele:
        //
        private Point Start = new Point();
        private Rectangle RcDraw; // = new Rectangle( x, y, width, heigth );

        private Pen Marker_Pen = new Pen(Color.Red, 1); // ... for marking lines (see marking elevation data)

        private bool mouseDown = false;

        private double minX_Value = 1000;
        private double maxX_Value = 1001;

        private double maxAlt = -1000;
        private double minAlt = 10000;

        private double maxX_Alt;
        private double minX_Alt;

        //
        // -------------------------------------------------------------------------------------------------------------------------------
        //
        // Routing Methods
        //
        private void InitializeRouting()
        {
            load_routerDB(strWorkPath + @"/Router_DB/GER-BaW.routerdb_All", false);

            all_RegShape = RoutingHelpers.fetchShapeFiles();
            all_srtmShape = RoutingHelpers.fetchSRTMFiles();
        }

        private void load_routerDB(string _filename, bool _message)
        {
            try
            {
                Cursor = Cursors.WaitCursor; // Sanduhr wird für die Zeit des Routings angezeigt

                using (var stream = new FileInfo(_filename).OpenRead())
                {
                    routerDb = RouterDb.Deserialize(stream);
                    if (_message) MessageBox.Show("Router-DataBase is now available");

                    textBox_Reg.Text = Path.GetFileName(_filename).ToString();

                    RegShape = RoutingIO.Load_RegShape(Path.GetFileName(_filename).ToString() + ".shape.csv", RegShape);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void Center_Track(List<List<Data>> _AllRoutes)
        {
            if (MouseKlicks.Count > 1)
            {
                _viewport.CenterX = _viewport.LongitudeToX(RoutingHelpers.Get_Min_Lon(_AllRoutes) + (RoutingHelpers.Get_Max_Lon(_AllRoutes) - RoutingHelpers.Get_Min_Lon(_AllRoutes)) / 2);
                _viewport.CenterY = _viewport.LatitudeToY(RoutingHelpers.Get_Min_Lat(_AllRoutes) + (RoutingHelpers.Get_Max_Lat(_AllRoutes) - RoutingHelpers.Get_Min_Lat(_AllRoutes)) / 2);

                double _delta_Lon = _viewport.LongitudeToX(RoutingHelpers.Get_Max_Lon(_AllRoutes)) - _viewport.LongitudeToX(RoutingHelpers.Get_Min_Lon(_AllRoutes));
                double _delta_Lat = _viewport.LatitudeToY(RoutingHelpers.Get_Max_Lat(_AllRoutes)) - _viewport.LatitudeToY(RoutingHelpers.Get_Min_Lat(_AllRoutes));

                double _unitsPerPixel_dummy_Lon = _delta_Lon / (Width * 0.9);
                double _unitsPerPixel_dummy_Lat = _delta_Lat / (Height * 0.9);

                // which is the larger dimension of track?
                if (_delta_Lon < _delta_Lat)
                {
                    _viewport.UnitsPerPixel = ZoomHelper.ZoomRoute(_tileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList(), _viewport.UnitsPerPixel, _unitsPerPixel_dummy_Lat);
                }
                else
                {
                    _viewport.UnitsPerPixel = ZoomHelper.ZoomRoute(_tileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList(), _viewport.UnitsPerPixel, _unitsPerPixel_dummy_Lon);
                }

                _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel);
            }
        }

        private void Track_Load()
        {
            using (OpenFileDialog ofDlg = new OpenFileDialog())
            {
                // Datei öffnen -> InitialDirectory does NOT WORK
                ofDlg.InitialDirectory = strWorkPath + "/Tracks";
                ofDlg.Filter = "GPX Files (*.gpx)|*.gpx|All Files (*.*)|*.*";
                if (ofDlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Erzeuge Xml-Dokumnent + Liste der Track-Punkte
                        XmlDocument docXML = new XmlDocument();
                        docXML.Load(ofDlg.FileName);

                        XmlNodeList elemList = docXML.GetElementsByTagName("trkpt");
                        XmlNodeList elemList2 = docXML.GetElementsByTagName("ele");

                        List<Data> EinzelRoute = new List<Data>();

                        // First element needs special treatment due to calc of dist and time...

                        string dummy_lon;
                        string dummy_lat;

                        if (elemList[0].Attributes[0].Name == "lon") // to be on safe side, in case lat and lon attributes are mixed-up in GPX file
                        {
                            dummy_lon = elemList[0].Attributes[0].Value;
                            dummy_lat = elemList[0].Attributes[1].Value;
                        }
                        else
                        {
                            dummy_lon = elemList[0].Attributes[1].Value;
                            dummy_lat = elemList[0].Attributes[0].Value;
                        }

                        string dummy_lon_old = dummy_lon; // for distance calculation
                        string dummy_lat_old = dummy_lat;

                        Data TrackPoint0 = new Data(
                            Convert.ToDouble(dummy_lat, CultureInfo.InvariantCulture),
                            Convert.ToDouble(dummy_lon, CultureInfo.InvariantCulture),
                            Convert.ToDouble(elemList2[0].InnerXml, CultureInfo.InvariantCulture),
                                0, 0);
                        EinzelRoute.Add(TrackPoint0);

                        // Now, here comes the rest...
                        for (int i = 1; i < elemList.Count; i++)
                        {
                            if (elemList[0].Attributes[0].Name == "lon") // to be on safe side, in case lat and lon attributes are mixed-up in GPX file
                            {
                                dummy_lon = elemList[i].Attributes[0].Value;
                                dummy_lat = elemList[i].Attributes[1].Value;
                            }
                            else
                            {
                                dummy_lon = elemList[i].Attributes[1].Value;
                                dummy_lat = elemList[i].Attributes[0].Value;
                            }

                            double Abstand = RoutingHelpers.GetDistanceBetweenTwoPoints(
                                Convert.ToDouble(dummy_lat_old, CultureInfo.InvariantCulture),
                                Convert.ToDouble(dummy_lon_old, CultureInfo.InvariantCulture),
                                Convert.ToDouble(dummy_lat, CultureInfo.InvariantCulture),
                                Convert.ToDouble(dummy_lon, CultureInfo.InvariantCulture));

                            dummy_lon_old = dummy_lon;
                            dummy_lat_old = dummy_lat;

                            double Zeit = (double)Abstand / 4000 * 3600; // this is for pedestrian w/ 4km/h

                            // get the right profile (for time calculation).     
                            if (hike)
                            {
                                Zeit = (double)Abstand / 4000 * 3600; // this is for pedestrian w/ 4km/h
                            }
                            if (cross)
                            {
                                Zeit = (double)Abstand / 20000 * 3600; // this is for bike w/ 20km/h
                            }
                            if (race)
                            {
                                Zeit = (double)Abstand / 24000 * 3600; // this is for car w/ 24km/h
                            }

                            Data TrackPoint = new Data(
                                Convert.ToDouble(dummy_lat, CultureInfo.InvariantCulture),
                                Convert.ToDouble(dummy_lon, CultureInfo.InvariantCulture),
                                Convert.ToDouble(elemList2[i].InnerXml, CultureInfo.InvariantCulture),
                                EinzelRoute[i - 1].Time + (int)Zeit, EinzelRoute[i - 1].Distance + Abstand);

                            EinzelRoute.Add(TrackPoint);
                        }

                        AllRoutes.Add(EinzelRoute);
                        Gesamtstrecke = (int)EinzelRoute[EinzelRoute.Count - 1].Distance;
                        Gesamtzeit = EinzelRoute[EinzelRoute.Count - 1].Time;

                        textBox_Dis.Text = ((double)Gesamtstrecke / 1000).ToString("#0.000") + " km";
                        int Stunden = (int)Gesamtzeit / 3600;
                        int Rest = (int)Gesamtzeit % 3600;
                        int Minuten = Rest / 60;
                        textBox_TotT.Text = Stunden.ToString() + "h " + Minuten.ToString() + " Minuten";

                        // Integrate mouse klick (2 klicks)
                        Data Klick1 = new Data(
                                Convert.ToDouble(elemList[0].Attributes[0].Value, CultureInfo.InvariantCulture),
                                Convert.ToDouble(elemList[0].Attributes[1].Value, CultureInfo.InvariantCulture),
                                0, 0, 0);
                        Data Klick2 = new Data(
                                Convert.ToDouble(elemList[elemList.Count - 1].Attributes[0].Value, CultureInfo.InvariantCulture),
                                Convert.ToDouble(elemList[elemList.Count - 1].Attributes[1].Value, CultureInfo.InvariantCulture),
                                0, 0, 0);
                        MouseKlicks.Add(Klick1);
                        MouseKlicks.Add(Klick2);

                        Center_Track(AllRoutes);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        } // load data from xml-file and plot track

        private Bitmap plot_Image()
        {
            Bitmap fusionImage = new Bitmap(Width, Height);
            Bitmap chart_image = new Bitmap(chart_ele.Width, chart_ele.Height, PixelFormat.Format32bppArgb);

            using (Graphics grfx = Graphics.FromImage(fusionImage))
            {
                grfx.DrawImage(_buffer, new Rectangle(0, 0, _buffer.Width, _buffer.Height));
                grfx.DrawImage(_route, new Rectangle(0, 0, _route.Width, _route.Height));

                // Plot route information top left into image...
                SolidBrush whiteBrush = new SolidBrush(Color.White);

                string map_text = "Distance: " + textBox_Dis.Text + " / Expected TotalTime: " + textBox_TotT.Text;
                Font drawFont = new Font("Microsoft Sans Serif", 12, FontStyle.Regular, GraphicsUnit.Point, 0);

                // getting the right dimension of the text background box... (using TextRenderer.MeasureText method)
                Size size = TextRenderer.MeasureText(map_text, drawFont);
                Rectangle Rahmen = new Rectangle(10, 10, size.Width + 15, size.Height);

                grfx.FillRectangle(whiteBrush, Rahmen);

                StringFormat drawFormat = new StringFormat();
                grfx.TextRenderingHint = TextRenderingHint.AntiAlias;
                grfx.DrawString(map_text, drawFont, Brushes.Black, 10, 10, drawFormat);

                // export chart_ele to bitmap (for inlay in image of route)
                chart_ele.DrawToBitmap(chart_image, new Rectangle(0, 0, chart_image.Size.Width, chart_image.Size.Height));

                Rectangle Inlay = new Rectangle(10, 50, chart_image.Size.Width, chart_image.Size.Height);

                grfx.DrawImage(chart_image, Inlay);
            }
            return fusionImage;
        }

        private void Safe_Image()
        {
            // First fit track to map...
            Center_Track(AllRoutes);

            // Displays a SaveFileDialog so the user can save the Image
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = strWorkPath + "\\Tracks";
            saveFileDialog1.Filter = "PNG Image|*.png|Bitmap Image|*.bmp|JPEG Image|*.jpg|Gif Image|*.gif";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                using (FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile())
                {
                    // Saves the Image in the appropriate ImageFormat based upon the
                    // File type selected in the dialog box.
                    // NOTE that the FilterIndex property is one-based.

                    ImageFormat imageFormat = null;
                    switch (saveFileDialog1.FilterIndex)
                    {
                        case 1:
                            imageFormat = ImageFormat.Png;
                            break;
                        case 2:
                            imageFormat = ImageFormat.Bmp;
                            break;
                        case 3:
                            imageFormat = ImageFormat.Jpeg;
                            break;
                        case 4:
                            imageFormat = ImageFormat.Gif;
                            break;
                        default:
                            throw new NotSupportedException("File extension is not supported");
                    }
                    plot_Image().Save(fs, imageFormat);
                }
            }
        }

        private void MakeRoute()
        {
            // get second last and last coordinates for calculating latest route segment
            double start_Lon = MouseKlicks[MouseKlicks.Count - 2].Lon;
            double start_Lat = MouseKlicks[MouseKlicks.Count - 2].Lat;
            double ziel_Lon = MouseKlicks[MouseKlicks.Count - 1].Lon;
            double ziel_Lat = MouseKlicks[MouseKlicks.Count - 1].Lat;
            if (!_newDB)
            {
                if (checkBox_Routing.Checked)
                {
                    var router = new Router(routerDb);

                    // get the right routerDB profile.     
                    if (hike)
                    {
                        profile = Vehicle.Pedestrian.Shortest();
                    }
                    if (cross)
                    {
                        profile = Vehicle.Bicycle.Shortest(); // the default OSM bike profile.
                    }
                    if (race)
                    {
                        profile = Vehicle.Car.Fastest(); // the default OSM bike profile.
                    }

                    try
                    {
                        Cursor = Cursors.WaitCursor; // Sanduhr wird für die Zeit des Routings angezeigt
                                                     // create a routerpoint from a location.                             
                                                     // snaps the given location to the nearest routable edge.
                        var start = router.Resolve(profile, (float)start_Lat, (float)start_Lon, 250); // 250m radius for search of next routing point in routing db
                        var end = router.Resolve(profile, (float)ziel_Lat, (float)ziel_Lon, 250); // standard = 50m is to less 

                        var route = router.Calculate(profile, start, end);

                        // Elevation data from SRTM - data-log to NASA sever does not work -> since 2014 NASA blocked direct log! 
                        // SRTMData srtmData = new SRTMData(root_path + "\\source\\repos\\00_Data\\16_SRTM", new NASASource(credentials));

                        var srtmData = new SRTM.SRTMData(strWorkPath + "\\SRTM_Data", new USGSSource());

                        List<Data> EinzelRoute = new List<Data>();

                        for (int i = 0; i < route.Shape.Length; i++)
                        {

                            Data TrackPoint = new Data(route.Shape[i].Latitude, route.Shape[i].Longitude,
                                (double)RoutingHelpers.check4NegativeValue(srtmData.GetElevation(route.Shape[i].Latitude, route.Shape[i].Longitude)),
                                Gesamtzeit + ((int)route.TotalTime / route.Shape.Length * (i + 1)),
                                Gesamtstrecke + ((int)route.TotalDistance / route.Shape.Length * (i + 1)));

                            EinzelRoute.Add(TrackPoint);
                        }

                        AllRoutes.Add(EinzelRoute);

                        Gesamtzeit += (int)route.TotalTime;
                        Gesamtstrecke += (int)route.TotalDistance;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ups, something went wrong...\n" + "Error code:\n" + ex.Message);
                        MouseKlicks.RemoveAt(MouseKlicks.Count - 1); // Cancle last mouse klick !
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
                else
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor; // Sanduhr wird für die Zeit des Routings angezeigt

                        List<Data> EinzelRoute = new List<Data>();

                        double Abstand = RoutingHelpers.GetDistanceBetweenTwoPoints(start_Lat, start_Lon, ziel_Lat, ziel_Lon);
                        double Zeit = 4;

                        // get the right routerDB profile (for time calculation).     
                        if (hike)
                        {
                            Zeit = Abstand / 4000 * 3600; // this is for pedestrian w/ 4km/h
                        }
                        if (cross)
                        {
                            Zeit = Abstand / 20000 * 3600; // this is for bike w/ 20km/h
                        }
                        if (race)
                        {
                            Zeit = Abstand / 40000 * 3600; // this is for car w/ 40km/h
                        }


                        var srtmData = new SRTM.SRTMData(strWorkPath + "\\SRTM_Data", new USGSSource());

                        //Data TrackPoint1 = new Data(start_Lat, start_Lon, (double)srtmData.GetElevationBilinear(start_Lat, start_Lon), Gesamtzeit, Gesamtstrecke);
                        //Data TrackPoint2 = new Data(ziel_Lat, ziel_Lon, (double)srtmData.GetElevationBilinear(ziel_Lat, ziel_Lon), Gesamtzeit + (int)Zeit, Gesamtstrecke + (int)Abstand);
                        // Bilinear methode is not stable! AND: It's not in NuGet-Package available -> To use this method one has to downlod latest version from GitHub! 

                        Data TrackPoint1 = new Data(start_Lat, start_Lon, (double)srtmData.GetElevation(start_Lat, start_Lon), Gesamtzeit, Gesamtstrecke);
                        Data TrackPoint2 = new Data(ziel_Lat, ziel_Lon, (double)srtmData.GetElevation(ziel_Lat, ziel_Lon), Gesamtzeit + (int)Zeit, Gesamtstrecke + (int)Abstand);

                        EinzelRoute.Add(TrackPoint1);
                        EinzelRoute.Add(TrackPoint2);

                        AllRoutes.Add(EinzelRoute);

                        Gesamtzeit += (int)Zeit;
                        Gesamtstrecke += (int)Abstand;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ups, something went wrong...\n" + "Error code:\n" + ex.Message);
                        MouseKlicks.RemoveAt(MouseKlicks.Count - 1); // Cancle last mouse klick !
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }

                Invalidate();

                textBox_Dis.Text = ((double)Gesamtstrecke / 1000).ToString("#0.000"); // Convert to km
                int Stunden = (int)Gesamtzeit / 3600;
                int Rest = (int)Gesamtzeit % 3600;
                int Minuten = Rest / 60;
                textBox_TotT.Text = Stunden.ToString() + "h " + Minuten.ToString() + "Minuten";

                make_Graph();
            }
        }

        private void newRoute()
        {
            MouseKlicks.Clear();
            AllRoutes.Clear();
            Gesamtstrecke = 0;
            Gesamtzeit = 0;

            localDate = DateTime.Now;

            textBox_Dis.Text = ((double)Gesamtstrecke / 1000).ToString("#0.000");
            int Stunden = (int)Gesamtzeit / 3600;
            int Rest = (int)Gesamtzeit % 3600;
            int Minuten = Rest / 60;
            textBox_TotT.Text = Stunden.ToString() + "h " + Minuten.ToString() + "Minuten";

            make_Graph();
        }
        //
        // ------------------------------------------------------------------------------------------------------------------------------
        //
        // Routing Eventhandler
        //
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!_newDB)
            {
                if (!_move) // no mouse_click event at the end of move event
                {
                    //PointF Punkt = _viewport.ScreenToWorld(_viewport.XToLongituide(e.X), _viewport.YToLatitude(e.Y));
                    PointF Punkt = _viewport.ScreenToWorld((e.X), (e.Y));

                    Data Klick = new Data(_viewport.YToLatitude(Punkt.Y), _viewport.XToLongituide(Punkt.X), 0, 0, 0);

                    MouseKlicks.Add(Klick);

                    if (MouseKlicks.Count > 1)
                    {
                        MakeRoute();
                    }
                }
            }
            else
            {
                _mousePosition = e.Location;
                Invalidate();
            }
        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_newDB)
            {
                string[] shape_files = Directory.GetFiles(strWorkPath + @"\Router_DB", "*.routerdb_All");

                for (int s = 0; s < all_RegShape.Count; s++)
                {
                    Point[] shape_Reg = new Point[all_RegShape[s].Count];

                    for (int i = 0; i < all_RegShape[s].Count; i++)
                    {
                        PointF Punkt_shape = _viewport.WorldToScreen(_viewport.LongitudeToX(Convert.ToDouble(all_RegShape[s][i][0], CultureInfo.InvariantCulture)),
                            _viewport.LatitudeToY(Convert.ToDouble(all_RegShape[s][i][1], CultureInfo.InvariantCulture)));

                        shape_Reg[i].X = (int)Punkt_shape.X;
                        shape_Reg[i].Y = (int)Punkt_shape.Y;
                    }

                    if (RoutingHelpers.IsPointInPolygon4(shape_Reg, _mousePosition))
                    {
                        load_routerDB(shape_files[s], true);
                        Invalidate();
                    }
                }
            }
        }
        //
        // ----------------------------------------------------------------------------------------------------------------------------------------
        // 
        // Routing Events (Buttons)
        //
        private void radioButton_Hik_CheckedChanged(object sender, EventArgs e)
        {
            hike = true;
            cross = false;
            race = false;
        }

        private void radioButton_Cross_CheckedChanged(object sender, EventArgs e)
        {
            hike = false;
            cross = true;
            race = false;
        }

        private void radioButton_Race_CheckedChanged(object sender, EventArgs e)
        {
            hike = false;
            cross = false;
            race = true;
        }

        private void checkBox_Boundary_Click(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void checkBox_DataBase_CheckedChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void button_newRoute_Click(object sender, EventArgs e)
        {
            newRoute();
        }

        private void button_SkipLastPart_Click(object sender, EventArgs e)
        {
            if (AllRoutes.Any()) //prevent IndexOutOfRangeException for empty list - code snippet from code.grepper.com
            {
                Gesamtstrecke = (int)AllRoutes[AllRoutes.Count - 1][0].Distance;
                Gesamtzeit = (int)AllRoutes[AllRoutes.Count - 1][0].Time;

                MouseKlicks.RemoveAt(MouseKlicks.Count - 1);
                AllRoutes.RemoveAt(AllRoutes.Count - 1);

                textBox_Dis.Text = ((double)Gesamtstrecke / 1000).ToString("#0.000") + " km";
                int Stunden = (int)Gesamtzeit / 3600;
                int Rest = (int)Gesamtzeit % 3600;
                int Minuten = Rest / 60;
                textBox_TotT.Text = Stunden.ToString() + "h " + Minuten.ToString() + " Minuten";

                Invalidate();
                // bereits in make_Graph einthalten

                make_Graph();
            }
        }

        private void button_LoadGPX_Click(object sender, EventArgs e)
        {
            MouseKlicks.Clear();
            AllRoutes.Clear();
            Gesamtstrecke = 0;
            Gesamtzeit = 0;

            Track_Load();

            minX_Value = 1000;
            maxX_Value = 1001;
            make_Graph();
        }

        private void button_CenterTrack_Click(object sender, EventArgs e)
        {
            Center_Track(AllRoutes);
        }

        private void button_saveGPX_Click(object sender, EventArgs e)
        {
            RoutingIO.save_Track(AllRoutes, localDate);

        }

        private void button_saveImage_Click(object sender, EventArgs e)
        {
            Safe_Image();
        }
        private void button_newRouteDB_Click(object sender, EventArgs e)
        {
            if (!_newDB)
            {
                _newDB = true;
                button_newRouteDB.Text = "Back to Routing-Modus";
            }
            else
            {
                _newDB = false;
                button_newRouteDB.Text = "Load new Route-DataBase";
            }
            /*
            using (OpenFileDialog ofDlg = new OpenFileDialog())
            {
                // Start with the right directory...
                ofDlg.InitialDirectory = root_path + "\\source\\repos\\00_Data\\04_Landkarte";
                // open file...
                ofDlg.Filter = "Select RouterDB file|*_All|All files|*.*";
                if (ofDlg.ShowDialog() == DialogResult.OK)
                {
                    load_routerDB(ofDlg.FileName, true);
                }
            }
            */
            Invalidate();
        }

        //
        // -----------------------------------------------------------------------------------------------------------------
        //
        // Elevation chart (chart_ele) part of code 
        //
        // Elevation - Eventhandler
        //
        private void chart_ele_MouseDown(object sender, MouseEventArgs e)
        {
            // Methode for marking elevation data...
            // Determine the initial coordinates...
            Start.X = e.X;
            RcDraw.X = e.X;
            Start.Y = -1000; // rectangle will be larger than chart_ele -> getting two vertical lines
            RcDraw.Y = -1000; // rectangle will be larger than chart_ele -> getting two vertical lines
            RcDraw.Height = 2000; // rectangle will be larger than chart_ele -> getting two vertical lines
            mouseDown = true;
            Marker_Pen.Color = Color.Red;
            label_delta.Text = "";
            label_delta.Visible = true;
        }

        private void chart_ele_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                // Determine the width of the rectangle...
                if (e.X < Start.X)
                {
                    RcDraw.Width = Start.X - e.X;
                    RcDraw.X = e.X;
                }
                else
                {
                    RcDraw.Width = e.X - Start.X;
                }

                chart_ele.Invalidate();
            }
        }

        private void chart_ele_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            bool first_Hit = true;

            minX_Value = chart_ele.ChartAreas[0].AxisX.PixelPositionToValue(RcDraw.X);
            maxX_Value = chart_ele.ChartAreas[0].AxisX.PixelPositionToValue(RcDraw.X + RcDraw.Width);

            for (int i = 0; i < AllRoutes.Count; i++)
            {
                for (int k = 0; k < AllRoutes[i].Count; k++)
                {
                    if (minX_Value < (AllRoutes[i][k].Distance / 1000) && first_Hit)
                    {
                        first_Hit = false;
                        minX_Alt = AllRoutes[i][k].Alt;
                    }
                    if (maxX_Value > (AllRoutes[i][k].Distance / 1000))
                    {
                        maxX_Alt = AllRoutes[i][k].Alt;
                    }
                }
            }

            Marker_Pen.Color = Color.Transparent;
            chart_ele.Invalidate();

            label_delta.Text = " Delta: " + (maxX_Value - minX_Value).ToString("###0.##km") + " / " + (maxX_Alt - minX_Alt).ToString("###0m")
                    + " / " + ((maxX_Alt - minX_Alt) / (maxX_Value - minX_Value) / 1000).ToString("#0%");

            make_Graph();
        }

        private void chart_ele_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Marker_Pen, RcDraw);
            Invalidate();
        }
        //
        // Elevation chart - method
        //
        private void make_Graph()
        {
            this.chart_ele.Series.Clear();
            Series series = this.chart_ele.Series.Add("Elevation");
            Series series2 = this.chart_ele.Series.Add("Marked");

            // series.ChartType = SeriesChartType.Spline;
            series.ChartType = SeriesChartType.FastLine;//  Line;
            series2.ChartType = SeriesChartType.Line;
            series2.BorderWidth = 3;

            series.Color = Color.Blue;
            series2.Color = Color.Red;
            chart_ele.ChartAreas[0].AxisY.LabelStyle.Format = "{#####}m";
            chart_ele.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Microsoft Sans Serif", 8, FontStyle.Regular, GraphicsUnit.Point, 0);
            chart_ele.ChartAreas[0].AxisX.LabelStyle.Format = "{####0.#}km";

            if (MouseKlicks.Count > 1)
            {
                for (int i = 0; i < AllRoutes.Count; i++)
                {
                    for (int k = 0; k < AllRoutes[i].Count; k++)
                    {
                        if (maxX_Value > AllRoutes[i][k].Distance / 1000 && minX_Value < AllRoutes[i][k].Distance / 1000)
                        {
                            series2.Points.AddXY(AllRoutes[i][k].Distance / 1000, AllRoutes[i][k].Alt);
                            if (minAlt > AllRoutes[i][k].Alt)
                            {
                                minAlt = AllRoutes[i][k].Alt;
                            }
                            if (maxAlt < AllRoutes[i][k].Alt)
                            {
                                maxAlt = AllRoutes[i][k].Alt;
                            }
                        }
                        series.Points.AddXY(AllRoutes[i][k].Distance / 1000, AllRoutes[i][k].Alt);
                    }
                }
                chart_ele.ChartAreas[0].AxisY.Maximum = RoutingHelpers.getmaxEle(AllRoutes) * 1.02; // sets the Maximum + 2%
                chart_ele.ChartAreas[0].AxisY.Minimum = RoutingHelpers.getminEle(AllRoutes) * 0.98; // sets the Minimum - 2%
                chart_ele.ChartAreas[0].RecalculateAxesScale(); // falls bei laufendem Programm Höhenpunkte dazu kommen...
            }
        }

        // 
        // -----------------------------------------------------------------------------------------------------------------------------
        // 
        // MapControl Part of Code
        // 

        //
        // MapControl Fields
        //
        private Fetcher<Image> _fetcher;
        private Renderer _renderer;
        private readonly MemoryCache<Tile<Image>> _tileCache = new MemoryCache<Tile<Image>>(200, 300);
        private ITileSource _tileSource;
        private Point _previousMousePosition;
        private Viewport _viewport = null;

        private bool _down = false;
        private bool _move = false;

        private Bitmap _buffer;

        //
        // -------------------------------------------------------------------------------------------------------------------------------
        //
        // MapControl Methods
        //

        private void InitializeMapControl()
        {
            _buffer = new Bitmap(Width, Height);
            _route = new Bitmap(Width, Height);

            _renderer = new Renderer(_buffer, _route);

            //_tileSource = KnownTileSources.Create();
            // OSM source
            _tileSource = new HttpTileSource(new GlobalSphericalMercator(0, 19), "https://tile.thunderforest.com/landscape/{z}/{x}/{y}.png?apikey=" + API_key, new[] { "a", "b", "c" }, "OSM");

            _fetcher = new Fetcher<Image>(_tileSource, _tileCache);
            _fetcher.DataChanged += FetcherOnDataChanged;
        }

        private static bool TryInitializeViewport(ref Viewport viewport, double actualWidth, double actualHeight, ITileSchema schema)
        {
            if (double.IsNaN(actualWidth)) return false;
            if (actualWidth <= 0) return false;

            //var nearestLevel = Utilities.GetNearestLevel(schema.Resolutions, schema.Extent.Width / actualWidth);
            var nearestLevel = Utilities.GetNearestLevel(schema.Resolutions, 305.748113086f);
            // Manipuliere so dass start mit BW scale erfolgt... -> Zoom = 8 -> 1222.992452344f / Zoom = 9 -> 305.748113086f
            viewport = new Viewport
            {
                Width = actualWidth,
                Height = actualHeight,
                UnitsPerPixel = schema.Resolutions[nearestLevel].UnitsPerPixel,
                //Center = new Point((int)schema.Extent.CenterX, (int)schema.Extent.CenterY)
                Center = new Point((int)(9.1 * 20037508 / 180), (int)((System.Math.Log(System.Math.Tan((48.5 + 90) / 360 * System.Math.PI)) / System.Math.PI * 180) * 20037508 / 180)) // eventuell kann auch PointD benutzt werden
                // Manipuliere so dass start mit Fokus auf BW erfolgt... -> Lon = 9.1 und Lat = 48.5 (Y ist wegen Mercator-Korrektur etwas komplexer...
            };
            return true;
        }

        public void SetTileSource(ITileSource source, double unitsPerPixel_old, double centerOldX, double centerOldY)
        {
            //_fetcher.DataChanged -= FetcherOnDataChanged;
            _fetcher.AbortFetch();

            _tileSource = source;
            _viewport.CenterX = centerOldX;
            _viewport.CenterY = centerOldY;
            _viewport.UnitsPerPixel = unitsPerPixel_old;
            _tileCache.Clear();
            _fetcher = new Fetcher<Image>(_tileSource, _tileCache);
            _fetcher.DataChanged += FetcherOnDataChanged;
            _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel); // start fetching...
        }
        //
        // ------------------------------------------------------------------------------------------------------------------------------
        //
        // MapControl Eventhandler
        //
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null) return;

            if (_viewport == null)
            {
                if (!TryInitializeViewport(ref _viewport, this.Width, this.Height, _tileSource.Schema)) return;
                _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel); // start fetching when viewport is first initialized
            }

            _renderer.Render(_viewport, _tileSource, _tileCache, AllRoutes, minX_Value, maxX_Value, RegShape, checkBox_Boundary.Checked,
                all_RegShape, _mousePosition, _newDB, all_srtmShape);

            e.Graphics.DrawImage(_buffer, 0, 0);
            e.Graphics.DrawImage(_route, 0, 0);
        }

        private void FetcherOnDataChanged(object sender, DataChangedEventArgs<Image> e)
        {
            if (!InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate ()
                {
                    FetcherOnDataChanged(sender, e);

                });
            }
            // code sniplet from https://smehrozalam.wordpress.com/2009/11/24/control-invoke-and-begininvoke-using-lamba-and-anonymous-delegates/
            else
            {
                if (e.Error == null && e.Tile != null)

                {
                    e.Tile.Image = TileToImage(e.Tile.Data);
                    _tileCache.Add(e.Tile.Info.Index, e.Tile);
                    Invalidate();
                }
            }
        }

        private static Image TileToImage(byte[] tile)
        {
            var stream = new MemoryStream(tile);
            var image = Image.FromStream(stream);
            return image;
        }

        //
        // ------------------------------------------------------------------------------------------------------------------------------
        //
        // MapControl Events (Buttons)
        //
        private void radioButton_Map_CheckedChanged(object sender, EventArgs e)
        {
            SetTileSource(new HttpTileSource(new GlobalSphericalMercator(0, 19),
                "https://tile.thunderforest.com/landscape/{z}/{x}/{y}.png?apikey=" + API_key, new[] { "a", "b", "c" }, "OSM"),
                _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);

            //SetTileSource(KnownTileSources.Create(), _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);
        }
        private void radioButton_Sat_CheckedChanged(object sender, EventArgs e)
        {
            SetTileSource(KnownTileSources.Create(KnownTileSource.BingAerial), _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);
        }

        private void radioButton_Hyb_CheckedChanged(object sender, EventArgs e)
        {
            SetTileSource(KnownTileSources.Create(KnownTileSource.BingHybrid), _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);
        }

        //
        // ----------------------------------------------------------------------------------------------------------------
        //
        // MapControl Overrides
        //

        protected override void OnMouseLeave(EventArgs e)
        {
            _previousMousePosition = new Point();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _down = false;
            _move = false;
            _previousMousePosition = new Point();
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _down = true;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!_down)
            {
                return;
            }

            if (_previousMousePosition == new Point())
            {
                _previousMousePosition = e.Location;
                return; // It turns out that sometimes MouseMove+Pressed is called before MouseDown
            }

            var currentMousePosition = e.Location; //Needed for both MouseMove and MouseWheel event
            _viewport.Transform(currentMousePosition.X, currentMousePosition.Y, _previousMousePosition.X, _previousMousePosition.Y);
            _previousMousePosition = currentMousePosition;
            _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel);

            _move = true;

            Invalidate();

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                _viewport.UnitsPerPixel = ZoomHelper.ZoomIn(_tileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList(), _viewport.UnitsPerPixel);
            }
            else if (e.Delta < 0)
            {
                _viewport.UnitsPerPixel = ZoomHelper.ZoomOut(_tileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList(), _viewport.UnitsPerPixel);
            }

            _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel);
            //e.Handled = true; //so that the scroll event is not sent to the html page.

            Invalidate();
            base.OnMouseWheel(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (_viewport == null) return;
            _viewport.Width = Width;
            _viewport.Height = Height;
            _buffer = new Bitmap(Width, Height);
            _route = new Bitmap(Width, Height);
            _renderer = new Renderer(_buffer, _route);
            _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel);

            Invalidate();
            base.OnResize(e);
        }


    }
}
