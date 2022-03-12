namespace TSSEPlugin;

using System;
using System.Runtime.InteropServices;
using anyID = System.UInt16;

/// <summary> Functions exported to plugin from main binary</summary>
public unsafe struct TS3Functions
{
    public delegate*<byte** /*result*/, uint> getClientLibVersion;
    public delegate*<ulong* /*result*/, uint> getClientLibVersionNumber;
    public delegate*<int /*port*/, ulong* /*result*/, uint> spawnNewServerConnectionHandler;
    public delegate*<ulong /*serverConnectionHandlerID*/, uint> destroyServerConnectionHandler;

    /* Error handling */
    public delegate*<uint /*errorCode*/, byte** /*error*/, uint> getErrorMessage;

    /* Memory management */
    public delegate*<void* /*pointer*/, uint> freeMemory;

    /* Logging */
    public delegate*</*const */byte* /*logMessage*/, LogLevel /*severity*/, /*const */byte* /*channel*/, ulong /*logID*/, uint> logMessage;

    /* Sound */
    public delegate*</*const */byte* /*modeID*/, byte**** /*result*/, uint> getPlaybackDeviceList;
    public delegate*<byte*** /*result*/, uint> getPlaybackModeList;
    public delegate*</*const */byte* /*modeID*/, byte**** /*result*/, uint> getCaptureDeviceList;
    public delegate*<byte*** /*result*/, uint> getCaptureModeList;
    public delegate*</*const */byte* /*modeID*/, byte*** /*result*/, uint> getDefaultPlaybackDevice;
    public delegate*<byte** /*result*/, uint> getDefaultPlayBackMode;
    public delegate*</*const */byte* /*modeID*/, byte*** /*result*/, uint> getDefaultCaptureDevice;
    public delegate*<byte** /*result*/, uint> getDefaultCaptureMode;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*modeID*/, /*const */byte* /*playbackDevice*/, uint> openPlaybackDevice;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*modeID*/, /*const */byte* /*captureDevice*/, uint> openCaptureDevice;
    public delegate*<ulong /*serverConnectionHandlerID*/, byte** /*result*/, int* /*isDefault*/, uint> getCurrentPlaybackDeviceName;
    public delegate*<ulong /*serverConnectionHandlerID*/, byte** /*result*/, uint> getCurrentPlayBackMode;
    public delegate*<ulong /*serverConnectionHandlerID*/, byte** /*result*/, int* /*isDefault*/, uint> getCurrentCaptureDeviceName;
    public delegate*<ulong /*serverConnectionHandlerID*/, byte** /*result*/, uint> getCurrentCaptureMode;
    public delegate*<ulong /*serverConnectionHandlerID*/, uint> initiateGracefulPlaybackShutdown;
    public delegate*<ulong /*serverConnectionHandlerID*/, uint> closePlaybackDevice;
    public delegate*<ulong /*serverConnectionHandlerID*/, uint> closeCaptureDevice;
    public delegate*<ulong /*serverConnectionHandlerID*/, uint> activateCaptureDevice;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*path*/, int /*loop*/, ulong* /*waveHandle*/, uint> playWaveFileHandle;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*waveHandle*/, int /*pause*/, uint> pauseWaveFileHandle;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*waveHandle*/, uint> closeWaveFileHandle;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*path*/, uint> playWaveFile;
    public delegate*</*const */byte* /*deviceID*/, /*const */byte* /*deviceDisplayName*/, int /*capFrequency*/, int /*capChannels*/, int /*playFrequency*/, int /*playChannels*/, uint> registerCustomDevice;
    public delegate*</*const */byte* /*deviceID*/, uint> unregisterCustomDevice;
    public delegate*</*const */byte* /*deviceName*/, /*const*/ short* /*buffer*/, int /*samples*/, uint> processCustomCaptureData;
    public delegate*</*const */byte* /*deviceName*/, short* /*buffer*/, int /*samples*/, uint> acquireCustomPlaybackData;

