public static class SorsTimings 
{
    public static float wait = 1f;

    // Cards and Entities
    public static float cardMoveTime = 0.5f;
    public static float effectTrigger = 1f;
    public static float effectExecution = 1f;
    public static float combatClash = 0.8f;
    
    // Turn State Machine
    public static float turnStateTransition = 1f;
    public static float draw = 0.15f;
    
    // Game Start
    public static float waitForSpawnFromFile = 6f;

    // Card Spawning
    public static float spawnCard = 0.05f;
    public static float showSpawnedCard = 1f;
    public static float moveSpawnedCard = 0.1f;

    // UI Transitions
    public static float showSpawnedEntity = 1f;
    public static float overlayScreenDisplayTime = 1f;
    public static float overlayScreenFadeTime = 0.5f;
	public static float cardPileRearrangement = 0.5f;

    public static void SkipCardSpawnAnimations(){
        waitForSpawnFromFile = 0.5f;
        spawnCard = 0f;
        showSpawnedCard = 0f;
        moveSpawnedCard = 0f;
    }
}
