#if TS_PLUGIN
using System.Numerics;
#elif SE_PLUGIN
using VRageMath;
#endif

namespace SharedPluginClasses;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

struct GameUpdatePacketHeader
{
    public uint Version;
    public bool InSession;
    public ulong LocalSteamID;

    public unsafe static readonly int Size = sizeof(GameUpdatePacketHeader);
}

struct GameUpdatePacket
{
    public GameUpdatePacketHeader Header;
    public Vector3 Forward;
    public Vector3 Up;
    public int PlayerCount;
    public int RemovedPlayerCount;
    public int NewPlayerCount;
    public int NewPlayerByteLength;

    public unsafe static readonly int Size = sizeof(GameUpdatePacket);
}

struct ClientGameState
{
    public ulong SteamID;
    public Vector3 Position;
    public bool HasConnection;

    public unsafe static readonly int Size = sizeof(ClientGameState);
}

#if BI_DIRECTIONAL
struct WhisperStatesHeader
{
    public int Version;
    public int NumWhisperStates;

    public unsafe static readonly int Size = sizeof(WhisperStatesHeader);
}

struct WhisperState
{
    public ulong SteamID;
    public bool IsWhispering;

    public unsafe static readonly int Size = sizeof(WhisperState);
}
#endif

#pragma warning restore CS0649