    /* Preprocessor */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*ident*/, float* /*result*/, uint> getPreProcessorInfoValueFloat;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*ident*/, byte** /*result*/, uint> getPreProcessorConfigValue;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*ident*/, /*const */byte* /*value*/, uint> setPreProcessorConfigValue;

    /* Encoder */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*ident*/, byte** /*result*/, uint> getEncodeConfigValue;

    /* Playback */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*ident*/, float* /*result*/, uint> getPlaybackConfigValueAsFloat;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*ident*/, /*const */byte* /*value*/, uint> setPlaybackConfigValue;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, float /*value*/, uint> setClientVolumeModifier;

    /* Recording status */
    public delegate*<ulong /*serverConnectionHandlerID*/, uint> startVoiceRecording;
    public delegate*<ulong /*serverConnectionHandlerID*/, uint> stopVoiceRecording;

    /* 3d sound positioning */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const*/ TS3_VECTOR* /*position*/, /*const*/ TS3_VECTOR* /*forward*/, /*const*/ TS3_VECTOR* /*up*/, uint> systemset3DListenerAttributes;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*waveHandle*/, /*const*/ TS3_VECTOR* /*position*/, uint> set3DWaveAttributes;
    public delegate*<ulong /*serverConnectionHandlerID*/, float /*distanceFactor*/, float /*rolloffScale*/, uint> systemset3DSettings;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, /*const*/ TS3_VECTOR* /*position*/, uint> channelset3DAttributes;

    /* Interaction with the server */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*identity*/, /*const */byte* /*ip*/, uint /*port*/, /*const */byte* /*nickname*/, /*const */byte** /*defaultChannelArray*/, /*const */byte* /*defaultChannelPassword*/, /*const */byte* /*serverPassword*/, uint> startConnection;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*quitMessage*/, uint> stopConnection;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, ulong /*newChannelID*/, /*const */byte* /*password*/, /*const */byte* /*returnCode*/, uint> requestClientMove;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, /*const */byte* /*returnCode*/, uint> requestClientVariables;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, /*const */byte* /*kickReason*/, /*const */byte* /*returnCode*/, uint> requestClientKickFromChannel;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, /*const */byte* /*kickReason*/, /*const */byte* /*returnCode*/, uint> requestClientKickFromServer;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, int /*force*/, /*const */byte* /*returnCode*/, uint> requestChannelDelete;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, ulong /*newChannelParentID*/, ulong /*newChannelOrder*/, /*const */byte* /*returnCode*/, uint> requestChannelMove;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*message*/, anyID /*targetClientID*/, /*const */byte* /*returnCode*/, uint> requestSendPrivateTextMsg;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*message*/, ulong /*targetChannelID*/, /*const */byte* /*returnCode*/, uint> requestSendChannelTextMsg;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*message*/, /*const */byte* /*returnCode*/, uint> requestSendServerTextMsg;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, /*const */byte* /*returnCode*/, uint> requestConnectionInfo;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, /*const*/ ulong* /*targetChannelIDArray*/, /*const */anyID* /*targetClientIDArray*/, /*const */byte* /*returnCode*/, uint> requestClientSetWhisperList;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const*/ ulong* /*channelIDArray*/, /*const */byte* /*returnCode*/, uint> requestChannelSubscribe;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*returnCode*/, uint> requestChannelSubscribeAll;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const*/ ulong* /*channelIDArray*/, /*const */byte* /*returnCode*/, uint> requestChannelUnsubscribe;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*returnCode*/, uint> requestChannelUnsubscribeAll;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const */byte* /*returnCode*/, uint> requestChannelDescription;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */anyID* /*clientIDArray*/, /*const */byte* /*returnCode*/, uint> requestMuteClients;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */anyID* /*clientIDArray*/, /*const */byte* /*returnCode*/, uint> requestUnmuteClients;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, /*const */byte* /*message*/, /*const */byte* /*returnCode*/, uint> requestClientPoke;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*clientUniqueIdentifier*/, /*const */byte* /*returnCode*/, uint> requestClientIDs;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*clientUniqueIdentifier*/, anyID /*clientID*/, /*const */byte* /*returnCode*/, uint> clientChatClosed;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, /*const */byte* /*returnCode*/, uint> clientChatComposing;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*password*/, /*const */byte* /*description*/, ulong /*duration*/, ulong /*targetChannelID*/, /*const */byte* /*targetChannelPW*/, /*const */byte* /*returnCode*/, uint> requestServerTemporaryPasswordAdd;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*password*/, /*const */byte* /*returnCode*/, uint> requestServerTemporaryPasswordDel;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*returnCode*/, uint> requestServerTemporaryPasswordList;

