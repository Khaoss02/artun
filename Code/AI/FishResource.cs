using Sandbox;

[GameResource( "Fish Definition", "fish", "Datos de un pez específico" )]
public partial class FishResource : GameResource
{
    [Property, Group("Visuales")] public string Name { get; set; } = "Pez";
    [Property, Group("Visuales")] public Model FishModel { get; set; }

    [Property, Group("Mecánicas")] public float Difficulty { get; set; } = 1.0f;
    [Property, Group("Mecánicas")] public float CaptureTime { get; set; } = 3.0f;
    [Property, Group("Mecánicas")] public float MovementRandomness { get; set; } = 0.5f;

    [Property, Group("IA"), TextArea] public string Personality { get; set; } = "Eres un pez animado.";

    // Añade esto a tu FishResource.cs
    [Property, Group("Visuales"), ImageAssetPath] 
    public string IconPath { get; set; } = "textures/items/fish.png";
}