using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RPS
{
    class Gpx2RPS
    {
        public List<List<Data>> WriteGPX2RPS(GpxTrack track_)
        {
            List<List<Data>> _newAllroutes = new List<List<Data>>();

            foreach (GpxTrackSegment _segment in track_.Segments)
            {
                List<Data> new_Route = new List<Data>();
                bool firstTrackPoint = true;

                if (_segment.TrackPoints != null)
                {

                    double dummy_lon_old = 0; // for distance calculation
                    double dummy_lat_old = 0;
                    double dummy_dist_old = 0;

                    foreach (GpxTrackPoint trackpoint_ in _segment.TrackPoints)
                    {
                        Data _newData = new Data();

                        if (firstTrackPoint)
                        {
                            firstTrackPoint = false;

                            _newData.Lat = trackpoint_.Latitude;
                            _newData.Lon = trackpoint_.Longitude;
                            if (trackpoint_.Elevation != null) _newData.Alt = (double)trackpoint_.Elevation;
                            _newData.Time = 0;
                            _newData.Distance = 0;
                            _newData.Road = 0;
                            _newData.Rought_Road = 0;
                            _newData.Path = 0;
                        }
                        else
                        {
                            _newData.Lat = trackpoint_.Latitude;
                            _newData.Lon = trackpoint_.Longitude;
                            if (trackpoint_.Elevation != null) _newData.Alt = (double)trackpoint_.Elevation;

                            dummy_dist_old = dummy_dist_old + RoutingHelpers.GetDistanceBetweenTwoPoints(dummy_lat_old, dummy_lon_old, trackpoint_.Latitude, trackpoint_.Longitude);
                            _newData.Distance = ((int)Math.Round(dummy_dist_old));

                            _newData.Time = 0;
                            // Hier muss noch Road, RougthRoad und Path in GPX definiert werden eventuell in -> GPXPoint oder einfach als zusätzliche Abfrage im Reader 
                            if (trackpoint_.Road != null) _newData.Road = (double)trackpoint_.Road;
                            if (trackpoint_.Rought_Road != null) _newData.Rought_Road = (double)trackpoint_.Rought_Road;
                            if (trackpoint_.Path != null) _newData.Path = (double)trackpoint_.Path;
                        }

                        dummy_lat_old = trackpoint_.Latitude;
                        dummy_lon_old = trackpoint_.Longitude;

                        new_Route.Add(_newData);
                    }
                }

                _newAllroutes.Add(new_Route);
            }

            return _newAllroutes;
        }
    }
}
