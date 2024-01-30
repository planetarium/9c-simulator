namespace NineSimulator.Simulator;

using Libplanet.Crypto;
using Nekoyume.Arena;
using Nekoyume.Model;
using Nekoyume.Model.BattleStatus.Arena;
using Nekoyume.TableData;
using NineSimulator.Model;

using SimulatorRandom = Random;
using SystemRandom = System.Random;

public class BulkSimulator
{
    public StateClient Client { get; private set; } = new StateClient();
    private Store store = new Store("Data Source=./acc.db");

    public async Task Execute(Arena avatar1, Arena avatar2)
    {
        var avatar1Address = new Address(avatar1.AvatarAddress);
        var avatar2Address = new Address(avatar2.AvatarAddress);

        var avatar1AvatarState = await Client.GetAvatarState(avatar1Address);
        var avatar1AvatarItemSlotState = await Client.GetItemSlotState(avatar1Address);
        var avatar1AvatarRuneStates = await Client.GetRuneStates(avatar1Address);

        var avatar2AvatarState = await Client.GetAvatarState(avatar2Address);
        var avatar2ItemSlotState = await Client.GetItemSlotState(avatar2Address);
        var avatar2RuneStates = await Client.GetRuneStates(avatar2Address);

        var seed = new SystemRandom().Next();
        var random = new SimulatorRandom(seed);
        var simulator = new ArenaSimulator(random, 5);

        var arenaLog = simulator.Simulate(
            new ArenaPlayerDigest(avatar1AvatarState, avatar1AvatarItemSlotState.Equipments, avatar1AvatarItemSlotState.Costumes, avatar1AvatarRuneStates),
            new ArenaPlayerDigest(avatar2AvatarState, avatar2ItemSlotState.Equipments, avatar2ItemSlotState.Costumes, avatar2RuneStates),
            new ArenaSimulatorSheets(
                await Client.GetSheet<MaterialItemSheet>(),
                await Client.GetSheet<SkillSheet>(),
                await Client.GetSheet<SkillBuffSheet>(),
                await Client.GetSheet<StatBuffSheet>(),
                await Client.GetSheet<SkillActionBuffSheet>(),
                await Client.GetSheet<ActionBuffSheet>(),
                await Client.GetSheet<CharacterSheet>(),
                await Client.GetSheet<CharacterLevelSheet>(),
                await Client.GetSheet<EquipmentItemSetEffectSheet>(),
                await Client.GetSheet<CostumeStatSheet>(),
                await Client.GetSheet<WeeklyArenaRewardSheet>(),
                await Client.GetSheet<RuneOptionSheet>()
            ),
            true);
        
        Console.WriteLine($"{arenaLog.Result}, {avatar1.AvatarAddress}, {avatar1.AvatarName}, {avatar1.Cp}, {avatar2.AvatarAddress}, {avatar2.AvatarName}, {avatar2.Cp}");

        store.AddArenaResult(arenaLog.Result == ArenaLog.ArenaResult.Win, avatar1.AvatarAddress, avatar1.AvatarName, avatar1.Cp, avatar2.AvatarAddress, avatar2.AvatarName, avatar2.Cp);
    }
}