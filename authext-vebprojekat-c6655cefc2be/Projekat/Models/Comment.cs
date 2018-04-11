using System;
using System.Collections.Generic;

namespace Projekat.Models
{
    public class Comment
    {
        public uint Id { get; set; }

        public string AuthorName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Content { get; set; }

        public List<Comment> Children { get; set; }

        public uint PositiveVotes { get; set; }
        public uint NegativeVotes { get; set; }
        public int Score => (int)PositiveVotes - (int)NegativeVotes;

        public bool Edited { get; set; }
        public bool LogicallyDeleted { get; set; }
    }

    public class ReplyVisualizerComment
    {
        public uint Id { get; set; }
        public string AuthorName { get; set; }
        public string Content { get; set; }

        public string SubforumName { get; set; }
        public string ThemeTitle { get; set; }
    }

    public class ReplyComment
    {
        public uint Parent { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Content { get; set; }

        public string SubforumName { get; set; }
        public string ThemeTitle { get; set; }
    }

    public class ReplyTopLevel
    {
        public string AuthorName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Content { get; set; }

        public string SubforumName { get; set; }
        public string ThemeTitle { get; set; }
    }

    public class EditComment
    {
        public uint Id { get; set; }
        public string Content { get; set; }
    }
}