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
    public uint getChannelVariableAsString(ulong serverConnectionHandlerID, ulong channelID, nint flag, byte** result) => ((delegate*<ulong, ulong, nint, byte**, uint>)pointers[86])(serverConnectionHandlerID, channelID, (nint)flag, result);
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
    public void sendPluginCommand(ulong serverConnectionHandlerID, /*const */byte* pluginID, /*const */byte* command, PluginTargetMode targetMode, /*const */anyID* targetIDs, /*const */byte* returnCode) => ((delegate*<ulong, /*const */byte*, /*const */byte*, PluginTargetMode, /*const */anyID*, /*const */byte*, void>)pointers[187])(serverConnectionHandlerID, pluginID, command, targetMode, targetIDs, returnCode);
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

    /* Server/Channel group helper functions */
    public uint getServerGroupIDByName(ulong serverConnectionHandlerID, /*const */byte* groupName, uint* result) => ((delegate*<ulong, byte*, uint*, uint>)pointers[226])(serverConnectionHandlerID, groupName, result);
    public uint getServerGroupNameByID(ulong scHandlerID, uint groupID, byte* result, nint max_len) => ((delegate*<ulong, uint, byte*, nint, uint>)pointers[227])(scHandlerID, groupID, result, max_len);
    public uint getChannelGroupIDByName(ulong serverConnectionHandlerID, /*const */byte* groupName, uint* result) => ((delegate*<ulong, byte*, uint*, uint>)pointers[228])(serverConnectionHandlerID, groupName, result);
    public uint getChannelGroupNameByID(ulong scHandlerID, uint groupID, byte* result, nint max_len) => ((delegate*<ulong, byte*, nint, uint>)pointers[229])(scHandlerID, result, max_len);
#pragma warning restore IDE1006 // Naming Styles
}

#region public_definitions.h

/// <summary>Describes a client position in 3 dimensional space, used for 3D Sound.</summary>
public struct TS3_VECTOR
{
    /// <summary>X co-ordinate in 3D space.</summary>
    public float x;

    /// <summary>Y co-ordinate in 3D space.</summary>
    public float y;

    /// <summary>Z co-ordinate in 3D space.</summary>
    public float z;
}

public enum Visibility
{
    /// <summary>Client joined from an unsubscribed channel, or joined the server.</summary>
    ENTER_VISIBILITY = 0,
    /// <summary>Client switched from one subscribed channel to a different subscribed channel.</summary>
    RETAIN_VISIBILITY,
    /// <summary>Client switches to an unsubscribed channel, or disconnected from server.</summary>
    LEAVE_VISIBILITY
}

enum ConnectStatus
{
    /// <summary>There is no activity to the server, this is the default value.</summary>
    STATUS_DISCONNECTED = 0,

    /// <summary>We are trying to connect, we haven't got a clientID yet, we haven't been accepted by the server.</summary>
    STATUS_CONNECTING,

    /// <summary>
    /// The server has accepted us, we can talk and hear and we got a clientID, but we
    /// don't have the channels and clients yet, we can get server infos (welcome msg etc.)
    /// </summary>
    STATUS_CONNECTED,

    /// <summary>We are CONNECTED and we are visible.</summary>
    STATUS_CONNECTION_ESTABLISHING,

    /// <summary>We are CONNECTED and we have the client and channels available.</summary>
    STATUS_CONNECTION_ESTABLISHED,
}

enum TalkStatus
{
    /// <summary>client is not talking</summary>
    STATUS_NOT_TALKING = 0,
    /// <summary>client is talking</summary>
    STATUS_TALKING = 1,
    /// <summary>client is talking while the microphone is muted (only valid for own client)</summary>
    STATUS_TALKING_WHILE_DISABLED = 2,
}

