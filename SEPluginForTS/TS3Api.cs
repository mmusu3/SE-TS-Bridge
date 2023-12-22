namespace SEPluginForTS;

using System;
using System.Runtime.InteropServices;
using anyID = System.UInt16;

/// <summary> Functions exported to plugin from main binary</summary>
public unsafe struct TS3Functions
{
    // NOTE: fixed buffers don't support pointer sized types. So this is only valid in x64 mode.
    fixed ulong pointers[226];

#pragma warning disable IDE1006 // Naming Styles
    public uint getClientLibVersion(byte** result) => ((delegate*<byte**, uint>)pointers[0])(result);
    public uint getClientLibVersionNumber(ulong* result) => ((delegate*<ulong*, uint>)pointers[1])(result);
    public uint spawnNewServerConnectionHandler(int port, ulong* result) => ((delegate*<int, ulong*, uint>)pointers[2])(port, result);
    public uint destroyServerConnectionHandler(ulong serverConnectionHandlerID) => ((delegate*<ulong, uint>)pointers[3])(serverConnectionHandlerID);

    /* Error handling */
    public uint getErrorMessage(uint errorCode, byte** error) => ((delegate*<uint, byte**, uint>)pointers[4])(errorCode, error);

    /* Memory management */
    public uint freeMemory(void* pointer) => ((delegate*<void*, uint>)pointers[5])(pointer);

    /* Logging */
    public uint logMessage(/*const */byte* logMessage, LogLevel severity, /*const */byte* channel, ulong logID) => ((delegate*</*const */byte*, LogLevel, /*const */byte*, ulong, uint>)pointers[6])(logMessage, severity, channel, logID);

