public static class SorsTimings 
{
    public static int second = 1000;

    // Cards and Entities
    public static float cardMoveTime = 0.5f;
    
    // Turn State Machine
    public static float turnStateTransition = 1f;
    public static float draw = 0.15f;
    public static float combatCleanUp = 1.5f;
    
    // Game Start
    public static float waitForSpawnFromFile = 7f;

    // Card Spawning
    public static float spawnCard = 0.05f;
    public static float showSpawnedCard = 1f;
    public static float moveSpawnedCard = 0.1f;

    // UI Transitions
    public static float showSpawnedEntity = 1f;
    public static int overlayScreenDisplayTime = 1000;
    public static int overlayScreenFadeTime = 500;
    internal static int hoverPreviewDelay = 500;
	public static float cardPileRearrangement = 0.5f;

    // VFX
    public static float damageTime = 0.3f;
    public static float attackTime = 0.5f;
    public static int effectTrigger = 600;
    public static float effectProjectile = 0.7f;
    public static float effectHitVFX = 1f;

    public static void SkipCardSpawnAnimations(){
        waitForSpawnFromFile = 0.01f;
        spawnCard = 0.01f;
        showSpawnedCard = 0.01f;
        moveSpawnedCard = 0.01f;

        // showSpawnedEntity = 0.01f;
    }
}