public enum ChannelProperties
{
    /// <summary>String.  Read/Write. Name of the channel. Always available.</summary>
    CHANNEL_NAME = 0,
    /// <summary>String.  Read/Write. Short single line text describing what the channel is about. Always available.</summary>
    CHANNEL_TOPIC,
    /// <summary>
    /// String.  Read/Write. Arbitrary text (up to 8k bytes) with information about the channel.
    /// Must be requested (<see cref="TS3Functions.requestChannelDescription"/>)
    /// </summary>
    CHANNEL_DESCRIPTION,
    /// <summary>
    /// String.  Read/Write. Password of the channel. Read access is limited to the server. Clients
    /// will only ever see the last password they attempted to use when joining the channel. Always available.
    /// </summary>
    CHANNEL_PASSWORD,
    /// <summary>
    /// Integer. Read/Write. The codec this channel is using. One of the
    /// values from the <see cref="CodecType"/> enum. Always available.
    /// </summary>
    CHANNEL_CODEC,
    /// <summary>
    /// Integer. Read/Write. The quality setting of the channel. Valid values are 0 to 10 inclusive.
    /// Higher value means better voice quality but also more bandwidth usage. Always available.
    /// </summary>
    CHANNEL_CODEC_QUALITY,
    /// <summary>
    /// Integer. Read/Write. The number of clients that can be in the channel simultaneously.
    /// Always available.
    /// </summary>
    CHANNEL_MAXCLIENTS,
    /// <summary>
    /// Integer. Read/Write. The total number of clients that can be in this channel and all
    /// sub channels of this channel. Always available.
    /// </summary>
    CHANNEL_MAXFAMILYCLIENTS,
    /// <summary>
    /// UInt64.  Read/Write. The ID of the channel below which this channel should be displayed. If 0
    /// the channel is sorted at the top of the current level. Always available.
    /// </summary>
    CHANNEL_ORDER,
    /// <summary>
    /// Integer. Read/Write. Boolean (1/0) indicating whether the channel remains when empty.
    /// Permanent channels are stored to the database and available after server restart. SDK
    /// users will need to take care of restoring channel at server start on their own.
    /// Mutually exclusive with \ref CHANNEL_FLAG_SEMI_PERMANENT. Always available.
    /// </summary>
    CHANNEL_FLAG_PERMANENT,
    /// <summary>
    /// Integer. Read/Write. Boolean (1/0) indicating whether the channel remains when
    /// empty. Semi permanent channels are not stored to disk and gone after server
    /// restart but remain while empty. Mutually exclusive with \ref
    /// CHANNEL_FLAG_PERMANENT. Always available.
    /// </summary>
    CHANNEL_FLAG_SEMI_PERMANENT,
    /// <summary>
    /// Integer. Read/Write. Boolean (1/0). The default channel is the channel that all clients
    /// are located in when they join the server, unless the client explicitly specified a
    /// different channel when connecting and is allowed to join their preferred channel. Only
    /// one channel on the server can have this flag set. The default channel must have \ref
    /// CHANNEL_FLAG_PERMANENT set. Always available.
    /// </summary>
    CHANNEL_FLAG_DEFAULT,
    /// <summary>
    /// Integer. Read/Write. Boolean (1/0) indicating whether this channel is password protected.
    /// When removing or setting \ref CHANNEL_PASSWORD you also need to adjust this flag.
    /// </summary>
    CHANNEL_FLAG_PASSWORD,
    /// <summary>
    /// (deprecated) Integer. Read/Write. Allows to increase packet size, reducing
    /// bandwith at the cost of higher latency of voice transmission. Valid values are
    /// 1-10 inclusive. 1 is the default and offers the lowest latency. Always available.
    /// </summary>
    CHANNEL_CODEC_LATENCY_FACTOR,
    /// <summary>
    /// Integer. Read/Write. Boolean (1/0). If 0 voice data is encrypted, if 1 the voice
    /// data is not encrypted. Only used if the server \ref
    /// VIRTUALSERVER_CODEC_ENCRYPTION_MODE is set to \ref CODEC_ENCRYPTION_PER_CHANNEL.
    /// Always available.
    /// </summary>
    CHANNEL_CODEC_IS_UNENCRYPTED,
    /// <summary>
    /// String.  Read/Write. SDK Only, not used by TeamSpeak. This channels security hash. When
    /// a client joins their \ref CLIENT_SECURITY_HASH is compared to this value, to allow or
    /// deny the client access to the channel. Used to enforce clients joining the server with
    /// specific identity and \ref CLIENT_META_DATA. See SDK Documentation about this feature
    /// for further details. Always available.
    /// </summary>
    CHANNEL_SECURITY_SALT,
    /// <summary>
    /// UInt64.  Read/Write. Number of seconds deletion of temporary channels is delayed after
    /// the last client leaves the channel. Channel is only deleted if empty when the delete
    /// delay expired. Always available.
    /// </summary>
    CHANNEL_DELETE_DELAY,
    /// <summary>
    /// String.  Read only.  An identifier that uniquely identifies a channel. Available in
    /// Server >= 3.10.0
    /// </summary>
    CHANNEL_UNIQUE_IDENTIFIER,

    // Rare properties

    CHANNEL_DUMMY_3,
    CHANNEL_DUMMY_4,
    CHANNEL_DUMMY_5,
    CHANNEL_DUMMY_6,
    CHANNEL_DUMMY_7,

