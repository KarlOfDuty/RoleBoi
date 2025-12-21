using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace RoleBoi
{
  internal static class Database
  {
    public class SavedRole
    {
      public ulong userID;
      public ulong roleID;
      public DateTime time;

      public SavedRole(SqliteDataReader reader)
      {
        userID = (ulong)reader.GetInt64(reader.GetOrdinal("user_id"));
        roleID = (ulong)reader.GetInt64(reader.GetOrdinal("role_id"));
        time = DateTime.Parse(reader.GetString(reader.GetOrdinal("time")));
      }
    }

    public static SqliteConnection GetConnection()
    {
      return new SqliteConnection("Data Source=" + Config.databaseFile + ";Cache=Shared");
    }

    public static void SetupTables()
    {
      using SqliteConnection c = GetConnection();

      c.Open();
      using SqliteCommand createTable = new SqliteCommand(
                "CREATE TABLE IF NOT EXISTS tracked_roles(" +
                "user_id INTEGER NOT NULL," +
                "role_id INTEGER NOT NULL," +
                "time TEXT NOT NULL);",
                c);
      createTable.ExecuteNonQuery();
    }

    public static bool TryAddRole(ulong userID, ulong roleID)
    {
      using SqliteConnection c = GetConnection();
      c.Open();

      using SqliteCommand cmd = new SqliteCommand(@"INSERT INTO tracked_roles (user_id, role_id, time) VALUES (@user_id, @role_id, CURRENT_TIMESTAMP);", c);
      cmd.Parameters.AddWithValue("@user_id", (long)userID);
      cmd.Parameters.AddWithValue("@role_id", (long)roleID);

      return cmd.ExecuteNonQuery() > 0;
    }

    public static bool TryGetRoles(ulong userID, out List<SavedRole> roles)
    {
      roles = null;
      using SqliteConnection c = GetConnection();
      c.Open();

      using SqliteCommand selection = new SqliteCommand(@"SELECT user_id, role_id, time FROM tracked_roles WHERE user_id=@user_id", c);
      selection.Parameters.AddWithValue("@user_id", (long)userID);
      SqliteDataReader results = selection.ExecuteReader();

      if (!results.Read())
      {
        return false;
      }

      roles = new List<SavedRole> { new SavedRole(results) };
      while (results.Read())
      {
        roles.Add(new SavedRole(results));
      }
      results.Close();
      return true;
    }

    public static bool TryRemoveRoles(ulong userID)
    {
      using (SqliteConnection c = GetConnection())
      {
        c.Open();

        using SqliteCommand deletion = new SqliteCommand(@"DELETE FROM tracked_roles WHERE user_id=@user_id", c);
        deletion.Parameters.AddWithValue("@user_id", (long)userID);

        return deletion.ExecuteNonQuery() > 0;
      }
    }
  }
}