    /* Sound */
    public uint getPlaybackDeviceList(/*const */byte* modeID, byte**** result) => ((delegate*</*const */byte*, byte****, uint>)pointers[7])(modeID, result);
    public uint getPlaybackModeList(byte*** result) => ((delegate*<byte***, uint>)pointers[8])(result);
    public uint getCaptureDeviceList(/*const */byte* modeID, byte**** result) => ((delegate*</*const */byte*, byte****, uint>)pointers[9])(modeID, result);
    public uint getCaptureModeList(byte*** result) => ((delegate*<byte***, uint>)pointers[10])(result);
    public uint getDefaultPlaybackDevice(/*const */byte* modeID, byte*** result) => ((delegate*</*const */byte*, byte***, uint>)pointers[11])(modeID, result);
    public uint getDefaultPlayBackMode(byte** result) => ((delegate*<byte**, uint>)pointers[12])(result);
    public uint getDefaultCaptureDevice(/*const */byte* modeID, byte*** result) => ((delegate*</*const */byte*, byte***, uint>)pointers[13])(modeID, result);
    public uint getDefaultCaptureMode(byte** result) => ((delegate*<byte**, uint>)pointers[14])(result);
    public uint openPlaybackDevice(ulong serverConnectionHandlerID, /*const */byte* modeID, /*const */byte* playbackDevice) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[15])(serverConnectionHandlerID, modeID, playbackDevice);
    public uint openCaptureDevice(ulong serverConnectionHandlerID, /*const */byte* modeID, /*const */byte* captureDevice) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[16])(serverConnectionHandlerID, modeID, captureDevice);
    public uint getCurrentPlaybackDeviceName(ulong serverConnectionHandlerID, byte** result, int* isDefault) => ((delegate*<ulong, byte**, int*, uint>)pointers[17])(serverConnectionHandlerID, result, isDefault);
    public uint getCurrentPlayBackMode(ulong serverConnectionHandlerID, byte** result) => ((delegate*<ulong, byte**, uint>)pointers[18])(serverConnectionHandlerID, result);
    public uint getCurrentCaptureDeviceName(ulong serverConnectionHandlerID, byte** result, int* isDefault) => ((delegate*<ulong, byte**, int*, uint>)pointers[19])(serverConnectionHandlerID, result, isDefault);
    public uint getCurrentCaptureMode(ulong serverConnectionHandlerID, byte** result) => ((delegate*<ulong, byte**, uint>)pointers[20])(serverConnectionHandlerID, result);
    public uint initiateGracefulPlaybackShutdown(ulong serverConnectionHandlerID) => ((delegate*<ulong, uint>)pointers[21])(serverConnectionHandlerID);
    public uint closePlaybackDevice(ulong serverConnectionHandlerID) => ((delegate*<ulong, uint>)pointers[22])(serverConnectionHandlerID);
    public uint closeCaptureDevice(ulong serverConnectionHandlerID) => ((delegate*<ulong, uint>)pointers[23])(serverConnectionHandlerID);
    public uint activateCaptureDevice(ulong serverConnectionHandlerID) => ((delegate*<ulong, uint>)pointers[24])(serverConnectionHandlerID);
    public uint playWaveFileHandle(ulong serverConnectionHandlerID, /*const */byte* path, int loop, ulong* waveHandle) => ((delegate*<ulong, /*const */byte*, int, ulong*, uint>)pointers[25])(serverConnectionHandlerID, path, loop, waveHandle);
    public uint pauseWaveFileHandle(ulong serverConnectionHandlerID, ulong waveHandle, int pause) => ((delegate*<ulong, ulong, int, uint>)pointers[26])(serverConnectionHandlerID, waveHandle, pause);
    public uint closeWaveFileHandle(ulong serverConnectionHandlerID, ulong waveHandle) => ((delegate*<ulong, ulong, uint>)pointers[27])(serverConnectionHandlerID, waveHandle);
    public uint playWaveFile(ulong serverConnectionHandlerID, /*const */byte* path) => ((delegate*<ulong, /*const */byte*, uint>)pointers[28])(serverConnectionHandlerID, path);
    public uint registerCustomDevice(/*const */byte* deviceID, /*const */byte* deviceDisplayName, int capFrequency, int capChannels, int playFrequency, int playChannels) => ((delegate*</*const */byte*, /*const */byte*, int, int, int, int, uint>)pointers[29])(deviceID, deviceDisplayName, capFrequency, capChannels, playFrequency, playChannels);
    public uint unregisterCustomDevice(/*const */byte* deviceID) => ((delegate*</*const */byte*, uint>)pointers[30])(deviceID);
    public uint processCustomCaptureData(/*const */byte* deviceName, /*const */short* buffer, int samples) => ((delegate*</*const */byte*, /*const */short*, int, uint>)pointers[31])(deviceName, buffer, samples);
    public uint acquireCustomPlaybackData(/*const */byte* deviceName, short* buffer, int samples) => ((delegate*</*const */byte*, short*, int, uint>)pointers[32])(deviceName, buffer, samples);

    /* Preprocessor */
    public uint getPreProcessorInfoValueFloat(ulong serverConnectionHandlerID, /*const */byte* ident, float* result) => ((delegate*<ulong, /*const */byte*, float*, uint>)pointers[33])(serverConnectionHandlerID, ident, result);
    public uint getPreProcessorConfigValue(ulong serverConnectionHandlerID, /*const */byte* ident, byte** result) => ((delegate*<ulong, /*const */byte*, byte**, uint>)pointers[34])(serverConnectionHandlerID, ident, result);
    public uint setPreProcessorConfigValue(ulong serverConnectionHandlerID, /*const */byte* ident, /*const */byte* value) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[35])(serverConnectionHandlerID, ident, value);

    /* Encoder */
    public uint getEncodeConfigValue(ulong serverConnectionHandlerID, /*const */byte* ident, byte** result) => ((delegate*<ulong, /*const */byte*, byte**, uint>)pointers[36])(serverConnectionHandlerID, ident, result);

    /* Playback */
    public uint getPlaybackConfigValueAsFloat(ulong serverConnectionHandlerID, /*const */byte* ident, float* result) => ((delegate*<ulong, /*const */byte*, float*, uint>)pointers[37])(serverConnectionHandlerID, ident, result);
    public uint setPlaybackConfigValue(ulong serverConnectionHandlerID, /*const */byte* ident, /*const */byte* value) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[38])(serverConnectionHandlerID, ident, value);
    public uint setClientVolumeModifier(ulong serverConnectionHandlerID, anyID clientID, float value) => ((delegate*<ulong, anyID, float, uint>)pointers[39])(serverConnectionHandlerID, clientID, value);

    /* Recording status */
    public uint startVoiceRecording(ulong serverConnectionHandlerID) => ((delegate*<ulong, uint>)pointers[40])(serverConnectionHandlerID);
    public uint stopVoiceRecording(ulong serverConnectionHandlerID) => ((delegate*<ulong, uint>)pointers[41])(serverConnectionHandlerID);

    /* 3d sound positioning */
    public uint systemset3DListenerAttributes(ulong serverConnectionHandlerID, /*const */TS3_VECTOR* position, /*const */TS3_VECTOR* forward, /*const */TS3_VECTOR* up) => ((delegate*<ulong, /*const */TS3_VECTOR*, /*const */TS3_VECTOR*, /*const */TS3_VECTOR*, uint>)pointers[42])(serverConnectionHandlerID, position, forward, up);
    public uint set3DWaveAttributes(ulong serverConnectionHandlerID, ulong waveHandle, /*const */TS3_VECTOR* position) => ((delegate*<ulong, ulong, /*const */TS3_VECTOR*, uint>)pointers[43])(serverConnectionHandlerID, waveHandle, position);
    public uint systemset3DSettings(ulong serverConnectionHandlerID, float distanceFactor, float rolloffScale) => ((delegate*<ulong, float, float, uint>)pointers[44])(serverConnectionHandlerID, distanceFactor, rolloffScale);
    public uint channelset3DAttributes(ulong serverConnectionHandlerID, anyID clientID, /*const */TS3_VECTOR* position) => ((delegate*<ulong, anyID, /*const */TS3_VECTOR*, uint>)pointers[45])(serverConnectionHandlerID, clientID, position);

    /* Interaction with the server */
    public uint startConnection(ulong serverConnectionHandlerID, /*const */byte* identity, /*const */byte* ip, uint port, /*const */byte* nickname, /*const */byte** defaultChannelArray, /*const */byte* defaultChannelPassword, /*const */byte* serverPassword) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint, /*const */byte*, /*const */byte**, /*const */byte*, /*const */byte*, uint>)pointers[46])(serverConnectionHandlerID, identity, ip, port, nickname, defaultChannelArray, defaultChannelPassword, serverPassword);
    public uint stopConnection(ulong serverConnectionHandlerID, /*const */byte* quitMessage) => ((delegate*<ulong, /*const */byte*, uint>)pointers[47])(serverConnectionHandlerID, quitMessage);
    public uint requestClientMove(ulong serverConnectionHandlerID, anyID clientID, ulong newChannelID, /*const */byte* password, /*const */byte* returnCode) => ((delegate*<ulong, anyID, ulong, /*const */byte*, /*const */byte*, uint>)pointers[48])(serverConnectionHandlerID, clientID, newChannelID, password, returnCode);
    public uint requestClientVariables(ulong serverConnectionHandlerID, anyID clientID, /*const */byte* returnCode) => ((delegate*<ulong, anyID, /*const */byte*, uint>)pointers[49])(serverConnectionHandlerID, clientID, returnCode);
    public uint requestClientKickFromChannel(ulong serverConnectionHandlerID, anyID clientID, /*const */byte* kickReason, /*const */byte* returnCode) => ((delegate*<ulong, anyID, /*const */byte*, /*const */byte*, uint>)pointers[50])(serverConnectionHandlerID, clientID, kickReason, returnCode);
    public uint requestClientKickFromServer(ulong serverConnectionHandlerID, anyID clientID, /*const */byte* kickReason, /*const */byte* returnCode) => ((delegate*<ulong, anyID, /*const */byte*, /*const */byte*, uint>)pointers[51])(serverConnectionHandlerID, clientID, kickReason, returnCode);
    public uint requestChannelDelete(ulong serverConnectionHandlerID, ulong channelID, int force, /*const */byte* returnCode) => ((delegate*<ulong, ulong, int, /*const */byte*, uint>)pointers[52])(serverConnectionHandlerID, channelID, force, returnCode);
    public uint requestChannelMove(ulong serverConnectionHandlerID, ulong channelID, ulong newChannelParentID, ulong newChannelOrder, /*const */byte* returnCode) => ((delegate*<ulong, ulong, ulong, ulong, /*const */byte*, uint>)pointers[53])(serverConnectionHandlerID, channelID, newChannelParentID, newChannelOrder, returnCode);
    public uint requestSendPrivateTextMsg(ulong serverConnectionHandlerID, /*const */byte* message, anyID targetClientID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, anyID, /*const */byte*, uint>)pointers[54])(serverConnectionHandlerID, message, targetClientID, returnCode);
    public uint requestSendChannelTextMsg(ulong serverConnectionHandlerID, /*const */byte* message, ulong targetChannelID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, ulong, /*const */byte*, uint>)pointers[55])(serverConnectionHandlerID, message, targetChannelID, returnCode);
    public uint requestSendServerTextMsg(ulong serverConnectionHandlerID, /*const */byte* message, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[56])(serverConnectionHandlerID, message, returnCode);
    public uint requestConnectionInfo(ulong serverConnectionHandlerID, anyID clientID, /*const */byte* returnCode) => ((delegate*<ulong, anyID, /*const */byte*, uint>)pointers[57])(serverConnectionHandlerID, clientID, returnCode);
    public uint requestClientSetWhisperList(ulong serverConnectionHandlerID, anyID clientID, /*const */ulong* targetChannelIDArray, /*const */anyID* targetClientIDArray, /*const */byte* returnCode) => ((delegate*<ulong, anyID, /*const */ulong*, /*const */anyID*, /*const */byte*, uint>)pointers[58])(serverConnectionHandlerID, clientID, targetChannelIDArray, targetClientIDArray, returnCode);
    public uint requestChannelSubscribe(ulong serverConnectionHandlerID, /*const */ulong* channelIDArray, /*const */byte* returnCode) => ((delegate*<ulong, /*const */ulong*, /*const */byte*, uint>)pointers[59])(serverConnectionHandlerID, channelIDArray, returnCode);
    public uint requestChannelSubscribeAll(ulong serverConnectionHandlerID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, uint>)pointers[60])(serverConnectionHandlerID, returnCode);
    public uint requestChannelUnsubscribe(ulong serverConnectionHandlerID, /*const */ulong* channelIDArray, /*const */byte* returnCode) => ((delegate*<ulong, /*const */ulong*, /*const */byte*, uint>)pointers[61])(serverConnectionHandlerID, channelIDArray, returnCode);
    public uint requestChannelUnsubscribeAll(ulong serverConnectionHandlerID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, uint>)pointers[62])(serverConnectionHandlerID, returnCode);
    public uint requestChannelDescription(ulong serverConnectionHandlerID, ulong channelID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[63])(serverConnectionHandlerID, channelID, returnCode);
    public uint requestMuteClients(ulong serverConnectionHandlerID, /*const */anyID* clientIDArray, /*const */byte* returnCode) => ((delegate*<ulong, /*const */anyID*, /*const */byte*, uint>)pointers[64])(serverConnectionHandlerID, clientIDArray, returnCode);
    public uint requestUnmuteClients(ulong serverConnectionHandlerID, /*const */anyID* clientIDArray, /*const */byte* returnCode) => ((delegate*<ulong, /*const */anyID*, /*const */byte*, uint>)pointers[65])(serverConnectionHandlerID, clientIDArray, returnCode);
    public uint requestClientPoke(ulong serverConnectionHandlerID, anyID clientID, /*const */byte* message, /*const */byte* returnCode) => ((delegate*<ulong, anyID, /*const */byte*, /*const */byte*, uint>)pointers[66])(serverConnectionHandlerID, clientID, message, returnCode);
    public uint requestClientIDs(ulong serverConnectionHandlerID, /*const */byte* clientUniqueIdentifier, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[67])(serverConnectionHandlerID, clientUniqueIdentifier, returnCode);
    public uint clientChatClosed(ulong serverConnectionHandlerID, /*const */byte* clientUniqueIdentifier, anyID clientID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, anyID, /*const */byte*, uint>)pointers[68])(serverConnectionHandlerID, clientUniqueIdentifier, clientID, returnCode);
    public uint clientChatComposing(ulong serverConnectionHandlerID, anyID clientID, /*const */byte* returnCode) => ((delegate*<ulong, anyID, /*const */byte*, uint>)pointers[69])(serverConnectionHandlerID, clientID, returnCode);
    public uint requestServerTemporaryPasswordAdd(ulong serverConnectionHandlerID, /*const */byte* password, /*const */byte* description, ulong duration, ulong targetChannelID, /*const */byte* targetChannelPW, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, ulong, ulong, /*const */byte*, /*const */byte*, uint>)pointers[70])(serverConnectionHandlerID, password, description, duration, targetChannelID, targetChannelPW, returnCode);
    public uint requestServerTemporaryPasswordDel(ulong serverConnectionHandlerID, /*const */byte* password, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[71])(serverConnectionHandlerID, password, returnCode);
    public uint requestServerTemporaryPasswordList(ulong serverConnectionHandlerID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, uint>)pointers[72])(serverConnectionHandlerID, returnCode);

    /* Access clientlib information */

    /* Query own client ID */
    public uint getClientID(ulong serverConnectionHandlerID, anyID* result) => ((delegate*<ulong, anyID*, uint>)pointers[73])(serverConnectionHandlerID, result);

    /* Client info */
    public uint getClientSelfVariableAsInt(ulong serverConnectionHandlerID, nint flag, int* result) => ((delegate*<ulong, nint, int*, uint>)pointers[74])(serverConnectionHandlerID, flag, result);
    public uint getClientSelfVariableAsString(ulong serverConnectionHandlerID, nint flag, byte** result) => ((delegate*<ulong, nint, byte**, uint>)pointers[75])(serverConnectionHandlerID, flag, result);
    public uint setClientSelfVariableAsInt(ulong serverConnectionHandlerID, nint flag, int value) => ((delegate*<ulong, nint, int, uint>)pointers[76])(serverConnectionHandlerID, flag, value);
    public uint setClientSelfVariableAsString(ulong serverConnectionHandlerID, nint flag, /*const */byte* value) => ((delegate*<ulong, nint, /*const */byte*, uint>)pointers[77])(serverConnectionHandlerID, flag, value);
    public uint flushClientSelfUpdates(ulong serverConnectionHandlerID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, uint>)pointers[78])(serverConnectionHandlerID, returnCode);
    public uint getClientVariableAsInt(ulong serverConnectionHandlerID, anyID clientID, nint flag, int* result) => ((delegate*<ulong, anyID, nint, int*, uint>)pointers[79])(serverConnectionHandlerID, clientID, flag, result);
    public uint getClientVariableAsulong(ulong serverConnectionHandlerID, anyID clientID, nint flag, ulong* result) => ((delegate*<ulong, anyID, nint, ulong*, uint>)pointers[80])(serverConnectionHandlerID, clientID, flag, result);
    public uint getClientVariableAsString(ulong serverConnectionHandlerID, anyID clientID, nint flag, byte** result) => ((delegate*<ulong, anyID, nint, byte**, uint>)pointers[81])(serverConnectionHandlerID, clientID, flag, result);
    public uint getClientList(ulong serverConnectionHandlerID, anyID** result) => ((delegate*<ulong, anyID**, uint>)pointers[82])(serverConnectionHandlerID, result);
    public uint getChannelOfClient(ulong serverConnectionHandlerID, anyID clientID, ulong* result) => ((delegate*<ulong, anyID, ulong*, uint>)pointers[83])(serverConnectionHandlerID, clientID, result);

    /* Channel info */
    public uint getChannelVariableAsInt(ulong serverConnectionHandlerID, ulong channelID, nint flag, int* result) => ((delegate*<ulong, ulong, nint, int*, uint>)pointers[84])(serverConnectionHandlerID, channelID, flag, result);
    public uint getChannelVariableAsulong(ulong serverConnectionHandlerID, ulong channelID, nint flag, ulong* result) => ((delegate*<ulong, ulong, nint, ulong*, uint>)pointers[85])(serverConnectionHandlerID, channelID, flag, result);
    public uint getChannelVariableAsString(ulong serverConnectionHandlerID, ulong channelID, nint flag, byte** result) => ((delegate*<ulong, ulong, nint, byte**, uint>)pointers[86])(serverConnectionHandlerID, channelID, flag, result);
    public uint getChannelIDFromChannelNames(ulong serverConnectionHandlerID, byte** channelNameArray, ulong* result) => ((delegate*<ulong, byte**, ulong*, uint>)pointers[87])(serverConnectionHandlerID, channelNameArray, result);
    public uint setChannelVariableAsInt(ulong serverConnectionHandlerID, ulong channelID, nint flag, int value) => ((delegate*<ulong, ulong, nint, int, uint>)pointers[88])(serverConnectionHandlerID, channelID, flag, value);
    public uint setChannelVariableAsulong(ulong serverConnectionHandlerID, ulong channelID, nint flag, ulong value) => ((delegate*<ulong, ulong, nint, ulong, uint>)pointers[89])(serverConnectionHandlerID, channelID, flag, value);
    public uint setChannelVariableAsString(ulong serverConnectionHandlerID, ulong channelID, nint flag, /*const */byte* value) => ((delegate*<ulong, ulong, nint, /*const */byte*, uint>)pointers[90])(serverConnectionHandlerID, channelID, flag, value);
    public uint flushChannelUpdates(ulong serverConnectionHandlerID, ulong channelID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[91])(serverConnectionHandlerID, channelID, returnCode);
    public uint flushChannelCreation(ulong serverConnectionHandlerID, ulong channelParentID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[92])(serverConnectionHandlerID, channelParentID, returnCode);
    public uint getChannelList(ulong serverConnectionHandlerID, ulong** result) => ((delegate*<ulong, ulong**, uint>)pointers[93])(serverConnectionHandlerID, result);
    public uint getChannelClientList(ulong serverConnectionHandlerID, ulong channelID, anyID** result) => ((delegate*<ulong, ulong, anyID**, uint>)pointers[94])(serverConnectionHandlerID, channelID, result);
    public uint getParentChannelOfChannel(ulong serverConnectionHandlerID, ulong channelID, ulong* result) => ((delegate*<ulong, ulong, ulong*, uint>)pointers[95])(serverConnectionHandlerID, channelID, result);

    /* Server info */
    public uint getServerConnectionHandlerList(ulong** result) => ((delegate*<ulong**, uint>)pointers[96])(result);
    public uint getServerVariableAsInt(ulong serverConnectionHandlerID, nint flag, int* result) => ((delegate*<ulong, nint, int*, uint>)pointers[97])(serverConnectionHandlerID, flag, result);
    public uint getServerVariableAsulong(ulong serverConnectionHandlerID, nint flag, ulong* result) => ((delegate*<ulong, nint, ulong*, uint>)pointers[98])(serverConnectionHandlerID, flag, result);
    public uint getServerVariableAsString(ulong serverConnectionHandlerID, nint flag, byte** result) => ((delegate*<ulong, nint, byte**, uint>)pointers[99])(serverConnectionHandlerID, flag, result);
    public uint requestServerVariables(ulong serverConnectionHandlerID) => ((delegate*<ulong, uint>)pointers[100])(serverConnectionHandlerID);

    /* Connection info */
    public uint getConnectionStatus(ulong serverConnectionHandlerID, int* result) => ((delegate*<ulong, int*, uint>)pointers[101])(serverConnectionHandlerID, result);
    public uint getConnectionVariableAsulong(ulong serverConnectionHandlerID, anyID clientID, nint flag, ulong* result) => ((delegate*<ulong, anyID, nint, ulong*, uint>)pointers[102])(serverConnectionHandlerID, clientID, flag, result);
    public uint getConnectionVariableAsDouble(ulong serverConnectionHandlerID, anyID clientID, nint flag, double* result) => ((delegate*<ulong, anyID, nint, double*, uint>)pointers[103])(serverConnectionHandlerID, clientID, flag, result);
    public uint getConnectionVariableAsString(ulong serverConnectionHandlerID, anyID clientID, nint flag, byte** result) => ((delegate*<ulong, anyID, nint, byte**, uint>)pointers[104])(serverConnectionHandlerID, clientID, flag, result);
    public uint cleanUpConnectionInfo(ulong serverConnectionHandlerID, anyID clientID) => ((delegate*<ulong, anyID, uint>)pointers[105])(serverConnectionHandlerID, clientID);

    /* Client related */
    public uint requestClientDBIDfromUID(ulong serverConnectionHandlerID, /*const */byte* clientUniqueIdentifier, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[106])(serverConnectionHandlerID, clientUniqueIdentifier, returnCode);
    public uint requestClientNamefromUID(ulong serverConnectionHandlerID, /*const */byte* clientUniqueIdentifier, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[107])(serverConnectionHandlerID, clientUniqueIdentifier, returnCode);
    public uint requestClientNamefromDBID(ulong serverConnectionHandlerID, ulong clientDatabaseID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[108])(serverConnectionHandlerID, clientDatabaseID, returnCode);
    public uint requestClientEditDescription(ulong serverConnectionHandlerID, anyID clientID, /*const */byte* clientDescription, /*const */byte* returnCode) => ((delegate*<ulong, anyID, /*const */byte*, /*const */byte*, uint>)pointers[109])(serverConnectionHandlerID, clientID, clientDescription, returnCode);
    public uint requestClientSetIsTalker(ulong serverConnectionHandlerID, anyID clientID, int isTalker, /*const */byte* returnCode) => ((delegate*<ulong, anyID, int, /*const */byte*, uint>)pointers[110])(serverConnectionHandlerID, clientID, isTalker, returnCode);
    public uint requestIsTalker(ulong serverConnectionHandlerID, int isTalkerRequest, /*const */byte* isTalkerRequestMessage, /*const */byte* returnCode) => ((delegate*<ulong, int, /*const */byte*, /*const */byte*, uint>)pointers[111])(serverConnectionHandlerID, isTalkerRequest, isTalkerRequestMessage, returnCode);

    /* Plugin related */
    public uint requestSendClientQueryCommand(ulong serverConnectionHandlerID, /*const */byte* command, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[112])(serverConnectionHandlerID, command, returnCode);

    /* Filetransfer */
    public uint getTransferFileName(anyID transferID, byte** result) => ((delegate*<anyID, byte**, uint>)pointers[113])(transferID, result);
    public uint getTransferFilePath(anyID transferID, byte** result) => ((delegate*<anyID, byte**, uint>)pointers[114])(transferID, result);
    public uint getTransferFileSize(anyID transferID, ulong* result) => ((delegate*<anyID, ulong*, uint>)pointers[115])(transferID, result);
    public uint getTransferFileSizeDone(anyID transferID, ulong* result) => ((delegate*<anyID, ulong*, uint>)pointers[116])(transferID, result);
    public uint isTransferSender(anyID transferID, int* result) => ((delegate*<anyID, int*, uint>)pointers[117])(transferID, result);  /* 1 == upload, 0 == download */
    public uint getTransferStatus(anyID transferID, int* result) => ((delegate*<anyID, int*, uint>)pointers[118])(transferID, result);
    public uint getCurrentTransferSpeed(anyID transferID, float* result) => ((delegate*<anyID, float*, uint>)pointers[119])(transferID, result);
    public uint getAverageTransferSpeed(anyID transferID, float* result) => ((delegate*<anyID, float*, uint>)pointers[120])(transferID, result);
    public uint getTransferRunTime(anyID transferID, ulong* result) => ((delegate*<anyID, ulong*, uint>)pointers[121])(transferID, result);
    public uint sendFile(ulong serverConnectionHandlerID, ulong channelID, /*const */byte* channelPW, /*const */byte* file, int overwrite, int resume, /*const */byte* sourceDirectory, anyID* result, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, /*const */byte*, int, int, /*const */byte*, anyID*, /*const */byte*, uint>)pointers[122])(serverConnectionHandlerID, channelID, channelPW, file, overwrite, resume, sourceDirectory, result, returnCode);
    public uint requestFile(ulong serverConnectionHandlerID, ulong channelID, /*const */byte* channelPW, /*const */byte* file, int overwrite, int resume, /*const */byte* destinationDirectory, anyID* result, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, /*const */byte*, int, int, /*const */byte*, anyID*, /*const */byte*, uint>)pointers[123])(serverConnectionHandlerID, channelID, channelPW, file, overwrite, resume, destinationDirectory, result, returnCode);
    public uint haltTransfer(ulong serverConnectionHandlerID, anyID transferID, int deleteUnfinishedFile, /*const */byte* returnCode) => ((delegate*<ulong, anyID, int, /*const */byte*, uint>)pointers[124])(serverConnectionHandlerID, transferID, deleteUnfinishedFile, returnCode);
    public uint requestFileList(ulong serverConnectionHandlerID, ulong channelID, /*const */byte* channelPW, /*const */byte* path, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, /*const */byte*, /*const */byte*, uint>)pointers[125])(serverConnectionHandlerID, channelID, channelPW, path, returnCode);
    public uint requestFileInfo(ulong serverConnectionHandlerID, ulong channelID, /*const */byte* channelPW, /*const */byte* file, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, /*const */byte*, /*const */byte*, uint>)pointers[126])(serverConnectionHandlerID, channelID, channelPW, file, returnCode);
    public uint requestDeleteFile(ulong serverConnectionHandlerID, ulong channelID, /*const */byte* channelPW, /*const */byte** file, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, /*const */byte**, /*const */byte*, uint>)pointers[127])(serverConnectionHandlerID, channelID, channelPW, file, returnCode);
    public uint requestCreateDirectory(ulong serverConnectionHandlerID, ulong channelID, /*const */byte* channelPW, /*const */byte* directoryPath, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, /*const */byte*, /*const */byte*, uint>)pointers[128])(serverConnectionHandlerID, channelID, channelPW, directoryPath, returnCode);
    public uint requestRenameFile(ulong serverConnectionHandlerID, ulong fromChannelID, /*const */byte* channelPW, ulong toChannelID, /*const */byte* toChannelPW, /*const */byte* oldFile, /*const */byte* newFile, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, ulong, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, uint>)pointers[129])(serverConnectionHandlerID, fromChannelID, channelPW, toChannelID, toChannelPW, oldFile, newFile, returnCode);

    /* Offline message management */
    public uint requestMessageAdd(ulong serverConnectionHandlerID, /*const */byte* toClientUID, /*const */byte* subject, /*const */byte* message, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, uint>)pointers[130])(serverConnectionHandlerID, toClientUID, subject, message, returnCode);
    public uint requestMessageDel(ulong serverConnectionHandlerID, ulong messageID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[131])(serverConnectionHandlerID, messageID, returnCode);
    public uint requestMessageGet(ulong serverConnectionHandlerID, ulong messageID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[132])(serverConnectionHandlerID, messageID, returnCode);
    public uint requestMessageList(ulong serverConnectionHandlerID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, uint>)pointers[133])(serverConnectionHandlerID, returnCode);
    public uint requestMessageUpdateFlag(ulong serverConnectionHandlerID, ulong messageID, int flag, /*const */byte* returnCode) => ((delegate*<ulong, ulong, int, /*const */byte*, uint>)pointers[134])(serverConnectionHandlerID, messageID, flag, returnCode);

    /* Interacting with the server - confirming passwords */
    public uint verifyServerPassword(ulong serverConnectionHandlerID, /*const */byte* serverPassword, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[135])(serverConnectionHandlerID, serverPassword, returnCode);
    public uint verifyChannelPassword(ulong serverConnectionHandlerID, ulong channelID, /*const */byte* channelPassword, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, /*const */byte*, uint>)pointers[136])(serverConnectionHandlerID, channelID, channelPassword, returnCode);

    /* Interacting with the server - banning */
    public uint banclient(ulong serverConnectionHandlerID, anyID clientID, ulong timeInSeconds, /*const */byte* banReason, /*const */byte* returnCode) => ((delegate*<ulong, anyID, ulong, /*const */byte*, /*const */byte*, uint>)pointers[137])(serverConnectionHandlerID, clientID, timeInSeconds, banReason, returnCode);
    public uint banadd(ulong serverConnectionHandlerID, /*const */byte* ipRegExp, /*const */byte* nameRegexp, /*const */byte* uniqueIdentity, /*const */byte* mytsID, ulong timeInSeconds, /*const */byte* banReason, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, ulong, /*const */byte*, /*const */byte*, uint>)pointers[138])(serverConnectionHandlerID, ipRegExp, nameRegexp, uniqueIdentity, mytsID, timeInSeconds, banReason, returnCode);
    public uint banclientdbid(ulong serverConnectionHandlerID, ulong clientDBID, ulong timeInSeconds, /*const */byte* banReason, /*const */byte* returnCode) => ((delegate*<ulong, ulong, ulong, /*const */byte*, /*const */byte*, uint>)pointers[139])(serverConnectionHandlerID, clientDBID, timeInSeconds, banReason, returnCode);
    public uint bandel(ulong serverConnectionHandlerID, ulong banID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[140])(serverConnectionHandlerID, banID, returnCode);
    public uint bandelall(ulong serverConnectionHandlerID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, uint>)pointers[141])(serverConnectionHandlerID, returnCode);
    public uint requestBanList(ulong serverConnectionHandlerID, ulong start, uint duration, /*const */byte* returnCode) => ((delegate*<ulong, ulong, uint, /*const */byte*, uint>)pointers[142])(serverConnectionHandlerID, start, duration, returnCode);

    /* Interacting with the server - complain */
    public uint requestComplainAdd(ulong serverConnectionHandlerID, ulong targetClientDatabaseID, /*const */byte* complainReason, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, /*const */byte*, uint>)pointers[143])(serverConnectionHandlerID, targetClientDatabaseID, complainReason, returnCode);
    public uint requestComplainDel(ulong serverConnectionHandlerID, ulong targetClientDatabaseID, ulong fromClientDatabaseID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, ulong, /*const */byte*, uint>)pointers[144])(serverConnectionHandlerID, targetClientDatabaseID, fromClientDatabaseID, returnCode);
    public uint requestComplainDelAll(ulong serverConnectionHandlerID, ulong targetClientDatabaseID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[145])(serverConnectionHandlerID, targetClientDatabaseID, returnCode);
    public uint requestComplainList(ulong serverConnectionHandlerID, ulong targetClientDatabaseID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[146])(serverConnectionHandlerID, targetClientDatabaseID, returnCode);

    /* Permissions */
    public uint requestServerGroupList(ulong serverConnectionHandlerID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, uint>)pointers[147])(serverConnectionHandlerID, returnCode);
    public uint requestServerGroupAdd(ulong serverConnectionHandlerID, /*const */byte* groupName, int groupType, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, int, /*const */byte*, uint>)pointers[148])(serverConnectionHandlerID, groupName, groupType, returnCode);
    public uint requestServerGroupDel(ulong serverConnectionHandlerID, ulong serverGroupID, int force, /*const */byte* returnCode) => ((delegate*<ulong, ulong, int, /*const */byte*, uint>)pointers[149])(serverConnectionHandlerID, serverGroupID, force, returnCode);
    public uint requestServerGroupAddClient(ulong serverConnectionHandlerID, ulong serverGroupID, ulong clientDatabaseID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, ulong, /*const */byte*, uint>)pointers[150])(serverConnectionHandlerID, serverGroupID, clientDatabaseID, returnCode);
    public uint requestServerGroupDelClient(ulong serverConnectionHandlerID, ulong serverGroupID, ulong clientDatabaseID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, ulong, /*const */byte*, uint>)pointers[151])(serverConnectionHandlerID, serverGroupID, clientDatabaseID, returnCode);
    public uint requestServerGroupsByClientID(ulong serverConnectionHandlerID, ulong clientDatabaseID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[152])(serverConnectionHandlerID, clientDatabaseID, returnCode);
    public uint requestServerGroupAddPerm(ulong serverConnectionHandlerID, ulong serverGroupID, int continueonerror, /*const */uint* permissionIDArray, /*const */int* permissionValueArray, /*const */int* permissionNegatedArray, /*const */int* permissionSkipArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, ulong, int, /*const */uint*, /*const */int*, /*const */int*, /*const */int*, int, /*const */byte*, uint>)pointers[153])(serverConnectionHandlerID, serverGroupID, continueonerror, permissionIDArray, permissionValueArray, permissionNegatedArray, permissionSkipArray, arraySize, returnCode);
    public uint requestServerGroupDelPerm(ulong serverConnectionHandlerID, ulong serverGroupID, int continueOnError, /*const */uint* permissionIDArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, ulong, int, /*const */uint*, int, /*const */byte*, uint>)pointers[154])(serverConnectionHandlerID, serverGroupID, continueOnError, permissionIDArray, arraySize, returnCode);
    public uint requestServerGroupPermList(ulong serverConnectionHandlerID, ulong serverGroupID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[155])(serverConnectionHandlerID, serverGroupID, returnCode);
    public uint requestServerGroupClientList(ulong serverConnectionHandlerID, ulong serverGroupID, int withNames, /*const */byte* returnCode) => ((delegate*<ulong, ulong, int, /*const */byte*, uint>)pointers[156])(serverConnectionHandlerID, serverGroupID, withNames, returnCode);
    public uint requestChannelGroupList(ulong serverConnectionHandlerID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, uint>)pointers[157])(serverConnectionHandlerID, returnCode);
    public uint requestChannelGroupAdd(ulong serverConnectionHandlerID, /*const */byte* groupName, int groupType, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, int, /*const */byte*, uint>)pointers[158])(serverConnectionHandlerID, groupName, groupType, returnCode);
    public uint requestChannelGroupDel(ulong serverConnectionHandlerID, ulong channelGroupID, int force, /*const */byte* returnCode) => ((delegate*<ulong, ulong, int, /*const */byte*, uint>)pointers[159])(serverConnectionHandlerID, channelGroupID, force, returnCode);
    public uint requestChannelGroupAddPerm(ulong serverConnectionHandlerID, ulong channelGroupID, int continueonerror, /*const */uint* permissionIDArray, /*const */int* permissionValueArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, ulong, int, /*const */uint*, /*const */int*, int, /*const */byte*, uint>)pointers[160])(serverConnectionHandlerID, channelGroupID, continueonerror, permissionIDArray, permissionValueArray, arraySize, returnCode);
    public uint requestChannelGroupDelPerm(ulong serverConnectionHandlerID, ulong channelGroupID, int continueOnError, /*const */uint* permissionIDArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, ulong, int, /*const */uint*, int, /*const */byte*, uint>)pointers[161])(serverConnectionHandlerID, channelGroupID, continueOnError, permissionIDArray, arraySize, returnCode);
    public uint requestChannelGroupPermList(ulong serverConnectionHandlerID, ulong channelGroupID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[162])(serverConnectionHandlerID, channelGroupID, returnCode);
    public uint requestSetClientChannelGroup(ulong serverConnectionHandlerID, /*const */ulong* channelGroupIDArray, /*const */ulong* channelIDArray, /*const */ulong* clientDatabaseIDArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, /*const */ulong*, /*const */ulong*, /*const */ulong*, int, /*const */byte*, uint>)pointers[163])(serverConnectionHandlerID, channelGroupIDArray, channelIDArray, clientDatabaseIDArray, arraySize, returnCode);
    public uint requestChannelAddPerm(ulong serverConnectionHandlerID, ulong channelID, /*const */uint* permissionIDArray, /*const */int* permissionValueArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */uint*, /*const */int*, int, /*const */byte*, uint>)pointers[164])(serverConnectionHandlerID, channelID, permissionIDArray, permissionValueArray, arraySize, returnCode);
    public uint requestChannelDelPerm(ulong serverConnectionHandlerID, ulong channelID, /*const */uint* permissionIDArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */uint*, int, /*const */byte*, uint>)pointers[165])(serverConnectionHandlerID, channelID, permissionIDArray, arraySize, returnCode);
    public uint requestChannelPermList(ulong serverConnectionHandlerID, ulong channelID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[166])(serverConnectionHandlerID, channelID, returnCode);
    public uint requestClientAddPerm(ulong serverConnectionHandlerID, ulong clientDatabaseID, /*const */uint* permissionIDArray, /*const */int* permissionValueArray, /*const */int* permissionSkipArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */uint*, /*const */int*, /*const */int*, int, /*const */byte*, uint>)pointers[167])(serverConnectionHandlerID, clientDatabaseID, permissionIDArray, permissionValueArray, permissionSkipArray, arraySize, returnCode);
    public uint requestClientDelPerm(ulong serverConnectionHandlerID, ulong clientDatabaseID, /*const */uint* permissionIDArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */uint*, int, /*const */byte*, uint>)pointers[168])(serverConnectionHandlerID, clientDatabaseID, permissionIDArray, arraySize, returnCode);
    public uint requestClientPermList(ulong serverConnectionHandlerID, ulong clientDatabaseID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, /*const */byte*, uint>)pointers[169])(serverConnectionHandlerID, clientDatabaseID, returnCode);
    public uint requestChannelClientAddPerm(ulong serverConnectionHandlerID, ulong channelID, ulong clientDatabaseID, /*const */uint* permissionIDArray, /*const */int* permissionValueArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, ulong, ulong, /*const */uint*, /*const */int*, int, /*const */byte*, uint>)pointers[170])(serverConnectionHandlerID, channelID, clientDatabaseID, permissionIDArray, permissionValueArray, arraySize, returnCode);
    public uint requestChannelClientDelPerm(ulong serverConnectionHandlerID, ulong channelID, ulong clientDatabaseID, /*const */uint* permissionIDArray, int arraySize, /*const */byte* returnCode) => ((delegate*<ulong, ulong, ulong, /*const */uint*, int, /*const */byte*, uint>)pointers[171])(serverConnectionHandlerID, channelID, clientDatabaseID, permissionIDArray, arraySize, returnCode);
    public uint requestChannelClientPermList(ulong serverConnectionHandlerID, ulong channelID, ulong clientDatabaseID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, ulong, /*const */byte*, uint>)pointers[172])(serverConnectionHandlerID, channelID, clientDatabaseID, returnCode);
    public uint privilegeKeyUse(ulong serverConnectionHandler, /*const */byte* tokenKey, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, uint>)pointers[173])(serverConnectionHandler, tokenKey, returnCode);
    public uint requestPermissionList(ulong serverConnectionHandler, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, uint>)pointers[174])(serverConnectionHandler, returnCode);
    public uint requestPermissionOverview(ulong serverConnectionHandler, ulong clientDBID, ulong channelID, /*const */byte* returnCode) => ((delegate*<ulong, ulong, ulong, /*const */byte*, uint>)pointers[175])(serverConnectionHandler, clientDBID, channelID, returnCode);

    /* Helper Functions */
    public uint clientPropertyStringToFlag(/*const */byte* clientPropertyString, nint* resultFlag) => ((delegate*</*const */byte*, nint*, uint>)pointers[176])(clientPropertyString, resultFlag);
    public uint channelPropertyStringToFlag(/*const */byte* channelPropertyString, nint* resultFlag) => ((delegate*</*const */byte*, nint*, uint>)pointers[177])(channelPropertyString, resultFlag);
    public uint serverPropertyStringToFlag(/*const */byte* serverPropertyString, nint* resultFlag) => ((delegate*</*const */byte*, nint*, uint>)pointers[178])(serverPropertyString, resultFlag);

    /* Client functions */
    public void getAppPath(byte* path, nint maxLen) => ((delegate*<byte*, nint, void>)pointers[179])(path, maxLen);
    public void getResourcesPath(byte* path, nint maxLen) => ((delegate*<byte*, nint, void>)pointers[180])(path, maxLen);
    public void getConfigPath(byte* path, nint maxLen) => ((delegate*<byte*, nint, void>)pointers[181])(path, maxLen);
    public void getPluginPath(byte* path, nint maxLen, /*const */byte* pluginID) => ((delegate*<byte*, nint, /*const */byte*, void>)pointers[182])(path, maxLen, pluginID);
    public ulong getCurrentServerConnectionHandlerID() => ((delegate*<ulong>)pointers[183])();
    public void printMessage(ulong serverConnectionHandlerID, /*const */byte* message, PluginMessageTarget messageTarget) => ((delegate*<ulong, /*const */byte*, PluginMessageTarget, void>)pointers[184])(serverConnectionHandlerID, message, messageTarget);
    public void printMessageToCurrentTab(/*const */byte* message) => ((delegate*</*const */byte*, void>)pointers[185])(message);
    public void urlsToBB(/*const */byte* text, byte* result, nint maxLen) => ((delegate*</*const */byte*, byte*, nint, void>)pointers[186])(text, result, maxLen);
    public void sendPluginCommand(ulong serverConnectionHandlerID, /*const */byte* pluginID, /*const */byte* command, int targetMode, /*const */anyID* targetIDs, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, int, /*const */anyID*, /*const */byte*, void>)pointers[187])(serverConnectionHandlerID, pluginID, command, targetMode, targetIDs, returnCode);
    public void getDirectories(/*const */byte* path, byte* result, nint maxLen) => ((delegate*</*const */byte*, byte*, nint, void>)pointers[188])(path, result, maxLen);
    public uint getServerConnectInfo(ulong scHandlerID, byte* host, ushort* port, byte* password, nint maxLen) => ((delegate*<ulong, byte*, ushort*, byte*, nint, uint>)pointers[189])(scHandlerID, host, port, password, maxLen);
    public uint getChannelConnectInfo(ulong scHandlerID, ulong channelID, byte* path, byte* password, nint maxLen) => ((delegate*<ulong, ulong, byte*, byte*, nint, uint>)pointers[190])(scHandlerID, channelID, path, password, maxLen);
    public void createReturnCode(/*const */byte* pluginID, byte* returnCode, nint maxLen) => ((delegate*</*const */byte*, byte*, nint, void>)pointers[191])(pluginID, returnCode, maxLen);
    public uint requestInfoUpdate(ulong scHandlerID, PluginItemType itemType, ulong itemID) => ((delegate*<ulong, PluginItemType, ulong, uint>)pointers[192])(scHandlerID, itemType, itemID);
    public ulong getServerVersion(ulong scHandlerID) => ((delegate*<ulong, ulong>)pointers[193])(scHandlerID);
    public uint isWhispering(ulong scHandlerID, anyID clientID, int* result) => ((delegate*<ulong, anyID, int*, uint>)pointers[194])(scHandlerID, clientID, result);
    public uint isReceivingWhisper(ulong scHandlerID, anyID clientID, int* result) => ((delegate*<ulong, anyID, int*, uint>)pointers[195])(scHandlerID, clientID, result);
    public uint getAvatar(ulong scHandlerID, anyID clientID, byte* result, nint maxLen) => ((delegate*<ulong, anyID, byte*, nint, uint>)pointers[196])(scHandlerID, clientID, result, maxLen);
    public void setPluginMenuEnabled(/*const */byte* pluginID, int menuID, int enabled) => ((delegate*</*const */byte*, int, int, void>)pointers[197])(pluginID, menuID, enabled);
    public void showHotkeySetup() => ((delegate*<void>)pointers[198])();
    public void requestHotkeyInputDialog(/*const */byte* pluginID, /*const */byte* keyword, int isDown, void* qParentWindow) => ((delegate*</*const */byte*, /*const */byte*, int, void*, void>)pointers[199])(pluginID, keyword, isDown, qParentWindow);
    public uint getHotkeyFromKeyword(/*const */byte* pluginID, /*const */byte** keywords, byte** hotkeys, nint arrayLen, nint hotkeyBufSize) => ((delegate*</*const */byte*, /*const */byte**, byte**, nint, nint, uint>)pointers[200])(pluginID, keywords, hotkeys, arrayLen, hotkeyBufSize);
    public uint getClientDisplayName(ulong scHandlerID, anyID clientID, byte* result, nint maxLen) => ((delegate*<ulong, anyID, byte*, nint, uint>)pointers[201])(scHandlerID, clientID, result, maxLen);
    public uint getBookmarkList(PluginBookmarkList** list) => ((delegate*<PluginBookmarkList**, uint>)pointers[202])(list);
    public uint getProfileList(PluginGuiProfile profile, int* defaultProfileIdx, byte*** result) => ((delegate*<PluginGuiProfile, int*, byte***, uint>)pointers[203])(profile, defaultProfileIdx, result);
    public uint guiConnect(PluginConnectTab connectTab, /*const */byte* serverLabel, /*const */byte* serverAddress, /*const */byte* serverPassword, /*const */byte* nickname, /*const */byte* channel, /*const */byte* channelPassword, /*const */byte* captureProfile, /*const */byte* playbackProfile, /*const */byte* hotkeyProfile, /*const */byte* soundProfile, /*const */byte* userIdentity, /*const */byte* oneTimeKey, /*const */byte* phoneticName, ulong* scHandlerID) => ((delegate*<PluginConnectTab, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, ulong*, uint>)pointers[204])(connectTab, serverLabel, serverAddress, serverPassword, nickname, channel, channelPassword, captureProfile, playbackProfile, hotkeyProfile, soundProfile, userIdentity, oneTimeKey, phoneticName, scHandlerID);
    public uint guiConnectBookmark(PluginConnectTab connectTab, /*const */byte* bookmarkuuid, ulong* scHandlerID) => ((delegate*<PluginConnectTab, /*const */byte*, ulong*, uint>)pointers[205])(connectTab, bookmarkuuid, scHandlerID);
    public uint createBookmark(/*const */byte* bookmarkuuid, /*const */byte* serverLabel, /*const */byte* serverAddress, /*const */byte* serverPassword, /*const */byte* nickname, /*const */byte* channel, /*const */byte* channelPassword, /*const */byte* captureProfile, /*const */byte* playbackProfile, /*const */byte* hotkeyProfile, /*const */byte* soundProfile, /*const */byte* uniqueUserId, /*const */byte* oneTimeKey, /*const */byte* phoneticName) => ((delegate*</*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, /*const */byte*, uint>)pointers[206])(bookmarkuuid, serverLabel, serverAddress, serverPassword, nickname, channel, channelPassword, captureProfile, playbackProfile, hotkeyProfile, soundProfile, uniqueUserId, oneTimeKey, phoneticName);
    public uint getPermissionIDByName(ulong serverConnectionHandlerID, /*const */byte* permissionName, uint* result) => ((delegate*<ulong, /*const */byte*, uint*, uint>)pointers[207])(serverConnectionHandlerID, permissionName, result);
    public uint getClientNeededPermission(ulong serverConnectionHandlerID, /*const */byte* permissionName, int* result) => ((delegate*<ulong, /*const */byte*, int*, uint>)pointers[208])(serverConnectionHandlerID, permissionName, result);
    public void notifyKeyEvent(/*const */byte* pluginID, /*const */byte* keyIdentifier, int up_down) => ((delegate*</*const */byte*, /*const */byte*, int, void>)pointers[209])(pluginID, keyIdentifier, up_down);

    /* Single-Track/Multi-Track recording */
    public uint startRecording(ulong serverConnectionHandlerID, int multitrack, int noFileSelector, /*const */byte* path) => ((delegate*<ulong, int, int, /*const */byte*, uint>)pointers[210])(serverConnectionHandlerID, multitrack, noFileSelector, path);
    public uint stopRecording(ulong serverConnectionHandlerID) => ((delegate*<ulong, uint>)pointers[211])(serverConnectionHandlerID);

    /* Convenience functions */
    public uint requestClientsMove(ulong serverConnectionHandlerID, /*const */anyID* clientIDArray, ulong newChannelID, /*const */byte* password, /*const */byte* returnCode) => ((delegate*<ulong, /*const */anyID*, ulong, /*const */byte*, /*const */byte*, uint>)pointers[212])(serverConnectionHandlerID, clientIDArray, newChannelID, password, returnCode);
    public uint requestClientsKickFromChannel(ulong serverConnectionHandlerID, /*const */anyID* clientIDArray, /*const */byte* kickReason, /*const */byte* returnCode) => ((delegate*<ulong, /*const */anyID*, /*const */byte*, /*const */byte*, uint>)pointers[213])(serverConnectionHandlerID, clientIDArray, kickReason, returnCode);
    public uint requestClientsKickFromServer(ulong serverConnectionHandlerID, /*const */anyID* clientIDArray, /*const */byte* kickReason, /*const */byte* returnCode) => ((delegate*<ulong, /*const */anyID*, /*const */byte*, /*const */byte*, uint>)pointers[214])(serverConnectionHandlerID, clientIDArray, kickReason, returnCode);
    public uint requestMuteClientsTemporary(ulong serverConnectionHandlerID, /*const */anyID* clientIDArray, /*const */byte* returnCode) => ((delegate*<ulong, /*const */anyID*, /*const */byte*, uint>)pointers[215])(serverConnectionHandlerID, clientIDArray, returnCode);
    public uint requestUnmuteClientsTemporary(ulong serverConnectionHandlerID, /*const */anyID* clientIDArray, /*const */byte* returnCode) => ((delegate*<ulong, /*const */anyID*, /*const */byte*, uint>)pointers[216])(serverConnectionHandlerID, clientIDArray, returnCode);
    public uint getPermissionNameByID(ulong scHandlerID, uint permissionID, byte* result, nint max_len) => ((delegate*<ulong, uint, byte*, nint, uint>)pointers[217])(scHandlerID, permissionID, result, max_len);
    public uint clientPropertyFlagToString(nint clientPropertyFlag, byte** resultString) => ((delegate*<nint, byte**, uint>)pointers[218])(clientPropertyFlag, resultString);
    public uint channelPropertyFlagToString(nint channelPropertyFlag, byte** resultString) => ((delegate*<nint, byte**, uint>)pointers[219])(channelPropertyFlag, resultString);
    public uint serverPropertyFlagToString(nint serverPropertyFlag, byte** resultString) => ((delegate*<nint, byte**, uint>)pointers[220])(serverPropertyFlag, resultString);

    /* Server editing */
    public uint setServerVariableAsInt(ulong serverConnectionHandlerID, nint flag, int value) => ((delegate*<ulong, nint, int, uint>)pointers[221])(serverConnectionHandlerID, flag, value);
    public uint setServerVariableAsulong(ulong serverConnectionHandlerID, nint flag, ulong value) => ((delegate*<ulong, nint, ulong, uint>)pointers[222])(serverConnectionHandlerID, flag, value);
    public uint setServerVariableAsDouble(ulong serverConnectionHandlerID, nint flag, double value) => ((delegate*<ulong, nint, double, uint>)pointers[223])(serverConnectionHandlerID, flag, value);
    public uint setServerVariableAsString(ulong serverConnectionHandlerID, nint flag, /*const */byte* value) => ((delegate*<ulong, nint, /*const */byte*, uint>)pointers[224])(serverConnectionHandlerID, flag, value);
    public uint flushServerUpdates(ulong serverConnectionHandlerID, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, uint>)pointers[225])(serverConnectionHandlerID, returnCode);