    /// <summary>
    /// Available for all channels that are "in view", always up-to-date
    /// </summary>
    CHANNEL_FLAG_MAXCLIENTS_UNLIMITED,
    /// <summary>
    /// Available for all channels that are "in view", always up-to-date
    /// </summary>
    CHANNEL_FLAG_MAXFAMILYCLIENTS_UNLIMITED,
    /// <summary>
    /// Available for all channels that are "in view", always up-to-date
    /// </summary>
    CHANNEL_FLAG_MAXFAMILYCLIENTS_INHERITED,
    /// <summary>
    /// Only available client side, stores whether we are subscribed to this channel
    /// </summary>
    CHANNEL_FLAG_ARE_SUBSCRIBED,
    /// <summary>
    /// Not available client side, the folder used for file-transfers for this channel
    /// </summary>
    CHANNEL_FILEPATH,
    /// <summary>
    /// Available for all channels that are "in view", always up-to-date
    /// </summary>
    CHANNEL_NEEDED_TALK_POWER,
    /// <summary>
    /// Available for all channels that are "in view", always up-to-date
    /// </summary>
    CHANNEL_FORCED_SILENCE,
    /// <summary>
    /// Available for all channels that are "in view", always up-to-date
    /// </summary>
    CHANNEL_NAME_PHONETIC,
    /// <summary>
    /// Available for all channels that are "in view", always up-to-date
    /// </summary>
    CHANNEL_ICON_ID,
    /// <summary>
    /// Available for all channels that are "in view", always up-to-date
    /// </summary>
    CHANNEL_BANNER_GFX_URL,
    /// <summary>
    /// Available for all channels that are "in view", always up-to-date
    /// </summary>
    CHANNEL_BANNER_MODE,
    CHANNEL_PERMISSION_HINTS,
    /// <summary>
    /// Storage space that is allowed to be used by this channels files (in MiB)
    /// </summary>
    CHANNEL_STORAGE_QUOTA,
    CHANNEL_ENDMARKER_RARE,
    /// <summary>
    /// (for clientlibv2) expected delete time in monotonic clock seconds or 0 if nothing is expected
    /// </summary>
    CHANNEL_DELETE_DELAY_DEADLINE = 127
};

enum ClientProperties
{
    /// <summary>
    /// String. Read only. Public Identity, can be used to identify a client installation.
    /// Remains identical as long as the client keeps using the same identity.
    /// Available for visible clients.
    /// </summary>
    CLIENT_UNIQUE_IDENTIFIER = 0,

    /// <summary>String. Read/Write. Display name of the client. Available for visible clients.</summary>
    CLIENT_NICKNAME,

    /// <summary>
    /// String. Read only. Version String of the client used. For clients other than
    /// ourself this needs to be requested (<see cref="TS3Functions.requestClientVariables"/>).
    /// </summary>
    CLIENT_VERSION,

    /// <summary>
    /// String. Read only. Operating system used by the client.
    /// For other clients other than ourself this needs to
    /// be requested (<see cref="TS3Functions.requestClientVariables"/>).
    /// </summary>
    CLIENT_PLATFORM,

    /// <summary>
    /// Integer. Read only. Whether the client is talking. Available on
    /// clients that are either whispering to us, or in our channel.
    /// </summary>
    CLIENT_FLAG_TALKING,

    /// <summary>
    /// Integer. Read/Write. Microphone mute status. Available for visible
    /// clients. One of the values from the <see cref="MuteInputStatus"/> enum.
    /// </summary>
    CLIENT_INPUT_MUTED,

    /// <summary>
    /// Integer. Read only. Speaker mute status. Speaker mute implies microphone mute.
    /// Available for visible clients. One of the values from the <see cref="MuteOutputStatus"/> enum.
    /// </summary>
    CLIENT_OUTPUT_MUTED,

    /// <summary>
    /// Integer. Read only. Speaker mute status. Microphone may be active.
    /// Available for visible clients. One of the values from the <see cref="MuteOutputStatus"/> enum.
    /// </summary>
    CLIENT_OUTPUTONLY_MUTED,

    /// <summary>
    /// Integer. Read only. Indicates whether a capture device is open.
    /// Available for visible clients. One of the values from the <see cref="HardwareInputStatus"/> enum.
    /// </summary>
    CLIENT_INPUT_HARDWARE,

    /// <summary>
    /// Integer. Read only. Indicates whether a playback device is open.
    /// Available for visible clients. One of the values from the <see cref="HardwareOutputStatus"/> enum.
    /// </summary>
    CLIENT_OUTPUT_HARDWARE,

    /// <summary>
    /// Integer. Read/Write. Not available server side. Local microphone mute status.
    /// Available only for own client. Used to implement Push To Talk.
    /// One of the values from the <see cref="InputDeactivationStatus"/> enum.
    /// </summary>
    CLIENT_INPUT_DEACTIVATED,

