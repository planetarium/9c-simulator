namespace NineSimulator.Simulator;

using System.Text;
using System.Net.Http.Json;
using System.Text.Json;
using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using System.Collections.Concurrent;

public class StateClient
{
    private readonly Uri _headlessEndpoint = new Uri("https://heimdall-full-state.nine-chronicles.com/graphql");

    private readonly HttpClient _httpClient = new();

    private static readonly Codec Codec = new();

    private readonly ConcurrentDictionary<Address, IValue?> _cache = new();

    public async Task<IValue?> GetState(Address address)
    {
        if (_cache.TryGetValue(address, out IValue? cachedValue))
        {
            return cachedValue;
        }

        using StringContent jsonContent = new
        (JsonSerializer.Serialize(new
        {
            query = @"query GetState($address: Address!) { state(address: $address) }",
            variables = new {
                address = address.ToString(),
            },
            operationName = "GetState",
        }),
            Encoding.UTF8,
            "application/json");
        using var response = await _httpClient.PostAsync(_headlessEndpoint, jsonContent);
        var resp = await response.Content.ReadFromJsonAsync<GetStateResponse>();

        if (resp.Data.State is null)
        {
            _cache.TryAdd(address, null);
            return null;
        }

        var decodedValue = Codec.Decode(Convert.FromHexString(resp.Data.State));
        _cache.TryAdd(address, decodedValue);
        return decodedValue;
    }

    public async Task<T> GetSheet<T>()
        where T : ISheet, new()
    {
        var sheetState = await GetState(Addresses.TableSheet.Derive(typeof(T).Name));
        if (sheetState is not Text sheetValue)
        {
            throw new ArgumentException(nameof(T));
        }

        var sheet = new T();
        sheet.Set(sheetValue.Value);
        return sheet;
    }

    public async Task<AvatarState> GetAvatarState(Address avatarAddress)
    {
        var state = await GetState(avatarAddress);
        if (state is not Dictionary dictionary)
        {
            throw new ArgumentException(nameof(avatarAddress));
        }

        var inventoryAddress = avatarAddress.Derive("inventory");
        var inventoryState = await GetState(inventoryAddress);
        if (inventoryState is not List list)
        {
            throw new ArgumentException(nameof(avatarAddress));
        }

        var inventory = new Inventory(list);

        var avatarState = new AvatarState(dictionary)
        {
            inventory = inventory
        };

        return avatarState;
    }

    public async Task<ItemSlotState> GetItemSlotState(Address avatarAddress)
    {
        var state = await GetState(
            ItemSlotState.DeriveAddress(avatarAddress, BattleType.Arena));
        return state switch
        {
            List list => new ItemSlotState(list),
            null => new ItemSlotState(BattleType.Arena),
            _ => throw new ArgumentException(nameof(avatarAddress))
        };
    }

    public async Task<List<RuneState>> GetRuneStates(Address avatarAddress)
    {
        var state = await GetState(
            RuneSlotState.DeriveAddress(avatarAddress, BattleType.Arena));
        var runeSlotState = state switch
        {
            List list => new RuneSlotState(list),
            null => new RuneSlotState(BattleType.Arena),
            _ => throw new ArgumentException(nameof(avatarAddress))
        };

        var runes = new List<RuneState>();
        foreach (var runeStateAddress in runeSlotState.GetEquippedRuneSlotInfos().Select(info => RuneState.DeriveAddress(avatarAddress, info.RuneId)))
        {
            if (await GetState(runeStateAddress) is List list)
            {
                runes.Add(new RuneState(list));
            }
        }

        return runes;
    }
}

record GetStateResponse(GetStateResponseData Data, object Errors);
record GetStateResponseData(string? State);
