using UnityEngine;

public static class SaveSystem
{
    private const string KEY_HAS_SAVE = "HasSaveData";
    private const string KEY_PLAYER_LIVES = "PlayerLives";
    private const string KEY_HAS_KEY = "HasKey";
    private const string KEY_PLAY_TIME = "PlayTime";
    private const string KEY_MAZE_SEED = "MazeSeed";
    private const string KEY_MAZE_WIDTH = "MazeWidth";
    private const string KEY_MAZE_HEIGHT = "MazeHeight";
    private const string KEY_PLAYER_POS_X = "PlayerPosX";
    private const string KEY_PLAYER_POS_Y = "PlayerPosY";
    private const string KEY_PLAYER_POS_Z = "PlayerPosZ";
    
    public static bool HasSaveData()
    {
        return PlayerPrefs.GetInt(KEY_HAS_SAVE, 0) == 1;
    }
    
    public static void SaveGame(GameData data)
    {
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.SetInt(KEY_PLAYER_LIVES, data.playerLives);
        PlayerPrefs.SetInt(KEY_HAS_KEY, data.hasKey ? 1 : 0);
        PlayerPrefs.SetFloat(KEY_PLAY_TIME, data.playTime);
        PlayerPrefs.SetInt(KEY_MAZE_SEED, data.mazeSeed);
        PlayerPrefs.SetInt(KEY_MAZE_WIDTH, data.mazeWidth);
        PlayerPrefs.SetInt(KEY_MAZE_HEIGHT, data.mazeHeight);
        PlayerPrefs.SetFloat(KEY_PLAYER_POS_X, data.playerPosition.x);
        PlayerPrefs.SetFloat(KEY_PLAYER_POS_Y, data.playerPosition.y);
        PlayerPrefs.SetFloat(KEY_PLAYER_POS_Z, data.playerPosition.z);
        
        PlayerPrefs.Save();
    }
    
    public static GameData LoadGame()
    {
        if (!HasSaveData())
            return null;
        
        GameData data = new GameData
        {
            playerLives = PlayerPrefs.GetInt(KEY_PLAYER_LIVES, 2),
            hasKey = PlayerPrefs.GetInt(KEY_HAS_KEY, 0) == 1,
            playTime = PlayerPrefs.GetFloat(KEY_PLAY_TIME, 0f),
            mazeSeed = PlayerPrefs.GetInt(KEY_MAZE_SEED, 0),
            mazeWidth = PlayerPrefs.GetInt(KEY_MAZE_WIDTH, 10),
            mazeHeight = PlayerPrefs.GetInt(KEY_MAZE_HEIGHT, 10),
            playerPosition = new Vector3(
                PlayerPrefs.GetFloat(KEY_PLAYER_POS_X, 0f),
                PlayerPrefs.GetFloat(KEY_PLAYER_POS_Y, 1f),
                PlayerPrefs.GetFloat(KEY_PLAYER_POS_Z, 0f)
            )
        };
        
        return data;
    }
    
    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(KEY_HAS_SAVE);
        PlayerPrefs.DeleteKey(KEY_PLAYER_LIVES);
        PlayerPrefs.DeleteKey(KEY_HAS_KEY);
        PlayerPrefs.DeleteKey(KEY_PLAY_TIME);
        PlayerPrefs.DeleteKey(KEY_MAZE_SEED);
        PlayerPrefs.DeleteKey(KEY_MAZE_WIDTH);
        PlayerPrefs.DeleteKey(KEY_MAZE_HEIGHT);
        PlayerPrefs.DeleteKey(KEY_PLAYER_POS_X);
        PlayerPrefs.DeleteKey(KEY_PLAYER_POS_Y);
        PlayerPrefs.DeleteKey(KEY_PLAYER_POS_Z);
        
        PlayerPrefs.Save();
    }
}

[System.Serializable]
public class GameData
{
    public int playerLives;
    public bool hasKey;
    public float playTime;
    public int mazeSeed;
    public int mazeWidth;
    public int mazeHeight;
    public Vector3 playerPosition;
}