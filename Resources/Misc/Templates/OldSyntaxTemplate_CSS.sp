#include <sourcemod>
#include <cstrike>

/**
 * @note #file is a somewhat recent discovery.
 * Instead of the full filepath, compile.exe will use
 * whatever name you supply here when printing errors/warnings.
 */
#file "Enter plugin name here"

#pragma semicolon 1

#define PLUGIN_VERSION "0.00"

EngineVersion:g_engineversion;

public Plugin:myinfo = 
{
	name = "Name of plugin here!",
	author = "Your name here!",
	description = "Brief description of plugin functionality here!",
	version = PLUGIN_VERSION,
	url = "Your website URL/AlliedModders profile URL"
};

public APLRes:AskPluginLoad2(Handle:myself, bool:late, String:error[], err_max)
{
	// No need for the old GetGameFolderName setup.
	g_engineversion = GetEngineVersion();
	if (g_engineversion != Engine_CSS)
	{
		SetFailState("This plugin was made for use with Counter-Strike: Source only.");
	}
} 

public OnPluginStart()
{
	/**
	 * @note For the love of god, please stop using FCVAR_PLUGIN.
	 * Console.inc even explains this above the entry for the FCVAR_PLUGIN define.
	 * "No logic using this flag ever existed in a released game. It only ever appeared in the first hl2sdk."
	 */
	CreateConVar("sm_pluginnamehere_version", PLUGIN_VERSION, "Standard plugin version ConVar. Please don't change me!", FCVAR_REPLICATED|FCVAR_NOTIFY|FCVAR_DONTRECORD);
}

public OnMapStart()
{
	/**
	 * @note Precache your models, sounds, etc. here!
	 * Not in OnConfigsExecuted! Doing so leads to issues.
	 */
}