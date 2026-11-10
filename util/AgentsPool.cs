using System;
using System.Collections.Generic;
using Godot;
using Simulation.Scenes;

namespace Simulation.Managers;


public class AgentsPool
{
    public event Action AllAgentsStopped;

    private static readonly PackedScene _agentScene = GD.Load<PackedScene>(@"res://scenes/agent/agent.tscn");
    private Agent[] _agents;
    private int _processingAgents = 0;


    public AgentsPool(int agentsAmount, Node parent)
    {
        _agents = new Agent[agentsAmount];
        InstantiateAgents(parent);
    }

    public IEnumerable<Agent> GetAgents() => _agents;
    public void Respawn()
    {
        _processingAgents = _agents.Length;
        for (var i = 0; i < _agents.Length; i++)
        {
            _agents[i].Reset();
        }
    }

    private void InstantiateAgents(Node parent)
    {
        for (var i = 0; i < _agents.Length; i++)
        {
            _agents[i] = _agentScene.Instantiate<Agent>();
            parent.AddChild(_agents[i]);
            _agents[i].Stopped += OnStopped;
        }
    }

    private void OnStopped()
    {
        _processingAgents--;
        
        LogUI.Send(LogUI.GetInstance().Agents, $"Agents: {_processingAgents}");

        if(_processingAgents == 0)
        {
            GD.Print(_processingAgents);
            AllAgentsStopped?.Invoke();
        }
    }

}