    /* Access clientlib information */

    /* Query own client ID */
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID* /*result*/, uint> getClientID;

    /* Client info */
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, int* /*result*/, uint> getClientSelfVariableAsInt;
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, byte** /*result*/, uint> getClientSelfVariableAsString;
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, int /*value*/, uint> setClientSelfVariableAsInt;
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, /*const */byte* /*value*/, uint> setClientSelfVariableAsString;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*returnCode*/, uint> flushClientSelfUpdates;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, nint /*flag*/, int* /*result*/, uint> getClientVariableAsInt;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, nint /*flag*/, ulong* /*result*/, uint> getClientVariableAsulong;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, nint /*flag*/, byte** /*result*/, uint> getClientVariableAsString;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID** /*result*/, uint> getClientList;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, ulong* /*result*/, uint> getChannelOfClient;

    /* Channel info */
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, nint /*flag*/, int* /*result*/, uint> getChannelVariableAsInt;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, nint /*flag*/, ulong* /*result*/, uint> getChannelVariableAsulong;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, nint /*flag*/, byte** /*result*/, uint> getChannelVariableAsString;
    public delegate*<ulong /*serverConnectionHandlerID*/, byte** /*channelNameArray*/, ulong* /*result*/, uint> getChannelIDFromChannelNames;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, nint /*flag*/, int /*value*/, uint> setChannelVariableAsInt;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, nint /*flag*/, ulong /*value*/, uint> setChannelVariableAsulong;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, nint /*flag*/, /*const */byte* /*value*/, uint> setChannelVariableAsString;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const */byte* /*returnCode*/, uint> flushChannelUpdates;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelParentID*/, /*const */byte* /*returnCode*/, uint> flushChannelCreation;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong** /*result*/, uint> getChannelList;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, anyID** /*result*/, uint> getChannelClientList;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, ulong* /*result*/, uint> getParentChannelOfChannel;

    /* Server info */
    public delegate*<ulong** /*result*/, uint> getServerConnectionHandlerList;
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, int* /*result*/, uint> getServerVariableAsInt;
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, ulong* /*result*/, uint> getServerVariableAsulong;
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, byte** /*result*/, uint> getServerVariableAsString;
    public delegate*<ulong /*serverConnectionHandlerID*/, uint> requestServerVariables;

    /* Connection info */
    public delegate*<ulong /*serverConnectionHandlerID*/, int* /*result*/, uint> getConnectionStatus;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, nint /*flag*/, ulong* /*result*/, uint> getConnectionVariableAsulong;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, nint /*flag*/, double* /*result*/, uint> getConnectionVariableAsDouble;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, nint /*flag*/, byte** /*result*/, uint> getConnectionVariableAsString;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, uint> cleanUpConnectionInfo;

    /* Client related */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*clientUniqueIdentifier*/, /*const */byte* /*returnCode*/, uint> requestClientDBIDfromUID;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*clientUniqueIdentifier*/, /*const */byte* /*returnCode*/, uint> requestClientNamefromUID;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*clientDatabaseID*/, /*const */byte* /*returnCode*/, uint> requestClientNamefromDBID;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, /*const */byte* /*clientDescription*/, /*const */byte* /*returnCode*/, uint> requestClientEditDescription;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, int /*isTalker*/, /*const */byte* /*returnCode*/, uint> requestClientSetIsTalker;
    public delegate*<ulong /*serverConnectionHandlerID*/, int /*isTalkerRequest*/, /*const */byte* /*isTalkerRequestMessage*/, /*const */byte* /*returnCode*/, uint> requestIsTalker;

