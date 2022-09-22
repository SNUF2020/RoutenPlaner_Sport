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
using System.Net.Mail;

// Code by S.Nuf.2022 (MIT Licence)
// RPS = Routen Planer Sport, Version5.0
//
// Structure of code: First part = Routing etc. / second part = elevation chart / Third part = MapControl
//
// Ver.1:
//  Source of MapControl: see GitHub Project BruTile > Samples > BruTile.Demo (WPF) -> Adapted to WinForm   
//  Map data: Thunderforest (API key necessary) or OSM and Bing (w/o API key)
//  Doku Routenplaner: https://docs.itinero.tech/docs/osmsharp/index.html
//  Raw data for routerDB (osm.pbf files): http://download.geofabrik.de/
//  Embedding of elevation data: https://github.com/itinero/srtm 
//  Elevation data from http://viewfinderpanoramas.org/dem3.html (90m-resolution, Alpes 30m) and (better): 
//   from https://e4ftl01.cr.usgs.gov/MEASURES/SRTMGL1.003/2000.02.11/ (NASA, world-wide with 30m-resolution)
//   Problem at both servers: Downloading of data at running program does not work -> Since app. 2014 from server-side disabled 
//   -> Data will be downloaded to local disc (at NASA server password required)
//   All elevation data are stored in SRTM_Data at SRTM-folder
//
// Ver.2:
//  Categorize surface of route (for my hikking buddys, sometimes complaining over to much asphalt streets) - Definitions:
//   Property "Highway = track" means "Rough road normally used for agricultural or forestry uses" = Feld-/Forstweg
//   Property "Highway = path" means " hiking path/trail" = Wanderweg
//   All other highway properties = solid road = asphaltierte Straße
//   See also: https://wiki.openstreetmap.org/wiki/Highways and https://wiki.openstreetmap.org/wiki/Hiking
//
// Ver.3:
//  Implementation of GpxReader.cs etc.: Source code based on dlg.krakow.pl code (copyright (c) 2011-2016, dlg.krakow.pl) 
//  Doku GPX-Writer/Reader: https://github.com/macias/Gpx/blob/master/Gpx/Implementation/GpxWriter.cs
//
// Ver.4:
//  Implementation of mail-forward funtion (sending GPX file to specific mail-adress)
//  Implementaion of elevation (ascent and descent) -> Savitzky-Golay Filter w/ dynamic range at edges of trial data
//  Elevation information will replace duration information 
//
// Ver.5:
//  Implementation of config-file (see class ConfigFileReader)

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

            ConfigFile_Load();
            InitializeMapControl();
            InitializeRouting();
        }
      
        // General methods
        //
        private void ConfigFile_Load()
        {
            try
            {
                using (ConfigFileReader reader = new ConfigFileReader(new FileStream("RPS_ConfigFile.txt", FileMode.Open)))
                {
                    API_key = reader.ConfigContent.ApiKey_Landscape;
                    Mail_key = reader.ConfigContent.ApiKey_MailKey;
                    Mail_Username = reader.ConfigContent.ApiKey_MailUser;
                    // 
                    Initial_Lon = Convert.ToDouble(reader.ConfigContent.StartPoint_Lon, CultureInfo.InvariantCulture);
                    Initial_Lat = Convert.ToDouble(reader.ConfigContent.StartPoint_Lat, CultureInfo.InvariantCulture);
                    Initial_Zoom = Convert.ToDouble(reader.ConfigContent.StartPoint_Zoom, CultureInfo.InvariantCulture);
                    Initial_RouterDB = reader.ConfigContent.StarRouter_DB;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " ConfigFile_Load");
            }
        } // Initial load of config file

        // --------------------------------------------------------------------------------------------------------------
        //
        // Routing code
        //

        //
        // Routing Fields
        //
        //private string root_path;

        // For general code version: Working directory -> ".../bin"
        private static string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        // All relevant data is stored in directories below this path.
        // - Router_DB
        // - SRTM_Data
        // - Tracks
        
        // Map and email -> See config file
        private string API_key; // for thunderstorm maps, etc.
        private string Mail_key;
        private string Mail_Username;

        // Initial starting point = BW, Stuttgart-Rohr -> see config file 
        private string Initial_RouterDB;
        private double Initial_Lon;
        private double Initial_Lat;
        private double Initial_Zoom;

        int Gesamtstrecke = 0;
        double GesamtAsphalt = 0;
        double GesamtWeg = 0;
        double GesamtPfad = 0;

        // Shape of routerDB, all routerDB's and all SRTM areas
        List<string[]> RegShape = new List<string[]>();
        List<List<string[]>> all_RegShape = new List<List<string[]>>();
        List<string[]> all_srtmShape = new List<string[]>();

        List<List<Data>> AllRoutes = new List<List<Data>>();
        List<Data> MouseKlicks = new List<Data>();

        DateTime localDate = DateTime.Now;
        int Gesamtzeit = 0;

        RouterDb routerDb = null;
        Itinero.Profiles.Profile profile = null;

        private Bitmap _route;

        private Point _mousePosition;

        private bool hike = true;
        private bool cross = false;
        private bool race = false;
        private bool _newDB = false;
        
        //
        // Gpx reading
        bool readerProcess_ok = false;
        
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
            Load_routerDB(strWorkPath + @"/Router_DB/" + Initial_RouterDB, false);

            all_RegShape = RoutingHelpers.FetchShapeFiles();
            all_srtmShape = RoutingHelpers.FetchSRTMFiles();
        }

        private void Load_routerDB(string _filename, bool _message)
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

        private void Calc_Elevation(List<double> data_list)
        {
            List<double> _data_list = RoutingHelpers.SavitzkyGolayFilter(data_list);

            double up = 0;
            double down = 0;

            double old_value = _data_list[0];

            for (int i = 1; i < _data_list.Count; i++)
            {
                double diff = _data_list[i] - old_value;
                old_value = _data_list[i];
                if (diff >= 0)
                {
                    up += diff;
                }
                else
                {
                    down += diff;
                }
            }

            textBox_ElevUp.Text = up.ToString("#0") + " m";
            textBox_ElevDown.Text = down.ToString("#0") + " m";
        }

        private void Get_SurfaceInformation(List<Data> Input)
        {
            List<Data> _Input = Input;

            GesamtAsphalt = 0;
            GesamtWeg = 0;
            GesamtPfad = 0;

            for (int i = 0; i < _Input.Count; i++)
            {
                if (_Input[i].Road > GesamtAsphalt) GesamtAsphalt = _Input[i].Road;
                if (_Input[i].Rought_Road > GesamtWeg) GesamtWeg = _Input[i].Rought_Road;
                if (_Input[i].Path > GesamtPfad) GesamtPfad = _Input[i].Path;
            }

            // last track segemnet is not in - therefore additional procedure...

            GesamtAsphalt += _Input[_Input.Count - 1].Road;
            GesamtWeg += _Input[_Input.Count - 1].Rought_Road;
            GesamtPfad += _Input[_Input.Count - 1].Path;
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

        private List<List<Data>> Track_Load(List<List<Data>> _AllRoutes)
        {
            List<List<Data>> newAllRoutes = new List<List<Data>>(_AllRoutes);
            readerProcess_ok = false;

            try
            {
                using (OpenFileDialog ofDlg = new OpenFileDialog())
                {
                    ofDlg.InitialDirectory = strWorkPath + "/Tracks";
                    //ofDlg.InitialDirectory = root_path + Tracks_Dir;
                    ofDlg.Filter = "GPX Files (*.gpx)|*.gpx|All Files (*.*)|*.*";
                    if (ofDlg.ShowDialog() == DialogResult.OK)
                    {
                        Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                        using (GpxReader reader = new GpxReader(new FileStream(ofDlg.FileName, FileMode.Open)))
                        {
                            Gpx2RPS writer = new Gpx2RPS();

                            while (reader.Read())
                            {

                                switch (reader.ObjectType)
                                {
                                    case GpxObjectType.Track:

                                        newAllRoutes = writer.WriteGPX2RPS(reader.Track);
                                        break;
                                }
                            }

                            readerProcess_ok = true;
                        }
                    }
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

            return newAllRoutes;
        }
    
        private Bitmap Plot_Image()
        {
            Bitmap fusionImage = new Bitmap(Width, Height);
            Bitmap chart_image = new Bitmap(chart_ele.Width, chart_ele.Height, PixelFormat.Format32bppArgb);

            using (Graphics grfx = Graphics.FromImage(fusionImage))
            {
                grfx.DrawImage(_buffer, new Rectangle(0, 0, _buffer.Width, _buffer.Height));
                grfx.DrawImage(_route, new Rectangle(0, 0, _route.Width, _route.Height));

                // Plot route information top left into image...
                SolidBrush whiteBrush = new SolidBrush(Color.White);

                string map_text = "Distance: " + textBox_Dis.Text + " / Expected Elevetion: " + textBox_ElevUp.Text
                    + " / Road: " + textBox_road.Text + " / Way: " + textBox_way.Text + " / Path: " + textBox_path.Text;
                Font drawFont = new Font("Microsoft Sans Serif", 12, FontStyle.Regular, GraphicsUnit.Point, 0);

                // getting the right dimension of the text background box... (using TextRenderer.MeasureText method)
                Size size = TextRenderer.MeasureText(map_text, drawFont);
                Rectangle Rahmen = new Rectangle(10, 10, size.Width + 35, size.Height);

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
            //saveFileDialog1.InitialDirectory = root_path + Tracks_Dir;
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
                    Plot_Image().Save(fs, imageFormat);
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
                        Cursor = Cursors.WaitCursor; // Sand glas will be shown for endurance of routing process

                        // create a routerpoint from a location.                             
                        // snaps the given location to the nearest routable edge.
                        var start = router.Resolve(profile, (float)start_Lat, (float)start_Lon, 250); // 250m radius for search of next routing point in routing db
                        var end = router.Resolve(profile, (float)ziel_Lat, (float)ziel_Lon, 250); // standard = 50m is to less 

                        var route = router.Calculate(profile, start, end);

                        // Elevation data from SRTM - data-log to NASA sever does not work -> since 2014 NASA blocked direct log! 
                        // SRTMData srtmData = new SRTMData(root_path + "\\source\\repos\\00_Data\\16_SRTM", new NASASource(credentials));

                        using (var writer = new StreamWriter(@"route.XML"))
                        {
                            route.WriteXml(writer);
                        }

                        XmlDocument doc = new XmlDocument();
                        doc.Load(@"route.XML");
                        XmlNodeList elemList = doc.GetElementsByTagName("meta");

                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Indent = true;
                        settings.NewLineOnAttributes = true;

                        double Strasse_Asphalt = 0;
                        double Feld_ForstWeg = 0;
                        double WanderWeg_Pfad = 0;

                        string _route = "";

                        double helpVariable_dist = 0;
                        for (int i = 0; i < elemList.Count; i++)
                        {
                            for (int k = 0; k < elemList[i].ChildNodes.Count; k++)
                            {
                                if (elemList[i].ChildNodes[k].Attributes[0].Value.ToString() == "highway")
                                {
                                    switch (elemList[i].ChildNodes[k].Attributes[1].Value.ToString())
                                    {
                                        case "path":
                                            _route = "path";
                                            break;
                                        case "track":
                                            _route = "track";
                                            break;
                                        default:
                                            _route = "road";
                                            break;
                                    }
                                }
                                if (elemList[i].ChildNodes[k].Attributes[0].Value.ToString() == "distance")
                                {
                                    switch (_route)
                                    {
                                        case "path":
                                            WanderWeg_Pfad += (Convert.ToDouble(elemList[i].ChildNodes[k].Attributes[1].Value.ToString(), CultureInfo.InvariantCulture) - helpVariable_dist);
                                            break;
                                        case "track":
                                            Feld_ForstWeg += (Convert.ToDouble(elemList[i].ChildNodes[k].Attributes[1].Value.ToString(), CultureInfo.InvariantCulture) - helpVariable_dist);
                                            break;
                                        case "road":
                                            Strasse_Asphalt += (Convert.ToDouble(elemList[i].ChildNodes[k].Attributes[1].Value.ToString(), CultureInfo.InvariantCulture) - helpVariable_dist);
                                            break;
                                    }
                                    helpVariable_dist = Convert.ToDouble(elemList[i].ChildNodes[k].Attributes[1].Value.ToString(), CultureInfo.InvariantCulture);
                                }
                            }
                        }

                        var srtmData = new SRTM.SRTMData(strWorkPath + "\\SRTM_Data", new USGSSource());

                        List<Data> EinzelRoute = new List<Data>();

                        // First element with special treatment (getting time and distance values from prior shape) -> for "skip-last-klick" methode 
                        
                        Data TrackPoint0 = new Data(route.Shape[0].Latitude, route.Shape[0].Longitude,
                                (double)RoutingHelpers.Check4NegativeValue(srtmData.GetElevation(route.Shape[0].Latitude, route.Shape[0].Longitude)),
                                Gesamtzeit, Gesamtstrecke, GesamtAsphalt, GesamtWeg, GesamtPfad);

                        EinzelRoute.Add(TrackPoint0);

                        for (int i = 1; i < route.Shape.Length; i++)
                        {

                            Data TrackPoint = new Data(route.Shape[i].Latitude, route.Shape[i].Longitude,
                                (double)RoutingHelpers.Check4NegativeValue(srtmData.GetElevation(route.Shape[i].Latitude, route.Shape[i].Longitude)),
                                Gesamtzeit + (int)(route.TotalTime / route.Shape.Length * (i + 1)), Gesamtstrecke + (int)(route.TotalDistance / route.Shape.Length * (i + 1)), 
                                Strasse_Asphalt, Feld_ForstWeg, WanderWeg_Pfad);

                            EinzelRoute.Add(TrackPoint);
                        }

                        AllRoutes.Add(EinzelRoute);

                        Gesamtzeit += (int)route.TotalTime;
                        Gesamtstrecke += (int)route.TotalDistance;
                        GesamtAsphalt += Strasse_Asphalt;
                        GesamtWeg += Feld_ForstWeg;
                        GesamtPfad += WanderWeg_Pfad;
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
                        Cursor = Cursors.WaitCursor; // Sand glass will be shown while SRTM processing

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
                        GesamtPfad += Abstand; // Assumption: all off-road distances are path
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

                Calc_Elevation(RoutingHelpers.GetEleData(AllRoutes));

                textBox_Dis.Text = ((double)Gesamtstrecke / 1000).ToString("#0.000") + " km";
                textBox_road.Text = (GesamtAsphalt / 1000).ToString("#0.000");
                textBox_way.Text = (GesamtWeg / 1000).ToString("#0.000") + " km";
                textBox_path.Text = (GesamtPfad / 1000).ToString("#0.000") + " km";

                Make_Graph();
            }
        }

        private void NewRoute()
        {
            MouseKlicks.Clear();
            AllRoutes.Clear();
            Gesamtstrecke = 0;
            Gesamtzeit = 0;
            GesamtAsphalt = 0;
            GesamtWeg = 0;
            GesamtPfad = 0;

            localDate = DateTime.Now;

            textBox_Dis.Text = ((double)Gesamtstrecke / 1000).ToString("#0.000");
            textBox_ElevUp.Text = "0 m";
            textBox_ElevDown.Text = "0 m";
            
            textBox_road.Text = (GesamtAsphalt / 1000).ToString("#0.000");
            textBox_way.Text = (GesamtWeg / 1000).ToString("#0.000") + " km";
            textBox_path.Text = (GesamtPfad / 1000).ToString("#0.000") + " km";

            Make_Graph();
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
                //string[] shape_files = Directory.GetFiles(root_path + @RouterDB_Dir, "*.routerdb_All");

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
                        Load_routerDB(shape_files[s], true);
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
        private void RadioButton_Hik_CheckedChanged(object sender, EventArgs e)
        {
            hike = true;
            cross = false;
            race = false;
        }

        private void RadioButton_Cross_CheckedChanged(object sender, EventArgs e)
        {
            hike = false;
            cross = true;
            race = false;
        }

        private void RadioButton_Race_CheckedChanged(object sender, EventArgs e)
        {
            hike = false;
            cross = false;
            race = true;
        }

        private void CheckBox_Boundary_Click(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void CheckBox_DataBase_CheckedChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void Button_newRoute_Click(object sender, EventArgs e)
        {
            NewRoute();
        }

        private void Button_SkipLastPart_Click(object sender, EventArgs e)
        {
            if (AllRoutes.Any()) //prevent IndexOutOfRangeException for empty list - code snippet from code.grepper.com
            {

                if (MouseKlicks.Count > 2) Gesamtstrecke = (int)AllRoutes[AllRoutes.Count - 1][0].Distance;
                else Gesamtstrecke = 0;

                Gesamtzeit = (int)AllRoutes[AllRoutes.Count - 1][0].Time;
                GesamtAsphalt = AllRoutes[AllRoutes.Count - 1][0].Road;
                GesamtWeg = AllRoutes[AllRoutes.Count - 1][0].Rought_Road;
                GesamtPfad = AllRoutes[AllRoutes.Count - 1][0].Path;

                MouseKlicks.RemoveAt(MouseKlicks.Count - 1);
                AllRoutes.RemoveAt(AllRoutes.Count - 1);

                Calc_Elevation(RoutingHelpers.GetEleData(AllRoutes));

                textBox_Dis.Text = ((double)Gesamtstrecke / 1000).ToString("#0.000") + " km";
                textBox_road.Text = (GesamtAsphalt / 1000).ToString("#0.000");
                textBox_way.Text = (GesamtWeg / 1000).ToString("#0.000") + " km";
                textBox_path.Text = (GesamtPfad / 1000).ToString("#0.000") + " km";

                Invalidate();
                // bereits in make_Graph einthalten

                Make_Graph();
            }
        }

        private void Button_LoadGPX_Click(object sender, EventArgs e)
        {
            AllRoutes = Track_Load(AllRoutes);

            if (readerProcess_ok) // only if reading of GPX file was successful
            {
                MouseKlicks.Clear();
                Gesamtstrecke = 0;
                Gesamtzeit = 0;
                Get_SurfaceInformation(AllRoutes[0]);

                Gesamtstrecke = (int)AllRoutes[0][AllRoutes[0].Count - 1].Distance;
                Gesamtzeit = AllRoutes[0][AllRoutes[0].Count - 1].Time;

                Calc_Elevation(RoutingHelpers.GetEleData(AllRoutes));

                textBox_Dis.Text = ((double)Gesamtstrecke / 1000).ToString("#0.000") + " km";
                textBox_road.Text = (GesamtAsphalt / 1000).ToString("#0.000");
                textBox_way.Text = (GesamtWeg / 1000).ToString("#0.000") + " km";
                textBox_path.Text = (GesamtPfad / 1000).ToString("#0.000") + " km";

                // Integrate mouse klick (2 klicks)
                Data Klick1 = new Data(AllRoutes[0][0].Lat, AllRoutes[0][0].Lon, 0, 0, 0);
                Data Klick2 = new Data(
                        AllRoutes[0][AllRoutes[0].Count - 1].Lat, AllRoutes[0][AllRoutes[0].Count - 1].Lon, 0, 0, 0);
                MouseKlicks.Add(Klick1);
                MouseKlicks.Add(Klick2);

                Center_Track(AllRoutes);

                minX_Value = 1000;
                maxX_Value = 1001;
                Make_Graph();
            }
        }

        private void Button_CenterTrack_Click(object sender, EventArgs e)
        {
            Center_Track(AllRoutes);
        }

        private void Button_saveGPX_Click(object sender, EventArgs e)
        {
            RoutingIO.Save_Track(AllRoutes, localDate);

        }

        private void Button_Mail_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofDlg = new OpenFileDialog())
            {
                ofDlg.InitialDirectory = strWorkPath + "/Tracks";
                ofDlg.Filter = "GPX Files (*.gpx)|*.gpx|PNG Image|*.png|Bitmap Image|*.bmp|JPEG Image|*.jpg|Gif Image|*.gif|All Files (*.*)|*.*";
                if (ofDlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        MailMaker mail = new MailMaker()
                        {
                            Absender = "stefan.nufer@t-online.de",
                            Empfänger = new List<string>() { "stefan.nufer@t-online.de" },
                            Kopie = new List<string>(),
                            Blindkopie = new List<string>(),
                            Betreff = Path.GetFileName(ofDlg.FileName).ToString(),
                            Nachricht = "Tour by SNuf" + DateTime.Today.Year.ToString(),
                            Servername = "securesmtp.t-online.de",
                            Port = "587",
                            Username = Mail_Username,
                            Passwort = Mail_key,
                            Anhänge = new List<Attachment> { new Attachment(ofDlg.FileName) }
                        };

                        mail.Send();

                        MessageBox.Show("File was sent to nufers@t-online.de");

                    }
                    catch (Exception ex)
                    { 
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        } // load data from xml-file and plot track

        private void Button_saveImage_Click(object sender, EventArgs e)
        {
            Safe_Image();
        }
        
        private void Button_newRouteDB_Click(object sender, EventArgs e)
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
           
            Invalidate();
        }

        //
        // -----------------------------------------------------------------------------------------------------------------
        //
        // Elevation chart (chart_ele) part of code 
        //
        // Elevation - Eventhandler
        //
        private void Chart_ele_MouseDown(object sender, MouseEventArgs e)
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

        private void Chart_ele_MouseMove(object sender, MouseEventArgs e)
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

        private void Chart_ele_MouseUp(object sender, MouseEventArgs e)
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

            Make_Graph();
        }

        private void Chart_ele_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Marker_Pen, RcDraw);
            Invalidate();
        }
        //
        // Elevation chart - method
        //
        private void Make_Graph()
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
                chart_ele.ChartAreas[0].AxisY.Maximum = RoutingHelpers.GetmaxEle(AllRoutes) * 1.02; // sets the Maximum + 2%
                chart_ele.ChartAreas[0].AxisY.Minimum = RoutingHelpers.GetminEle(AllRoutes) * 0.98; // sets the Minimum - 2%
                chart_ele.ChartAreas[0].RecalculateAxesScale(); // if new min/max values come in (e.g. while making new route)
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

            // Thunderforest source
            //_tileSource = new HttpTileSource(new GlobalSphericalMercator(0, 19), API_key, new[] { "a", "b", "c" }, "OSM");
            
            // OSM source
            _tileSource = KnownTileSources.Create(KnownTileSource.OpenStreetMap);

            _fetcher = new Fetcher<Image>(_tileSource, _tileCache);
            _fetcher.DataChanged += FetcherOnDataChanged;
        }

        private static bool TryInitializeViewport(ref Viewport viewport, double actualWidth, double actualHeight, ITileSchema schema, double _Initial_Lon, double _Initial_Lat, double _Initial_Zoom)
        {
            if (double.IsNaN(actualWidth)) return false;
            if (actualWidth <= 0) return false;

            //var nearestLevel = Utilities.GetNearestLevel(schema.Resolutions, schema.Extent.Width / actualWidth);
            var nearestLevel = Utilities.GetNearestLevel(schema.Resolutions, _Initial_Zoom);

        viewport = new Viewport
            {
                Width = actualWidth,
                Height = actualHeight,
                UnitsPerPixel = schema.Resolutions[nearestLevel].UnitsPerPixel,
                //Center = new Point((int)schema.Extent.CenterX, (int)schema.Extent.CenterY)
                Center = new Point((int)(_Initial_Lon * 20037508 / 180), (int)((System.Math.Log(System.Math.Tan((_Initial_Lat + 90) / 360 * System.Math.PI)) / System.Math.PI * 180) * 20037508 / 180)) // eventuell kann auch PointD benutzt werden
                // Manipuliere so dass start mit Fokus auf BW erfolgt... -> Initial_Lon = 9.1 und Initial_Lat = 48.5 (Y ist wegen Mercator-Korrektur etwas komplexer...)
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
                if (!TryInitializeViewport(ref _viewport, this.Width, this.Height, _tileSource.Schema, Initial_Lon, Initial_Lat, Initial_Zoom)) return;
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
        private void RadioButton_Map_CheckedChanged(object sender, EventArgs e)
        {
            //SetTileSource(new HttpTileSource(new GlobalSphericalMercator(0, 19), API_key, new[] { "a", "b", "c" }, "OSM"), _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);

            SetTileSource(KnownTileSources.Create(KnownTileSource.OpenStreetMap), _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);
        }
        
        private void RadioButton_Sat_CheckedChanged(object sender, EventArgs e)
        {
            SetTileSource(KnownTileSources.Create(KnownTileSource.BingAerial), _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);
        }

        private void RadioButton_Hyb_CheckedChanged(object sender, EventArgs e)
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