#pragma warning restore IDE1006 // Naming Styles
}

#region public_definitions.h

public struct TS3_VECTOR
{
    /// <summary>X co-ordinate in 3D space.</summary>
    public float x;

    /// <summary>Y co-ordinate in 3D space.</summary>
    public float y;

    /// <summary>Z co-ordinate in 3D space.</summary>
    public float z;
}

enum TalkStatus
{
    STATUS_NOT_TALKING = 0,
    STATUS_TALKING = 1,
    STATUS_TALKING_WHILE_DISABLED = 2,
}

enum ClientProperties
{
    /// <summary>Automatically up-to-date for any client "in view", can be used to identify this particular client installation.</summary>
    CLIENT_UNIQUE_IDENTIFIER = 0,

    /// <summary>Automatically up-to-date for any client "in view".</summary>
    CLIENT_NICKNAME,

    /// <summary>For other clients than ourself, this needs to be requested (=> requestClientVariables).</summary>
    CLIENT_VERSION,

    /// <summary>For other clients than ourself, this needs to be requested (=> requestClientVariables).</summary>
    CLIENT_PLATFORM,

    /// <summary>Automatically up-to-date for any client that can be heard (in room / whisper).</summary>
    CLIENT_FLAG_TALKING,

    /// <summary>Automatically up-to-date for any client "in view", this clients microphone mute status.</summary>
    CLIENT_INPUT_MUTED,

