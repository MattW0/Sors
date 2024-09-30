public static class SorsTimings 
{
    public static int second = 1000;

    // Cards and Entities
    public static float cardMoveTime = 0.5f;
    
    // Turn State Machine
    public static float turnStateTransition = 1f;
    public static float draw = 0.15f;
    public static float combatCleanUp = 1.5f;
    
    // Card Spawning
    public static int spawnCard = 50;
    public static int showSpawnedCard = 1000;
    public static int moveSpawnedCard = 100;
    public static int waitForSpawnFromFile = 2000; // Game state loading

    // UI Transitions
    public static int showSpawnedEntity = 1000;
    public static int overlayScreenDisplayTime = 1000;
    public static int overlayScreenFadeTime = 500;
    public static int hoverPreviewDelay = 500;
	public static float cardPileRearrangement = 0.5f;

    // VFX
    public static float damageTime = 0.3f;
    public static float attackTime = 0.5f;
    public static int effectTrigger = 600;
    public static float effectProjectile = 0.7f;
    public static float effectHitVFX = 1f;

    public static void SkipCardSpawnAnimations(){
        waitForSpawnFromFile = 1;
        spawnCard = 1;
        showSpawnedCard = 1;
        moveSpawnedCard = 1;
    }
}