    /// <summary>UInt64. Read only. Seconds since last activity. Available only for own client.</summary>
    CLIENT_IDLE_TIME,

    /// <summary>
    /// String. Read only. User specified channel they joined when connecting to the server.
    /// Available only for own client.
    /// </summary>
    CLIENT_DEFAULT_CHANNEL,

    /// <summary>
    /// String. Read only. User specified channel password for the channel they attempted
    /// to join when connecting to the server. Available only for own client.
    /// </summary>
    CLIENT_DEFAULT_CHANNEL_PASSWORD,

    /// <summary>
    /// String. Read only. User specified server password.
    /// Available only for own client.
    /// </summary>
    CLIENT_SERVER_PASSWORD,

    /// <summary>
    /// String. Read/Write. Can be used to store up to 4096 bytes of information
    /// on clients. Not used by TeamSpeak. Available for visible clients.
    /// </summary>
    CLIENT_META_DATA,

    /// <summary>
    /// Integer. Read only. Not available server side. Indicates whether we
    /// have muted the client using <see cref="TS3Functions.requestMuteClients"/>.
    /// Available for visible clients other than ourselves.
    /// </summary>
    CLIENT_IS_MUTED,

    /// <summary>
    /// Integer. Read only. Indicates whether the client is recording
    /// incoming audio. Available for visible clients.
    /// </summary>
    CLIENT_IS_RECORDING,

    /// <summary>
    /// Integer. Read only. Volume adjustment for this client as
    /// set by <see cref="TS3Functions.setClientVolumeModifier"/>.
    /// Available for visible clients.
    /// </summary>
    CLIENT_VOLUME_MODIFICATOR,

    /// <summary>String. Read only. TeamSpeak internal signature.</summary>
    CLIENT_VERSION_SIGN,

    /// <summary>
    /// String. Read/Write. This clients security hash. Not used by TeamSpeak, SDK only.
    /// Hash is provided by an outside source. A channel will use the security salt + other
    /// client data to calculate a hash, which must be the same as the one provided here.
    /// See SDK documentation about Client / Channel Security Hashes for more details.
    /// </summary>
    CLIENT_SECURITY_HASH,

    /// <summary>String. Read only. SDK only. List of available ciphers this client can use.</summary>
    CLIENT_ENCRYPTION_CIPHERS,

    CLIENT_ENDMARKER,
}

#endregion

#region public_rare_definitions.h

public enum PluginTargetMode
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
    /// <summary>Logging is disabled</summary>
    LogType_NONE = 0x0000,
    /// <summary>Log to regular file</summary>
    LogType_FILE = 0x0001,
    /// <summary>Log to standard output / error</summary>
    LogType_CONSOLE = 0x0002,
    /// <summary>User defined logging. Will call the 'ServerLibFunctions.onUserLoggingMessageEvent' callback for every message to be logged</summary>
    LogType_USERLOGGING = 0x0004,
    /// <summary>Not used</summary>
    LogType_NO_NETLOGGING = 0x0008,
    /// <summary>Log to database (deprecated, server only, no effect in SDK)</summary>
    LogType_DATABASE = 0x0010,
    /// <summary>Log to syslog (only available on Linux)</summary>
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

#region public_errors.h

enum Ts3ErrorType : uint
{
    //general
    ///<summary>Indicates success.</summary>
    ERROR_ok                        = 0x0000,
    ERROR_undefined                 = 0x0001,
    ///<summary>The attempted operation is not available in this context</summary>
    ERROR_not_implemented           = 0x0002,
    ///<summary>
    ///Indicates success, but no change occurred. Returned for
    ///example upon flushing (e.g. using <see cref="TS3Functions.flushChannelUpdates"/>)
    ///when all indicated changes already matched the current state.
    ///</summary>
    ERROR_ok_no_update              = 0x0003,
    ERROR_dont_notify               = 0x0004,
    ERROR_lib_time_limit_reached    = 0x0005,
    ///<summary>Not enough system memory to perform operation</summary>
    ERROR_out_of_memory             = 0x0006,
    ERROR_canceled                  = 0x0007,

    //dunno
    ERROR_command_not_found             = 0x0100,
    ///<summary>Unspecified failure to create a listening port</summary>
    ERROR_unable_to_bind_network_port   = 0x0101,
    ///<summary>Failure to initialize a listening port for FileTransfer</summary>
    ERROR_no_network_port_available     = 0x0102,
    ///<summary>Specified port is already in use by a different application</summary>
    ERROR_port_already_in_use           = 0x0103,

