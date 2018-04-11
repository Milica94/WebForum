using System;
using Projekat.Models;
using System.Collections.Generic;

namespace Projekat.Dao
{
    public interface IDao
    {
        // User related
        bool CanLogIn(string username, string password);
        bool Register(User user);
        bool AddModerator(string subforum, string newmod);
        bool RemoveModerator(string subforum, string mod);
        User GetUser(string username);
        IEnumerable<ThemeVote> GetUserThemeVotes(string username);
        IEnumerable<CommentVote> GetUserCommentVotes(string username);
        bool ChangeRole(string username, User.ForumRole role);
        bool CanCreateSubforum(string username);
        bool CanDeleteSubforum(string username, string subforum);
        bool CanEditOrDeleteTheme(string username, string subforum, string title);
        bool CanEditComment(string username, uint id);
        bool CanDeleteComment(string username, uint id);
        bool CanAddOrRemoveModerator(string username, string subforum);

        // Message related
        IEnumerable<FromMessage> GetMessagesFrom(string username);
        IEnumerable<ToMessage> GetMessagesTo(string username);
        Message GetMessage(uint id);
        bool SendMessage(string from, string to, string content);

        // Subforum related
        IEnumerable<string> GetSubforumNames();
        Subforum GetSubforum(string name);
        bool CreateSubforum(string username, CreationSubforum subforum);
        bool DeleteSubforum(string subforum);

        // Theme related
        Theme GetTheme(string subforum, string title);
        bool CreateTheme(string subforum, CreationTheme theme);
        bool EditTheme(string subforum, string title, Kind kind, string content);
        bool DeleteTheme(string subforum, string title);
        bool VoteTheme(string username, string subforum, string title, bool isPositive);

        // Comment related
        bool AddTopLevelComment(ReplyTopLevel comment);
        bool AddComment(ReplyComment comment);
        ReplyVisualizerComment GetComment(uint id);
        bool EditComment(string username, uint id, EditComment comment);
        bool DeleteComment(uint id);
        bool VoteComment(string username, uint id, bool isPositive);

        // Search related
        IEnumerable<SubforumSearchResult> SearchSubforums(SearchSubforums search);
        IEnumerable<ThemeSearchResult> SearchThemes(SearchThemes search);
        IEnumerable<UserSearchResult> SearchUsers(SearchUsers search);
    }
}