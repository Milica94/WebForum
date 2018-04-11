using Projekat.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Microsoft.Ajax.Utilities;

namespace Projekat.Dao
{
    public class DbDao : IDao
    {
        public DbDao(SQLiteConnection conn)
        {
            _conn = conn;
        }

        // User related
        public bool CanLogIn(string username, string password)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT COUNT()
                      FROM Users
                      WHERE Name = @name AND Password = @password
                      LIMIT 1;";
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@password", password);

                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    return reader.GetInt32(0) == 1;
                }
            }
        }

        public bool Register(User user)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO Users(Name, Password, FirstName, LastName, Role, PhoneNo, Email, RegisteredOn)
                      VALUES (@name, @password, @firstname, @lastname, @role, @phoneno, @email, @registeredon);";
                cmd.Parameters.AddWithValue("@name", user.Name);
                cmd.Parameters.AddWithValue("@password", user.Password);
                cmd.Parameters.AddWithValue("@firstname", user.FirstName);
                cmd.Parameters.AddWithValue("@lastname", user.LastName);
                cmd.Parameters.AddWithValue("@role", (int)user.Role);
                cmd.Parameters.AddWithValue("@phoneno", user.PhoneNo);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@registeredon", user.RegisteredOn.ToString("yyyy-MM-dd"));

                return Execute(cmd);
            }
        }

        public bool AddModerator(string subforum, string newmod)
        {
            if (!IsModerator(newmod))
                return false;

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO Moderators(Moderator, Subforum)
                      VALUES(@name, @subforum);";
                cmd.Parameters.AddWithValue("@name", newmod);
                cmd.Parameters.AddWithValue("@subforum", subforum);

                return Execute(cmd);
            }
        }

        public bool RemoveModerator(string subforum, string mod)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"DELETE FROM Moderators
                      WHERE Moderator = @mod AND Subforum = @subforum;";
                cmd.Parameters.AddWithValue("@mod", mod);
                cmd.Parameters.AddWithValue("@subforum", subforum);

                return Execute(cmd);
            }
        }

        public User GetUser(string username)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT FirstName, LastName, Role, PhoneNo, Email, RegisteredOn
                      FROM Users
                      WHERE Name = @name
                      LIMIT 1;";
                cmd.Parameters.AddWithValue("@name", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new User()
                    {
                        Name = username,
                        FirstName = reader.GetString(0),
                        LastName = reader.GetString(1),
                        Role = (User.ForumRole)reader.GetInt32(2),
                        PhoneNo = reader.GetString(3),
                        Email = reader.GetString(4),
                        RegisteredOn = DateTime.Parse(reader.GetString(5))
                    };
                }
            }
        }

        public IEnumerable<ThemeVote> GetUserThemeVotes(string username)
        {
            var list = new List<ThemeVote>();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Subforum, Title, IsPositive
                      FROM VotedThemes
                      WHERE Name = @name;";
                cmd.Parameters.AddWithValue("@name", username);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ThemeVote()
                        {
                            Subforum = reader.GetString(0),
                            Title = reader.GetString(1),
                            IsPositive = reader.GetInt32(2) == 1
                        });
                    }
                }
            }

            return list;
        }

        public IEnumerable<CommentVote> GetUserCommentVotes(string username)
        {
            var list = new List<CommentVote>();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT c.Subforum, c.Theme, c.Author, c.Content, v.IsPositive
                      FROM VotedComments v
                      INNER JOIN Comments c ON c.ID = v.ID
                      WHERE v.Name = @name;";
                cmd.Parameters.AddWithValue("@name", username);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new CommentVote()
                        {
                            Subforum = reader.GetString(0),
                            Title = reader.GetString(1),
                            AuthorName = reader.GetString(2),
                            Content = reader.GetString(3),
                            IsPositive = reader.GetInt32(4) == 1
                        });
                    }
                }
            }

            return list;
        }

        public bool ChangeRole(string username, User.ForumRole role)
        {
            var success = true;

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"UPDATE Users
                      SET Role = @role
                      WHERE Name = @name;";
                cmd.Parameters.AddWithValue("@role", (int)role);
                cmd.Parameters.AddWithValue("@name", username);

                success &= Execute(cmd);
            }

            if (role != User.ForumRole.Normal)
                return success;

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"DELETE FROM Moderators
                      WHERE Moderator = @name;";
                cmd.Parameters.AddWithValue("@name", username);

                return Execute(cmd);
            }
        }

        public bool CanCreateSubforum(string username)
        {
            return
                IsAdministrator(username) ||
                IsModerator(username);
        }

        public bool CanDeleteSubforum(string username, string subforum)
        {
            return
                IsAdministrator(username) ||
                IsMainModerator(username, subforum);
        }

        public bool CanEditOrDeleteTheme(string username, string subforum, string title)
        {
            return
                IsAdministrator(username) ||
                IsMainModerator(username, subforum) ||
                IsThemeAuthor(username, subforum, title);
        }

        public bool CanEditComment(string username, uint id)
        {
            string subforum;

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Subforum
                      FROM Comments
                      WHERE ID = @id;";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;

                    subforum = reader.GetString(0);
                }
            }

            return
                IsMainModerator(username, subforum) ||
                IsCommentAuthor(username, id);
        }

        public bool CanDeleteComment(string username, uint id)
        {
            string subforum;

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Subforum
                      FROM Comments
                      WHERE ID = @id;";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;

                    subforum = reader.GetString(0);
                }
            }

            return
                IsAdministrator(username) ||
                IsMainModerator(username, subforum) ||
                IsModeratorFor(username, subforum) ||
                IsCommentAuthor(username, id);
        }

        public bool CanAddOrRemoveModerator(string username, string subforum)
        {
            return
                IsAdministrator(username) ||
                IsMainModerator(username, subforum);
        }

        // Message related
        public IEnumerable<FromMessage> GetMessagesFrom(string username)
        {
            var list = new List<FromMessage>();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT ID, ToUser
                      FROM Messages
                      WHERE FromUser = @name
                      ORDER BY ID DESC;";
                cmd.Parameters.AddWithValue("@name", username);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new FromMessage()
                        {
                            Id = (uint)reader.GetInt32(0),
                            ToName = reader.GetString(1)
                        });
                    }
                }
            }

            return list;
        }

        public IEnumerable<ToMessage> GetMessagesTo(string username)
        {
            var list = new List<ToMessage>();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT ID, FromUser
                      FROM Messages
                      WHERE ToUser = @name
                      ORDER BY ID DESC;";
                cmd.Parameters.AddWithValue("@name", username);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ToMessage()
                        {
                            Id = (uint)reader.GetInt32(0),
                            FromName = reader.GetString(1)
                        });
                    }
                }
            }

            return list;
        }

        public Message GetMessage(uint id)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT FromUser, ToUser, Content
                      FROM Messages
                      WHERE ID = @id;";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new Message()
                    {
                        Id = id,
                        FromName = reader.GetString(0),
                        ToName = reader.GetString(1),
                        Content = reader.GetString(2)
                    };
                }
            }
        }

        public bool SendMessage(string from, string to, string content)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO Messages(ID, FromUser, ToUser, Content, WasRead)
                      VALUES(NULL, @fromuser, @touser, @content, 0);";
                cmd.Parameters.AddWithValue("@fromuser", from);
                cmd.Parameters.AddWithValue("@touser", to);
                cmd.Parameters.AddWithValue("@content", content);

                return Execute(cmd);
            }
        }

        // Subforum related
        public IEnumerable<string> GetSubforumNames()
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Name
                      FROM Subforums
                      ORDER BY ROWID ASC;";

                var names = new List<string>();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        names.Add(reader.GetString(0));
                }

                return names;
            }
        }

        public Subforum GetSubforum(string name)
        {
            var subforum = new Subforum();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Description, Rules, IconPath, MainModerator
                      FROM Subforums
                      WHERE Name = @name;";
                cmd.Parameters.AddWithValue("@name", name);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    subforum.Name = name;
                    subforum.Description = reader.GetString(0);
                    subforum.Rules = DecodeRules(reader.GetString(1));
                    subforum.IconPath = reader.GetString(2);
                    subforum.MainModeratorName = reader.GetString(3);
                    subforum.ModeratorNames = new List<string>();
                    subforum.Themes = new List<SubforumTheme>();
                }
            }

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Moderator
                      FROM Moderators
                      WHERE Subforum = @name;";
                cmd.Parameters.AddWithValue("@name", name);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        subforum.ModeratorNames.Add(reader.GetString(0));
                }
            }

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Title
                      FROM Themes
                      WHERE Subforum = @name;";
                cmd.Parameters.AddWithValue("@name", name);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var title = reader.GetString(0);
                        var votes = GetThemeVotes(name, title);

                        subforum.Themes.Add(new SubforumTheme()
                        {
                            Title = title,
                            PositiveVotes = votes.Item1,
                            NegativeVotes = votes.Item2
                        });
                    }
                }
            }

            return subforum;
        }

        public bool CreateSubforum(string username, CreationSubforum subforum)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO Subforums(Name, Description, Rules, IconPath, MainModerator)
                      VALUES (@name, @description, @rules, @iconpath, @moderator);";
                cmd.Parameters.AddWithValue("@name", subforum.Name);
                cmd.Parameters.AddWithValue("@description", subforum.Description);
                cmd.Parameters.AddWithValue("@rules", subforum.Rules);
                cmd.Parameters.AddWithValue("@iconpath", subforum.IconPath);
                cmd.Parameters.AddWithValue("@moderator", username);

                return Execute(cmd);
            }
        }

        public bool DeleteSubforum(string subforum)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"DELETE FROM Subforums
                      WHERE Name = @subforum;";
                cmd.Parameters.AddWithValue("@subforum", subforum);

                return Execute(cmd);
            }
        }

        // Theme related
        public Theme GetTheme(string subforum, string title)
        {
            var theme = new Theme();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Kind, Author, Content, CreatedOn
                      FROM Themes
                      WHERE Subforum = @subforum AND Title = @title;";
                cmd.Parameters.AddWithValue("@subforum", subforum);
                cmd.Parameters.AddWithValue("@title", title);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    theme.Title = title;
                    theme.Kind = (Kind) reader.GetInt32(0);
                    theme.AuthorName = reader.GetString(1);
                    theme.Content = reader.GetString(2);
                    theme.CreatedOn = DateTime.Parse(reader.GetString(3));
                }
            }

            theme.Comments = GetComments(subforum, title);

            var votes = GetThemeVotes(subforum, title);
            theme.PositiveVotes = votes.Item1;
            theme.NegativeVotes = votes.Item2;

            return theme;
        }

        public bool CreateTheme(string subforum, CreationTheme theme)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO Themes(Title, Kind, Author, Content, CreatedOn, Subforum)
                      VALUES (@title, @kind, @author, @content, @createdon, @subforum);";
                cmd.Parameters.AddWithValue("@title", theme.Title);
                cmd.Parameters.AddWithValue("@kind", (int)theme.Kind);
                cmd.Parameters.AddWithValue("@author", theme.AuthorName);
                cmd.Parameters.AddWithValue("@content", theme.Content);
                cmd.Parameters.AddWithValue("@createdOn", theme.CreatedOn.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@subforum", subforum);

                return Execute(cmd);
            }
        }

        public bool EditTheme(string subforum, string title, Kind kind, string content)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"UPDATE Themes
                      SET Content = @content, Kind = @kind
                      WHERE Subforum = @subforum AND Title = @title;";
                cmd.Parameters.AddWithValue("@content", content);
                cmd.Parameters.AddWithValue("@kind", (int) kind);
                cmd.Parameters.AddWithValue("@subforum", subforum);
                cmd.Parameters.AddWithValue("@title", title);

                return Execute(cmd);
            }
        }

        public bool DeleteTheme(string subforum, string title)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"DELETE FROM Themes
                      WHERE Title = @title AND Subforum = @subforum;";
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@subforum", subforum);

                return Execute(cmd);
            }
        }

        public bool VoteTheme(string username, string subforum, string title, bool isPositive)
        {
            var isPrevVotePositive = GetThemeVote(username, subforum, title);

            if (isPrevVotePositive != null)
            {
                if (isPrevVotePositive == isPositive)
                    return RemoveThemeVote(username, subforum, title);
                else
                    return FlipThemeVote(username, subforum, title);
            }

            return AddThemeVote(username, subforum, title, isPositive);
        }

        // Comment related
        public bool AddTopLevelComment(ReplyTopLevel comment)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO Comments(
                          ID,
                          Theme,
                          Subforum,
                          Author,
                          Parent,
                          CreatedOn,
                          Content,
                          WasChanged,
                          WasLogicallyDeleted)
                      VALUES(NULL, @theme, @subforum, @author, NULL, @createdon, @content, 0, 0);";
                cmd.Parameters.AddWithValue("@theme", comment.ThemeTitle);
                cmd.Parameters.AddWithValue("@subforum", comment.SubforumName);
                cmd.Parameters.AddWithValue("@author", comment.AuthorName);
                cmd.Parameters.AddWithValue("@createdon", comment.CreatedOn.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@content", comment.Content);

                return Execute(cmd);
            }
        }

        public bool AddComment(ReplyComment comment)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO Comments(
                          ID,
                          Theme,
                          Subforum,
                          Author,
                          Parent,
                          CreatedOn,
                          Content,
                          WasChanged,
                          WasLogicallyDeleted)
                      VALUES(NULL, @theme, @subforum, @author, @parent, @createdon, @content, 0, 0);";
                cmd.Parameters.AddWithValue("@theme", comment.ThemeTitle);
                cmd.Parameters.AddWithValue("@subforum", comment.SubforumName);
                cmd.Parameters.AddWithValue("@author", comment.AuthorName);
                cmd.Parameters.AddWithValue("@parent", comment.Parent);
                cmd.Parameters.AddWithValue("@createdon", comment.CreatedOn.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@content", comment.Content);

                return Execute(cmd);
            }
        }

        public ReplyVisualizerComment GetComment(uint id)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Theme, Subforum, Author, Content
                      FROM Comments
                      WHERE ID = @id;";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new ReplyVisualizerComment()
                    {
                        Id = id,
                        ThemeTitle = reader.GetString(0),
                        SubforumName = reader.GetString(1),
                        AuthorName = reader.GetString(2),
                        Content = reader.GetString(3)
                    };
                }
            }
        }

        public bool EditComment(string username, uint id, EditComment comment)
        {
            string subforum;

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Subforum
                      FROM Comments
                      WHERE ID = @id;";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;

                    subforum = reader.GetString(0);
                }
            }

            string showChanged;
            if (IsMainModerator(username, subforum))
                showChanged = "";
            else
                showChanged = ", WasChanged = 1";

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    $@"UPDATE Comments
                      SET Content = @content {showChanged}
                      WHERE ID = @id;";
                cmd.Parameters.AddWithValue("@content", comment.Content);
                cmd.Parameters.AddWithValue("@id", comment.Id);

                return Execute(cmd);
            }
        }

        public bool DeleteComment(uint id)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"UPDATE Comments
                      SET WasLogicallyDeleted = 1
                      WHERE ID = @id;";
                cmd.Parameters.AddWithValue("@id", id);

                return Execute(cmd);
            }
        }

        public bool VoteComment(string username, uint id, bool isPositive)
        {
            var isPrevVotePositive = GetCommentVote(username, id);

            if (isPrevVotePositive != null)
            {
                if (isPrevVotePositive == isPositive)
                    return RemoveCommentVote(username, id);
                else
                    return FlipCommentVote(username, id);
            }

            return AddCommentVote(username, id, isPositive);
        }

        // Search related
        public IEnumerable<SubforumSearchResult> SearchSubforums(SearchSubforums search)
        {
            var list = new List<SubforumSearchResult>();

            var nameCond = search.Name.IsNullOrWhiteSpace() ? "1 = 1" : "Name = @name";
            var descCond = search.Description.IsNullOrWhiteSpace() ? "1 = 1" : "Description = @desc";
            var modCond = search.MainModerator.IsNullOrWhiteSpace() ? "1 = 1" : "MainModerator = @mod";

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    $@"SELECT Name, Description, MainModerator
                      FROM Subforums
                      WHERE {nameCond} AND {descCond} AND {modCond};";
                if (!search.Name.IsNullOrWhiteSpace())
                    cmd.Parameters.AddWithValue("@name", search.Name);
                if (!search.Description.IsNullOrWhiteSpace())
                    cmd.Parameters.AddWithValue("@desc", search.Description);
                if (!search.MainModerator.IsNullOrWhiteSpace())
                    cmd.Parameters.AddWithValue("@mod", search.MainModerator);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SubforumSearchResult()
                        {
                            Name = reader.GetString(0),
                            Description = reader.GetString(1),
                            MainModerator = reader.GetString(2)
                        });
                    }
                }
            }

            return list;
        }

        public IEnumerable<ThemeSearchResult> SearchThemes(SearchThemes search)
        {
            var list = new List<ThemeSearchResult>();

            var titleCond = search.Title.IsNullOrWhiteSpace() ? "1 = 1" : "Title = @title";
            var contentCond = search.Content.IsNullOrWhiteSpace() ? "1 = 1" : "Content = @content";
            var authorCond = search.AuthorName.IsNullOrWhiteSpace() ? "1 = 1" : "Author = @name";
            var subforumCond = search.SubforumName.IsNullOrWhiteSpace() ? "1 = 1" : "Subforum = @subforum";

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    $@"SELECT Title, Content, Author, Subforum
                       FROM Themes
                       WHERE {titleCond} AND {contentCond} AND {authorCond} AND {subforumCond};";
                if (!search.Title.IsNullOrWhiteSpace())
                    cmd.Parameters.AddWithValue("@title", search.Title);
                if (!search.Content.IsNullOrWhiteSpace())
                    cmd.Parameters.AddWithValue("@content", search.Content);
                if (!search.AuthorName.IsNullOrWhiteSpace())
                    cmd.Parameters.AddWithValue("@name", search.AuthorName);
                if (!search.SubforumName.IsNullOrWhiteSpace())
                    cmd.Parameters.AddWithValue("@subforum", search.SubforumName);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ThemeSearchResult()
                        {
                            Title = reader.GetString(0),
                            Content = reader.GetString(1),
                            AuthorName = reader.GetString(2),
                            SubforumName = reader.GetString(3)
                        });
                    }
                }
            }

            return list;
        }

        public IEnumerable<UserSearchResult> SearchUsers(SearchUsers search)
        {
            var list = new List<UserSearchResult>();

            var nameCond = search.Name.IsNullOrWhiteSpace() ? "1 = 1" : "Name = @name";

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    $@"SELECT Name, Role, RegisteredOn
                      FROM Users
                      WHERE {nameCond};";
                if (!search.Name.IsNullOrWhiteSpace())
                    cmd.Parameters.AddWithValue("@name", search.Name);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new UserSearchResult()
                        {
                            Name = reader.GetString(0),
                            Role = (User.ForumRole)reader.GetInt32(1),
                            RegisteredOn = DateTime.Parse(reader.GetString(2))
                        });
                    }
                }
            }

            return list;
        }


        // Helper checkers
        private bool IsAdministrator(string username)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Role = @admin
                      FROM Users
                      WHERE Name = @name;";
                cmd.Parameters.AddWithValue("@admin", (int)User.ForumRole.Administrator);
                cmd.Parameters.AddWithValue("@name", username);

                return IsTrue(cmd);
            }
        }

        private bool IsMainModerator(string username, string subforum)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT MainModerator = @name
                      FROM Subforums
                      WHERE Name = @subforum;";
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@subforum", subforum);

                return IsTrue(cmd);
            }
        }

        private bool IsModerator(string username)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Role = @moderator
                      FROM Users
                      WHERE Name = @name";
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@moderator", (int) User.ForumRole.Moderator);

                return IsTrue(cmd);
            }
        }

        private bool IsModeratorFor(string username, string subforum)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT COUNT()
                      FROM Moderators
                      WHERE Moderator = @name AND Subforum = @subforum;";
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@subforum", subforum);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;

                    return reader.GetInt32(0) > 0;
                }
            }
        }

        private bool IsThemeAuthor(string username, string subforum, string title)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Author = @name
                      FROM Themes
                      WHERE Subforum = @subforum AND Title = @title;";
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@subforum", subforum);
                cmd.Parameters.AddWithValue("@title", title);

                return IsTrue(cmd);
            }
        }

        private bool IsCommentAuthor(string username, uint id)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT Author = @name
                      FROM Comments
                      WHERE ID = @id";
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@id", id);

                return IsTrue(cmd);
            }
        }

        // Comment vote helpers
        private bool? GetCommentVote(string username, uint id)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT IsPositive
                      FROM VotedComments
                      WHERE ID = @id AND Name = @name;";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return reader.GetInt32(0) == 1;
                }
            }
        }

        private bool AddCommentVote(string username, uint id, bool isPositive)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO VotedComments(ID, Name, IsPositive)
                      VALUES(@id, @name, @ispositive);";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@ispositive", isPositive);

                return Execute(cmd);
            }
        }

        private bool FlipCommentVote(string username, uint id)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"UPDATE VotedComments
                      SET IsPositive = NOT(IsPositive)
                      WHERE ID = @id AND Name = @name;";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", username);

                return Execute(cmd);
            }
        }

        private bool RemoveCommentVote(string username, uint id)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"DELETE FROM VotedComments
                      WHERE ID = @id AND Name = @name;";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", username);

                return Execute(cmd);
            }
        }

        // Theme vote helpers
        private bool? GetThemeVote(string username, string subforum, string title)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT IsPositive
                      FROM VotedThemes
                      WHERE Subforum = @subforum AND Title = @title AND Name = @name;";
                cmd.Parameters.AddWithValue("@subforum", subforum);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@name", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return reader.GetInt32(0) == 1;
                }
            }
        }

        private bool AddThemeVote(string username, string subforum, string title, bool isPositive)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"INSERT INTO VotedThemes(Subforum, Title, Name, IsPositive)
                      VALUES(@subforum, @title, @name, @ispositive);";
                cmd.Parameters.AddWithValue("@subforum", subforum);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@name", username);
                cmd.Parameters.AddWithValue("@ispositive", isPositive);

                return Execute(cmd);
            }
        }

        private bool FlipThemeVote(string username, string subforum, string title)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"UPDATE VotedThemes
                      SET IsPositive = NOT(IsPositive)
                      WHERE Subforum = @subforum AND Title = @title AND Name = @name;";
                cmd.Parameters.AddWithValue("@subforum", subforum);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@name", username);

                return Execute(cmd);
            }
        }

        private bool RemoveThemeVote(string username, string subforum, string title)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"DELETE FROM VotedThemes
                      WHERE Subforum = @subforum AND Title = @title AND Name = @name;";
                cmd.Parameters.AddWithValue("@subforum", subforum);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@name", username);

                return Execute(cmd);
            }
        }


        private static List<string> DecodeRules(string rules)
        {
           return new List<string>(rules.Split('\n'));
        }

        private List<Comment> GetComments(string subforum, string title)
        {
            var list = new List<Comment>();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    $@"SELECT ID, Author, CreatedOn, Content, WasChanged, WasLogicallyDeleted
                      FROM Comments
                      WHERE Subforum = @subforum AND Theme = @title AND Parent IS NULL;";
                cmd.Parameters.AddWithValue("@subforum", subforum);
                cmd.Parameters.AddWithValue("@title", title);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Comment()
                        {
                            Id = (uint)reader.GetInt32(0),
                            AuthorName = reader.GetString(1),
                            CreatedOn = DateTime.Parse(reader.GetString(2)),
                            Content = reader.GetString(3),
                            Edited = reader.GetInt32(4) == 1,
                            LogicallyDeleted = reader.GetInt32(5) == 1,
                        });
                    }
                }
            }

            foreach (var comment in list)
            {
                var votes = GetCommentVotes(comment.Id);
                comment.PositiveVotes = votes.Item1;
                comment.NegativeVotes = votes.Item2;
                comment.Children = GetChildrenComments(comment.Id);
            }

            list.Sort((l, r) => r.Score - l.Score);

            return list;
        }

        private List<Comment> GetChildrenComments(uint parent)
        {
            var list = new List<Comment>();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT ID, Author, CreatedOn, Content, WasChanged, WasLogicallyDeleted
                      FROM Comments
                      WHERE Parent = @parent;";
                cmd.Parameters.AddWithValue("@parent", parent);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Comment()
                        {
                            Id = (uint)reader.GetInt32(0),
                            AuthorName = reader.GetString(1),
                            CreatedOn = DateTime.Parse(reader.GetString(2)),
                            Content = reader.GetString(3),
                            Edited = reader.GetInt32(4) == 1,
                            LogicallyDeleted = reader.GetInt32(5) == 1,
                        });
                    }
                }
            }

            foreach (var comment in list)
            {
                var votes = GetCommentVotes(comment.Id);
                comment.PositiveVotes = votes.Item1;
                comment.NegativeVotes = votes.Item2;
                comment.Children = GetChildrenComments(comment.Id);
            }

            list.Sort((l, r) => (int)(l.PositiveVotes - l.NegativeVotes - r.PositiveVotes + r.NegativeVotes));

            return list;
        }

        private Tuple<uint, uint> GetCommentVotes(uint id)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT COUNT(CASE IsPositive WHEN 1 THEN 1 ELSE NULL END), COUNT(CASE IsPositive WHEN 0 THEN 1 ELSE NULL END)
                      FROM VotedComments
                      WHERE ID = @id;";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new Tuple<uint, uint>(
                        (uint)reader.GetInt32(0),
                        (uint)reader.GetInt32(1));
                }
            }
        }

        private Tuple<uint, uint> GetThemeVotes(string subforum, string title)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    @"SELECT COUNT(CASE IsPositive WHEN 1 THEN 1 ELSE NULL END), COUNT(CASE IsPositive WHEN 0 THEN 1 ELSE NULL END)
                      FROM VotedThemes
                      WHERE Subforum = @subforum AND Title = @title;";
                cmd.Parameters.AddWithValue("@subforum", subforum);
                cmd.Parameters.AddWithValue("@title", title);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new Tuple<uint, uint>(
                        (uint)reader.GetInt32(0),
                        (uint)reader.GetInt32(1));
                }
            }
        }

        // Low-level helpers
        private static bool Execute(SQLiteCommand cmd)
        {
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException)
            {
                return false;
            }

            return true;
        }

        private static bool IsTrue(SQLiteCommand cmd)
        {
            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                    return false;

                return reader.GetInt32(0) == 1;
            }
        }


        private readonly SQLiteConnection _conn;
    }
}