    //client
    ///<summary>Client no longer connected</summary>
    ERROR_client_invalid_id                     = 0x0200,
    ///<summary>Client name is already in use. Client names must be unique</summary>
    ERROR_client_nickname_inuse                 = 0x0201,
    ///<summary>Too many clients on the server</summary>
    ERROR_client_protocol_limit_reached         = 0x0203,
    ///<summary>Function called for normal clients that is only available for query clients or vice versa</summary>
    ERROR_client_invalid_type                   = 0x0204,
    ///<summary>Attempting to subscribe to a channel already subscribed to</summary>
    ERROR_client_already_subscribed             = 0x0205,
    ERROR_client_not_logged_in                  = 0x0206,
    ///<summary>Identity not valid or insufficient security level</summary>
    ERROR_client_could_not_validate_identity    = 0x0207,
    ERROR_client_invalid_password               = 0x0208,
    ///<summary>Server requires newer client version as determined by the min_client_version properties</summary>
    ERROR_client_version_outdated               = 0x020a,
    ///<summary>Triggered flood protection. Further information is supplied in the extra message if applicable.</summary>
    ERROR_client_is_flooding                    = 0x020c,
    ERROR_client_hacked                         = 0x020d,
    ERROR_client_cannot_verify_now              = 0x020e,
    ERROR_client_login_not_permitted            = 0x020f,
    ///<summary>Action is only available on subscribed channels</summary>
    ERROR_client_not_subscribed                 = 0x0210,

    //channel
    ///<summary>Channel does not exist on the server (any longer)</summary>
    ERROR_channel_invalid_id                    = 0x0300,
    ///<summary>Too many channels on the server</summary>
    ERROR_channel_protocol_limit_reached        = 0x0301,
    ///<summary>Attempting to move a client or channel to its current channel</summary>
    ERROR_channel_already_in                    = 0x0302,
    ///<summary>Channel name is already taken by another channel. Channel names must be unique</summary>
    ERROR_channel_name_inuse                    = 0x0303,
    ///<summary>Attempting to delete a channel with clients or sub channels in it</summary>
    ERROR_channel_not_empty                     = 0x0304,
    ///<summary>Default channel cannot be deleted. Set a new default channel first (see <see cref="TS3Functions.setChannelVariableAsInt"/> or \ref ts3server_setChannelVariableAsInt )</summary>
    ERROR_channel_can_not_delete_default        = 0x0305,
    ///<summary>Attempt to set a non permanent channel as default channel. Set channel to permanent first (see <see cref="TS3Functions.setChannelVariableAsInt"/> or \ref ts3server_setChannelVariableAsInt )</summary>
    ERROR_channel_default_require_permanent     = 0x0306,
    ///<summary>Invalid combination of <see cref="ChannelProperties"/>, trying to remove <see cref="ChannelProperties.CHANNEL_FLAG_DEFAULT"/> or set a password on the default channel</summary>
    ERROR_channel_invalid_flags                 = 0x0307,
    ///<summary>Attempt to move a permanent channel into a non-permanent one, or set a channel to be permanent that is a sub channel of a non-permanent one</summary>
    ERROR_channel_parent_not_permanent          = 0x0308,
    ///<summary>Channel is full as determined by its <see cref="ChannelProperties.CHANNEL_MAXCLIENTS"/> setting</summary>
    ERROR_channel_maxclients_reached            = 0x0309,
    ///<summary>Channel tree is full as determined by its <see cref="ChannelProperties.CHANNEL_MAXFAMILYCLIENTS"/> setting</summary>
    ERROR_channel_maxfamily_reached             = 0x030a,
    ///<summary>Invalid value for the <see cref="ChannelProperties.CHANNEL_ORDER"/> property. The specified channel must exist on the server and be on the same level.</summary>
    ERROR_channel_invalid_order                 = 0x030b,
    ///<summary>Invalid \ref CHANNEL_FILEPATH set for the channel</summary>
    ERROR_channel_no_filetransfer_supported     = 0x030c,
    ///<summary>Channel has a password not matching the password supplied in the call</summary>
    ERROR_channel_invalid_password              = 0x030d,
    // used in public_rare_errors                 = 0x030e,
    ERROR_channel_invalid_security_hash         = 0x030f,