    /* Plugin related */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*command*/, /*const */byte* /*returnCode*/, uint> requestSendClientQueryCommand;

    /* Filetransfer */
    public delegate*<anyID /*transferID*/, byte** /*result*/, uint> getTransferFileName;
    public delegate*<anyID /*transferID*/, byte** /*result*/, uint> getTransferFilePath;
    public delegate*<anyID /*transferID*/, ulong* /*result*/, uint> getTransferFileSize;
    public delegate*<anyID /*transferID*/, ulong* /*result*/, uint> getTransferFileSizeDone;
    public delegate*<anyID /*transferID*/, int* /*result*/, uint> isTransferSender;  /* 1 == upload, 0 == download */
    public delegate*<anyID /*transferID*/, int* /*result*/, uint> getTransferStatus;
    public delegate*<anyID /*transferID*/, float* /*result*/, uint> getCurrentTransferSpeed;
    public delegate*<anyID /*transferID*/, float* /*result*/, uint> getAverageTransferSpeed;
    public delegate*<anyID /*transferID*/, ulong* /*result*/, uint> getTransferRunTime;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const */byte* /*channelPW*/, /*const */byte* /*file*/, int /*overwrite*/, int /*resume*/, /*const */byte* /*sourceDirectory*/, anyID* /*result*/, /*const */byte* /*returnCode*/, uint> sendFile;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const */byte* /*channelPW*/, /*const */byte* /*file*/, int /*overwrite*/, int /*resume*/, /*const */byte* /*destinationDirectory*/, anyID* /*result*/, /*const */byte* /*returnCode*/, uint> requestFile;
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*transferID*/, int /*deleteUnfinishedFile*/, /*const */byte* /*returnCode*/, uint> haltTransfer;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const */byte* /*channelPW*/, /*const */byte* /*path*/, /*const */byte* /*returnCode*/, uint> requestFileList;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const */byte* /*channelPW*/, /*const */byte* /*file*/, /*const */byte* /*returnCode*/, uint> requestFileInfo;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const */byte* /*channelPW*/, /*const */byte** /*file*/, /*const */byte* /*returnCode*/, uint> requestDeleteFile;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const */byte* /*channelPW*/, /*const */byte* /*directoryPath*/, /*const */byte* /*returnCode*/, uint> requestCreateDirectory;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*fromChannelID*/, /*const */byte* /*channelPW*/, ulong /*toChannelID*/, /*const */byte* /*toChannelPW*/, /*const */byte* /*oldFile*/, /*const */byte* /*newFile*/, /*const */byte* /*returnCode*/, uint> requestRenameFile;

    /* Offline message management */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*toClientUID*/, /*const */byte* /*subject*/, /*const */byte* /*message*/, /*const */byte* /*returnCode*/, uint> requestMessageAdd;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*messageID*/, /*const */byte* /*returnCode*/, uint> requestMessageDel;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*messageID*/, /*const */byte* /*returnCode*/, uint> requestMessageGet;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*returnCode*/, uint> requestMessageList;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*messageID*/, int /*flag*/, /*const */byte* /*returnCode*/, uint> requestMessageUpdateFlag;

    /* Interacting with the server - confirming passwords */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*serverPassword*/, /*const */byte* /*returnCode*/, uint> verifyServerPassword;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const */byte* /*channelPassword*/, /*const */byte* /*returnCode*/, uint> verifyChannelPassword;

