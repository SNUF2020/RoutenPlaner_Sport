namespace RPS
{
    class Data
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }
        public int Time { get; set; }
        public double Distance { get; set; }

        //
        // ----------------------------------------------------------------------------------------
        // 
        // Constructors
        public Data(double Breite, double Länge, double Höhe, int Zeit, double Strecke)
        {
            Lat = Breite;
            Lon = Länge;
            Alt = Höhe;
            Time = Zeit;
            Distance = Strecke;
        } // first overload - for initial construction of new location
    }
}