    //server
    ///<summary>Chosen virtual server does not exist or is offline</summary>
    ERROR_server_invalid_id = 0x0400,
    ///<summary>attempting to delete a server that is running. Stop the server before deleting it.</summary>
    ERROR_server_running = 0x0401,
    ///<summary>Client disconnected because the server is going offline</summary>
    ERROR_server_is_shutting_down = 0x0402,
    ///<summary>
    ///Given in the onConnectStatusChange event when the server has
    ///reached its maximum number of clients as defined by the
    ///<see cref="VirtualServerProperties.VIRTUALSERVER_MAXCLIENTS"/> property
    ///</summary>
    ERROR_server_maxclients_reached = 0x0403,
    ///<summary>
    ///Specified server password is wrong. Provide the correct
    ///password in the <see cref="TS3Functions.startConnection"/> /
    ///<see cref="TS3Functions.startConnectionWithChannelID"/> call.
    ///</summary>
    ERROR_server_invalid_password = 0x0404,
    ///<summary>Server is in virtual status. The attempted action is not possible in this state. Start the virtual server first.</summary>
    ERROR_server_is_virtual = 0x0407,
    ///<summary>Attempting to stop a server that is not online.</summary>
    ERROR_server_is_not_running = 0x0409,
    ERROR_server_is_booting = 0x040a, // Not used
    ERROR_server_status_invalid = 0x040b,
    ///<summary>Attempt to connect to an outdated server version. The server needs to be updated.</summary>
    ERROR_server_version_outdated = 0x040d,
    ///<summary>This server is already running within the instance. Each virtual server may only exist once.</summary>
    ERROR_server_duplicate_running = 0x040e,

    //parameter
    ERROR_parameter_quote = 0x0600, // Not used
    ///<summary>Attempt to flush changes without previously calling set*VariableAs* since the last flush</summary>
    ERROR_parameter_invalid_count = 0x0601,
    ///<summary>At least one of the supplied parameters did not meet the criteria for that parameter</summary>
    ERROR_parameter_invalid = 0x0602,
    ///<summary>Failure to supply all the necessary parameters</summary>
    ERROR_parameter_not_found = 0x0603,
    ///<summary>Invalid type supplied for a parameter, such as passing a string (ie. "five") that expects a number.</summary>
    ERROR_parameter_convert = 0x0604,
    ///<summary>Value out of allowed range. Such as strings are too long/short or numeric values outside allowed range</summary>
    ERROR_parameter_invalid_size = 0x0605,
    ///<summary>Neglecting to specify a required parameter</summary>
    ERROR_parameter_missing = 0x0606,
    ///<summary>Attempting to deploy a modified snapshot</summary>
    ERROR_parameter_checksum = 0x0607,

    //unsorted, need further investigation
    ///<summary>Failure to create default channel</summary>
    ERROR_vs_critical = 0x0700,
    ///<summary>Generic error with the connection.</summary>
    ERROR_connection_lost = 0x0701,
    ///<summary>
    ///Attempting to call functions with a serverConnectionHandler that is
    ///not connected. You can use <see cref="TS3Functions.getConnectionStatus"/>
    ///to check whether the connection handler is connected to a server
    ///</summary>
    ERROR_not_connected = 0x0702,
    ///<summary>
    ///Attempting to query connection information (bandwidth usage, ping, etc) without
    ///requesting them first using <see cref="TS3Functions.requestConnectionInfo"/>
    ///</summary>
    ERROR_no_cached_connection_info = 0x0703,
    ///<summary>
    ///Requested information is not currently available. You may have to
    ///call <see cref="TS3Functions.requestClientVariables"/> or <see cref="TS3Functions.requestServerVariables"/>
    ///</summary>
    ERROR_currently_not_possible = 0x0704,
    ///<summary>No TeamSpeak server running on the specified IP address and port</summary>
    ERROR_failed_connection_initialisation = 0x0705,
    ///<summary>Failure to resolve the specified hostname to an IP address</summary>
    ERROR_could_not_resolve_hostname = 0x0706,
    ///<summary>Attempting to perform actions on a non-existent server connection handler</summary>
    ERROR_invalid_server_connection_handler_id = 0x0707,
    ERROR_could_not_initialise_input_manager = 0x0708, // Not used
    ///<summary>Calling client library functions without successfully calling \ref ts3client_initClientLib before</summary>
    ERROR_clientlibrary_not_initialised = 0x0709,
    ///<summary>Calling server library functions without successfully calling \ref ts3server_initServerLib before</summary>
    ERROR_serverlibrary_not_initialised = 0x070a,
    ///<summary>Using a whisper list that contain more clients than the servers \ref VIRTUALSERVER_MIN_CLIENTS_IN_CHANNEL_BEFORE_FORCED_SILENCE property</summary>
    ERROR_whisper_too_many_targets = 0x070b,
    ///<summary>The active whisper list is empty or no clients matched the whisper list (e.g. all channels in the list are empty)</summary>
    ERROR_whisper_no_targets = 0x070c,
    ///<summary>Invalid or unsupported protocol (e.g. attempting an IPv6 connection on an IPv4 only machine)</summary>
    ERROR_connection_ip_protocol_missing = 0x070d,
    ERROR_handshake_failed = 0x070e,
    ERROR_illegal_server_license = 0x070f,

