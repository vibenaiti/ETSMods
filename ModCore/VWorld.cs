using Unity.Entities;

namespace ModCore;

/// <summary>
/// Shim replacing Bloodstone.API.VWorld.
/// Server world is captured from ServerBootstrapSystem.OnGameDataInitialized to ensure
/// we have the correct game world (not just DefaultGameObjectInjectionWorld).
/// </summary>
public static class VWorld
{
    private static World _serverWorld;

    /// <summary>Set by InitializationPatch1 when OnGameDataInitialized fires.</summary>
    public static void SetServerWorld(World world) => _serverWorld = world;

    public static World Server => _serverWorld ?? World.DefaultGameObjectInjectionWorld;
}
