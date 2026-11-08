## Work Queue


1. [x] Genome Data I
	+ [x] Create [[data/Genome.cs]]
	1. [x] Create properties (*float*) `Speed`, (*float*) `Size` and (*float*) `DirectionChangePeriod`
	2. [x] Set them default values
	3. [x] Create `Mutate` method
	4. [x] Create `Crossover` method


2. [ ] Agent Phenotype I
	+ [x] Create [[scenes/agent/Agent.cs]]
	1. [ ] Create event `FinishReached()`, fields (*Area2D*) `_finishCheckArea`, (*Vector2*) `_currentDirection`, (*float*) `_moveTime` and (*float*) `_liveTime`
	2. [ ] Set them default values
	3. [ ] Create `ChangeDirection` method
	4. [ ] Create `Move` method, set speed to `Genome.Speed` and call it into `_PhysicsProcess`
	5. [ ] Implement timer (in `_PhysicsProcess` with `delta`), set wait time to `Genome.DirectionChangePeriod`, on timeout call `ChangeDirection`
	6. [ ] Invoke `FinishReached` on `_finishCheckArea.AreaEntered`
	7. [ ] On `FinishReached` freeze the agent


3. [ ] Evolution Manager I
	+ [x] Create [[managers/EvolutionManager.cs]]
	1. [ ] Create fields (*int*) `_populationSize`, (*float*) `_mutationRate`, (*float*) `_epochTime`, (*float*) `_currentEpochTimeLeft`, (*Agent[]*) `_currentAgents`
	2. [ ] Set them default values
	3. [ ] Create `Spawn` method (don't forget to add agent to `_currentAgents`, connect to it's event `Agent.FinishReached`)
	4. [ ] Create `SpawnPopulation` method
	5. [ ] Create `ClearPopulation` method
	6. [ ] Create `RestartEpoch` *placeholder* method
	7. [ ] Implement timer (in `_PhysicsProcess` with `delta`), set wait time to `_epochTime`, on timeout call `RestartEpoch`
	8. [ ] Create `GetBestMembers` method
	9. [ ] Create `EvoluteGenome` method
	10. [ ] Create *full* `RestartEpoch` method

---
