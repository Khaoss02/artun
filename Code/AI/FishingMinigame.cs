using Sandbox;
using System;
using System.Linq;

public sealed class FishingEvent : Component
{
    [Property] public FishResource CurrentFish { get; set; }
    [Property] public GameObject FishingUIPanel { get; set; }
    
    [Header( "Referencias de Escena" )]
    [Property] public GameObject PlayerObject { get; set; }

    [Property] public bool IsActive { get; set; } = false;

    // Cambiamos a Vector2 para coordenadas X e Y
    public Vector2 PlayerPos { get; private set; } = new Vector2(0.5f, 0.5f);
    public Vector2 FishPos { get; private set; } = new Vector2(0.5f, 0.5f);
    public float CaptureProgress { get; private set; } = 0f;

    // Velocidad del puntero del jugador
    public float PlayerSpeed { get; set; } = 1.2f;

    protected override void OnUpdate()
    {
        if ( Input.Pressed( "Menu" ) )
        {
            if ( !IsActive ) StartFishing();
            else StopFishing();
        }

        if ( !IsActive ) return;

        // --- MOVIMIENTO DEL JUGADOR (WASD) ---
        float moveX = 0f;
        float moveY = 0f;

        if ( Input.Down( "Right" ) ) moveX += 1f;
        if ( Input.Down( "Left" ) ) moveX -= 1f;
        if ( Input.Down( "Backward" ) ) moveY += 1f; // Mover hacia abajo en el UI
        if ( Input.Down( "Forward" ) ) moveY -= 1f;  // Mover hacia arriba en el UI

        // Normalizamos para que no se mueva más rápido en diagonal
        Vector2 moveDir = new Vector2(moveX, moveY).Normal;
        
        // Actualizamos y limitamos la posición entre 0 y 1 (dentro de la caja)
        PlayerPos += moveDir * Time.Delta * PlayerSpeed;
        PlayerPos = new Vector2(PlayerPos.x.Clamp(0, 1), PlayerPos.y.Clamp(0, 1));

        // --- MOVIMIENTO DEL PEZ ---
        if ( CurrentFish != null )
        {
            // Usamos Seno y Coseno con distintas velocidades para crear un patrón de movimiento "aleatorio"
            float t = Time.Now * CurrentFish.Difficulty;
            
            // X se mueve con un seno, Y con un coseno desfasado por el MovementRandomness
            float fishX = MathF.Sin(t) * 0.4f + 0.5f;
            float fishY = MathF.Cos(t * 0.8f + CurrentFish.MovementRandomness) * 0.4f + 0.5f;
            
            FishPos = new Vector2(fishX, fishY);
        }

        // --- LÓGICA DE CAPTURA ---
        // Comprobamos la distancia entre el jugador y el pez (radio de solapamiento)
        float overlapRadius = 0.15f; 
        bool isOverlapping = Vector2.Distance(PlayerPos, FishPos) < overlapRadius;

        // Sumamos o restamos progreso
        CaptureProgress += (isOverlapping ? (Time.Delta / CurrentFish.CaptureTime) : -Time.Delta * 0.5f);
        CaptureProgress = CaptureProgress.Clamp( 0, 1 );

        if ( CaptureProgress >= 1.0f ) Win();
    }

    private void StartFishing()
    {
        IsActive = true;
        CaptureProgress = 0f;
        
        // Reiniciamos posiciones al centro
        PlayerPos = new Vector2(0.5f, 0.5f);
        FishPos = new Vector2(0.5f, 0.5f);

        if ( FishingUIPanel is not null ) FishingUIPanel.Enabled = true;

        Mouse.Visible = true;
        SetPlayerState( false );
    }

    private void StopFishing()
    {
        IsActive = false;
        if ( FishingUIPanel is not null ) FishingUIPanel.Enabled = false;

        Mouse.Visible = false;
        SetPlayerState( true );
    }

    private void SetPlayerState( bool state )
    {
        if ( PlayerObject is null ) return;

        var components = PlayerObject.Components.GetAll<Component>( FindMode.EverythingInSelfAndDescendants );
        
        foreach ( var comp in components )
        {
            if ( comp is CameraComponent || comp is FishingEvent ) continue;

            var name = comp.GetType().Name.ToLower();
            if ( name.Contains("controller") || name.Contains("player") || name.Contains("move") || name.Contains("look") )
            {
                comp.Enabled = state;
            }
        }
    }

    private void Win()
    {
        // 1. Buscamos el inventario en la escena
        var playerInventory = Scene.GetAllComponents<Inventory>().FirstOrDefault();

        if ( playerInventory != null && CurrentFish != null )
        {
            // 2. Comprobamos si hay espacio
            if ( playerInventory.CaughtFishes.Count < 16 )
            {
                // 3. Añadimos EL RECURSO al inventario
                playerInventory.CaughtFishes.Add( CurrentFish );
                Log.Info( $"¡Has pescado un {CurrentFish.Name} y se ha guardado en el inventario!" );
            }
            else
            {
                Log.Warning( "¡El inventario está lleno! El pez se ha escapado." );
            }
        }

        StopFishing();
    }

    protected override void OnDisabled()
    {
        StopFishing();
    }
}
