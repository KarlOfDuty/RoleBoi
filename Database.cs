using System;
using System.Collections.Generic;
using System.IO;
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
      return new SqliteConnection("Data Source=" + Config.DatabaseFile + ";Cache=Shared");
    }

    private static int ExecuteNonQuery(string sql, Dictionary<string, long> longVars = null)
    {
      using SqliteConnection c = GetConnection();
      c.Open();

      using SqliteCommand cmd = new(sql, c);
      if (longVars != null)
      {
        foreach (KeyValuePair<string, long> longVar in longVars)
        {
          cmd.Parameters.AddWithValue(longVar.Key, longVar.Value);
        }
      }

      return cmd.ExecuteNonQuery();
    }

    public static void SetupTables()
    {
      Logger.Log("Initializing database: " + Path.GetFullPath(Config.DatabaseFile));
      ExecuteNonQuery("CREATE TABLE IF NOT EXISTS user_roles (user_id INTEGER NOT NULL, role_id INTEGER NOT NULL, time TEXT NOT NULL);");
      ExecuteNonQuery("CREATE TABLE IF NOT EXISTS config_tracked_roles    (role_id INTEGER PRIMARY KEY)");
      ExecuteNonQuery("CREATE TABLE IF NOT EXISTS config_pingable_roles   (role_id INTEGER PRIMARY KEY)");
      ExecuteNonQuery("CREATE TABLE IF NOT EXISTS config_selectable_roles (role_id INTEGER PRIMARY KEY)");
      ExecuteNonQuery("CREATE TABLE IF NOT EXISTS config_join_roles       (role_id INTEGER PRIMARY KEY)");
    }

    public static bool TryAddUserRole(ulong userID, ulong roleID)
    {
      int result = ExecuteNonQuery("INSERT INTO user_roles (user_id, role_id, time) VALUES (@user_id, @role_id, CURRENT_TIMESTAMP);",
                                   new() { { "@user_id", (long)userID }, { "@role_id", (long)roleID } });
      return result > 0;
    }

    public static bool TryGetUserRoles(ulong userID, out List<SavedRole> roles)
    {
      roles = null;
      using SqliteConnection c = GetConnection();
      c.Open();

      using SqliteCommand selection = new SqliteCommand(@"SELECT user_id, role_id, time FROM user_roles WHERE user_id=@user_id;", c);
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

    public static bool TryRemoveUserRoles(ulong userID)
    {
      int result = ExecuteNonQuery("DELETE FROM user_roles WHERE user_id=@user_id;",
                                   new() { { "@user_id", (long)userID } });
      return result > 0;
    }

    private static bool TryAddConfigRole(string table, ulong roleID)
    {
      int result = ExecuteNonQuery($"INSERT INTO {table} (role_id) VALUES (@role_id);",
                                   new() { { "@role_id", (long)roleID } });
      return result > 0;
    }

    private static bool TryRemoveConfigRole(string table, ulong roleID)
    {
      int result = ExecuteNonQuery($"DELETE FROM {table} WHERE role_id=@role_id;",
                                   new() { { "@role_id", (long)roleID } });
      return result > 0;
    }

    private static List<ulong> GetConfigRoles(string table)
    {
      using SqliteConnection c = GetConnection();
      c.Open();

      using SqliteCommand selection = new SqliteCommand($"SELECT role_id FROM {table};", c);
      using SqliteDataReader reader = selection.ExecuteReader();

      List<ulong> roles = new List<ulong>();
      while (reader.Read())
      {
        roles.Add((ulong)reader.GetInt64(reader.GetOrdinal("role_id")));
      }
      return roles;
    }

    public static bool TryAddTrackedRole(ulong roleID) => TryAddConfigRole("config_tracked_roles", roleID);
    public static bool TryRemoveTrackedRole(ulong roleID) => TryRemoveConfigRole("config_tracked_roles", roleID);
    public static List<ulong> GetTrackedRoles() => GetConfigRoles("config_tracked_roles");

    public static bool TryAddPingableRole(ulong roleID) => TryAddConfigRole("config_pingable_roles", roleID);
    public static bool TryRemovePingableRole(ulong roleID) => TryRemoveConfigRole("config_pingable_roles", roleID);
    public static List<ulong> GetPingableRoles() => GetConfigRoles("config_pingable_roles");

    public static bool TryAddSelectableRole(ulong roleID) => TryAddConfigRole("config_selectable_roles", roleID);
    public static bool TryRemoveSelectableRole(ulong roleID) => TryRemoveConfigRole("config_selectable_roles", roleID);
    public static List<ulong> GetSelectableRoles() => GetConfigRoles("config_selectable_roles");

    public static bool TryAddJoinRole(ulong roleID) => TryAddConfigRole("config_join_roles", roleID);
    public static bool TryRemoveJoinRole(ulong roleID) => TryRemoveConfigRole("config_join_roles", roleID);
    public static List<ulong> GetJoinRoles() => GetConfigRoles("config_join_roles");
  }
}
