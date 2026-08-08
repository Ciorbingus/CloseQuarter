using Silk.NET.OpenGL;

namespace CloseQuarter.Client.Models;

public class CharacterDefinition
{
    public string Id { get; }
    public string DisplayName { get; }
    public Func<GL, Model3D> ModelFactory { get; }
        public Func<IMoveset> MovesetFactory { get; }
    public string BodyTexturePath { get; }
    public string HeadTexturePath { get; }

    public CharacterDefinition(
        string id, 
        string displayName, 
        Func<GL, Model3D> modelFactory, 
        Func<IMoveset> movesetFactory, 
        string bodyTexturePath, 
        string headTexturePath)
    {
        Id = id;
        DisplayName = displayName;
        ModelFactory = modelFactory;
        MovesetFactory = movesetFactory;
        BodyTexturePath = bodyTexturePath;
        HeadTexturePath = headTexturePath;
    }
}

public static class CharacterRoster
{
    public static Dictionary<string, CharacterDefinition> Characters { get; } = new()
    {
        {
            "bobi", new CharacterDefinition(
                "bobi",
                "Bobi",
                (gl) => new BoxerModel(gl),  
                () => new BoxerMoveset(),
                "Textures/player.png",
                "Textures/player_face.png"
            )
        },
        {
            "toma", new CharacterDefinition(
                "toma",
                "Toma",
                (gl) => new MilitaryModel(gl), 
                () => new MilitaryMoveset(),
                "Textures/player.png",
                "Textures/player2_face.png"
            )
        }
    };
}