using System.IO;
using UnityEngine;

// Finds, reads and writes a character's files, for both the hitbox editor and the game.
// Everything for a character lives in one folder: Assets/Resources/Characters/<id>/
// holding <id>.character.json, <id>.skeleton.json and the sprite sheets.
//
// In the editor the files are read and written on disk directly, so edits show up without a reimport.
// In a build they ship inside Resources and are read only. Saves go to persistentDataPath instead,
// and a saved copy there is read in place of the shipped one
public static class CharacterFiles
{
    // Path under a Resources folder, for Resources.Load
    public static string ResourceFolder(CharacterId id) => $"Characters/{id}";

    private static string CharacterName(CharacterId id) => $"{id}.character";
    private static string SkeletonName(CharacterId id) => $"{id}.skeleton";

    // Where saves go: the project in the editor, the player's machine in a build
    public static string WriteFolder(CharacterId id) => Application.isEditor
        ? Path.Combine(Application.dataPath, "Resources", "Characters", id.ToString())
        : Path.Combine(Application.persistentDataPath, "Characters", id.ToString());

    // A character with no file yet reads as empty
    public static CharacterFile ReadCharacter(CharacterId id)
    {
        string json = ReadText(id, CharacterName(id));
        CharacterFile file = json != null ? CharacterFileJson.ReadCharacter(json) : new CharacterFile();

        // Hand edited files may have keys out of order, and everything that reads keys assumes they are sorted
        foreach (var state in file.States.Values)
        {
            foreach (var box in state.Boxes)
            {
                BoxKeyMath.Sort(box.Keys);
            }
        }
        return file;
    }

    // Null when the character has no bake, as with sprite characters
    public static BakedSkeletonFile ReadSkeleton(CharacterId id)
    {
        string json = ReadText(id, SkeletonName(id));
        return json != null ? CharacterFileJson.ReadSkeleton(json) : null;
    }

    public static void WriteCharacter(CharacterId id, CharacterFile file)
    {
        string folder = WriteFolder(id);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, CharacterName(id) + ".json"), CharacterFileJson.WriteCharacter(file));
    }

    // Read fresh on every call, so edits made in the hitbox editor show up on the next match
    public static CharacterDefinition LoadDefinition(CharacterId id)
    {
        return CharacterDefinitionBuilder.Build(ReadCharacter(id), ReadSkeleton(id));
    }

    // Null when the file doesn't exist
    private static string ReadText(CharacterId id, string name)
    {
        // The write folder first. In the editor it is the Resources folder itself, read from disk so it's never stale
        string path = Path.Combine(WriteFolder(id), name + ".json");
        if (File.Exists(path)) return File.ReadAllText(path);
        if (Application.isEditor) return null;

        var asset = Resources.Load<TextAsset>($"{ResourceFolder(id)}/{name}");
        return asset != null ? asset.text : null;
    }
}
