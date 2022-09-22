using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace RPS
{
    class RoutingIO
    {
        public static List<string[]> Load_RegShape(string _shapeFile, List<string[]> _RegShape_act)
        {
            List<string[]> _RegShape = new List<string[]>();

            string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            try
            {
                // open file, here: UTF8
                using (StreamReader sr = new StreamReader(strWorkPath + @"\\Router_DB\\" + _shapeFile, Encoding.UTF8))
                {

                    // read file until end
                    while (!sr.EndOfStream)
                    {
                        // read single line and split with seperattor sign ";" into columns (stringarray)
                        string[] currentline = sr.ReadLine().Replace(",", ".").Split(new string[] { ";" }, StringSplitOptions.None);
                        _RegShape.Add(currentline);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return _RegShape_act;
            }

            //_RegShape_act.Clear();
            return _RegShape;
        }

        public static void Save_Track(List<List<Data>> _AllRoutes, DateTime _localDate)
        {
            if (_AllRoutes != null)
            {
                using (SaveFileDialog sFDlg = new SaveFileDialog())
                {
                    sFDlg.Filter = "Save route as GPX-file|*.gpx";
                    sFDlg.Title = "Save Routing File";

                    if (sFDlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            //Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                            XmlWriterSettings settings = new XmlWriterSettings();
                            settings.Indent = true;
                            settings.NewLineOnAttributes = true;

                            XmlWriter writer = XmlWriter.Create(@sFDlg.FileName, settings);

                            // Start-Element/-Attribute "gpx": code snippet from https://github.com/macias/Gpx/tree/master/Gpx 
                            const string GPX_VERSION = "1.1";
                            const string GPX_CREATOR = "RoutenPlanerSport";
                            const string GARMIN_EXTENSIONS_PREFIX = "gpxx";
                            const string GARMIN_WAYPOINT_EXTENSIONS_PREFIX = "gpxwpx";
                            const string GARMIN_TRACKPOINT_EXTENSIONS_V2_PREFIX = "gpxtpx";
                            const string DLG_EXTENSIONS_PREFIX = "dlg";
                            const string GPX_NAMESPACE = "http://www.topografix.com/GPX/1/1";
                            const string GARMIN_EXTENSIONS_NAMESPACE = "http://www.garmin.com/xmlschemas/GpxExtensions/v3";
                            // const string GARMIN_TRACKPOINT_EXTENSIONS_V1_NAMESPACE = "http://www.garmin.com/xmlschemas/TrackPointExtension/v1";
                            const string GARMIN_TRACKPOINT_EXTENSIONS_V2_NAMESPACE = "http://www.garmin.com/xmlschemas/TrackPointExtension/v2";
                            const string GARMIN_WAYPOINT_EXTENSIONS_NAMESPACE = "http://www.garmin.com/xmlschemas/WaypointExtension/v1";
                            const string DLG_EXTENSIONS_NAMESPACE = "http://dlg.krakow.pl/gpx/extensions/v1";

                            writer.WriteStartElement("gpx", GPX_NAMESPACE);
                            writer.WriteAttributeString("version", GPX_VERSION);
                            writer.WriteAttributeString("creator", GPX_CREATOR);
                            writer.WriteAttributeString("xmlns", GARMIN_EXTENSIONS_PREFIX, null, GARMIN_EXTENSIONS_NAMESPACE);
                            writer.WriteAttributeString("xmlns", GARMIN_WAYPOINT_EXTENSIONS_PREFIX, null, GARMIN_WAYPOINT_EXTENSIONS_NAMESPACE);
                            writer.WriteAttributeString("xmlns", GARMIN_TRACKPOINT_EXTENSIONS_V2_PREFIX, null, GARMIN_TRACKPOINT_EXTENSIONS_V2_NAMESPACE);
                            writer.WriteAttributeString("xmlns", DLG_EXTENSIONS_PREFIX, null, DLG_EXTENSIONS_NAMESPACE);

                            writer.WriteStartElement("trk");

                            writer.WriteStartElement("trkseg");


                            for (int i = 0; i < _AllRoutes.Count; i++)
                            {
                                for (int k = 0; k < _AllRoutes[i].Count; k++)
                                {
                                    writer.WriteStartElement("trkpt");
                                    writer.WriteAttributeString("lat", _AllRoutes[i][k].Lat.ToString().Replace(',', '.'));
                                    writer.WriteAttributeString("lon", _AllRoutes[i][k].Lon.ToString().Replace(',', '.'));

                                    writer.WriteStartElement("ele");
                                    writer.WriteString(_AllRoutes[i][k].Alt.ToString().Replace(',', '.'));
                                    writer.WriteEndElement(); // ele

                                    writer.WriteStartElement("time");
                                    // track time = seconds from track beginning - convert to start time
                                    writer.WriteString(_localDate.AddSeconds((double)_AllRoutes[i][k].Time).ToString("yyyy-MM-ddTHH':'mm':'ss.FFFZ"));
                                    writer.WriteEndElement(); // time                                    

                                    writer.WriteStartElement("road");
                                    writer.WriteString(_AllRoutes[i][k].Road.ToString().Replace(',', '.'));
                                    writer.WriteEndElement(); // way

                                    writer.WriteStartElement("way");
                                    writer.WriteString(_AllRoutes[i][k].Rought_Road.ToString().Replace(',', '.'));
                                    writer.WriteEndElement(); // way

                                    writer.WriteStartElement("path");
                                    writer.WriteString(_AllRoutes[i][k].Path.ToString().Replace(',', '.'));
                                    writer.WriteEndElement(); // way

                                    writer.WriteEndElement(); // trkpt
                                }
                            }

                            writer.WriteEndElement(); // trkseg

                            writer.WriteEndElement(); // trk

                            writer.WriteEndElement(); // gpx

                            // Write the XML and close the writer.
                            writer.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            //Cursor = Cursors.Default;
                        }
                    }
                }
            }
        }
    }
}