    /// <summary>Automatically up-to-date for any client "in view", this clients headphones/speakers/mic combined mute status.</summary>
    CLIENT_OUTPUT_MUTED,

    /// <summary>Automatically up-to-date for any client "in view", this clients headphones/speakers only mute status.</summary>
    CLIENT_OUTPUTONLY_MUTED,

    /// <summary>Automatically up-to-date for any client "in view", this clients microphone hardware status (is the capture device opened?).</summary>
    CLIENT_INPUT_HARDWARE,

    /// <summary>Automatically up-to-date for any client "in view", this clients headphone/speakers hardware status (is the playback device opened?).</summary>
    CLIENT_OUTPUT_HARDWARE,

    /// <summary>Only usable for ourself, not propagated to the network.</summary>
    CLIENT_INPUT_DEACTIVATED,

    /// <summary>Internal use.</summary>
    CLIENT_IDLE_TIME,

    /// <summary>Only usable for ourself, the default channel we used to connect on our last connection attempt.</summary>
    CLIENT_DEFAULT_CHANNEL,

    /// <summary>Internal use.</summary>
    CLIENT_DEFAULT_CHANNEL_PASSWORD,

    /// <summary>Internal use.</summary>
    CLIENT_SERVER_PASSWORD,

    /// <summary>Automatically up-to-date for any client "in view", not used by TeamSpeak, free storage for sdk users.</summary>
    CLIENT_META_DATA,

