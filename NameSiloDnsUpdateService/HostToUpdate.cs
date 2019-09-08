namespace NameSiloDnsUpdateService
{
    public class HostToUpdate
    {
        public string Host { get; private set; }
        public string Domain { get; set; }
        public int Hours { get; private set; }
        public int Minutes { get; private set; }
        public int Seconds { get; private set; }
    }
}
