namespace NineSimulator;

using NineSimulator.Model;
using System.Text.Json;

public class ArenaDataReader
{

    public List<Arena> LoadArenaData()
    {
        using (StreamReader r = new StreamReader("arena_heimdall.json"))
        {
            string json = r.ReadToEnd();
            List<Arena> result = JsonSerializer.Deserialize<List<Arena>>(json);


            return result;
        }
    }
}
