namespace RPS
{
    class Data
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }
        public int Time { get; set; }
        public double Distance { get; set; }
       
        public double Road { get; set; }
        public double Rought_Road { get; set; }
        public double Path { get; set; }

        //
        // ----------------------------------------------------------------------------------------
        // 
        // Constructors
        public Data()
        {

        }

        public Data(double Breite, double Länge, double Höhe, int Zeit, double Strecke)
        {
            Lat = Breite;
            Lon = Länge;
            Alt = Höhe;
            Time = Zeit;
            Distance = Strecke;
        } // first overload - for initial construction of new location

        public Data(double _Lat, double _Lon, double _Alt, int _time, double _Dist, double _Road, double _Rought, double _Path)
        {
            Time = _time;
            Lat = _Lat;
            Lon = _Lon;
            Alt = _Alt;
            Distance = _Dist;
            Road = _Road;
            Rought_Road = _Rought;
            Path = _Path;

        } // second overload - for road surface documentation
    }
}
