using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Projekat.Models
{
    public class CommentVote
    {
        public string Subforum { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string Content { get; set; }
        public bool IsPositive { get; set; }
    }

    public class ThemeVote
    {
        public string Subforum { get; set; }
        public string Title { get; set; }
        public bool IsPositive { get; set; }
    }
}