    //file transfer
    ///<summary>Invalid UTF8 string or not a valid file</summary>
    ERROR_file_invalid_name = 0x0800,
    ///<summary>Permissions prevent opening the file</summary>
    ERROR_file_invalid_permissions = 0x0801,
    ///<summary>Target path already exists as a directory</summary>
    ERROR_file_already_exists = 0x0802,
    ///<summary>Attempt to access or move non existing file</summary>
    ERROR_file_not_found = 0x0803,
    ///<summary>Generic file input / output error</summary>
    ERROR_file_io_error = 0x0804,
    ///<summary>
    ///Attempt to get information about a file transfer after it has already been cleaned up.
    ///File transfer information is not available indefinitely after the transfer completed
    ///</summary>
    ERROR_file_invalid_transfer_id = 0x0805,
    ///<summary>specified path contains invalid characters or does not start with "/"</summary>
    ERROR_file_invalid_path = 0x0806,
    ERROR_file_no_files_available = 0x0807, // Not used
    ///<summary>File overwrite and resume are mutually exclusive. Only one or neither can be 1.</summary>
    ERROR_file_overwrite_excludes_resume = 0x0808,
    ///<summary>Attempt to write more bytes than claimed file size.</summary>
    ERROR_file_invalid_size = 0x0809,
    ///<summary>File is currently not available, try again later.</summary>
    ERROR_file_already_in_use = 0x080a,
    ///<summary>Generic failure in file transfer connection / other party did not conform to file transfer protocol</summary>
    ERROR_file_could_not_open_connection = 0x080b,
    ///<summary>Operating system reports hard disk is full. May be caused by quota limitations.</summary>
    ERROR_file_no_space_left_on_device = 0x080c,
    ///<summary>File is too large for the file system of the target device.</summary>
    ERROR_file_exceeds_file_system_maximum_size = 0x080d,
    ERROR_file_transfer_connection_timeout = 0x080e, // Not used
    ///<summary>File input / output timeout or connection failure</summary>
    ERROR_file_connection_lost = 0x080f,
    ERROR_file_exceeds_supplied_size = 0x0810, // Not used
    ///<summary>Indicates successful completion</summary>
    ERROR_file_transfer_complete = 0x0811,
    ///<summary>Transfer was cancelled through @ref ts3client_haltTransfer</summary>
    ERROR_file_transfer_canceled = 0x0812,
    ///<summary>Transfer failed because the server is shutting down, or network connection issues</summary>
    ERROR_file_transfer_interrupted = 0x0813,
    ///<summary>Transfer terminated due to server bandwidth quota being exceeded. No client can transfer files.</summary>
    ERROR_file_transfer_server_quota_exceeded = 0x0814,
    ///<summary>Attempt to transfer more data than allowed by this clients' bandwidth quota. Other clients may continue to transfer files.</summary>
    ERROR_file_transfer_client_quota_exceeded = 0x0815,
    ERROR_file_transfer_reset = 0x0816, // Not used
    ///<summary>Too many file transfers are in progress. Try again later</summary>
    ERROR_file_transfer_limit_reached = 0x0817,
    ERROR_file_invalid_storage_class = 0x0818, // TODO: Invalid storage class for HTTP FileTransfer (what is a storage class?)
    ///<summary>Avatar image exceeds maximum width or height accepted by the server.</summary>
    ERROR_file_invalid_dimension = 0x0819,
    ///<summary>Transfer failed because the channel quota was exceeded. Uploading to this channel is not possible, but other channels may be fine.</summary>
    ERROR_file_transfer_channel_quota_exceeded = 0x081a,

