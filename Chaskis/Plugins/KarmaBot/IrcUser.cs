﻿//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using SQLite.Net.Attributes;

namespace Chaskis.Plugins.KarmaBot
{
    /// <summary>
    /// And IRC user that had a karma count.
    /// </summary>
    public class IrcUser
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// Sets everything to default values.
        /// </summary>
        public IrcUser()
        {
            this.Id = null;
            this.KarmaCount = 0;
            this.UserName = string.Empty;
        }

        // -------- Properties --------

        /// <summary>
        /// The unique ID for SQLite.
        /// This MUST be null for InsertOrReplace to work,
        /// otherwise we'll just consistently insert at row 0.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        /// <summary>
        /// The number of karma the user has.
        /// </summary>
        [Indexed]
        public int KarmaCount { get; set; }

        /// <summary>
        /// The name of the user.
        /// </summary>
        public string UserName { get; set; }
    }
}