    /* Interacting with the server - banning */
    public delegate*<ulong /*serverConnectionHandlerID*/, anyID /*clientID*/, ulong /*timeInSeconds*/, /*const */byte* /*banReason*/, /*const */byte* /*returnCode*/, uint> banclient;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*ipRegExp*/, /*const */byte* /*nameRegexp*/, /*const */byte* /*uniqueIdentity*/, /*const */byte* /*mytsID*/, ulong /*timeInSeconds*/, /*const */byte* /*banReason*/, /*const */byte* /*returnCode*/, uint> banadd;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*clientDBID*/, ulong /*timeInSeconds*/, /*const */byte* /*banReason*/, /*const */byte* /*returnCode*/, uint> banclientdbid;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*banID*/, /*const */byte* /*returnCode*/, uint> bandel;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*returnCode*/, uint> bandelall;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*start*/, uint /*duration*/, /*const */byte* /*returnCode*/, uint> requestBanList;

    /* Interacting with the server - complain */
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*targetClientDatabaseID*/, /*const */byte* /*complainReason*/, /*const */byte* /*returnCode*/, uint> requestComplainAdd;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*targetClientDatabaseID*/, ulong /*fromClientDatabaseID*/, /*const */byte* /*returnCode*/, uint> requestComplainDel;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*targetClientDatabaseID*/, /*const */byte* /*returnCode*/, uint> requestComplainDelAll;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*targetClientDatabaseID*/, /*const */byte* /*returnCode*/, uint> requestComplainList;

    /* Permissions */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*returnCode*/, uint> requestServerGroupList;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*groupName*/, int /*groupType*/, /*const */byte* /*returnCode*/, uint> requestServerGroupAdd;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*serverGroupID*/, int /*force*/, /*const */byte* /*returnCode*/, uint> requestServerGroupDel;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*serverGroupID*/, ulong /*clientDatabaseID*/, /*const */byte* /*returnCode*/, uint> requestServerGroupAddClient;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*serverGroupID*/, ulong /*clientDatabaseID*/, /*const */byte* /*returnCode*/, uint> requestServerGroupDelClient;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*clientDatabaseID*/, /*const */byte* /*returnCode*/, uint> requestServerGroupsByClientID;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*serverGroupID*/, int /*continueonerror*/, /*const*/ uint* /*permissionIDArray*/, /*const*/ int* /*permissionValueArray*/, /*const*/ int* /*permissionNegatedArray*/, /*const*/ int* /*permissionSkipArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestServerGroupAddPerm;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*serverGroupID*/, int /*continueOnError*/, /*const*/ uint* /*permissionIDArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestServerGroupDelPerm;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*serverGroupID*/, /*const */byte* /*returnCode*/, uint> requestServerGroupPermList;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*serverGroupID*/, int /*withNames*/, /*const */byte* /*returnCode*/, uint> requestServerGroupClientList;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*returnCode*/, uint> requestChannelGroupList;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*groupName*/, int /*groupType*/, /*const */byte* /*returnCode*/, uint> requestChannelGroupAdd;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelGroupID*/, int /*force*/, /*const */byte* /*returnCode*/, uint> requestChannelGroupDel;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelGroupID*/, int /*continueonerror*/, /*const*/ uint* /*permissionIDArray*/, /*const*/ int* /*permissionValueArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestChannelGroupAddPerm;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelGroupID*/, int /*continueOnError*/, /*const*/ uint* /*permissionIDArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestChannelGroupDelPerm;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelGroupID*/, /*const */byte* /*returnCode*/, uint> requestChannelGroupPermList;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const*/ ulong* /*channelGroupIDArray*/, /*const*/ ulong* /*channelIDArray*/, /*const*/ ulong* /*clientDatabaseIDArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestSetClientChannelGroup;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const*/ uint* /*permissionIDArray*/, /*const*/ int* /*permissionValueArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestChannelAddPerm;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const*/ uint* /*permissionIDArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestChannelDelPerm;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, /*const */byte* /*returnCode*/, uint> requestChannelPermList;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*clientDatabaseID*/, /*const*/ uint* /*permissionIDArray*/, /*const*/ int* /*permissionValueArray*/, /*const*/ int* /*permissionSkipArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestClientAddPerm;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*clientDatabaseID*/, /*const*/ uint* /*permissionIDArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestClientDelPerm;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*clientDatabaseID*/, /*const */byte* /*returnCode*/, uint> requestClientPermList;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, ulong /*clientDatabaseID*/, /*const*/ uint* /*permissionIDArray*/, /*const*/ int* /*permissionValueArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestChannelClientAddPerm;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, ulong /*clientDatabaseID*/, /*const*/ uint* /*permissionIDArray*/, int /*arraySize*/, /*const */byte* /*returnCode*/, uint> requestChannelClientDelPerm;
    public delegate*<ulong /*serverConnectionHandlerID*/, ulong /*channelID*/, ulong /*clientDatabaseID*/, /*const */byte* /*returnCode*/, uint> requestChannelClientPermList;
    public delegate*<ulong /*serverConnectionHandler*/, /*const */byte* /*tokenKey*/, /*const */byte* /*returnCode*/, uint> privilegeKeyUse;
    public delegate*<ulong /*serverConnectionHandler*/, /*const */byte* /*returnCode*/, uint> requestPermissionList;
    public delegate*<ulong /*serverConnectionHandler*/, ulong /*clientDBID*/, ulong /*channelID*/, /*const */byte* /*returnCode*/, uint> requestPermissionOverview;