    /// <summary>Only make sense on the client side locally, "1" if this client is currently muted by us, "0" if he is not.</summary>
    CLIENT_IS_MUTED,

    /// <summary>Automatically up-to-date for any client "in view".</summary>
    CLIENT_IS_RECORDING,

    /// <summary>Internal use.</summary>
    CLIENT_VOLUME_MODIFICATOR,

    /// <summary>Sign.</summary>
    CLIENT_VERSION_SIGN,

    /// <summary>SDK use, not used by teamspeak. Hash is provided by an outside source. A channel will use the security salt + other client data to calculate a hash, which must be the same as the one provided here.</summary>
    CLIENT_SECURITY_HASH,

    /// <summary>Internal use.</summary>
    CLIENT_ENCRYPTION_CIPHERS,

    CLIENT_ENDMARKER,
}

#endregion

#region public_rare_definitions.h

enum PluginTargetMode
{
    /// <summary>Send plugincmd to all clients in current channel.</summary>
    PluginCommandTarget_CURRENT_CHANNEL = 0,

    /// <summary>Send plugincmd to all clients on server.</summary>
    PluginCommandTarget_SERVER,

    /// <summary>Send plugincmd to all given client ids.</summary>
    PluginCommandTarget_CLIENT,

    /// <summary>Send plugincmd to all subscribed clients in current channel.</summary>
    PluginCommandTarget_CURRENT_CHANNEL_SUBSCRIBED_CLIENTS,

