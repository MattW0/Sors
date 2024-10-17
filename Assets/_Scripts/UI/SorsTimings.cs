public static class SorsTimings 
{
    public static int wait = 1000;
    public static int waitShort = 100;
    public static int waitLong = 1500;
    
    // Turn State Machine
    public static int draw = 150;

    // UI Transitions
    public static float cardMoveTime = 0.5f;
    public static int showSpawnedEntity = 1000;
    public static int overlayScreenDisplayTime = 1000;
    public static int overlayScreenFadeTime = 500;
    public static int hoverPreviewDelay = 500;
	public static float cardPileRearrangement = 0.5f;

    // VFX
    public static int effectTrigger = 600;
    public static float damageTime = 0.3f;
    public static float attackTime = 0.5f;
    public static float effectProjectile = 0.7f;
    public static float effectHitVFX = 1f;
    
    // Card Spawning
    public static int spawnCard = 50;
    public static int showSpawnedCard = 2000;
    public static int moveSpawnedCard = 100;
    public static int waitForSpawnFromFile = 2000; // Game state loading

    public static void SkipCardSpawnAnimations(){
        spawnCard = 1;
        showSpawnedCard = 1;
        moveSpawnedCard = 1;
        waitForSpawnFromFile = 1;
    }
}
