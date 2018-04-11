using System;

namespace Projekat.Models
{
    public class SearchSubforums
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string MainModerator { get; set; } 
    }

    public class SubforumSearchResult
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string MainModerator { get; set; }
    }

    public class SearchThemes
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public string SubforumName { get; set; }
    }

    public class ThemeSearchResult
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public string SubforumName { get; set; }
    }

    public class SearchUsers
    {
        public string Name { get; set; }
    }

    public class UserSearchResult
    {
        public string Name { get; set; }
        public User.ForumRole Role { get; set; }
        public DateTime RegisteredOn { get; set; }
    }
}