    PluginCommandTarget_MAX
}

#endregion

#region logtypes.h

[Flags]
public enum LogTypes
{
    LogType_NONE = 0x0000,
    LogType_FILE = 0x0001,
    LogType_CONSOLE = 0x0002,
    LogType_USERLOGGING = 0x0004,
    LogType_NO_NETLOGGING = 0x0008,
    LogType_DATABASE = 0x0010,
    LogType_SYSLOG = 0x0020,
}

public enum LogLevel
{
    /// <summary>These messages stop the program.</summary>
    LogLevel_CRITICAL = 0,

    /// <summary>Everything that is really bad, but not so bad we need to shut down.</summary>
    LogLevel_ERROR,

    /// <summary>Everything that *might* be bad.</summary>
    LogLevel_WARNING,

    /// <summary>Output that might help find a problem.</summary>
    LogLevel_DEBUG,

    /// <summary>Informational output, like "starting database version x.y.z".</summary>
    LogLevel_INFO,

    /// <summary>Developer only output (will not be displayed in release mode).</summary>
    LogLevel_DEVEL
}

#endregion

#region plugin_definitions.h

/// <summary> Return values for ts3plugin_offersConfigure</summary>
public enum PluginConfigureOffer
{
    /// <summary>Plugin does not implement ts3plugin_configure</summary>
    PLUGIN_OFFERS_NO_CONFIGURE = 0,

