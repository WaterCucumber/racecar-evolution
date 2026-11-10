using Godot;
using Simulation.Data;
using System.Text.Json;
using static System.IO.Path;

namespace Simulation.Util;


public static class SaveManager
{
    private const string SavePath = "user://";
    private readonly static JsonSerializerOptions _options = new() { WriteIndented = true };


    public static void SaveBrain(NeuralNetwork brain, string saveFile)
    {
        NeuralNetworkData data = brain.ExportData();

        string jsonString = JsonSerializer.Serialize(data, _options);

        using var file = FileAccess.Open(Combine(SavePath, saveFile), FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(jsonString);
            GD.Print($"Brain successful exported to: {ProjectSettings.GlobalizePath(Combine(SavePath, saveFile))}");
        }
        else
        {
            GD.PrintErr("Can not open file to write JSON!");
        }
    }

    public static bool LoadBrain(NeuralNetwork brain, string loadFile)
    {
        if (!FileAccess.FileExists(Combine(SavePath, loadFile)))
        {
            GD.Print("File of the save is not found. Neural network begins with random weights.");
            return false;
        }

        using var file = FileAccess.Open(Combine(SavePath, loadFile), FileAccess.ModeFlags.Read);
        if (file == null) return false;

        string jsonString = file.GetAsText();

        try
        {
            NeuralNetworkData data = JsonSerializer.Deserialize<NeuralNetworkData>(jsonString);
            
            if (data != null)
            {
                brain.ImportData(data, loadFile);
                return true;
            }
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"Error while reading or parsing JSON: {e.Message}");
        }

        return false;
    }
}