    /* Helper Functions */
    public delegate*</*const */byte* /*clientPropertyString*/, nint* /*resultFlag*/, uint> clientPropertyStringToFlag;
    public delegate*</*const */byte* /*channelPropertyString*/, nint* /*resultFlag*/, uint> channelPropertyStringToFlag;
    public delegate*</*const */byte* /*serverPropertyString*/, nint* /*resultFlag*/, uint> serverPropertyStringToFlag;

    /* Client functions */
    public delegate*<byte* /*path*/, nint /*maxLen*/, void> getAppPath;
    public delegate*<byte* /*path*/, nint /*maxLen*/, void> getResourcesPath;
    public delegate*<byte* /*path*/, nint /*maxLen*/, void> getConfigPath;
    public delegate*<byte* /*path*/, nint /*maxLen*/, /*const */byte* /*pluginID*/, void> getPluginPath;
    public delegate*<ulong> getCurrentServerConnectionHandlerID;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*message*/, PluginMessageTarget /*messageTarget*/, void> printMessage;
    public delegate*</*const */byte* /*message*/, void> printMessageToCurrentTab;
    public delegate*</*const */byte* /*text*/, byte* /*result*/, nint /*maxLen*/, void> urlsToBB;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*pluginID*/, /*const */byte* /*command*/, int /*targetMode*/, /*const */anyID* /*targetIDs*/, /*const */byte* /*returnCode*/, void> sendPluginCommand;
    public delegate*</*const */byte* /*path*/, byte* /*result*/, nint /*maxLen*/, void> getDirectories;
    public delegate*<ulong /*scHandlerID*/, byte* /*host*/, ushort* /*port*/, byte* /*password*/, nint /*maxLen*/, uint> getServerConnectInfo;
    public delegate*<ulong /*scHandlerID*/, ulong /*channelID*/, byte* /*path*/, byte* /*password*/, nint /*maxLen*/, uint> getChannelConnectInfo;
    public delegate*</*const */byte* /*pluginID*/, byte* /*returnCode*/, nint /*maxLen*/, void> createReturnCode;
    public delegate*<ulong /*scHandlerID*/, PluginItemType /*itemType*/, ulong /*itemID*/, uint> requestInfoUpdate;
    public delegate*<ulong /*scHandlerID*/, ulong> getServerVersion;
    public delegate*<ulong /*scHandlerID*/, anyID /*clientID*/, int* /*result*/, uint> isWhispering;
    public delegate*<ulong /*scHandlerID*/, anyID /*clientID*/, int* /*result*/, uint> isReceivingWhisper;
    public delegate*<ulong /*scHandlerID*/, anyID /*clientID*/, byte* /*result*/, nint /*maxLen*/, uint> getAvatar;
    public delegate*</*const */byte* /*pluginID*/, int /*menuID*/, int /*enabled*/, void> setPluginMenuEnabled;
    public delegate*<void> showHotkeySetup;
    public delegate*</*const */byte* /*pluginID*/, /*const */byte* /*keyword*/, int /*isDown*/, void* /*qParentWindow*/, void> requestHotkeyInputDialog;
    public delegate*</*const */byte* /*pluginID*/, /*const */byte** /*keywords*/, byte** /*hotkeys*/, nint /*arrayLen*/, nint /*hotkeyBufSize*/, uint> getHotkeyFromKeyword;
    public delegate*<ulong /*scHandlerID*/, anyID /*clientID*/, byte* /*result*/, nint /*maxLen*/, uint> getClientDisplayName;
    public delegate*<PluginBookmarkList** /*list*/, uint> getBookmarkList;
    public delegate*<PluginGuiProfile /*profile*/, int* /*defaultProfileIdx*/, byte*** /*result*/, uint> getProfileList;
    public delegate*<PluginConnectTab /*connectTab*/, /*const */byte* /*serverLabel*/, /*const */byte* /*serverAddress*/, /*const */byte* /*serverPassword*/, /*const */byte* /*nickname*/, /*const */byte* /*channel*/, /*const */byte* /*channelPassword*/, /*const */byte* /*captureProfile*/, /*const */byte* /*playbackProfile*/, /*const */byte* /*hotkeyProfile*/, /*const */byte* /*soundProfile*/, /*const */byte* /*userIdentity*/, /*const */byte* /*oneTimeKey*/, /*const */byte* /*phoneticName*/, ulong* /*scHandlerID*/, uint> guiConnect;
    public delegate*<PluginConnectTab /*connectTab*/, /*const */byte* /*bookmarkuuid*/, ulong* /*scHandlerID*/, uint> guiConnectBookmark;
    public delegate*</*const */byte* /*bookmarkuuid*/, /*const */byte* /*serverLabel*/, /*const */byte* /*serverAddress*/, /*const */byte* /*serverPassword*/, /*const */byte* /*nickname*/, /*const */byte* /*channel*/, /*const */byte* /*channelPassword*/, /*const */byte* /*captureProfile*/, /*const */byte* /*playbackProfile*/, /*const */byte* /*hotkeyProfile*/, /*const */byte* /*soundProfile*/, /*const */byte* /*uniqueUserId*/, /*const */byte* /*oneTimeKey*/, /*const */byte* /*phoneticName*/, uint> createBookmark;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*permissionName*/, uint* /*result*/, uint> getPermissionIDByName;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*permissionName*/, int* /*result*/, uint> getClientNeededPermission;
    public delegate*</*const */byte* /*pluginID*/, /*const */byte* /*keyIdentifier*/, int /*up_down*/, void> notifyKeyEvent;