    /// <summary>Plugin does implement ts3plugin_configure and requests to run this function in an own thread</summary>
    PLUGIN_OFFERS_CONFIGURE_NEW_THREAD,

    /// <summary>Plugin does implement ts3plugin_configure and requests to run this function in the Qt GUI thread</summary>
    PLUGIN_OFFERS_CONFIGURE_QT_THREAD
}

public enum PluginMessageTarget
{
    PLUGIN_MESSAGE_TARGET_SERVER = 0,
    PLUGIN_MESSAGE_TARGET_CHANNEL
}

public enum PluginItemType
{
    PLUGIN_SERVER = 0,
    PLUGIN_CHANNEL,
    PLUGIN_CLIENT
}

public enum PluginMenuType
{
    PLUGIN_MENU_TYPE_GLOBAL = 0,
    PLUGIN_MENU_TYPE_CHANNEL,
    PLUGIN_MENU_TYPE_CLIENT
}

public unsafe struct PluginMenuItem
{
    public const int PLUGIN_MENU_BUFSZ = 128;

    public PluginMenuType type;
    public int id;
    public fixed byte text[PLUGIN_MENU_BUFSZ];
    public fixed byte icon[PLUGIN_MENU_BUFSZ];
}

public unsafe struct PluginHotkey
{
    public const int PLUGIN_HOTKEY_BUFSZ = 128;

    public fixed byte keyword[PLUGIN_HOTKEY_BUFSZ];
    public fixed byte description[PLUGIN_HOTKEY_BUFSZ];
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct PluginBookmarkItem
{
    [FieldOffset(0)] public byte* name;
    // NOTE: 64bit only!!
    [FieldOffset(8)] public byte isFolder;
    // 3 byte padding.
    // union {
    [FieldOffset(12)] public byte* uuid;
    [FieldOffset(12)] public PluginBookmarkList* folder;
    // }
}

public unsafe struct PluginBookmarkList
{
    public int itemcount;
    public PluginBookmarkItem* items;
}

public enum PluginGuiProfile
{
    PLUGIN_GUI_SOUND_CAPTURE = 0,
    PLUGIN_GUI_SOUND_PLAYBACK,
    PLUGIN_GUI_HOTKEY,
    PLUGIN_GUI_SOUNDPACK,
    PLUGIN_GUI_IDENTITY
}

public enum PluginConnectTab
{
    PLUGIN_CONNECT_TAB_NEW = 0,
    PLUGIN_CONNECT_TAB_CURRENT,
    PLUGIN_CONNECT_TAB_NEW_IF_CURRENT_CONNECTED
}

#endregion

#region clientlib_publicdefinitions.h

public enum Visibility
{
    ENTER_VISIBILITY = 0,
    RETAIN_VISIBILITY,
    LEAVE_VISIBILITY
}

enum ConnectStatus
{
    /// <summary>There is no activity to the server, this is the default value.</summary>
    STATUS_DISCONNECTED = 0,

    /// <summary>We are trying to connect, we haven't got a clientID yet, we haven't been accepted by the server.</summary>
    STATUS_CONNECTING,

    /// <summary>The server has accepted us, we can talk and hear and we got a clientID, but we don't have the channels and clients yet, we can get server infos (welcome msg etc.)</summary>
    STATUS_CONNECTED,

    /// <summary>We are CONNECTED and we are visible.</summary>
    STATUS_CONNECTION_ESTABLISHING,

    /// <summary>We are CONNECTED and we have the client and channels available.</summary>
    STATUS_CONNECTION_ESTABLISHED,
}

#endregion

#region public_errors.h

enum Ts3ErrorType
{
    //general
    ERROR_ok = 0x0000,
    ERROR_undefined = 0x0001,
    ERROR_not_implemented = 0x0002,
    ERROR_ok_no_update = 0x0003,
    ERROR_dont_notify = 0x0004,
    ERROR_lib_time_limit_reached = 0x0005,
    ERROR_out_of_memory = 0x0006,
    ERROR_canceled = 0x0007,

