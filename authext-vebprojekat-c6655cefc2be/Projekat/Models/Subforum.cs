using System.Collections.Generic;

namespace Projekat.Models
{
    public class Subforum
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }

        public List<string> Rules { get; set; }

        public List<SubforumTheme> Themes { get; set; }

        public string MainModeratorName { get; set; }
        public List<string> ModeratorNames { get; set; }
    }

    public class SubforumTheme
    {
        public string Title { get; set; }
        public uint PositiveVotes { get; set; }
        public uint NegativeVotes { get; set; }
        public int Score => (int) PositiveVotes - (int) NegativeVotes;
    }

    public class CreationSubforum
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public string Rules { get; set; }
    }
}