    //sound
    ///<summary>Cannot set or query pre processor variables with preprocessing disabled</summary>
    ERROR_sound_preprocessor_disabled = 0x0900,
    ERROR_sound_internal_preprocessor = 0x0901,
    ERROR_sound_internal_encoder = 0x0902,
    ERROR_sound_internal_playback = 0x0903,
    ///<summary>No audio capture devices are available</summary>
    ERROR_sound_no_capture_device_available = 0x0904,
    ///<summary>No audio playback devices are available</summary>
    ERROR_sound_no_playback_device_available = 0x0905,
    ///<summary>Error accessing audio device, or audio device does not support the requested mode</summary>
    ERROR_sound_could_not_open_capture_device = 0x0906,
    ///<summary>Error accessing audio device, or audio device does not support the requested mode</summary>
    ERROR_sound_could_not_open_playback_device = 0x0907,
    ///<summary>
    ///Attempt to open a sound device on a connection handler which already
    ///has an open device. Close the already open device first using
    ///<see cref="TS3Functions.closeCaptureDevice"/> or <see cref="TS3Functions.closePlaybackDevice"/>
    ///</summary>
    ERROR_sound_handler_has_device = 0x0908,
    ///<summary>Attempt to use a device for capture that does not support capturing audio</summary>
    ERROR_sound_invalid_capture_device = 0x0909,
    ///<summary>Attempt to use a device for playback that does not support playback of audio</summary>
    ERROR_sound_invalid_playback_device = 0x090a,
    ///<summary>Attempt to use a non WAV file in <see cref="TS3Functions.playWaveFile"/> or <see cref="TS3Functions.playWaveFileHandle"/></summary>
    ERROR_sound_invalid_wave = 0x090b,
    ///<summary>Unsupported wave file used in <see cref="TS3Functions.playWaveFile"/> or <see cref="TS3Functions.playWaveFileHandle"/>.</summary>
    ERROR_sound_unsupported_wave = 0x090c,
    ///<summary>Failure to open the specified sound file</summary>
    ERROR_sound_open_wave = 0x090d,
    ERROR_sound_internal_capture = 0x090e,
    ///<summary>
    ///Attempt to unregister a custom device that is being used. Close the device first using
    ///<see cref="TS3Functions.closeCaptureDevice"/> or <see cref="TS3Functions.closePlaybackDevice"/>
    ///</summary>
    ERROR_sound_device_in_use = 0x090f,
    ///<summary>Attempt to register a custom device with a device id that has already been used in a previous call. Device ids must be unique.</summary>
    ERROR_sound_device_already_registerred = 0x0910,
    ///<summary>
    ///Attempt to open, close, unregister or use a device which is not known. Custom devices
    ///must be registered before being used (see <see cref="TS3Functions.registerCustomDevice"/>)
    ///</summary>
    ERROR_sound_unknown_device = 0x0911,
    ERROR_sound_unsupported_frequency = 0x0912,
    ///<summary>Invalid device audio channel count, must be > 0</summary>
    ERROR_sound_invalid_channel_count = 0x0913,
    ///<summary>Failure to read sound samples from an opened wave file. Is this a valid wave file?</summary>
    ERROR_sound_read_wave = 0x0914,
    ERROR_sound_need_more_data = 0x0915, // for internal purposes only
    ERROR_sound_device_busy = 0x0916, // for internal purposes only
    ///<summary>Indicates there is currently no data for playback, e.g. nobody is speaking right now.</summary>
    ERROR_sound_no_data = 0x0917,
    ///<summary>Opening a device with an unsupported channel count</summary>
    ERROR_sound_channel_mask_mismatch = 0x0918,

    //permissions
    ///<summary>Not enough permissions to perform the requested activity</summary>
    ERROR_permissions_client_insufficient = 0x0a08,
    ///<summary>Permissions to use sound device not granted by operating system, e.g. Windows denied microphone access.</summary>
    ERROR_permissions = 0x0a0c,

    //accounting
    ///<summary>Attempt to use more virtual servers than allowed by the license</summary>
    ERROR_accounting_virtualserver_limit_reached = 0x0b00,
    ///<summary>Attempt to set more slots than allowed by the license</summary>
    ERROR_accounting_slot_limit_reached = 0x0b01,
    ERROR_accounting_license_file_not_found = 0x0b02, // Not used
    ///<summary>License expired or not valid yet</summary>
    ERROR_accounting_license_date_not_ok = 0x0b03,
    ///<summary>Failure to communicate with accounting backend</summary>
    ERROR_accounting_unable_to_connect_to_server = 0x0b04,
    ///<summary>Failure to write update license file</summary>
    ERROR_accounting_unknown_error = 0x0b05,
    ERROR_accounting_server_error = 0x0b06, // Not used
    ///<summary>More than one process of the server is running</summary>
    ERROR_accounting_instance_limit_reached = 0x0b07,
    ///<summary>Shared memory access failure.</summary>
    ERROR_accounting_instance_check_error = 0x0b08,
    ///<summary>License is not a TeamSpeak license</summary>
    ERROR_accounting_license_file_invalid = 0x0b09,
    ///<summary>A copy of this server is already running in another instance. Each server may only exist once.</summary>
    ERROR_accounting_running_elsewhere = 0x0b0a,
    ///<summary>A copy of this server is running already in this process. Each server may only exist once.</summary>
    ERROR_accounting_instance_duplicated = 0x0b0b,
    ///<summary>Attempt to start a server that is already running</summary>
    ERROR_accounting_already_started = 0x0b0c,
    ERROR_accounting_not_started = 0x0b0d,
    ///<summary>Starting instance / virtual servers too often in too short a time period</summary>
    ERROR_accounting_to_many_starts = 0x0b0e,

    //provisioning server
    /// @cond HAS_PROVISIONING
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
    /// @endcond
}

#endregion