    //dunno
    ERROR_command_not_found = 0x0100,
    ERROR_unable_to_bind_network_port = 0x0101,
    ERROR_no_network_port_available = 0x0102,
    ERROR_port_already_in_use = 0x0103,

    //client
    ERROR_client_invalid_id = 0x0200,
    ERROR_client_nickname_inuse = 0x0201,
    ERROR_client_protocol_limit_reached = 0x0203,
    ERROR_client_invalid_type = 0x0204,
    ERROR_client_already_subscribed = 0x0205,
    ERROR_client_not_logged_in = 0x0206,
    ERROR_client_could_not_validate_identity = 0x0207,
    ERROR_client_version_outdated = 0x020a,
    ERROR_client_is_flooding = 0x020c,
    ERROR_client_hacked = 0x020d,
    ERROR_client_cannot_verify_now = 0x020e,
    ERROR_client_login_not_permitted = 0x020f,
    ERROR_client_not_subscribed = 0x0210,

    //channel
    ERROR_channel_invalid_id = 0x0300,
    ERROR_channel_protocol_limit_reached = 0x0301,
    ERROR_channel_already_in = 0x0302,
    ERROR_channel_name_inuse = 0x0303,
    ERROR_channel_not_empty = 0x0304,
    ERROR_channel_can_not_delete_default = 0x0305,
    ERROR_channel_default_require_permanent = 0x0306,
    ERROR_channel_invalid_flags = 0x0307,
    ERROR_channel_parent_not_permanent = 0x0308,
    ERROR_channel_maxclients_reached = 0x0309,
    ERROR_channel_maxfamily_reached = 0x030a,
    ERROR_channel_invalid_order = 0x030b,
    ERROR_channel_no_filetransfer_supported = 0x030c,
    ERROR_channel_invalid_password = 0x030d,
    ERROR_channel_invalid_security_hash = 0x030f, //note 0x030e is defined in public_rare_errors;

    //server
    ERROR_server_invalid_id = 0x0400,
    ERROR_server_running = 0x0401,
    ERROR_server_is_shutting_down = 0x0402,
    ERROR_server_maxclients_reached = 0x0403,
    ERROR_server_invalid_password = 0x0404,
    ERROR_server_is_virtual = 0x0407,
    ERROR_server_is_not_running = 0x0409,
    ERROR_server_is_booting = 0x040a,
    ERROR_server_status_invalid = 0x040b,
    ERROR_server_version_outdated = 0x040d,
    ERROR_server_duplicate_running = 0x040e,

    //parameter
    ERROR_parameter_quote = 0x0600,
    ERROR_parameter_invalid_count = 0x0601,
    ERROR_parameter_invalid = 0x0602,
    ERROR_parameter_not_found = 0x0603,
    ERROR_parameter_convert = 0x0604,
    ERROR_parameter_invalid_size = 0x0605,
    ERROR_parameter_missing = 0x0606,
    ERROR_parameter_checksum = 0x0607,

    //unsorted, need further investigation
    ERROR_vs_critical = 0x0700,
    ERROR_connection_lost = 0x0701,
    ERROR_not_connected = 0x0702,
    ERROR_no_cached_connection_info = 0x0703,
    ERROR_currently_not_possible = 0x0704,
    ERROR_failed_connection_initialisation = 0x0705,
    ERROR_could_not_resolve_hostname = 0x0706,
    ERROR_invalid_server_connection_handler_id = 0x0707,
    ERROR_could_not_initialise_input_manager = 0x0708,
    ERROR_clientlibrary_not_initialised = 0x0709,
    ERROR_serverlibrary_not_initialised = 0x070a,
    ERROR_whisper_too_many_targets = 0x070b,
    ERROR_whisper_no_targets = 0x070c,
    ERROR_connection_ip_protocol_missing = 0x070d,
    //reserved                                   = 0x070e,
    ERROR_illegal_server_license = 0x070f,

    //file transfer
    ERROR_file_invalid_name = 0x0800,
    ERROR_file_invalid_permissions = 0x0801,
    ERROR_file_already_exists = 0x0802,
    ERROR_file_not_found = 0x0803,
    ERROR_file_io_error = 0x0804,
    ERROR_file_invalid_transfer_id = 0x0805,
    ERROR_file_invalid_path = 0x0806,
    ERROR_file_no_files_available = 0x0807,
    ERROR_file_overwrite_excludes_resume = 0x0808,
    ERROR_file_invalid_size = 0x0809,
    ERROR_file_already_in_use = 0x080a,
    ERROR_file_could_not_open_connection = 0x080b,
    ERROR_file_no_space_left_on_device = 0x080c,
    ERROR_file_exceeds_file_system_maximum_size = 0x080d,
    ERROR_file_transfer_connection_timeout = 0x080e,
    ERROR_file_connection_lost = 0x080f,
    ERROR_file_exceeds_supplied_size = 0x0810,
    ERROR_file_transfer_complete = 0x0811,
    ERROR_file_transfer_canceled = 0x0812,
    ERROR_file_transfer_interrupted = 0x0813,
    ERROR_file_transfer_server_quota_exceeded = 0x0814,
    ERROR_file_transfer_client_quota_exceeded = 0x0815,
    ERROR_file_transfer_reset = 0x0816,
    ERROR_file_transfer_limit_reached = 0x0817,

    //sound
    ERROR_sound_preprocessor_disabled = 0x0900,
    ERROR_sound_internal_preprocessor = 0x0901,
    ERROR_sound_internal_encoder = 0x0902,
    ERROR_sound_internal_playback = 0x0903,
    ERROR_sound_no_capture_device_available = 0x0904,
    ERROR_sound_no_playback_device_available = 0x0905,
    ERROR_sound_could_not_open_capture_device = 0x0906,
    ERROR_sound_could_not_open_playback_device = 0x0907,
    ERROR_sound_handler_has_device = 0x0908,
    ERROR_sound_invalid_capture_device = 0x0909,
    ERROR_sound_invalid_playback_device = 0x090a,
    ERROR_sound_invalid_wave = 0x090b,
    ERROR_sound_unsupported_wave = 0x090c,
    ERROR_sound_open_wave = 0x090d,
    ERROR_sound_internal_capture = 0x090e,
    ERROR_sound_device_in_use = 0x090f,
    ERROR_sound_device_already_registerred = 0x0910,
    ERROR_sound_unknown_device = 0x0911,
    ERROR_sound_unsupported_frequency = 0x0912,
    ERROR_sound_invalid_channel_count = 0x0913,
    ERROR_sound_read_wave = 0x0914,
    ERROR_sound_need_more_data = 0x0915, //for internal purposes only
    ERROR_sound_device_busy = 0x0916, //for internal purposes only
    ERROR_sound_no_data = 0x0917,
    ERROR_sound_channel_mask_mismatch = 0x0918,


    //permissions
    ERROR_permissions_client_insufficient = 0x0a08,
    ERROR_permissions = 0x0a0c,

    //accounting
    ERROR_accounting_virtualserver_limit_reached = 0x0b00,
    ERROR_accounting_slot_limit_reached = 0x0b01,
    ERROR_accounting_license_file_not_found = 0x0b02,
    ERROR_accounting_license_date_not_ok = 0x0b03,
    ERROR_accounting_unable_to_connect_to_server = 0x0b04,
    ERROR_accounting_unknown_error = 0x0b05,
    ERROR_accounting_server_error = 0x0b06,
    ERROR_accounting_instance_limit_reached = 0x0b07,
    ERROR_accounting_instance_check_error = 0x0b08,
    ERROR_accounting_license_file_invalid = 0x0b09,
    ERROR_accounting_running_elsewhere = 0x0b0a,
    ERROR_accounting_instance_duplicated = 0x0b0b,
    ERROR_accounting_already_started = 0x0b0c,
    ERROR_accounting_not_started = 0x0b0d,
    ERROR_accounting_to_many_starts = 0x0b0e,

    //provisioning server
    ERROR_provisioning_invalid_password = 0x1100,
    ERROR_provisioning_invalid_request = 0x1101,
    ERROR_provisioning_no_slots_available = 0x1102,
    ERROR_provisioning_pool_missing = 0x1103,
    ERROR_provisioning_pool_unknown = 0x1104,
    ERROR_provisioning_unknown_ip_location = 0x1105,
    ERROR_provisioning_internal_tries_exceeded = 0x1106,
    ERROR_provisioning_too_many_slots_requested = 0x1107,
    ERROR_provisioning_too_many_reserved = 0x1108,
    ERROR_provisioning_could_not_connect = 0x1109,
    ERROR_provisioning_auth_server_not_connected = 0x1110,
    ERROR_provisioning_auth_data_too_large = 0x1111,
    ERROR_provisioning_already_initialized = 0x1112,
    ERROR_provisioning_not_initialized = 0x1113,
    ERROR_provisioning_connecting = 0x1114,
    ERROR_provisioning_already_connected = 0x1115,
    ERROR_provisioning_not_connected = 0x1116,
    ERROR_provisioning_io_error = 0x1117,
    ERROR_provisioning_invalid_timeout = 0x1118,
    ERROR_provisioning_ts3server_not_found = 0x1119,
    ERROR_provisioning_no_permission = 0x111A,
}

#endregion
