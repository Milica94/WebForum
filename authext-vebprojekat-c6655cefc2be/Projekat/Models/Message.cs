namespace Projekat.Models
{
    public class Message
    {
        public uint Id { get; set; }
        public string FromName { get; set; }
        public string ToName { get; set; }
        public string Content { get; set; }
        public bool Read { get; set; }
    }

    public class FromMessage
    {
        public uint Id { get; set; }
        public string ToName { get; set; }
    }

    public class ToMessage
    {
        public uint Id { get; set; }
        public string FromName { get; set; }
    }
}