using System;
using System.Collections.Generic;

namespace Projekat.Models
{
    public enum Kind
    {
        Text,
        Image,
        Link
    }

    public class Theme
    {
        public string Title { get; set; }
        public Kind Kind { get; set; }

        public string AuthorName { get; set; }

        public List<Comment> Comments { get; set; }

        public string Content { get; set; }
 
        public DateTime CreatedOn { get; set; }

        public uint PositiveVotes { get; set; }
        public uint NegativeVotes { get; set; }
    }

    public class CreationTheme
    {
        public string Title { get; set; }
        public Kind Kind { get; set; }
        public string AuthorName { get; set; }
        public string Content { get; set;}
        public DateTime CreatedOn { get; set; }
    }
}