    /* Single-Track/Multi-Track recording */
    public delegate*<ulong /*serverConnectionHandlerID*/, int /*multitrack*/, int /*noFileSelector*/, /*const */byte* /*path*/, uint> startRecording;
    public delegate*<ulong /*serverConnectionHandlerID*/, uint> stopRecording;

    /* Convenience functions */
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */anyID* /*clientIDArray*/, ulong /*newChannelID*/, /*const */byte* /*password*/, /*const */byte* /*returnCode*/, uint> requestClientsMove;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */anyID* /*clientIDArray*/, /*const */byte* /*kickReason*/, /*const */byte* /*returnCode*/, uint> requestClientsKickFromChannel;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */anyID* /*clientIDArray*/, /*const */byte* /*kickReason*/, /*const */byte* /*returnCode*/, uint> requestClientsKickFromServer;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */anyID* /*clientIDArray*/, /*const */byte* /*returnCode*/, uint> requestMuteClientsTemporary;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */anyID* /*clientIDArray*/, /*const */byte* /*returnCode*/, uint> requestUnmuteClientsTemporary;
    public delegate*<ulong /*scHandlerID*/, uint /*permissionID*/, byte* /*result*/, nint /*max_len*/, uint> getPermissionNameByID;
    public delegate*<nint /*clientPropertyFlag*/, byte** /*resultString*/, uint> clientPropertyFlagToString;
    public delegate*<nint /*channelPropertyFlag*/, byte** /*resultString*/, uint> channelPropertyFlagToString;
    public delegate*<nint /*serverPropertyFlag*/, byte** /*resultString*/, uint> serverPropertyFlagToString;

