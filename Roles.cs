using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RoleBoi;

public class Roles
{
  public static List<ulong> savedRoles = new List<ulong>();

  public static void LoadRoles()
  {
    if (!File.Exists("./roles.json"))
    {
      File.WriteAllText("./roles.json", "[]");
    }

    string jsonString = File.ReadAllText("./roles.json");

    savedRoles = JsonConvert.DeserializeObject<List<ulong>>(jsonString) ?? new List<ulong>();
  }

  public static void SaveRoles()
  {
    File.WriteAllText("./roles.json", JsonConvert.SerializeObject(savedRoles));
  }

  public static void AddRole(ulong roleID)
  {
    savedRoles.Add(roleID);
    SaveRoles();
  }

  public static void RemoveRole(ulong roleID)
  {
    savedRoles.Remove(roleID);
    SaveRoles();
  }
}