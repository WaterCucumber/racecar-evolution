using System.Threading.Tasks;
using Godot;
using Simulation.Components;
using Simulation.Scenes;

namespace Simulation.Util;

/// <summary>
/// Use for easy track-setup with <c>ChangeDifficulty()</c>
/// </summary>
public class TrainingTrackSetuper

{
    private const float DefaultMinRadius = 400;
    private const float MinimumMinRadius = 200;
    private const float DefaultMaxRadius = 500;
    private const float MaximumMaxRadius = 700;
    private const float DefaultNumPoints = 6;
    private const float MaximumNumPoints = 22;
    private const float RadiusRase = 4f;
    private const float NumPointsRase = 1 / 5f;
    
    private readonly TrackCreator _trackCreator;
    private float _minR = DefaultMinRadius, _maxR = DefaultMaxRadius, _numPoints = DefaultNumPoints;

    public TrainingTrackSetuper(Node2D parent)
    {
        _trackCreator = new(parent);
        LogUI.Send(LogUI.GetInstance().Difficulty, $"Difficulty: {GetDifficulty(),4:F2}");
    }


    public float GetDifficulty() => _maxR / _minR * ((int)_numPoints) / (DefaultMaxRadius / DefaultMinRadius * DefaultNumPoints);
    public void SetDifficulty(float v)
    {
        if (v < 1) return;

        float a = 4 / 5f * DefaultMinRadius;
        float b = DefaultNumPoints * (DefaultMinRadius * RadiusRase + RadiusRase * v * DefaultMaxRadius) + DefaultMinRadius * DefaultMaxRadius * NumPointsRase;
        float c = DefaultMaxRadius * DefaultNumPoints * DefaultMinRadius * (1 - v);

        float d = Mathf.Sqrt(b * b - 4 * a * c);
        float i = (-b + d) / a / 2;

        _minR = DefaultMinRadius - i * RadiusRase;
        _maxR = DefaultMaxRadius + i * RadiusRase;
        _numPoints = DefaultNumPoints + i * NumPointsRase;
        ClampVariables();
        LogUI.Send(LogUI.GetInstance().Difficulty, $"Difficulty: {GetDifficulty(),4:F2}");
    }

    public async Task<Checkpoints> CreateTrack() => await _trackCreator.CreateTrackAsync((int)_numPoints, _minR, _maxR);
    public void ChangeDifficulty(bool increase)
    {
        int f = increase ? 1 : -1;
        _minR -= RadiusRase * f;
        _maxR += RadiusRase * f;
        _numPoints += NumPointsRase * f;
        ClampVariables();
        LogUI.Send(LogUI.GetInstance().Difficulty, $"Difficulty: {GetDifficulty(),4:F2}");
    }

    private void ClampVariables()
    {
        _maxR = Mathf.Clamp(_maxR, DefaultMaxRadius, MaximumMaxRadius);
        _minR = Mathf.Clamp(_minR, MinimumMinRadius, DefaultMinRadius);
        _numPoints = Mathf.Clamp(_numPoints, DefaultNumPoints, MaximumNumPoints);
    }
}
