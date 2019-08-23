﻿using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace LRS.Business
{
    public class JournalObject
    {
        #region Properties

        public int JournalId { get; set; }
        public string JournalName { get; set; }
        public bool Active { get; set; }

        #endregion

        #region Constructor

        public JournalObject() { }

        #endregion

        #region Repository

        private static IJournalRepository repo => new JournalRepository();

        #endregion

        #region Static Methods

        internal static List<JournalObject> GetJournals() => repo.GetJournals();

        internal static JournalObject GetJournal(int journalId) => repo.GetJournal(journalId);

        #endregion

        #region Object Methods

        public void Save()
        {
            repo.SaveJournal(this);
            MemoryCache.ClearJournalSuggestion();
        }

        public void Delete()
        {
            repo.DeleteJournal(this);
            MemoryCache.ClearJournalSuggestion();
        }

        #endregion
    }

    public interface IJournalRepository
    {
        List<JournalObject> GetJournals();
        JournalObject GetJournal(int journalId);
        JournalObject SaveJournal(JournalObject journal);
        bool DeleteJournal(JournalObject journal);
    }

    public class JournalRepository : IJournalRepository
    {
        public List<JournalObject> GetJournals() => Config.Conn.Query<JournalObject>("SELECT * FROM lu_Journal").ToList();

        public JournalObject GetJournal(int journalId) => Config.Conn.Query<JournalObject>("SELECT * FROM lu_Journal WHERE JournalId = @JournalId", new { JournalId = journalId }).FirstOrDefault();

        public JournalObject SaveJournal(JournalObject journal)
        {
            if (journal.JournalId > 0) // Update
            {
                string sql = @"
                    UPDATE  lu_Journal
                    SET     JournalName = @JournalName,
                            Active = @Active
                    WHERE   JournalId = @JournalId";
                Config.Conn.Execute(sql, journal);
            }
            else
            {
                string sql = @"
                    INSERT INTO lu_Journal (
                        JournalName,
                        Active
                    )
                    VALUES (
                        @JournalName,
                        @Active
                    )
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";
                journal.JournalId = Config.Conn.Query<int>(sql, journal).Single();
            }
            return journal;
        }

        public bool DeleteJournal(JournalObject journal)
        {
            try
            {
                Config.Conn.Execute("DELETE FROM lu_Journal WHERE JournalId = @JournalId", journal);
            }
            catch { return false; }
            return true;
        }
    }
}