    /* Server editing */
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, int /*value*/, uint> setServerVariableAsInt;
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, ulong /*value*/, uint> setServerVariableAsulong;
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, double /*value*/, uint> setServerVariableAsDouble;
    public delegate*<ulong /*serverConnectionHandlerID*/, nint /*flag*/, /*const */byte* /*value*/, uint> setServerVariableAsString;
    public delegate*<ulong /*serverConnectionHandlerID*/, /*const */byte* /*returnCode*/, uint> flushServerUpdates;
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

enum ClientProperties
{
    CLIENT_UNIQUE_IDENTIFIER = 0,           //automatically up-to-date for any client "in view", can be used to identify this particular client installation
    CLIENT_NICKNAME,                        //automatically up-to-date for any client "in view"
    CLIENT_VERSION,                         //for other clients than ourself, this needs to be requested (=> requestClientVariables)
    CLIENT_PLATFORM,                        //for other clients than ourself, this needs to be requested (=> requestClientVariables)
    CLIENT_FLAG_TALKING,                    //automatically up-to-date for any client that can be heard (in room / whisper)
    CLIENT_INPUT_MUTED,                     //automatically up-to-date for any client "in view", this clients microphone mute status
    CLIENT_OUTPUT_MUTED,                    //automatically up-to-date for any client "in view", this clients headphones/speakers/mic combined mute status
    CLIENT_OUTPUTONLY_MUTED,                //automatically up-to-date for any client "in view", this clients headphones/speakers only mute status
    CLIENT_INPUT_HARDWARE,                  //automatically up-to-date for any client "in view", this clients microphone hardware status (is the capture device opened?)
    CLIENT_OUTPUT_HARDWARE,                 //automatically up-to-date for any client "in view", this clients headphone/speakers hardware status (is the playback device opened?)
    CLIENT_INPUT_DEACTIVATED,               //only usable for ourself, not propagated to the network
    CLIENT_IDLE_TIME,                       //internal use
    CLIENT_DEFAULT_CHANNEL,                 //only usable for ourself, the default channel we used to connect on our last connection attempt
    CLIENT_DEFAULT_CHANNEL_PASSWORD,        //internal use
    CLIENT_SERVER_PASSWORD,                 //internal use
    CLIENT_META_DATA,                       //automatically up-to-date for any client "in view", not used by TeamSpeak, free storage for sdk users
    CLIENT_IS_MUTED,                        //only make sense on the client side locally, "1" if this client is currently muted by us, "0" if he is not
    CLIENT_IS_RECORDING,                    //automatically up-to-date for any client "in view"
    CLIENT_VOLUME_MODIFICATOR,              //internal use
    CLIENT_VERSION_SIGN,                    //sign
    CLIENT_SECURITY_HASH,                   //SDK use, not used by teamspeak. Hash is provided by an outside source. A channel will use the security salt + other client data to calculate a hash, which must be the same as the one provided here.
    CLIENT_ENCRYPTION_CIPHERS,              //internal use
    CLIENT_ENDMARKER,
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
    LogLevel_CRITICAL = 0, //these messages stop the program
    LogLevel_ERROR,        //everything that is really bad, but not so bad we need to shut down
    LogLevel_WARNING,      //everything that *might* be bad
    LogLevel_DEBUG,        //output that might help find a problem
    LogLevel_INFO,         //informational output, like "starting database version x.y.z"
    LogLevel_DEVEL         //developer only output (will not be displayed in release mode)
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
